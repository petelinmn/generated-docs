---
title: "OpenSearch basics"
lang: en
track: opensearch
kind: module
doc: overview
slug: 01-opensearch-basics
part: 1
tags: [opensearch, index, document, shard, replica, refresh, bulk, optimistic-concurrency]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# OpenSearch basics

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

OpenSearch stores JSON **documents** in **indexes** and searches them. An index is split into **shards**, and shards are spread over the **nodes** of a **cluster**. Every operation is an HTTP request to the REST API.

What mainly sets it apart from a database: a write is visible to a read by `_id` immediately, but to **search** only after a refresh, once a second by default.

## Key ideas

- A document has an `_id`, a `_source` (the original JSON) and a version.
- `index` overwrites the whole document, `create` fails if the `_id` is taken, `update` changes some fields.
- Concurrent changes are guarded by the `if_seq_no` and `if_primary_term` pair.
- The number of primary shards is set when an index is created; the number of replicas can be changed.
- Many documents are written through `_bulk`, and errors are checked per operation.

## Minimal example

```python
from opensearchpy import OpenSearch

client = OpenSearch(hosts=[{"host": "localhost", "port": 9200}])
client.index(index="products", id="1", body={"name": "Blue mug", "price": 12.5}, params={"refresh": "true"})
print(client.search(index="products", body={"query": {"match": {"name": "mug"}}})["hits"]["total"])
```

## When to use

- Full-text search, catalog filters and facets, log search, analytics over large volumes.

**Do not use** it as the only store when you need transactions and relations between entities.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn opensearch 01-opensearch-basics level 5` |
| 6. Tests | — | `/learn opensearch 01-opensearch-basics level 6` |

## Related materials

- [OpenSearch track](../../overview.en.md)
- [02 · Mappings and text analysis](../02-mappings-and-analysis/overview.en.md)
- [Kafka basics](../../../kafka/modules/01-kafka-basics/overview.en.md) — another distributed service with partitions and replicas
