---
title: "SQL-запросы: базовая теория"
lang: ru
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

# SQL-запросы: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## SELECT и WHERE

```sql
SELECT name, amount
FROM orders
WHERE amount >= 30
ORDER BY amount DESC;
```

Строки фильтруются по предикату `WHERE`, а результат сортируется по убывающей сумме.

## GROUP BY и HAVING

```sql
SELECT customer, sum(amount) AS total
FROM orders
GROUP BY customer
HAVING sum(amount) >= 30;
```

`HAVING` фильтрует сгруппированные строки, поэтому сумма не доступна обычному `WHERE`.

## Пагинация

```sql
SELECT id, name
FROM users
ORDER BY id
LIMIT 20 OFFSET 40;
```

`OFFSET` пропускает строки до начала страницы, но keyset-пагинация лучше при частом изменении данных.

## Ошибки новичка

- ❌ Считать `WHERE` фильтром после агрегации → ✅ использовать `HAVING`.
- ❌ Ожидать сортировку без `ORDER BY` → ✅ задавать ORDER.
- ❌ Сравнивать `NULL` через `=` → ✅ `IS NULL`.
- ❌ Использовать `LIMIT` на числовом наборе без сортировки → ✅ добавить deterministic `ORDER BY`.

## Источники

- [PostgreSQL SELECT](https://www.postgresql.org/docs/current/sql-select.html)
- [PostgreSQL queries](https://www.postgresql.org/docs/current/queries.html)
