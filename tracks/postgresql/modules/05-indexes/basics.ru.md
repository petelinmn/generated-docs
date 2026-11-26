---
title: "Индексы: базовая теория"
lang: ru
track: postgresql
kind: module
doc: basics
slug: 05-indexes
part: 2
tags: [index, b-tree, gin, brin, query-plan]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Индексы: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** `SELECT`, `WHERE`, `ORDER BY` · **Время:** ~2 ч

## Данные для примеров

Все планы модуля получены на PostgreSQL 17.11 с этой таблицей. Планы зависят от данных и статистики, поэтому на других данных они могут отличаться.

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

## Первый индекс

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

`Seq Scan` читает всю таблицу и отбрасывает неподходящее. `Index Scan` идёт к нужной части индекса и достаёт строки из таблицы. Индекс упорядочен по `(user_id, created_at)`, поэтому строки одного пользователя уже расположены по времени и читаются в обратную сторону: шага сортировки нет.

## Что индексируется само

Ограничения `PRIMARY KEY` и `UNIQUE` создают уникальные B-tree-индексы. Внешний ключ **не** создаёт индекс по ссылающейся колонке.

## Составные индексы

`(user_id, created_at)` упорядочен по первой колонке, а внутри неё по второй. Он обслуживает:

| Условие запроса | Насколько хорошо |
|---|---|
| `user_id = ?` | хорошо |
| `user_id = ? AND created_at > ?` | хорошо |
| `user_id = ? ORDER BY created_at` | хорошо, без сортировки |
| только `created_at = ?` | плохо: нет первой колонки, чтобы сузить поиск |

## Селективность

```sql
CREATE INDEX events_status ON events (status);
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE status = 'done';    -- Seq Scan: 99% of rows
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE status = 'failed';  -- Index Scan: 1% of rows
```

Достать почти все строки через индекс дороже, чем один раз прочитать таблицу, поэтому планировщик индекс игнорирует.

## Как читать план

| Узел | Смысл |
|---|---|
| `Seq Scan` | читается вся таблица |
| `Index Scan` | индекс, затем таблица для каждого совпадения |
| `Index Only Scan` | отвечает один индекс, таблица — только когда видимость неизвестна |
| `Bitmap Index Scan` + `Bitmap Heap Scan` | сначала собрать места совпадений, потом читать страницы таблицы по порядку |

`EXPLAIN` показывает план, `EXPLAIN ANALYZE` ещё и выполняет запрос и показывает фактические строки. Осторожно: `ANALYZE` по-настоящему выполняет `UPDATE` и `DELETE`.

## Ошибки новичка

- ❌ Считать, что индекс используется всегда → ✅ проверять через `EXPLAIN`.
- ❌ Два индекса по одной колонке там, где запросу нужен один составной → ✅ строить индекс по форме запроса.
- ❌ Забыть колонку внешнего ключа → ✅ индексировать, если по ней соединяют или удаляют.
- ❌ Проверять на десяти строках → ✅ планы на крошечных таблицах ничего не говорят.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [PostgreSQL 17: типы индексов](https://www.postgresql.org/docs/17/indexes-types.html)
- [PostgreSQL 17: составные индексы](https://www.postgresql.org/docs/17/indexes-multicolumn.html)
- [PostgreSQL 17: index-only scans](https://www.postgresql.org/docs/17/indexes-index-only-scans.html)
- [PostgreSQL 17: CREATE INDEX](https://www.postgresql.org/docs/17/sql-createindex.html)
