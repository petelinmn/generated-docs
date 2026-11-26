// Solutions to the exercises of system-design/03-caching.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
using System.Collections.Concurrent;

namespace CachingDesign.Solutions;

public enum Freshness { Fresh, Stale, Missing }

public static class Exercises
{
    // Exercise 1: database load that reaches the origin for a given hit ratio.
    public static double OriginQps(double totalQps, double hitRatio) => totalQps * (1 - hitRatio);

    // Exercise 2: classify an entry for stale-while-revalidate.
    public static Freshness Classify(TimeSpan age, TimeSpan ttl, TimeSpan staleWindow) =>
        age < ttl ? Freshness.Fresh
        : age < ttl + staleWindow ? Freshness.Stale
        : Freshness.Missing;
}

// Exercise 3: a write is ignored if it is older than what is already cached.
public sealed class VersionedCache<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, (long Version, TValue Value)> _entries = new();

    public void Set(TKey key, long version, TValue value) =>
        _entries.AddOrUpdate(
            key,
            (version, value),
            (_, current) => version > current.Version ? (version, value) : current);

    public TValue? Get(TKey key) => _entries.TryGetValue(key, out var e) ? e.Value : default;
}
