---
title: "Indexes: basics"
lang: en
track: postgresql
kind: module
doc: basics
slug: 05-indexes
part: 2
tags: [index, b-tree, gin, brin, query-plan]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Indexes: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** `SELECT`, `WHERE`, `ORDER BY` · **Time:** ~2 h

## Data for the examples

All plans in this module come from PostgreSQL 17.11 with this table. Plans depend on data and statistics, so on other data they may differ.

```sql
CREATE TABLE events (
    id int PRIMARY KEY, user_id int NOT NULL, status text NOT NULL,
    email text NOT NULL, created_at timestamptz NOT NULL, payload jsonb NOT NULL
);
INSERT INTO events
SELECT i, i % 1000,
       CASE WHEN i % 100 = 0 THEN 'failed' ELSE 'done' END,
       'User' || i || '@example.com',
       timestamptz '2026-01-01 00:00+00' + i * interval '1 minute',
       jsonb_build_object('kind', 'k' || (i % 50))
FROM generate_series(1, 100000) i;
VACUUM ANALYZE events;
SET max_parallel_workers_per_gather = 0;   -- simpler plans for the examples
```

## The first index

```sql
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE user_id = 42;
-- Seq Scan on events
--   Filter: (user_id = 42)

CREATE INDEX events_user_created ON events (user_id, created_at);

EXPLAIN (COSTS OFF) SELECT * FROM events WHERE user_id = 42 ORDER BY created_at DESC LIMIT 10;
-- Limit
--   ->  Index Scan Backward using events_user_created on events
--         Index Cond: (user_id = 42)
```

`Seq Scan` reads the whole table and discards what does not match. `Index Scan` goes to the matching part of the index and fetches rows from the table. The index is ordered by `(user_id, created_at)`, so for one user the rows are already in time order and can be read backwards: there is no sort step.

## What gets an index automatically

`PRIMARY KEY` and `UNIQUE` constraints create unique B-tree indexes. A foreign key does **not** create an index on the referencing column.

## Composite indexes

`(user_id, created_at)` is ordered by the first column, then by the second within it. It serves:

| Query condition | How well |
|---|---|
| `user_id = ?` | well |
| `user_id = ? AND created_at > ?` | well |
| `user_id = ? ORDER BY created_at` | well, no sort |
| `created_at = ?` only | poorly: there is no leading column to narrow the search |

## Selectivity

```sql
CREATE INDEX events_status ON events (status);
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE status = 'done';    -- Seq Scan: 99% of rows
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE status = 'failed';  -- Index Scan: 1% of rows
```

Fetching almost every row through an index costs more than reading the table once, so the planner ignores the index.

## Reading a plan

| Node | Meaning |
|---|---|
| `Seq Scan` | the whole table is read |
| `Index Scan` | index, then the table for each match |
| `Index Only Scan` | the index alone answers, the table only when visibility is unknown |
| `Bitmap Index Scan` + `Bitmap Heap Scan` | collect matching places first, then read table pages in order |

`EXPLAIN` shows the plan, `EXPLAIN ANALYZE` also runs the query and shows actual rows. Be careful: `ANALYZE` really executes `UPDATE` and `DELETE`.

## Beginner mistakes

- ❌ Assuming an index is always used → ✅ check with `EXPLAIN`.
- ❌ Two single-column indexes where the query needs one composite → ✅ shape the index like the query.
- ❌ Forgetting the foreign key column → ✅ index it if you join or delete by it.
- ❌ Testing on ten rows → ✅ plans on tiny tables say nothing.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [PostgreSQL 17: index types](https://www.postgresql.org/docs/17/indexes-types.html)
- [PostgreSQL 17: multicolumn indexes](https://www.postgresql.org/docs/17/indexes-multicolumn.html)
- [PostgreSQL 17: index-only scans](https://www.postgresql.org/docs/17/indexes-index-only-scans.html)
- [PostgreSQL 17: CREATE INDEX](https://www.postgresql.org/docs/17/sql-createindex.html)
