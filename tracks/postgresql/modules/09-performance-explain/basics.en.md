---
title: "Performance and EXPLAIN: basics"
lang: en
track: postgresql
kind: module
doc: basics
slug: 09-performance-explain
part: 2
tags: [explain, query-plan, statistics, vacuum, performance]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Performance and EXPLAIN: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [indexes](../05-indexes/overview.en.md) · **Time:** ~2 h

## Data for the examples

All results were produced on PostgreSQL 17.11. Plans and times depend on data and hardware.

```sql
CREATE TABLE orders (
    id int PRIMARY KEY, customer_id int NOT NULL, status text NOT NULL,
    city text NOT NULL, country text NOT NULL,
    amount numeric(10,2) NOT NULL, created_at timestamptz NOT NULL
);
INSERT INTO orders
SELECT i, i % 5000,
       CASE WHEN i % 50 = 0 THEN 'new' ELSE 'done' END,
       'city' || (i % 100), 'country' || (i % 100 / 10),
       (i % 1000) / 10.0,
       timestamptz '2026-01-01 00:00+00' + i * interval '30 seconds'
FROM generate_series(1, 200000) i;
CREATE INDEX orders_customer ON orders (customer_id);
VACUUM ANALYZE orders;
SET max_parallel_workers_per_gather = 0;   -- simpler plans for the examples
```

## Reading EXPLAIN

```sql
EXPLAIN SELECT * FROM orders WHERE customer_id = 42;
```

```text
Bitmap Heap Scan on orders  (cost=4.60..147.55 rows=40 width=41)
  Recheck Cond: (customer_id = 42)
  ->  Bitmap Index Scan on orders_customer  (cost=0.00..4.59 rows=40 width=0)
        Index Cond: (customer_id = 42)
```

| Field | Meaning |
|---|---|
| `cost=4.60..147.55` | startup cost (before the first row) and total cost |
| `rows=40` | estimated rows returned by the node |
| `width=41` | estimated average row size in bytes |

Costs are in arbitrary units: by convention reading one page sequentially costs 1 (`seq_page_cost`). They are useful only for comparing plans of the same query. A plan is a tree. Execution is driven from the top: each node asks its children for rows, so a `Limit` can stop its children early, and the inner side of a nested loop runs once per outer row.

## EXPLAIN ANALYZE

```sql
EXPLAIN (ANALYZE, BUFFERS, TIMING OFF) SELECT * FROM orders WHERE customer_id = 42;
```

```text
Bitmap Heap Scan on orders  (cost=4.60..147.55 rows=40 width=41) (actual rows=40 loops=1)
  Recheck Cond: (customer_id = 42)
  Heap Blocks: exact=40
  Buffers: shared hit=40 read=2
  ->  Bitmap Index Scan on orders_customer  (cost=0.00..4.59 rows=40 width=0) (actual rows=40 loops=1)
        Index Cond: (customer_id = 42)
        Buffers: shared read=2
Planning Time: 0.036 ms
Execution Time: 0.095 ms
```

`actual rows` is what the node really returned. Here the estimate and reality agree: 40 and 40. `TIMING OFF` hides per-node times, which cost something to measure and vary from run to run.

`BUFFERS` shows 8 kB pages: `shared hit` came from PostgreSQL's cache, `read` from the operating system or disk. In PostgreSQL 17 `BUFFERS` has to be requested explicitly; PostgreSQL 18 includes it in `EXPLAIN ANALYZE` automatically.

## ANALYZE really runs the statement

```sql
BEGIN;
EXPLAIN (ANALYZE, COSTS OFF) DELETE FROM orders WHERE id = 1;   -- the row is really deleted
ROLLBACK;                                                       -- and comes back
```

## What to look at first

1. The node with the largest share of time or buffers.
2. Estimated `rows` against actual rows.
3. `Rows Removed by Filter`: rows read only to be thrown away.
4. Sorts and hashes that went to disk.

## Beginner mistakes

- ❌ Reading only the total cost → ✅ compare estimates and actual rows node by node.
- ❌ `EXPLAIN ANALYZE` of an `UPDATE` on production → ✅ inside `BEGIN ... ROLLBACK`, or on a copy.
- ❌ Measuring once, cold → ✅ several runs; know whether the cache was warm.
- ❌ Looking at a plan on ten rows → ✅ realistic data volumes.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [PostgreSQL 17: using EXPLAIN](https://www.postgresql.org/docs/17/using-explain.html)
- [PostgreSQL 17: EXPLAIN](https://www.postgresql.org/docs/17/sql-explain.html)
- [PostgreSQL 17: planner statistics](https://www.postgresql.org/docs/17/planner-stats.html)
