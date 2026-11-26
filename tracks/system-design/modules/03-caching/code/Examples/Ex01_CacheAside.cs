// Example: cache-aside with TTL, and invalidation after a write.
// Module: system-design/03-caching (basics)
// Tests: dotnet test --filter "FullyQualifiedName~CacheAside"
using System.Collections.Concurrent;

namespace CachingDesign.Examples;

public sealed class TtlCache<TKey, TValue>(TimeProvider clock) where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, (TValue Value, DateTimeOffset ExpiresAt)> _entries = new();

    public bool TryGet(TKey key, out TValue value)
    {
        if (_entries.TryGetValue(key, out var e) && clock.GetUtcNow() < e.ExpiresAt)
        {
            value = e.Value;
            return true;
        }
        value = default!;
        return false;
    }

    public void Set(TKey key, TValue value, TimeSpan ttl) => _entries[key] = (value, clock.GetUtcNow() + ttl);

    public void Remove(TKey key) => _entries.TryRemove(key, out _);
}

public sealed class ProductStore
{
    private readonly ConcurrentDictionary<int, decimal> _prices = new();
    public int Reads { get; private set; }

    public decimal Read(int id) { Reads++; return _prices.GetValueOrDefault(id); }
    public void Write(int id, decimal price) => _prices[id] = price;
}

public sealed class PriceService(ProductStore store, TtlCache<int, decimal> cache)
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    public decimal GetPrice(int id)
    {
        if (cache.TryGet(id, out decimal cached)) return cached;
        decimal price = store.Read(id);
        cache.Set(id, price, Ttl);
        return price;
    }

    public void SetPrice(int id, decimal price)
    {
        store.Write(id, price);
        cache.Remove(id); // delete, do not update: concurrent writers could leave the older value
    }
}
