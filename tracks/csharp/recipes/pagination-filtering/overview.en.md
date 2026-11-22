---
title: "Pagination and filtering"
lang: en
track: csharp
kind: recipe
doc: overview
slug: pagination-filtering
part: 2
levels: [1, 2, 3, 4]
tags: [pagination, filtering, ef-core, offset-pagination, keyset-pagination, cursor]
versions: ".NET 11, C# 15; Microsoft.EntityFrameworkCore.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Pagination and filtering

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A recipe: serve records in portions and with filters through EF Core. Offset (`Skip/Take`) is simple and gives page numbers, but degrades on deep pages and gives duplicates under changes. Keyset (a cursor "after such-and-such") is stable and fast with a suitable index, but cannot jump to page N and needs a tie-breaker. All examples were verified by running on EF Core 11 + SQLite, including a measurement on a million rows.

## Key ideas

- Always an explicit `OrderBy` (best on a unique key).
- Pagination in the SQL query, not in memory.
- Filters are built in parts (`Where` is deferred).
- Offset: a duplicate at the boundary after an insert; at position 900,000 of 1,000,000 — 15 ms against 0 ms for keyset.
- Keyset on a non-unique field needs `Id` as a tie-breaker.
- An opaque cursor, a limited page size, `size + 1` instead of `COUNT`.
- `^1` cannot be used inside a query expression.

## Minimal example

```csharp
var items = await db.Products.OrderBy(p => p.Id).Skip((page - 1) * size).Take(size).ToListAsync();
```

## When to use and when not

- ✅ Any list that may grow (products, orders, events).
- ✅ Offset: admin panels, small sets, page numbers.
- ✅ Keyset: feeds, infinite scroll, big tables.
- ❌ Offset on big changing tables.
- ❌ Pagination without a sort.
- ❌ Dynamic filters on arbitrary client fields.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Minimal](basics.en.md) | offset, keyset, filters, SQL |
| [2. Production](advanced.en.md) | duplicates, cost, tie-breaker, a cursor API, indexes |
| [3. Scale and edges](expert.en.md) | total, search, exports, gotchas, 10 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp pagination-filtering level 5` |
| 6. Tests | not generated yet — `/learn csharp pagination-filtering level 6` |

## Related materials

- Prerequisites: [07 · LINQ](../../modules/07-linq/overview.en.md), [15 · EF Core](../../modules/15-ef-core/overview.en.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md)
- Nearby: [Caching](../caching/overview.en.md), [Rate limiting](../rate-limiting/overview.en.md), [19 · Performance](../../modules/19-performance/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — pagination in Python APIs
- Recipes and libraries: [Dapper](../../libraries/dapper/overview.en.md), [Mapperly](../../libraries/mapperly/overview.en.md)
