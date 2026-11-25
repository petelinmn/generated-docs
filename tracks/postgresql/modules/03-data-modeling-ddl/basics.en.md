---
title: "Data modeling and DDL: basics"
lang: en
track: postgresql
kind: module
doc: basics
slug: 03-data-modeling-ddl
part: 1
tags: [ddl, data-types, primary-key, foreign-key, constraints]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Data modeling and DDL: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## CREATE TABLE

```sql
CREATE TABLE products (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    sku text NOT NULL UNIQUE,
    price numeric(12,2) NOT NULL CHECK (price >= 0)
);
```

## One-to-many relationship

```sql
CREATE TABLE customers (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email text NOT NULL UNIQUE
);

CREATE TABLE orders (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    customer_id bigint NOT NULL REFERENCES customers(id)
);
```

Every order references an existing customer.

## ALTER and DROP

```sql
ALTER TABLE products ADD COLUMN active boolean NOT NULL DEFAULT true;
ALTER TABLE products DROP COLUMN active;
DROP TABLE products;
```

In production, migration and application compatibility must be checked first.

## Beginner mistakes

- ❌ Use text for a strict number → ✅ use numeric or integer.
- ❌ Create a foreign key on nullable data without intent → ✅ use NOT NULL.
- ❌ Store email without UNIQUE → ✅ reject duplicates.
- ❌ Drop a table before checking backups and dependencies → ✅ inspect first.

## Sources

- [PostgreSQL data types](https://www.postgresql.org/docs/current/datatype.html)
- [PostgreSQL constraints](https://www.postgresql.org/docs/current/ddl-constraints.html)
