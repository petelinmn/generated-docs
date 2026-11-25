---
title: "PostgreSQL basics: expert"
lang: en
track: postgresql
kind: module
doc: expert
slug: 01-postgresql-basics
part: 1
tags: [architecture, catalogs, processes, connections, postgres]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# PostgreSQL basics: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

This builds on [advanced theory](advanced.en.md).

## Object reference

| Level | Purpose |
|---|---|
| Cluster | data directory and a server process set |
| Database | isolation boundary for object catalogs |
| Schema | namespace for tables, types, functions, and views |
| Table | rows with a fixed set of named columns |
| Row | values in the context of a table |

## Processes and connections

PostgreSQL serves connections through server processes and shared memory. A connection authenticates, selects a database and role, and uses a backend process or implementation-specific worker architecture.

Applications should use connection pooling when connections are expensive or numerous. The server checks roles and privileges.

## System catalogs

`pg_catalog` stores definitions of tables, columns, types, operators, and database objects. SQL DDL changes catalogs; they are not ordinary application storage.

## Traps

1. Two databases may contain identical schema and table names without conflict.
2. A database or schema is not a tenant or user automatically.
3. `DROP DATABASE` cannot run inside a transaction.
4. `TABLE` in psql is shorthand for `SELECT * FROM`, not a DDL command.
5. A server cluster contains databases, but each application connects to a specific one.

## Interview questions

<details><summary>1. How does a database differ from a schema?</summary>

A database contains schemas and provides a stronger isolation boundary. A schema is a namespace inside a database.

</details>

<details><summary>2. What is pg_catalog for?</summary>

It stores PostgreSQL system metadata and enables diagnosis of objects, dependencies, and configuration.

</details>

<details><summary>3. Why pool connections?</summary>

Backend connection creation can be expensive under high concurrency. A pool limits connection count and reuses sessions.

</details>

<details><summary>4. Can DDL run inside a transaction?</summary>

Many DDL commands support transactions. Exceptions depend on the specific operation and must be checked in official documentation.

</details>

<details><summary>5. Why does search_path matter?</summary>

It sets lookup order for unqualified names and may change which object resolves the SQL.

</details>

<details><summary>6. What happens after a semicolon in psql?</summary>

psql sends the accumulated SQL statement to the server. Execution starts after the statement is terminated or a meta-command is entered.

</details>

<details><summary>7. How does a SQL client differ from the server?</summary>

The client parses user input and sends protocol requests. The server plans and executes queries.

</details>

<details><summary>8. Why should a PostgreSQL port not be exposed without configuration?</summary>

Password and TLS configuration, roles, privileges, and a firewall protect data from unauthorized access.

</details>

<details><summary>9. Which object identifies a row?</summary>

PostgreSQL itself does not guarantee a unique application row identity without a primary key. The model must choose one.

</details>

<details><summary>10. How does information_schema differ from pg_catalog?</summary>

`information_schema` provides standardized portable views, while `pg_catalog` contains PostgreSQL-specific metadata.

</details>

## Related materials

- Advanced: [basic concepts](advanced.en.md).
- Next module: [02 · SQL queries](../02-sql-queries/overview.en.md).

## Sources

- [PostgreSQL: overview](https://www.postgresql.org/docs/current/intro-whatis.html)
- [PostgreSQL: system catalogs](https://www.postgresql.org/docs/current/catalogs.html)
- [PostgreSQL: connection and authentication](https://www.postgresql.org/docs/current/runtime-config-connection.html)
