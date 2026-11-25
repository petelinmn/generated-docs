---
title: "Quiz: mappings and analysis"
lang: en
track: opensearch
kind: module
doc: quiz
slug: 02-mappings-and-analysis
part: 1
tags: [mapping, analyzer, inverted-index, text-keyword, nested, dynamic-mapping]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Quiz: mappings and analysis

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What type does a field get if the first value is `"42"`?

- A) `integer`
- B) `date`
- C) `text` with a `keyword` sub-field
- D) `long`

<details><summary>Answer</summary>

**C.** The value arrived as a quoted string, and default dynamic mapping turns a string into `text` with a `keyword` subfield. It does not become a number, and it is not a date.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Which field suits `keyword`?

- A) a category code
- B) a product description
- C) an article body
- D) a review

<details><summary>Answer</summary>

**A.** The rest are searched by words.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 What does `english` produce for "foxes jumped"?

- A) `foxes, jumped`
- B) `Foxes, Jumped`
- C) one string
- D) `fox, jump`

<details><summary>Answer</summary>

**D.** Stemming.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 Can an existing `long` field be changed to `keyword`?

- A) yes, after closing the index
- B) no, a new index is needed
- C) yes, `put_mapping`
- D) yes, after a refresh

<details><summary>Answer</summary>

**B.** An existing field's type cannot change because data is already indexed the old way. You need a new index and a `reindex`; closing, `put_mapping` and refresh do not help.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 `title` = "Hello World" (`text`). What does `term: {title: "Hello World"}` find?

- A) all documents
- B) nothing
- C) the document
- D) a 400 error

<details><summary>Answer</summary>

**B.** `term` does not analyze the query.
→ [Fundamentals](basics.en.md)

</details>

## 6. 🟡 An `edge_ngram` autocomplete finds "Laptop" for the query "lamp". Why?

- A) no `refresh`
- B) `max_gram` is too small
- C) the field is `keyword`
- D) no `search_analyzer` is set

<details><summary>Answer</summary>

**D.** The query is cut into prefixes too, and `la` matches.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What is true with `dynamic: false`?

- [ ] A) a new field is stored in `_source`
- [ ] B) it cannot be searched
- [ ] C) the document is rejected
- [ ] D) the field is added to the mapping

<details><summary>Answer</summary>

**A, B.** With `false` the document is accepted and kept whole in `_source`, but the unknown field is not mapped or searchable. Rejecting the document is what `strict` does.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 An array of objects must be filtered by a pair of values of one element. The type?

- A) `keyword`
- B) `text`
- C) `nested`
- D) `object`

<details><summary>Answer</summary>

**C.** `nested` stores each array element separately, so conditions apply within one element. `object` flattens the array and mixes values, and `text` and `keyword` do not describe objects.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🟡 `7.9` was written into a `long` field. What happened in the experiment?

- A) an error
- B) the field became `float`
- C) `8` was stored
- D) accepted, found by `term: 7`

<details><summary>Answer</summary>

**D.** Coercion; `_source` is unchanged.
→ [Advanced theory](advanced.en.md)

</details>

## 10. 🔴 Which structure does sorting need?

- A) the translog
- B) doc values
- C) the inverted index
- D) norms

<details><summary>Answer</summary>

**B.** Doc values are a columnar document-to-value structure used for sorting and aggregations. The inverted index answers which documents hold a term; the translog is for durability and norms for scoring.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 A `keyword` with `ignore_above: 256`, a 300-character value:

- A) is truncated to 256
- B) is indexed in full
- C) is stored but not found by this field
- D) the document is rejected

<details><summary>Answer</summary>

**C.** Values over the threshold are not indexed, yet the document is accepted and `_source` keeps them in full. Nothing is truncated or rejected.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: customers send arbitrary product attributes. How do you avoid a mapping explosion?

<details><summary>Answer</summary>

Do not turn attributes into field names: `dynamic: false` or `strict` for the object; `flat_object`; or a `nested` array of `{name, value}`; the field limit is a safeguard, not a solution.
→ [Expert theory](expert.en.md)

</details>
