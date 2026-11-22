---
title: "Pagination and filtering: the minimal solution"
lang: en
track: csharp
kind: recipe
doc: basics
slug: pagination-filtering
part: 2
tags: [pagination, filtering, ef-core, offset-pagination, keyset-pagination, linq]
versions: ".NET 11, C# 15; Microsoft.EntityFrameworkCore.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Pagination and filtering: the minimal solution

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Minimal** · [2. Production](advanced.en.md) · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [07 · LINQ](../../modules/07-linq/overview.en.md), [15 · EF Core](../../modules/15-ef-core/overview.en.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md). **Time:** ~30 min.

## The task

Return a list of records from an API not all at once but in portions (pages) and with filters. Without pagination a large list overloads the DB, the network and the client. There are two main ways: **offset** (`Skip/Take`, a page number) and **keyset** (a cursor: "records after such-and-such").

## Offset pagination

```csharp
int page = 2, size = 10;
var q = db.Products.OrderBy(p => p.Id).Skip((page - 1) * size).Take(size);
var items = await q.ToListAsync();
var total = await db.Products.CountAsync();
```

Verified on 25 records (SQLite): page 2 — `P11…P20`, `total = 25`. The generated SQL:

```sql
SELECT "p"."Id", "p"."Category", "p"."Name", "p"."Price"
FROM "Products" AS "p"
ORDER BY "p"."Id"
LIMIT @p OFFSET @p
```

(in the `ToQueryString()` output both parameters are named `@p` with the value 10 — a display quirk: in the example `OFFSET = 10` and `LIMIT = 10`).

**Always specify `OrderBy`**: without an explicit sort the row order is not guaranteed, and pages may overlap or lose records.

## Keyset pagination

Instead of a page number — "the value of the last item":

```csharp
async Task<List<Product>> Keyset(int afterId, int take) =>
    await db.Products.Where(p => p.Id > afterId).OrderBy(p => p.Id).Take(take).ToListAsync();
```

Verified: three consecutive calls (`afterId = 0`, then the `Id` of the last item) returned 10, 10 and 5 records. The SQL is `WHERE "p"."Id" > 10 ORDER BY "p"."Id" LIMIT @p`: the DB goes through the index straight to the right place instead of counting off skipped rows.

## Filtering

Add conditions as needed, building the query in parts (the query does not run until you call `ToListAsync`):

```csharp
var filtered = db.Products.AsQueryable();
if (cat is not null) filtered = filtered.Where(p => p.Category == cat);
if (min is not null) filtered = filtered.Where(p => p.Price >= min);
```

Verified: category `x` and price from 20 gave 9 records; the SQL is `WHERE "p"."Category" = @cat AND ef_compare("p"."Price", @min) >= 0` (SQLite does not store `decimal` natively, so EF adds a call to `ef_compare` — it is absent on other DBMSs).

## Beginner mistakes

- ❌ `Skip/Take` without `OrderBy` → ✅ always an explicit sort (best on a unique key).
- ❌ Returning everything and paginating in memory (`ToList().Skip()`) → ✅ paginate in the query before `ToListAsync`.
- ❌ A page size from the client without a limit → ✅ `Math.Clamp` ([production](advanced.en.md#an-api-with-a-cursor)).
- ❌ `Count` on every request for huge tables → ✅ decide whether an exact total is needed.
- ❌ Building filters by concatenating SQL strings → ✅ LINQ predicates (parameterization protects from SQL injection).

## Related materials

- Next: [production](advanced.en.md) — an API with a cursor, a composite key, stability.
- [07 · LINQ](../../modules/07-linq/overview.en.md) — `IQueryable` and deferred execution.
- [15 · EF Core](../../modules/15-ef-core/overview.en.md) — queries and tracking.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md) — query parameters.
- [Caching](../caching/overview.en.md) — caching pages.
- [Python](../../../python/overview.en.md) — pagination in Python APIs.

## Sources

- [EF Core: Pagination](https://learn.microsoft.com/ef/core/querying/pagination)
- [EF Core: Querying data](https://learn.microsoft.com/ef/core/querying/)
- [EF Core: SQLite provider limitations](https://learn.microsoft.com/ef/core/providers/sqlite/limitations)
