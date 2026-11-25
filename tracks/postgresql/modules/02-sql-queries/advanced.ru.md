---
title: "SQL-запросы: продвинутая теория"
lang: ru
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

# SQL-запросы: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

## NULL и DISTINCT

```sql
SELECT name
FROM users
WHERE deleted_at IS NULL;

SELECT DISTINCT customer
FROM orders;
```

`NULL` неизвестен и не равен никакому значению. `DISTINCT` сравнивает наборы значений в выборке.

## CASE и set operations

```sql
SELECT name,
       CASE WHEN active THEN 'active' ELSE 'inactive' END AS status
FROM users;

SELECT customer FROM old_orders
UNION
SELECT customer FROM orders;
```

`UNION` удаляет дубликаты, а `UNION ALL` сохраняет их.

## Window-функции

```sql
SELECT customer, amount,
       sum(amount) OVER (PARTITION BY customer ORDER BY id) AS running_total
FROM orders;
```

Window-функция не сворачивает строки в одну группу.

## Источники

- [PostgreSQL window functions](https://www.postgresql.org/docs/current/functions-window.html)
- [PostgreSQL set operations](https://www.postgresql.org/docs/current/queries-union.html)
