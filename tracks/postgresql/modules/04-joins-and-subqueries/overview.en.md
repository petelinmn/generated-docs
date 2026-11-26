---
title: "Joins and subqueries"
lang: en
track: postgresql
kind: module
doc: overview
slug: 04-joins-and-subqueries
part: 1
levels: [1, 2, 3, 4]
tags: [join, subquery, cte, lateral, set-operations]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Joins and subqueries

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A relational database stores facts in separate tables and puts them back together in a query. Joins combine rows of two tables by a condition, subqueries use the result of one query inside another, and `WITH` (a common table expression, CTE) gives a part of a query a name. Most reporting and application queries are built from these three tools, and most of their surprises come from `NULL` and from rows that multiply.

## Key ideas

- A join type answers one question: what happens to rows that found no pair.
- In an outer join a condition in `ON` and the same condition in `WHERE` give different results.
- `EXISTS` and `NOT EXISTS` ask "is there at least one row" and never duplicate the outer row.
- `NOT IN` with a `NULL` in the list returns nothing.
- Joining two one-to-many relations at once multiplies rows and breaks sums.
- `LATERAL` lets a subquery in `FROM` refer to the current row of the previous table.
- A recursive CTE walks trees and graphs.

## Minimal example

```sql
CREATE TABLE customers (id int PRIMARY KEY, name text NOT NULL);
CREATE TABLE orders (id int PRIMARY KEY, customer_id int, amount numeric(10,2) NOT NULL);
INSERT INTO customers VALUES (1,'Ann'),(2,'Bob'),(3,'Eve');
INSERT INTO orders VALUES (10,1,50),(11,1,70),(12,2,20),(13,NULL,5);
```

```sql
SELECT c.name, o.amount FROM customers c JOIN orders o ON o.customer_id = c.id ORDER BY o.id;
-- Ann 50.00 | Ann 70.00 | Bob 20.00

SELECT c.name, o.amount FROM customers c LEFT JOIN orders o ON o.customer_id = c.id ORDER BY c.id, o.id;
-- Ann 50.00 | Ann 70.00 | Bob 20.00 | Eve NULL
```

## When to use and when not

- ✅ Combining related tables, reports, "find rows without a pair".
- ✅ Naming steps of a long query with `WITH`.
- ❌ A join only to test existence: it duplicates rows; use `EXISTS`.
- ❌ `NATURAL JOIN` in application code: the condition changes silently with the schema.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | join types, `EXISTS`, `WITH` |
| [2. Advanced](advanced.en.md) | `ON` and `WHERE`, `NULL` traps, row multiplication, `LATERAL`, recursion |
| [3. Expert](expert.en.md) | join algorithms and plans, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn postgresql 04-joins-and-subqueries level 5` |
| 6. Tests | not created yet — `/learn postgresql 04-joins-and-subqueries level 6` |

## Related materials

- Prerequisites: [02 · SQL queries](../02-sql-queries/overview.en.md), [03 · Schema design](../03-data-modeling-ddl/overview.en.md)
- Next: the following module of the [PostgreSQL roadmap](../../overview.en.md)
- The same idea in other tracks: [LINQ in C#](../../../csharp/modules/07-linq/overview.en.md)
- Where it matters in design: [data storage](../../../system-design/modules/04-data-storage/overview.en.md)
