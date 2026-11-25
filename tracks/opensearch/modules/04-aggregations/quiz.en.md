---
title: "Quiz: aggregations"
lang: en
track: opensearch
kind: module
doc: quiz
slug: 04-aggregations
part: 1
tags: [aggregations, buckets, metrics, cardinality, composite-aggregation]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Quiz: aggregations

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Why `size: 0` in an aggregation request?

- A) to disable aggregations
- B) to return all buckets
- C) to return no documents
- D) to reset counters

<details><summary>Answer</summary>

**C.**
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Which field can `terms` group by?

- A) `keyword`
- B) `text` with default settings
- C) `_source`
- D) any object

<details><summary>Answer</summary>

**A.**
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 How many buckets does `terms` return without `size`?

- A) all
- B) 1
- C) 100
- D) at most 10

<details><summary>Answer</summary>

**D.**
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 A `range` bucket `from: 30, to: 100`. A price of exactly 100:

- A) causes an error
- B) is not included
- C) is included
- D) is in both neighbours

<details><summary>Answer</summary>

**B.** `from` is inclusive, `to` is not.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 A typo in the field name of `sum`. The result?

- A) the sum of all fields
- B) 0.0 and no error
- C) a 400 error
- D) `null`

<details><summary>Answer</summary>

**B.** `avg` gives `null` in the same case.
→ [Expert theory](expert.en.md)

</details>

## 6. 🟡 What is true of `global`?

- [ ] A) it can hold nested aggregations
- [ ] B) it respects only the query's `filter`
- [ ] C) it returns documents
- [ ] D) it ignores the query

<details><summary>Answer</summary>

**A, D.**
→ [Fundamentals](basics.en.md)

</details>

## 7. 🟡 You need to export every city × product combination. With what?

- A) `composite`
- B) `terms` with `size: 10`
- C) `top_hits`
- D) `histogram`

<details><summary>Answer</summary>

**A.**
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What reduces the `terms` error over several shards?

- A) `size: 0`
- B) `refresh`
- C) increase `shard_size`
- D) add replicas

<details><summary>Answer</summary>

**C.**
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🟡 How do you keep only buckets whose sum exceeds a threshold?

- A) `post_filter`
- B) a query `filter`
- C) `missing`
- D) `bucket_selector`

<details><summary>Answer</summary>

**D.**
→ [Advanced theory](advanced.en.md)

</details>

## 10. 🔴 What did default `cardinality` return for 50,000 values in the experiment?

- A) an error
- B) an approximate number more than 1% off
- C) exactly 50000
- D) 3000

<details><summary>Answer</summary>

**B.** 50741.
→ [Advanced theory](advanced.en.md)

</details>

## 11. 🔴 A month has no data, `derivative` with the default `gap_policy`:

- A) an error
- B) the month is removed from the response
- C) a gap: neighbouring values are not computed
- D) it counts as zero

<details><summary>Answer</summary>

**C.**
→ [Advanced theory](advanced.en.md)

</details>

## 12. 🔴 Open question: a "top 100 customers by revenue" report over a 20-shard index must be exact. Options?

<details><summary>Answer</summary>

Route by customer (all of a customer's documents in one shard), minding skew; walk all customers with `composite` and sort in the application; keep a precomputed summary index. Raising `shard_size` lowers the error but does not guarantee exactness in general.
→ [Advanced theory](advanced.en.md)

</details>
