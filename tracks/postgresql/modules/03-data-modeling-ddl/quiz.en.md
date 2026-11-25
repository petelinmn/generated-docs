---
title: "Quiz: data modeling and DDL"
lang: en
track: postgresql
kind: module
doc: quiz
slug: 03-data-modeling-ddl
part: 1
tags: [ddl, primary-key, foreign-key, constraints]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Quiz: data modeling and DDL

🌐 [Русский](quiz.ru.md) · **English**

## 1. 🟢 Which type fits a fixed-precision money amount?

- A) `numeric`
- B) `text`
- C) `jsonb`
- D) `bigserial`

<details><summary>Answer</summary>

**A.** `numeric(p,s)` preserves an exact decimal amount.

</details>

## 2. 🟡 What does `ON DELETE CASCADE` do?

- A) Blocks parent deletion
- B) Deletes dependent child rows
- C) Creates a backup
- D) Nothing

<details><summary>Answer</summary>

**B.** Cascade propagation removes linked child rows.

</details>

## 3. 🔴 Find the issue

```sql
CREATE TABLE orders (
    id bigint PRIMARY KEY,
    customer_id bigint REFERENCES customers(id)
);
```

<details><summary>Answer</summary>

If a customer is required, add `NOT NULL` to `customer_id`. Otherwise an order can exist without a customer.

</details>
