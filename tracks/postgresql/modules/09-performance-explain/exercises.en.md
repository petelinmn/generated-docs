---
title: "Exercises: performance and EXPLAIN"
lang: en
track: postgresql
kind: module
doc: exercises
slug: 09-performance-explain
part: 2
tags: [explain, query-plan, statistics, vacuum, performance]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Exercises: performance and EXPLAIN

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Use the table from [basics](basics.en.md) and check each answer with `EXPLAIN (ANALYZE, BUFFERS)`.

## 1. 🟢 New orders first

**Task:** "the ten oldest new orders" reads 500 rows to return 10. Make it read about 10.

<details><summary>Hint</summary>

Look at `Rows Removed by Filter`. Which rows does the index need to contain?

</details>

<details><summary>Solution</summary>

```sql
CREATE INDEX orders_status_id ON orders (id) WHERE status = 'new';
-- or a composite index (status, id)
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF, COSTS OFF)
SELECT * FROM orders WHERE status = 'new' ORDER BY id LIMIT 10;
```

The plan becomes a scan of the partial index without a filter.

</details>

## 2. 🟡 Fast deep pages

**Task:** paging by 10 rows must not get slower on page 10,000.

<details><summary>Hint</summary>

Start after the last row the client has seen instead of counting rows to skip.

</details>

<details><summary>Solution</summary>

```sql
-- first page
SELECT id, amount FROM orders ORDER BY id LIMIT 10;
-- next page: the client sends the last id it saw
SELECT id, amount FROM orders WHERE id > 10 ORDER BY id LIMIT 10;
```

You cannot jump to an arbitrary page number, but "next page" costs the same everywhere.

</details>

## 3. 🔴 Where autovacuum is behind

**Task:** list the tables with the most dead rows and when they were last vacuumed and analyzed.

<details><summary>Hint</summary>

Statistics per table live in one view.

</details>

<details><summary>Solution</summary>

```sql
SELECT relname, n_live_tup, n_dead_tup, last_autovacuum, last_autoanalyze
FROM pg_stat_user_tables
ORDER BY n_dead_tup DESC
LIMIT 10;
```

`n_live_tup` and `n_dead_tup` are estimates collected by the statistics system and may lag a little.

</details>
