---
title: "Indexes"
lang: en
track: postgresql
kind: module
doc: overview
slug: 05-indexes
part: 2
levels: [1, 2, 3, 4]
tags: [index, b-tree, gin, brin, query-plan]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Indexes

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

An index is a separate structure that lets PostgreSQL find rows without reading the whole table. It makes some queries dramatically cheaper and every write a bit more expensive, and it takes disk space. The planner decides whether to use it: a good index matches the shape of a real query, not just a column.

## Key ideas

- B-tree is the default and serves equality, ranges and ordering.
- The column order of a composite index decides which queries it serves well.
- For a large share of the table a sequential scan is cheaper than an index.
- Partial and expression indexes cover exactly what a query asks.
- An index-only scan skips the table only while the visibility map allows it.
- GIN serves "contains" searches over `jsonb` and arrays, BRIN is tiny for naturally ordered data.
- `CREATE INDEX CONCURRENTLY` keeps writes running but can leave an invalid index.

## Minimal example

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

## When to use and when not

- ✅ A frequent query reads a small part of a large table.
- ✅ Sorting plus `LIMIT` that an index can deliver in order.
- ❌ A small table or a query returning most rows.
- ❌ An index on every column "just in case".

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | B-tree, composite indexes, reading a plan, selectivity |
| [2. Advanced](advanced.en.md) | partial, expression, covering, GIN, BRIN, building without blocking |
| [3. Expert](expert.en.md) | visibility map, HOT, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn postgresql 05-indexes level 5` |
| 6. Tests | not created yet — `/learn postgresql 05-indexes level 6` |

## Related materials

- Prerequisites: [02 · SQL queries](../02-sql-queries/overview.en.md), [04 · Joins and subqueries](../04-joins-and-subqueries/overview.en.md)
- Next: the following module of the [PostgreSQL roadmap](../../overview.en.md)
- Why indexes shape a design: [data storage](../../../system-design/modules/04-data-storage/overview.en.md)
- From .NET: [EF Core](../../../csharp/modules/15-ef-core/overview.en.md)
- A different index structure: [OpenSearch text analysis](../../../opensearch/modules/02-mappings-and-analysis/overview.en.md)
