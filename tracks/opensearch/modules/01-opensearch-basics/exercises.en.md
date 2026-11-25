---
title: "Exercises: OpenSearch basics"
lang: en
track: opensearch
kind: module
doc: exercises
slug: 01-opensearch-basics
part: 1
tags: [opensearch, index, document, shard, replica, refresh, bulk, optimistic-concurrency]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Exercises: OpenSearch basics

🌐 [Русский](exercises.ru.md) · **English**

> You need the node from the [track overview](../../overview.en.md#environment) and the `client` object from the [fundamentals](basics.en.md).

## 1. The first index

Create an index `books` with 1 shard and no replicas, write three books, read one by `_id` and print `_cat/indices`.

<details><summary>Hint</summary>

`indices.create`, `index`, `get`, `cat.indices`.

</details>

<details><summary>Solution</summary>

```python
client.indices.create(index="books", body={"settings": {"number_of_shards": 1, "number_of_replicas": 0}})
for i, title in enumerate(["Dune", "Emma", "Ulysses"], 1):
    client.index(index="books", id=str(i), body={"title": title}, params={"refresh": "true"})
print(client.get(index="books", id="2")["_source"])
print(client.cat.indices(index="books", params={"v": "true"}))
```

</details>

## 2. Visibility

Show that with `refresh_interval = -1` a document is readable by `_id` but not counted by search.

<details><summary>Hint</summary>

Compare `count` before and after `indices.refresh`.

</details>

<details><summary>Solution</summary>

```python
client.indices.put_settings(index="books", body={"index": {"refresh_interval": "-1"}})
client.index(index="books", id="4", body={"title": "Solaris"})
print(client.get(index="books", id="4")["found"], client.count(index="books")["count"])
client.indices.refresh(index="books")
print(client.count(index="books")["count"])
client.indices.put_settings(index="books", body={"index": {"refresh_interval": "1s"}})
```

</details>

## 3. A counter without races

Increment a document's `views` field so that parallel calls lose no updates. Solve it two ways.

<details><summary>Hint</summary>

A script with `retry_on_conflict`, or a loop with `if_seq_no`.

</details>

<details><summary>Solution</summary>

```python
from opensearchpy.exceptions import ConflictError

# 1) server-side script
client.update(index="books", id="1", params={"retry_on_conflict": 5}, body={
    "script": {"source": "ctx._source.views = (ctx._source.views == null ? 0 : ctx._source.views) + 1"}})

# 2) optimistic loop
while True:
    d = client.get(index="books", id="1")
    src = d["_source"] | {"views": d["_source"].get("views", 0) + 1}
    try:
        client.index(index="books", id="1", body=src,
                     params={"if_seq_no": d["_seq_no"], "if_primary_term": d["_primary_term"]})
        break
    except ConflictError:
        continue
```

</details>

## 4. A batch with errors

Send a batch in which some `create` operations conflict and print the `_id`s of the rejected ones.

<details><summary>Hint</summary>

`helpers.bulk(..., raise_on_error=False)`.

</details>

<details><summary>Solution</summary>

```python
from opensearchpy import helpers

actions = [{"_op_type": "create", "_index": "books", "_id": str(i), "title": "t"} for i in range(1, 7)]
ok, errors = helpers.bulk(client, actions, raise_on_error=False, refresh=True)
print(ok, [e["create"]["_id"] for e in errors])
```

</details>

## 5. An idempotent sink

Write `apply(event)` for events `{id, version, title}`: repeated and stale events must not change data.

<details><summary>Hint</summary>

`version_type=external`, catching `ConflictError`.

</details>

<details><summary>Solution</summary>

```python
from opensearchpy.exceptions import ConflictError

def apply(event):
    try:
        client.index(index="books", id=event["id"], body={"title": event["title"]},
                     params={"version": event["version"], "version_type": "external"})
        return True
    except ConflictError:
        return False   # stale or duplicate

print(apply({"id": "x", "version": 5, "title": "new"}))   # True
print(apply({"id": "x", "version": 4, "title": "old"}))   # False
print(apply({"id": "x", "version": 5, "title": "new"}))   # False
```

</details>
