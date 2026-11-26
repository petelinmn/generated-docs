---
title: "Exercises: jSON and operators"
lang: en
track: postgresql
kind: module
doc: exercises
slug: 08-json-and-operators
part: 2
tags: [jsonb, json, jsonpath, gin, operators]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Exercises: jSON and operators

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Use the data from [basics](basics.en.md).

## 1. 🟢 Cheap products

**Task:** names and prices of products cheaper than 20, in price order. Document 3 stores the price as a string.

<details><summary>Hint</summary>

Extract text, then cast.

</details>

<details><summary>Solution</summary>

```sql
SELECT body ->> 'name' AS name, (body ->> 'price')::numeric AS price
FROM docs
WHERE (body ->> 'price')::numeric < 20
ORDER BY price;
-- Pen 2 | Book 15
```

The cast also works for the string `"30"`; a value such as `"n/a"` would make the query fail.

</details>

## 2. 🟡 Tag statistics

**Task:** count how many documents use each tag.

<details><summary>Hint</summary>

Turn the array into rows.

</details>

<details><summary>Solution</summary>

```sql
SELECT t.tag, count(*)
FROM docs d CROSS JOIN LATERAL jsonb_array_elements_text(d.body -> 'tags') AS t(tag)
GROUP BY t.tag ORDER BY t.tag;
-- cheap 1 | office 1 | read 1
```

</details>

## 3. 🔴 Adding stock where there is none

**Task:** set the stock in `kzn` to 3 for document 3, which has no `stock` object at all.

<details><summary>Hint</summary>

`jsonb_set` does not create intermediate objects.

</details>

<details><summary>Solution</summary>

```sql
UPDATE docs
SET body = jsonb_set(body, '{stock}',
                     coalesce(body -> 'stock', '{}') || '{"kzn": 3}')
WHERE id = 3;

SELECT body -> 'stock' FROM docs WHERE id = 3;   -- {"kzn": 3}
```

`coalesce` supplies an empty object, `||` keeps the other cities if there are any.

</details>
