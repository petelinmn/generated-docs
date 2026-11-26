---
title: "Индексы"
lang: ru
track: postgresql
kind: module
doc: overview
slug: 05-indexes
part: 2
levels: [1, 2, 3, 4]
tags: [index, b-tree, gin, brin, query-plan]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Индексы

🌐 **Русский** · [English](overview.en.md)

## Коротко

Индекс — это отдельная структура, которая позволяет PostgreSQL находить строки, не читая всю таблицу. Одни запросы он удешевляет в разы, а каждую запись делает чуть дороже и занимает место на диске. Использовать ли его, решает планировщик: хороший индекс повторяет форму настоящего запроса, а не просто названную колонку.

## Ключевые идеи

- B-tree — тип по умолчанию: равенство, диапазоны и порядок.
- Порядок колонок составного индекса решает, какие запросы он обслужит хорошо.
- Для большой доли таблицы последовательное чтение дешевле индекса.
- Частичные индексы и индексы по выражению покрывают ровно то, что спрашивает запрос.
- Index-only scan не читает таблицу, только пока это позволяет карта видимости.
- GIN ищет «содержит» в `jsonb` и массивах, BRIN крошечный для естественно упорядоченных данных.
- `CREATE INDEX CONCURRENTLY` не останавливает запись, но может оставить недействительный индекс.

## Минимальный пример

```sql
CREATE TABLE events (
    id int PRIMARY KEY, user_id int NOT NULL, status text NOT NULL,
    email text NOT NULL, created_at timestamptz NOT NULL, payload jsonb NOT NULL
);
INSERT INTO events
SELECT i, i % 1000,
       CASE WHEN i % 100 = 0 THEN 'failed' ELSE 'done' END,
       'User' || i || '@example.com',
       timestamptz '2026-01-01 00:00+00' + i * interval '1 minute',
       jsonb_build_object('kind', 'k' || (i % 50))
FROM generate_series(1, 100000) i;
VACUUM ANALYZE events;
SET max_parallel_workers_per_gather = 0;   -- simpler plans for the examples
```

```sql
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE user_id = 42;
-- Seq Scan on events
--   Filter: (user_id = 42)

CREATE INDEX events_user_created ON events (user_id, created_at);

EXPLAIN (COSTS OFF) SELECT * FROM events WHERE user_id = 42 ORDER BY created_at DESC LIMIT 10;
-- Limit
--   ->  Index Scan Backward using events_user_created on events
--         Index Cond: (user_id = 42)
```

## Когда применять, а когда нет

- ✅ Частый запрос читает малую часть большой таблицы.
- ✅ Сортировка с `LIMIT`, которую индекс отдаёт уже по порядку.
- ❌ Маленькая таблица или запрос, возвращающий большинство строк.
- ❌ Индекс на каждую колонку «на всякий случай».

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | B-tree, составные индексы, чтение плана, селективность |
| [2. Продвинутая теория](advanced.ru.md) | частичные, по выражению, покрывающие, GIN, BRIN, построение без блокировки |
| [3. Экспертная теория](expert.ru.md) | карта видимости, HOT, справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn postgresql 05-indexes level 5` |
| 6. Тесты | ещё не создан — `/learn postgresql 05-indexes level 6` |

## Связанные материалы

- Пререквизиты: [02 · SQL-запросы](../02-sql-queries/overview.ru.md), [04 · Соединения и подзапросы](../04-joins-and-subqueries/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте PostgreSQL](../../overview.ru.md)
- Почему индексы влияют на проект: [хранение данных](../../../system-design/modules/04-data-storage/overview.ru.md)
- Из .NET: [EF Core](../../../csharp/modules/15-ef-core/overview.ru.md)
- Другая структура индекса: [анализ текста в OpenSearch](../../../opensearch/modules/02-mappings-and-analysis/overview.ru.md)
- [06 · Транзакции](../06-transactions/overview.ru.md)
- [08 · JSON и операторы](../08-json-and-operators/overview.ru.md)
- [09 · Производительность и EXPLAIN](../09-performance-explain/overview.ru.md)
