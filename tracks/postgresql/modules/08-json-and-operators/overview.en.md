---
title: "JSON and operators"
lang: en
track: postgresql
kind: module
doc: overview
slug: 08-json-and-operators
part: 2
levels: [1, 2, 3, 4]
tags: [jsonb, json, jsonpath, gin, operators]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# JSON and operators

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

PostgreSQL can store JSON documents in a column and query inside them. `jsonb` is the binary form that you will almost always want: it can be indexed and searched efficiently. It suits attributes whose set varies from row to row. It does not replace columns: types, constraints, foreign keys and statistics work far better on ordinary columns.

## Key ideas

- `json` keeps the exact text, `jsonb` keeps a parsed structure.
- `->` returns `jsonb`, `->>` returns `text`; compare numbers after a cast.
- `@>` asks "does the document contain this", `?` asks "is there such a key".
- A JSON `null`, a missing key and SQL `NULL` are three different things.
- Updating a part of a document writes a new version of the row.
- GIN serves containment and key tests; an expression index serves one extracted value.
- jsonpath expresses conditions inside nested structures.

## Minimal example

```sql
CREATE TABLE docs (id int PRIMARY KEY, body jsonb NOT NULL);
INSERT INTO docs VALUES
 (1, '{"name":"Pen","price":2,"tags":["office","cheap"],"stock":{"msk":5,"spb":0}}'),
 (2, '{"name":"Book","price":15,"tags":["read"],"stock":{"msk":0}}'),
 (3, '{"name":"Lamp","price":"30","tags":[],"discount":null}');
```

```sql
SELECT body -> 'name'          AS j,       -- "Pen"    (jsonb)
       body ->> 'name'         AS t,       -- Pen      (text)
       body #>> '{tags,0}'     AS first,   -- office
       body -> 'tags' -> -1    AS last,    -- "cheap"
       body['stock']['msk']    AS msk,     -- 5
       body -> 'nope'          AS missing  -- NULL
FROM docs WHERE id = 1;
```

```sql
SELECT id FROM docs WHERE body @> '{"tags":["cheap"]}';   -- 1
SELECT id FROM docs WHERE body ? 'discount';              -- 3
SELECT id FROM docs WHERE body -> 'tags' ? 'read';        -- 2
```

## When to use and when not

- ✅ Optional or user-defined attributes, raw payloads from external systems.
- ✅ A nested structure that is read and written as a whole.
- ❌ Values you filter, join or constrain constantly: make them columns.
- ❌ A large document in which one counter changes often.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | `json` and `jsonb`, access and containment operators |
| [2. Advanced](advanced.en.md) | types, modifying, building and expanding documents, indexes, constraints |
| [3. Expert](expert.en.md) | jsonpath, SQL/JSON functions, reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn postgresql 08-json-and-operators level 5` |
| 6. Tests | not created yet — `/learn postgresql 08-json-and-operators level 6` |

## Related materials

- Prerequisites: [04 · Joins and subqueries](../04-joins-and-subqueries/overview.en.md), [05 · Indexes](../05-indexes/overview.en.md)
- Next: the following module of the [PostgreSQL roadmap](../../overview.en.md)
- Documents or relations: [data storage](../../../system-design/modules/04-data-storage/overview.en.md)
- Documents with search: [OpenSearch](../../../opensearch/modules/01-opensearch-basics/overview.en.md)
