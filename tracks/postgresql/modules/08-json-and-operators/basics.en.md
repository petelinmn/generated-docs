---
title: "JSON and operators: basics"
lang: en
track: postgresql
kind: module
doc: basics
slug: 08-json-and-operators
part: 2
tags: [jsonb, json, jsonpath, gin, operators]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# JSON and operators: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** `SELECT`, `WHERE` · **Time:** ~2 h

## Data for the examples

All results were produced on PostgreSQL 17.11. Each example starts from this data. Note that document 3 stores the price as a string.

```sql
CREATE TABLE docs (id int PRIMARY KEY, body jsonb NOT NULL);
INSERT INTO docs VALUES
 (1, '{"name":"Pen","price":2,"tags":["office","cheap"],"stock":{"msk":5,"spb":0}}'),
 (2, '{"name":"Book","price":15,"tags":["read"],"stock":{"msk":0}}'),
 (3, '{"name":"Lamp","price":"30","tags":[],"discount":null}');
```

## json or jsonb

```sql
SELECT '{"b":1,  "a":2, "b":3}'::json  AS j,    -- {"b":1,  "a":2, "b":3}
       '{"b":1,  "a":2, "b":3}'::jsonb AS jb;   -- {"a": 2, "b": 3}
```

| | `json` | `jsonb` |
|---|---|---|
| Stored as | the input text | a binary structure |
| Whitespace, key order, duplicate keys | preserved | not preserved; the last duplicate wins |
| Processing | reparsed every time | no reparsing |
| Indexing inside | no GIN operators | GIN |

Use `jsonb` unless you must keep the text exactly as received.

## Getting values out

```sql
SELECT body -> 'name'          AS j,       -- "Pen"    (jsonb)
       body ->> 'name'         AS t,       -- Pen      (text)
       body #>> '{tags,0}'     AS first,   -- office
       body -> 'tags' -> -1    AS last,    -- "cheap"
       body['stock']['msk']    AS msk,     -- 5
       body -> 'nope'          AS missing  -- NULL
FROM docs WHERE id = 1;
```

`->` keeps the result as `jsonb`, so you can continue descending; `->>` ends with `text`. `#>` and `#>>` take a path.

## Searching

```sql
SELECT id FROM docs WHERE body @> '{"tags":["cheap"]}';   -- 1
SELECT id FROM docs WHERE body ? 'discount';              -- 3
SELECT id FROM docs WHERE body -> 'tags' ? 'read';        -- 2
```

## Beginner mistakes

- ❌ `body -> 'name' = 'Pen'` → ✅ `body ->> 'name' = 'Pen'` to compare text.
- ❌ Comparing numbers as text → ✅ cast: `(body ->> 'price')::numeric`.
- ❌ Putting everything into one `jsonb` column → ✅ columns for the stable, important fields.
- ❌ Expecting key order to survive in `jsonb` → ✅ it does not.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [PostgreSQL 17: JSON types](https://www.postgresql.org/docs/17/datatype-json.html)
- [PostgreSQL 17: JSON functions and operators](https://www.postgresql.org/docs/17/functions-json.html)
