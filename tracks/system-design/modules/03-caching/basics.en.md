---
title: "Caching: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 03-caching
part: 1
tags: [system-design, caching, cache-aside, cache-invalidation, cache-stampede, lru-cache, hybrid-cache]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: verified
updated: 2026-11-26
---

# Caching: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** async/await, DI in .NET · **Time:** ~2 h

## Why it matters

If the cache serves 99% of requests, at 10,000 requests per second only 100 reach the database. The reverse is also true: when the cache fails, the database suddenly gets all 10,000.

## Cache levels

| Where | What it caches | Who controls it |
|---|---|---|
| Browser | HTTP responses | `Cache-Control` headers |
| CDN and proxies | static files and public responses | headers and CDN settings |
| Application memory (L1) | objects | code |
| Distributed cache (L2), for example Redis | serialized values | code |
| Database | data and index pages | the DBMS |

## Cache-aside

The application fills the cache itself on a miss, and removes the entry after a write.

```csharp
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
```

File: [Ex01_CacheAside.cs](code/Examples/Ex01_CacheAside.cs).

## TTL and invalidation

TTL (time to live) is the period after which an entry counts as absent. Time comes from `TimeProvider`, so tests move the clock instead of waiting. The tests show the main limitation: if someone writes to the database bypassing the service, the cache returns the old value until the TTL expires. The TTL is the safety net for a missed invalidation.

## HybridCache

`HybridCache` from the `Microsoft.Extensions.Caching.Hybrid` package combines L1 and an optional L2 and performs cache-aside in one call.

```csharp
public ValueTask<string> GetNameAsync(int id, Task gate, CancellationToken ct = default) =>
    cache.GetOrCreateAsync(
        $"product:{id}",
        async _ =>
        {
            Interlocked.Increment(ref _loads);
            await gate; // stands in for a slow database query
            return $"Product {id}";
        },
        tags: ["products"],
        cancellationToken: ct);
```

Ten concurrent calls on an empty cache run the load once. `RemoveByTagAsync("products")` invalidates the entries with that tag. File: [Ex04_HybridCache.cs](code/Examples/Ex04_HybridCache.cs).

## Beginner mistakes

- ❌ A cache without a TTL → ✅ always bound the lifetime.
- ❌ A key that misses part of the input (language, user) → ✅ everything that affects the result goes into the key.
- ❌ Invalidating first and writing to the database afterwards → ✅ write first, then delete.
- ❌ Treating the cache as storage → ✅ the system must survive it being cleared.

## Related materials

- [Advanced theory](advanced.en.md)
- [Caching recipe for ASP.NET Core](../../../csharp/recipes/caching/overview.en.md)

## Sources

- [HybridCache in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Cache-Aside pattern](https://learn.microsoft.com/azure/architecture/patterns/cache-aside)
