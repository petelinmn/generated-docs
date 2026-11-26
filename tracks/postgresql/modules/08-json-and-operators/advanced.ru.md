---
title: "JSON и операторы: продвинутая теория"
lang: ru
track: postgresql
kind: module
doc: advanced
slug: 08-json-and-operators
part: 2
tags: [jsonb, json, jsonpath, gin, operators]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# JSON и операторы: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Типы внутри документов

```sql
SELECT id, body ->> 'price' FROM docs ORDER BY body ->> 'price';
-- 2 '15' | 1 '2' | 3 '30'    (text order)

SELECT id, (body ->> 'price')::numeric FROM docs ORDER BY (body ->> 'price')::numeric;
-- 1 2 | 2 15 | 3 30

SELECT id FROM docs WHERE body @> '{"price":30}';     -- no rows
SELECT id FROM docs WHERE body @> '{"price":"30"}';   -- 3
```

`30` и `"30"` — разные значения. База не навязывает тип внутри документа, пока вы не добавите ограничение.

## Правила вхождения

```sql
SELECT '[1,2,3]'::jsonb   @> '[3,1]',          -- t: order does not matter
       '[1,[2,3]]'::jsonb @> '[2]',            -- f: nesting does
       '{"a":{"b":1}}'::jsonb @> '{"b":1}';    -- f: only from the top level

SELECT id FROM docs WHERE body ?| array['discount','stock'];  -- 1, 2, 3: any key
SELECT id FROM docs WHERE body ?& array['discount','stock'];  -- no rows: all keys
```

## Изменение

```sql
SELECT jsonb_set(body, '{stock,spb}', '7') -> 'stock' FROM docs WHERE id = 1;
-- {"msk": 5, "spb": 7}

SELECT jsonb_set('{}'::jsonb, '{a,b}', '1');                    -- {}   (no parent, nothing done)
SELECT '{"a":{"x":1,"y":2}}'::jsonb || '{"a":{"z":3}}';         -- {"a": {"z": 3}}
SELECT '{"a":1,"b":2}'::jsonb - 'a';                            -- {"b": 2}
SELECT '{"a":{"b":1,"c":2}}'::jsonb #- '{a,b}';                 -- {"a": {"c": 2}}
SELECT jsonb_insert('[1,3]', '{1}', '2');                       -- [1, 2, 3]

UPDATE docs SET body['stock']['spb'] = '9' WHERE id = 2;        -- subscript assignment
```

`||` сливает только верхний уровень: вложенный объект заменяется, а не объединяется. Каждое изменение создаёт новый документ и новую версию строки.

## Построение документов из строк

```sql
SELECT jsonb_build_object('id', 1, 'tags', jsonb_build_array('a','b'));
-- {"id": 1, "tags": ["a", "b"]}

SELECT jsonb_agg(body ->> 'name' ORDER BY id) FROM docs;
-- ["Pen", "Book", "Lamp"]

SELECT jsonb_object_agg(body ->> 'name', body -> 'price' ORDER BY id) FROM docs;
-- {"Pen": 2, "Book": 15, "Lamp": "30"}
```

## Разворачивание документов в строки

```sql
SELECT d.id, t.tag
FROM docs d
CROSS JOIN LATERAL jsonb_array_elements_text(d.body -> 'tags') AS t(tag)
ORDER BY 1, 2;
-- 1 cheap | 1 office | 2 read      (document 3 has no tags and disappears)

SELECT * FROM jsonb_to_record('{"name":"Pen","price":2}') AS x(name text, price int);
```

`LEFT JOIN LATERAL ... ON true` сохраняет документы с пустым массивом.

## Индексы

```sql
CREATE INDEX docs_gin ON docs USING gin (body);                  -- jsonb_ops
-- body @> '{"name":"Pen"}'      Bitmap Index Scan on docs_gin
-- body ? 'discount'             Bitmap Index Scan on docs_gin
-- body -> 'tags' ? 'read'       Seq Scan: the index is on body, not on body -> 'tags'

CREATE INDEX docs_gin_path ON docs USING gin (body jsonb_path_ops);
-- body @> '{"name":"Pen"}'      Bitmap Index Scan on docs_gin_path
-- body ? 'discount'             Seq Scan

CREATE INDEX docs_name ON docs ((body ->> 'name'));
-- body ->> 'name' = 'Pen'       Index Scan using docs_name
```

Планы получены с `enable_seqscan = off`, потому что на трёх строках дешевле последовательное чтение.

| Класс операторов | Операторы | Размер |
|---|---|---|
| `jsonb_ops` (по умолчанию) | `?`, `?\|`, `?&`, `@>`, `@?`, `@@` | больше |
| `jsonb_path_ops` | `@>`, `@?`, `@@` | обычно намного меньше |

## Вычисляемые колонки и ограничения

```sql
ALTER TABLE docs ADD COLUMN name text GENERATED ALWAYS AS (body ->> 'name') STORED;
ALTER TABLE docs ADD CONSTRAINT has_name
    CHECK (body ? 'name' AND jsonb_typeof(body -> 'name') = 'string');
INSERT INTO docs(id, body) VALUES (4, '{"name":5}');
-- ERROR:  new row for relation "docs" violates check constraint "has_name"
```

Вычисляемая колонка даёт часто используемому полю имя, тип и собственную статистику.

## Лучшие практики

- Ключи, по которым фильтруют или соединяют, держите колонками или вычисляемыми колонками.
- Ограничивайте форму документов, на которую полагается приложение.
- Для фильтров, которые должен обслуживать GIN, предпочитайте `@>`.
- Держите документы умеренного размера.

## Частые ошибки

- ❌ GIN по `body` для `body ->> 'x' = ...` → ✅ индекс по выражению.
- ❌ `||` для глубокого слияния → ✅ `jsonb_set` по вложенному пути.
- ❌ Приведение типа без учёта плохих значений → ✅ проверять `jsonb_typeof` или ограничить данные.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [PostgreSQL 17: типы JSON](https://www.postgresql.org/docs/17/datatype-json.html)
- [PostgreSQL 17: функции и операторы JSON](https://www.postgresql.org/docs/17/functions-json.html)
