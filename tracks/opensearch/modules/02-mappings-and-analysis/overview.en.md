---
title: "Mappings and text analysis"
lang: en
track: opensearch
kind: module
doc: overview
slug: 02-mappings-and-analysis
part: 1
tags: [mapping, analyzer, inverted-index, text-keyword, nested, dynamic-mapping]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-26
---

# Mappings and text analysis

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A **mapping** is the schema of an index: which fields exist and how to index them. For text an **analyzer** turns a string into terms that are later searched. A `text` field is searched by words, a `keyword` field only as a whole.

## Key ideas

- An analyzer = character filters → tokenizer → token filters.
- Dynamic mapping guesses types from the first document, and not always well.
- The type of an existing field cannot be changed — you need a new index.
- `term` does not analyze the query, `match` does.
- Arrays of objects lose the link between fields of one element unless declared `nested`.

## Minimal example

```python
client.indices.create(index="goods", body={"mappings": {"properties": {
    "name": {"type": "text", "analyzer": "english", "fields": {"raw": {"type": "keyword"}}},
    "category": {"type": "keyword"}, "price": {"type": "double"}}}})
```

## When it matters

- Whenever an index outlives an experiment: define the mapping explicitly.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn opensearch 02-mappings-and-analysis level 5` |
| 6. Tests | — | `/learn opensearch 02-mappings-and-analysis level 6` |

## Related materials

- [OpenSearch track](../../overview.en.md)
- [01 · OpenSearch basics](../01-opensearch-basics/overview.en.md)
- [03 · Search queries](../03-search-queries/overview.en.md)
- [04 · Aggregations](../04-aggregations/overview.en.md)
- [05 · Indexing at scale](../05-indexing-at-scale/overview.en.md)
- [06 · Relevance](../06-relevance-tuning/overview.en.md)
- [System design: choosing a store](../../../system-design/modules/04-data-storage/overview.en.md)
- [Indexes in PostgreSQL](../../../postgresql/modules/05-indexes/overview.en.md)
