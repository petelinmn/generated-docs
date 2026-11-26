---
title: "Представления и функции: экспертная теория"
lang: ru
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

# Представления и функции: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Представление хранится как правило переписывания: когда запрос называет его, подставляется запрос представления и планируется вместе с внешним. Поэтому условия обычно доходят до базовых таблиц и их индексов. Так же может встраиваться простая SQL-функция; функции PL/pgSQL выполняются как есть, и их внутренности внешнему плану не видны.

### Изменчивость

Категория говорит планировщику, что можно предполагать. По умолчанию — `VOLATILE`. Функция `STABLE` или `IMMUTABLE` не может изменять базу. Вызов `IMMUTABLE`-функции с постоянными аргументами может быть вычислен во время планирования; в индексных выражениях допустимы только `IMMUTABLE`-функции.

### Права

По умолчанию представление обращается к таблицам с правами своего владельца, а с `security_invoker = true` — с правами того, кто его запрашивает. `security_barrier` нужен представлениям, скрывающим строки: он не даёт пользовательским функциям увидеть отфильтрованные строки. Функции по умолчанию `SECURITY INVOKER`, а `EXECUTE` на новую функцию выдаётся `PUBLIC`.

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

Для `SECURITY DEFINER` задавайте `search_path` из схем, куда недоверенные пользователи не могут писать, с `pg_temp` в конце, и делайте `REVOKE EXECUTE ... FROM PUBLIC` перед выдачей права тем, кому оно нужно.

## Справочник

| Атрибут `CREATE FUNCTION` | По умолчанию | Смысл |
|---|---|---|
| `VOLATILE` / `STABLE` / `IMMUTABLE` | `VOLATILE` | изменчивость |
| `CALLED ON NULL INPUT` / `STRICT` | вызывается | поведение при `NULL` в аргументах |
| `SECURITY INVOKER` / `DEFINER` | invoker | чьи права |
| `PARALLEL UNSAFE` / `RESTRICTED` / `SAFE` | unsafe | использование в параллельных планах |
| `SET parameter = value` | — | настройка на время вызова |
| `COST`, `ROWS` | — | оценки для планировщика |

| Что возвращает | Синтаксис |
|---|---|
| одно значение | `RETURNS type` |
| набор одного типа | `RETURNS SETOF type` |
| набор строк | `RETURNS TABLE (col type, ...)` |
| через параметры | `OUT` / `INOUT` |

| Переменная триггера | Содержимое |
|---|---|
| `NEW`, `OLD` | новая и старая строка в строковых триггерах |
| `TG_OP` | `INSERT`, `UPDATE`, `DELETE`, `TRUNCATE` |
| `TG_NAME`, `TG_TABLE_NAME` | имена триггера и таблицы |
| `TG_WHEN`, `TG_LEVEL` | момент и уровень |
| `FOUND` | затронул ли строку последний оператор (в любом коде PL/pgSQL) |

| Параметр представления | Действие |
|---|---|
| `WITH [CASCADED \| LOCAL] CHECK OPTION` | записанное должно оставаться видимым |
| `security_invoker` | проверять права вызывающего |
| `security_barrier` | для представлений, скрывающих строки |

## Подвохи

### Список колонок представления заморожен

```sql
CREATE VIEW all_products AS SELECT * FROM products;
ALTER TABLE products ADD COLUMN sku text;
SELECT * FROM all_products WHERE id = 1;      -- no sku column: * was expanded at creation
ALTER TABLE products DROP COLUMN name;
-- ERROR:  cannot drop column name of table products because other objects depend on it
```

### Ложный IMMUTABLE даёт устаревшие результаты

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

Подготовленный запрос сохраняет значение, вычисленное раньше.

### Индекс может принять функцию, которую вы не пометили

```sql
CREATE TABLE w (sku text);
CREATE FUNCTION lower_sql(t text) RETURNS text LANGUAGE sql AS $$ SELECT lower(t) $$;
CREATE INDEX ix_sql ON w (lower_sql(sku));   -- accepted: the body is inlined and checked

CREATE FUNCTION lower_pl(t text) RETURNS text LANGUAGE plpgsql AS $$ BEGIN RETURN lower(t); END $$;
CREATE INDEX ix_pl ON w (lower_pl(sku));
-- ERROR:  functions in index expression must be marked IMMUTABLE
```

Первый индекс работает, только пока тело остаётся встраиваемым и неизменяемым; объявляйте ту изменчивость, которую имеете в виду.

### STRICT молча пропускает тело

```sql
CREATE FUNCTION f_strict(a int) RETURNS int LANGUAGE plpgsql STRICT AS
$$ BEGIN RAISE NOTICE 'called'; RETURN 1; END $$;
CREATE FUNCTION f_loose(a int) RETURNS int LANGUAGE plpgsql AS
$$ BEGIN RAISE NOTICE 'called'; RETURN 1; END $$;

-- both bodies raise a notice and return 1; only f_strict is declared STRICT
SELECT f_strict(NULL), f_loose(NULL);   -- NULL | 1, one notice only
```

## Компромиссы и альтернативы

| Выбор | Плюс | Минус |
|---|---|---|
| Логика в базе | одно правило для всех клиентов, без лишних обращений | труднее тестировать, версионировать и масштабировать |
| Триггеры | код приложения не может их обойти | скрытое поведение, медленнее массовые изменения |
| Ограничения вместо триггеров | декларативно, корректно при параллельности | ограниченная выразительность |
| Вычисляемые колонки | простые производные значения | только из той же строки |
| Материализованное представление | быстрое чтение тяжёлых агрегатов | устаревает до обновления |
| Сводная таблица, поддерживаемая кодом | инкрементальные обновления | корректность на вас |

## Вопросы с собеседований

**1. Чем представление отличается от материализованного?**

<details><summary>Ответ</summary>

Представление хранит запрос; материализованное — его результат, который нужно обновлять.

</details>

**2. Какие представления обновляемы автоматически?**

<details><summary>Ответ</summary>

Над одной таблицей или обновляемым представлением без группировок, distinct, лимитов, операций над множествами, агрегатов и оконных функций.

</details>

**3. Что означают категории изменчивости?**

<details><summary>Ответ</summary>

`VOLATILE` (по умолчанию) может меняться при каждом вызове; `STABLE` возвращает то же для тех же аргументов в пределах оператора; `IMMUTABLE` — всегда.

</details>

**4. Функция или процедура?**

<details><summary>Ответ</summary>

Функция используется в выражениях и возвращает значение; процедуру запускают через `CALL`, и она может управлять транзакциями.

</details>

**5. Что делает `STRICT`?**

<details><summary>Ответ</summary>

Функция не вызывается при аргументе `NULL`; результат — `NULL`.

</details>

**6. Чем опасен `SECURITY DEFINER`?**

<details><summary>Ответ</summary>

Он выполняется с правами владельца. Нужно зафиксировать `search_path` с `pg_temp` в конце и отозвать `EXECUTE` у `PUBLIC`.

</details>

**7. В каком порядке срабатывают триггеры одного вида?**

<details><summary>Ответ</summary>

По алфавиту имён.

</details>

**8. Триггер на строку или на оператор?**

<details><summary>Ответ</summary>

На строку — чтобы менять или проверять каждую; на оператор с переходными таблицами — для обработки набором.

</details>

**9. Подвох: добавили колонку в таблицу. Покажет ли её представление с `SELECT *`?**

<details><summary>Ответ</summary>

Нет. Список колонок фиксируется при создании представления.

</details>

**10. Подвох: функция читает таблицу и помечена `IMMUTABLE`. Что может произойти?**

<details><summary>Ответ</summary>

Результат может быть вычислен один раз и переиспользован: появятся устаревшие значения.

</details>

**11. Как безопасно строить динамический SQL?**

<details><summary>Ответ</summary>

`format` с `%I` и `%L` и `EXECUTE ... USING` для значений.

</details>

**12. Что позволяет `CREATE OR REPLACE VIEW`?**

<details><summary>Ответ</summary>

Те же колонки в том же порядке и с теми же типами; новые — только в конец.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [PostgreSQL 17: CREATE VIEW](https://www.postgresql.org/docs/17/sql-createview.html)
- [PostgreSQL 17: CREATE FUNCTION](https://www.postgresql.org/docs/17/sql-createfunction.html)
- [PostgreSQL 17: поведение триггеров](https://www.postgresql.org/docs/17/trigger-definition.html)
- [PostgreSQL 17: изменчивость функций](https://www.postgresql.org/docs/17/xfunc-volatility.html)
