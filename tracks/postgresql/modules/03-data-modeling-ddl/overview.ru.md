---
title: "Проектирование схемы и DDL"
lang: ru
track: postgresql
kind: module
doc: overview
slug: 03-data-modeling-ddl
part: 1
levels: [1, 2, 3, 4]
tags: [ddl, data-types, primary-key, foreign-key, constraints, normalization]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Проектирование схемы и DDL

🌐 **Русский** · [English](overview.en.md)

## Коротко

DDL описывает структуру базы: таблицы, столбцы, типы и ограничения. Хорошая схема связывает сущности первичными и внешними ключами, не допускает некорректные значения и представляет отношения явно.

## Ключевые идеи

- Типы выбираются для семантики и диапазона значений.
- Первичный ключ однозначно идентифицирует строку.
- Внешний ключ проверяет ссылочную целостность.
- `NOT NULL`, `UNIQUE`, `CHECK` дополняют связи.
- Нормализация уменьшает дублирование и зависит от сценариев чтения.
- Изменение схемы лучше выполнять миграциями.

## Минимальный пример

```sql
CREATE TABLE customers (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email text NOT NULL UNIQUE
);

CREATE TABLE orders (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    customer_id bigint NOT NULL REFERENCES customers(id),
    amount numeric(12,2) NOT NULL CHECK (amount >= 0)
);
```

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | типы, DDL, ключи, ограничения |
| [2. Продвинутая теория](advanced.ru.md) | нормализация, enums, generated columns, миграции |
| [3. Экспертная теория](expert.ru.md) | справочник отношений, catalog dependencies, pitfalls |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn postgresql 03-data-modeling-ddl level 5` |
| 6. Тесты | ещё не создан — `/learn postgresql 03-data-modeling-ddl level 6` |

## Связанные материалы

- Пререквизиты: [02 · SQL-запросы](../02-sql-queries/overview.ru.md).
- Дальше: следующий модуль по [дорожной карте PostgreSQL](../../overview.ru.md).
- [System design: выбор хранилища](../../../system-design/modules/04-data-storage/overview.ru.md)
- [04 · Соединения и подзапросы](../04-joins-and-subqueries/overview.ru.md)

## Источники

- [PostgreSQL data types](https://www.postgresql.org/docs/current/datatype.html)
- [PostgreSQL constraints](https://www.postgresql.org/docs/current/ddl-constraints.html)
- [PostgreSQL table basics](https://www.postgresql.org/docs/current/ddl-basics.html)
