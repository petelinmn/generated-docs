---
title: "Search queries: advanced theory"
lang: en
track: opensearch
kind: module
doc: advanced
slug: 03-search-queries
part: 1
tags: [query-dsl, full-text-search, bool-query, relevance, bm25, pagination]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Search queries: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

The data and the `search` function are from the [fundamentals](basics.en.md).

## `should` and `minimum_should_match`

```python
mouse, wireless, keyboard = ({"match": {"name": w}} for w in ("mouse", "wireless", "keyboard"))
search(query={"bool": {"should": [mouse, keyboard]}})                  # ids 1, 2, 3, 5
search(query={"bool": {"filter": [{"term": {"category": "electronics"}}], "should": [mouse]}})
# [('1', 0.398), ('2', 0.0), ('3', 0.0)]    should only boosts here
search(query={"bool": {"should": [mouse, wireless, keyboard], "minimum_should_match": 2}})   # ids 1, 3
search(query={"constant_score": {"filter": {"term": {"category": "electronics"}}, "boost": 2}})  # score 2.0
```

## Deep pagination: `search_after`

`from + size` is limited by `index.max_result_window` (10000). `search_after` continues from the last sort value; the sort must order documents unambiguously.

```python
r = client.search(index="shop", body={"sort": [{"price": "asc"}, {"_id": "asc"}], "size": 2})
last = r["hits"]["hits"][-1]["sort"]          # [9.5, '4']     ids 5, 4
search(sort=[{"price": "asc"}, {"_id": "asc"}], size=2, search_after=last)   # ids 1, 3
```

Sorting by `_id` is used here for clarity; at scale keep a separate unique `keyword` field.

## A consistent snapshot: PIT

A **point in time** freezes the state of the index for the duration of a traversal.

```python
pit = client.transport.perform_request("POST", "/shop/_search/point_in_time", params={"keep_alive": "1m"})
client.index(index="shop", id="6", body={"name": "New Mouse", "category": "electronics", "price": 1},
             params={"refresh": "true"})
r = client.search(body={"pit": {"id": pit["pit_id"], "keep_alive": "1m"}, "sort": [{"price": "asc"}]})
# ids 5, 4, 1, 3, 2  - the new document is not visible in the PIT
# a normal search returns 6, 5, 4, 1, 3, 2
client.transport.perform_request("DELETE", "/_search/point_in_time", body={"pit_id": [pit["pit_id"]]})
client.delete(index="shop", id="6", params={"refresh": "true"})
```

The older way is `scroll`; it is not meant for user-facing pagination, and its context must be closed.

## A query string typed by a user

```python
search(query={"query_string": {"query": "category:electronics AND price:[20 TO 60]"}})   # ids 1, 3
search(query={"query_string": {"query": "name:(mouse"}})            # 400 query_shard_exception
search(query={"simple_query_string": {"query": "name:(mouse", "fields": ["name"]}})   # ids 1, 5, no error
```

`query_string` gives the full syntax and fails on a mistake in it; for user input use `match` or `simple_query_string`.

## `post_filter`: facets

`post_filter` cuts results **after** aggregations, so category counts stay complete.

```python
r = client.search(index="shop", body={"query": {"match": {"name": "mouse"}},
    "post_filter": {"term": {"category": "accessories"}},
    "aggs": {"c": {"terms": {"field": "category"}}}})
# hits: only id 5; buckets: accessories 1, electronics 1
```

## Number of matches

`hits.total` is exact up to 10000, beyond that `"relation": "gte"` is returned. `track_total_hits` sets the threshold; with 2 the response was `{"value": 2, "relation": "gte"}`. If you need only the number, use `_count`.

## Common mistakes

- `should` next to `filter` without `minimum_should_match` when a match was meant to be required.
- A `wildcard` with a leading asterisk on large indexes.
- `query_string` fed with user input.
- Not closing PIT and `scroll`.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [01 · Basics](../01-opensearch-basics/expert.en.md) — the two phases of search
- [04 · Aggregations](../04-aggregations/overview.en.md)
- [06 · Relevance](../06-relevance-tuning/overview.en.md)
- [09 · Vector and hybrid search](../09-vector-search/overview.en.md)

## Sources

- [Paginate results](https://opensearch.org/docs/latest/search-plugins/searching-data/paginate/)
- [Point in time](https://opensearch.org/docs/latest/search-plugins/searching-data/point-in-time/)
- [Query string query](https://opensearch.org/docs/latest/query-dsl/full-text/query-string/)
