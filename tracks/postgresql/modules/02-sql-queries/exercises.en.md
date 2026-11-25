---
title: "Exercises: SQL queries"
lang: en
track: postgresql
kind: module
doc: exercises
slug: 02-sql-queries
part: 1
tags: [sql, select, group-by, having, null]
status: draft
updated: 2026-11-25
---

# Exercises: SQL queries

🌐 [Русский](exercises.ru.md) · **English**

## 1. 🟢 Filtering

Select every active order whose amount is greater than 100.

<details><summary>Solution</summary>

```sql
SELECT *
FROM orders
WHERE active IS TRUE AND amount > 100;
```

</details>

## 2. 🟡 Aggregation

Return customers whose order total is greater than or equal to 100.

<details><summary>Solution</summary>

```sql
SELECT customer, sum(amount) AS total
FROM orders
GROUP BY customer
HAVING sum(amount) >= 100
ORDER BY total DESC;
```

</details>
