---
title: "Quiz: jSON and operators"
lang: en
track: postgresql
kind: module
doc: quiz
slug: 08-json-and-operators
part: 2
tags: [jsonb, json, jsonpath, gin, operators]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Quiz: jSON and operators

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `->>` return?

- A) `jsonb`
- B) `text`
- C) `json`
- D) The type of the value

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 Which type preserves duplicate keys of the input?

- A) `jsonb`
- B) `json`
- C) Both
- D) Neither

<details><summary>Answer</summary>

**B.** `json` stores the text as it was.

→ [Basics](basics.en.md)

</details>

## 3. 🟢 What does `@>` check?

- A) Greater than
- B) That the left document contains the right one
- C) That a key exists
- D) Text similarity

<details><summary>Answer</summary>

**B.** Key existence is `?`.

→ [Basics](basics.en.md)

</details>

## 4. 🟢 Which documents of the module match `body ? 'discount'`?

- A) None, the value is null
- B) Document 3
- C) All
- D) Documents 1 and 2

<details><summary>Answer</summary>

**B.** The key exists even though its value is JSON `null`.

→ [Basics](basics.en.md)

</details>

## 5. 🟡 Order the module documents by `body ->> 'price'`. Which id comes first?

- A) 1
- B) 2
- C) 3
- D) The order is undefined

<details><summary>Answer</summary>

**B.** Text order: `'15'` sorts before `'2'`.

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 What is the result?

```sql
SELECT '{"a":{"x":1}}'::jsonb || '{"a":{"y":2}}';
```

<details><summary>Answer</summary>

```text
{"a": {"y": 2}}
```

The concatenation is shallow: the nested object is replaced.

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select all true statements about indexes

- [ ] A) `jsonb_ops` supports `?`
- [ ] B) `jsonb_path_ops` supports `?`
- [ ] C) `jsonb_path_ops` is usually smaller
- [ ] D) A GIN on `body` serves `body ->> 'name' = 'x'`

<details><summary>Answer</summary>

**A, C.** Equality on an extracted value needs an expression index.

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Which index serves `body ->> 'name' = 'Pen'`?

- A) GIN on `body`
- B) B-tree on `((body ->> 'name'))`
- C) BRIN on `body`
- D) None possible

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 Find the problem: "documents cheaper than 20"

```sql
SELECT id FROM docs WHERE body ->> 'price' < '20';
```

<details><summary>Answer</summary>

Strings are compared, not numbers: `'100' < '20'` is true and `'3' < '20'` is false. Cast to `numeric`.

→ [Advanced](advanced.en.md)

</details>

## 10. 🔴 What does this print?

```sql
SELECT jsonb_set('{"a":1}'::jsonb, '{a}', NULL);
```

<details><summary>Answer</summary>

```text
NULL
```

SQL `NULL` as the new value makes the result `NULL`; `jsonb_set_lax` avoids it.

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Does `CHECK (jsonb_typeof(body -> 'name') = 'string')` reject `{}`?

- A) Yes
- B) No
- C) Only with `NOT NULL`
- D) It raises a type error

<details><summary>Answer</summary>

**B.** The condition is `NULL`, and a check fails only on `false`. `NOT NULL` on the column does not help.

→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question: attributes of products differ by category. How do you store them?

<details><summary>Key points</summary>

- Common fields as typed columns, the rest in `jsonb`.
- Constraints on required keys and types.
- GIN or expression indexes according to real queries.
- Promote a key to a column when it becomes central.

→ [Expert](expert.en.md)

</details>
