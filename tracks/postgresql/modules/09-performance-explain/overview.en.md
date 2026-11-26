---
title: "Performance and EXPLAIN"
lang: en
track: postgresql
kind: module
doc: overview
slug: 09-performance-explain
part: 2
levels: [1, 2, 3, 4]
tags: [explain, query-plan, statistics, vacuum, performance]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Performance and EXPLAIN

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

The planner picks a plan by estimating costs, and the estimates come from table statistics. Most slow queries are explained by one of a few things: the plan reads far more rows than it returns, the estimates are wrong, an operation does not fit into memory, or the table is bloated with dead rows. `EXPLAIN` shows the plan, `EXPLAIN ANALYZE` runs it and shows what really happened, and `pg_stat_statements` tells which queries deserve attention.

## Key ideas

- Find the expensive queries first, by total time, not by a hunch.
- Compare estimated `rows` with actual rows: a large gap means bad statistics.
- `EXPLAIN ANALYZE` really executes the statement.
- Actual rows and time are per loop: multiply by `loops`.
- A sort or a hash that does not fit into `work_mem` goes to disk.
- `ANALYZE` refreshes statistics, `VACUUM` makes space of dead rows reusable.
- A setting that changes plans should be changed with evidence, not by advice.

## Minimal example

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

## When to use and when not

- ✅ A query is slow or its time has grown.
- ✅ Before and after adding an index or rewriting a query.
- ❌ Tuning the server before the expensive queries are known.
- ❌ Judging a plan on a database with a few rows.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | `EXPLAIN`, costs, `EXPLAIN ANALYZE`, buffers |
| [2. Advanced](advanced.en.md) | estimates and statistics, memory, loops, `OFFSET`, `VACUUM` and bloat |
| [3. Expert](expert.en.md) | generic plans, `pg_stat_statements`, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn postgresql 09-performance-explain level 5` |
| 6. Tests | not created yet — `/learn postgresql 09-performance-explain level 6` |

## Related materials

- Prerequisites: [05 · Indexes](../05-indexes/overview.en.md), [06 · Transactions](../06-transactions/overview.en.md)
- Next: the following module of the [PostgreSQL roadmap](../../overview.en.md)
- Measuring the whole system: [observability and SLOs](../../../system-design/modules/14-observability-and-slo/overview.en.md)
- From .NET: [EF Core](../../../csharp/modules/15-ef-core/overview.en.md)
- [Query plans and storage engines: data storage](../../../system-design/modules/04-data-storage/overview.en.md)
