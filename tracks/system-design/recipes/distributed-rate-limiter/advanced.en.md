---
title: "Distributed rate limiter: advanced"
lang: en
track: system-design
kind: recipe
doc: advanced
slug: distributed-rate-limiter
part: 2
tags: [rate-limiting, redis, token-bucket, lua, distributed-systems]
versions: ".NET 11, C# 15; StackExchange.Redis 3.4.35; Redis 8"
status: draft
updated: 2026-11-27
---

# Distributed rate limiter: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## The script

```csharp
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
```

A script runs atomically: other clients see all of its effects or none, and the server is blocked while it runs, so keep it short. Every key is passed in `KEYS`. `TIME` gives one clock for all instances. The `PEXPIRE` removes buckets that have had time to refill completely.

```csharp
var result = (RedisResult[])(await redis.ScriptEvaluateAsync(
    Script,
    [(RedisKey)(prefix + key)],
    [limit.Capacity, limit.RefillPerSecond, cost]).WaitAsync(cancellationToken))!;

return new Decision((int)result[0] == 1, (int)result[1], TimeSpan.FromMilliseconds((long)result[2]));
```

File: [Ex02_Redis.cs](code/Examples/Ex02_Redis.cs).

Check with thirty concurrent requests from three "instances" against a capacity of 5: exactly five pass.

> Lua numbers are converted to integers in replies and lose the fraction: `return 3.9` gives 3. That is why the token count is stored as a string, and the reply holds only whole numbers.

## Running the integration tests

```bash
docker run --rm -d --name rl-redis -p 127.0.0.1:6379:6379 redis:8-alpine
dotnet test
docker stop rl-redis
```

Without Redis on `127.0.0.1:6379` these tests are reported as skipped.

## Answering the client

```csharp
if (!decision.Allowed)
{
    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
    // Whole seconds, rounded up: rounding down invites a request that fails again.
    context.Response.Headers.RetryAfter =
        Math.Max(1, (int)Math.Ceiling(decision.RetryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
    return;
}
```

429 means too many requests; `Retry-After` tells when to try again. Identify the client by something it cannot forge. File: [Ex05_Middleware.cs](code/Examples/Ex05_Middleware.cs).

## When the store fails

```csharp
try
{
    // The limiter is on every request; it must not become the slowest part of it.
    return await shared.AcquireAsync(key, cost, cancellationToken).AsTask()
        .WaitAsync(timeout, clock, cancellationToken);
}
// A caller's mistake or cancellation is not a store failure and must not be "allowed".
catch (Exception e) when (e is not ArgumentException && !cancellationToken.IsCancellationRequested)
{
    Interlocked.Increment(ref _failures);
    return onFailure switch
    {
        OnFailure.Allow => new Decision(true, 0, TimeSpan.Zero),
        OnFailure.UseFallback when fallback is not null =>
            await fallback.AcquireAsync(key, cost, cancellationToken),
        _ => new Decision(false, 0, TimeSpan.FromSeconds(1)),
    };
}
```

| Policy | Good for | Risk |
|---|---|---|
| Fail open | availability matters, limits are a courtesy | the protected system is exposed |
| Fail closed | login attempts, expensive operations | the limiter takes the service down |
| Local fallback | a compromise | N instances allow N local limits |

The wait is bounded by a timeout. The client library's own timeouts are 5000 ms by default, far too long for a check made on every request. The timeout abandons the wait, not the command: Redis may still execute it and take the token. File: [Ex03_Resilient.cs](code/Examples/Ex03_Resilient.cs).

## Common mistakes

- ❌ `GET`, compute, `SET` from the application → ✅ one script.
- ❌ The clock of each instance → ✅ the clock of the store.
- ❌ Keys without expiry → ✅ expire idle buckets.
- ❌ A new connection per request → ✅ one shared `ConnectionMultiplexer`.

## Related materials

- [Basics](basics.en.md)
- [Expert](expert.en.md)

## Sources

- [Redis: scripting with Lua](https://redis.io/docs/latest/develop/programmability/eval-intro/)
- [Redis: Lua API](https://redis.io/docs/latest/develop/programmability/lua-api/)
- [Redis: TIME](https://redis.io/docs/latest/commands/time/)
- [RFC 6585, status 429](https://www.rfc-editor.org/rfc/rfc6585#section-4)
- [ASP.NET Core rate limiting](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
