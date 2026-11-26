---
title: "JSON и операторы: базовая теория"
lang: ru
track: postgresql
kind: module
doc: basics
slug: 08-json-and-operators
part: 2
tags: [jsonb, json, jsonpath, gin, operators]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# JSON и операторы: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** `SELECT`, `WHERE` · **Время:** ~2 ч

## Данные для примеров

Все результаты получены на PostgreSQL 17.11. Каждый пример начинается с этих данных. Обратите внимание: в документе 3 цена записана строкой.

```sql
CREATE TABLE docs (id int PRIMARY KEY, body jsonb NOT NULL);
INSERT INTO docs VALUES
 (1, '{"name":"Pen","price":2,"tags":["office","cheap"],"stock":{"msk":5,"spb":0}}'),
 (2, '{"name":"Book","price":15,"tags":["read"],"stock":{"msk":0}}'),
 (3, '{"name":"Lamp","price":"30","tags":[],"discount":null}');
```

## json или jsonb

```sql
SELECT '{"b":1,  "a":2, "b":3}'::json  AS j,    -- {"b":1,  "a":2, "b":3}
       '{"b":1,  "a":2, "b":3}'::jsonb AS jb;   -- {"a": 2, "b": 3}
```

| | `json` | `jsonb` |
|---|---|---|
| Хранится как | входной текст | двоичная структура |
| Пробелы, порядок ключей, дубли ключей | сохраняются | не сохраняются; побеждает последний дубль |
| Обработка | разбирается каждый раз | без повторного разбора |
| Индексация содержимого | нет операторов GIN | GIN |

Берите `jsonb`, если не нужно сохранить текст в точности как получен.

## Как достать значения

```sql
SELECT body -> 'name'          AS j,       -- "Pen"    (jsonb)
       body ->> 'name'         AS t,       -- Pen      (text)
       body #>> '{tags,0}'     AS first,   -- office
       body -> 'tags' -> -1    AS last,    -- "cheap"
       body['stock']['msk']    AS msk,     -- 5
       body -> 'nope'          AS missing  -- NULL
FROM docs WHERE id = 1;
```

`->` оставляет результат в `jsonb`, поэтому можно спускаться дальше; `->>` заканчивается на `text`. `#>` и `#>>` принимают путь.

## Поиск

```sql
SELECT id FROM docs WHERE body @> '{"tags":["cheap"]}';   -- 1
SELECT id FROM docs WHERE body ? 'discount';              -- 3
SELECT id FROM docs WHERE body -> 'tags' ? 'read';        -- 2
```

## Ошибки новичка

- ❌ `body -> 'name' = 'Pen'` → ✅ `body ->> 'name' = 'Pen'` для сравнения текста.
- ❌ Сравнивать числа как текст → ✅ приводить: `(body ->> 'price')::numeric`.
- ❌ Складывать всё в одну колонку `jsonb` → ✅ для устойчивых важных полей — колонки.
- ❌ Ждать сохранения порядка ключей в `jsonb` → ✅ он не сохраняется.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [PostgreSQL 17: типы JSON](https://www.postgresql.org/docs/17/datatype-json.html)
- [PostgreSQL 17: функции и операторы JSON](https://www.postgresql.org/docs/17/functions-json.html)
