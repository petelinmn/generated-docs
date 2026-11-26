---
title: "Indexes: advanced"
lang: en
track: postgresql
kind: module
doc: advanced
slug: 05-indexes
part: 2
tags: [index, b-tree, gin, brin, query-plan]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Indexes: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Partial indexes

```sql
CREATE INDEX events_failed ON events (created_at) WHERE status = 'failed';

EXPLAIN (COSTS OFF) SELECT id FROM events WHERE status = 'failed' ORDER BY created_at LIMIT 5;
-- Limit
--   ->  Index Scan using events_failed on events

-- the same query for status = 'done' cannot use it: Sort over Seq Scan
```

The index holds only rows satisfying its `WHERE`, so it is small and cheap to maintain. The planner uses it only when the query condition implies the index condition.

## Expression indexes

```sql
CREATE INDEX events_email ON events (email);
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE lower(email) = 'user7@example.com';
-- Seq Scan on events

CREATE INDEX events_lower_email ON events (lower(email));
ANALYZE events;
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE lower(email) = 'user7@example.com';
-- Index Scan using events_lower_email on events
```

An index stores values, not the results of functions over them. `ANALYZE` after creating it collects statistics for the expression.

## LIKE and operator classes

```sql
-- database collation is en_US.utf8
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE email LIKE 'User777%';        -- Seq Scan
CREATE INDEX events_email_pattern ON events (email text_pattern_ops);
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE email LIKE 'User777%';
-- Index Scan using events_email_pattern on events
--   Index Cond: ((email ~>=~ 'User777') AND (email ~<~ 'User778'))
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE email LIKE '%777@example.com'; -- Seq Scan
```

In a collation other than `C`, an ordinary B-tree does not serve `LIKE 'abc%'`; the `text_pattern_ops` operator class compares character by character and does. A leading `%` cannot use either; for substring search there is the `pg_trgm` extension.

## Covering indexes and index-only scans

```sql
CREATE INDEX events_user_inc ON events (user_id) INCLUDE (status);

EXPLAIN (ANALYZE, COSTS OFF, TIMING OFF, SUMMARY OFF) SELECT user_id, status FROM events WHERE user_id = 42;
-- Index Only Scan using events_user_inc ... (actual rows=100)
--   Heap Fetches: 0

UPDATE events SET status = status WHERE user_id = 42;
-- the same EXPLAIN: Heap Fetches: 200

VACUUM events;
-- the same EXPLAIN: Heap Fetches: 0
```

`INCLUDE` columns are payload: they are stored in the index but do not order it. Index entries carry no visibility information, so PostgreSQL consults the visibility map; for pages changed recently it has to visit the table (`Heap Fetches`) until `VACUUM` marks them all-visible.

## GIN for jsonb

```sql
CREATE INDEX events_payload ON events USING gin (payload);
ANALYZE events;

EXPLAIN (COSTS OFF) SELECT * FROM events WHERE payload @> '{"kind":"k7"}';
-- Bitmap Heap Scan on events
--   ->  Bitmap Index Scan on events_payload

EXPLAIN (COSTS OFF) SELECT * FROM events WHERE payload ->> 'kind' = 'k7';
-- Seq Scan on events
```

The default `jsonb_ops` GIN index supports containment `@>` and key-existence operators, not an arbitrary expression such as `->>` equality. For that, create a B-tree on the expression `(payload ->> 'kind')`.

## BRIN

```sql
CREATE INDEX events_brin ON events USING brin (created_at);
CREATE INDEX events_btree_time ON events (created_at);
```

| Object | Size |
|---|---|
| table `events` | 9888 kB |
| BRIN on `created_at` | 24 kB |
| B-tree on `created_at` | 2208 kB |
| GIN on `payload` | 536 kB |

BRIN keeps a summary (minimum and maximum) per range of table pages. It works when the column correlates with the physical order, such as an insertion time in an append-only table. It only narrows down pages, so rows are rechecked.

## Building without blocking writes

A plain `CREATE INDEX` blocks writes (not reads) until it finishes. `CONCURRENTLY` allows writes, takes longer, and cannot run in a transaction:

```sql
BEGIN;
CREATE INDEX CONCURRENTLY x1 ON events (id);
-- ERROR:  CREATE INDEX CONCURRENTLY cannot run inside a transaction block
ROLLBACK;

CREATE TABLE dup (a int);
INSERT INTO dup VALUES (1), (1);
CREATE UNIQUE INDEX CONCURRENTLY dup_a ON dup (a);
-- ERROR:  could not create unique index "dup_a"

SELECT c.relname, i.indisvalid FROM pg_index i JOIN pg_class c ON c.oid = i.indexrelid
WHERE i.indrelid = 'dup'::regclass;
-- dup_a | f
```

An invalid index is not used by queries but still costs writes. Drop it and retry, or use `REINDEX INDEX CONCURRENTLY`.

## Best practices

- Start from the slow query, then design the index.
- Equality columns first, the range or sort column after them.
- Find unused indexes through `pg_stat_user_indexes` over a representative period.
- Create indexes on live tables with `CONCURRENTLY` and check validity afterwards.

## Common mistakes

- ❌ A function on the column while indexing the bare column → ✅ expression index.
- ❌ Duplicate indexes `(a)` and `(a, b)` without a reason → ✅ usually the composite is enough.
- ❌ BRIN on randomly ordered values → ✅ every range then covers everything.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [PostgreSQL 17: index types](https://www.postgresql.org/docs/17/indexes-types.html)
- [PostgreSQL 17: multicolumn indexes](https://www.postgresql.org/docs/17/indexes-multicolumn.html)
- [PostgreSQL 17: index-only scans](https://www.postgresql.org/docs/17/indexes-index-only-scans.html)
- [PostgreSQL 17: CREATE INDEX](https://www.postgresql.org/docs/17/sql-createindex.html)
