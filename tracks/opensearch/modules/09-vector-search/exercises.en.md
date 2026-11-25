---
title: "Exercises: vector search"
lang: en
track: opensearch
kind: module
doc: exercises
slug: 09-vector-search
part: 2
tags: [vector-search, knn, embeddings, hybrid-search, hnsw]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Exercises: vector search

🌐 [Русский](exercises.ru.md) · **English**

> You need the data, `search` and `knn` from the [fundamentals](basics.en.md).

## 1. Nearest food

Find the two neighbours of `[0, 0, 1]`.

<details><summary>Hint</summary>

`knn(vector, k=2)`.

</details>

<details><summary>Solution</summary>

```python
print(search(knn([0.0, 0.0, 1.0], k=2)))
```

</details>

## 2. Cheap neighbours

The three nearest to `Q` costing under 30.

<details><summary>Hint</summary>

A filter inside `knn`.

</details>

<details><summary>Solution</summary>

```python
print(search(knn(k=3, filter={"range": {"price": {"lt": 30}}})))
```

</details>

## 3. Compare with exact

Write a recall function: the share of the exact top 3 found by the approximate top 3.

<details><summary>Hint</summary>

`script_score` and sets.

</details>

<details><summary>Solution</summary>

```python
def recall(vector, k=3):
    exact = {"size": k, "query": {"script_score": {"query": {"match_all": {}}, "script": {
        "lang": "knn", "source": "knn_score",
        "params": {"field": "v", "query_value": vector, "space_type": "l2"}}}}}
    truth = {i for i, _ in search(exact)}
    found = {i for i, _ in search(knn(vector, k=k))}
    return len(truth & found) / len(truth)

print(recall(Q))
```

</details>

## 4. Half and half

Hybrid search with equal weights.

<details><summary>Hint</summary>

A pipeline with `[0.5, 0.5]`.

</details>

<details><summary>Solution</summary>

```python
client.transport.perform_request("PUT", "/_search/pipeline/half", body={"phase_results_processors": [
    {"normalization-processor": {"normalization": {"technique": "min_max"},
     "combination": {"technique": "arithmetic_mean", "parameters": {"weights": [0.5, 0.5]}}}}]})
print(search({"query": {"hybrid": {"queries": [
    {"match": {"title": "mouse"}}, {"knn": {"v": {"vector": [0.0, 1.0, 0.0], "k": 3}}}]}}},
    search_pipeline="half"))
```

</details>

## 5. How much memory

Estimate memory for 2 million 768-dimension vectors, `m = 16`, with one replica.

<details><summary>Hint</summary>

The expert-level formula, times the number of copies.

</details>

<details><summary>Solution</summary>

```python
def hnsw_bytes(vectors, dimension, m=16, replicas=1):
    return 1.1 * (4 * dimension + 8 * m) * vectors * (1 + replicas)

print(round(hnsw_bytes(2_000_000, 768) / 1e9, 2), "GB")
```

</details>
