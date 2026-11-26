---
title: "JSON and operators: advanced"
lang: en
track: postgresql
kind: module
doc: advanced
slug: 08-json-and-operators
part: 2
tags: [jsonb, json, jsonpath, gin, operators]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# JSON and operators: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Types inside documents

```sql
SELECT id, body ->> 'price' FROM docs ORDER BY body ->> 'price';
-- 2 '15' | 1 '2' | 3 '30'    (text order)

SELECT id, (body ->> 'price')::numeric FROM docs ORDER BY (body ->> 'price')::numeric;
-- 1 2 | 2 15 | 3 30

SELECT id FROM docs WHERE body @> '{"price":30}';     -- no rows
SELECT id FROM docs WHERE body @> '{"price":"30"}';   -- 3
```

`30` and `"30"` are different values. The database will not enforce a type inside a document unless you add a constraint.

## Containment rules

```sql
SELECT '[1,2,3]'::jsonb   @> '[3,1]',          -- t: order does not matter
       '[1,[2,3]]'::jsonb @> '[2]',            -- f: nesting does
       '{"a":{"b":1}}'::jsonb @> '{"b":1}';    -- f: only from the top level

SELECT id FROM docs WHERE body ?| array['discount','stock'];  -- 1, 2, 3: any key
SELECT id FROM docs WHERE body ?& array['discount','stock'];  -- no rows: all keys
```

## Modifying

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

`||` merges only the top level: a nested object is replaced, not merged. Every modification produces a new document and a new row version.

## Building documents from rows

```sql
SELECT jsonb_build_object('id', 1, 'tags', jsonb_build_array('a','b'));
-- {"id": 1, "tags": ["a", "b"]}

SELECT jsonb_agg(body ->> 'name' ORDER BY id) FROM docs;
-- ["Pen", "Book", "Lamp"]

SELECT jsonb_object_agg(body ->> 'name', body -> 'price' ORDER BY id) FROM docs;
-- {"Pen": 2, "Book": 15, "Lamp": "30"}
```

## Expanding documents into rows

```sql
SELECT d.id, t.tag
FROM docs d
CROSS JOIN LATERAL jsonb_array_elements_text(d.body -> 'tags') AS t(tag)
ORDER BY 1, 2;
-- 1 cheap | 1 office | 2 read      (document 3 has no tags and disappears)

SELECT * FROM jsonb_to_record('{"name":"Pen","price":2}') AS x(name text, price int);
```

`LEFT JOIN LATERAL ... ON true` keeps documents with an empty array.

## Indexes

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

The plans were obtained with `enable_seqscan = off`, because on three rows a sequential scan is cheaper.

| Operator class | Operators | Size |
|---|---|---|
| `jsonb_ops` (default) | `?`, `?\|`, `?&`, `@>`, `@?`, `@@` | larger |
| `jsonb_path_ops` | `@>`, `@?`, `@@` | usually much smaller |

## Generated columns and constraints

```sql
ALTER TABLE docs ADD COLUMN name text GENERATED ALWAYS AS (body ->> 'name') STORED;
ALTER TABLE docs ADD CONSTRAINT has_name
    CHECK (body ? 'name' AND jsonb_typeof(body -> 'name') = 'string');
INSERT INTO docs(id, body) VALUES (4, '{"name":5}');
-- ERROR:  new row for relation "docs" violates check constraint "has_name"
```

A generated column gives an often used field a name, a type and its own statistics.

## Best practices

- Keep keys that are filtered or joined as columns or generated columns.
- Constrain the shape of documents that the application relies on.
- Prefer `@>` for filters that a GIN index should serve.
- Keep documents of a moderate size.

## Common mistakes

- ❌ A GIN index on `body` for `body ->> 'x' = ...` → ✅ an expression index.
- ❌ `||` for a deep merge → ✅ `jsonb_set` on the nested path.
- ❌ Casting without regard to bad values → ✅ check `jsonb_typeof` or constrain the data.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [PostgreSQL 17: JSON types](https://www.postgresql.org/docs/17/datatype-json.html)
- [PostgreSQL 17: JSON functions and operators](https://www.postgresql.org/docs/17/functions-json.html)
