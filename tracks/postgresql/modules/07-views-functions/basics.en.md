---
title: "Views and functions: basics"
lang: en
track: postgresql
kind: module
doc: basics
slug: 07-views-functions
part: 2
tags: [view, materialized-view, plpgsql, stored-procedure, trigger]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Views and functions: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** `SELECT`, joins · **Time:** ~2 h

## Data for the examples

All results were produced on PostgreSQL 17.11.

```sql
CREATE TABLE products (
    id int PRIMARY KEY, name text NOT NULL,
    price numeric(10,2) NOT NULL, active boolean NOT NULL DEFAULT true
);
INSERT INTO products VALUES (1,'Pen',2,true), (2,'Book',15,true), (3,'Lamp',30,false);
```

## Views

```sql
CREATE VIEW active_products AS
    SELECT id, name, price, active FROM products WHERE active;

SELECT id, name FROM active_products ORDER BY id;   -- 1 Pen | 2 Book
```

A view is not a copy: each query against it runs the underlying query on current data. It gives a name to a query and a place to restrict columns and rows.

## SQL functions

```sql
CREATE FUNCTION price_with_tax(price numeric, rate numeric DEFAULT 0.2) RETURNS numeric
LANGUAGE sql IMMUTABLE
RETURN round(price * (1 + rate), 2);

SELECT price_with_tax(10), price_with_tax(10, rate => 0.1);   -- 12.00 | 11.00
```

Parameters may have defaults and can be passed by name with `=>`. `IMMUTABLE` says the result depends only on the arguments.

## Functions returning rows

```sql
CREATE FUNCTION cheaper_than(limit_price numeric) RETURNS TABLE (id int, price numeric)
LANGUAGE sql STABLE AS $$
    SELECT id, price FROM products WHERE price < limit_price
$$;

SELECT * FROM cheaper_than(20) ORDER BY id;
```

A table function is used in `FROM` and joins like a table.

## Dollar quoting

The body is a string. `$$ ... $$` lets you write quotes inside it without escaping. The newer `RETURN expr` and `BEGIN ATOMIC ... END` forms are parsed when the function is created.

## Beginner mistakes

- ❌ Expecting a view to make a query faster → ✅ it runs the same query.
- ❌ `SELECT *` in a view → ✅ list the columns.
- ❌ Naming a parameter like a column → ✅ use a prefix such as `p_`.
- ❌ Logic in functions that nobody keeps in version control → ✅ treat them as code, with migrations.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [PostgreSQL 17: CREATE VIEW](https://www.postgresql.org/docs/17/sql-createview.html)
- [PostgreSQL 17: CREATE FUNCTION](https://www.postgresql.org/docs/17/sql-createfunction.html)
- [PostgreSQL 17: trigger behavior](https://www.postgresql.org/docs/17/trigger-definition.html)
