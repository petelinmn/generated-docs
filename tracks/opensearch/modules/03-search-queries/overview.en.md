---
title: "Search queries"
lang: en
track: opensearch
kind: module
doc: overview
slug: 03-search-queries
part: 1
tags: [query-dsl, full-text-search, bool-query, relevance, bm25, pagination]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Search queries

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

**Query DSL** is OpenSearch's JSON query language. Queries are either **full-text** (they analyze the string) or **term-level** (they look for the value as is). `bool` combines them. In query context a relevance `_score` is computed, in filter context it is not.

## Key ideas

- `match` for text, `term` for `keyword`, numbers and dates.
- Put into `filter` whatever must not affect ranking.
- `should` without `must` or `filter` requires at least one match; next to `filter` it does not.
- Do deep pagination with `search_after`, not `from`.

## Minimal example

```python
client.search(index="shop", body={"query": {"bool": {
    "must": [{"match": {"description": "keyboard"}}],
    "filter": [{"term": {"category": "electronics"}}, {"range": {"stock": {"gt": 0}}}]}}})
```

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn opensearch 03-search-queries level 5` |
| 6. Tests | — | `/learn opensearch 03-search-queries level 6` |

## Related materials

- [OpenSearch track](../../overview.en.md)
- [02 · Mappings and text analysis](../02-mappings-and-analysis/overview.en.md)
- [01 · OpenSearch basics](../01-opensearch-basics/overview.en.md)
- [04 · Aggregations](../04-aggregations/overview.en.md)
- [06 · Relevance](../06-relevance-tuning/overview.en.md)
