---
title: "Представления и функции: продвинутая теория"
lang: ru
track: postgresql
kind: module
doc: advanced
slug: 07-views-functions
part: 2
tags: [view, materialized-view, plpgsql, stored-procedure, trigger]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Представления и функции: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Изменяемые представления

Примеры продолжают [базовую теорию](basics.ru.md): таблица `products` и представление `active_products`.

Представление ровно с одной таблицей (или изменяемым представлением) в `FROM` и без `DISTINCT`, `GROUP BY`, `HAVING`, `LIMIT`, `OFFSET`, `WITH`, операций над множествами, агрегатов, оконных и возвращающих наборы функций в списке выборки принимает `INSERT`, `UPDATE` и `DELETE`.

```sql
UPDATE active_products SET price = 3 WHERE id = 1;          -- changes products

INSERT INTO active_products VALUES (4, 'Ghost', 1, false);  -- accepted, but invisible in the view

CREATE OR REPLACE VIEW active_products AS
    SELECT id, name, price, active FROM products WHERE active
    WITH CHECK OPTION;
INSERT INTO active_products VALUES (5, 'Ghost2', 1, false);
-- ERROR:  new row violates check option for view "active_products"
```

`WITH CHECK OPTION` по умолчанию `CASCADED`: проверяются и условия нижележащих представлений; `LOCAL` проверяет только условие этого представления. Для остальных представлений пишут триггер `INSTEAD OF`.

## Материализованные представления

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

Обычный `REFRESH` может блокировать читателей. `CONCURRENTLY` не блокирует, но требует уникального индекса по простым колонкам, охватывающего все строки, и уже заполненного представления.

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

`INTO STRICT` выдаёт ошибку, если строк не ровно одна; без него отсутствие строки просто оставит `NULL`. Блок с `EXCEPTION` — это подтранзакция, вход и выход из него заметно дороже.

## Процедуры

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

Процедура может делать `COMMIT` и `ROLLBACK`, когда вызвана на верхнем уровне; так же может анонимный блок `DO`. Внутри явного блока транзакции — не может, и управление транзакциями запрещено внутри блока с обработчиком исключений.

## Триггеры

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

| Момент | Возвращаемое значение строкового триггера |
|---|---|
| `BEFORE` | строка, которую использовать; `NULL` пропускает операцию для этой строки |
| `AFTER` | игнорируется |
| `INSTEAD OF` (только представления, на строку) | не `NULL` означает, что строка обработана |

Триггер уровня оператора выполняется один раз, даже при нуле строк, и видит все изменённые строки через переходную таблицу (transition table):

```sql
CREATE FUNCTION count_changed() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN RAISE NOTICE 'changed rows: %', (SELECT count(*) FROM new_rows); RETURN NULL; END $$;

CREATE TRIGGER products_stmt AFTER UPDATE ON products
    REFERENCING NEW TABLE AS new_rows
    FOR EACH STATEMENT EXECUTE FUNCTION count_changed();

UPDATE products SET active = true;                -- NOTICE:  changed rows: 4
UPDATE products SET active = true WHERE false;    -- NOTICE:  changed rows: 0
```

## Динамический SQL

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

`%I` экранирует идентификаторы, `%L` — литералы; для значений предпочитайте `EXECUTE ... USING`. Никогда не склеивайте пользовательский ввод с текстом SQL.

## Лучшие практики

- Держите триггеры маленькими, без неожиданных побочных эффектов.
- Объявляйте правильную изменчивость.
- Выдавайте ошибки с `ERRCODE`, который приложение сможет распознать.
- Обновляйте материализованные представления по расписанию и следите за их возрастом.

## Частые ошибки

- ❌ Обновлять ту же строку из `AFTER`-триггера вместо изменения `NEW` в `BEFORE`.
- ❌ Забыть `RETURN` в триггере или функции → ошибка при выполнении.
- ❌ Строковый триггер с запросом на каждую строку при массовой загрузке → триггер уровня оператора с переходной таблицей.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [PostgreSQL 17: CREATE VIEW](https://www.postgresql.org/docs/17/sql-createview.html)
- [PostgreSQL 17: CREATE FUNCTION](https://www.postgresql.org/docs/17/sql-createfunction.html)
- [PostgreSQL 17: поведение триггеров](https://www.postgresql.org/docs/17/trigger-definition.html)
- [PostgreSQL 17: управление транзакциями в PL/pgSQL](https://www.postgresql.org/docs/17/plpgsql-transactions.html)
- [PostgreSQL 17: REFRESH MATERIALIZED VIEW](https://www.postgresql.org/docs/17/sql-refreshmaterializedview.html)
