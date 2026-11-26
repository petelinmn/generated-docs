---
title: "Joins and subqueries: advanced"
lang: en
track: postgresql
kind: module
doc: advanced
slug: 04-joins-and-subqueries
part: 1
tags: [join, subquery, cte, lateral, set-operations]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Joins and subqueries: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## ON versus WHERE in outer joins

```sql
-- condition in ON: filters which orders match, every customer stays
SELECT c.name, o.amount FROM customers c
LEFT JOIN orders o ON o.customer_id = c.id AND o.amount > 60;
-- Ann 70.00 | Bob NULL | Eve NULL

-- condition in WHERE: filters the joined result, NULL rows disappear
SELECT c.name, o.amount FROM customers c
LEFT JOIN orders o ON o.customer_id = c.id
WHERE o.amount > 60;
-- Ann 70.00
```

`ON` decides what counts as a pair; `WHERE` runs after the join. A `WHERE` condition on the right table silently turns a `LEFT JOIN` into an inner one.

## NOT IN and NULL

```sql
SELECT name FROM customers WHERE id NOT IN (SELECT customer_id FROM orders);
-- (0 rows): order 13 has customer_id NULL
```

`x NOT IN (1, 2, NULL)` cannot be true: whether `x` differs from the unknown value is unknown. Use `NOT EXISTS`, or filter `NULL` out of the subquery.

## Row multiplication

```sql
-- join order rows with two tags of Ann: every order is counted twice
SELECT sum(o.amount), count(*)
FROM customers c
JOIN orders o ON o.customer_id = c.id
JOIN (VALUES (1,'a'),(1,'b')) t(customer_id, tag) ON t.customer_id = c.id;
-- 240.00 | 4   (Ann's real total is 120.00)

-- aggregate each side first, then join
SELECT c.name, o.total, t.tags
FROM customers c
JOIN (SELECT customer_id, sum(amount) AS total FROM orders GROUP BY customer_id) o ON o.customer_id = c.id
JOIN (SELECT customer_id, count(*) AS tags
      FROM (VALUES (1,'a'),(1,'b')) v(customer_id, tag) GROUP BY customer_id) t ON t.customer_id = c.id;
-- Ann | 120.00 | 2
```

Each independent "many" side multiplies the others. Aggregate before joining, or use separate subqueries.

## LATERAL

```sql
-- the largest order of every customer
SELECT c.name, top.id, top.amount
FROM customers c
LEFT JOIN LATERAL (
    SELECT id, amount FROM orders o
    WHERE o.customer_id = c.id
    ORDER BY amount DESC LIMIT 1
) AS top ON true
ORDER BY c.id;
-- Ann 11 70.00 | Bob 12 20.00 | Eve NULL NULL
```

Without `LATERAL` a subquery in `FROM` cannot see `c.id`. `LEFT JOIN LATERAL ... ON true` keeps customers for which the subquery returned nothing. This is the usual way to get "top N per group".

## Recursive CTE

```sql
-- employees from basics: Root <- Mid <- Leaf
WITH RECURSIVE chain AS (
    SELECT id, name, 1 AS depth FROM employees WHERE manager_id IS NULL
    UNION ALL
    SELECT e.id, e.name, c.depth + 1
    FROM employees e JOIN chain c ON e.manager_id = c.id
)
SELECT name, depth FROM chain ORDER BY depth;
-- Root 1 | Mid 2 | Leaf 3
```

The first part runs once; the second part runs again and again over the rows produced by the previous step, until it returns nothing. On cyclic data it would never stop, so PostgreSQL offers a `CYCLE` clause:

```sql
WITH RECURSIVE g(id, next) AS (VALUES (1,2),(2,1)),
walk AS (
    SELECT id, next FROM g WHERE id = 1
    UNION ALL
    SELECT g.id, g.next FROM g JOIN walk w ON g.id = w.next
) CYCLE id SET is_cycle USING path
SELECT id, is_cycle, path FROM walk;
-- 1 f {(1)} | 2 f {(1),(2)} | 1 t {(1),(2),(1)}
```

`SEARCH DEPTH FIRST BY` and `SEARCH BREADTH FIRST BY` add an ordering column for the traversal.

## Set operations

| Operation | Result | Duplicates |
|---|---|---|
| `UNION` | rows of either query | removed |
| `UNION ALL` | rows of both queries | kept, and no deduplication work |
| `INTERSECT` | rows present in both | removed |
| `EXCEPT` | rows of the first that are not in the second | removed |

`INTERSECT ALL` and `EXCEPT ALL` keep duplicates by count. The queries must return the same number of columns with compatible types.

## Best practices

- Qualify every column with a table alias.
- Prefer `NOT EXISTS` to `NOT IN` for subqueries.
- Use `UNION ALL` unless duplicates really must be removed.
- Check the row count after each added join.

## Common mistakes

- ❌ `SELECT DISTINCT` to hide duplicates from a join → ✅ find which join multiplies rows.
- ❌ `count(*)` after `LEFT JOIN` → ✅ `count(o.id)`: `count(*)` counts the `NULL` row too.
- ❌ A correlated subquery per column of the same table → ✅ one join or one `LATERAL`.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [PostgreSQL 17: table expressions and joins](https://www.postgresql.org/docs/17/queries-table-expressions.html)
- [PostgreSQL 17: WITH queries](https://www.postgresql.org/docs/17/queries-with.html)
- [PostgreSQL 17: subquery expressions](https://www.postgresql.org/docs/17/functions-subquery.html)
