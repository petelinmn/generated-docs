---
title: "Exercises: mappings and analysis"
lang: en
track: opensearch
kind: module
doc: exercises
slug: 02-mappings-and-analysis
part: 1
tags: [mapping, analyzer, inverted-index, text-keyword, nested, dynamic-mapping]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Exercises: mappings and analysis

🌐 [Русский](exercises.ru.md) · **English**

> You need the node from the [track overview](../../overview.en.md#environment) and the `client` object from the [fundamentals](basics.en.md).

## 1. Explore analyzers

Compare `standard`, `simple` and `english` on "Running dogs' toys, v2.0".

<details><summary>Hint</summary>

`indices.analyze`.

</details>

<details><summary>Solution</summary>

```python
for name in ("standard", "simple", "english"):
    r = client.indices.analyze(body={"text": "Running dogs' toys, v2.0", "analyzer": name})
    print(name, [t["token"] for t in r["tokens"]])
```

</details>

## 2. An explicit mapping

Create `notes` with `title` (full-text and sortable), `tag`, `created`; forbid unknown fields.

<details><summary>Hint</summary>

`dynamic: strict` and multi-fields.

</details>

<details><summary>Solution</summary>

```python
client.indices.create(index="notes", body={"settings": {"number_of_replicas": 0}, "mappings": {"dynamic": "strict", "properties": {
    "title": {"type": "text", "analyzer": "english", "fields": {"raw": {"type": "keyword"}}},
    "tag": {"type": "keyword"}, "created": {"type": "date"}}}})
for i, t in enumerate(["Banana", "Apple"], 1):
    client.index(index="notes", id=str(i), body={"title": t, "tag": "a"}, params={"refresh": "true"})
r = client.search(index="notes", body={"sort": [{"title.raw": "asc"}]})
print([h["_source"]["title"] for h in r["hits"]["hits"]])
```

</details>

## 3. Accent-insensitive search

Make the query `cafe` find `Café`.

<details><summary>Hint</summary>

A custom analyzer with `lowercase` and `asciifolding`.

</details>

<details><summary>Solution</summary>

```python
client.indices.create(index="places", body={
    "settings": {"analysis": {"analyzer": {"folded": {"type": "custom", "tokenizer": "standard", "filter": ["lowercase", "asciifolding"]}}}},
    "mappings": {"properties": {"name": {"type": "text", "analyzer": "folded"}}}})
client.index(index="places", id="1", body={"name": "Café Roma"}, params={"refresh": "true"})
print(client.search(index="places", body={"query": {"match": {"name": "cafe"}}})["hits"]["total"]["value"])
client.indices.delete(index="places")
```

</details>

## 4. A pair of values

An order has lines `{sku, qty}`. Find orders where `sku=A` has quantity at least 5.

<details><summary>Hint</summary>

`nested`.

</details>

<details><summary>Solution</summary>

```python
client.indices.create(index="orders", body={"mappings": {"properties": {"lines": {"type": "nested", "properties": {
    "sku": {"type": "keyword"}, "qty": {"type": "integer"}}}}}})
client.index(index="orders", id="1", body={"lines": [{"sku": "A", "qty": 1}, {"sku": "B", "qty": 9}]})
client.index(index="orders", id="2", body={"lines": [{"sku": "A", "qty": 7}]}, params={"refresh": "true"})
r = client.search(index="orders", body={"query": {"nested": {"path": "lines", "query": {"bool": {"filter": [
    {"term": {"lines.sku": "A"}}, {"range": {"lines.qty": {"gte": 5}}}]}}}}})
print([h["_id"] for h in r["hits"]["hits"]])
client.indices.delete(index="orders")
```

</details>

## 5. Changing a type

Field `n` in index `old` is `text`. Move the data into an index where it is `integer`.

<details><summary>Hint</summary>

A new index and `reindex`.

</details>

<details><summary>Solution</summary>

```python
client.index(index="old", id="1", body={"n": "5"}, params={"refresh": "true"})
client.indices.create(index="new", body={"mappings": {"properties": {"n": {"type": "integer"}}}})
client.reindex(body={"source": {"index": "old"}, "dest": {"index": "new"}}, params={"refresh": "true"})
print(client.indices.get_mapping(index="new")["new"]["mappings"]["properties"]["n"])
print(client.search(index="new", body={"query": {"range": {"n": {"gte": 3}}}})["hits"]["total"]["value"])
client.indices.delete(index="old,new")
```

</details>
