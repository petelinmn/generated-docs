---
title: "Vector search: fundamentals"
lang: en
track: opensearch
kind: module
doc: basics
slug: 09-vector-search
part: 2
tags: [vector-search, knn, embeddings, hybrid-search, hnsw]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Vector search: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## The idea

Full-text search looks for matching words. Vector search looks for close meaning: a model turns text or an image into an array of numbers (an embedding) and similar things end up near each other. OpenSearch stores the vectors and finds the nearest ones (k-NN).

Here the three-number vectors are made up by hand, so results are predictable and no model is needed. Real embeddings have hundreds or thousands of dimensions and come from a trained model.

## The index

You need the `index.knn` setting and a `knn_vector` field with an exact dimension.

```python
from opensearchpy import OpenSearch
from opensearchpy.exceptions import RequestError

client = OpenSearch(hosts=[{"host": "localhost", "port": 9200}])

docs = [  # id, title, category, price, toy vector: [peripherals-ness, furniture-ness, food-ness]
    ("1", "wireless mouse", "peripherals", 20, [0.9, 0.1, 0.0]),
    ("2", "wired keyboard", "peripherals", 35, [0.7, 0.3, 0.0]),
    ("3", "office chair", "furniture", 120, [0.0, 0.9, 0.1]),
    ("4", "standing desk", "furniture", 300, [0.1, 0.8, 0.2]),
    ("5", "coffee beans", "food", 15, [0.0, 0.1, 0.9]),
    ("6", "mouse pad", "peripherals", 8, [0.8, 0.2, 0.1]),
]

def make(name, method=None):
    field = {"type": "knn_vector", "dimension": 3}
    if method:
        field["method"] = method
    client.indices.create(index=name, body={
        "settings": {"index.knn": True, "number_of_shards": 1, "number_of_replicas": 0},
        "mappings": {"properties": {"title": {"type": "text"}, "category": {"type": "keyword"},
                                    "price": {"type": "integer"}, "v": field}}})
    for i, title, category, price, v in docs:
        client.index(index=name, id=i, body={"title": title, "category": category, "price": price, "v": v})
    client.indices.refresh(index=name)

make("items")

def search(body, index="items", **params):
    r = client.search(index=index, body=body, params=params)
    return [(h["_id"], round(h["_score"], 4)) for h in r["hits"]["hits"]]

Q = [0.85, 0.15, 0.0]          # "something for the computer"

def knn(vector=Q, **kw):
    return {"query": {"knn": {"v": {"vector": vector, **kw}}}}
```

## Finding the nearest

```python
search(knn(k=3))                     # [('1', 0.995), ('6', 0.9852), ('2', 0.9569)]
search({"size": 2, **knn(k=5)})      # [('1', 0.995), ('6', 0.9852)]
```

`k` is how many neighbours to find, `size` how many to return. With the default metric (`l2`) the score is `1 / (1 + distance²)`: closer means higher, 1 at most.

## The dimension is strict

```python
try:
    search(knn([1, 0], k=2))
except RequestError as e:
    print(e.error)                   # search_phase_execution_exception: Dimension should be: 3
```

Both documents and queries must carry exactly as many numbers as the mapping says. Query and document vectors must come from **the same model**.

## Approximate search

Comparing a query with every vector is expensive, so by default an HNSW graph is built: search is fast but approximate and may occasionally miss a true neighbour.

## Common beginner mistakes

- Forgetting `index.knn: true`: a `knn` query is rejected (`not built for ANN search`).
- Switching the model without reindexing the documents.
- Reading the score as a probability or comparing it across metrics.

## Related materials

- [Advanced theory](advanced.en.md)
- [06 · Relevance](../06-relevance-tuning/basics.en.md)

## Sources

- [Vector search](https://docs.opensearch.org/latest/vector-search/)
- [k-NN vector field type](https://docs.opensearch.org/latest/mappings/supported-field-types/knn-vector/)
