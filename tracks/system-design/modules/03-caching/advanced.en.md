---
title: "Caching: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 03-caching
part: 1
tags: [system-design, caching, cache-aside, cache-invalidation, cache-stampede, lru-cache, hybrid-cache]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: verified
updated: 2026-11-26
---

# Caching: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Write strategies

| Strategy | How it works | Risk |
|---|---|---|
| Cache-aside | the application reads and invalidates | read/write race |
| Read-through | the cache loads on a miss by itself | loading logic lives in the cache layer |
| Write-through | write to cache and source together | slower writes |
| Write-behind | cache first, source later | data loss on failure |
| Refresh-ahead | refresh before expiry | wasted work for rare keys |

## Cache stampede

A popular key expired, and all requests went to the database at once. The first remedy is to merge concurrent loads of one key (single-flight).

```csharp
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
```

The protection works within a process: ten instances will make up to ten loads. The second remedy is TTL jitter, so that keys written together do not expire together.

```csharp
public static TimeSpan WithJitter(TimeSpan ttl, double ratio, Random random) =>
    ttl * (1 + ratio * (2 * random.NextDouble() - 1));
```

File: [Ex02_Stampede.cs](code/Examples/Ex02_Stampede.cs). The third is to serve the stale value while a background refresh runs (stale-while-revalidate).

## Eviction

| Policy | What is removed | Weak spot |
|---|---|---|
| LRU | not used for the longest time | one full scan washes out hot keys |
| LFU | rarely used | slow to forget formerly popular keys |
| FIFO | oldest by write time | ignores accesses |
| Random | a random one | unpredictable but cheap |

O(1) LRU is a dictionary plus a doubly linked list: on access the node moves to the head, on overflow the tail is removed. Implementation: [Ex03_Lru.cs](code/Examples/Ex03_Lru.cs).

## HTTP caching

| Directive | Meaning |
|---|---|
| `max-age` | how many seconds the response is fresh |
| `s-maxage` | the same for shared caches, overrides `max-age` |
| `no-cache` | may be stored, but used only after revalidation with the server |
| `no-store` | do not store |
| `private` | do not store in a shared cache |
| `stale-while-revalidate` | a stale response may be served while refreshing in the background |
| `stale-if-error` | a stale response may be served on a server error |

## L1 and L2

L1 is fast and needs no network, but each instance has its own. `RemoveByTagAsync` in `HybridCache` affects the current instance and L2; it does not touch the memory of other instances. Set a short `LocalCacheExpiration`: it bounds how long they diverge.

## Best practices

- Measure the hit ratio, source latency and eviction count.
- Design the behavior for a cache failure: a timeout, a limit on database load.
- Version keys when the value format changes.
- Limit value size and key count.

## Common mistakes

- ❌ Warming the cache with an identical TTL → ✅ add jitter.
- ❌ Not caching "not found" when nonexistent keys are requested often → ✅ negative caching with a short TTL.
- ❌ A hot key only in L2 → ✅ add L1 or key copies.
- ❌ Caching a personal response in a CDN → ✅ `private` or `no-store`.

## Comparison with other tracks

- Configuring `IMemoryCache`, `HybridCache` and output caching — the [C# recipe](../../../csharp/recipes/caching/overview.en.md).
- LLM prompt caching is a different mechanism with the same idea of reuse: [Claude: prompt caching](../../../claude/modules/05-prompt-caching/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [HybridCache in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [RFC 9111: HTTP Caching](https://www.rfc-editor.org/rfc/rfc9111)
- [RFC 5861: stale-while-revalidate and stale-if-error](https://www.rfc-editor.org/rfc/rfc5861)
