---
title: "Performance and EXPLAIN: expert"
lang: en
track: postgresql
kind: module
doc: expert
slug: 09-performance-explain
part: 2
tags: [explain, query-plan, statistics, vacuum, performance]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Performance and EXPLAIN: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

The planner enumerates access paths and join orders, estimates the cost of each from statistics (`pg_statistic`, readable through `pg_stats`) and cost settings, and keeps the cheapest. Selectivity comes from the most common values, histograms and the number of distinct values; the `correlation` of a column with the physical order affects index scan costs.

```sql
SELECT attname, n_distinct, correlation FROM pg_stats
WHERE tablename = 'orders' AND attname IN ('id', 'status', 'city');
-- city    100  0.017
-- id       -1  1
-- status    2  0.96
```

A negative `n_distinct` is a fraction of rows: `-1` means every value is unique.

### Prepared statements and generic plans

```sql
CREATE TABLE skew (id int PRIMARY KEY, kind text);
INSERT INTO skew
SELECT i, CASE WHEN i <= 99000 THEN 'common' ELSE 'rare' || (i % 10) END
FROM generate_series(1, 100000) i;
CREATE INDEX skew_kind ON skew (kind);
ANALYZE skew;

PREPARE q(text) AS SELECT count(*) FROM skew WHERE kind = $1;
EXPLAIN (COSTS OFF) EXECUTE q('rare1');    -- Index Only Scan, Index Cond: (kind = 'rare1')
EXPLAIN (COSTS OFF) EXECUTE q('common');   -- Seq Scan, Filter: (kind = 'common')

SET plan_cache_mode = force_generic_plan;
EXPLAIN (COSTS OFF) EXECUTE q('common');   -- Index Only Scan, Index Cond: (kind = $1)
```

With the default `plan_cache_mode = auto` the first five executions get custom plans for the actual values; then a generic plan is used if its estimated cost is not much worse than the average custom one. For skewed data a generic plan may suit one value badly; `force_custom_plan` is the remedy for such a query.

### pg_stat_statements

```sql
-- server started with shared_preload_libraries = 'pg_stat_statements'
CREATE EXTENSION pg_stat_statements;
SELECT count(*) FROM orders WHERE customer_id = 1;
SELECT count(*) FROM orders WHERE customer_id = 2;

SELECT query, calls FROM pg_stat_statements
WHERE query LIKE 'SELECT count(*) FROM orders%';
-- SELECT count(*) FROM orders WHERE customer_id = $1 | 2

-- the usual question: what takes the most time overall
SELECT calls, round(total_exec_time) AS total_ms, round(mean_exec_time, 2) AS mean_ms, query
FROM pg_stat_statements ORDER BY total_exec_time DESC LIMIT 10;
```

The extension must be in `shared_preload_libraries`. Statements that differ only in constants are counted together.

### Settings in a plan

```sql
SET random_page_cost = 1.1;
EXPLAIN (COSTS OFF, SETTINGS) SELECT * FROM orders WHERE customer_id = 7;
-- Index Scan using orders_customer on orders
--   Index Cond: (customer_id = 7)
-- Settings: max_parallel_workers_per_gather = '0', random_page_cost = '1.1'
```

`SETTINGS` lists the planner settings that differ from defaults: useful when a plan differs between servers.

## Reference

| `EXPLAIN` option | What it adds |
|---|---|
| `ANALYZE` | execute and show actual rows, loops, times |
| `BUFFERS` | pages hit, read, dirtied, written |
| `TIMING` | per-node time (with `ANALYZE`) |
| `SUMMARY` | planning and execution time |
| `VERBOSE` | output columns, schema-qualified names |
| `SETTINGS` | changed planner settings |
| `WAL` | WAL records and bytes (with `ANALYZE`) |
| `SERIALIZE` | cost of converting output (with `ANALYZE`) |
| `MEMORY` | planner memory |
| `GENERIC_PLAN` | plan with `$n` parameters, without `ANALYZE` |
| `FORMAT TEXT \| JSON \| XML \| YAML` | output format |

| Plan line | Meaning |
|---|---|
| `Rows Removed by Filter` | read and discarded |
| `Heap Fetches` | table visits of an index-only scan |
| `Sort Method: external merge  Disk:` | sort spilled to disk |
| `Batches: N` (> 1) | hash split because of memory |
| `Heap Blocks: lossy` | bitmap did not fit `work_mem` |
| `Workers Launched` | parallel workers that actually ran |

| Setting | Default | Effect |
|---|---|---|
| `work_mem` | 4MB | memory per sort or hash |
| `hash_mem_multiplier` | 2.0 | extra memory for hashes |
| `shared_buffers` | 128MB (typically) | PostgreSQL's page cache |
| `effective_cache_size` | 4GB | assumed total cache, only an estimate |
| `random_page_cost` / `seq_page_cost` | 4.0 / 1.0 | relative cost of page reads |
| `default_statistics_target` | 100 | statistics detail |
| `plan_cache_mode` | `auto` | custom or generic plans |
| `autovacuum_vacuum_threshold` / `_scale_factor` | 50 / 0.2 | when to vacuum |
| `autovacuum_analyze_threshold` / `_scale_factor` | 50 / 0.1 | when to analyze |
| `track_io_timing` | off | I/O times in plans and statistics |

On SSDs `random_page_cost` is often lowered toward 1.1; check the effect on real queries.

| Diagnostic view | What it shows |
|---|---|
| `pg_stat_statements` | totals per normalized query |
| `pg_stat_user_tables` | sequential and index scans, dead rows, last (auto)vacuum and analyze |
| `pg_stat_user_indexes` | index usage |
| `pg_stat_activity` | what runs now and what it waits for |
| `pg_stats` | column statistics |

## Gotchas

### A rare filter with LIMIT

```sql
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF, COSTS OFF)
SELECT * FROM orders WHERE status = 'new' ORDER BY id LIMIT 10;
-- Limit (actual rows=10 loops=1)
--   ->  Index Scan using orders_pkey on orders (actual rows=10 loops=1)
--         Filter: (status = 'new'::text)
--         Rows Removed by Filter: 490
```

Here 2% of rows match, so 500 rows were read for 10. If matches are rare or all at the end of the index, the same plan reads almost the whole table. An index on the filtered rows helps.

### A cast on the column disables the index

```sql
CREATE TABLE codes (code text PRIMARY KEY);
INSERT INTO codes SELECT i::text FROM generate_series(1, 100000) i;
ANALYZE codes;

EXPLAIN (COSTS OFF) SELECT * FROM codes WHERE code = '42';      -- Index Only Scan using codes_pkey
EXPLAIN (COSTS OFF) SELECT * FROM codes WHERE code::int = 42;   -- Seq Scan, Filter: ((code)::integer = 42)
```

Compare with a value of the column's type, not the column converted to the parameter's type. Mismatched types from an application driver produce the same effect.

### A hash join split into batches

`Batches: 128` in the memory example means the hash table was written to disk in 128 parts.

## Trade-offs and alternatives

| Remedy | Pro | Con |
|---|---|---|
| An index | large speedup for a query | slower writes, space |
| Rewriting the query | no new objects | needs understanding of the plan |
| `ANALYZE`, extended statistics | better estimates everywhere | another object to maintain |
| More `work_mem` | no disk spill | risk of running out of memory |
| A materialized view or summary table | fast reports | staleness |
| Partitioning | pruning and cheap removal of old data | more complex schema |
| Hardware | helps everything a little | does not fix a bad plan |

## Interview questions

**1. What are the two numbers in `cost=`?**

<details><summary>Answer</summary>

Startup cost before the first row and total cost to finish the node, in planner units.

</details>

**2. How does `EXPLAIN` differ from `EXPLAIN ANALYZE`?**

<details><summary>Answer</summary>

The first only plans; the second executes and adds actual rows, loops and times.

</details>

**3. Estimated 200 rows, actual 2,000,000. What do you do?**

<details><summary>Answer</summary>

Run `ANALYZE`; check correlated columns (extended statistics), the statistics target, functions over columns.

</details>

**4. What does `Sort Method: external merge` mean?**

<details><summary>Answer</summary>

The sort did not fit into `work_mem` and used temporary files.

</details>

**5. Is `work_mem` per query?**

<details><summary>Answer</summary>

No, per sort or hash operation; a query may use it several times.

</details>

**6. How do you find the queries worth optimizing?**

<details><summary>Answer</summary>

`pg_stat_statements` ordered by total execution time.

</details>

**7. `VACUUM` versus `VACUUM FULL`?**

<details><summary>Answer</summary>

`VACUUM` frees space for reuse while the table stays available; `VACUUM FULL` rewrites the table under an exclusive lock and returns space to the OS.

</details>

**8. When does autovacuum start on a table?**

<details><summary>Answer</summary>

When dead rows exceed threshold + scale factor × rows, 50 + 0.2 × rows by default.

</details>

**9. What is a generic plan?**

<details><summary>Answer</summary>

A plan of a prepared statement built without parameter values. After five custom plans the server may switch to it if it is not more expensive on average.

</details>

**10. Trick question: `actual rows=10 loops=1000`. How many rows?**

<details><summary>Answer</summary>

10,000: the figures are per loop.

</details>

**11. Trick question: `ORDER BY id LIMIT 10` with a filter is slow, though there is an index on `id`. Why?**

<details><summary>Answer</summary>

The index walks in `id` order and discards rows until ten match; with rare matches it reads a lot.

</details>

**12. What changed in `EXPLAIN` in PostgreSQL 18?**

<details><summary>Answer</summary>

`EXPLAIN ANALYZE` includes `BUFFERS` automatically.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [PostgreSQL 17: using EXPLAIN](https://www.postgresql.org/docs/17/using-explain.html)
- [PostgreSQL 17: EXPLAIN](https://www.postgresql.org/docs/17/sql-explain.html)
- [PostgreSQL 17: planner statistics](https://www.postgresql.org/docs/17/planner-stats.html)
- [PostgreSQL 17: query planning settings](https://www.postgresql.org/docs/17/runtime-config-query.html)
- [PostgreSQL 17: PREPARE](https://www.postgresql.org/docs/17/sql-prepare.html)
- [PostgreSQL 17: pg_stat_statements](https://www.postgresql.org/docs/17/pgstatstatements.html)
- [PostgreSQL 18: release notes](https://www.postgresql.org/docs/18/release-18.html)
