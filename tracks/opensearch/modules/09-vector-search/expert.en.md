---
title: "Vector search: expert theory"
lang: en
track: opensearch
kind: module
doc: expert
slug: 09-vector-search
part: 2
tags: [vector-search, knn, embeddings, hybrid-search, hnsw]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Vector search: expert theory

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

`search` and `hybrid` come from the [fundamentals](basics.en.md) and [advanced theory](advanced.en.md).

## Engines and parameters

| Parameter | Default | Values and meaning |
|---|---|---|
| `engine` | `faiss` | `faiss`, `lucene`; `nmslib` is rejected for new indices since 3.0.0 |
| `method.name` | `hnsw` | `hnsw`, `ivf` (faiss, needs training) |
| `space_type` | `l2` | `l2`, `cosinesimil`, `innerproduct`, `l1`, `linf`, `hamming` — not all with every engine |
| `m` | 16 | links per graph node |
| `ef_construction` | 100 | build accuracy |
| `ef_search` | 100 | search accuracy (faiss); lucene uses `k` |
| `dimension` | — | 1–16 000 |
| `data_type` | `float` | `float`, `byte` (−128…127), `binary` |

## Memory

The documented HNSW estimate is `1.1 * (4 * dimension + 8 * m)` bytes per vector. For a million 256-dimension vectors with `m = 16` that is about 1.27 GB, replicas not included.

| Setting | Default |
|---|---|
| `knn.memory.circuit_breaker.enabled` | `true` |
| `knn.memory.circuit_breaker.limit` | `50%` |
| `knn.algo_param.index_thread_qty` | `1` |

`GET /_plugins/_knn/stats` shows `graph_memory_usage`, graph cache hits and misses and `circuit_breaker_triggered`.

## Compression

A field can be declared with `"mode": "on_disk"` and `"compression_level": "32x"`: vectors are quantised and candidates are rescored with full values from disk.

> ⚠️ Verify: only creating such a mapping was checked here, not quality or memory use; `ivf`, PQ, binary vectors and lucene-specific limits were not run.

## Embeddings inside the cluster

The ML Commons and neural search plugins can host or connect a model: an ingest pipeline computes the vector and the `neural` query accepts text.

> ⚠️ Verify: this route is described from the documentation and was not run.

## A hybrid query with no pipeline

```python
search(hybrid)       # no search pipeline
# [('1', -9549512000.0), ('1', -4422440400.0), ('1', 0.468), ('6', 0.468), ('1', -4422440400.0),
#  ('3', 0.9804), ('4', 0.9174), ('2', 0.5051), ('1', -9549512000.0)]
```

The huge negative numbers are internal delimiters marking the subquery lists in shard results; the normalisation or RRF processor normally removes them. Always name a pipeline.

## Gotchas

1. `post_filter` and an outer `bool.filter` returned nothing where a filter inside `knn` found two documents.
2. `k` together with `min_score`: `requires exactly one of k, distance or score`.
3. `hits.total` equals the neighbours found (5 with `size: 2`, `k: 5`), not the number of "relevant" documents.
4. k-NN has no "nothing found": there is always a nearest one, however unlike.
5. Weights not summing to 1 are rejected when the pipeline is created.
6. `min_max` gives the worst of a list almost zero (document `2`: 0.0007) although its raw score was 0.5.

## Interview questions

1. What is an embedding?
<details><summary>Answer</summary>

A vector from a model in which closeness reflects similarity.
</details>

2. Exact vs approximate k-NN?
<details><summary>Answer</summary>

Brute force vs a graph or clusters: speed traded for recall.
</details>

3. What is HNSW?
<details><summary>Answer</summary>

A layered proximity graph; `m` is connectivity, `ef_construction` and `ef_search` the candidate list width at build and query time.
</details>

4. How do you save memory?
<details><summary>Answer</summary>

Fewer dimensions, quantisation (16-bit, byte, binary, PQ), `on_disk` mode — at the cost of accuracy or latency.
</details>

5. Why can a filter return fewer than k?
<details><summary>Answer</summary>

When it runs after the neighbour search.
</details>

6. Cosine or inner product?
<details><summary>Answer</summary>

Cosine ignores length; for normalised vectors the ordering is the same.
</details>

7. How do you combine BM25 and vectors?
<details><summary>Answer</summary>

`hybrid` with normalisation and weights, or RRF.
</details>

8. When is RRF better than normalisation?
<details><summary>Answer</summary>

When score distributions are incomparable or have outliers and there is no data to tune weights.
</details>

9. How do you measure recall?
<details><summary>Answer</summary>

Compare approximate results with exact `script_score` ones on a sample of queries.
</details>

10. What do you do when the model changes?
<details><summary>Answer</summary>

A new index, reindex with new vectors, switch the alias.
</details>

11. What is the k-NN circuit breaker?
<details><summary>Answer</summary>

A cap on memory for native graphs; `knn.memory.circuit_breaker.limit` is 50% by default.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [07 · Cluster and shards](../07-cluster-and-shards/advanced.en.md)

## Sources

- [Methods and engines](https://docs.opensearch.org/latest/mappings/supported-field-types/knn-methods-engines/)
- [Approximate k-NN search](https://docs.opensearch.org/latest/vector-search/vector-search-techniques/approximate-knn/)
- [Normalization processor](https://docs.opensearch.org/latest/search-plugins/search-pipelines/normalization-processor/)
- [Vector quantization](https://docs.opensearch.org/latest/vector-search/optimizing-storage/knn-vector-quantization/)
