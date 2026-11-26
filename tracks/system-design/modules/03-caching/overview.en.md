---
title: "Caching"
lang: en
track: system-design
kind: module
doc: overview
slug: 03-caching
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, caching, cache-aside, cache-invalidation, cache-stampede, lru-cache, hybrid-cache]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: verified
updated: 2026-11-26
---

# Caching

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A cache keeps a copy of data closer and cheaper than the source. It lowers latency and offloads the database, and in return brings stale data and new kinds of failure. This is the architectural view; configuring ASP.NET Core caches is covered in a [recipe](../../../csharp/recipes/caching/overview.en.md).

## Key ideas

- There is a cache at every level: browser, CDN, application, shared cache, database.
- Cache-aside is the most common pattern: read from the cache, on a miss from the source.
- A TTL bounds staleness, invalidation after a write reduces it.
- Capacity is limited: an eviction policy is needed.
- Expiry of a popular key causes a stampede.
- L1 in memory is fast, but each instance has its own.
- The hit ratio determines how many requests reach the source.

## Minimal example

```csharp
if (cache.TryGet(id, out decimal cached)) return cached;
decimal price = store.Read(id);
cache.Set(id, price, Ttl);
return price;
```

## When to use and when not

- ✅ Reads far outnumber writes.
- ✅ The result is expensive to compute.
- ✅ Slight staleness is acceptable.
- ❌ Decisions that need current data: charging money, stock levels.
- ❌ Every request is unique: there will be no hits.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | cache levels, cache-aside, TTL, invalidation, HybridCache |
| [2. Advanced](advanced.en.md) | write strategies, stampede, eviction, HTTP cache, L1 and L2 |
| [3. Expert](expert.en.md) | races, reference, .NET gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | cache-aside, single-flight, LRU, HybridCache |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [scaling and load balancing](../02-scalability-and-load-balancing/overview.en.md)
- Practice in .NET: [caching recipe](../../../csharp/recipes/caching/overview.en.md)
- Load estimates: [design process](../01-design-process/overview.en.md)
- [Case studies](../17-design-case-studies/overview.en.md)
