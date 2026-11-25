---
title: "Vector and hybrid search"
lang: en
track: opensearch
kind: module
doc: overview
slug: 09-vector-search
part: 2
tags: [vector-search, knn, embeddings, hybrid-search, hnsw]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Vector and hybrid search

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Search by meaning over embeddings: the `knn_vector` field, the `knn` query, filtering and **hybrid search** combining words and vectors.

## Key ideas

- The dimension and the model are fixed for an index.
- HNSW is fast but approximate; exact search is `script_score`.
- A filter inside `knn` and one outside it are different things.
- BM25 and vector scores cannot be added directly: normalise or use RRF.
- Memory for the graphs is sized in advance.

## Minimal example

```python
client.search(index="items", body={"query": {"knn": {"v": {
    "vector": [0.85, 0.15, 0.0], "k": 3, "filter": {"term": {"category": "peripherals"}}}}}})
```

## When to use

Search by meaning, similar items, RAG. If users look up part numbers and exact names, start with full-text search.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn opensearch 09-vector-search level 5` |
| 6. Tests | — | `/learn opensearch 09-vector-search level 6` |

## Related materials

- [OpenSearch track](../../overview.en.md)
- [03 · Search queries](../03-search-queries/overview.en.md)
- [06 · Relevance](../06-relevance-tuning/overview.en.md)
