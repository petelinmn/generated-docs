---
title: "Exercises: indexing at scale"
lang: en
track: opensearch
kind: module
doc: exercises
slug: 05-indexing-at-scale
part: 2
tags: [bulk, index-template, alias, reindex, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Exercises: indexing at scale

🌐 [Русский](exercises.ru.md) · **English**

> You need the node from the [track overview](../../overview.en.md#environment) and the `client` object from [module 01](../01-opensearch-basics/basics.en.md).

## 1. A load

Load 500 documents into `catalog-v1` and check the count.

<details><summary>Hint</summary>

`helpers.bulk`.

</details>

<details><summary>Solution</summary>

```python
from opensearchpy import helpers

client.indices.create(index="catalog-v1", body={"settings": {"number_of_replicas": 0}})
helpers.bulk(client, ({"_index": "catalog-v1", "_id": str(i), "price": str(i)} for i in range(500)), refresh=True)
print(client.count(index="catalog-v1")["count"])
```

</details>

## 2. A migration

Make `price` a number and switch the alias `catalog` to a new index.

<details><summary>Hint</summary>

A new index, `reindex`, `_aliases`.

</details>

<details><summary>Solution</summary>

```python
client.indices.put_alias(index="catalog-v1", name="catalog")
client.indices.create(index="catalog-v2", body={"settings": {"number_of_replicas": 0}, "mappings": {"properties": {"price": {"type": "integer"}}}})
client.reindex(body={"source": {"index": "catalog-v1"}, "dest": {"index": "catalog-v2"}}, params={"refresh": "true"})
client.indices.update_aliases(body={"actions": [{"remove": {"index": "catalog-v1", "alias": "catalog"}}, {"add": {"index": "catalog-v2", "alias": "catalog"}}]})
print(client.count(index="catalog", body={"query": {"range": {"price": {"gte": 490}}}})["count"])
```

</details>

## 3. Bad documents

Load a mix of good and bad documents into `catalog` and collect the rejection reasons.

<details><summary>Hint</summary>

`streaming_bulk` with `raise_on_error=False`.

</details>

<details><summary>Solution</summary>

```python
mixed = [{"_index": "catalog", "price": v} for v in (1, "two", 3)]
failed = [item["index"]["error"]["type"] for ok, item in helpers.streaming_bulk(client, mixed, raise_on_error=False) if not ok]
print(failed)
```

</details>

## 4. A template

Make every new `events-*` index get 0 replicas and an `at` field of type `date`.

<details><summary>Hint</summary>

`put_index_template`.

</details>

<details><summary>Solution</summary>

```python
client.indices.put_index_template(name="t-events", body={"index_patterns": ["events-*"], "template": {
    "settings": {"number_of_replicas": 0}, "mappings": {"properties": {"at": {"type": "date"}}}}})
client.indices.create(index="events-1")
print(client.indices.get_mapping(index="events-1")["events-1"]["mappings"]["properties"]["at"])
client.indices.delete_index_template(name="t-events")
```

</details>

## 5. Rollover

Set up a write alias and roll over after three documents.

<details><summary>Hint</summary>

`is_write_index` and `max_docs`.

</details>

<details><summary>Solution</summary>

```python
client.indices.create(index="app-ex-000001", body={"settings": {"number_of_replicas": 0}, "aliases": {"app-ex": {"is_write_index": True}}})
for i in range(3):
    client.index(index="app-ex", body={"i": i}, params={"refresh": "true"})
r = client.indices.rollover(alias="app-ex", body={"conditions": {"max_docs": 3}})
print(r["rolled_over"], r["new_index"])
```

</details>
