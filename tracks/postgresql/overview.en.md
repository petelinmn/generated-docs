---
title: "PostgreSQL"
lang: en
track: postgresql
kind: track
doc: overview
slug: postgresql
tags: [postgresql, sql, relational-database, transactions, indexes, ddl]
versions: "PostgreSQL 17+"
default_level: 4
status: draft
updated: 2026-11-25
---

# PostgreSQL

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

PostgreSQL is an open-source object-relational database management system with SQL support, ACID transactions, and an extensive type system. The server stores data in tables, executes queries through a planner, checks integrity through constraints, and provides multiple transaction isolation levels.

PostgreSQL is suitable for OLTP applications, relational search, JSON documents, geospatial data, and analytics. This track starts with installation, SQL, and schema design, then covers queries, indexes, transactions, performance, and operations.

**Prerequisites:** SQL knowledge is not required; basic command-line reading and writing is sufficient.

## Environment

| What | Tool |
|---|---|
| Server | PostgreSQL 17 in Docker: `postgres:17-alpine` |
| Client | `psql`, run inside the container or as a local utility |
| Example language | SQL and Python 3.12+ with `psycopg` |
| Module code | at levels 5–6: `code/` with SQL examples, solutions, and pytest |
| Run SQL in the disposable server | `docker exec -i learn-postgres psql -U postgres -d learn < file.sql` |
| Run module tests | `uv run pytest` inside `code/` |

```bash
docker run -d --name learn-postgres \
  -e POSTGRES_PASSWORD=learn -e POSTGRES_DB=learn \
  -p 5432:5432 postgres:17-alpine
docker exec -it learn-postgres psql -U postgres -d learn
```

> The `learn` password is only for a disposable local container. Do not publish the port or use these settings in production.

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). The default detail level is **4** (theory and self-check).

### Part 1. Fundamentals

- [01 · PostgreSQL basics](modules/01-postgresql-basics/overview.en.md) — architecture, client/server, databases, schemas, tables, `psql`
- [02 · SQL queries](modules/02-sql-queries/overview.en.md) — `SELECT`, filtering, sorting, set-like row operations, aggregates
- [03 · Data modeling and DDL](modules/03-data-modeling-ddl/overview.en.md) — types, primary and foreign keys, constraints, DDL
- [04 · Joins and subqueries](modules/04-joins-and-subqueries/overview.en.md) — INNER, OUTER, CROSS, CTE, EXISTS

### Part 2. Advanced topics

- [05 · Indexes](modules/05-indexes/overview.en.md) — B-tree, Hash, GIN, GiST, BRIN, partial indexes
- [06 · Transactions](modules/06-transactions/overview.en.md) — ACID, isolation levels, locks, and MVCC
- [07 · Views and functions](modules/07-views-functions/overview.en.md) — views, functions, triggers, procedural languages
- [08 · JSON and operators](modules/08-json-and-operators/overview.en.md) — `jsonb`, JSON indexes, operator classes
- [09 · Performance and EXPLAIN](modules/09-performance-explain/overview.en.md) — query plans, statistics, VACUUM and ANALYZE

### Part 3. Expert topics

- [10 · Partitioning](modules/10-partitioning/overview.en.md) — declarative partitioning and inheritance
- [11 · Replication](modules/11-replication/overview.en.md) — streaming replication, WAL, logical replication
- [12 · Backups and recovery](modules/12-backups-and-recovery/overview.en.md) — pg_dump, pg_basebackup, PITR
- [13 · Security](modules/13-security/overview.en.md) — roles, privileges, RLS, SSL
- [14 · Operations and observability](modules/14-operations/overview.en.md) — configuration, logging, metrics, extensions

## Libraries and tools

- [psycopg](libraries/psycopg/overview.en.md) — PostgreSQL Python driver
- [SQLAlchemy](libraries/sqlalchemy/overview.en.md) — ORM and SQL toolkit
- [psql](tools/psql/overview.en.md) — interactive and batch SQL client
- [pg_dump](tools/pg-dump/overview.en.md) — logical backups
- [pgAdmin](tools/pgadmin/overview.en.md) — graphical administration

## Recipes

- [Pagination](recipes/pagination/overview.en.md) — keyset and offset pagination
- [Schema migrations](recipes/schema-migrations/overview.en.md) — safe changes and rollbacks
- [Transactional outbox](recipes/outbox/overview.en.md) — reliable event publication
- [Safe configuration](recipes/configuration/overview.en.md) — roles and connection strings

## Projects

- [Inventory service](projects/inventory-service/overview.en.md) — schema, queries, transactions, and backup

## Related tracks

- [Python](../python/overview.en.md) — examples and clients
- [C# / .NET](../csharp/overview.en.md) — EF Core and databases
- [Kubernetes](../kubernetes/overview.en.md) — StatefulSets and operators
- [Linux](../linux/overview.en.md) — processes, networking, and diagnostics

## Sources

- [PostgreSQL 19 documentation](https://www.postgresql.org/docs/19/index.html)
- [PostgreSQL tutorial](https://www.postgresql.org/docs/current/tutorial-sql.html)
- [PostgreSQL SQL commands](https://www.postgresql.org/docs/current/sql.html)
- [PostgreSQL EXPLAIN](https://www.postgresql.org/docs/current/using-explain.html)
