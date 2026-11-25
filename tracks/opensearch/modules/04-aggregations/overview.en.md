---
title: "Aggregations"
lang: en
track: opensearch
kind: module
doc: overview
slug: 04-aggregations
part: 1
tags: [aggregations, buckets, metrics, cardinality, composite-aggregation]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Aggregations

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

**Aggregations** compute summaries over the matched documents: metrics (sums, averages), **buckets** (groups) and calculations over buckets. They run with the query and respect it.

## Key ideas

- A metric returns a number, a bucket aggregation returns groups, and groups can hold nested aggregations.
- You can group by `keyword`, numbers and dates, not by `text`.
- `terms` over several shards is approximate; `doc_count_error_upper_bound` shows the error.
- `cardinality` is approximate too.
- To walk all groups there is `composite`.

## Minimal example

```python
r = client.search(index="sales", body={"size": 0, "aggs": {
    "by_city": {"terms": {"field": "city"}, "aggs": {"avg_price": {"avg": {"field": "price"}}}}}})
print(r["aggregations"]["by_city"]["buckets"])
```

## When to use

Catalog facets, dashboards, log statistics. For exact financial reports keep in mind that `terms` and `cardinality` are approximate.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn opensearch 04-aggregations level 5` |
| 6. Tests | — | `/learn opensearch 04-aggregations level 6` |

## Related materials

- [OpenSearch track](../../overview.en.md)
- [03 · Search queries](../03-search-queries/overview.en.md)
- [02 · Mappings and text analysis](../02-mappings-and-analysis/overview.en.md)
- [06 · Relevance](../06-relevance-tuning/overview.en.md)
