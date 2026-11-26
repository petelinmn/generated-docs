---
title: "Views and functions: expert"
lang: en
track: postgresql
kind: module
doc: expert
slug: 07-views-functions
part: 2
tags: [view, materialized-view, plpgsql, stored-procedure, trigger]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Views and functions: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

A view is stored as a rewrite rule: when a query names it, the view's query is substituted and planned together with the outer query. So conditions can usually reach the base tables and their indexes. A simple SQL function may be inlined the same way; PL/pgSQL functions are executed as they are and their internals are invisible to the outer plan.

### Volatility

The category tells the planner what it may assume. `VOLATILE` is the default. A `STABLE` or `IMMUTABLE` function cannot modify the database. A call to an `IMMUTABLE` function with constant arguments may be evaluated while planning; only `IMMUTABLE` functions may be used in index expressions.

### Rights

By default a view accesses its tables with the rights of the view owner; with `security_invoker = true`, with the rights of the user querying it. `security_barrier` is for views meant to hide rows: it keeps user-supplied functions from seeing rows the view filters out. Functions are `SECURITY INVOKER` by default, and `EXECUTE` on a new function is granted to `PUBLIC`.

```sql
CREATE ROLE reader;
CREATE TABLE secret (v int);  INSERT INTO secret VALUES (7);
CREATE VIEW v_owner AS SELECT v FROM secret;
CREATE VIEW v_invoker WITH (security_invoker = true) AS SELECT v FROM secret;
GRANT SELECT ON v_owner, v_invoker TO reader;
CREATE FUNCTION peek() RETURNS int LANGUAGE sql
    SECURITY DEFINER SET search_path = public, pg_temp
    AS $$ SELECT v FROM secret $$;

SET ROLE reader;
SELECT * FROM v_owner;     -- 7: checked with the view owner's rights
SELECT * FROM v_invoker;   -- ERROR:  permission denied for table secret
SELECT peek();             -- 7: reader never got EXECUTE explicitly
RESET ROLE;
```

For `SECURITY DEFINER` set `search_path` to schemas untrusted users cannot write to, with `pg_temp` last, and `REVOKE EXECUTE ... FROM PUBLIC` before granting it to whoever needs it.

## Reference

| `CREATE FUNCTION` attribute | Default | Meaning |
|---|---|---|
| `VOLATILE` / `STABLE` / `IMMUTABLE` | `VOLATILE` | volatility |
| `CALLED ON NULL INPUT` / `STRICT` | called | behaviour on `NULL` arguments |
| `SECURITY INVOKER` / `DEFINER` | invoker | whose rights |
| `PARALLEL UNSAFE` / `RESTRICTED` / `SAFE` | unsafe | use in parallel plans |
| `SET parameter = value` | — | setting for the duration of the call |
| `COST`, `ROWS` | — | planner estimates |

| Returns | Syntax |
|---|---|
| one value | `RETURNS type` |
| a set of one type | `RETURNS SETOF type` |
| a set of rows | `RETURNS TABLE (col type, ...)` |
| through parameters | `OUT` / `INOUT` |

| Trigger variable | Content |
|---|---|
| `NEW`, `OLD` | the new and old row in row-level triggers |
| `TG_OP` | `INSERT`, `UPDATE`, `DELETE`, `TRUNCATE` |
| `TG_NAME`, `TG_TABLE_NAME` | trigger and table names |
| `TG_WHEN`, `TG_LEVEL` | timing and level |
| `FOUND` | whether the last statement affected a row (any PL/pgSQL code) |

| View option | Effect |
|---|---|
| `WITH [CASCADED \| LOCAL] CHECK OPTION` | writes must stay visible |
| `security_invoker` | check rights of the caller |
| `security_barrier` | for row-hiding views |

## Gotchas

### A view's column list is frozen

```sql
CREATE VIEW all_products AS SELECT * FROM products;
ALTER TABLE products ADD COLUMN sku text;
SELECT * FROM all_products WHERE id = 1;      -- no sku column: * was expanded at creation
ALTER TABLE products DROP COLUMN name;
-- ERROR:  cannot drop column name of table products because other objects depend on it
```

### A false IMMUTABLE gives stale results

```sql
CREATE TABLE settings (rate numeric);  INSERT INTO settings VALUES (0.1);
CREATE FUNCTION current_rate() RETURNS numeric LANGUAGE plpgsql IMMUTABLE AS
$$ BEGIN RETURN (SELECT rate FROM settings); END $$;

PREPARE q AS SELECT current_rate();
EXECUTE q;                        -- 0.1
UPDATE settings SET rate = 0.5;
EXECUTE q;                        -- 0.1  (stale)
SELECT current_rate();            -- 0.5
```

The prepared statement keeps the value computed earlier.

### An index may accept a function you did not mark

```sql
CREATE TABLE w (sku text);
CREATE FUNCTION lower_sql(t text) RETURNS text LANGUAGE sql AS $$ SELECT lower(t) $$;
CREATE INDEX ix_sql ON w (lower_sql(sku));   -- accepted: the body is inlined and checked

CREATE FUNCTION lower_pl(t text) RETURNS text LANGUAGE plpgsql AS $$ BEGIN RETURN lower(t); END $$;
CREATE INDEX ix_pl ON w (lower_pl(sku));
-- ERROR:  functions in index expression must be marked IMMUTABLE
```

The first index works only while the body stays inlinable and immutable; declare the volatility you mean.

### STRICT silently skips the body

```sql
CREATE FUNCTION f_strict(a int) RETURNS int LANGUAGE plpgsql STRICT AS
$$ BEGIN RAISE NOTICE 'called'; RETURN 1; END $$;
CREATE FUNCTION f_loose(a int) RETURNS int LANGUAGE plpgsql AS
$$ BEGIN RAISE NOTICE 'called'; RETURN 1; END $$;

-- both bodies raise a notice and return 1; only f_strict is declared STRICT
SELECT f_strict(NULL), f_loose(NULL);   -- NULL | 1, one notice only
```

## Trade-offs and alternatives

| Choice | Pro | Con |
|---|---|---|
| Logic in the database | one rule for all clients, no round trips | harder to test, version and scale |
| Triggers | cannot be bypassed by application code | hidden behaviour, slower bulk changes |
| Constraints instead of triggers | declarative, correct under concurrency | limited expressiveness |
| Generated columns | simple derived values | only from the same row |
| Materialized view | fast reads of heavy aggregates | stale until refreshed |
| Summary table kept by code | incremental updates | you maintain correctness |

## Interview questions

**1. How does a view differ from a materialized view?**

<details><summary>Answer</summary>

A view stores the query; a materialized view stores its result and must be refreshed.

</details>

**2. Which views are automatically updatable?**

<details><summary>Answer</summary>

Those over a single table or updatable view without grouping, distinct, limits, set operations, aggregates or window functions.

</details>

**3. What do the volatility categories mean?**

<details><summary>Answer</summary>

`VOLATILE` (default) may change anything on every call; `STABLE` returns the same for the same arguments within a statement; `IMMUTABLE` always.

</details>

**4. Function or procedure?**

<details><summary>Answer</summary>

A function is used in expressions and returns a value; a procedure is run with `CALL` and may control transactions.

</details>

**5. What does `STRICT` do?**

<details><summary>Answer</summary>

The function is not called for a `NULL` argument; the result is `NULL`.

</details>

**6. Why is `SECURITY DEFINER` dangerous?**

<details><summary>Answer</summary>

It runs with the owner's rights. Fix `search_path` with `pg_temp` last and revoke `EXECUTE` from `PUBLIC`.

</details>

**7. In what order do triggers of the same kind fire?**

<details><summary>Answer</summary>

Alphabetically by name.

</details>

**8. Row-level or statement-level trigger?**

<details><summary>Answer</summary>

Row-level to change or check each row; statement-level with transition tables for set-based work.

</details>

**9. Trick question: you added a column to a table. Does `SELECT *` view show it?**

<details><summary>Answer</summary>

No. The column list is fixed when the view is created.

</details>

**10. Trick question: a function reads a table and is marked `IMMUTABLE`. What can happen?**

<details><summary>Answer</summary>

Results may be computed once and reused, so stale values appear.

</details>

**11. How do you build dynamic SQL safely?**

<details><summary>Answer</summary>

`format` with `%I` and `%L`, and `EXECUTE ... USING` for values.

</details>

**12. What does `CREATE OR REPLACE VIEW` allow?**

<details><summary>Answer</summary>

The same columns in the same order and types; new columns only at the end.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [PostgreSQL 17: CREATE VIEW](https://www.postgresql.org/docs/17/sql-createview.html)
- [PostgreSQL 17: CREATE FUNCTION](https://www.postgresql.org/docs/17/sql-createfunction.html)
- [PostgreSQL 17: trigger behavior](https://www.postgresql.org/docs/17/trigger-definition.html)
- [PostgreSQL 17: function volatility](https://www.postgresql.org/docs/17/xfunc-volatility.html)
