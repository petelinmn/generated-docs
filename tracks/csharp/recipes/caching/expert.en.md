---
title: "Caching: scale and edges"
lang: en
track: csharp
kind: recipe
doc: expert
slug: caching
part: 2
tags: [caching, cache-aside, cache-invalidation, redis, ttl, interview-questions]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: draft
updated: 2026-11-22
---

# Caching: scale and edges

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · [2. Production](advanced.en.md) · **3. Scale and edges** · [4. Quiz](quiz.en.md)

**Prerequisites:** [minimal](basics.en.md) and [production](advanced.en.md). **Time:** ~30 min.

## Scale

- **Several instances.** Each has its own `IMemoryCache`: the same data is loaded N times and can diverge. A shared L2 (Redis) gives one source, but L1 stays local ([production](advanced.en.md#invalidation-between-instances)).
- **A cold start.** After a deploy the cache is empty: all requests go to the DB. Warm the cache (a background job) or ramp traffic up gradually.
- **A stampede and a "hot key".** `HybridCache` collapses simultaneous loads of one key into one (verified: 10 → 1) within a process; across processes a stampede is possible.
- **Volume.** An in-memory cache competes with the application for RAM; set limits and count entry sizes.
- **Write strategies.** Cache-aside (read: cache → source), write-through (write to both), write-behind (asynchronously) — the choice depends on the acceptable inconsistency.

## Edges and gotchas

**1. `GetOrCreateAsync` in `IMemoryCache` does not protect from a stampede.** Verified: 10 simultaneous → 10 loads.

**2. `MemoryDistributedCache` does not work as L2.** `HybridCache` ignores it (verified: 2 loads in two instances).

**3. `RemoveAsync` does not reset other instances' L1.** Another instance returns the old value until `LocalCacheExpiration` elapses (verified).

**4. `SizeLimit` requires `Size`.** Without `Size` — `InvalidOperationException` (verified); when the limit is exceeded a new entry is not added (verified: `c` was not added).

**5. `null` is cached too.** Verified: after `GetOrCreateAsync` returning `null` the key exists with the value `null`. This is "negative caching": useful (do not keep asking for something absent) and dangerous (an absent entry "sticks" until it expires).

**6. Output cache and users.** Responses are cached by path/query; responses with authorization are not cached by default — do not lift this restriction without analysis.

> ⚠️ Verify: the exact rules by which output cache skips requests with `Authorization`/cookies were not run in this material — check the documentation before enabling it for personal data.

**7. L2 serialization.** Objects in L2 are serialized: the type must be supported by the `HybridCache` serializer (built in for `string` and `byte[]`; for others — JSON by default or your own serializer).

**8. Lifetimes and clock drift.** Relative lifetimes are more reliable than absolute ones when node clocks diverge.

**9. Double invalidation.** A tag must be reset in output cache and in `HybridCache` separately — they are different stores.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `IMemoryCache` | fast, simple | local, no stampede protection |
| `HybridCache` | stampede, tags, L1+L2 | L1 is not invalidated between nodes |
| Redis directly | shared, rich structures | network, serialization, operations |
| Output cache | caches the whole response | not for every endpoint |
| A CDN/proxy | outside the app, scales | invalidation through headers |
| In-code memoization | trivial | process lifetime, memory leaks |

## Interview questions

<details><summary>1. What is cache-aside?</summary>

The application reads the cache; on a miss it loads from the source and puts the result in the cache. `GetOrCreateAsync` and `HybridCache` work this way.

</details>

<details><summary>2. What is a cache stampede and how do you protect against it?</summary>

Simultaneous misses on one key start many loads. Protection — a per-key lock or `HybridCache` (verified: 10 simultaneous → 1 load; with `IMemoryCache` — 10).

</details>

<details><summary>3. How is `HybridCache` better than `IMemoryCache`?</summary>

Stampede protection, tags, L1+L2, one serialization API.

</details>

<details><summary>4. How do you invalidate a group of entries?</summary>

Tags: `RemoveByTagAsync` in `HybridCache`, `EvictByTagAsync` in output cache (both verified).

</details>

<details><summary>5. What are L1 and L2?</summary>

L1 is a fast process cache; L2 is a shared distributed one (Redis, SQL) available to all instances.

</details>

<details><summary>6. Why can several instances have stale data even after `RemoveAsync`?</summary>

Other instances' L1 is not reset (verified); they serve the old value until the local lifetime elapses.

</details>

<details><summary>7. How do you limit the size of `IMemoryCache`?</summary>

`SizeLimit` and `Size` on every entry; without `Size` — an exception, on overflow entries are not added (verified).

</details>

<details><summary>8. What is negative caching?</summary>

Caching the absence of a result (`null`): lowers load but fixes "none" for the entry's lifetime.

</details>

<details><summary>9. Trick question: you plugged in `AddDistributedMemoryCache()` and expect a shared cache between two instances. What happens?</summary>

It lives in each process's memory and `HybridCache` does not use it as L2 (verified). You need Redis/SQL and the like.

</details>

<details><summary>10. Trick question: the cache sped up the average time, but complaints about stale data grew. What do you check?</summary>

Lifetimes (L1 vs L2), key completeness, invalidation on write, L1 resets invisible to other nodes, negative caching.

</details>

## Related materials

- Back: [production](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [21 · Application architecture](../../modules/21-architecture/overview.en.md) — consistency and boundaries.
- [19 · Performance](../../modules/19-performance/overview.en.md) — measuring before and after a cache.
- [Kubernetes](../../../kubernetes/overview.en.md) — Redis in a cluster.

## Sources

- [HybridCache library](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Output caching middleware](https://learn.microsoft.com/aspnet/core/performance/caching/output)
- [Cache-aside pattern](https://learn.microsoft.com/azure/architecture/patterns/cache-aside)
