---
title: "Pagination and filtering: scale and edges"
lang: en
track: csharp
kind: recipe
doc: expert
slug: pagination-filtering
part: 2
tags: [pagination, keyset-pagination, dynamic-filtering, sorting, search, interview-questions]
versions: ".NET 11, C# 15; Microsoft.EntityFrameworkCore.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Pagination and filtering: scale and edges

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · [2. Production](advanced.en.md) · **3. Scale and edges** · [4. Quiz](quiz.en.md)

**Prerequisites:** [minimal](basics.en.md) and [production](advanced.en.md). **Time:** ~30 min.

## Scale

- **Millions of rows.** Offset degrades with depth (verified: at position 900,000 15 ms against 0 ms for keyset on a simple key); keyset stays flat if there is a suitable index.
- **The total.** `COUNT(*)` on big tables is slow. Options: an approximate number (DB statistics), a cached counter ([Caching](../caching/overview.en.md)), dropping the total in favor of "is there a next page" (the `size + 1` trick).
- **Full-text search.** `LIKE '%text%'` does not use an ordinary index. For search use DBMS full-text indexes or a separate engine (Elasticsearch, OpenSearch).
- **Arbitrary sorts.** Every allowed sort combination needs an index; allow only a predefined list.
- **Dynamic filters.** An allow-list of fields and operators; do not accept field names/expressions from the client directly.
- **Big exports.** For export — streaming reads (`AsAsyncEnumerable`) or a keyset walk in batches, not one page of a million rows.

> ⚠️ Verify: full-text search performance and specific engine settings were not measured in this material.

## Edges and gotchas

**1. Without `OrderBy` pages are non-deterministic.** The row order is not guaranteed; pages may overlap.

**2. Offset and data changes.** Verified: an insert between requests produced a duplicate `P11` at the page boundary.

**3. A tie-breaker is mandatory.** Verified: keyset by `Price` without `Id` skipped records with an equal price (it returned `P08` first instead of `P06`).

**4. `^1` in a query.** An index "from the end" and ranges cannot be used inside expression trees (CS8790/CS8791) — move the value into a variable.

**5. The tracker state after recreating the DB.** In tests and scripts `ChangeTracker.Clear()` is needed, otherwise repeated inserts fail with a key conflict (verified: `InvalidOperationException` — an instance with the same key is already tracked).

**6. `decimal` in SQLite.** EF Core adds `ef_compare` to comparisons with `decimal`; other DBMSs have different SQL. Check the SQL on the target DBMS.

**7. A cursor must be validated.** A broken cursor → `400` (verified), not `500`; a cursor must not contain sensitive data (Base64 is not encryption).

**8. A user-chosen page size.** Without `Math.Clamp` a client will request `size=1000000` (verified: `1000` limited to `20`).

**9. Changing filters between pages.** A cursor is tied to a set of filters/sort: another filter with the same cursor gives a meaningless result — include them in the cursor or check they match.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Offset | page numbers, simple | degradation, instability |
| Keyset | constant speed, stability | no jump to page N |
| Search-after in a search engine | scale, relevance | infrastructure |
| GraphQL connections (Relay) | standardized cursors | a separate stack |
| Load everything and paginate on the client | the simplest | only for small sets |

## Interview questions

<details><summary>1. How does offset pagination differ from keyset?</summary>

Offset is `Skip/Take` by page number; keyset is "records after the value of the last item's key" with `WHERE` and an index-ordered sort.

</details>

<details><summary>2. Why is offset slow on deep pages?</summary>

The DB reads and discards all skipped rows. Verified: at position 900,000 of 1,000,000 — 15 ms against 0 ms for keyset.

</details>

<details><summary>3. What problems does offset have with changing data?</summary>

Duplicates and gaps at page boundaries (verified: `P11` appeared on both pages after an insert).

</details>

<details><summary>4. Why is a tie-breaker needed in keyset?</summary>

If the sort field is not unique, records with an equal value are skipped; put a unique key at the end of the sort (verified).

</details>

<details><summary>5. How do you learn whether there is a next page without `COUNT`?</summary>

Request `size + 1` records; the extra one means there is a next page (verified in the API).

</details>

<details><summary>6. Why can't you write `items[^1].Id` in a query?</summary>

Expression trees do not support the from-end index (CS8790/CS8791); move the value into a local variable.

</details>

<details><summary>7. How do you defend against `size=1000000`?</summary>

`Math.Clamp(size, 1, max)` (verified: 1000 → 20).

</details>

<details><summary>8. What should the index be for keyset on `(Price, Id)`?</summary>

A composite index `(Price, Id)` matching `WHERE` and `ORDER BY`.

</details>

<details><summary>9. Trick question: `Skip/Take` without `OrderBy` "works" in tests. Why is it a bug?</summary>

The row order is not guaranteed; on another DBMS, plan or after changes the pages start overlapping or losing records. Always an explicit `OrderBy`.

</details>

<details><summary>10. Trick question: a client sends the page-2 cursor with a different filter. What happens?</summary>

A cursor is only a position in a specific sort; with another filter/sort the result is meaningless. Include them in the cursor or verify.

</details>

## Related materials

- Back: [production](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [21 · Application architecture](../../modules/21-architecture/overview.en.md) — query and API boundaries.
- [19 · Performance](../../modules/19-performance/overview.en.md) — how to measure.
- [Linux](../../../linux/overview.en.md) — `tail`/`sort` as an analogue of stream processing.

## Sources

- [EF Core: Pagination](https://learn.microsoft.com/ef/core/querying/pagination)
- [EF Core: Expression trees restrictions](https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-messages/expression-tree-restrictions)
- [Use The Index, Luke](https://use-the-index-luke.com/no-offset)
