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

**C.** BM25 normalises by field length, so one occurrence in a short text weighs more. The scores are not always equal, and `_id` plays no part.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 What does `title^3` mean?

- A) the field's contribution is multiplied by 3
- B) the word must occur 3 times
- C) 3 typos
- D) the third version of the field

<details><summary>Answer</summary>

**A.** `^3` is a boost: the field's score is multiplied by three. It says nothing about occurrence counts, typos or versions.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 How do you demote documents with a word without removing them?

- A) `must_not`
- B) `post_filter`
- C) `filter`
- D) `boosting` with `negative_boost`

<details><summary>Answer</summary>

**D.** `boosting` lowers the score of documents matching `negative` but keeps them. `must_not` removes them, and `filter` and `post_filter` select rather than rank.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 The default `multi_match` type?

- A) `phrase`
- B) `best_fields`
- C) `most_fields`
- D) `cross_fields`

<details><summary>Answer</summary>

**B.** The default is `best_fields`, which takes the best field's score. The other types must be named.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 `field_value_factor` on `views` without a modifier. What happened?

- A) scores were normalised to 1
- B) views decided the order
- C) nothing
- D) an error

<details><summary>Answer</summary>

**B.** With no modifier, large `views` values multiply the score directly and swamp text relevance. Nothing is normalised, and it is not an error.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 What is true of synonyms in a `search_analyzer`?

- [ ] A) they act on the query
- [ ] B) they change `_source`
- [ ] C) they need a new index per search
- [ ] D) they need no reindexing

<details><summary>Answer</summary>

**A, D.** Search-time synonyms expand the query, so indexed data need not be rebuilt. They leave `_source` alone and need no new index per search.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What happens to a document outside `window_size` in `rescore`?

- A) it is not rescored
- B) it is removed
- C) it gets 0
- D) it is rescored later

<details><summary>Answer</summary>

**A.** `rescore` recomputes only the top window on each shard; the rest keep their original score and stay in the results.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 A `script_score` result is negative:

- A) the absolute value is used
- B) the score is 0
- C) a 400 error
- D) the document goes last

<details><summary>Answer</summary>

**C.** Scores cannot be negative, so the request is rejected. No absolute value is taken and nothing is replaced with zero.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 What does `b = 0` do in BM25?

- A) removes idf
- B) removes term frequency
- C) zeroes the score
- D) removes the effect of field length

<details><summary>Answer</summary>

**D.** `b` sets the strength of length normalisation; at zero, length is ignored. idf and term frequency remain.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 Identical documents got different scores across three shards. What evened them out?

- A) `explain`
- B) `dfs_query_then_fetch`
- C) `refresh`
- D) a replica

<details><summary>Answer</summary>

**B.** `dfs_query_then_fetch` first gathers term statistics from all shards, making scores comparable. `explain` only explains, and refresh or replicas do not merge statistics.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 The only relevant document is third. MRR@3?

- A) 0
- B) 3
- C) 1/3
- D) 1

<details><summary>Answer</summary>

**C.** Reciprocal rank is one over the position of the first relevant document: 1/3. It would be 0 outside the top three and 1 in first place.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: users complain that old popular products top the results. What do you do?

<details><summary>Answer</summary>

Collect the problem queries and ratings; use `explain` to see popularity's share; damp it (`log1p`, addition, a cap), add recency decay with a non-zero floor; compare `_rank_eval` metrics before and after; try on a share of traffic.
→ [Advanced theory](advanced.en.md)

</details>
