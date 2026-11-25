---
title: "Aggregations: expert theory"
lang: en
track: opensearch
kind: module
doc: expert
slug: 04-aggregations
part: 1
tags: [aggregations, buckets, metrics, cardinality, composite-aggregation]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Aggregations: expert theory

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## How an aggregation runs

Each shard computes a partial result from the doc values of its segments, and the coordinating node merges them (reduce). Sums and minimums merge exactly; tops and distinct counts only approximately.

## Catalog

> ⚠️ Verify: the list follows the documentation; only the aggregations shown in the examples were run for this module.

| Group | Aggregations |
|---|---|
| Metrics | `avg`, `sum`, `min`, `max`, `value_count`, `stats`, `extended_stats`, `cardinality`, `percentiles`, `percentile_ranks`, `median_absolute_deviation`, `weighted_avg`, `top_hits`, `scripted_metric`, `geo_bounds`, `geo_centroid`, `matrix_stats` |
| Bucket | `terms`, `multi_terms`, `rare_terms`, `significant_terms`, `significant_text`, `histogram`, `date_histogram`, `auto_date_histogram`, `range`, `date_range`, `ip_range`, `filter`, `filters`, `global`, `missing`, `nested`, `reverse_nested`, `composite`, `sampler`, `diversified_sampler`, `adjacency_matrix`, `geohash_grid`, `geotile_grid`, `geo_distance` |
| Pipeline | `avg_bucket`, `sum_bucket`, `min_bucket`, `max_bucket`, `stats_bucket`, `percentiles_bucket`, `cumulative_sum`, `derivative`, `moving_fn`, `serial_diff`, `bucket_script`, `bucket_selector`, `bucket_sort` |

## Defaults and limits

| Parameter | Value |
|---|---|
| `terms.size` | 10 |
| `search.max_buckets` | 65535 |
| `gap_policy` | `skip` |

The `search.max_buckets` value came from the node settings. A `terms` request with `size: 70000` over 50,000 values succeeded: the buckets were below the limit — what counts is the number of buckets created, not `size`.

## Gotchas

1. `fixed_interval: "month"` is a parse error: month exists only in `calendar_interval`.
2. `avg` on an unmapped field gives `null` while `sum` gives `0.0`: a typo in a field name looks like a plausible zero.
3. `date_histogram` returns empty intervals by default; `min_doc_count: 1` removes them.
4. An empty bucket has `sum` 0.0, yet a pipeline aggregation with `skip` treats it as a gap.
5. `terms` on an array counts a document in each of its buckets.
6. `post_filter` narrows hits but not aggregations: with it `hits.total` became 2 while the buckets stayed complete.

## Interview questions

1. How does a metric aggregation differ from a bucket one?
<details><summary>Answer</summary>

A metric computes a number; a bucket aggregation splits documents into groups and can hold nested aggregations.
</details>

2. Why is `terms` approximate?
<details><summary>Answer</summary>

Shards send only their own top `shard_size` values.
</details>

3. How do you make `terms` more accurate?
<details><summary>Answer</summary>

Increase `shard_size` at the cost of memory and traffic, or route one key's documents to one shard.
</details>

4. What does `sum_other_doc_count` show?
<details><summary>Answer</summary>

How many documents are not in the returned buckets.
</details>

5. Why can't you aggregate on `text`?
<details><summary>Answer</summary>

It has no doc values; `fielddata` loads terms into the heap.
</details>

6. How do you get every group?
<details><summary>Answer</summary>

`composite` with `after_key`.
</details>

7. `global` versus `filter`?
<details><summary>Answer</summary>

`global` ignores the query, `filter` narrows further.
</details>

8. `calendar_interval` versus `fixed_interval`?
<details><summary>Answer</summary>

Calendar intervals respect varying month lengths; a fixed one is a constant duration and has no month unit.
</details>

9. How do you filter buckets by a metric?
<details><summary>Answer</summary>

`bucket_selector`.
</details>

10. Is `cardinality` exact?
<details><summary>Answer</summary>

No, it is a HyperLogLog++ estimate; accuracy is tuned with `precision_threshold`.
</details>

11. Does `post_filter` affect aggregations?
<details><summary>Answer</summary>

No.
</details>

12. How do you reduce a dashboard's load?
<details><summary>Answer</summary>

`size: 0`, filters in filter context, fewer nesting levels, pre-summarised data.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [02 · Mappings and analysis](../02-mappings-and-analysis/expert.en.md) — doc values

## Sources

- [Metric aggregations](https://docs.opensearch.org/latest/aggregations/metric/)
- [Bucket aggregations](https://docs.opensearch.org/latest/aggregations/bucket/)
- [Pipeline aggregations](https://docs.opensearch.org/latest/aggregations/pipeline/)
