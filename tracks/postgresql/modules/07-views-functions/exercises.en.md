---
title: "Exercises: views and functions"
lang: en
track: postgresql
kind: module
doc: exercises
slug: 07-views-functions
part: 2
tags: [view, materialized-view, plpgsql, stored-procedure, trigger]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Exercises: views and functions

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Use the data from [basics](basics.en.md).

## 1. 🟢 A safe view

**Task:** a view of products cheaper than 10, through which nobody can create or leave a more expensive product, checked with the caller's rights.

<details><summary>Hint</summary>

Two things: a check option and a view option.

</details>

<details><summary>Solution</summary>

```sql
CREATE VIEW cheap_products WITH (security_invoker = true) AS
    SELECT id, name, price, active FROM products WHERE price < 10
    WITH CHECK OPTION;
```

An update that raises the price to 10 or more is rejected too.

</details>

## 2. 🟡 A function that does not fail silently

**Task:** `set_price(id, price)` must raise an error for a missing product.

<details><summary>Hint</summary>

PL/pgSQL knows whether the last statement touched a row.

</details>

<details><summary>Solution</summary>

```sql
CREATE FUNCTION set_price(p_id int, p_price numeric) RETURNS void LANGUAGE plpgsql AS $$
BEGIN
    UPDATE products SET price = p_price WHERE id = p_id;
    IF NOT FOUND THEN
        RAISE EXCEPTION 'product % not found', p_id USING ERRCODE = 'P0002';
    END IF;
END $$;
```

An `UPDATE` of zero rows is not an error by itself.

</details>

## 3. 🔴 Guarding price cuts

**Task:** forbid reducing a price by more than half in one update.

<details><summary>Hint</summary>

You need both `OLD` and `NEW`, before the row is written.

</details>

<details><summary>Solution</summary>

```sql
CREATE FUNCTION forbid_cheaper() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF NEW.price < OLD.price / 2 THEN
        RAISE EXCEPTION 'price of % cannot drop by more than half', OLD.id;
    END IF;
    RETURN NEW;
END $$;
CREATE TRIGGER products_guard BEFORE UPDATE OF price ON products
    FOR EACH ROW EXECUTE FUNCTION forbid_cheaper();
```

A `CHECK` constraint cannot express this: it does not see the old value.

</details>
