---
title: "Quiz: indexes"
lang: en
track: postgresql
kind: module
doc: quiz
slug: 05-indexes
part: 2
tags: [index, b-tree, gin, brin, query-plan]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Quiz: indexes

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which index type does `CREATE INDEX` create by default?

- A) Hash
- B) B-tree
- C) GIN
- D) BRIN

<details><summary>Answer</summary>

**B.** The others require `USING`.

→ [Basics](basics.en.md)

</details>

## 2. 🟢 Which constraint does NOT create an index by itself?

- A) `PRIMARY KEY`
- B) `UNIQUE`
- C) `FOREIGN KEY`
- D) None of them

<details><summary>Answer</summary>

**C.** The referencing columns get no automatic index.

→ [Basics](basics.en.md)

</details>

## 3. 🟢 Which query is served best by an index `(user_id, created_at)`?

- A) `WHERE created_at = ?`
- B) `WHERE user_id = ? ORDER BY created_at DESC`
- C) `WHERE status = ?`
- D) `ORDER BY email`

<details><summary>Answer</summary>

**B.** Equality on the leading column plus order by the second. A can only walk the whole index.

→ [Basics](basics.en.md)

</details>

## 4. 🟢 99% of rows have `status = 'done'`. What will the planner likely do for it?

- A) Index Scan
- B) Index Only Scan
- C) Seq Scan
- D) Refuse the query

<details><summary>Answer</summary>

**C.** Reading the table once is cheaper than fetching nearly every row through the index.

→ [Basics](basics.en.md)

</details>

## 5. 🟡 Will this index help `WHERE lower(email) = ?`

```sql
CREATE INDEX ON events (email);
```

<details><summary>Answer</summary>

No. It stores `email`, not `lower(email)`; an expression index is needed.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 Select all true statements about partial indexes

- [ ] A) It contains only rows matching its `WHERE`
- [ ] B) It serves any query on the indexed column
- [ ] C) It is usually smaller than a full index
- [ ] D) The query condition must imply the index condition

<details><summary>Answer</summary>

**A, C, D.** A query for other rows cannot use it.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 `Heap Fetches` became large after mass updates. What helps?

- A) `VACUUM`
- B) `ANALYZE` only
- C) Dropping the `INCLUDE` columns
- D) A hash index

<details><summary>Answer</summary>

**A.** `VACUUM` marks pages all-visible in the visibility map; `ANALYZE` only collects statistics.

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Which condition does a default GIN index on `payload` serve?

- A) `payload ->> 'kind' = 'k7'`
- B) `payload @> '{"kind":"k7"}'`
- C) `length(payload::text) > 10`
- D) `ORDER BY payload`

<details><summary>Answer</summary>

**B.** Containment is a GIN operator; the `->>` form needs an expression index.

→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 Find the problem in the migration

```sql
BEGIN;
CREATE INDEX CONCURRENTLY ix ON events (user_id);
COMMIT;
```

<details><summary>Answer</summary>

`CREATE INDEX CONCURRENTLY` cannot run inside a transaction block.

→ [Advanced](advanced.en.md)

</details>

## 10. 🔴 Which index type can be `UNIQUE`?

- A) B-tree
- B) Hash
- C) GIN
- D) BRIN

<details><summary>Answer</summary>

**A.** Only B-tree supports uniqueness.

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Which updates can be HOT?

- A) Any update
- B) One changing no indexed column (apart from summarizing indexes) with room on the page
- C) Only on tables without indexes
- D) Only after `VACUUM FULL`

<details><summary>Answer</summary>

**B.** Both conditions are required.

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: a query became slow although "the index is used". How do you investigate?

<details><summary>Key points</summary>

- `EXPLAIN (ANALYZE, BUFFERS)`: actual rows, buffers, which conditions are in `Index Cond` and which in `Filter`.
- Is the whole index walked? Is there a leading column condition?
- `Heap Fetches` and `VACUUM`, bloat, stale statistics.
- Compare with a better shaped index on a copy of the data.

→ [Expert](expert.en.md)

</details>
