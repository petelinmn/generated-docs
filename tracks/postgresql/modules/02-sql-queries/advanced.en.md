---
title: "SQL queries: advanced"
lang: en
track: postgresql
kind: module
doc: advanced
slug: 02-sql-queries
part: 1
tags: [null, distinct, case, unions, window-functions]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# SQL queries: advanced

🌐 [Русский](advanced.ru.md) · **English**

## NULL and DISTINCT

```sql
SELECT name
FROM users
WHERE deleted_at IS NULL;

SELECT DISTINCT customer
FROM orders;
```

`NULL` is unknown and is not equal to any value. `DISTINCT` compares value sets in a result.

## CASE and set operations

```sql
SELECT name,
       CASE WHEN active THEN 'active' ELSE 'inactive' END AS status
FROM users;

SELECT customer FROM old_orders
UNION
SELECT customer FROM orders;
```

`UNION` removes duplicates; `UNION ALL` preserves them.

## Window functions

```sql
SELECT customer, amount,
       sum(amount) OVER (PARTITION BY customer ORDER BY id) AS running_total
FROM orders;
```

A window function does not collapse rows into one group.

## Sources

- [PostgreSQL window functions](https://www.postgresql.org/docs/current/functions-window.html)
- [PostgreSQL set operations](https://www.postgresql.org/docs/current/queries-union.html)
