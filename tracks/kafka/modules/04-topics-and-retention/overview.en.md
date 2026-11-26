---
title: "Topics, retention and compaction"
lang: en
track: kafka
kind: module
doc: overview
slug: 04-topics-and-retention
part: 1
levels: [1, 2, 3, 4]
tags: [kafka, retention, log-compaction, tombstone, segment, topic-config]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Topics, retention and compaction

🌐 [Русский](overview.ru.md) · **English**

## In short

Records in Kafka are removed not by reading but by the topic's cleanup policy. By default that is **retention** (`retention.ms`, 168 h in the image): the log consists of segments, and whole segments are deleted. The alternative is **compaction** (`cleanup.policy=compact`): the latest value of each key remains, and a key is deleted with a `None` record (a tombstone). The number of partitions can be increased but not decreased, and increasing it changes the key mapping.

## Key ideas

- **Segments are the unit of deletion;** the period is an upper bound: in the experiment the log emptied entirely, last records included.
- **Offsets are not reused:** after cleaning the numbering continues.
- **`kafka-delete-records.sh`** moves `low_watermark`.
- **`segment.bytes` is at least 1 MiB.**
- **Compaction** preserves offsets (with gaps); the active segment is not cleaned.
- **A tombstone** is a record with a key and the value `None`; a consumer handles it as a deletion.
- **Adding partitions:** some keys change partition; an old producer does not learn of the new ones at once.

## Minimal example

```python
fresh("ret", retention_ms=4000, segment_ms=1500)   # topic with a 4 s retention
```

Everything was run against a real Apache Kafka 4.3.1 in Docker with accelerated cleanup checks (see the [basic theory](basics.en.md)).

## When to use and when not

| Task | Policy |
|---|---|
| An event history for a period | `delete` and `retention.ms` |
| The current state by key | `compact` |
| "Eternal" storage is needed | storage outside Kafka |

- ❌ Do not count on the last record outliving the period.
- ❌ Do not add partitions without assessing the effect on per-key order.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | retention, manual deletion, settings, searching by time |
| [2. Advanced theory](advanced.en.md) | compaction, tombstones, adding partitions |
| [3. Expert theory](expert.en.md) | reference of settings and tools, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kafka 04-topics-and-retention level 5` |
| 6. Tests | not created yet — `/learn kafka 04-topics-and-retention level 6` |

## Related materials

- Prerequisites: [01 · Kafka basics](../01-kafka-basics/overview.en.md), [03 · Consumers and groups](../03-consumers-and-groups/overview.en.md)
- Next: [05 · Delivery semantics](../05-delivery-semantics/overview.en.md)
- Analogues in other tracks: [RabbitMQ: queues and delivery](../../../rabbitmq/modules/03-queues-and-delivery/overview.en.md)
- [Index lifecycle in OpenSearch](../../../opensearch/modules/08-index-lifecycle/overview.en.md)
- [System design: event sourcing and CQRS](../../../system-design/modules/13-event-sourcing-and-cqrs/overview.en.md)
