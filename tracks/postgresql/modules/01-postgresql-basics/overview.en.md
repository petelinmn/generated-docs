---
title: "PostgreSQL basics"
lang: en
track: postgresql
kind: module
doc: overview
slug: 01-postgresql-basics
part: 1
levels: [1, 2, 3, 4]
tags: [postgresql, architecture, client-server, database, schema, table, psql]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# PostgreSQL basics

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

PostgreSQL uses a client/server model: applications send SQL to the server, which plans queries and reads or changes data. One server cluster contains databases, while schemas group tables and objects within a database.

## Key ideas

- A **PostgreSQL cluster** is a set of databases served by one server instance.
- **A database isolates contents**, while a schema organizes objects inside it.
- **`psql`** sends SQL in interactive or batch mode.
- **A table has rows and named columns with types.**
- SQL is divided into DDL, DML, and DQL.
- The `postgres` system database and application database are separate.

## Minimal example

```sql
CREATE TABLE users (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name text NOT NULL
);

INSERT INTO users (name) VALUES ('Ada');
TABLE users;
```

```text
 id | name
----+------
  1 | Ada
```

## When to use and when not

- ✅ Use databases for strong application or tenant isolation.
- ✅ Use schemas to group objects and manage lookup paths.
- ✅ Use `psql` for interactive diagnosis and scripts.
- ❌ Do not use one table for unrelated entity structures without a reason.
- ❌ Do not expose a production port with weak local password settings.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | container, client/server, databases, schemas, tables, psql |
| [2. Advanced](advanced.en.md) | identifiers, search_path, DDL transactions, metadata |
| [3. Expert](expert.en.md) | server processes, catalogs, connection model, traps |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not generated yet — `/learn postgresql 01-postgresql-basics level 5` |
| 6. Tests | not generated yet — `/learn postgresql 01-postgresql-basics level 6` |

## Related materials

- Prerequisites: none.
- Next: [02 · SQL queries](../02-sql-queries/overview.en.md).
- Related track: [Python](../../../python/overview.en.md).

## Sources

- [PostgreSQL: What is PostgreSQL](https://www.postgresql.org/docs/current/intro-whatis.html)
- [PostgreSQL tutorial: SQL](https://www.postgresql.org/docs/current/tutorial-sql.html)
- [PostgreSQL: Table basics](https://www.postgresql.org/docs/current/ddl-basics.html)
