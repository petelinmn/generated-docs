---
title: "Mappings and analysis: advanced theory"
lang: en
track: opensearch
kind: module
doc: advanced
slug: 02-mappings-and-analysis
part: 1
tags: [mapping, analyzer, inverted-index, text-keyword, nested, dynamic-mapping]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Mappings and analysis: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## A custom analyzer

```python
client.indices.create(index="st", body={
    "settings": {"number_of_replicas": 0, "analysis": {
        "char_filter": {"amp": {"type": "mapping", "mappings": ["& => and"]}},
        "filter": {"syn": {"type": "synonym", "synonyms": ["tv, television"]},
                   "eg": {"type": "edge_ngram", "min_gram": 2, "max_gram": 10}},
        "analyzer": {
            "my": {"type": "custom", "char_filter": ["html_strip", "amp"],
                   "tokenizer": "standard", "filter": ["lowercase", "asciifolding", "syn"]},
            "autocomplete": {"type": "custom", "tokenizer": "standard", "filter": ["lowercase", "eg"]}}}},
    "mappings": {"properties": {
        "ac": {"type": "text", "analyzer": "autocomplete", "search_analyzer": "standard"},
        "ac_bad": {"type": "text", "analyzer": "autocomplete"},
        "flat": {"type": "object"}, "nest": {"type": "nested"}}}})

analyze("<b>Café</b> & TV", "my", "st")      # ['cafe', 'and', 'tv', 'television']
analyze("Laptop", "autocomplete", "st")      # ['la', 'lap', 'lapt', 'lapto', 'laptop']
```

The order of token filters matters: synonyms placed after `lowercase` see already lowercased terms.

## Autocomplete: different analyzers for indexing and search

`edge_ngram` is needed at indexing time, while the query must not be cut into prefixes.

```python
client.index(index="st", id="1", body={"ac": "Laptop", "ac_bad": "Laptop"}, params={"refresh": "true"})
ids({"match": {"ac": "lap"}}, "st")      # ['1']
ids({"match": {"ac": "lamp"}}, "st")     # []
ids({"match": {"ac_bad": "lamp"}}, "st") # ['1']  the query was cut into "la", "lam", "lamp"
```

## A normalizer for `keyword`

A `normalizer` applies per-character filters while keeping the value a single term.

```python
client.indices.create(index="nz", body={
    "settings": {"analysis": {"normalizer": {"low": {"type": "custom", "filter": ["lowercase"]}}}},
    "mappings": {"properties": {"k": {"type": "keyword", "normalizer": "low"}}}})
client.index(index="nz", id="1", body={"k": "ABC"}, params={"refresh": "true"})
ids({"term": {"k": "abc"}}, "nz")            # ['1']
client.get(index="nz", id="1")["_source"]    # {'k': 'ABC'}
```

## Objects and `nested`

```python
objs = [{"n": "a", "q": 1}, {"n": "b", "q": 2}]
client.index(index="st", id="1", body={"flat": objs, "nest": objs}, params={"refresh": "true"})

def pair(field, q):
    return {"bool": {"filter": [{"term": {field + ".n.keyword": "a"}}, {"term": {field + ".q": q}}]}}

ids(pair("flat", 2), "st")                                    # ['1']  false positive
ids({"nested": {"path": "nest", "query": pair("nest", 2)}}, "st")  # []
ids({"nested": {"path": "nest", "query": pair("nest", 1)}}, "st")  # ['1']
ids({"term": {"nest.q": 1}}, "st")                            # []  needs a nested query
```

A plain object is flattened into the arrays `flat.n` and `flat.q`, and the pairing is lost. `nested` indexes each element as a separate hidden document: for one document with two elements `_cat/indices` showed 3 documents while `_count` showed 1. It is not free.

## Controlling dynamic mapping

```python
client.indices.create(index="m", body={"mappings": {"dynamic": "strict", "properties": {"a": {"type": "keyword"}}}})
client.index(index="m", body={"a": "1", "zz": 2})     # 400 strict_dynamic_mapping_exception

client.indices.create(index="ex", body={"mappings": {"dynamic": False, "properties": {"a": {"type": "keyword"}}}})
client.index(index="ex", id="1", body={"a": "1", "zz": "hi"}, params={"refresh": "true"})
ids({"match": {"zz": "hi"}}, "ex")                    # []  not indexed
client.get(index="ex", id="1")["_source"]             # {'a': '1', 'zz': 'hi'}  still stored
```

| `dynamic` | Unknown field |
|---|---|
| `true` (default) | added to the mapping |
| `false` | stored in `_source`, not indexed |
| `strict` | the document is rejected |

## Type coercion

A `long` field accepts the string `"7"` and the number `7.9` (coercion): both are found by `term: 7`, while `_source` stays as sent. The string `"abc"`, and an object in a `text` field, give `mapper_parsing_exception`. Coercion is turned off with `coerce: false`.

## Common mistakes

- The same `edge_ngram` for indexing and search.
- Dynamic keys (ids used as field names) — a "mapping explosion".
- `nested` where no link within an element is needed.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [Schemas in Kafka](../../../kafka/modules/06-schemas-and-serialization/basics.en.md) — evolving data structure too
- [04 · Aggregations](../04-aggregations/overview.en.md)
- [05 · Indexing at scale](../05-indexing-at-scale/overview.en.md)
- [06 · Relevance](../06-relevance-tuning/overview.en.md)

## Sources

- [Custom analyzers](https://opensearch.org/docs/latest/analyzers/custom-analyzer/)
- [Nested field type](https://opensearch.org/docs/latest/field-types/supported-field-types/nested/)
- [Dynamic parameter](https://opensearch.org/docs/latest/field-types/mapping-parameters/dynamic/)
