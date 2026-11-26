---
title: "Performance and EXPLAIN: advanced"
lang: en
track: postgresql
kind: module
doc: advanced
slug: 09-performance-explain
part: 2
tags: [explain, query-plan, statistics, vacuum, performance]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Performance and EXPLAIN: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Estimates come from statistics

```sql
CREATE TABLE fresh (id int, v int);
ALTER TABLE fresh SET (autovacuum_enabled = false);
INSERT INTO fresh SELECT i, i % 10 FROM generate_series(1, 100000) i;

EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF) SELECT * FROM fresh WHERE v = 3;
-- Seq Scan on fresh  (cost=0.00..1694.48 rows=501 width=8) (actual rows=10000 loops=1)

ANALYZE fresh;
-- the same EXPLAIN:  (cost=0.00..1693.00 rows=10087 width=8) (actual rows=10000 loops=1)
```

Before `ANALYZE` the planner guessed 501 rows instead of 10,000. Autovacuum also runs `ANALYZE`, but after a bulk load it is worth running it yourself. Statistics are collected from a sample; `default_statistics_target` (100) sets its size and the length of the most-common-values lists, and can be raised per column with `ALTER TABLE ... ALTER COLUMN ... SET STATISTICS`.

## Correlated columns

```sql
-- every city belongs to exactly one country
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF)
SELECT * FROM orders WHERE city = 'city42' AND country = 'country4';
-- (cost=... rows=201 ...) (actual rows=2000 loops=1)

CREATE STATISTICS orders_city_country (dependencies) ON city, country FROM orders;
ANALYZE orders;
-- the same EXPLAIN:  rows=2067 ... (actual rows=2000 loops=1)
```

The planner assumes conditions are independent and multiplies their selectivities, which gives 201 instead of 2000. Extended statistics teach it the dependency. The kinds are `dependencies`, `ndistinct` and `mcv`.

## Memory: sorts and hashes

```sql
SET work_mem = '64kB';
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF) SELECT * FROM orders ORDER BY amount;
-- Sort Method: external merge  Disk: 11408kB

SET work_mem = '64MB';
-- the same EXPLAIN:  Sort Method: quicksort  Memory: 18629kB
RESET work_mem;
```

```sql
SET enable_mergejoin = off;   -- only to show a hash join on this data
SET work_mem = '64kB';
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF, COSTS OFF)
SELECT count(*) FROM orders o JOIN orders p ON p.id = o.customer_id;
--   ->  Hash (actual rows=200000 loops=1)
--         Buckets: 4096  Batches: 128  Memory Usage: 89kB

SET work_mem = '64MB';
--         Buckets: 262144  Batches: 1  Memory Usage: 9080kB
```

`work_mem` (4 MB by default) is the limit **per operation**: one query may have several sorts and hashes, and many sessions run at once. Hash operations may use `work_mem × hash_mem_multiplier` (2.0 by default). Raise it for a session or a role that runs reports, not blindly for the whole server.

## Loops

```sql
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF, COSTS OFF)
SELECT c.id, (SELECT count(*) FROM orders o WHERE o.customer_id = c.id)
FROM generate_series(1, 5) c(id);
```

```text
Function Scan on generate_series c (actual rows=5 loops=1)
  SubPlan 1
    ->  Aggregate (actual rows=1 loops=5)
          ->  Index Only Scan using orders_customer on orders o (actual rows=40 loops=5)
                Index Cond: (customer_id = c.id)
```

`actual rows=40 loops=5` means 40 rows per execution, 200 in total. Times in such nodes are per loop too.

## OFFSET reads what it skips

```sql
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF, COSTS OFF)
SELECT * FROM orders ORDER BY id OFFSET 100000 LIMIT 10;
-- Limit (actual rows=10 loops=1)
--   ->  Index Scan using orders_pkey on orders (actual rows=100010 loops=1)

EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF, COSTS OFF)
SELECT * FROM orders WHERE id > 100000 ORDER BY id LIMIT 10;
-- Limit (actual rows=10 loops=1)
--   ->  Index Scan using orders_pkey on orders (actual rows=10 loops=1)
--         Index Cond: (id > 100000)
```

Keyset pagination remembers the last key and starts after it.

## VACUUM and bloat

```sql
CREATE TABLE b (id int PRIMARY KEY, v int);
ALTER TABLE b SET (autovacuum_enabled = false);
INSERT INTO b SELECT i, 0 FROM generate_series(1, 100000) i;
VACUUM ANALYZE b;                                   -- size 3544 kB

UPDATE b SET v = 1;
SELECT pg_stat_force_next_flush();
SELECT n_dead_tup FROM pg_stat_user_tables WHERE relname = 'b';   -- 100000
                                                    -- size 7080 kB
VACUUM b;
UPDATE b SET v = 2;                                 -- still 7080 kB: freed space is reused
VACUUM FULL b;                                      -- 3544 kB: the table is rewritten
```

An `UPDATE` writes a new row version; the old one becomes dead. `VACUUM` marks the space as free for new versions but usually does not shrink the file. `VACUUM FULL` rewrites the table and holds an `ACCESS EXCLUSIVE` lock for the whole time, so it is not something to run casually.

Autovacuum vacuums a table when the dead rows exceed `autovacuum_vacuum_threshold + autovacuum_vacuum_scale_factor × rows` (50 + 0.2 × rows by default) and analyzes it with 50 + 0.1 × rows changed rows. For a table of a hundred million rows that is twenty million dead rows; large hot tables get their own lower settings.

## Best practices

- Change one thing at a time and measure before and after.
- Run `ANALYZE` after bulk loads and large deletes.
- Tune autovacuum per table for large tables with many updates.
- Watch long transactions: they keep `VACUUM` from removing old versions.

## Common mistakes

- ❌ A global `work_mem` of gigabytes → ✅ per role or per session.
- ❌ `VACUUM FULL` on a schedule → ✅ ordinary `VACUUM`, and find the cause of bloat.
- ❌ Disabling autovacuum "for speed" → ✅ it prevents bloat and wraparound.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [PostgreSQL 17: using EXPLAIN](https://www.postgresql.org/docs/17/using-explain.html)
- [PostgreSQL 17: EXPLAIN](https://www.postgresql.org/docs/17/sql-explain.html)
- [PostgreSQL 17: planner statistics](https://www.postgresql.org/docs/17/planner-stats.html)
- [PostgreSQL 17: resource consumption](https://www.postgresql.org/docs/17/runtime-config-resource.html)
- [PostgreSQL 17: routine vacuuming](https://www.postgresql.org/docs/17/routine-vacuuming.html)
- [PostgreSQL 17: autovacuum settings](https://www.postgresql.org/docs/17/runtime-config-autovacuum.html)
