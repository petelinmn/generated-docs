---
title: "Indexing at scale: fundamentals"
lang: en
track: opensearch
kind: module
doc: basics
slug: 05-indexing-at-scale
part: 2
tags: [bulk, index-template, alias, reindex, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Indexing at scale: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Batch writes

```python
from opensearchpy import helpers

def docs(index, n):
    return ({"_index": index, "_id": str(i), "n": i, "msg": f"event {i}"} for i in range(n))

client.indices.create(index="events", body={"settings": {"number_of_replicas": 0}})
print(helpers.bulk(client, docs("events", 1000), chunk_size=500, refresh=True))   # (1000, [])
```

The `_bulk` body is lines of JSON (NDJSON): an action line, then a document line. The `helpers.bulk` helper builds them for you.

## What it gives

A measurement on one node in Docker (one shard, no replicas, short documents, a single thread, connecting to `127.0.0.1`). It shows orders of magnitude, not a target for your system:

| Method | Documents | Speed or time |
|---|---|---|
| one request each | 1000 | 5.0 s |
| one each with `refresh=true` | 1000 | 12.7 s |
| `_bulk` of 1 | 1000 | ≈ 200 docs/s |
| `_bulk` of 100 | 20000 | ≈ 15 000 docs/s |
| `_bulk` of 1000 | 20000 | ≈ 44 000 docs/s |
| `_bulk` of 5000 | 20000 | ≈ 45 000 docs/s |

Past a certain batch size the gain stops. Find the size by measuring; the HTTP body limit `http.max_content_length` is `100mb`.

## Errors in a batch

```python
client.indices.create(index="app-v", body={"mappings": {"properties": {"n": {"type": "long"}}}})
bad = [{"_index": "app-v", "n": 1}, {"_index": "app-v", "n": "x"}]
[(ok, item["index"]["status"]) for ok, item in helpers.streaming_bulk(client, bad, raise_on_error=False)]
# [(True, 201), (False, 400)]
try:
    helpers.bulk(client, [{"_index": "app-v", "n": "x"}])
except helpers.BulkIndexError as e:
    print(len(e.errors))     # 1
```

`helpers.bulk` raises `BulkIndexError` by default; the successful operations of the batch have already been applied.

## Aliases

```python
client.indices.create(index="items-v1", body={"settings": {"number_of_replicas": 0}})
client.index(index="items-v1", id="1", body={"price": "10"}, params={"refresh": "true"})
client.indices.put_alias(index="items-v1", name="items")
client.index(index="items", id="2", body={"price": "20"}, params={"refresh": "true"})
print(client.count(index="items-v1")["count"])   # 2: written through the alias
```

An alias is a second name for one or several indexes.

## Changing structure without downtime

```python
client.indices.create(index="items-v2", body={"settings": {"number_of_replicas": 0},
    "mappings": {"properties": {"price": {"type": "integer"}}}})
r = client.reindex(body={"source": {"index": "items-v1"}, "dest": {"index": "items-v2"}},
                   params={"refresh": "true"})
print(r["total"], r["created"])                  # 2 2
client.indices.update_aliases(body={"actions": [
    {"remove": {"index": "items-v1", "alias": "items"}},
    {"add": {"index": "items-v2", "alias": "items"}}]})
print(list(client.indices.get_alias(name="items")))   # ['items-v2']
```

The actions of one `_aliases` call are applied atomically: readers never see a moment without an index.

## Common beginner mistakes

- Writing one document at a time.
- Addressing `items-v1` directly from the application.
- Expecting `reindex` to carry the mapping over: create the target index first.
- Forgetting that writes arriving during a `reindex` have to be caught up.

## Related materials

- [Advanced theory](advanced.en.md)
- [01 · Basics](../01-opensearch-basics/basics.en.md)

## Sources

- [Bulk API](https://docs.opensearch.org/latest/api-reference/document-apis/bulk/)
- [Index aliases](https://docs.opensearch.org/latest/im-plugin/index-alias/)
- [Reindex data](https://docs.opensearch.org/latest/im-plugin/reindex-data/)
