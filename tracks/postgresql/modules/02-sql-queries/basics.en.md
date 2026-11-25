---
title: "SQL queries: basics"
lang: en
track: postgresql
kind: module
doc: basics
slug: 02-sql-queries
part: 1
tags: [sql, select, where, group-by, having, order-by, limit]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# SQL queries: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## SELECT and WHERE

```sql
SELECT name, amount
FROM orders
WHERE amount >= 30
ORDER BY amount DESC;
```

Rows are filtered by `WHERE`, and the result is sorted by descending amount.

## GROUP BY and HAVING

```sql
SELECT customer, sum(amount) AS total
FROM orders
GROUP BY customer
HAVING sum(amount) >= 30;
```

`HAVING` filters grouped rows, so aggregate totals are not available to ordinary `WHERE`.

## Pagination

```sql
SELECT id, name
FROM users
ORDER BY id
LIMIT 20 OFFSET 40;
```

`OFFSET` skips rows before the page, but keyset pagination is better when data changes frequently.

## Beginner mistakes

- ❌ Treat `WHERE` as a post-aggregate filter → ✅ use `HAVING`.
- ❌ Expect sorting without `ORDER BY` → ✅ specify it.
- ❌ Compare `NULL` with `=` → ✅ use `IS NULL`.
- ❌ Use `LIMIT` with an unsorted set → ✅ add deterministic `ORDER BY`.

## Sources

- [PostgreSQL SELECT](https://www.postgresql.org/docs/current/sql-select.html)
- [PostgreSQL queries](https://www.postgresql.org/docs/current/queries.html)
