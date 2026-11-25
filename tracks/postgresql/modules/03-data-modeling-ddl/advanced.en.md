---
title: "Data modeling and DDL: advanced"
lang: en
track: postgresql
kind: module
doc: advanced
slug: 03-data-modeling-ddl
part: 1
tags: [normalization, enum, generated-column, migrations, on-delete]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Data modeling and DDL: advanced

🌐 [Русский](advanced.ru.md) · **English**

## Normalization

A normalized model avoids storing the same fact in multiple places. Denormalization is possible for read performance, but synchronization becomes a responsibility.

## ON DELETE

```sql
CREATE TABLE order_items (
    order_id bigint NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id bigint NOT NULL REFERENCES products(id),
    quantity integer NOT NULL CHECK (quantity > 0),
    PRIMARY KEY (order_id, product_id)
);
```

`CASCADE` removes dependent rows with the parent; it must be explicit in deletion design.

## Generated columns

```sql
CREATE TABLE invoices (
    net numeric(12,2),
    tax numeric(12,2),
    total numeric(12,2) GENERATED ALWAYS AS (net + tax) STORED
);
```

## Migrations

Safe migrations add compatibility before removing old dependencies. Applications must support read/write compatibility across deployed versions.

## Sources

- [PostgreSQL constraints](https://www.postgresql.org/docs/current/ddl-constraints.html)
- [PostgreSQL generated columns](https://www.postgresql.org/docs/current/ddl-generated-columns.html)
