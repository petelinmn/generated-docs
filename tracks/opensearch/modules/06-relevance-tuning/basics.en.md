---
title: "Relevance: fundamentals"
lang: en
track: opensearch
kind: module
doc: basics
slug: 06-relevance-tuning
part: 2
tags: [relevance, bm25, boosting, function-score, synonyms]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Relevance: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Data for the examples

```python
from opensearchpy import helpers

client.indices.create(index="art", body={
    "settings": {"number_of_shards": 1, "number_of_replicas": 0, "analysis": {
        "filter": {"syn": {"type": "synonym_graph", "synonyms": ["laptop, notebook computer", "mice => mouse"]}},
        "analyzer": {"search_syn": {"type": "custom", "tokenizer": "standard", "filter": ["lowercase", "syn"]}}}},
    "mappings": {"properties": {"title": {"type": "text", "search_analyzer": "search_syn"},
        "body": {"type": "text"}, "views": {"type": "integer"}, "published": {"type": "date"}}}})
rows = [
    ("1", "Search engines", "search search search", 10, "2026-11-01"),
    ("2", "Search", "a very long article about many things where search appears just once among lots of other words here", 1000, "2026-06-01"),
    ("3", "Cooking", "search for the best recipe", 5000, "2025-01-01"),
    ("4", "Laptop review", "the laptop is fine", 50, "2026-11-20"),
    ("5", "Notebook computer deals", "cheap notebook computer", 70, "2026-10-01"),
    ("6", "Paper notebook", "a notebook for writing", 20, "2026-10-01"),
    ("7", "Mouse guide", "choose a mouse", 30, "2026-09-01"),
]
fields = ("title", "body", "views", "published")
helpers.bulk(client, [{"_index": "art", "_id": i, **dict(zip(fields, rest))} for i, *rest in rows], refresh=True)

def search(query, **extra):
    r = client.search(index="art", body={"query": query, **extra})
    return [(h["_id"], round(h["_score"], 3)) for h in r["hits"]["hits"]]

body_q = {"match": {"body": "search"}}
```

## BM25

```python
search(body_q)      # [('1', 0.657), ('3', 0.396), ('2', 0.2)]
```

Document 1 comes first: the word occurs three times in a short field. Document 2 comes last: one occurrence in a long field. Three factors: term rarity (idf), saturating frequency and field length relative to the average.

## Several fields

```python
base = {"query": "search", "fields": ["title", "body"]}
search({"multi_match": base})                              # [('1', 0.657), ('2', 0.652), ('3', 0.396)]  best_fields
search({"multi_match": {**base, "type": "most_fields"}})   # [('1', 1.17), ('2', 0.852), ('3', 0.396)]
search({"multi_match": {**base, "tie_breaker": 0.3}})      # [('1', 0.811), ('2', 0.712), ('3', 0.396)]
search({"multi_match": {**base, "fields": ["title^5", "body"]}})  # [('2', 3.259), ('1', 2.563), ('3', 0.396)]
```

| `type` | How scores are combined |
|---|---|
| `best_fields` (default) | takes the best field; `tie_breaker` adds a share of the others |
| `most_fields` | sums the fields |
| `cross_fields` | treats the fields as one |

`title^5` multiplies the title's contribution by 5 — and the first two documents swap.

## Raising and lowering

```python
search({"bool": {"must": body_q, "should": {"range": {"published": {"gte": "2026-10-01", "boost": 2}}}}})
# [('1', 2.657), ('3', 0.396), ('2', 0.2)]     fresh documents get a bonus
search({"boosting": {"positive": body_q, "negative": {"match": {"body": "recipe"}}, "negative_boost": 0.1}})
# [('1', 0.657), ('2', 0.2), ('3', 0.04)]      pushed down, not removed
```

## Common beginner mistakes

- Comparing `_score` across queries: the score is not normalised.
- Excluding with `must_not` what should only be demoted.
- Tuning weights on a single query.

## Related materials

- [Advanced theory](advanced.en.md)
- [03 · Search queries](../03-search-queries/expert.en.md)

## Sources

- [Multi-match query](https://docs.opensearch.org/latest/query-dsl/full-text/multi-match/)
- [Boosting query](https://docs.opensearch.org/latest/query-dsl/compound/boosting/)
