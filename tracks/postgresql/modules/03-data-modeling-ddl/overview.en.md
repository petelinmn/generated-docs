---
title: "Data modeling and DDL"
lang: en
track: postgresql
kind: module
doc: overview
slug: 03-data-modeling-ddl
part: 1
levels: [1, 2, 3, 4]
tags: [ddl, data-types, primary-key, foreign-key, constraints, normalization]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Data modeling and DDL

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

DDL describes database structure: tables, columns, types, and constraints. A good schema connects entities through primary and foreign keys, rejects invalid values, and represents relationships explicitly.

## Key ideas

- Types are chosen for semantics and value ranges.
- A primary key uniquely identifies a row.
- A foreign key checks referential integrity.
- `NOT NULL`, `UNIQUE`, and `CHECK` supplement relationships.
- Normalization reduces duplication and depends on read scenarios.
- Schema changes should run through migrations.

## Minimal example

```sql
CREATE TABLE customers (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email text NOT NULL UNIQUE
);

CREATE TABLE orders (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    customer_id bigint NOT NULL REFERENCES customers(id),
    amount numeric(12,2) NOT NULL CHECK (amount >= 0)
);
```

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | types, DDL, keys, constraints |
| [2. Advanced](advanced.en.md) | normalization, enums, generated columns, migrations |
| [3. Expert](expert.en.md) | relationship reference, catalog dependencies, pitfalls |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not generated yet — `/learn postgresql 03-data-modeling-ddl level 5` |
| 6. Tests | not generated yet — `/learn postgresql 03-data-modeling-ddl level 6` |

## Related materials

- Prerequisites: [02 · SQL queries](../02-sql-queries/overview.en.md).
- Next: the next module in the [PostgreSQL track roadmap](../../overview.en.md).

## Sources

- [PostgreSQL data types](https://www.postgresql.org/docs/current/datatype.html)
- [PostgreSQL constraints](https://www.postgresql.org/docs/current/ddl-constraints.html)
- [PostgreSQL table basics](https://www.postgresql.org/docs/current/ddl-basics.html)
