// Example: cache stampede, single-flight protection and TTL jitter.
// Module: system-design/03-caching (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Stampede"
using System.Collections.Concurrent;

namespace CachingDesign.Examples;

// Concurrent callers of the same key share one load. This is per process, not per cluster.
public sealed class SingleFlight<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Lazy<Task<TValue>>> _inFlight = new();

    public async Task<TValue> RunAsync(TKey key, Func<Task<TValue>> load)
    {
        // Lazy guarantees one invocation even if GetOrAdd creates several candidates.
        var flight = _inFlight.GetOrAdd(key, _ => new Lazy<Task<TValue>>(load));
        try
        {
            return await flight.Value;
        }
        finally
        {
            // Remove only our own entry, so a failed load is retried by the next caller.
            _inFlight.TryRemove(new KeyValuePair<TKey, Lazy<Task<TValue>>>(key, flight));
        }
    }
}

public static class Ttl
{
    // Spreads expirations: keys written together do not expire together.
    public static TimeSpan WithJitter(TimeSpan ttl, double ratio, Random random) =>
        ttl * (1 + ratio * (2 * random.NextDouble() - 1));
}
