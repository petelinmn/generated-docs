---
title: "Vector search: advanced theory"
lang: en
track: opensearch
kind: module
doc: advanced
slug: 09-vector-search
part: 2
tags: [vector-search, knn, embeddings, hybrid-search, hnsw]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Vector search: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

The data, `search` and `knn` are from the [fundamentals](basics.en.md).

## Filtering

```python
furniture = {"term": {"category": "furniture"}}
search(knn(k=2, filter=furniture))                       # [('4', 0.4938), ('3', 0.4357)]
search({**knn(k=2), "post_filter": furniture})           # []
search({"query": {"bool": {"must": knn(k=2)["query"], "filter": furniture}}})    # []
```

A filter **inside** `knn` finds neighbours among the matching documents. `post_filter` and an outer `bool.filter` discard from the `k` neighbours already found — two peripherals here, leaving nothing.

## A threshold instead of k

```python
search(knn(max_distance=0.05))       # [('1', 0.995), ('6', 0.9852), ('2', 0.9569)]
search(knn(min_score=0.9))           # the same three
```

Exactly one of `k`, `max_distance`, `min_score` is given. For `l2` the distance here is the squared Euclidean one.

## Exact search

```python
exact = {"script_score": {"query": furniture, "script": {"lang": "knn", "source": "knn_score",
         "params": {"field": "v", "query_value": Q, "space_type": "l2"}}}}
search({"query": exact})             # [('4', 0.4938), ('3', 0.4357)]
```

It scores every document matching the query: exact, with a cost that grows with their number. Good for small sets and for measuring the recall of approximate search. It works without `index.knn` too.

## Space types

```python
for space in ("l2", "cosinesimil", "innerproduct"):
    make("s-" + space, {"name": "hnsw", "space_type": space, "engine": "faiss"})

search(knn(k=3), "s-l2")                       # [('1', 0.995), ('6', 0.9852), ('2', 0.9569)]
search(knn(k=3), "s-cosinesimil")              # [('1', 0.999), ('6', 0.9951), ('2', 0.9868)]
search(knn(k=3), "s-innerproduct")             # [('1', 1.78), ('6', 1.71), ('2', 1.64)]
long = [8.5, 1.5, 0]                           # the same direction, ten times longer
search(knn(long, k=3), "s-l2")                 # [('1', 0.0165), ('6', 0.0161), ('2', 0.0158)]
search(knn(long, k=3), "s-cosinesimil")        # [('1', 0.999), ('6', 0.9951), ('2', 0.9868)]
search(knn(long, k=3), "s-innerproduct")       # [('1', 8.8), ('6', 8.1), ('2', 7.4)]
```

`cosinesimil` ignores vector length, `l2` and `innerproduct` do not. Use the metric your model was trained for.

## Hybrid search

BM25 and k-NN scores live on different scales, so `bool.should` just mixes the numbers. The `hybrid` query with a search pipeline brings them to a common scale.

```python
text = {"match": {"title": "mouse"}}
vector = {"knn": {"v": {"vector": [0.0, 1.0, 0.0], "k": 3}}}      # "furniture"
search({"query": text})              # [('1', 0.468), ('6', 0.468)]
search({"query": vector})            # [('3', 0.9804), ('4', 0.9174), ('2', 0.5051)]

def pipeline(name, processor):
    client.transport.perform_request("PUT", "/_search/pipeline/" + name,
                                     body={"phase_results_processors": [processor]})

def norm(text_weight):
    return {"normalization-processor": {"normalization": {"technique": "min_max"},
            "combination": {"technique": "arithmetic_mean",
                            "parameters": {"weights": [text_weight, round(1 - text_weight, 2)]}}}}

pipeline("vector-heavy", norm(0.3))
pipeline("text-heavy", norm(0.7))
pipeline("rrf", {"score-ranker-processor": {"combination": {"technique": "rrf"}}})

hybrid = {"query": {"hybrid": {"queries": [text, vector]}}}
search(hybrid, search_pipeline="vector-heavy")  # [('3', 0.7), ('4', 0.6073), ('1', 0.3), ('6', 0.3), ('2', 0.0007)]
search(hybrid, search_pipeline="text-heavy")    # [('1', 0.7), ('6', 0.7), ('3', 0.3), ('4', 0.2603), ('2', 0.0003)]
search(hybrid, search_pipeline="rrf")           # [('1', 0.0164), ('3', 0.0164), ('4', 0.0161), ('6', 0.0161), ('2', 0.0159)]
```

- **Normalisation** (`min_max`) with weights: weights follow the subquery order and sum to 1.
- **RRF** (reciprocal rank fusion): adds `1 / (rank_constant + position)` and ignores raw scores; `rank_constant` is 60 by default.
- A pipeline can be the index default: `index.search.default_pipeline`.

## Common mistakes

- Running `hybrid` with no pipeline: the response holds internal marker values.
- Filtering outside and getting fewer than `k`.
- Tuning weights by eye instead of rated queries.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [06 · Relevance: measuring quality](../06-relevance-tuning/expert.en.md)

## Sources

- [Filtering in k-NN search](https://docs.opensearch.org/latest/vector-search/filter-search-knn/index/)
- [Exact k-NN with a scoring script](https://docs.opensearch.org/latest/vector-search/vector-search-techniques/knn-score-script/)
- [Hybrid search](https://docs.opensearch.org/latest/vector-search/ai-search/hybrid-search/index/)
- [Score ranker processor](https://docs.opensearch.org/latest/search-plugins/search-pipelines/score-ranker-processor/)
