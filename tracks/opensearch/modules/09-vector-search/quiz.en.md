---
title: "Quiz: vector search"
lang: en
track: opensearch
kind: module
doc: quiz
slug: 09-vector-search
part: 2
tags: [vector-search, knn, embeddings, hybrid-search, hnsw]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Quiz: vector search

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does a `knn_vector` field hold?

- A) free text
- B) a document hash
- C) a fixed-length array of numbers
- D) a list of words

<details><summary>Answer</summary>

**C.**
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 `k: 5`, `size: 2`. How many documents in `hits.hits` (one shard)?

- A) 2
- B) 5
- C) 10
- D) 7

<details><summary>Answer</summary>

**A.**
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 The query vector is shorter than in the mapping:

- A) padded with zeros
- B) extra dimensions ignored
- C) an empty result
- D) an error

<details><summary>Answer</summary>

**D.**
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 The default space type?

- A) `hamming`
- B) `l2`
- C) `cosinesimil`
- D) `innerproduct`

<details><summary>Answer</summary>

**B.**
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 Where does a filter go to get k matching results?

- A) in `aggs`
- B) inside `knn`
- C) in `post_filter`
- D) in an outer `bool.filter`

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 Which space type ignores vector length?

- A) `l2`
- B) `innerproduct`
- C) `l1`
- D) `cosinesimil`

<details><summary>Answer</summary>

**D.**
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What is true of RRF?

- [ ] A) it uses positions, not scores
- [ ] B) `rank_constant` is 60 by default
- [ ] C) it requires `min_max` normalisation
- [ ] D) it works with one subquery only

<details><summary>Answer</summary>

**A, B.**
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What computes exact neighbours?

- A) `match_all`
- B) a BM25 `rescore`
- C) `script_score` with `knn_score`
- D) `knn` with a large `size`

<details><summary>Answer</summary>

**C.**
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 `hybrid` with no search pipeline:

- A) RRF kicks in
- B) scores are normalised anyway
- C) only the first subquery is returned
- D) internal markers and duplicates in the response

<details><summary>Answer</summary>

**D.**
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 The HNSW memory estimate per vector:

- A) `m * k` bytes
- B) `1.1 * (4*d + 8*m)` bytes
- C) `d` bytes
- D) `4*d` bits

<details><summary>Answer</summary>

**B.**
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 Which engine cannot be used for a new index in 3.x?

- A) `lucene`
- B) all are available
- C) `nmslib`
- D) `faiss`

<details><summary>Answer</summary>

**C.**
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: for a part-number query vector search returns look-alikes but not the product. What do you do?

<details><summary>Answer</summary>

Embeddings tell exact codes apart poorly. Add a full-text or `keyword` subquery and combine with `hybrid`; route part-number-like queries to exact matching; verify on rated queries.
→ [Advanced theory](advanced.en.md)

</details>
