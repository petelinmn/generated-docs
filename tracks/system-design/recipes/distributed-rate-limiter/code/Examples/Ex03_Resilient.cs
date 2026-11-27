// Example: what to answer when the shared store is slow or down.
// Recipe: system-design/recipes/distributed-rate-limiter (advanced, expert)
// Tests: dotnet test --filter "FullyQualifiedName~Resilient"
namespace RateLimiting.Examples;

public enum OnFailure
{
    Allow,       // fail open: availability first
    Deny,        // fail closed: protection first
    UseFallback, // a stricter local limit
}

public sealed class ResilientLimiter(
    ILimiter shared, OnFailure onFailure, TimeSpan timeout, TimeProvider clock, ILimiter? fallback = null) : ILimiter
{
    private long _failures;

    public long Failures => Interlocked.Read(ref _failures);

    public async ValueTask<Decision> AcquireAsync(string key, int cost = 1, CancellationToken cancellationToken = default)
    {
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
    }
}
