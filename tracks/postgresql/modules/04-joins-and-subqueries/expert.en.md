---
title: "Joins and subqueries: expert"
lang: en
track: postgresql
kind: module
doc: expert
slug: 04-joins-and-subqueries
part: 1
tags: [join, subquery, cte, lateral, set-operations]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Joins and subqueries: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

SQL says which rows you want; the planner picks the algorithm and the join order. PostgreSQL has three join strategies.

| Algorithm | How it works | Fits when | Condition |
|---|---|---|---|
| Nested loop | for each outer row, look for matches in the inner side | the outer side is small, the inner side has an index | any |
| Hash join | build a hash table from one side, probe it with the other | large unsorted inputs | equality only |
| Merge join | walk two sorted inputs together | inputs are already sorted by the key or large | mergeable (equality-like) operators |

`EXISTS` and `NOT EXISTS` usually become semi-joins and anti-joins, which stop at the first match. `NOT IN` cannot become an anti-join because of its `NULL` rules, and the plans differ:

```sql
EXPLAIN (COSTS OFF)
SELECT * FROM customers c WHERE NOT EXISTS (SELECT 1 FROM orders o WHERE o.customer_id = c.id);
```

```text
Hash Right Anti Join
  Hash Cond: (o.customer_id = c.id)
  ->  Seq Scan on orders o
  ->  Hash
        ->  Seq Scan on customers c
```

```sql
EXPLAIN (COSTS OFF)
SELECT * FROM customers c WHERE id NOT IN (SELECT customer_id FROM orders);
```

```text
Seq Scan on customers c
  Filter: (NOT (ANY (id = (hashed SubPlan 1).col1)))
  SubPlan 1
    ->  Seq Scan on orders
```

The exact plan depends on statistics and sizes; read it with `EXPLAIN`, do not guess.

### Join order

The number of possible orders grows very fast with the number of tables. `from_collapse_limit` and `join_collapse_limit` (both 8 by default) bound how many items the planner reorders together; from `geqo_threshold` items (12 by default) the genetic optimizer is used instead of exhaustive search. Setting `join_collapse_limit = 1` makes the planner keep explicit `JOIN`s in the written order.

### CTE: folded or materialized

A non-recursive, side-effect-free `WITH` query referenced once is folded into the parent query by default, so conditions can be pushed into it. A query referenced more than once is computed separately. `AS MATERIALIZED` forces separate computation, `AS NOT MATERIALIZED` forces merging.

### Data-modifying WITH

`INSERT`, `UPDATE`, `DELETE` and `MERGE` may appear inside `WITH`. They run exactly once and to completion, even if the outer query does not read their output. All parts of the statement see the same snapshot, so they see each other's changes only through `RETURNING`:

```sql
WITH gone AS (DELETE FROM orders WHERE customer_id IS NULL RETURNING *)
SELECT (SELECT count(*) FROM gone)   AS deleted,
       (SELECT count(*) FROM orders) AS visible_in_same_statement;
-- 1 | 4   (the next statement already sees 3 rows)
```

## Reference

| Construct | Meaning |
|---|---|
| `a JOIN b ON cond` | pairs satisfying `cond` |
| `a LEFT/RIGHT/FULL [OUTER] JOIN b ON cond` | keeps unmatched rows of the named side |
| `a CROSS JOIN b` | Cartesian product, same as `a, b` |
| `USING (c1, c2)` | equality on same-named columns, each output once |
| `NATURAL JOIN` | `USING` over all same-named columns; with none it behaves as `CROSS JOIN` |
| `LATERAL` | the item may refer to earlier items of `FROM` |

| Subquery expression | Result | On an empty set |
|---|---|---|
| `EXISTS (q)` | true if `q` returns a row | false |
| `x IN (q)` / `x = ANY (q)` | true if equal to some value | false |
| `x NOT IN (q)` / `x <> ALL (q)` | true if different from every value | true |
| `x op ANY (q)` (`SOME`) | true if true for at least one | false |
| `x op ALL (q)` | true if true for all | true |
| `(row) = (q)` | row comparison with a single-row query | NULL |
| scalar `(q)` | the single value; more than one row is an error | NULL |

| CTE feature | Syntax |
|---|---|
| Recursion | `WITH RECURSIVE t AS (base UNION [ALL] step)` |
| Planner control | `AS [NOT] MATERIALIZED (...)` |
| Traversal order | `SEARCH {DEPTH \| BREADTH} FIRST BY col SET ordercol` |
| Cycle protection | `CYCLE col SET flag USING path` |

`UNION` (without `ALL`) in a recursive query discards rows already seen, which also stops some loops.

## Gotchas

### A scalar subquery with several rows fails only at run time

```sql
SELECT (SELECT amount FROM orders WHERE id = 999);          -- NULL
SELECT (SELECT amount FROM orders WHERE customer_id = 1);
-- ERROR:  more than one row returned by a subquery used as an expression
```

The query works while every customer has one order and breaks when a second one appears.

### ALL over an empty set is true

```sql
SELECT 5 > ALL (SELECT amount FROM orders WHERE false),   -- t
       5 > ANY (SELECT amount FROM orders WHERE false);   -- f
```

"The price is higher than all competitors" is true when there are no competitors.

### NULL never joins to NULL

```sql
SELECT count(*) FROM (VALUES (NULL::int)) a(x) JOIN (VALUES (NULL::int)) b(x) USING (x);             -- 0
SELECT count(*) FROM (VALUES (NULL::int)) a(x) JOIN (VALUES (NULL::int)) b(x)
    ON a.x IS NOT DISTINCT FROM b.x;                                                                 -- 1
```

`IS NOT DISTINCT FROM` treats two `NULL`s as equal, but it is not an ordinary equality operator for join algorithms, so check the plan on large tables.

### NATURAL JOIN picks columns by name

```sql
-- both tables have a column "id", so NATURAL compares customers.id with orders.id
SELECT count(*) FROM customers NATURAL JOIN orders;   -- 0
```

Adding a column such as `created_at` to both tables later changes the result of an old query without any error.

## Trade-offs and alternatives

| Choice | Pro | Con |
|---|---|---|
| Join | columns from both tables, planner freedom | duplicates when the match is one-to-many |
| `EXISTS` | no duplicates, stops at the first match | no columns from the inner table |
| `LATERAL` | top-N per group, reuse of an expression | usually driven row by row; needs an index |
| Window function | ranks inside groups in one pass | reads all rows of the groups |
| `AS MATERIALIZED` | an expensive step runs once | conditions are not pushed into it |
| Denormalization | no join at read time | copies must be kept in agreement |

## Interview questions

**1. What is the difference between `INNER JOIN` and `LEFT JOIN`?**

<details><summary>Answer</summary>

`LEFT JOIN` also keeps left rows that found no pair, with `NULL` in the right columns.

</details>

**2. Where do you put a filter on the right table of a `LEFT JOIN`?**

<details><summary>Answer</summary>

In `ON` to restrict which rows match; in `WHERE` only if unmatched rows must be dropped.

</details>

**3. Why can `NOT IN` return nothing?**

<details><summary>Answer</summary>

A `NULL` in the subquery makes the comparison unknown for every non-matching value.

</details>

**4. Join or `EXISTS`?**

<details><summary>Answer</summary>

`EXISTS` when only presence matters: no duplicates and no columns from the inner side are needed.

</details>

**5. Name the join algorithms.**

<details><summary>Answer</summary>

Nested loop, hash join, merge join. Hash join needs an equality condition.

</details>

**6. Is a CTE always an optimization barrier?**

<details><summary>Answer</summary>

No. A side-effect-free non-recursive CTE referenced once is folded by default; `MATERIALIZED` restores separate computation.

</details>

**7. How do you take the latest three orders of every customer?**

<details><summary>Answer</summary>

`LEFT JOIN LATERAL (... ORDER BY ... LIMIT 3) ON true`, or `row_number()` over a partition and a filter on it.

</details>

**8. How does a recursive CTE terminate?**

<details><summary>Answer</summary>

When the recursive part returns no new rows. Cycles need `CYCLE`, a path check or `UNION` deduplication.

</details>

**9. `UNION` versus `UNION ALL`?**

<details><summary>Answer</summary>

`UNION` removes duplicates, which costs extra work.

</details>

**10. Trick question: a `LEFT JOIN` returns fewer rows than expected. How is that possible?**

<details><summary>Answer</summary>

A `WHERE` condition on the right table removed the rows filled with `NULL`.

</details>

**11. Trick question: after adding a second join, `sum` doubled. Why?**

<details><summary>Answer</summary>

Two one-to-many relations multiply each other. Aggregate each before joining.

</details>

**12. Can a `DELETE` in `WITH` be skipped if its result is not used?**

<details><summary>Answer</summary>

No. Data-modifying statements in `WITH` run exactly once and to completion.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [PostgreSQL 17: table expressions and joins](https://www.postgresql.org/docs/17/queries-table-expressions.html)
- [PostgreSQL 17: WITH queries](https://www.postgresql.org/docs/17/queries-with.html)
- [PostgreSQL 17: subquery expressions](https://www.postgresql.org/docs/17/functions-subquery.html)
- [PostgreSQL 17: planner and join strategies](https://www.postgresql.org/docs/17/planner-optimizer.html)
- [PostgreSQL 17: query planning settings](https://www.postgresql.org/docs/17/runtime-config-query.html)
