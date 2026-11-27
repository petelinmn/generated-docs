// Example: one bucket shared by all instances: a Lua script in Redis does read, refill and write atomically.
// Recipe: system-design/recipes/distributed-rate-limiter (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Redis"   (needs Redis on 127.0.0.1:6379)
using StackExchange.Redis;

namespace RateLimiting.Examples;

public sealed class RedisLimiter(IDatabase redis, Limit limit, string prefix = "rl:") : ILimiter
{
    // KEYS[1] bucket; ARGV: capacity, refill per second, cost.
    private const string Script = """
        local capacity = tonumber(ARGV[1])
        local rate = tonumber(ARGV[2])
        local cost = tonumber(ARGV[3])

        -- the clock of Redis, so that instances with different clocks agree
        local t = redis.call('TIME')
        local now = tonumber(t[1]) + tonumber(t[2]) / 1000000

        local state = redis.call('HMGET', KEYS[1], 'tokens', 'at')
        local tokens = tonumber(state[1]) or capacity
        local at = tonumber(state[2]) or now
        tokens = math.min(capacity, tokens + math.max(0, now - at) * rate)

        local allowed = 0
        local retry_ms = 0
        if tokens >= cost then
          allowed = 1
          tokens = tokens - cost
        else
          retry_ms = math.ceil((cost - tokens) / rate * 1000)
        end

        redis.call('HSET', KEYS[1], 'tokens', tostring(tokens), 'at', string.format('%.6f', now))
        -- a bucket idle long enough to be full again can be forgotten
        redis.call('PEXPIRE', KEYS[1], math.ceil(capacity / rate * 1000) + 1000)

        return { allowed, math.floor(tokens), retry_ms }
        """;

    public async ValueTask<Decision> AcquireAsync(string key, int cost = 1, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cost);
        var result = (RedisResult[])(await redis.ScriptEvaluateAsync(
            Script,
            [(RedisKey)(prefix + key)],
            [limit.Capacity, limit.RefillPerSecond, cost]).WaitAsync(cancellationToken))!;

        return new Decision((int)result[0] == 1, (int)result[1], TimeSpan.FromMilliseconds((long)result[2]));
    }
}
