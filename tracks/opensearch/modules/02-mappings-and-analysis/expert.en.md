---
title: "Mappings and analysis: expert theory"
lang: en
track: opensearch
kind: module
doc: expert
slug: 02-mappings-and-analysis
part: 1
tags: [mapping, analyzer, inverted-index, text-keyword, nested, dynamic-mapping]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Mappings and analysis: expert theory

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Structures inside Lucene

| Structure | Used for | Turned off with |
|---|---|---|
| Inverted index | term search | `index: false` |
| Doc values (columnar) | sorting, aggregations, scripts | `doc_values: false` |
| `_source` | returning results, reindex, `update` | `_source.enabled: false` |
| Norms | field length in scoring | `norms: false` |

`text` has no doc values, so sorting on it needs `fielddata=true` — loading into heap memory, which is usually avoided. Without `_source`, `update` and `reindex` from that index stop working.

## Field types

The list follows the documentation (this module verified `text`, `keyword`, `long`, `float`, `double`, `boolean`, `date`, `object`, `nested`):

| Group | Types |
|---|---|
| Strings | `text`, `keyword`, `match_only_text`, `wildcard` |
| Numbers | `long`, `integer`, `short`, `byte`, `double`, `float`, `half_float`, `scaled_float`, `unsigned_long` |
| Other simple | `boolean`, `date`, `date_nanos`, `ip`, `binary` |
| Ranges | `integer_range`, `long_range`, `float_range`, `double_range`, `date_range`, `ip_range` |
| Structures | `object`, `nested`, `flat_object`, `join` |
| Geo | `geo_point`, `geo_shape` |
| Special | `completion`, `search_as_you_type`, `token_count`, `percolator`, `rank_feature`, `alias`, `knn_vector` |

> ⚠️ Verify: the lists of types, parameters and analyzer parts follow the documentation and were not all run; `coerce: false` was not run either.

## Mapping parameters

| Parameter | Meaning |
|---|---|
| `analyzer`, `search_analyzer` | analysis at write and search time |
| `normalizer` | processing of `keyword` |
| `fields` | several ways to index one value |
| `copy_to` | copy the value into a common field |
| `ignore_above` | do not index an overly long `keyword` |
| `ignore_malformed` | skip a bad value instead of rejecting |
| `null_value` | what to index for an explicit `null` |
| `format` | date formats |
| `coerce` | type coercion |

## Parts of an analyzer

| Part | Count | Examples |
|---|---|---|
| character filters | 0 or more | `html_strip`, `mapping`, `pattern_replace` |
| tokenizer | exactly 1 | `standard`, `whitespace`, `keyword`, `pattern`, `ngram`, `edge_ngram`, `path_hierarchy` |
| token filters | 0 or more | `lowercase`, `asciifolding`, `stop`, `stemmer`, `synonym`, `synonym_graph`, `ngram`, `shingle` |

## Gotchas

1. A value longer than `ignore_above` is not indexed in the `keyword`: a document with a 300-character `title` was not found by `term` on `title.keyword`, though it was stored.
2. The **first** document decides a field's type: `"42"` makes the field text for good.
3. The float 7.9 in a `long` field is not rejected.
4. `standard` does not split `mail.com`, `simple` does.
5. Changing an analyzer does not reindex old documents.
6. The `index.mapping.total_fields.limit` default is 1000.

## Interview questions

1. What does an analyzer consist of?
<details><summary>Answer</summary>

Character filters, one tokenizer, token filters — in that order.
</details>

2. Why does `term` not find "Hello World" in `text`?
<details><summary>Answer</summary>

The query is not analyzed, and the index holds the terms `hello` and `world`.
</details>

3. How do you search both by words and exactly?
<details><summary>Answer</summary>

Multi-fields: `text` with a `keyword` sub-field.
</details>

4. How do you change a field's type?
<details><summary>Answer</summary>

A new index with the mapping you need, reindex, switch an alias.
</details>

5. `object` versus `nested`?
<details><summary>Answer</summary>

`object` is flattened and loses the link within an element; `nested` stores elements as hidden documents.
</details>

6. What are doc values?
<details><summary>Answer</summary>

An on-disk columnar representation for sorting and aggregations.
</details>

7. What is `search_analyzer` for?
<details><summary>Answer</summary>

So the query is analyzed differently from the document, for example with `edge_ngram` autocomplete.
</details>

8. What is a mapping explosion?
<details><summary>Answer</summary>

Unbounded growth of the number of fields that bloats the cluster state; `dynamic: strict`, the field limit and `flat_object` help.
</details>

9. What does `dynamic: false` do?
<details><summary>Answer</summary>

New fields stay in `_source` but are not indexed.
</details>

10. Why is disabling `_source` dangerous?
<details><summary>Answer</summary>

`update` and reindex stop working, and there is nothing to return in results.
</details>

11. How do you check what an analyzer does?
<details><summary>Answer</summary>

`_analyze`.
</details>

12. How does a `normalizer` differ from an analyzer?
<details><summary>Answer</summary>

It is for `keyword`: no tokenization, the value stays one term.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [03 · Search queries](../03-search-queries/expert.en.md)

## Sources

- [Supported field types](https://opensearch.org/docs/latest/field-types/supported-field-types/index/)
- [Mapping parameters](https://opensearch.org/docs/latest/field-types/mapping-parameters/index/)
- [Token filters](https://opensearch.org/docs/latest/analyzers/token-filters/index/)
