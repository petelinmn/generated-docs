---
title: "Caching: production"
lang: en
track: csharp
kind: recipe
doc: advanced
slug: caching
part: 2
tags: [caching, cache-stampede, hybrid-cache, distributed-cache, invalidation, tags]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: draft
updated: 2026-11-22
---

# Caching: production

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · **2. Production** · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [the minimal solution](basics.en.md). **Time:** ~45 min.

## Cache stampede

When an entry expires and N requests arrive at once, a naive cache starts N identical expensive loads. Verified on 10 simultaneous calls for an empty key:

| Cache | Loads |
|---|---|
| `IMemoryCache.GetOrCreateAsync` | **10** — the factories are not synchronized |
| `HybridCache.GetOrCreateAsync` | **1** — the others wait for the first load |

This is the main reason to pick `HybridCache`. With `IMemoryCache` you have to write the protection (a per-key lock, `SemaphoreSlim`) yourself.

## Tags and invalidation

`HybridCache` lets you tag entries and reset a group:

```csharp
await hc.GetOrCreateAsync("p1", async ct => await Load(), tags: ["products"]);
await hc.GetOrCreateAsync("p2", async ct => await Load(), tags: ["products"]);
await hc.RemoveByTagAsync("products");
```

Verified: after `RemoveByTagAsync("products")` both entries loaded again (2 reloads). `EvictByTagAsync` in output cache works the same way (verified: after evicting the tag the next request ran the handler — `call 2`).

## A two-level cache: L1 + L2

`HybridCache` keeps L1 (process memory) and, if an `IDistributedCache` (Redis, SQL Server…) is registered, L2 shared between instances:

```csharp
builder.Services.AddStackExchangeRedisCache(o => o.Configuration = "localhost:6379");   // L2
builder.Services.AddHybridCache();
```

> ⚠️ Verify: Redis was not run in this material (no server); check the package and connection settings against the documentation. The two-level behavior was verified on a wrapper over `MemoryDistributedCache` (below).

Verified on two "instances" with a shared L2: the first loaded `Ann`, the second got `Ann` from L2 without loading (1 load in total).

**A gotcha:** if you register `MemoryDistributedCache` itself as L2 (`AddDistributedMemoryCache()`), `HybridCache` **does not use it**: in the second instance the load ran again (`Bob`, 2 loads), nothing reached L2. Verified; it makes sense — it is a per-process cache, not a shared one. A shared L2 needs a real distributed cache.

## Invalidation between instances

`RemoveAsync` on one instance removes the entry from L2 and its own L1, but **other instances' L1 is not reset**: verified — after `RemoveAsync` on the first instance the second still returned `Ann` from its L1 (no new loads). Therefore:

- set a short `LocalCacheExpiration` (L1) so staleness is bounded;
- for strict consistency do not cache, or add a notification mechanism (pub/sub, a bus).

## Entry options

```csharp
services.AddHybridCache(o => o.DefaultEntryOptions = new HybridCacheEntryOptions
{
    Expiration = TimeSpan.FromMinutes(5),            // L2 (and overall) lifetime
    LocalCacheExpiration = TimeSpan.FromMinutes(1)   // L1 lifetime
});
```

`IMemoryCache` manages size: with a `SizeLimit` every entry must specify `Size`. Verified: a limit of 2 and three entries of size 1 — the last one was not added because of the overflow (`a=True b=True c=False`, `Count=2`); an entry **without** `Size` under a `SizeLimit` is an `InvalidOperationException`.

## What to cache and what not

| Good | Bad |
|---|---|
| reference data that rarely changes | data with strict consistency (a balance) |
| results of expensive queries | personal data under a shared key |
| external API responses (mind their limits) | entries without clear invalidation |

## Best practices

| Rule | Why |
|---|---|
| A lifetime on every entry | bounds staleness |
| The key includes all inputs | no collisions |
| `HybridCache` instead of hand-made `IMemoryCache` + locks | stampede protection |
| A short L1 with several instances | L1 is not invalidated between nodes |
| Hit/miss metrics | see the cache's benefit ([22](../../modules/22-observability/overview.en.md)) |
| Do not cache errors for long | do not entrench a failure |

## Related materials

- Back: [minimal](basics.en.md). Next: [scale and edges](expert.en.md).
- [22 · Observability](../../modules/22-observability/overview.en.md) — cache metrics.
- [Rate limiting](../rate-limiting/overview.en.md) — lowering load without rejections.
- [Background jobs](../background-jobs/overview.en.md) — cache warm-up.

## Sources

- [HybridCache library](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Distributed caching](https://learn.microsoft.com/aspnet/core/performance/caching/distributed)
- [Memory caching: size limit](https://learn.microsoft.com/aspnet/core/performance/caching/memory)
