---
title: "Quiz: relevance"
lang: en
track: opensearch
kind: module
doc: quiz
slug: 06-relevance-tuning
part: 2
tags: [relevance, bm25, boosting, function-score, synonyms]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Quiz: relevance

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 The same term once in a short and in a long field. Which usually ranks higher?

- A) always equal
- B) depends on `_id`
- C) the short field
- D) the long field

<details><summary>Answer</summary>

**C.**
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 What does `title^3` mean?

- A) the field's contribution is multiplied by 3
- B) the word must occur 3 times
- C) 3 typos
- D) the third version of the field

<details><summary>Answer</summary>

**A.**
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 How do you demote documents with a word without removing them?

- A) `must_not`
- B) `post_filter`
- C) `filter`
- D) `boosting` with `negative_boost`

<details><summary>Answer</summary>

**D.**
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 The default `multi_match` type?

- A) `phrase`
- B) `best_fields`
- C) `most_fields`
- D) `cross_fields`

<details><summary>Answer</summary>

**B.**
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 `field_value_factor` on `views` without a modifier. What happened?

- A) scores were normalised to 1
- B) views decided the order
- C) nothing
- D) an error

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 What is true of synonyms in a `search_analyzer`?

- [ ] A) they act on the query
- [ ] B) they change `_source`
- [ ] C) they need a new index per search
- [ ] D) they need no reindexing

<details><summary>Answer</summary>

**A, D.**
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What happens to a document outside `window_size` in `rescore`?

- A) it is not rescored
- B) it is removed
- C) it gets 0
- D) it is rescored later

<details><summary>Answer</summary>

**A.**
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 A `script_score` result is negative:

- A) the absolute value is used
- B) the score is 0
- C) a 400 error
- D) the document goes last

<details><summary>Answer</summary>

**C.**
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 What does `b = 0` do in BM25?

- A) removes idf
- B) removes term frequency
- C) zeroes the score
- D) removes the effect of field length

<details><summary>Answer</summary>

**D.**
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 Identical documents got different scores across three shards. What evened them out?

- A) `explain`
- B) `dfs_query_then_fetch`
- C) `refresh`
- D) a replica

<details><summary>Answer</summary>

**B.**
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 The only relevant document is third. MRR@3?

- A) 0
- B) 3
- C) 1/3
- D) 1

<details><summary>Answer</summary>

**C.**
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: users complain that old popular products top the results. What do you do?

<details><summary>Answer</summary>

Collect the problem queries and ratings; use `explain` to see popularity's share; damp it (`log1p`, addition, a cap), add recency decay with a non-zero floor; compare `_rank_eval` metrics before and after; try on a share of traffic.
→ [Advanced theory](advanced.en.md)

</details>
