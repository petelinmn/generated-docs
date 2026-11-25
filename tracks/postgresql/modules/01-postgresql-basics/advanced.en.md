---
title: "PostgreSQL basics: advanced"
lang: en
track: postgresql
kind: module
doc: advanced
slug: 01-postgresql-basics
part: 1
tags: [search-path, identifiers, ddl-transactions, information-schema, pg-catalog]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# PostgreSQL basics: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

This builds on [basics](basics.en.md).

## Identifiers and schemas

Unquoted identifiers are folded to lowercase. Quoted identifiers preserve case but make every reference more cumbersome:

```sql
CREATE TABLE app.accounts (
    account_id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    display_name text
);
SHOW search_path;
SET search_path TO app, public;
SELECT * FROM accounts;
```

Applications should use one explicit schema strategy and consistent naming.

## DDL transactions

Most PostgreSQL DDL commands can run inside a transaction:

```sql
BEGIN;
CREATE TABLE app.audit_log (id bigint GENERATED ALWAYS AS IDENTITY, event text);
ALTER TABLE app.audit_log ADD CONSTRAINT audit_event_required CHECK (event IS NOT NULL);
ROLLBACK;
```

The table and constraint do not exist after rollback.

## Metadata

The `pg_catalog` system catalog describes database objects. `information_schema` is a simpler standard-oriented view.

```sql
SELECT table_schema, table_name
FROM information_schema.tables
WHERE table_type = 'BASE TABLE'
ORDER BY table_schema, table_name;
```

## Best practices

- Qualify objects explicitly in application SQL and migrations.
- Create an application database or schema separately as needed.
- Use transactions for related DDL changes.
- Avoid arbitrary quoted identifiers.
- Inspect metadata before dropping objects.

## Common mistakes

- ❌ Rely on implicit schema lookup → ✅ set and document `search_path`.
- ❌ Create test objects in public → ✅ isolate with a schema or role.
- ❌ Assume rollback covers a network cutoff → ✅ verify server state and logs.

## Related materials

- Next module: [02 · SQL queries](../02-sql-queries/overview.en.md).

## Sources

- [PostgreSQL schemas](https://www.postgresql.org/docs/current/ddl-schemas.html)
- [PostgreSQL identifiers](https://www.postgresql.org/docs/current/sql-syntax-lexical.html)
- [PostgreSQL system catalogs](https://www.postgresql.org/docs/current/catalogs.html)
