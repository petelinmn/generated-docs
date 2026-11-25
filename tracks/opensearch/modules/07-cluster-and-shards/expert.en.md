---
title: "Cluster and shards: expert theory"
lang: en
track: opensearch
kind: module
doc: expert
slug: 07-cluster-and-shards
part: 2
tags: [cluster, shard, replica, shard-allocation, high-availability]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Cluster and shards: expert theory

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Node roles

| Role | What it does |
|---|---|
| `cluster_manager` | takes part in elections; the elected one manages the state |
| `data` | holds shards, indexes and searches |
| `ingest` | runs ingest pipelines |
| `remote_cluster_client` | talks to other clusters |
| empty list | coordinates requests only |

The example nodes had `cluster_manager, data, ingest, remote_cluster_client` by default.

## The path of a request

- Write: the coordinating node picks the shard from the routing value and forwards to the primary, which applies it and sends it to the replicas.
- Search: one copy of every shard (query phase), then the winning documents are fetched (fetch phase).

## Settings and defaults

| Setting | Default |
|---|---|
| `index.unassigned.node_left.delayed_timeout` | `1m` |
| `index.write.wait_for_active_shards` | `1` |
| `index.auto_expand_replicas` | `false` |
| `index.replication.type` | `DOCUMENT` |
| `cluster.max_shards_per_node` | `1000` |
| `cluster.routing.allocation.enable` | `all` |
| `cluster.routing.rebalance.enable` | `all` |
| `cluster.routing.allocation.node_concurrent_recoveries` | `2` |
| `indices.recovery.max_bytes_per_sec` | 40 MB/s |
| `cluster.routing.allocation.disk.watermark.low` / `high` / `flood_stage` | 85% / 90% / 95% |

Values read from a 3.10.1 node with `include_defaults`. `transient` is lost on a full cluster restart, `persistent` survives.

## Losing the majority

```text
$ docker stop os-node2 os-node3                      2 of 3 cluster-manager-eligible nodes are gone

GET /_cluster/health                                 503 cluster_manager_not_discovered_exception
GET /_cluster/state/blocks?local=true                "no cluster-manager", levels: ["metadata_write"]
POST /orders/_doc/100                                500 node_not_connected_exception (that shard's primary was on a stopped node)
GET /orders/_search?size=0                           200, "_shards": {"total": 3, "successful": 2, "failed": 1}
```

The surviving node still serves reads from what it has, but the cluster state cannot change.

## Partial results

```text
index "solo": 3 primaries, 0 replicas, 30 documents; one node with a shard is stopped

GET /_cluster/health                  red
GET /solo/_count                      {"count": 24, "_shards": {"total": 3, "successful": 2, "failed": 0}}
GET /solo/_search?allow_partial_search_results=false
                                      search_phase_execution_exception: Search rejected due to missing shards [[solo][0]]
node started again                    count is 30
```

## Recovery

`_cat/recovery` shows the type: `existing_store` is a primary reopened from local disk, `peer` a copy fetched from another node. After the node returned, our `peer` recoveries showed `files_percent` 0.0%: no files were copied again.

## Changing the shard count

| Operation | Shards in the result | Requirements |
|---|---|---|
| shrink | a factor of the source | `index.blocks.write`, a copy of every shard on one node |
| split | a multiple of the source | `index.blocks.write` |
| clone | the same | `index.blocks.write` |
| reindex | any | documents are copied |

## Gotchas

1. `_count` on a red index returned 24 of 30 with `failed: 0`; only `successful < total` reveals the loss.
2. Search returns partial results by default. Check `_shards` or pass `allow_partial_search_results=false`.
3. `wait_for_active_shards=2` with one active copy raises `unavailable_shards_exception`; without it the same document is written.
4. A clone inherited the `index.routing.allocation.require._name` left over from shrink preparation and went yellow.
5. The shard limit is cluster-wide: with `max_shards_per_node: 5` and three nodes creation failed at a limit of 15, replicas included.
6. `index.auto_expand_replicas: 0-all` on three nodes produced 2 replicas.
7. Right after a settings change `cluster.health` may answer before the move has started: add `wait_for_events="languid"`.
8. In opensearch-py 3.2.0 `timeout="60s"` on `cluster.health` was taken as the connection timeout and raised an error instead of reaching the server.

## Not exercised here

Dedicated cluster-manager and coordinating nodes, disk watermark triggering, segment replication, cross-cluster replication and search.

> ⚠️ Verify: the behaviour of the items above, and the statements about elections and the write path, come from documentation and general knowledge, not from runs.

## Interview questions

1. How do green, yellow and red differ?
<details><summary>Answer</summary>

All copies assigned; some replicas missing; at least one primary missing.
</details>

2. Why can't the primary count be changed?
<details><summary>Answer</summary>

It determines which shard a document goes to. Use reindex, split or shrink into a new index.
</details>

3. What happens on one node with `number_of_replicas: 1`?
<details><summary>Answer</summary>

Yellow: a replica cannot sit next to its primary.
</details>

4. Why three cluster-manager-eligible nodes?
<details><summary>Answer</summary>

A majority of three survives losing one and prevents a split cluster.
</details>

5. What does the cluster do after a node leaves?
<details><summary>Answer</summary>

Promotes replicas to primaries, then after the delay rebuilds the missing copies.
</details>

6. How do you find why a shard is unassigned?
<details><summary>Answer</summary>

`_cluster/allocation/explain`: the deciders for every node.
</details>

7. Do replicas replace snapshots?
<details><summary>Answer</summary>

No: they repeat deletions and mistakes.
</details>

8. What are disk watermarks?
<details><summary>Answer</summary>

Disk fill thresholds (85, 90, 95% by default): stop allocating, move shards away, block index writes.
</details>

9. Do replicas speed up search?
<details><summary>Answer</summary>

They add throughput when spare nodes exist; a single query need not get faster.
</details>

10. Why are thousands of tiny shards dangerous?
<details><summary>Answer</summary>

Memory, a large cluster state, slow recoveries and a wide query fan-out.
</details>

11. What does shrink do and require?
<details><summary>Answer</summary>

Creates an index with fewer primaries (a factor of the source); needs a write block and a copy of every shard on one node.
</details>

12. What is segment replication?
<details><summary>Answer</summary>

A mode where replicas receive finished segments instead of re-indexing; the default `index.replication.type` is `DOCUMENT`.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [06 · Relevance: shard statistics](../06-relevance-tuning/expert.en.md)

## Sources

- [Cluster reroute](https://docs.opensearch.org/latest/api-reference/cluster-api/cluster-reroute/)
- [Segment replication](https://docs.opensearch.org/latest/tuning-your-cluster/availability-and-recovery/segment-replication/index/)
- [Shrink index](https://docs.opensearch.org/latest/api-reference/index-apis/shrink-index/)
- [Clone index](https://docs.opensearch.org/latest/api-reference/index-apis/clone/)
