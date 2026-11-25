---
title: "OpenSearch basics: fundamentals"
lang: en
track: opensearch
kind: module
doc: basics
slug: 01-opensearch-basics
part: 1
tags: [opensearch, index, document, shard, replica, refresh, bulk, optimistic-concurrency]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# OpenSearch basics: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Why it is needed

A relational database answers poorly to "find products like 'wireless mouse' and show the best matches first". OpenSearch builds an [inverted index](../02-mappings-and-analysis/basics.en.md) and computes relevance.

## Core concepts

| Concept | What it is |
|---|---|
| Cluster | one or more nodes with a shared state |
| Node | one OpenSearch process |
| Index | a set of documents with common settings and a mapping |
| Document | a JSON object with an `_id` unique within the index |
| Primary shard | a part of an index; each shard is a Lucene index |
| Replica | a copy of a primary shard on another node |

## Connecting

Start the node with the command from the [track overview](../../overview.en.md#environment).

```python
from opensearchpy import OpenSearch

client = OpenSearch(hosts=[{"host": "localhost", "port": 9200}])
print(client.info()["version"]["number"])   # 3.10.1
client.indices.create(index="products",
    body={"settings": {"number_of_shards": 1, "number_of_replicas": 0}})
```

The same actions with `curl`:

```bash
curl -X PUT "localhost:9200/products/_doc/1?refresh=true" \
  -H "Content-Type: application/json" -d '{"name":"Blue mug","price":12.5}'
curl "localhost:9200/products/_search?q=name:mug&pretty"
curl "localhost:9200/_cat/indices?v"
```

## CRUD

```python
r = client.index(index="products", id="1", body={"name": "Blue mug", "price": 12.5})
# result: created, _version: 1
r = client.index(index="products", id="1", body={"name": "Blue mug", "price": 13.0})
# result: updated, _version: 2
r = client.update(index="products", id="1", body={"doc": {"price": 14.0}})
# result: updated, _version: 3
r = client.update(index="products", id="1", body={"doc": {"price": 14.0}})
# result: noop, _version: 3
print(client.get(index="products", id="1")["_source"])
# {'name': 'Blue mug', 'price': 14.0}
client.delete(index="products", id="1")          # result: deleted
```

- `index` with a taken `_id` **replaces** the whole document.
- `update` with `doc` merges fields; if nothing changed the result is `noop` and the version does not grow.
- Without an `id` OpenSearch generates one (20 characters in the experiment).
- `create` with a taken `_id` answers `409 version_conflict_engine_exception`.

## When a document becomes searchable

Search sees a document after a **refresh**. A read by `_id` (`get`) sees it immediately.

```python
client.indices.put_settings(index="products", body={"index": {"refresh_interval": "-1"}})
client.index(index="products", id="9", body={"name": "Hidden"})
print(client.get(index="products", id="9")["found"])   # True
print(client.count(index="products")["count"])         # does not include document 9
client.indices.refresh(index="products")
print(client.count(index="products")["count"])         # includes it
client.indices.put_settings(index="products", body={"index": {"refresh_interval": "1s"}})
```

By default `index.refresh_interval` is `1s`. The request parameter `refresh=wait_for` makes a write wait for the next refresh; `refresh=true` refreshes at once and is too expensive for bulk writing.

## Bulk writes

```python
from opensearchpy import helpers

actions = [{"_op_type": "index", "_index": "products", "_id": str(i), "name": f"Item {i}"} for i in range(10, 15)]
actions.append({"_op_type": "create", "_index": "products", "_id": "10", "name": "dup"})
ok, errors = helpers.bulk(client, actions, raise_on_error=False, refresh=True)
print(ok)                                               # 5
print([(e["create"]["_id"], e["create"]["status"]) for e in errors])   # [('10', 409)]
```

`_bulk` is not atomic: some operations can succeed and others fail. Check the errors of each.

## Cluster health

| Status | Meaning |
|---|---|
| `green` | all primaries and replicas are assigned |
| `yellow` | primaries are assigned, some replicas are not |
| `red` | some primary shards are not assigned |

An index created automatically by the first write got 1 primary shard and 1 replica. On a single node there is nowhere to put the replica, hence the `yellow` status.

## Common beginner mistakes

- Expecting search to see a document right after the write.
- Using `index` where one field needs changing.
- Not checking `_bulk` errors.
- Relying on an automatically created index in production.
- Exposing a node with security disabled to a network.

## Cheat sheet

| Action | REST |
|---|---|
| create an index | `PUT /<index>` |
| write | `PUT /<index>/_doc/<id>` or `POST /<index>/_doc` |
| create only | `PUT /<index>/_create/<id>` |
| read | `GET /<index>/_doc/<id>` |
| change | `POST /<index>/_update/<id>` |
| delete | `DELETE /<index>/_doc/<id>` |
| batch | `POST /_bulk` |
| state | `GET /_cluster/health`, `GET /_cat/indices?v` |

## Related materials

- [Advanced theory](advanced.en.md)
- [02 · Mappings and text analysis](../02-mappings-and-analysis/basics.en.md)
- [Python](../../../python/overview.en.md)

## Sources

- [Index document API](https://opensearch.org/docs/latest/api-reference/document-apis/index-document/)
- [Bulk API](https://opensearch.org/docs/latest/api-reference/document-apis/bulk/)
- [Cluster health](https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-health/)
