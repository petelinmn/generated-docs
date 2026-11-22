---
title: "Caching"
lang: en
track: csharp
kind: recipe
doc: overview
slug: caching
part: 2
levels: [1, 2, 3, 4]
tags: [caching, memory-cache, hybrid-cache, output-cache, cache-stampede, redis]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: draft
updated: 2026-11-22
---

# Caching

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A recipe: do not compute the same thing twice. `IMemoryCache` is a simple local cache; `HybridCache` is L1 in memory plus an optional shared L2 (`IDistributedCache`, for example Redis), stampede protection and tags; output caching caches ready HTTP responses. The main risks are stale data, a stampede and inconsistency between instances. Redis was not run in this material.

## Key ideas

- Every entry has a lifetime and a key that includes all inputs.
- `GetOrCreateAsync` — cache-aside in one line.
- `IMemoryCache` does not protect from a stampede (10 simultaneous → 10 loads), `HybridCache` does (→ 1).
- Tags: `RemoveByTagAsync` (Hybrid), `EvictByTagAsync` (output cache).
- L1 is not invalidated between instances — set a short `LocalCacheExpiration`.
- `AddDistributedMemoryCache()` does not work as a shared L2.
- `null` is cached — be careful with negative caching.

## Minimal example

```csharp
builder.Services.AddHybridCache();
var value = await hc.GetOrCreateAsync("key", async ct => await Load(ct));
```

## When to use and when not

- ✅ Expensive and frequently read data: reference data, aggregates, external APIs.
- ✅ Whole responses of public endpoints (output cache).
- ✅ Degradation when a dependency fails.
- ❌ Data with strict consistency (balances, stock).
- ❌ Personal data under a shared key.
- ❌ A cache without metrics and invalidation.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Minimal](basics.en.md) | `IMemoryCache`, `HybridCache`, output caching |
| [2. Production](advanced.en.md) | stampede, tags, L1+L2, invalidation between instances, size |
| [3. Scale and edges](expert.en.md) | several instances, cold start, gotchas, 10 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp caching level 5` |
| 6. Tests | not generated yet — `/learn csharp caching level 6` |

## Related materials

- Prerequisites: [10 · async/await](../../modules/10-async-await/overview.en.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md)
- Nearby: [15 · EF Core](../../modules/15-ef-core/overview.en.md), [19 · Performance](../../modules/19-performance/overview.en.md), [Resilient HttpClient](../http-client-resilience/overview.en.md), [Rate limiting](../rate-limiting/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — `functools.cache`
- Recipes and libraries: [Pagination and filtering](../pagination-filtering/overview.en.md)
