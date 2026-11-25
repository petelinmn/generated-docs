---
title: "Проектирование схемы и DDL: базовая теория"
lang: ru
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

# Проектирование схемы и DDL: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## CREATE TABLE

```sql
CREATE TABLE products (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    sku text NOT NULL UNIQUE,
    price numeric(12,2) NOT NULL CHECK (price >= 0)
);
```

## Связь один ко многим

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

Каждый заказ ссылается на существующего клиента.

## ALTER и DROP

```sql
ALTER TABLE products ADD COLUMN active boolean NOT NULL DEFAULT true;
ALTER TABLE products DROP COLUMN active;
DROP TABLE products;
```

В production сначала проверяют миграцию и совместимость приложения.

## Ошибки новичка

- ❌ Использовать text для строгого числа → ✅ numeric или integer.
- ❌ Создать внешний ключ на nullable-столбец без необходимости → ✅ NOT NULL.
- ❌ Хранить email без UNIQUE → ✅ ограничить дубли.
- ❌ Удалять таблицу до резервного копирования → ✅ проверить зависимости.

## Источники

- [PostgreSQL data types](https://www.postgresql.org/docs/current/datatype.html)
- [PostgreSQL constraints](https://www.postgresql.org/docs/current/ddl-constraints.html)
