---
title: "Pagination and filtering: production"
lang: en
track: csharp
kind: recipe
doc: advanced
slug: pagination-filtering
part: 2
tags: [pagination, keyset-pagination, cursor, ef-core, api-design, performance]
versions: ".NET 11, C# 15; Microsoft.EntityFrameworkCore.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Pagination and filtering: production

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · **2. Production** · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [the minimal solution](basics.en.md). **Time:** ~45 min.

## Why offset breaks

**Duplicates and gaps under changes.** Verified: 20 records, "newest first" ordering. Page 1 — 10 records (`P20…P11`); between requests `NEW` was added; page 2 (`Skip(10)`) again contains `P11` — a duplicate at the boundary. The keyset variant (`Id < last id`) gave no duplicates (0) after the same insert.

**The cost of a deep offset.** The DB has to walk through and discard all skipped rows. Measured on SQLite with 1,000,000 rows (the average of 5 runs, 20 records per page):

| Position | offset | keyset |
|---|---|---|
| 0 | 0 ms | 0 ms |
| 100,000 | 1 ms | 0 ms |
| 900,000 | 15 ms | 0 ms |

For SQLite and a simple indexed key the difference is small in absolute numbers, but it grows linearly with the position; on "heavy" queries (sorting on a non-indexed field, joins) it is many times larger.

## A composite sort key

If the sort is not on a unique field (price, date), keyset needs a **tie-breaker** — a unique key at the end. Verified on 12 records with repeating prices: after the element `(Price = 20, Id = 5)` the naive query `Price > 20` skipped the other records with price 20 (it returned `P08` first), while the correct `Price > 20 OR (Price == 20 AND Id > 5)` continued from `P06`:

```csharp
var page = await db.Products
    .Where(p => p.Price > lp || (p.Price == lp && p.Id > lid))
    .OrderBy(p => p.Price).ThenBy(p => p.Id)
    .Take(5).ToListAsync();
```

> You cannot use `^1` and range indexers (`items[^1].Id`) in a query expression: the compiler forbids them in expression trees (errors CS8790/CS8791). Move the value into a local variable.

## An API with a cursor

Make the cursor opaque (Base64) and return it in the response. Take `size + 1` records to learn whether there is a next page:

```csharp
app.MapGet("/products", async (ShopDb db, string? cursor, int? size, string? category) =>
{
    var take = Math.Clamp(size ?? 10, 1, 20);                       // max page size
    int afterId = 0;
    if (cursor is not null)
    {
        try { afterId = int.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(cursor))); }
        catch (Exception) { return Results.BadRequest("invalid cursor"); }
    }
    var q = db.Products.AsNoTracking().Where(p => p.Id > afterId);
    if (category is not null) q = q.Where(p => p.Category == category);
    var items = await q.OrderBy(p => p.Id).Take(take + 1).ToListAsync();
    var hasMore = items.Count > take;
    if (hasMore) items.RemoveAt(items.Count - 1);
    var next = hasMore ? Convert.ToBase64String(Encoding.UTF8.GetBytes(items[^1].Id.ToString())) : null;
    return Results.Ok(new { items = items.Select(i => i.Name), nextCursor = next });
});
```

Verified by running (25 records, `size=10`): page 1 — 10 records and the cursor `MTA=`, page 2 — 10 and `MjA=`, page 3 — 5 and `null`; 25 records in total, all distinct. `size=1000` is limited to 20; the filter `category=even` returned 12 records; a broken cursor → `400`. For read-only queries use `AsNoTracking()` ([15](../../modules/15-ef-core/overview.en.md)).

## Indexes

Keyset is fast only with an index matching `WHERE` and `ORDER BY`. Sorting by `(Price, Id)` needs a composite index `(Price, Id)`; filtering by `Category` needs an index on it (or a composite `(Category, Id)`).

```csharp
modelBuilder.Entity<Product>().HasIndex(p => new { p.Category, p.Id });
```

> ⚠️ Verify: the effect of specific indexes on query plans was not measured in this material — check your DBMS's `EXPLAIN`.

## Offset or keyset

| | Offset | Keyset |
|---|---|---|
| Jump to page N | yes | no (only forward/back) |
| Stability under changes | duplicates/gaps | stable |
| Speed on deep pages | drops | constant |
| Complexity | low | higher (cursor, tie-breaker) |
| Fits | admin panels, small sets | feeds, infinite scroll, big tables |

## Best practices

| Rule | Why |
|---|---|
| An explicit `OrderBy` on a unique key | deterministic pages |
| An upper bound on the page size | protects the DB |
| An opaque cursor | the internals can change without breaking clients |
| `AsNoTracking` for reads | less memory |
| Indexes for the filter and the sort | speed |
| A total only if needed | `COUNT` on big tables is expensive |

## Related materials

- Back: [minimal](basics.en.md). Next: [scale and edges](expert.en.md).
- [15 · EF Core](../../modules/15-ef-core/overview.en.md) — indexes and tracking.
- [19 · Performance](../../modules/19-performance/overview.en.md) — measuring.
- [Rate limiting](../rate-limiting/overview.en.md) — protection from mass page crawling.

## Sources

- [EF Core: Pagination](https://learn.microsoft.com/ef/core/querying/pagination)
- [EF Core: Indexes](https://learn.microsoft.com/ef/core/modeling/indexes)
- [Use The Index, Luke: Fetching the next page](https://use-the-index-luke.com/no-offset)
