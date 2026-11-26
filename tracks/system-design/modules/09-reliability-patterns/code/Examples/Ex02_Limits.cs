// Example: rate limiting, bulkhead and load shedding with System.Threading.RateLimiting.
// Module: system-design/09-reliability-patterns (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Limits"
using System.Threading.RateLimiting;

namespace Reliability.Examples;

public static class Limits
{
    // Manual replenishment keeps the example independent of the wall clock.
    public static TokenBucketRateLimiter TokenBucket(int burst, int perPeriod) => new(new TokenBucketRateLimiterOptions
    {
        TokenLimit = burst,
        TokensPerPeriod = perPeriod,
        ReplenishmentPeriod = TimeSpan.FromMilliseconds(1),
        QueueLimit = 0, // reject at once instead of queueing
        AutoReplenishment = false,
    });

    // A bulkhead: one slow dependency can occupy only its own slots.
    public static ConcurrencyLimiter Bulkhead(int slots) =>
        new(new ConcurrencyLimiterOptions { PermitLimit = slots, QueueLimit = 0 });

    public static async Task<(bool Accepted, T? Result)> RunOrShedAsync<T>(RateLimiter limiter, Func<Task<T>> action)
    {
        using RateLimitLease lease = limiter.AttemptAcquire();
        if (!lease.IsAcquired) return (false, default);

        return (true, await action()); // disposing the lease returns the slot
    }
}
