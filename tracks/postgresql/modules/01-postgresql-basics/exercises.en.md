---
title: "Exercises: PostgreSQL basics"
lang: en
track: postgresql
kind: module
doc: exercises
slug: 01-postgresql-basics
part: 1
tags: [postgresql, psql, database, schema, table]
status: draft
updated: 2026-11-25
---

# Exercises: PostgreSQL basics

🌐 [Русский](exercises.ru.md) · **English**

## 1. 🟢 Start the server

Start a PostgreSQL container and enter `psql`.

**Expected result:** `SELECT current_database();` returns `learn`.

<details><summary>Solution</summary>

```bash
docker run -d --name learn-postgres \
  -e POSTGRES_PASSWORD=learn -e POSTGRES_DB=learn \
  -p 5432:5432 postgres:17-alpine
docker exec -it learn-postgres psql -U postgres -d learn
```

</details>

## 2. 🟢 Create a table

Create an `app` schema and an `app.users` table with an identity primary key.

<details><summary>Solution</summary>

```sql
CREATE SCHEMA app;
CREATE TABLE app.users (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name text NOT NULL
);
```

</details>

## 3. 🟡 DDL transaction

Create a table inside a transaction, inspect metadata, and roll it back.

<details><summary>Solution</summary>

```sql
BEGIN;
CREATE TABLE app.audit_log (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    event text NOT NULL
);
SELECT to_regclass('app.audit_log');
ROLLBACK;
SELECT to_regclass('app.audit_log');
```

</details>
