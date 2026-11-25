---
title: "Search queries: fundamentals"
lang: en
track: opensearch
kind: module
doc: basics
slug: 03-search-queries
part: 1
tags: [query-dsl, full-text-search, bool-query, relevance, bm25, pagination]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Search queries: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Data for the examples

```python
from opensearchpy import helpers

client.indices.create(index="shop", body={"settings": {"number_of_shards": 1, "number_of_replicas": 0},
    "mappings": {"properties": {
        "name": {"type": "text", "analyzer": "english", "fields": {"raw": {"type": "keyword"}}},
        "description": {"type": "text", "analyzer": "english"},
        "category": {"type": "keyword"}, "price": {"type": "double"},
        "stock": {"type": "integer"}, "created": {"type": "date"}}}})

rows = [
    ("1", "Wireless Mouse", "Ergonomic wireless mouse with silent buttons", "electronics", 25.0, 10, "2026-01-10"),
    ("2", "Wired Keyboard", "Mechanical keyboard with a wired connection", "electronics", 70.0, 0, "2026-02-05"),
    ("3", "Wireless Keyboard", "Compact keyboard, wireless and quiet", "electronics", 55.0, 3, "2026-03-20"),
    ("4", "Coffee Mug", "Ceramic mug for coffee and tea", "kitchen", 9.5, 100, "2025-12-01"),
    ("5", "Mouse Pad", "Soft pad for your mouse", "accessories", 7.0, 50, "2026-03-01"),
]
fields = ("name", "description", "category", "price", "stock", "created")
helpers.bulk(client, [{"_index": "shop", "_id": i, **dict(zip(fields, rest))} for i, *rest in rows], refresh=True)

def search(**body):
    r = client.search(index="shop", body=body)
    return [(h["_id"], h["_score"] and round(h["_score"], 3)) for h in r["hits"]["hits"]]
```

## Full-text queries

```python
search(query={"match": {"name": "wireless mouse"}})
# [('1', 0.796), ('3', 0.398), ('5', 0.398)]            any of the words
search(query={"match": {"name": {"query": "wireless mouse", "operator": "and"}}})
# [('1', 0.796)]                                        all words
search(query={"match": {"name": "keyboards"}})
# [('2', 0.398), ('3', 0.398)]                          stemmed
search(query={"match_phrase": {"description": "wireless mouse"}})
# [('1', 0.738)]                                        words in order
search(query={"match_phrase": {"description": {"query": "mouse silent", "slop": 1}}})
# [('1', 0.604)]                                        one gap allowed; with slop 0: []
search(query={"multi_match": {"query": "mouse", "fields": ["name^3", "description"]}})
# [('1', 1.194), ('5', 1.194)]                          name weighs three times more
search(query={"match": {"name": {"query": "keybord", "fuzziness": "AUTO"}}})
# [('2', 0.341), ('3', 0.341)]                          typo tolerated
```

## Term-level queries

```python
search(query={"term": {"category": "kitchen"}})                      # [('4', 1.0)]
search(query={"term": {"name": "Wireless"}})                         # []  text is lowercased in the index
search(query={"terms": {"category": ["kitchen", "accessories"]}})    # ids 4, 5
search(query={"range": {"price": {"gte": 25, "lt": 70}}})            # ids 1, 3
search(query={"range": {"created": {"gte": "2026-03-01||-1M", "lt": "2026-03-01"}}})  # id 2
search(query={"prefix": {"name.raw": "Wire"}})                       # ids 1, 2, 3
search(query={"wildcard": {"name.raw": "*Key*"}})                    # ids 2, 3
search(query={"ids": {"values": ["2", "4"]}})                        # ids 2, 4
```

## Combining: `bool`

| Clause | Must match | Affects `_score` |
|---|---|---|
| `must` | yes | yes |
| `filter` | yes | no |
| `should` | depends on `minimum_should_match` | yes |
| `must_not` | must not | no |

```python
search(query={"bool": {"must": [{"match": {"description": "keyboard"}}],
    "filter": [{"term": {"category": "electronics"}}, {"range": {"stock": {"gt": 0}}}]}})
# [('3', 0.406)]
search(query={"bool": {"filter": [{"term": {"category": "electronics"}}]}})     # ids 1, 2, 3 with score 0.0
search(query={"bool": {"must_not": [{"term": {"category": "electronics"}}]}})   # ids 4, 5 with score 0.0
```

## Sorting, fields and pages

```python
search(sort=[{"price": "asc"}])                  # ids 5, 4, 1, 3, 2; _score is None
search(sort=[{"name.raw": "desc"}], size=3)      # ids 1, 3, 2
search(**{"from": 2, "size": 2, "sort": [{"price": "asc"}]})   # ids 1, 3
```

By default results are ordered by `_score` and 10 documents are returned. When sorting by a field the score is not computed.

## Highlighting and field selection

```python
r = client.search(index="shop", body={"query": {"match": {"description": "mouse"}},
    "_source": ["name", "price"], "highlight": {"fields": {"description": {}}}})
# highlights: 'Soft pad for your <em>mouse</em>',
#             'Ergonomic wireless <em>mouse</em> with silent buttons'
```

## Common beginner mistakes

- `term` on a `text` field.
- Putting every condition into `must`, exact filters included.
- Confusing word order: `match` ignores it, `match_phrase` respects it.
- Paging far with `from`.

## Related materials

- [Advanced theory](advanced.en.md)
- [02 · Mappings and analysis](../02-mappings-and-analysis/basics.en.md)

## Sources

- [Query DSL](https://opensearch.org/docs/latest/query-dsl/)
- [Boolean query](https://opensearch.org/docs/latest/query-dsl/compound/bool/)
- [Match query](https://opensearch.org/docs/latest/query-dsl/full-text/match/)
