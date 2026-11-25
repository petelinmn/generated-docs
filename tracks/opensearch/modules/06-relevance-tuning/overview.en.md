---
title: "Relevance"
lang: en
track: opensearch
kind: module
doc: overview
slug: 06-relevance-tuning
part: 2
tags: [relevance, bm25, boosting, function-score, synonyms]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Relevance

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

How to control the order of results: the **BM25** score, field weights, combining the text score with popularity and recency, **synonyms** and measuring quality.

## Key ideas

- Understand the score with `explain` first, then change it.
- Field boosts and the `multi_match` type solve most tasks.
- A raw numeric field as a multiplier easily drowns out the text.
- Synonyms are more convenient applied at search time.
- Changes are checked against a set of rated queries.

## Minimal example

```python
client.search(index="art", body={"query": {"function_score": {
    "query": {"multi_match": {"query": "search", "fields": ["title^3", "body"]}},
    "field_value_factor": {"field": "views", "modifier": "log1p"},
    "boost_mode": "sum"}}})
```

## When to use

When results are "technically right but unhelpful". Do not turn knobs without a measurable goal.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn opensearch 06-relevance-tuning level 5` |
| 6. Tests | — | `/learn opensearch 06-relevance-tuning level 6` |

## Related materials

- [OpenSearch track](../../overview.en.md)
- [03 · Search queries](../03-search-queries/overview.en.md)
- [02 · Mappings and text analysis](../02-mappings-and-analysis/overview.en.md)
- [09 · Vector and hybrid search](../09-vector-search/overview.en.md)
