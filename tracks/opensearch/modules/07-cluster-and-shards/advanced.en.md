---
title: "Cluster and shards: advanced theory"
lang: en
track: opensearch
kind: module
doc: advanced
slug: 07-cluster-and-shards
part: 2
tags: [cluster, shard, replica, shard-allocation, high-availability]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Cluster and shards: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

The cluster, `client` and the `orders` index are from the [fundamentals](basics.en.md).

## Why a shard is unassigned

```python
client.indices.put_settings(index="orders", body={"number_of_replicas": 3})   # 4 copies, 3 nodes
client.cluster.health(index="orders", wait_for_status="yellow", wait_for_events="languid")["status"]   # 'yellow'
r = client.cluster.allocation_explain(body={"index": "orders", "shard": 0, "primary": False})
r["can_allocate"]                                                  # 'no'
any(d["decider"] == "same_shard" for n in r["node_allocation_decisions"] for d in n["deciders"])   # True
client.indices.put_settings(index="orders", body={"number_of_replicas": 1})
```

## Draining a node before maintenance

```python
client.cluster.put_settings(body={"transient": {"cluster.routing.allocation.exclude._name": "os-node3"}})
client.cluster.health(wait_for_events="languid", wait_for_no_relocating_shards=True)
print(client.cat.shards(index="orders", h="node"))                # no os-node3
client.cluster.put_settings(body={"transient": {"cluster.routing.allocation.exclude._name": None}})
```

For a short restart moving everything is unnecessary: `cluster.routing.allocation.enable: primaries` forbids assigning replicas, and you reset it once the node is back. With it a new index stays yellow and `allocation/explain` says why.

## Availability zones

```python
client.cluster.put_settings(body={"persistent": {
    "cluster.routing.allocation.awareness.attributes": "zone",
    "cluster.routing.allocation.awareness.force.zone.values": "a,b,c"}})
```

Shard copies are spread over the values of `node.attr.zone`. Observed with three zones when the node of zone `c` stopped:

| Copies per shard | After 75 seconds |
|---|---|
| 2 (1 replica) | green: the copy was rebuilt in another zone |
| 3 (2 replicas) | yellow: the `awareness` decider will not put two copies into one zone |

That is the point of forced values (`force`): losing a zone does not overload the survivors with extra copies.

## Custom routing

```python
client.indices.create(index="tenants", body={"settings": {"number_of_shards": 3},
                                             "mappings": {"_routing": {"required": True}}})
try:
    client.index(index="tenants", id="1", body={})
except RequestError as e:
    print(e.error)                                # routing_missing_exception
client.index(index="tenants", id="1", body={}, routing="acme", refresh=True)
client.search(index="tenants", routing="acme")["_shards"]["total"]   # 1
client.search(index="tenants")["_shards"]["total"]                   # 3
```

Searching one shard is cheaper, but a large tenant makes it hot. The same `routing` is needed to get or delete by `_id`.

## How many shards

- AWS guidance: 10–50 GiB per shard; the low end for search latency, the high end for logs.
- The same source: at most 25 shards per GiB of node heap.
- These are starting points, not guarantees: load-test on your own data.
- Primaries scale writes and volume, replicas scale reads and resilience.

## Common mistakes

- Forgetting to reset `exclude` or `allocation.enable` after maintenance.
- Two cluster-manager-eligible nodes: losing either loses the majority. Use three.
- Routing by a skewed field.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [05 · Indexing: rollover and split](../05-indexing-at-scale/advanced.en.md)
- [08 · Index lifecycle](../08-index-lifecycle/overview.en.md)

## Sources

- [Cluster allocation explain](https://docs.opensearch.org/latest/api-reference/cluster-api/cluster-allocation/)
- [Cluster settings](https://docs.opensearch.org/latest/install-and-configure/configuring-opensearch/cluster-settings/)
- [Routing metadata field](https://docs.opensearch.org/latest/field-types/metadata-fields/routing/)
- [Choosing the number of shards (AWS)](https://docs.aws.amazon.com/opensearch-service/latest/developerguide/bp-sharding.html)
