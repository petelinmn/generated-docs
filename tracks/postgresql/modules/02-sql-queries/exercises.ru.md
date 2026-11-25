---
title: "Упражнения: SQL-запросы"
lang: ru
track: postgresql
kind: module
doc: exercises
slug: 02-sql-queries
part: 1
tags: [sql, select, group-by, having, null]
status: draft
updated: 2026-11-25
---

# Упражнения: SQL-запросы

🌐 **Русский** · [English](exercises.en.md)

## 1. 🟢 Фильтрация

Выберите все активные заказы с суммой больше 100.

<details><summary>Решение</summary>

```sql
SELECT *
FROM orders
WHERE active IS TRUE AND amount > 100;
```

</details>

## 2. 🟡 Агрегация

Получите клиентов с суммой заказов больше или равной 100.

<details><summary>Решение</summary>

```sql
SELECT customer, sum(amount) AS total
FROM orders
GROUP BY customer
HAVING sum(amount) >= 100
ORDER BY total DESC;
```

</details>
