---
title: "Основы PostgreSQL: базовая теория"
lang: ru
track: postgresql
kind: module
doc: basics
slug: 01-postgresql-basics
part: 1
tags: [postgresql, architecture, client-server, database, schema, table, psql]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Основы PostgreSQL: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** нет · **Время:** ~2 ч

## Зачем это нужно

PostgreSQL хранит реляционные данные и выполняет SQL-запросы. Чтобы работать с сервером, достаточно понимать различия базы, схемы, таблицы и SQL-клиента.

## Запуск локального сервера

```bash
docker run -d --name learn-postgres \
  -e POSTGRES_PASSWORD=learn -e POSTGRES_DB=learn \
  -p 5432:5432 postgres:17-alpine
docker exec -it learn-postgres psql -U postgres -d learn
```

Внутри `psql` метакоманда `\l` показывает базы, `\dt` — таблицы, `\d имя` — структуру объекта.

## База, схема и таблица

База изолирует содержимое на уровне каталога. Схема по умолчанию называется `public` и содержит таблицы:

```sql
CREATE SCHEMA app;
CREATE TABLE app.users (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name text NOT NULL
);
INSERT INTO app.users (name) VALUES ('Ada');
SELECT * FROM app.users;
```

```text
 id | name
----+------
  1 | Ada
```

## Команды SQL

- DDL: `CREATE`, `ALTER`, `DROP`.
- DML: `INSERT`, `UPDATE`, `DELETE`, `MERGE`.
- DQL: `SELECT`.
- TCL: `BEGIN`, `COMMIT`, `ROLLBACK`.

## Классный пример с psql

```sql
SELECT current_database(), current_user, version();
```

## Ошибки новичка

- ❌ Считать базу и схему одним объектом → ✅ база содержит схемы.
- ❌ Писать `psql learn` и забыть пользователя → ✅ использовать `psql -U postgres -d learn`.
- ❌ Обращаться к таблице без схемы при нестандартном `search_path` → ✅ явно указывать `app.users`.
- ❌ Применять DDL без точки отката в production → ✅ планировать миграции и резервные копии.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md).
- Следующий модуль: [02 · SQL-запросы](../02-sql-queries/overview.ru.md).

## Источники

- [PostgreSQL tutorial](https://www.postgresql.org/docs/current/tutorial-sql.html)
- [PostgreSQL data definition](https://www.postgresql.org/docs/current/ddl.html)
- [psql](https://www.postgresql.org/docs/current/app-psql.html)
