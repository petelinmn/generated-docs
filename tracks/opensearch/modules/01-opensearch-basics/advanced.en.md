---
title: "OpenSearch basics: advanced theory"
lang: en
track: opensearch
kind: module
doc: advanced
slug: 01-opensearch-basics
part: 1
tags: [opensearch, index, document, shard, replica, refresh, bulk, optimistic-concurrency]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# OpenSearch basics: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Concurrent changes

Every change of a document gets a `_seq_no`; together with `_primary_term` it lets you write "only if the document has not changed".

```python
from opensearchpy.exceptions import ConflictError

d = client.get(index="products", id="1")
cond = {"if_seq_no": d["_seq_no"], "if_primary_term": d["_primary_term"]}
client.index(index="products", id="1", body=d["_source"] | {"price": 15}, params=cond)   # ok
try:
    client.index(index="products", id="1", body=d["_source"] | {"price": 99}, params=cond)
except ConflictError as e:
    print(e.status_code, e.error)   # 409 version_conflict_engine_exception
```

This is optimistic locking: on a conflict read again and retry. For `update` there is the `retry_on_conflict` parameter.

### External version

If the version is kept at the source (say, in a database), use `version_type=external`: a write with version 5 succeeded, a following one with version 4 got `409`. This way a stale event from a queue does not overwrite a fresh one.

## Routing

A document's shard is chosen by a hash of the routing value; by default that is the `_id`. You can set your own so that one customer's documents sit together and a query goes to one shard.

```python
client.indices.create(index="r", body={"settings": {"number_of_shards": 3, "number_of_replicas": 0}})
client.index(index="r", id="a", body={"n": 100}, params={"routing": "alice", "refresh": "true"})
client.get(index="r", id="a")                                  # NotFoundError (404)
client.get(index="r", id="a", params={"routing": "alice"})     # found
```

If routing is set on write, it is also needed on read, update and delete. A large customer can then overload one shard.

## Scripts, upsert and by-query operations

`update` accepts a script (`ctx._source.n += 5`) and `doc_as_upsert: true` to create a missing document.

```python
r = client.update_by_query(index="r", params={"refresh": "true"}, body={
    "query": {"term": {"g": 1}},
    "script": {"source": "ctx._source.n += params.by", "params": {"by": 1000}}})
# total: 15, updated: 15, version_conflicts: 0
r = client.delete_by_query(index="r", params={"refresh": "true"},
                           body={"query": {"range": {"n": {"gte": 1000}}}})
# total: 15, deleted: 15
```

`update_by_query` and `delete_by_query` are not transactional: they work on a snapshot, abort on a version conflict by default, and what was already done is not rolled back. `conflicts=proceed` continues and counts conflicts.

## Index settings

- `number_of_replicas` can be changed at any time: `0 → 1` on a single node turned the index `yellow`.
- `number_of_shards` cannot be changed on an open index: `400 ... Can't update non dynamic settings`. A different shard count needs reindex, split or shrink.

## Write performance

- Write in `_bulk` batches; find the size by measuring.
- Do not call `refresh=true` for every document.
- For an initial load people raise `refresh_interval` (or set `-1`) and temporarily remove replicas, then restore them.

## Common mistakes

- Reading without `routing` a document written with it.
- "Read, modify, write" without `if_seq_no`.
- Assuming `delete_by_query` can be rolled back.
- Expecting `get` and `search` to agree within the same second.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [Delivery semantics in Kafka](../../../kafka/modules/05-delivery-semantics/basics.en.md) — an idempotent sink
- [05 · Indexing at scale](../05-indexing-at-scale/overview.en.md)

## Sources

- [Update document](https://opensearch.org/docs/latest/api-reference/document-apis/update-document/)
- [Update by query](https://opensearch.org/docs/latest/api-reference/document-apis/update-by-query/)
- [Routing field](https://opensearch.org/docs/latest/field-types/metadata-fields/routing/)
