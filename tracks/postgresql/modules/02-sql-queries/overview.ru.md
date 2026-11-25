---
title: "SQL-запросы"
lang: ru
track: postgresql
kind: module
doc: overview
slug: 02-sql-queries
part: 1
levels: [1, 2, 3, 4]
tags: [sql, select, where, group-by, having, order-by, limit, aggregates]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# SQL-запросы

🌐 **Русский** · [English](overview.en.md)

## Коротко

`SELECT` читает строки и вычисляет выражения; `FROM` выбирает источники, `WHERE` фильтрует до агрегации, `GROUP BY` объединяет строки, `HAVING` фильтрует группы, а `ORDER BY` задаёт порядок вывода. PostgreSQL обрабатывает запрос логическим планом и формирует результат в виде таблицы строк.

## Ключевые идеи

- SELECT может вычислять выражения, функции и агрегаты.
- `WHERE` не принимает результаты агрегатов.
- `HAVING` фильтрует сгруппированные группы.
- `ORDER BY` нужен для заданного порядка результата.
- `LIMIT` и `OFFSET` ограничивают вывод, а keyset-пагинация часто лучше offset на больших наборах.
- Порядок таблицы не гарантируется без `ORDER BY`.

## Минимальный пример

```sql
CREATE TABLE orders (id integer, customer text, amount numeric);
INSERT INTO orders VALUES (1, 'Ada', 25), (2, 'Ada', 40), (3, 'Linus', 10);

SELECT customer, sum(amount) AS total
FROM orders
GROUP BY customer
HAVING sum(amount) >= 30
ORDER BY total DESC;
```

```text
 customer | total
----------+-------
 Ada      |    65
```

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | SELECT, WHERE, GROUP BY, HAVING, ORDER BY, LIMIT |
| [2. Продвинутая теория](advanced.ru.md) | DISTINCT, NULL, CASE, set operations, window functions |
| [3. Экспертная теория](expert.ru.md) | логический порядок, планировщик, типы выражений, подвохи |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn postgresql 02-sql-queries level 5` |
| 6. Тесты | ещё не создан — `/learn postgresql 02-sql-queries level 6` |

## Связанные материалы

- Пререквизиты: [01 · Основы PostgreSQL](../01-postgresql-basics/overview.ru.md).
- Дальше: [03 · Проектирование схемы](../03-data-modeling-ddl/overview.ru.md).

## Источники

- [PostgreSQL SELECT](https://www.postgresql.org/docs/current/sql-select.html)
- [PostgreSQL queries](https://www.postgresql.org/docs/current/queries.html)
- [PostgreSQL aggregate functions](https://www.postgresql.org/docs/current/functions-aggregate.html)
