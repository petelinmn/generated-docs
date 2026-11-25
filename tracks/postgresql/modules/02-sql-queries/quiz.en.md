---
title: "Quiz: SQL queries"
lang: en
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

# Quiz: SQL queries

🌐 [Русский](quiz.ru.md) · **English**

## 1. 🟢 How do you test a column for NULL?

- A) `= NULL`
- B) `IS NULL`
- C) `== NULL`
- D) `NULL`

<details><summary>Answer</summary>

**B.** NULL is checked with `IS NULL` or `IS NOT NULL`.

</details>

## 2. 🟡 Why is HAVING used after GROUP BY?

- A) To filter rows before grouping
- B) To filter aggregated groups
- C) To sort
- D) To drop tables

<details><summary>Answer</summary>

**B.** HAVING operates on grouped results.

</details>

## 3. 🔴 Find the issue

```sql
SELECT customer, sum(amount)
FROM orders
WHERE sum(amount) > 100
GROUP BY customer;
```

<details><summary>Answer</summary>

An aggregate cannot be used in WHERE. Move the condition to HAVING.

</details>
