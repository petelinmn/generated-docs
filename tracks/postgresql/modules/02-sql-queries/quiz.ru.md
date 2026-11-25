---
title: "Квиз: SQL-запросы"
lang: ru
track: postgresql
kind: module
doc: quiz
slug: 02-sql-queries
part: 1
tags: [sql, select, where, having, null, aggregates]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Квиз: SQL-запросы

🌐 **Русский** · [English](quiz.en.md)

## 1. 🟢 Как проверить столбец на NULL?

- A) `= NULL`
- B) `IS NULL`
- C) `== NULL`
- D) `NULL`

<details><summary>Ответ</summary>

**B.** NULL проверяется предикатом `IS NULL` или `IS NOT NULL`.

</details>

## 2. 🟡 Почему HAVING используется после GROUP BY?

- A) Для фильтра строк до группировки
- B) Для фильтра агрегированных групп
- C) Для сортировки
- D) Для удаления таблиц

<details><summary>Ответ</summary>

**B.** HAVING работает с результатом группировки.

</details>

## 3. 🔴 Найдите проблему

```sql
SELECT customer, sum(amount)
FROM orders
WHERE sum(amount) > 100
GROUP BY customer;
```

<details><summary>Ответ</summary>

Агрегат нельзя использовать в WHERE. Перенесите условие в HAVING.

</details>
