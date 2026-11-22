---
title: "Caching: the minimal solution"
lang: en
track: csharp
kind: recipe
doc: basics
slug: caching
part: 2
tags: [caching, memory-cache, hybrid-cache, output-cache, expiration]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: draft
updated: 2026-11-22
---

# Caching: the minimal solution

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Minimal** · [2. Production](advanced.en.md) · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [10 · async/await](../../modules/10-async-await/overview.en.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md). **Time:** ~30 min.

## The task

Do not compute or request the same thing again and again: a DB answer, the result of an external call, a rendered page. A cache lowers latency and load but brings the risk of stale data, so every entry needs a lifetime and a clear way to reset it.

## Three tools

| Tool | Where it stores | When |
|---|---|---|
| `IMemoryCache` | in process memory | a simple local cache, one instance |
| `HybridCache` | L1 in memory + (optionally) L2 in `IDistributedCache` | the modern choice: stampede protection, tags, a shared cache |
| Output caching | ready HTTP responses | cache an endpoint's whole response |

## `IMemoryCache`

```csharp
builder.Services.AddMemoryCache();

var a = await mem.GetOrCreateAsync("k", async e =>
{
    e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(300);
    return await Load();           // an expensive call
});
```

Verified: two consecutive calls — `v1 v1`, the load ran **once**; after expiry (400 ms) — a new load (`v2`, 2 loads in total).

## `HybridCache`

```bash
dotnet add package Microsoft.Extensions.Caching.Hybrid
```

```csharp
builder.Services.AddHybridCache();

var value = await hc.GetOrCreateAsync("h", async ct => await Load(ct));
await hc.RemoveAsync("h");
```

Verified: the entry lifetime (`Expiration = 1 s`) elapsed → a reload (`v2`); `RemoveAsync` → the next call loads again (`v3`).

## Output caching

A cache of an endpoint's ready response (`Microsoft.AspNetCore.OutputCaching`, part of ASP.NET Core):

```csharp
builder.Services.AddOutputCache(o => o.AddPolicy("short", p => p.Expire(TimeSpan.FromSeconds(1)).Tag("items")));
app.UseOutputCache();
app.MapGet("/time", () => $"call {Interlocked.Increment(ref n)}").CacheOutput("short");
```

Verified: two requests in a row — `call 1`, `call 1` (the handler ran once); after 1 s expired — `call 3`; an endpoint without `CacheOutput` runs every time (`call 4`, `call 5`).

## Beginner mistakes

- ❌ A cache without a lifetime → ✅ always set `Expiration`/`AbsoluteExpiration`.
- ❌ A key without query parameters (`"products"` for different filters) → ✅ the key includes all inputs.
- ❌ Caching one user's data under a shared key → ✅ data leaks between users.
- ❌ "Check first, then write" by hand → ✅ `GetOrCreateAsync`/`HybridCache`, otherwise a stampede ([production](advanced.en.md#cache-stampede)).
- ❌ Caching everything → ✅ only the expensive and frequently read.

## Related materials

- Next: [production](advanced.en.md) — stampede, tags, L2, invalidation.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md) — middleware and endpoints.
- [15 · EF Core](../../modules/15-ef-core/overview.en.md) — what to cache from the DB.
- [Resilient HttpClient](../http-client-resilience/overview.en.md) — a cache as degradation when a dependency fails.
- [Python](../../../python/overview.en.md) — `functools.cache` and caches in web frameworks.

## Sources

- [Memory caching in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/memory)
- [HybridCache library](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Output caching middleware](https://learn.microsoft.com/aspnet/core/performance/caching/output)
