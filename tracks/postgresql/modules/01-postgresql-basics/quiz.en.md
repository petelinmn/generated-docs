---
title: "Quiz: PostgreSQL basics"
lang: en
track: postgresql
kind: module
doc: quiz
slug: 01-postgresql-basics
part: 1
tags: [postgresql, database, schema, table, psql]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Quiz: PostgreSQL basics

🌐 [Русский](quiz.ru.md) · **English**

## 1. 🟢 What is the PostgreSQL server's role?

- A) Store data and execute SQL
- B) Edit HTML
- C) Only start Docker
- D) Replace the SQL client

<details><summary>Answer</summary>

**A.** The server stores data and plans and executes queries.

</details>

## 2. 🟢 How does a schema differ from a database?

- A) A schema is inside a database
- B) They are the same
- C) A database is inside a schema
- D) A schema replaces a table

<details><summary>Answer</summary>

**A.** A database contains schemas; a schema is a namespace inside a database.

</details>

## 3. 🟢 Which type declares an automatically generated identity field?

- A) `GENERATED ALWAYS AS IDENTITY`
- B) `SERIALIZE`
- C) `AUTO TABLE`
- D) `NULL`

<details><summary>Answer</summary>

**A.** Identity automatically generates values for the column.

</details>

## 4. 🟡 What does `SELECT current_database();` show?

- A) Docker version
- B) The current database name
- C) All cluster databases
- D) The operating-system user

<details><summary>Answer</summary>

**B.** The current database is available through a system function.

</details>

## 5. 🔴 Find the issue

```sql
SELECT * FROM accounts;
SET search_path TO reporting;
```

<details><summary>Answer</summary>

The query runs before the new `search_path` is set and may resolve a different object. Qualify schemas explicitly or set search_path before queries.

</details>
