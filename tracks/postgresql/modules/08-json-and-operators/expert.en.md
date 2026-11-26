---
title: "JSON and operators: expert"
lang: en
track: postgresql
kind: module
doc: expert
slug: 08-json-and-operators
part: 2
tags: [jsonb, json, jsonpath, gin, operators]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# JSON and operators: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

`jsonb` is parsed once on input into a binary structure, so reading a field needs no text parsing, but input is a little slower than for `json`. Large values are compressed and may be moved out of the main row (TOAST). On `UPDATE`, unchanged out-of-line values are normally kept as they are; a changed document is written anew. Any update locks the whole row, so the documentation advises keeping documents to a manageable size.

`jsonb_ops` indexes keys and values as separate items; `jsonb_path_ops` indexes hashes of whole paths to values, which is why it is smaller and cannot answer "is there such a key".

## jsonpath

```sql
SELECT id FROM docs WHERE body @? '$.stock.* ? (@ > 0)';   -- 1
SELECT id FROM docs WHERE body @@ '$.price > 10';          -- 2   ("30" is a string)
SELECT jsonb_path_query_array('{"a":[1,2,3]}', '$.a[*] ? (@ >= $min)', '{"min":2}');  -- [2, 3]

SELECT jsonb_path_exists('{"a":1}', 'lax $.b');      -- f
SELECT jsonb_path_exists('{"a":1}', 'strict $.b');
-- ERROR:  JSON object does not contain key "b"
```

`$` is the document, `.key` and `[*]` descend, `? (...)` filters, `@` is the current item, `$min` is a variable passed from SQL.

## SQL/JSON functions (PostgreSQL 17)

```sql
SELECT * FROM JSON_TABLE('[{"n":"a","p":1},{"n":"b"}]'::jsonb, '$[*]'
    COLUMNS (n text PATH '$.n', p int PATH '$.p'));
-- a 1 | b NULL

SELECT JSON_VALUE('{"p":"15"}'::jsonb, '$.p' RETURNING int),   -- 15
       JSON_QUERY('{"t":[1,2]}'::jsonb, '$.t'),                -- [1, 2]
       JSON_EXISTS('{"a":1}'::jsonb, '$.a');                   -- t
```

## Reference

| Operator | Meaning | Returns |
|---|---|---|
| `->`, `->>` | by key or array index | `jsonb`, `text` |
| `#>`, `#>>` | by path | `jsonb`, `text` |
| `@>`, `<@` | contains, is contained | `boolean` |
| `?`, `?\|`, `?&` | key or string element: one, any, all | `boolean` |
| `@?`, `@@` | jsonpath returns items, jsonpath predicate is true | `boolean` |
| `\|\|` | shallow concatenation | `jsonb` |
| `-`, `#-` | delete key or element, delete at path | `jsonb` |

| Function | Purpose |
|---|---|
| `jsonb_set`, `jsonb_set_lax`, `jsonb_insert` | change a value, with `NULL` policy, insert |
| `jsonb_build_object`, `jsonb_build_array`, `to_jsonb` | build |
| `jsonb_agg`, `jsonb_object_agg` | aggregate rows |
| `jsonb_array_elements[_text]`, `jsonb_each`, `jsonb_object_keys` | expand |
| `jsonb_to_record`, `jsonb_to_recordset`, `jsonb_populate_record` | to typed rows |
| `jsonb_typeof`, `jsonb_array_length`, `jsonb_strip_nulls` | inspect and clean |
| `jsonb_path_query`, `..._first`, `..._array`, `jsonb_path_exists`, `jsonb_path_match` | jsonpath |

`jsonb_set` creates the last key of the path if it is missing (`create_if_missing` is true by default), but all earlier steps must exist.

## Gotchas

### Three kinds of "nothing"

```sql
SELECT body -> 'discount' IS NULL,        -- f: there is a JSON null
       body ->> 'discount',               -- NULL
       jsonb_typeof(body -> 'discount'),  -- null
       body -> 'nope' IS NULL             -- t: the key is missing
FROM docs WHERE id = 3;
```

### SQL NULL destroys the document

```sql
SELECT jsonb_set('{"a":1}'::jsonb, '{a}', NULL);       -- NULL: the whole document is gone
SELECT jsonb_set_lax('{"a":1}'::jsonb, '{a}', NULL);   -- {"a": null}
```

### A CHECK passes when the key is missing

```sql
CREATE TABLE c (body jsonb CHECK (jsonb_typeof(body -> 'name') = 'string'));
INSERT INTO c VALUES ('{}');   -- accepted: the check evaluates to NULL, not false
```

A check constraint rejects a row only on `false`. Add `body ? 'name'` or use `coalesce`.

### jsonb cannot hold the zero character

`'{"a":"\u0000"}'::jsonb` fails with `unsupported Unicode escape sequence`. Payloads from outside may contain it.

## Trade-offs and alternatives

| Choice | Pro | Con |
|---|---|---|
| Columns | types, constraints, statistics, small updates | schema change for each attribute |
| `jsonb` | flexible attributes | weaker checks, whole-document rewrites |
| Columns + `jsonb` for the rest | the usual compromise | decide where a field belongs |
| Entity–attribute–value table | per-attribute rows and indexes | many joins, awkward queries |
| A document database | document-first model and tooling | another system to operate |

## Interview questions

**1. `json` or `jsonb`?**

<details><summary>Answer</summary>

`jsonb` for querying and indexing; `json` only to keep the exact input text.

</details>

**2. `->` versus `->>`?**

<details><summary>Answer</summary>

The first returns `jsonb`, the second `text`.

</details>

**3. Which operators does a default GIN index on `jsonb` support?**

<details><summary>Answer</summary>

`?`, `?|`, `?&`, `@>`, `@?`, `@@`.

</details>

**4. When is `jsonb_path_ops` better?**

<details><summary>Answer</summary>

Only containment and jsonpath are needed: the index is usually much smaller. Key-exists operators are lost.

</details>

**5. What does a row update cost with a large document?**

<details><summary>Answer</summary>

A new row version and a lock on the whole row; a changed large value is written anew.

</details>

**6. How do you index one field of a document?**

<details><summary>Answer</summary>

An expression index `((body ->> 'field'))` or a generated column with an index.

</details>

**7. What is the difference between lax and strict jsonpath?**

<details><summary>Answer</summary>

Lax (default) adapts to the structure and suppresses structural errors; strict raises them.

</details>

**8. What appeared in PostgreSQL 17 for JSON?**

<details><summary>Answer</summary>

`JSON_TABLE`, `JSON_VALUE`, `JSON_QUERY`, `JSON_EXISTS`.

</details>

**9. Trick question: `body -> 'k' IS NULL` is false but `body ->> 'k' IS NULL` is true. Why?**

<details><summary>Answer</summary>

The key holds a JSON `null`.

</details>

**10. Trick question: an `UPDATE` with `jsonb_set` erased the document. How?**

<details><summary>Answer</summary>

The new value was SQL `NULL`, so the result was `NULL`.

</details>

**11. When should a field become a column?**

<details><summary>Answer</summary>

When it is filtered, joined, sorted or constrained regularly.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [PostgreSQL 17: JSON types](https://www.postgresql.org/docs/17/datatype-json.html)
- [PostgreSQL 17: JSON functions and operators](https://www.postgresql.org/docs/17/functions-json.html)
- [PostgreSQL 17: TOAST](https://www.postgresql.org/docs/17/storage-toast.html)
- [PostgreSQL 17: release notes](https://www.postgresql.org/docs/17/release-17.html)
