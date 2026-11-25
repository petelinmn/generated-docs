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

**C.** `match` analyses the query like the field and finds a word inside text. `term` looks up an exact unanalysed term, `ids` matches identifiers, and `exists` only checks the field is present.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Where do you put a category filter that must not affect ranking?

- A) `bool.filter`
- B) `bool.must`
- C) `bool.should`
- D) `highlight`

<details><summary>Answer</summary>

**A.** Clauses in `filter` select documents without scoring and can be cached. `must` and `should` affect the score, and `highlight` only marks matches.
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

**B.** The default `size` is 10; get more with a larger `size` or pagination. All documents are never returned at once.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 A `bool` with `filter` and one `should` without `minimum_should_match`:

- A) an error
- B) `should` only raises the score
- C) `should` is required
- D) `should` is ignored

<details><summary>Answer</summary>

**B.** With a `filter` or `must` present, `should` is optional and only adds score to documents that match it. It is not an error, and the clause is not ignored.
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

**A, B, D.** `search_after` continues from the last sort values, so it needs a sort and is not bound by the 10,000 window; a PIT freezes the view. It cannot jump to an arbitrary page.
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

**D.** idf grows the fewer documents contain the term. tf is frequency within a document, and `k1` and `b` tune saturation and length normalisation.
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

**C.** BM25 statistics are computed per shard by default and differ between shards. There is no randomness, and neither `_source` nor `_id` is part of the formula.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: design a catalog query — a search box, category and price filters, in-stock items higher, category counts.

<details><summary>Answer</summary>

`bool`: `must` is a `multi_match` with a name boost and `fuzziness`; `filter` is the price; `should` is `range stock > 0` as a boost; the selected category goes to `post_filter`; `aggs terms` on category; sort by `_score` with a unique second key for `search_after`.
→ [Advanced theory](advanced.en.md)

</details>
