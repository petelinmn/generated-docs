---
title: "PostgreSQL basics: basics"
lang: en
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

# PostgreSQL basics: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · **4. Quiz** ([quiz](quiz.en.md), [exercises](exercises.en.md))

**Prerequisites:** none · **Time:** ~2 h

## Why it matters

PostgreSQL stores relational data and executes SQL queries. Working with it requires understanding the differences among a database, a schema, a table, and the SQL client.

## Start a local server

```bash
docker run -d --name learn-postgres \
  -e POSTGRES_PASSWORD=learn -e POSTGRES_DB=learn \
  -p 5432:5432 postgres:17-alpine
docker exec -it learn-postgres psql -U postgres -d learn
```

Inside `psql`, `\l` lists databases, `\dt` lists tables, and `\d name` describes an object.

## Database, schema, and table

A database isolates contents at the catalog level. The default schema is `public` and contains tables:

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

## SQL command groups

- DDL: `CREATE`, `ALTER`, `DROP`.
- DML: `INSERT`, `UPDATE`, `DELETE`, `MERGE`.
- DQL: `SELECT`.
- TCL: `BEGIN`, `COMMIT`, `ROLLBACK`.

## A small psql example

```sql
SELECT current_database(), current_user, version();
```

## Beginner mistakes

- ❌ Treat a database and a schema as the same object → ✅ a database contains schemas.
- ❌ Run `psql learn` and forget the user → ✅ use `psql -U postgres -d learn`.
- ❌ Refer to a table without a schema after changing `search_path` → ✅ use `app.users`.
- ❌ Run DDL without a rollback plan in production → ✅ plan migrations and backups.

## Related materials

- Next: [advanced theory](advanced.en.md).
- Next module: [02 · SQL queries](../02-sql-queries/overview.en.md).

## Sources

- [PostgreSQL tutorial](https://www.postgresql.org/docs/current/tutorial-sql.html)
- [PostgreSQL data definition](https://www.postgresql.org/docs/current/ddl.html)
- [psql](https://www.postgresql.org/docs/current/app-psql.html)
