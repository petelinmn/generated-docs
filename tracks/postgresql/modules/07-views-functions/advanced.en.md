---
title: "Views and functions: advanced"
lang: en
track: postgresql
kind: module
doc: advanced
slug: 07-views-functions
part: 2
tags: [view, materialized-view, plpgsql, stored-procedure, trigger]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Views and functions: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Updatable views

A view with exactly one table (or updatable view) in `FROM` and without `DISTINCT`, `GROUP BY`, `HAVING`, `LIMIT`, `OFFSET`, `WITH`, set operations, aggregates, window or set-returning functions in the select list accepts `INSERT`, `UPDATE` and `DELETE`.

```sql
UPDATE active_products SET price = 3 WHERE id = 1;          -- changes products

INSERT INTO active_products VALUES (4, 'Ghost', 1, false);  -- accepted, but invisible in the view

CREATE OR REPLACE VIEW active_products AS
    SELECT id, name, price, active FROM products WHERE active
    WITH CHECK OPTION;
INSERT INTO active_products VALUES (5, 'Ghost2', 1, false);
-- ERROR:  new row violates check option for view "active_products"
```

`WITH CHECK OPTION` is `CASCADED` by default: conditions of underlying views are checked too; `LOCAL` checks only this view's condition. For other views write an `INSTEAD OF` trigger.

## Materialized views

```sql
CREATE MATERIALIZED VIEW totals AS
    SELECT active, sum(price) AS total FROM products GROUP BY active;

UPDATE products SET price = price + 100 WHERE id = 2;
SELECT * FROM totals ORDER BY active;   -- unchanged: the result is stored

REFRESH MATERIALIZED VIEW CONCURRENTLY totals;
-- ERROR:  cannot refresh materialized view "public.totals" concurrently
CREATE UNIQUE INDEX totals_active ON totals (active);
REFRESH MATERIALIZED VIEW CONCURRENTLY totals;
```

A plain `REFRESH` can block readers. `CONCURRENTLY` does not, but needs a unique index on plain columns covering all rows, and the view must already be populated.

## PL/pgSQL

```sql
CREATE FUNCTION get_price(p_id int) RETURNS numeric LANGUAGE plpgsql STABLE AS $$
DECLARE result numeric;
BEGIN
    SELECT price INTO STRICT result FROM products WHERE id = p_id;
    RETURN result;
EXCEPTION
    WHEN no_data_found THEN
        RAISE EXCEPTION 'product % not found', p_id USING ERRCODE = 'P0002';
END $$;

SELECT get_price(99);   -- ERROR:  product 99 not found
```

`INTO STRICT` raises an error unless exactly one row is returned; without it a missing row just leaves `NULL`. A block with `EXCEPTION` is a subtransaction and noticeably more expensive to enter and leave.

## Procedures

```sql
CREATE TABLE log (n int);
CREATE PROCEDURE fill(count int) LANGUAGE plpgsql AS $$
BEGIN
    FOR i IN 1..count LOOP
        INSERT INTO log VALUES (i);
        IF i % 2 = 0 THEN COMMIT; ELSE ROLLBACK; END IF;
    END LOOP;
END $$;

CALL fill(4);
SELECT array_agg(n ORDER BY n) FROM log;   -- {2,4}

BEGIN;
CALL fill(4);    -- ERROR:  invalid transaction termination
ROLLBACK;
SELECT fill(1);  -- ERROR:  fill(integer) is a procedure
```

A procedure may `COMMIT` and `ROLLBACK` when called at the top level. It cannot do so inside an explicit transaction block, and transaction control is not allowed inside a block with an exception handler.

## Triggers

```sql
CREATE TABLE price_history (product_id int, old_price numeric, new_price numeric);
ALTER TABLE products ADD COLUMN updated_at timestamptz;

CREATE FUNCTION touch() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN NEW.updated_at := now(); RETURN NEW; END $$;

CREATE FUNCTION audit_price() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO price_history VALUES (OLD.id, OLD.price, NEW.price);
    RETURN NULL;
END $$;

CREATE TRIGGER products_touch BEFORE UPDATE ON products
    FOR EACH ROW EXECUTE FUNCTION touch();
CREATE TRIGGER products_audit AFTER UPDATE OF price ON products
    FOR EACH ROW WHEN (OLD.price IS DISTINCT FROM NEW.price)
    EXECUTE FUNCTION audit_price();

UPDATE products SET price = price WHERE id = 1;   -- no history row
UPDATE products SET price = 4 WHERE id = 1;       -- one history row: 1 | 3.00 | 4.00
```

| Timing | Row trigger's return value |
|---|---|
| `BEFORE` | the row to use; `NULL` skips the operation for this row |
| `AFTER` | ignored |
| `INSTEAD OF` (views only, per row) | non-`NULL` means the row was handled |

A statement-level trigger runs once, even for zero rows, and can see all changed rows through a transition table:

```sql
CREATE FUNCTION count_changed() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN RAISE NOTICE 'changed rows: %', (SELECT count(*) FROM new_rows); RETURN NULL; END $$;

CREATE TRIGGER products_stmt AFTER UPDATE ON products
    REFERENCING NEW TABLE AS new_rows
    FOR EACH STATEMENT EXECUTE FUNCTION count_changed();

UPDATE products SET active = true;                -- NOTICE:  changed rows: 4
UPDATE products SET active = true WHERE false;    -- NOTICE:  changed rows: 0
```

## Dynamic SQL

```sql
CREATE FUNCTION count_rows(tbl text) RETURNS bigint LANGUAGE plpgsql AS $$
DECLARE n bigint;
BEGIN
    EXECUTE format('SELECT count(*) FROM %I', tbl) INTO n;   -- %I quotes an identifier
    RETURN n;
END $$;

SELECT count_rows('products; DROP TABLE products');
-- ERROR:  relation "products; DROP TABLE products" does not exist
SELECT format('%I | %L', 'a b', 'o''k');   -- "a b" | 'o''k'
```

`%I` quotes identifiers, `%L` quotes literals; for values prefer `EXECUTE ... USING`. Never concatenate user input into SQL text.

## Best practices

- Keep triggers small, without surprising side effects.
- Declare the correct volatility.
- Raise errors with an `ERRCODE` the application can recognise.
- Refresh materialized views on a schedule and monitor their age.

## Common mistakes

- ❌ Updating the same row from an `AFTER` trigger instead of changing `NEW` in `BEFORE`.
- ❌ Forgetting `RETURN` in a trigger or function → run-time error.
- ❌ A row trigger doing a query per row on bulk loads → a statement trigger with a transition table.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [PostgreSQL 17: CREATE VIEW](https://www.postgresql.org/docs/17/sql-createview.html)
- [PostgreSQL 17: CREATE FUNCTION](https://www.postgresql.org/docs/17/sql-createfunction.html)
- [PostgreSQL 17: trigger behavior](https://www.postgresql.org/docs/17/trigger-definition.html)
- [PostgreSQL 17: PL/pgSQL transaction management](https://www.postgresql.org/docs/17/plpgsql-transactions.html)
- [PostgreSQL 17: REFRESH MATERIALIZED VIEW](https://www.postgresql.org/docs/17/sql-refreshmaterializedview.html)
