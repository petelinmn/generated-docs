---
title: "Exercises: search queries"
lang: en
track: opensearch
kind: module
doc: exercises
slug: 03-search-queries
part: 1
tags: [query-dsl, full-text-search, bool-query, relevance, bm25, pagination]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Exercises: search queries

🌐 [Русский](exercises.ru.md) · **English**

> You need the node from the [track overview](../../overview.en.md#environment) and the data and `search` function from the [fundamentals](basics.en.md).

## 1. Exact and full-text

Find keyboards (by name) cheaper than 60.

<details><summary>Hint</summary>

`match` in `must`, `range` in `filter`.

</details>

<details><summary>Solution</summary>

```python
print(search(query={"bool": {"must": [{"match": {"name": "keyboard"}}], "filter": [{"range": {"price": {"lt": 60}}}]}}))
```

</details>

## 2. Typos and stemming

The query `wireles mous` with `fuzziness` on `name` puts the wired keyboard first. Find out why and make it find the wireless mouse.

<details><summary>Hint</summary>

Use `_analyze` to see which terms `english` produces for the misspelled query.

</details>

<details><summary>Solution</summary>

```python
q = {"query": "wireles mous", "fuzziness": "AUTO"}
print(search(query={"match": {"name": q}}))          # Wired Keyboard first
# the stemmer cut the misspelled words to "wirel" and "mou"; "wirel" is close to "wire" (wired)
print([t["token"] for t in client.indices.analyze(body={"text": "wireles mous", "analyzer": "english"})["tokens"]])

# add a non-stemmed sub-field and reindex the existing documents in place
client.indices.put_mapping(index="shop", body={"properties": {"name": {
    "type": "text", "analyzer": "english", "fields": {"raw": {"type": "keyword"}, "std": {"type": "text"}}}}})
client.update_by_query(index="shop", params={"refresh": "true"})
print(search(query={"match": {"name.std": q | {"operator": "and"}}}))   # only document 1
```

</details>

## 3. Boost in-stock items

List electronics so that in-stock items come first.

<details><summary>Hint</summary>

`filter` plus `should`.

</details>

<details><summary>Solution</summary>

```python
print(search(query={"bool": {"filter": [{"term": {"category": "electronics"}}], "should": [{"range": {"stock": {"gt": 0}}}]}}))
```

</details>

## 4. Walk all pages

Print all `_id`s in pages of 2 with `search_after`.

<details><summary>Hint</summary>

Repeat until a page is empty.

</details>

<details><summary>Solution</summary>

```python
body = {"sort": [{"price": "asc"}, {"_id": "asc"}], "size": 2}
while True:
    hits = client.search(index="shop", body=body)["hits"]["hits"]
    if not hits:
        break
    print([h["_id"] for h in hits])
    body["search_after"] = hits[-1]["sort"]
```

</details>

## 5. Facets

Show `electronics` products but counts for all categories.

<details><summary>Hint</summary>

`post_filter` and `terms`.

</details>

<details><summary>Solution</summary>

```python
r = client.search(index="shop", body={"post_filter": {"term": {"category": "electronics"}},
                                      "aggs": {"c": {"terms": {"field": "category"}}}})
print([h["_id"] for h in r["hits"]["hits"]])
print({b["key"]: b["doc_count"] for b in r["aggregations"]["c"]["buckets"]})
```

</details>
