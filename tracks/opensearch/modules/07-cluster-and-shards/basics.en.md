---
title: "Cluster and shards: fundamentals"
lang: en
track: opensearch
kind: module
doc: basics
slug: 07-cluster-and-shards
part: 2
tags: [cluster, shard, replica, shard-allocation, high-availability]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Cluster and shards: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## The words you need

| Term | What it is |
|---|---|
| node | one OpenSearch process |
| cluster | nodes sharing a `cluster.name` |
| cluster manager | the elected node that owns and changes the cluster state |
| primary shard | a part of an index that takes writes first |
| replica | a copy of a primary on another node; it serves searches too |

## A three-node cluster

You need Docker and about 3 GB of free memory.

```bash
# disposable local cluster, security disabled: never expose it
docker network create osnet
for i in 1 2 3; do
  zone=$(echo "a b c" | cut -d' ' -f$i)
  port=""; [ $i = 1 ] && port="-p 127.0.0.1:9200:9200"
  docker run -d --name os-node$i --network osnet $port \
    -e cluster.name=learn -e node.name=os-node$i -e node.attr.zone=$zone \
    -e discovery.seed_hosts=os-node1,os-node2,os-node3 \
    -e cluster.initial_cluster_manager_nodes=os-node1,os-node2,os-node3 \
    -e DISABLE_SECURITY_PLUGIN=true -e DISABLE_INSTALL_DEMO_CONFIG=true \
    -e "OPENSEARCH_JAVA_OPTS=-Xms512m -Xmx512m" opensearchproject/opensearch:3.10.1
done
# cleanup: docker rm -f os-node1 os-node2 os-node3 && docker network rm osnet
```

```python
from opensearchpy import OpenSearch

client = OpenSearch(hosts=[{"host": "localhost", "port": 9200}])
client.cluster.health(wait_for_nodes="3")

client.indices.create(index="orders", body={"settings": {"number_of_shards": 3, "number_of_replicas": 1}})
for i in range(1, 31):
    client.index(index="orders", id=str(i), body={"n": i})
client.indices.refresh(index="orders")

def health(index="orders"):
    h = client.cluster.health(index=index, wait_for_status="green")
    return h["status"], h["active_shards"], h["unassigned_shards"]

health()                      # ('green', 6, 0)
```

3 primaries with one replica each make 6 copies.

## Where the shards are

```python
print(client.cat.shards(index="orders", h="shard,prirep,docs,node", s="shard,prirep", v=True))
```

```text
shard prirep docs node
0     p         6 os-node2
0     r         6 os-node1
1     p        13 os-node1
1     r        13 os-node3
2     p        11 os-node3
2     r        11 os-node2
```

Two copies of a shard never share a node. A document goes to a shard by a hash of its routing value (`_id` by default), so the split is not always even: here 6, 13 and 11 documents.

## Cluster colour

| Status | Means |
|---|---|
| green | every copy is assigned |
| yellow | all primaries are there, some replicas are not: data is available, less redundancy |
| red | at least one primary is missing: some data is unavailable |

## What happens when a node goes away

```text
$ docker stop os-node3

status yellow, 2 nodes, active_shards 4, unassigned_shards 2, delayed_unassigned_shards 2
... 75 seconds later
status green, active_shards 6, unassigned_shards 0      the missing replicas were rebuilt on the two remaining nodes
```

First the cluster waits: the node may come back after a restart. The default `index.unassigned.node_left.delayed_timeout` is `1m`, after which replicas are rebuilt. Bring the node back: `docker start os-node3`.

## Shard and replica counts

```python
from opensearchpy.exceptions import RequestError

client.indices.put_settings(index="orders", body={"number_of_replicas": 2})
health()                      # ('green', 9, 0)      replicas can be changed at any time

try:
    client.indices.put_settings(index="orders", body={"number_of_shards": 6})
except RequestError as e:
    print(e.status_code)      # 400                  primaries are fixed at creation

client.indices.put_settings(index="orders", body={"number_of_replicas": 1})
```

## Common beginner mistakes

- Many tiny shards "for growth": each costs memory and takes part in searches.
- Treating replicas as a backup: deletion and corruption are repeated on replicas. Backups are snapshots.
- Panicking over yellow on a single node: a replica simply has nowhere to go.

## Related materials

- [Advanced theory](advanced.en.md)
- [01 · OpenSearch basics](../01-opensearch-basics/basics.en.md)

## Sources

- [Cluster health API](https://docs.opensearch.org/latest/api-reference/cluster-api/cluster-health/)
- [Creating a cluster](https://docs.opensearch.org/latest/tuning-your-cluster/)
