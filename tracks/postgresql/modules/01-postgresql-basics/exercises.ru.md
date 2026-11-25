---
title: "Упражнения: основы PostgreSQL"
lang: ru
track: postgresql
kind: module
doc: exercises
slug: 01-postgresql-basics
part: 1
tags: [postgresql, psql, database, schema, table]
status: draft
updated: 2026-11-25
---

# Упражнения: основы PostgreSQL

🌐 **Русский** · [English](exercises.en.md)

## 1. 🟢 Запуск сервера

Запустите контейнер PostgreSQL и войдите в `psql`.

**Ожидаемый результат:** команда `SELECT current_database();` возвращает `learn`.

<details><summary>Решение</summary>

```bash
docker run -d --name learn-postgres \
  -e POSTGRES_PASSWORD=learn -e POSTGRES_DB=learn \
  -p 5432:5432 postgres:17-alpine
docker exec -it learn-postgres psql -U postgres -d learn
```

</details>

## 2. 🟢 Создайте таблицу

Создайте схему `app` и таблицу `app.users` с identity-первичным ключом.

<details><summary>Решение</summary>

```sql
CREATE SCHEMA app;
CREATE TABLE app.users (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name text NOT NULL
);
```

</details>

## 3. 🟡 Транзакция DDL

Создайте таблицу в транзакции, проверьте её метаданные и откатите.

<details><summary>Решение</summary>

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
