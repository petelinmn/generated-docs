// Example: the token bucket as a pure function and a limiter that keeps buckets in one process.
// Recipe: system-design/recipes/distributed-rate-limiter (basics)
// Tests: dotnet test --filter "FullyQualifiedName~TokenBucket"
namespace RateLimiting.Examples;

// Capacity bounds a burst; the refill rate bounds the long-term average.
public sealed record Limit(int Capacity, double RefillPerSecond);

public sealed record Decision(bool Allowed, int Remaining, TimeSpan RetryAfter);

public readonly record struct Bucket(double Tokens, DateTimeOffset UpdatedAt);

public interface ILimiter
{
    ValueTask<Decision> AcquireAsync(string key, int cost = 1, CancellationToken cancellationToken = default);
}

public static class TokenBucket
{
    public static (Decision Decision, Bucket Next) Take(Bucket? current, Limit limit, DateTimeOffset now, int cost)
    {
        // A negative cost would add tokens.
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cost);
        var bucket = current ?? new Bucket(limit.Capacity, now);
        // Time that goes backwards must not remove or add tokens.
        double elapsed = Math.Max(0, (now - bucket.UpdatedAt).TotalSeconds);
        double tokens = Math.Min(limit.Capacity, bucket.Tokens + elapsed * limit.RefillPerSecond);

        bool allowed = tokens >= cost;
        if (allowed) tokens -= cost;

        TimeSpan retryAfter = allowed
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds((cost - tokens) / limit.RefillPerSecond);
        return (new Decision(allowed, (int)tokens, retryAfter), new Bucket(tokens, now));
    }
}

public sealed class InProcessLimiter(Limit limit, TimeProvider clock) : ILimiter
{
    private readonly Dictionary<string, Bucket> _buckets = [];
    private readonly Lock _gate = new();

    public ValueTask<Decision> AcquireAsync(string key, int cost = 1, CancellationToken cancellationToken = default)
    {
        lock (_gate) // read and write must be one step
        {
            Bucket? current = _buckets.TryGetValue(key, out var found) ? found : null;
            var (decision, next) = TokenBucket.Take(current, limit, clock.GetUtcNow(), cost);
            _buckets[key] = next;
            return ValueTask.FromResult(decision);
        }
    }
}
