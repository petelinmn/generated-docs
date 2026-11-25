---
title: "Mappings and analysis: fundamentals"
lang: en
track: opensearch
kind: module
doc: basics
slug: 02-mappings-and-analysis
part: 1
tags: [mapping, analyzer, inverted-index, text-keyword, nested, dynamic-mapping]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Mappings and analysis: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## The inverted index

Instead of "document → words" the index stores "term → list of documents". That makes word search fast, and **which terms exactly** came out of a string decides whether it is found.

## Analyzers

```python
def analyze(text, analyzer, index=None):
    r = client.indices.analyze(index=index, body={"text": text, "analyzer": analyzer})
    return [t["token"] for t in r["tokens"]]

text = "The QUICK brown-foxes, 2 jumped! user@mail.com"
```

| Analyzer | Result |
|---|---|
| `standard` | `the, quick, brown, foxes, 2, jumped, user, mail.com` |
| `simple` | `the, quick, brown, foxes, jumped, user, mail, com` |
| `whitespace` | `The, QUICK, brown-foxes,, 2, jumped!, user@mail.com` |
| `stop` | `quick, brown, foxes, jumped, user, mail, com` |
| `keyword` | the whole string as one term |
| `pattern` | `the, quick, brown, foxes, 2, jumped, user, mail, com` |
| `english` | `quick, brown, fox, 2, jump, user, mail.com` |

For Russian there is the `russian` analyzer: «Быстрые лисы прыгали через ленивых собак» → `быстр, лис, прыга, ленив, собак` (the stop word «через» is dropped, endings are cut). `standard` only lowercases: `быстрые, лисы, и, ёжик`.

## Dynamic mapping

```python
client.index(index="d", id="1", params={"refresh": "true"}, body={
    "title": "Hello World", "n": 5, "f": 1.5, "b": True, "when": "2026-01-15T10:00:00Z",
    "day": "2026-01-15", "numstr": "42", "obj": {"a": 1}, "list": [1, 2], "nul": None})
print(client.indices.get_mapping(index="d")["d"]["mappings"]["properties"])
```

| Value | Type |
|---|---|
| `"Hello World"`, `"42"` | `text` with a sub-field `keyword` (`ignore_above: 256`) |
| `5`, `[1, 2]` | `long` |
| `1.5` | `float` |
| `true` | `boolean` |
| `"2026-01-15T10:00:00Z"`, `"2026-01-15"` | `date` |
| `{"a": 1}` | an object with field `a` |
| `null` | no field is created |

## `text` and `keyword`

```python
def ids(query, index="d"):
    return [h["_id"] for h in client.search(index=index, body={"query": query})["hits"]["hits"]]

ids({"term": {"title": "Hello World"}})          # []      the indexed terms are "hello", "world"
ids({"term": {"title": "hello"}})                # ['1']
ids({"term": {"title.keyword": "Hello World"}})  # ['1']
ids({"match": {"title": "WORLD"}})               # ['1']   match analyzes the query
```

| | `text` | `keyword` |
|---|---|---|
| Analyzed | yes | no |
| Full-text search | yes | no |
| Exact match, filters | awkward | yes |
| Sorting and aggregations | not by default | yes |

Sorting on `text` fails: `Text fields are not optimised for operations that require per-document field data ...`. Use `title.keyword`.

## Explicit mapping

```python
client.indices.create(index="goods", body={"mappings": {"properties": {
    "name": {"type": "text", "analyzer": "english", "fields": {"raw": {"type": "keyword"}}},
    "category": {"type": "keyword"},
    "price": {"type": "double"},
    "created": {"type": "date"}}}})
```

## What can be changed

You can add a new field. You cannot change the type of an existing one: `mapper [n] cannot be changed from type [long] to [keyword]`. You need a new index and a data transfer (reindex).

## Common beginner mistakes

- Relying on dynamic mapping: quoted numbers become text.
- `term` on a `text` field.
- Storing identifiers as `text`.
- Not checking an analyzer with `_analyze`.

## Related materials

- [Advanced theory](advanced.en.md)
- [01 · Basics](../01-opensearch-basics/basics.en.md) · [03 · Queries](../03-search-queries/basics.en.md)

## Sources

- [Mappings and field types](https://opensearch.org/docs/latest/field-types/)
- [Text analysis](https://opensearch.org/docs/latest/analyzers/)
- [Analyze API](https://opensearch.org/docs/latest/api-reference/analyze-apis/)
