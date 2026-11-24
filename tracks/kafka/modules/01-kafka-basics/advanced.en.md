---
title: "Kafka basics: advanced level"
lang: en
track: kafka
kind: module
doc: advanced
slug: 01-kafka-basics
part: 1
tags: [kafka, kraft, broker, consumer-group, committed-offset, lag, internal-topics]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Kafka basics: advanced level

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run on Apache Kafka 4.3.1 (KRaft, one node) with `confluent-kafka` 2.16.2.

## What a broker and KRaft mode are

A **broker** is a Kafka process that stores partitions and serves clients. A cluster is several brokers. The cluster's metadata (which topics, where partitions are, who the leader is) is kept by a **controller**; in current versions this is a built-in **KRaft** quorum (based on Raft), with no separate ZooKeeper.

The configuration of the `apache/kafka:4.3.1` image (from `/opt/kafka/config/server.properties`):

```text
process.roles=broker,controller
node.id=1
listeners=PLAINTEXT://:9092,CONTROLLER://:9093
log.dirs=/tmp/kraft-combined-logs
num.partitions=1
log.retention.hours=168
log.segment.bytes=1073741824
offsets.topic.replication.factor=1
```

- `process.roles=broker,controller` — one process plays both roles (a teaching "combined" mode).
- Clients connect to `9092`, while the controller listens on `9093`.
- A topic gets 1 partition by default, records are kept for 168 hours (7 days), a log segment is 1 GiB.
- `/tmp/kraft-combined-logs` is the data directory: when the container is removed the data disappears.

The quorum state:

```text
$ kafka-metadata-quorum.sh --bootstrap-server localhost:9092 describe --status
ClusterId:              5L6g3nShT-eMCtK--X86sw
LeaderId:               1
LeaderEpoch:            1
HighWatermark:          70
MaxFollowerLag:         0
MaxFollowerLagTimeMs:   0
```

> ⚠️ Verify: the meaning of the quorum's `LeaderEpoch` and `HighWatermark` (broadly, the number of the leader-election "epoch" and the position of committed metadata records) comes from the documentation and was not verified here.

## Internal topics

Besides `orders-*` the data directory holds service directories:

```text
__cluster_metadata-0
__consumer_offsets-0
__consumer_offsets-1
...
```

- `__cluster_metadata-0` — the cluster's metadata log (KRaft).
- `__consumer_offsets-N` — an internal topic where Kafka writes the **committed offsets of consumer groups**. A reader's position is stored in Kafka itself.

## Consumer-group positions

The group `grp-d78c2` read the topic with `earliest` and committed its offsets. `kafka-consumer-groups.sh --describe`:

```text
GROUP      TOPIC   PARTITION  CURRENT-OFFSET  LOG-END-OFFSET  LAG  CONSUMER-ID  HOST  CLIENT-ID
grp-d78c2  orders  1          3               3               0    -            -     -
grp-d78c2  orders  2          4               4               0    -            -     -

Consumer group 'grp-d78c2' has no active members.
```

- `CURRENT-OFFSET` is the group's committed position (the next record to read), `LOG-END-OFFSET` is the end of the log, `LAG` is the difference: how many records the group has not read yet.
- There are no active members (`-`), but the positions are kept: a group started again continues from the committed offsets:

```text
same group again (offsets committed): []
```

The restart returned nothing: everything was already read and the position did not reset to `earliest`. The parameter `auto.offset.reset` applies **only** when a group has no stored position.

## Describing a topic

```text
Topic: orders  TopicId: MtHLzlWzRhyNBS79_B02cg  PartitionCount: 3  ReplicationFactor: 1  Configs: min.insync.replicas=1,segment.bytes=1073741824
  Partition: 0  Leader: 1  Replicas: 1  Isr: 1  Elr:   LastKnownElr:
```

`Leader` is the broker serving writes and reads of the partition; `Replicas` are the brokers with copies; `Isr` (in-sync replicas) are the copies that keep up with the leader. More in [module 07](../../overview.en.md).

## Best practices

- Name topics by meaning and convention (for example `orders.created`), plan the number of partitions in advance: it cannot be reduced ([module 04](../04-topics-and-retention/overview.en.md)).
- Always call `flush()` before exiting and check for errors in the delivery callback.
- Give groups meaningful `group.id`s: positions are stored by them.
- Watch `LAG` ([module 09](../../overview.en.md)).

## Common mistakes

- ❌ Relying on `auto.offset.reset` for a group with a stored position → ✅ it has no effect there.
- ❌ A shared `group.id` for unrelated applications → ✅ they will start sharing partitions.
- ❌ Keeping broker data in a container's `/tmp` in production → ✅ a volume or disk.
- ❌ Not looking at `LAG` → ✅ monitor the lag.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- Next module: [02 · Producers](../02-producers/overview.en.md)

## Sources

- [Apache Kafka: Configuration](https://kafka.apache.org/documentation/#configuration)
- [Apache Kafka: KRaft](https://kafka.apache.org/documentation/#kraft)
- [Apache Kafka: Managing consumer groups](https://kafka.apache.org/documentation/#basic_ops_consumer_group)
