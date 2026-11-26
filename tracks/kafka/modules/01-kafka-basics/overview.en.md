---
title: "Kafka basics"
lang: en
track: kafka
kind: module
doc: overview
slug: 01-kafka-basics
part: 1
levels: [1, 2, 3, 4]
tags: [kafka, topic, partition, offset, consumer-group, kraft, log]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Kafka basics

🌐 [Русский](overview.ru.md) · **English**

## In short

Kafka stores records in a **log**: a topic is split into partitions, and each partition is an ordered sequence of records with increasing **offsets**. A producer appends records, a consumer reads them remembering its position; reading deletes nothing, so the same data is read by many independent groups and re-read when needed.

## Key ideas

- **A log, not a queue:** a record stays until retention ends (168 h by default).
- **A topic → partitions:** order is guaranteed only within a partition; partitions can be added but not removed.
- **The key determines the partition:** records with one key in one partition.
- **`produce` is asynchronous:** the result is in a callback, `flush()` is needed.
- **A group's position is stored in Kafka** (`__consumer_offsets`); `auto.offset.reset` applies only when there is no position.
- **KRaft:** metadata is held by a built-in controller quorum, no ZooKeeper.
- **Topic auto-creation is on** in this image: a typo creates a new topic.

## Minimal example

```python
p = Producer({"bootstrap.servers": "localhost:9092"})
p.produce("orders", key=b"user1", value=b"order-1")
p.flush()
```

Everything was run against a real Apache Kafka 4.3.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Task | Kafka |
|---|---|
| A stream of events with re-reading and many readers | fits |
| A change log, audit, integrating systems | fits |
| Complex routing and deletion after processing | consider a queue ([RabbitMQ](../../../rabbitmq/overview.en.md)) |

- ❌ Do not expect order across a whole topic.
- ❌ Do not rely on topic auto-creation in production.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | the log, topic, partitions, writing, reading, offsets, keys |
| [2. Advanced theory](advanced.en.md) | the broker and KRaft, configuration, internal topics, group positions |
| [3. Expert theory](expert.en.md) | reference of the model and API, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kafka 01-kafka-basics level 5` |
| 6. Tests | not created yet — `/learn kafka 01-kafka-basics level 6` |

## Related materials

- Prerequisites: [the Python track](../../../python/overview.en.md)
- Next: [02 · Producers](../02-producers/overview.en.md)
- Analogues in other tracks: [RabbitMQ](../../../rabbitmq/modules/01-messaging-basics/overview.en.md)
- [Scaling and consistent hashing](../../../system-design/modules/02-scalability-and-load-balancing/overview.en.md)
