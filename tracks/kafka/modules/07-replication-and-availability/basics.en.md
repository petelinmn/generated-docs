---
title: "Replication and availability: basic level"
lang: en
track: kafka
kind: module
doc: basics
slug: 07-replication-and-availability
part: 2
tags: [kafka, replication, leader, isr, min-insync-replicas, acks, high-availability]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Replication and availability: basic level

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · Kafka basics](../01-kafka-basics/basics.en.md), [02 · Producers](../02-producers/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

The code was run on a cluster of **three brokers** of Apache Kafka 4.3.1 (KRaft, image `apache/kafka:4.3.1`, containers `kafka1`, `kafka2`, `kafka3` on the Docker network `kfk`, host ports 9091–9093) with `confluent-kafka` 2.16.2; outputs are copied from the runs. Each broker combines the broker and controller roles (`process.roles=broker,controller`), the controller quorum is three nodes. Brokers were stopped and started with `docker stop`/`docker start`. **This is a single-machine teaching rig:** it shows the logic of failures but not network faults between machines. Helpers: `fresh(topic, parts=3, rf=3, **cfg)` recreates a topic; `state(topic)` parses the output of `kafka-topics.sh --describe`; `send(producer, topic, n)` publishes `n` records and returns the results from the callbacks (`ok` or an error name).

## Replication

Every partition has a **leader** and **followers**: copies on other brokers. All writes and reads go through the leader, and followers pull data from it. The number of copies is the **replication factor** (RF).

```text
p0 leader=3 replicas=3,1,2 isr=3,1,2
p1 leader=1 replicas=1,2,3 isr=1,2,3
p2 leader=2 replicas=2,3,1 isr=2,3,1
```

A topic with three partitions and RF=3:

- `leader` — the broker serving the partition: leaders are **spread** across brokers (3, 1, 2).
- `replicas` — the brokers with copies; the first in the list is the **preferred leader**.
- `isr` — the **in-sync replicas**: copies that keep up with the leader. Here all three.

## A leader failure

Stop broker 3 — the leader of partition 0:

```text
kafka3
p0 leader now: 1 replicas: [3, 1, 2] isr: [1, 2]
acks=all, 2 of 3 alive: ['ok', 'ok', 'ok']
```

Within seconds broker 1 became partition 0's leader, broker 3 dropped out of the ISR (`isr: [1, 2]`) and writes with `acks=all` carried on. Clients refresh metadata and find the new leader themselves.

## `acks` and `min.insync.replicas`

The durability of a write is determined by two settings ([module 02](../02-producers/basics.en.md#broker-acknowledgements-acks)):

- `acks=all` — a write is acknowledged when **all in-sync replicas** have accepted it;
- `min.insync.replicas` (a topic setting; here 2) — the **minimum** number of in-sync replicas at which a write with `acks=all` is accepted at all.

Together: with RF=3 and `min.insync.replicas=2` a write is confirmed on at least two brokers, and the cluster survives the loss of **one** broker without stopping writes.

Stop a second broker (one of three remains):

```text
acks=all, 1 of 3 alive: ['_MSG_TIMED_OUT', '_MSG_TIMED_OUT']
acks=1 , 1 of 3 alive: ['ok', 'ok']
```

- With `acks=all` writes are **not accepted**: there are fewer than two in-sync replicas, and the client eventually gets `_MSG_TIMED_OUT`.
- With `acks=1` writes go through — the leader is alive — but they are **not protected**: only the leader confirmed, and its failure would lose them.

## Recovery

After the brokers were started the replicas caught up with the leaders and rejoined the ISR:

```text
p0 leader=2 replicas=[3, 1, 2] isr=[1, 2, 3]
p1 leader=2 replicas=[1, 2, 3] isr=[1, 2, 3]
p2 leader=2 replicas=[2, 3, 1] isr=[1, 2, 3]
under-replicated: (none)
```

The ISR is full, there are no under-replicated partitions. The leaders did **not** return to the preferred ones: broker 2 stayed the leader of all three partitions.

## Common beginner mistakes

- ❌ RF=1 in production → ✅ at least RF=3 for important topics.
- ❌ `acks=all` with `min.insync.replicas=1` → ✅ `all` protects only together with `min.insync.replicas` ≥ 2.
- ❌ `acks=1` for important data → ✅ a write confirmed only by the leader is lost when it fails.
- ❌ Assuming leaders return to their places after recovery → ✅ they may stay on other brokers.
- ❌ Two brokers "for safety" → ✅ with RF=3 you need three.

## Related materials

- Next: [advanced theory](advanced.en.md) — the controller quorum, leader election
- Before: [06 · Schemas and serialization](../06-schemas-and-serialization/basics.en.md), [02 · Producers](../02-producers/basics.en.md)
- Next module: [08 · Kafka Connect](../08-kafka-connect/overview.en.md)
- Analogue in another track: [RabbitMQ: clustering and quorum queues](../../../rabbitmq/modules/08-clustering/basics.en.md)

## Sources

- [Apache Kafka: Replication](https://kafka.apache.org/documentation/#replication)
- [Apache Kafka: Topic configs: min.insync.replicas](https://kafka.apache.org/documentation/#topicconfigs_min.insync.replicas)
- [Apache Kafka: Producer configs: acks](https://kafka.apache.org/documentation/#producerconfigs_acks)
