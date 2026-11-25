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

**C.** `size` controls how many documents come back; at 0 you get only aggregation results, which is cheaper. It does not affect buckets or counts.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Which field can `terms` group by?

- A) `keyword`
- B) `text` with default settings
- C) `_source`
- D) any object

<details><summary>Answer</summary>

**A.** `keyword` has doc values and keeps the whole value. A default `text` field has none, and you cannot aggregate on `_source` or an arbitrary object.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 How many buckets does `terms` return without `size`?

- A) all
- B) 1
- C) 100
- D) at most 10

<details><summary>Answer</summary>

**D.** `terms` returns 10 buckets by default. To get every value use a larger `size` or `composite`.
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

**A, D.** `global` steps outside the query and covers all documents, and can hold sub-aggregations. Query filters do not apply to it, and it returns no documents.
→ [Fundamentals](basics.en.md)

</details>

## 7. 🟡 You need to export every city × product combination. With what?

- A) `composite`
- B) `terms` with `size: 10`
- C) `top_hits`
- D) `histogram`

<details><summary>Answer</summary>

**A.** `composite` pages through every combination. `terms` with `size: 10` gives only the top values, `top_hits` returns documents and `histogram` buckets numbers by interval.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What reduces the `terms` error over several shards?

- A) `size: 0`
- B) `refresh`
- C) increase `shard_size`
- D) add replicas

<details><summary>Answer</summary>

**C.** A larger `shard_size` makes each shard send more candidates, so the merged top is more accurate. Replicas and refresh do not affect accuracy, and `size: 0` does not refine counts.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🟡 How do you keep only buckets whose sum exceeds a threshold?

- A) `post_filter`
- B) a query `filter`
- C) `missing`
- D) `bucket_selector`

<details><summary>Answer</summary>

**D.** `bucket_selector` is a pipeline aggregation that filters computed buckets. Query filters and `post_filter` work on documents, and `missing` collects documents lacking the field.
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

**C.** By default a gap is skipped, so derivatives that depend on the missing value are not produced. It is not an error, the bucket stays, and treating it as zero needs another policy.
→ [Advanced theory](advanced.en.md)

</details>

## 12. 🔴 Open question: a "top 100 customers by revenue" report over a 20-shard index must be exact. Options?

<details><summary>Answer</summary>

Route by customer (all of a customer's documents in one shard), minding skew; walk all customers with `composite` and sort in the application; keep a precomputed summary index. Raising `shard_size` lowers the error but does not guarantee exactness in general.
→ [Advanced theory](advanced.en.md)

</details>
