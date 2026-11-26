---
title: "Joins and subqueries: basics"
lang: en
track: postgresql
kind: module
doc: basics
slug: 04-joins-and-subqueries
part: 1
tags: [join, subquery, cte, lateral, set-operations]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Joins and subqueries: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** `SELECT` and `WHERE` · **Time:** ~2 h

## Data for the examples

All results in this module were produced on PostgreSQL 17.11 with this data. Order 13 deliberately has no customer, and Eve has no orders.

```sql
CREATE TABLE customers (id int PRIMARY KEY, name text NOT NULL);
CREATE TABLE orders (id int PRIMARY KEY, customer_id int, amount numeric(10,2) NOT NULL);
INSERT INTO customers VALUES (1,'Ann'),(2,'Bob'),(3,'Eve');
INSERT INTO orders VALUES (10,1,50),(11,1,70),(12,2,20),(13,NULL,5);
```

## Join types

| Join | What stays in the result |
|---|---|
| `JOIN` (`INNER JOIN`) | only pairs that satisfy the condition |
| `LEFT JOIN` | the pairs plus every left row without a pair, filled with `NULL` |
| `RIGHT JOIN` | the same from the right side |
| `FULL JOIN` | the pairs plus unmatched rows of both sides |
| `CROSS JOIN` | every left row with every right row, no condition |

```sql
SELECT c.name, o.amount FROM customers c JOIN orders o ON o.customer_id = c.id ORDER BY o.id;
-- Ann 50.00 | Ann 70.00 | Bob 20.00

SELECT c.name, o.amount FROM customers c LEFT JOIN orders o ON o.customer_id = c.id ORDER BY c.id, o.id;
-- Ann 50.00 | Ann 70.00 | Bob 20.00 | Eve NULL
```

A `FULL JOIN` shows both kinds of loners at once:

```sql
SELECT c.name, o.id AS order_id
FROM customers c FULL JOIN orders o ON o.customer_id = c.id
ORDER BY c.id, o.id;
-- Ann 10 | Ann 11 | Bob 12 | Eve NULL | NULL 13
```

`CROSS JOIN` of 3 customers and 4 orders gives 12 rows.

## ON, USING and table aliases

`ON` takes any boolean condition. `USING (col)` is a shorthand for equality of same-named columns and outputs that column once. Short aliases (`customers c`) keep the condition readable and are required when a table is joined with itself:

```sql
CREATE TABLE employees (id int PRIMARY KEY, name text, manager_id int);
INSERT INTO employees VALUES (1,'Root',NULL),(2,'Mid',1),(3,'Leaf',2);

SELECT e.name, m.name AS manager
FROM employees e LEFT JOIN employees m ON m.id = e.manager_id
ORDER BY e.id;
-- Root NULL | Mid Root | Leaf Mid
```

## Does a related row exist

```sql
-- customers that have orders: Ann, Bob (each once)
SELECT name FROM customers c
WHERE EXISTS (SELECT 1 FROM orders o WHERE o.customer_id = c.id);

-- customers without orders: Eve
SELECT name FROM customers c
WHERE NOT EXISTS (SELECT 1 FROM orders o WHERE o.customer_id = c.id);
```

A join would return Ann twice, once per order; `EXISTS` returns her once.

## WITH: naming a step

```sql
WITH totals AS (
    SELECT customer_id, sum(amount) AS total FROM orders GROUP BY customer_id
)
SELECT c.name, t.total
FROM customers c JOIN totals t ON t.customer_id = c.id
WHERE t.total > 30;
-- Ann 120.00
```

## Beginner mistakes

- ❌ Forgetting the join condition → ✅ write `ON`; an accidental cross join multiplies the sizes.
- ❌ `INNER JOIN` in a "all customers" report → ✅ `LEFT JOIN`, otherwise customers without orders vanish.
- ❌ `= NULL` to find unmatched rows → ✅ `IS NULL` or `NOT EXISTS`.
- ❌ Relying on the row order of a join → ✅ add `ORDER BY`.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [PostgreSQL 17: table expressions and joins](https://www.postgresql.org/docs/17/queries-table-expressions.html)
- [PostgreSQL 17: WITH queries](https://www.postgresql.org/docs/17/queries-with.html)
- [PostgreSQL 17: subquery expressions](https://www.postgresql.org/docs/17/functions-subquery.html)
