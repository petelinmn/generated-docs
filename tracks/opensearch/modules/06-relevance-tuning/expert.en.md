---
title: "Relevance: expert theory"
lang: en
track: opensearch
kind: module
doc: expert
slug: 06-relevance-tuning
part: 2
tags: [relevance, bm25, boosting, function-score, synonyms]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Relevance: expert theory

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Dissecting the score

The formula from the `_explain` response (a `match` on `body`):

`score = boost * idf * tf`, `idf = log(1 + (N - n + 0.5) / (n + 0.5))`, `tf = freq / (freq + k1 * (1 - b + b * dl / avgdl))`

```text
document 1:  idf 0.8267 (n=3, N=7)   tf 0.7952 (freq 3, dl 3,  avgdl 5.71)   score 0.6574
document 2:  idf 0.8267 (n=3, N=7)   tf 0.2418 (freq 1, dl 18, avgdl 5.71)   score 0.1999
```

## Similarity settings

The defaults are `k1 = 1.2`, `b = 0.75`. They can be set per field:

```python
client.indices.create(index="art2", body={
    "settings": {"number_of_shards": 1, "number_of_replicas": 0, "similarity": {"nolen": {"type": "BM25", "b": 0}}},
    "mappings": {"properties": {"body": {"type": "text", "similarity": "nolen"}}}})
# the same seven bodies, the same match query:
# default: [('1', 0.657), ('3', 0.396), ('2', 0.2)]
# b = 0:   [('1', 0.59), ('2', 0.376), ('3', 0.376)]
```

## Shard statistics

By default (`query_then_fetch`) each shard uses its own idf. In a three-shard index documents `0` and `1` with identical text got 1.133 and 1.032; with `search_type=dfs_query_then_fetch` both got 1.109. It costs an extra round trip to the shards.

## Measuring quality

```python
r = client.transport.perform_request("POST", "/art/_rank_eval", body={
    "requests": [{"id": "q1", "request": {"query": body_q},
                  "ratings": [{"_index": "art", "_id": "2", "rating": 1}]}],
    "metric": {"mean_reciprocal_rank": {"k": 3}}})
print(r["metric_score"])     # 0.333...: the relevant document is third
```

With ratings saying only document 2 is relevant, precision@3 is 0.333 as well.

## Tools

| Tool | For | Verified |
|---|---|---|
| `_explain`, `"explain": true` | score computation; for a non-match `no matching term` | yes |
| `_name` | which clauses matched | yes |
| `_rank_eval` | metrics over rated queries | yes |
| `_analyze` | terms and positions | yes |
| `rank_feature` | an efficient numeric signal; needs a `rank_feature` field | only the rejection on `integer` |
| Learning to Rank | a ranking model via a plugin | ⚠️ |
| Search pipelines | server-side processing of requests and results | ⚠️ |

> ⚠️ Verify: Learning to Rank and search pipelines are mentioned from the documentation and were not run.

## Gotchas

1. A raw `field_value_factor` gave a score of 1980: the text stopped affecting the order.
2. `gauss` with `scale: 30d` nearly zeroed old documents' scores — and multiplying by zero erases text relevance.
3. A script with a negative result is a 400 error.
4. `rescore` sees only its window: a candidate not selected cannot rise.
5. The synonym for `laptop` put "Notebook computer deals" above "Laptop review": query expansion changes ordering, not only recall.

## Interview questions

1. What does BM25 consist of?
<details><summary>Answer</summary>

idf, saturating term frequency (`k1`), length normalisation (`b`).
</details>

2. What does `b = 0` do?
<details><summary>Answer</summary>

Turns off the influence of field length.
</details>

3. `best_fields` versus `most_fields`?
<details><summary>Answer</summary>

The first takes the maximum over fields, the second sums them.
</details>

4. What is `tie_breaker` for?
<details><summary>Answer</summary>

So that matches in the other fields count for something too.
</details>

5. How do you use popularity without drowning the text?
<details><summary>Answer</summary>

A logarithm or saturating function, addition instead of multiplication, a cap on the contribution.
</details>

6. What is `rescore`?
<details><summary>Answer</summary>

Re-ranking each shard's top documents with an expensive query.
</details>

7. Synonyms at index time or search time?
<details><summary>Answer</summary>

At search time: no reindexing, and `synonym_graph` handles multi-word ones correctly.
</details>

8. Why are scores odd on a small index?
<details><summary>Answer</summary>

Per-shard statistics; `dfs_query_then_fetch` or a single shard helps.
</details>

9. How do you measure search quality?
<details><summary>Answer</summary>

Rated queries and precision@k, MRR, DCG via `_rank_eval`; in production, clicks and experiments.
</details>

10. What are named queries?
<details><summary>Answer</summary>

A `_name` on a query clause; `matched_queries` in the response shows what matched.
</details>

11. Can `_score`s of different queries be compared?
<details><summary>Answer</summary>

No, the score is not normalised.
</details>

12. What do you check when a wanted document ranks low?
<details><summary>Answer</summary>

`explain`, the analyzer's terms, field length, weights and functions.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [04 · Aggregations](../04-aggregations/basics.en.md)

## Sources

- [Similarity](https://docs.opensearch.org/latest/field-types/mapping-parameters/similarity/)
- [Ranking evaluation API](https://docs.opensearch.org/latest/api-reference/search-apis/rank-eval/)
- [Explain API](https://docs.opensearch.org/latest/api-reference/search-apis/explain/)
