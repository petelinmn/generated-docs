---
title: "Views and functions"
lang: en
track: postgresql
kind: module
doc: overview
slug: 07-views-functions
part: 2
levels: [1, 2, 3, 4]
tags: [view, materialized-view, plpgsql, stored-procedure, trigger]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Views and functions

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A view is a saved query that behaves like a table. A function is code stored in the database that returns a value or a set of rows, a procedure is code you `CALL`, and a trigger is a function that the database runs by itself when data changes. PostgreSQL's built-in procedural language is PL/pgSQL. These tools put logic next to the data: useful for shared rules and dangerous when it becomes invisible.

## Key ideas

- A plain view stores the query, not the data; a materialized view stores the result.
- Simple views are updatable; `WITH CHECK OPTION` stops rows from escaping the view.
- `SELECT *` in a view is expanded once, when the view is created.
- Volatility (`VOLATILE`, `STABLE`, `IMMUTABLE`) is a promise the planner relies on.
- Only a procedure called with `CALL` can commit inside its body.
- `BEFORE` triggers can change or skip a row, `AFTER` triggers react to it.
- `SECURITY DEFINER` runs with the owner's rights and needs a fixed `search_path`.

## Minimal example

```sql
CREATE TABLE products (
    id int PRIMARY KEY, name text NOT NULL,
    price numeric(10,2) NOT NULL, active boolean NOT NULL DEFAULT true
);
INSERT INTO products VALUES (1,'Pen',2,true), (2,'Book',15,true), (3,'Lamp',30,false);
```

```sql
CREATE VIEW active_products AS
    SELECT id, name, price, active FROM products WHERE active;

SELECT id, name FROM active_products ORDER BY id;   -- 1 Pen | 2 Book
```

```sql
CREATE FUNCTION price_with_tax(price numeric, rate numeric DEFAULT 0.2) RETURNS numeric
LANGUAGE sql IMMUTABLE
RETURN round(price * (1 + rate), 2);

SELECT price_with_tax(10), price_with_tax(10, rate => 0.1);   -- 12.00 | 11.00
```

## When to use and when not

- ✅ A stable interface over a schema that changes; restricting what a role sees.
- ✅ Invariants and audit that must hold for every client of the database.
- ❌ Business processes with external calls inside triggers.
- ❌ Layers of views over views that nobody can explain a plan for.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | views, SQL functions, parameters, table functions |
| [2. Advanced](advanced.en.md) | updatable and materialized views, PL/pgSQL, procedures, triggers |
| [3. Expert](expert.en.md) | volatility, security, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn postgresql 07-views-functions level 5` |
| 6. Tests | not created yet — `/learn postgresql 07-views-functions level 6` |

## Related materials

- Prerequisites: [04 · Joins and subqueries](../04-joins-and-subqueries/overview.en.md), [06 · Transactions](../06-transactions/overview.en.md)
- Next: the following module of the [PostgreSQL roadmap](../../overview.en.md)
- From .NET: [EF Core](../../../csharp/modules/15-ef-core/overview.en.md)
- Where stored logic fits in a design: [architecture styles](../../../system-design/modules/11-architecture-styles/overview.en.md)
