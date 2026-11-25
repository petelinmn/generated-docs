---
title: "Aggregations: advanced theory"
lang: en
track: opensearch
kind: module
doc: advanced
slug: 04-aggregations
part: 1
tags: [aggregations, buckets, metrics, cardinality, composite-aggregation]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Aggregations: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

The data and the `agg` function are from the [fundamentals](basics.en.md).

## Pipeline aggregations

They are computed from the results of other aggregations, addressed with `buckets_path`.

```python
months = {"date_histogram": {"field": "day", "calendar_interval": "month", "format": "yyyy-MM"}, "aggs": {
    "q": {"sum": {"field": "qty"}},
    "cum": {"cumulative_sum": {"buckets_path": "q"}},
    "d": {"derivative": {"buckets_path": "q", "gap_policy": "insert_zeros"}}}}
agg({"h": months, "best": {"max_bucket": {"buckets_path": "h>q"}}})
# q: 4, 0, 9     cum: 4, 4, 13     d: -, -4, 9     best: 9.0 (2026-03)
agg({"c": {"terms": {"field": "city"}, "aggs": {
    "q": {"sum": {"field": "qty"}},
    "big": {"bucket_selector": {"buckets_path": {"q": "q"}, "script": "params.q > 3"}}}}})
# Oslo (q 4) and Riga (q 8); Bern is dropped
```

The default `gap_policy` is `skip`: in the experiment, with an empty February and `skip`, the derivative had no value for February or for March.

## The best document of a group: `top_hits`

```python
r = agg({"c": {"terms": {"field": "city"}, "aggs": {
    "top": {"top_hits": {"size": 1, "sort": [{"price": "desc"}], "_source": ["product", "price"]}}}}})
# Oslo: desk 100 · Riga: lamp 30 · Bern: desk 120
```

## Walking all buckets: `composite`

`terms` returns only the top values, while `composite` pages through every combination by `after_key`.

```python
sources = [{"city": {"terms": {"field": "city"}}}, {"product": {"terms": {"field": "product"}}}]
after, pages = None, []
while True:
    comp = {"size": 3, "sources": sources}
    if after:
        comp["after"] = after
    res = agg({"c": {"composite": comp}})["c"]
    if not res["buckets"]:
        break
    pages.append([(b["key"]["city"], b["key"]["product"], b["doc_count"]) for b in res["buckets"]])
    after = res["after_key"]
```

## Accuracy of `terms`

Each shard sends its best `shard_size` values and the coordinator merges them. On 5 shards and 3000 documents with 60 values (random data with a fixed seed):

| Parameters | Top 5 | `doc_count_error_upper_bound` |
|---|---|---|
| exact count | k00 230, k01 147, k02 116, k03 98, k05 92 | — |
| `size 5, shard_size 5` | k00 224, k01 137, k02 81, k05 57, k03 44 | 110 |
| `size 5` (default) | k00 230, k01 145, k02 109, k03 98, k04 86 | 63 |
| `size 5, shard_size 100` | matches the exact one | 0 |

With the defaults `k04` made it into the top five instead of `k05`. `show_term_doc_count_error: true` shows the error estimate per bucket.

## Accuracy of `cardinality`

50,000 distinct values on one shard:

| `precision_threshold` | Result |
|---|---|
| 100 | 51394 |
| 3000 | 50741 |
| 40000 | 49929 |
| default | 50741 |

## Common mistakes

- Presenting an approximate top as exact.
- Setting a huge `size` on `terms` instead of using `composite`.
- Ordering `terms` by ascending count: the rarest values differ between shards and the error is unbounded.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [01 · Basics](../01-opensearch-basics/advanced.en.md) — routing and shards
- [06 · Relevance](../06-relevance-tuning/overview.en.md)

## Sources

- [Pipeline aggregations](https://docs.opensearch.org/latest/aggregations/pipeline/)
- [Composite aggregation](https://docs.opensearch.org/latest/aggregations/bucket/composite/)
- [Cardinality](https://docs.opensearch.org/latest/aggregations/metric/cardinality/)
