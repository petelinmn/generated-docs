---
title: "Quiz: search queries"
lang: en
track: opensearch
kind: module
doc: quiz
slug: 03-search-queries
part: 1
tags: [query-dsl, full-text-search, bool-query, relevance, bm25, pagination]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Quiz: search queries

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What do you use to find a word in a product description?

- A) `ids`
- B) `exists`
- C) `match`
- D) `term`

<details><summary>Answer</summary>

**C.**
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Where do you put a category filter that must not affect ranking?

- A) `bool.filter`
- B) `bool.must`
- C) `bool.should`
- D) `highlight`

<details><summary>Answer</summary>

**A.**
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 `match` for "wireless mouse" without `operator`:

- A) both words are required
- B) the exact phrase is required
- C) an error
- D) any of the words is enough

<details><summary>Answer</summary>

**D.** The default is `or`.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 How many documents are returned by default?

- A) 1
- B) 10
- C) 100
- D) all

<details><summary>Answer</summary>

**B.**
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 A `bool` with `filter` and one `should` without `minimum_should_match`:

- A) an error
- B) `should` only raises the score
- C) `should` is required
- D) `should` is ignored

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 How do you show complete category counts while one category is selected?

- A) `filter` inside `bool`
- B) `must_not`
- C) `_source`
- D) `post_filter`

<details><summary>Answer</summary>

**D.** It is applied after aggregations.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What is true of `search_after`?

- [ ] A) It requires a sort
- [ ] B) It can go beyond 10000
- [ ] C) It can jump straight to page 500
- [ ] D) With a PIT it gives a consistent traversal

<details><summary>Answer</summary>

**A, B, D.**
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 A user typed `name:(mouse` into a search box. What is safer?

- A) `term`
- B) `script`
- C) `simple_query_string`
- D) `query_string`

<details><summary>Answer</summary>

**C.** `query_string` returned 400.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 Which BM25 factor grows when a term is rare?

- A) tf
- B) b
- C) k1
- D) idf

<details><summary>Answer</summary>

**D.**
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 Will `match` on an `english` field find `mouse` with the query "mice"?

- A) only with `highlight`
- B) no
- C) yes
- D) only with `refresh`

<details><summary>Answer</summary>

**B.** The stemmer does not know irregular forms; synonyms are needed.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 Why did two identical documents get different scores?

- A) randomness
- B) because of `_source`
- C) they are in different shards with different statistics
- D) the `_id` affects BM25

<details><summary>Answer</summary>

**C.**
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: design a catalog query — a search box, category and price filters, in-stock items higher, category counts.

<details><summary>Answer</summary>

`bool`: `must` is a `multi_match` with a name boost and `fuzziness`; `filter` is the price; `should` is `range stock > 0` as a boost; the selected category goes to `post_filter`; `aggs terms` on category; sort by `_score` with a unique second key for `search_after`.
→ [Advanced theory](advanced.en.md)

</details>
