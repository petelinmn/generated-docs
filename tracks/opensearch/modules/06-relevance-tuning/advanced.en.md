---
title: "Relevance: advanced theory"
lang: en
track: opensearch
kind: module
doc: advanced
slug: 06-relevance-tuning
part: 2
tags: [relevance, bm25, boosting, function-score, synonyms]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Relevance: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

The data and the `search` function are from the [fundamentals](basics.en.md).

## `function_score`

```python
fvf = lambda **f: {"function_score": {"query": body_q, "field_value_factor": {"field": "views", **f}}}
search(fvf())                      # [('3', 1980.068), ('2', 199.922), ('1', 6.574)]
search(fvf(modifier="log1p"))      # [('3', 1.465), ('1', 0.685), ('2', 0.6)]
search({"function_score": {"query": body_q, "boost_mode": "sum",
                           "field_value_factor": {"field": "views", "modifier": "log1p"}}})
# [('3', 4.095), ('2', 3.2), ('1', 1.699)]
```

Without a modifier the view count became a multiplier and fully decided the order. `log1p` smooths it; `boost_mode` decides how the function combines with the query score (multiplication by default).

## Decay and a custom formula

```python
search({"function_score": {"query": body_q, "functions": [
    {"gauss": {"published": {"origin": "2026-11-25", "scale": "30d", "decay": 0.5}}}]}})
# [('1', 0.422), ('2', 0.0), ('3', 0.0)]    old documents are almost zeroed
search({"script_score": {"query": body_q, "script": {"source": "_score * Math.log(2 + doc['views'].value)"}}})
# [('3', 3.373), ('1', 1.634), ('2', 1.381)]
search({"script_score": {"query": body_q, "script": {"source": "-1"}}})      # error 400
```

A negative score is forbidden. Fix `origin` in tests: with `now` the result changes over time.

## `rescore`

An expensive query is applied only to the top `window_size` documents of each shard.

```python
search(body_q, rescore={"window_size": 2, "query": {
    "rescore_query": {"match_phrase": {"body": "best recipe"}},
    "query_weight": 1, "rescore_query_weight": 10}})
# [('3', 16.434), ('1', 0.657), ('2', 0.2)]
```

Document 3 was second, so it fell inside the window of two and moved to the top. A document outside the window would not have been rescored.

## Synonyms

```python
search({"match": {"title": "laptop"}})              # [('5', 1.03), ('4', 0.738)]
search({"match": {"title": "notebook computer"}})   # [('5', 1.03), ('4', 0.738)]   not "Paper notebook"
search({"match": {"title": "notebook"}})            # [('6', 0.513), ('5', 0.422)]
search({"match": {"title": "mice"}})                # [('7', 0.738)]
```

- `a, b` — equivalent;
- `a => b` — replace left with right;
- `synonym_graph` handles multi-word synonyms correctly and is meant for query analysis.

Synonyms in a `search_analyzer` need no reindexing of documents.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [02 · Mappings and analysis](../02-mappings-and-analysis/advanced.en.md)

## Sources

- [Function score query](https://docs.opensearch.org/latest/query-dsl/compound/function-score/)
- [Script score query](https://docs.opensearch.org/latest/query-dsl/specialized/script-score/)
- [Synonym graph token filter](https://docs.opensearch.org/latest/analyzers/token-filters/synonym-graph/)
