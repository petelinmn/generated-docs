---
title: "Представления и функции: базовая теория"
lang: ru
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

# Представления и функции: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** `SELECT`, соединения · **Время:** ~2 ч

## Данные для примеров

Все результаты получены на PostgreSQL 17.11.

```sql
CREATE TABLE products (
    id int PRIMARY KEY, name text NOT NULL,
    price numeric(10,2) NOT NULL, active boolean NOT NULL DEFAULT true
);
INSERT INTO products VALUES (1,'Pen',2,true), (2,'Book',15,true), (3,'Lamp',30,false);
```

## Представления

```sql
CREATE VIEW active_products AS
    SELECT id, name, price, active FROM products WHERE active;

SELECT id, name FROM active_products ORDER BY id;   -- 1 Pen | 2 Book
```

Представление — не копия: каждый запрос к нему выполняет базовый запрос на текущих данных. Оно даёт запросу имя и место, где можно ограничить колонки и строки.

## SQL-функции

```sql
CREATE FUNCTION price_with_tax(price numeric, rate numeric DEFAULT 0.2) RETURNS numeric
LANGUAGE sql IMMUTABLE
RETURN round(price * (1 + rate), 2);

SELECT price_with_tax(10), price_with_tax(10, rate => 0.1);   -- 12.00 | 11.00
```

У параметров бывают значения по умолчанию, их можно передавать по имени через `=>`. `IMMUTABLE` говорит, что результат зависит только от аргументов.

## Функции, возвращающие строки

```sql
CREATE FUNCTION cheaper_than(limit_price numeric) RETURNS TABLE (id int, price numeric)
LANGUAGE sql STABLE AS $$
    SELECT id, price FROM products WHERE price < limit_price
$$;

SELECT * FROM cheaper_than(20) ORDER BY id;
```

Табличную функцию используют в `FROM`, и она соединяется как таблица.

## Долларовые кавычки

Тело функции — это строка. `$$ ... $$` позволяет писать внутри кавычки без экранирования. Более новые формы `RETURN expr` и `BEGIN ATOMIC ... END` разбираются при создании функции.

## Ошибки новичка

- ❌ Ждать, что представление ускорит запрос → ✅ выполняется тот же запрос.
- ❌ `SELECT *` в представлении → ✅ перечислять колонки.
- ❌ Называть параметр как колонку → ✅ префикс вроде `p_`.
- ❌ Логика в функциях, которую никто не хранит в системе контроля версий → ✅ это код, с миграциями.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [PostgreSQL 17: CREATE VIEW](https://www.postgresql.org/docs/17/sql-createview.html)
- [PostgreSQL 17: CREATE FUNCTION](https://www.postgresql.org/docs/17/sql-createfunction.html)
- [PostgreSQL 17: поведение триггеров](https://www.postgresql.org/docs/17/trigger-definition.html)
