---
title: "Indexes: expert"
lang: en
track: postgresql
kind: module
doc: expert
slug: 05-indexes
part: 2
tags: [index, b-tree, gin, brin, query-plan]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Indexes: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

A table (heap) is a set of 8 kB pages with row versions in no particular order. An index entry maps a key to the physical position of a row version. MVCC keeps old versions, and index entries do not know whether a version is visible to you.

- **Visibility map.** One bit per heap page says that all its rows are visible to everyone; index-only scans rely on it, `VACUUM` sets it.
- **HOT updates.** If an `UPDATE` changes no column referenced by any index (summarizing indexes, in core only BRIN, do not count) and the new version fits on the same page, no new index entries are needed. A lower table `fillfactor` leaves room for this.
- **B-tree maintenance.** The default index `fillfactor` is 90. Deduplication (`deduplicate_items`, on by default) stores repeated keys compactly; it is not used for indexes with `INCLUDE` columns.
- **Version note.** PostgreSQL 18 added B-tree skip scan, which relaxes the leading-column rule. The statements about the rule here are checked on 17.

## Reference

| Type | Typical operators and uses | Multicolumn | Unique | `INCLUDE` |
|---|---|---|---|---|
| B-tree | `< <= = >= >`, `BETWEEN`, `IN`, `IS NULL`, ordering | yes | yes | yes |
| Hash | `=` | no | no | no |
| GiST | geometry, ranges, nearest neighbour; depends on the operator class | yes | no | yes |
| SP-GiST | partitioned structures: points, prefixes | no | no | yes |
| GIN | many values per row: `jsonb`, arrays, full text | yes | no | no |
| BRIN | min/max per block range | yes | no | no |

An index may have up to 32 columns, including `INCLUDE` columns.

| `CREATE INDEX` option | Meaning |
|---|---|
| `UNIQUE` | forbid duplicates (B-tree) |
| `NULLS NOT DISTINCT` | treat `NULL`s as equal for uniqueness |
| `CONCURRENTLY` | build without blocking writes |
| `USING method` | access method, `btree` by default |
| `(expression)` | index an expression |
| `opclass` | for example `text_pattern_ops`, `jsonb_path_ops` |
| `ASC` / `DESC`, `NULLS FIRST` / `LAST` | order inside a B-tree |
| `INCLUDE (cols)` | payload columns |
| `WHERE predicate` | partial index |
| `WITH (fillfactor = n)` | free space left in index pages |

| Maintenance | Command or view |
|---|---|
| Usage statistics | `pg_stat_user_indexes` |
| Size | `pg_relation_size('index_name')` |
| Rebuild | `REINDEX INDEX [CONCURRENTLY]` |
| Remove without blocking | `DROP INDEX CONCURRENTLY` |
| Validity | `pg_index.indisvalid` |

## Gotchas

### "Index Scan" can mean reading the whole index

```sql
CREATE INDEX IF NOT EXISTS events_user_created ON events (user_id, created_at);   -- from basics
DROP INDEX IF EXISTS events_btree_time;   -- if you created it in the advanced part
-- only the second column of (user_id, created_at)
EXPLAIN (ANALYZE, BUFFERS, COSTS OFF, TIMING OFF, SUMMARY OFF)
SELECT * FROM events WHERE created_at = timestamptz '2026-01-02 00:00+00';
-- Index Scan using events_user_created ...   Buffers: shared hit=1 read=386

CREATE INDEX events_time ON events (created_at);
-- Index Scan using events_time ...           Buffers: shared hit=1 read=2
```

Both plans say `Index Scan`; one touched 387 buffers, the other 3. How they split into `hit` and `read` depends on the cache.

### Unique does not mean "one NULL"

```sql
CREATE TABLE u (a int UNIQUE);
INSERT INTO u VALUES (NULL), (NULL);          -- both accepted

CREATE TABLE u2 (a int UNIQUE NULLS NOT DISTINCT);
INSERT INTO u2 VALUES (NULL);
INSERT INTO u2 VALUES (NULL);
-- ERROR:  duplicate key value violates unique constraint "u2_a_key"
```

### A foreign key gives no index

```sql
CREATE TABLE parent (id int PRIMARY KEY);
CREATE TABLE child (id int PRIMARY KEY, parent_id int REFERENCES parent(id));
SELECT indexname FROM pg_indexes WHERE tablename = 'child';
-- child_pkey   (nothing on parent_id)
```

Deleting a parent row then has to scan the child table.

### NULLs sort as the largest values

```sql
CREATE TABLE s (x int);
INSERT INTO s VALUES (2), (NULL), (1);

SELECT x FROM s ORDER BY x;        -- 1, 2, NULL
SELECT x FROM s ORDER BY x DESC;   -- NULL, 2, 1
```

`ORDER BY created_at DESC LIMIT 10` returns rows with `NULL` first unless you say `NULLS LAST`, and the index should be declared to match.

## Trade-offs and alternatives

| Choice | Pro | Con |
|---|---|---|
| More indexes | faster reads | slower writes, space, fewer HOT updates |
| Composite instead of several | serves filter and order together | useless without the leading column (in 17) |
| Partial | small, cheap | serves only matching queries |
| Covering | no table access | larger index, depends on `VACUUM` |
| BRIN instead of B-tree | tiny | only for physically ordered data, less precise |
| Partitioning | drop old data cheaply, smaller indexes per part | more complex schema |

## Interview questions

**1. Which index types are built in?**

<details><summary>Answer</summary>

B-tree (default), Hash, GiST, SP-GiST, GIN and BRIN. `bloom` is an extension.

</details>

**2. Why is an index not used?**

<details><summary>Answer</summary>

The query returns too much of the table, the condition does not match the index (function, other operator class, no leading column), the table is small or statistics are stale.

</details>

**3. Which index can enforce uniqueness?**

<details><summary>Answer</summary>

Only B-tree.

</details>

**4. Does a foreign key create an index?**

<details><summary>Answer</summary>

Not on the referencing columns. The referenced side already has a unique index.

</details>

**5. What does `Heap Fetches` mean?**

<details><summary>Answer</summary>

How many times an index-only scan had to visit the table to check visibility.

</details>

**6. What is a HOT update?**

<details><summary>Answer</summary>

An update that creates no new index entries: no indexed column changed (summarizing indexes such as BRIN do not count) and the page has room.

</details>

**7. When is BRIN a good choice?**

<details><summary>Answer</summary>

A very large table whose column values follow the physical order.

</details>

**8. What can go wrong with `CREATE INDEX CONCURRENTLY`?**

<details><summary>Answer</summary>

It may fail and leave an invalid index that must be dropped or rebuilt.

</details>

**9. How does `INCLUDE` differ from adding a key column?**

<details><summary>Answer</summary>

Included columns are not part of the search key or of uniqueness; they only allow index-only scans.

</details>

**10. Trick question: `EXPLAIN` shows `Index Scan`. Is the query efficient?**

<details><summary>Answer</summary>

Not necessarily: it may walk the whole index. Look at `Index Cond`, buffers and actual rows.

</details>

**11. Trick question: two `NULL`s in a unique column — error?**

<details><summary>Answer</summary>

Not by default. With `NULLS NOT DISTINCT` it is.

</details>

**12. Did anything change about the leading column rule in PostgreSQL 18?**

<details><summary>Answer</summary>

PostgreSQL 18 added B-tree skip scan, so a multicolumn index can help in more cases without a condition on the leading column. This module is checked on 17.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [PostgreSQL 17: index types](https://www.postgresql.org/docs/17/indexes-types.html)
- [PostgreSQL 17: multicolumn indexes](https://www.postgresql.org/docs/17/indexes-multicolumn.html)
- [PostgreSQL 17: index-only scans](https://www.postgresql.org/docs/17/indexes-index-only-scans.html)
- [PostgreSQL 17: CREATE INDEX](https://www.postgresql.org/docs/17/sql-createindex.html)
- [PostgreSQL 17: heap-only tuples](https://www.postgresql.org/docs/17/storage-hot.html)
- [PostgreSQL 18: release notes](https://www.postgresql.org/docs/18/release-18.html)
