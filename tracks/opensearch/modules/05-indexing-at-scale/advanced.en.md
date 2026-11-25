---
title: "Indexing at scale: advanced theory"
lang: en
track: opensearch
kind: module
doc: advanced
slug: 05-indexing-at-scale
part: 2
tags: [bulk, index-template, alias, reindex, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Indexing at scale: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

This builds on the data and `helpers` from the [fundamentals](basics.en.md).

## Index templates

```python
client.transport.perform_request("PUT", "/_component_template/c-common", body={"template": {
    "settings": {"number_of_replicas": 0}, "mappings": {"properties": {"@timestamp": {"type": "date"}}}}})
client.indices.put_index_template(name="t-logs", body={
    "index_patterns": ["logs-*"], "priority": 10, "composed_of": ["c-common"],
    "template": {"settings": {"number_of_shards": 2},
                 "mappings": {"properties": {"level": {"type": "keyword"}}}, "aliases": {"all-logs": {}}}})
client.index(index="logs-2026.01", body={"level": "INFO", "@timestamp": "2026-01-01T00:00:00Z"})
# the new index got: 2 shards, 0 replicas, fields @timestamp and level, alias all-logs
client.indices.put_index_template(name="t-logs", body={"index_patterns": ["logs-*"], "priority": 10,
    "template": {"settings": {"number_of_shards": 3}}})
print(client.indices.get_settings(index="logs-2026.01")["logs-2026.01"]["settings"]["index"]["number_of_shards"])
# 2: existing indexes are not changed
```

A component template is a reusable part. Two templates with overlapping patterns and equal `priority` cannot be created (a 400 error). `_index_template/_simulate_index/<name>` shows what a future index will get.

## An alias over several indexes

```python
for name in ("app-a", "app-b"):
    client.indices.create(index=name, body={"settings": {"number_of_replicas": 0}})
    client.indices.put_alias(index=name, name="app-pair")
client.index(index="app-pair", body={})        # error 400: no write index is defined for alias
client.indices.update_aliases(body={"actions": [{"add": {"index": "app-b", "alias": "app-pair", "is_write_index": True}}]})
print(client.index(index="app-pair", body={})["_index"])    # app-b
client.get(index="app-pair", id="1")           # error 400: alias has more than one index
```

## A filtered alias

```python
client.indices.create(index="items-f", body={"settings": {"number_of_replicas": 0}})
for i, price in enumerate((10, 20, 30), 1):
    client.index(index="items-f", id=str(i), body={"price": price}, params={"refresh": "true"})
client.indices.put_alias(index="items-f", name="items-expensive", body={"filter": {"range": {"price": {"gte": 20}}}})
print(client.count(index="items-expensive")["count"])    # 2
```

An alias filter is a convenience, not security: it does not forbid direct access to the index.

## Rollover

```python
client.indices.create(index="app-log-000001", body={"settings": {"number_of_replicas": 0},
    "aliases": {"app-log": {"is_write_index": True}}})
client.index(index="app-log", body={"a": 1}, params={"refresh": "true"})
r = client.indices.rollover(alias="app-log", body={"conditions": {"max_docs": 5}})
print(r["rolled_over"])                          # False
r = client.indices.rollover(alias="app-log", body={"conditions": {"max_docs": 1}})
print(r["rolled_over"], r["new_index"])          # True app-log-000002
```

Conditions are checked only at the moment of the call; automation comes from ISM (module 08 of the roadmap).

## A long reindex

```python
import time

task = client.reindex(body={"source": {"index": "events"}, "dest": {"index": "events-copy"}},
                      params={"wait_for_completion": "false", "slices": "auto"})
while not (state := client.tasks.get(task_id=task["task"]))["completed"]:
    time.sleep(0.2)
print(state["response"]["total"], state["response"]["created"])   # 1000 1000
```

## A subset and a transformation

```python
r = client.reindex(params={"refresh": "true"}, body={
    "source": {"index": "events", "query": {"range": {"n": {"lt": 10}}}, "_source": ["n"]},
    "dest": {"index": "events-small"},
    "script": {"source": "ctx._source.n *= 2"}})
print(r["total"])                                # 10
```

## Running reindex again

| `dest.op_type` | A repeat over two already copied documents |
|---|---|
| default (`index`) | `updated: 2` |
| `create` + `conflicts: proceed` | `created: 0`, `version_conflicts: 2` |

## Settings for an initial load

For the duration of a load people usually set `refresh_interval: -1` and `number_of_replicas: 0`, then restore them. While there are no replicas, losing a node loses the shard's data: do this only if the load can be repeated. `index.translog.durability: async` speeds writes further, but a crash loses acknowledged operations of the last seconds.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [01 · Basics](../01-opensearch-basics/expert.en.md) — translog, refresh

## Sources

- [Index templates](https://docs.opensearch.org/latest/im-plugin/index-templates/)
- [Rollover](https://docs.opensearch.org/latest/api-reference/index-apis/rollover/)
- [Reindex API](https://docs.opensearch.org/latest/api-reference/document-apis/reindex/)
