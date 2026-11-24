---
title: "Replication and availability"
lang: en
track: kafka
kind: module
doc: overview
slug: 07-replication-and-availability
part: 2
levels: [1, 2, 3, 4]
tags: [kafka, replication, isr, min-insync-replicas, leader-election, kraft]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Replication and availability

🌐 [Русский](overview.ru.md) · **English**

## In short

Every partition has a **leader** and **followers** (copies on other brokers); the number of copies is the replication factor (RF). The in-sync replicas form the **ISR**; when a leader fails, a replica from the ISR becomes the new one. Write durability is set by `acks=all` and `min.insync.replicas`: with RF=3 and `min.insync.replicas=2` the cluster survives losing one broker without stopping writes. Metadata is managed by the KRaft controller quorum.

## Key ideas

- **Writes and reads go through the leader;** followers copy.
- **A leader failure:** a new leader from the ISR within seconds (in the test broker 1 from the ISR `[1, 2]`).
- **`acks=all` + `min.insync.replicas=2`:** with two failures a write is not accepted (`_MSG_TIMED_OUT`); `acks=1` goes through but is not protected.
- **The ISR recovers** after brokers start; **leaders do not necessarily return** to the preferred ones.
- **Without a controller majority** metadata does not change (creating a topic times out).
- **RF=3** for important topics; controllers in production are dedicated.
- **Health:** `--under-replicated-partitions` is empty.

## Minimal example

```bash
kafka-topics.sh --bootstrap-server kafka1:19092 --create --topic rep --partitions 3 --replication-factor 3 --config min.insync.replicas=2
```

Everything was run against a real three-broker Apache Kafka 4.3.1 cluster in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Task | Setting |
|---|---|
| Important data | RF=3, `min.insync.replicas=2`, `acks=all` |
| Maximum speed, loss is acceptable | `acks=1` |
| A single broker for development | RF=1 |

- ❌ Do not treat RF=3 as protection against losing two brokers.
- ❌ Do not restart several brokers at once.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | replication, the ISR, a leader failure, `acks` and `min.insync.replicas`, recovery |
| [2. Advanced theory](advanced.en.md) | the controller quorum, leader election, the preferred leader, watching state |
| [3. Expert theory](expert.en.md) | reference of fields and settings, the experiment table, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kafka 07-replication-and-availability level 5` |
| 6. Tests | not created yet — `/learn kafka 07-replication-and-availability level 6` |

## Related materials

- Prerequisites: [01 · Kafka basics](../01-kafka-basics/overview.en.md), [02 · Producers](../02-producers/overview.en.md)
- Next: [08 · Kafka Connect](../08-kafka-connect/overview.en.md)
- Analogues in other tracks: [RabbitMQ: clustering](../../../rabbitmq/modules/08-clustering/overview.en.md)
