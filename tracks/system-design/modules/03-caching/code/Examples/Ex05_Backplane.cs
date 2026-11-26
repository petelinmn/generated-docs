// Example: invalidating in-process caches of several instances through publish/subscribe.
// Module: system-design/03-caching (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Backplane"
namespace CachingDesign.Examples;

// Stands in for Redis Pub/Sub: at most once, nothing is stored for absent subscribers.
public sealed class Backplane
{
    private readonly List<LocalCache> _subscribers = [];

    public void Subscribe(LocalCache cache) => _subscribers.Add(cache);
    public void Unsubscribe(LocalCache cache) => _subscribers.Remove(cache);
    public void Publish(string key) => _subscribers.ForEach(s => s.Invalidate(key));
}

public sealed class LocalCache(TimeProvider clock, TimeSpan ttl, Func<string, string> load)
{
    private readonly TtlCache<string, string> _entries = new(clock);

    public string Get(string key)
    {
        if (_entries.TryGet(key, out string cached)) return cached;
        string value = load(key);
        _entries.Set(key, value, ttl); // the TTL covers a lost invalidation
        return value;
    }

    public void Invalidate(string key) => _entries.Remove(key);
}
