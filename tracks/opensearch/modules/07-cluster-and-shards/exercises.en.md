---
title: "Exercises: cluster and shards"
lang: en
track: opensearch
kind: module
doc: exercises
slug: 07-cluster-and-shards
part: 2
tags: [cluster, shard, replica, shard-allocation, high-availability]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Exercises: cluster and shards

🌐 [Русский](exercises.ru.md) · **English**

> You need the three-node cluster, `client` and the `orders` index from the [fundamentals](basics.en.md).

## 1. Shard map

Count the `orders` shard copies on each node.

<details><summary>Hint</summary>

`cat.shards` with `format="json"`.

</details>

<details><summary>Solution</summary>

```python
from collections import Counter

print(Counter(s["node"] for s in client.cat.shards(index="orders", format="json")))
```

</details>

## 2. Read the reason

Create an index with 3 replicas and print why a copy is unassigned.

<details><summary>Hint</summary>

`cluster.allocation_explain`.

</details>

<details><summary>Solution</summary>

```python
client.indices.create(index="ex-explain", body={"settings": {"number_of_shards": 1, "number_of_replicas": 3}})
client.cluster.health(index="ex-explain", wait_for_status="yellow", wait_for_events="languid")
r = client.cluster.allocation_explain(body={"index": "ex-explain", "shard": 0, "primary": False})
print(r["allocate_explanation"])
print({d["decider"] for n in r["node_allocation_decisions"] for d in n["deciders"]})
client.indices.delete(index="ex-explain")
```

</details>

## 3. Drain a node

Move all shards off `os-node2`, wait for it to finish, restore the setting.

<details><summary>Hint</summary>

`exclude._name`, `wait_for_no_relocating_shards`.

</details>

<details><summary>Solution</summary>

```python
key = "cluster.routing.allocation.exclude._name"
client.cluster.put_settings(body={"transient": {key: "os-node2"}})
client.cluster.health(wait_for_events="languid", wait_for_no_relocating_shards=True)
print(sorted({s["node"] for s in client.cat.shards(format="json") if s["node"]}))
client.cluster.put_settings(body={"transient": {key: None}})
```

</details>

## 4. An incomplete answer

Write a count function that raises when not every shard answered.

<details><summary>Hint</summary>

Compare `successful` and `total`.

</details>

<details><summary>Solution</summary>

```python
def strict_count(index):
    r = client.count(index=index)
    if r["_shards"]["successful"] < r["_shards"]["total"]:
        raise RuntimeError(f"partial result: {r['_shards']}")
    return r["count"]

print(strict_count("orders"))
```

</details>

## 5. Fewer shards

Turn a 4-shard index into a 2-shard one.

<details><summary>Hint</summary>

Write block, gather copies on one node, `shrink`.

</details>

<details><summary>Solution</summary>

```python
client.indices.create(index="ex-big", body={"settings": {"number_of_shards": 4}})
client.indices.put_settings(index="ex-big", body={"index.blocks.write": True,
    "index.number_of_replicas": 0, "index.routing.allocation.require._name": "os-node1"})
client.cluster.health(index="ex-big", wait_for_events="languid", wait_for_status="green", wait_for_no_relocating_shards=True)
client.indices.shrink(index="ex-big", target="ex-small", body={"settings": {"index.number_of_shards": 2,
    "index.number_of_replicas": 1, "index.routing.allocation.require._name": None, "index.blocks.write": None}})
print(client.cluster.health(index="ex-small", wait_for_status="green")["status"])
client.indices.delete(index="ex-big,ex-small")
```

</details>
