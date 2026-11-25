---
title: "Search queries: expert theory"
lang: en
track: opensearch
kind: module
doc: expert
slug: 03-search-queries
part: 1
tags: [query-dsl, full-text-search, bool-query, relevance, bm25, pagination]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Search queries: expert theory

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## What the score is made of

`"explain": true` shows the BM25 computation for `match: {name: "mouse"}`:

```text
0.3979 weight(name:mous in 0) [PerFieldSimilarity], result of:
  0.3979 score(freq=1.0), computed as boost * idf * tf from:
    0.8755 idf, computed as log(1 + (N - n + 0.5) / (n + 0.5)) from:
      2 n, number of documents containing term
      5 N, total number of documents with field
    0.4545 tf, computed as freq / (freq + k1 * (1 - b + b * dl / avgdl)) from:
      1.0 freq    1.2 k1    0.75 b    2.0 dl    2.0 avgdl
```

- **idf**: the rarer a term, the more it is worth.
- **tf**: repetitions raise the score with saturation (`k1`).
- **b**: a penalty for field length relative to the average.

Statistics are computed **per shard**, so on small indexes with several shards identical documents can get different scores.

## Diagnosing a query

| Tool | What it shows |
|---|---|
| `explain` | the score computation of a document |
| `_validate/query?explain=true` | how a query is rewritten: `match` "Wireless Mice" with `english` → `name:wireless name:mice` |
| `"profile": true` | time per part of the query on each shard |

The second example is instructive: the `english` stemmer does not turn `mice` into `mouse` — it cuts suffixes and does not know irregular forms.

## Query catalog

From the documentation; this module verified `match`, `match_phrase`, `multi_match`, `query_string`, `simple_query_string`, `term`, `terms`, `range`, `exists`, `prefix`, `wildcard`, `ids`, `bool`, `constant_score`, `nested`.

| Group | Queries |
|---|---|
| Full-text | `match`, `match_phrase`, `match_phrase_prefix`, `match_bool_prefix`, `multi_match`, `combined_fields`, `query_string`, `simple_query_string`, `intervals` |
| Term-level | `term`, `terms`, `terms_set`, `range`, `exists`, `prefix`, `wildcard`, `regexp`, `fuzzy`, `ids` |
| Compound | `bool`, `boosting`, `constant_score`, `dis_max`, `function_score`, `hybrid` |
| Joining | `nested`, `has_child`, `has_parent`, `parent_id` |
| Geo | `geo_distance`, `geo_bounding_box`, `geo_polygon`, `geo_shape` |
| Specialised | `script`, `script_score`, `more_like_this`, `percolate`, `knn`, `neural`, `rank_feature`, `distance_feature`, `wrapper` |

## Default values

| Parameter | Value |
|---|---|
| `size` | 10 |
| `from + size` at most | 10000 |
| `track_total_hits` | exact count up to 10000 |
| `match.operator` | `or` |
| BM25 `k1`, `b` | 1.2 and 0.75 |

## Gotchas

1. A filter-only `bool` gives `_score` 0.0, while `match_all` and a bare `term` give 1.0 (in the experiment, a `term` on a `keyword` field with one match).
2. `exists` on a missing field gives an empty result, not an error.
3. Date math `2026-03-01||-1M` counts from the given date.
4. `match` on a field with a different analyzer looks for different terms.
5. Sorting on `text` is a 400 error.
6. The `english` analyzer does not know irregular forms (`mice`).
7. `fuzziness` applies to terms **after** analysis: the typo `wireles` becomes `wirel` under `english` and finds `wired`, not `wireless`. Keep a non-stemmed sub-field for fuzzy search.

## Interview questions

1. How does query context differ from filter context?
<details><summary>Answer</summary>

The first computes a score, the second only yes/no, and filter results can be cached.
</details>

2. `match` versus `term`?
<details><summary>Answer</summary>

`match` analyzes the query string, `term` looks for the term as is.
</details>

3. How does BM25 work?
<details><summary>Answer</summary>

A sum over terms of idf times a saturating frequency corrected for field length.
</details>

4. Why do different shards give different scores?
<details><summary>Answer</summary>

Term statistics are local to a shard.
</details>

5. What does `minimum_should_match` do?
<details><summary>Answer</summary>

Sets how many `should` clauses must match.
</details>

6. Why is a deep `from` expensive?
<details><summary>Answer</summary>

Each shard collects the `from + size` best and the coordinator merges them.
</details>

7. What is a PIT?
<details><summary>Answer</summary>

A frozen view of an index for consistent traversal together with `search_after`.
</details>

8. What is `post_filter` for?
<details><summary>Answer</summary>

To filter hits without changing aggregations.
</details>

9. Why is a leading asterisk in `wildcard` slow?
<details><summary>Answer</summary>

The range cannot be narrowed by the start of a term, so the dictionary is scanned.
</details>

10. How do you tolerate typos?
<details><summary>Answer</summary>

`fuzziness: "AUTO"` — the distance depends on term length.
</details>

11. `multi_match` with `name^3`?
<details><summary>Answer</summary>

The contribution of the `name` field is multiplied by 3.
</details>

12. How do you find why one document ranks above another?
<details><summary>Answer</summary>

`explain` for both.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [02 · Mappings and analysis](../02-mappings-and-analysis/expert.en.md)

## Sources

- [Query DSL](https://opensearch.org/docs/latest/query-dsl/)
- [Explain API](https://opensearch.org/docs/latest/api-reference/search-apis/explain/)
- [Profile API](https://opensearch.org/docs/latest/api-reference/search-apis/profile/)
