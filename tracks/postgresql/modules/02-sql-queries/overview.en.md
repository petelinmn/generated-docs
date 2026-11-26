---
title: "SQL queries"
lang: en
track: postgresql
kind: module
doc: overview
slug: 02-sql-queries
part: 1
levels: [1, 2, 3, 4]
tags: [sql, select, where, group-by, having, order-by, limit, aggregates]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# SQL queries

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

`SELECT` reads rows and evaluates expressions; `FROM` chooses sources, `WHERE` filters before aggregation, `GROUP BY` combines rows, `HAVING` filters groups, and `ORDER BY` sets result order. PostgreSQL processes a query through a logical plan and returns a table of rows.

## Key ideas

- SELECT evaluates expressions, functions, and aggregates.
- `WHERE` cannot use aggregate results.
- `HAVING` filters grouped groups.
- `ORDER BY` is required for a specified result order.
- `LIMIT` and `OFFSET` bound output; keyset pagination is often better for large sets.
- Table row order is not guaranteed without `ORDER BY`.

## Minimal example

```sql
CREATE TABLE orders (id integer, customer text, amount numeric);
INSERT INTO orders VALUES (1, 'Ada', 25), (2, 'Ada', 40), (3, 'Linus', 10);

SELECT customer, sum(amount) AS total
FROM orders
GROUP BY customer
HAVING sum(amount) >= 30
ORDER BY total DESC;
```

```text
 customer | total
----------+-------
 Ada      |    65
```

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | SELECT, WHERE, GROUP BY, HAVING, ORDER BY, LIMIT |
| [2. Advanced](advanced.en.md) | DISTINCT, NULL, CASE, set operations, window functions |
| [3. Expert](expert.en.md) | logical order, planner, expression types, traps |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not generated yet — `/learn postgresql 02-sql-queries level 5` |
| 6. Tests | not generated yet — `/learn postgresql 02-sql-queries level 6` |

## Related materials

- Prerequisites: [01 · PostgreSQL basics](../01-postgresql-basics/overview.en.md).
- Next: [03 · Data modeling and DDL](../03-data-modeling-ddl/overview.en.md).
- [System design: choosing a store](../../../system-design/modules/04-data-storage/overview.en.md)
- [04 · Joins and subqueries](../04-joins-and-subqueries/overview.en.md)
- [05 · Indexes](../05-indexes/overview.en.md)

## Sources

- [PostgreSQL SELECT](https://www.postgresql.org/docs/current/sql-select.html)
- [PostgreSQL queries](https://www.postgresql.org/docs/current/queries.html)
- [PostgreSQL aggregate functions](https://www.postgresql.org/docs/current/functions-aggregate.html)
