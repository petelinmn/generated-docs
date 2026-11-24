---
title: "Consumers and groups"
lang: en
track: kafka
kind: module
doc: overview
slug: 03-consumers-and-groups
part: 1
levels: [1, 2, 3, 4]
tags: [kafka, consumer, consumer-group, rebalance, offset-commit, at-least-once]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Consumers and groups

🌐 [Русский](overview.ru.md) · **English**

## In short

Consumers join **groups** with a shared `group.id`. A topic's partitions are divided among the group's members: each goes to exactly one, so the group reads a topic in parallel and the number of active members is at most the number of partitions. A group **commits** its position in Kafka; after a restart reading continues from the committed offset.

## Key ideas

- **One partition — one group member;** extra consumers idle.
- **Rebalancing** redistributes partitions when membership changes; eager revokes all partitions, `cooperative-sticky` and the new protocol (`group.protocol=consumer`) only the moving ones.
- **The committed offset is the next one to read** (`offset + 1`); `-1001` means no position.
- **No commit — a repeat** (at least once): commit after processing, the handler is idempotent.
- **`max.poll.interval.ms`:** too long a pause between `poll` calls gives `_MAX_POLL_EXCEEDED`.
- **Without a group:** `assign` and `seek` set the position by hand.

## Minimal example

```python
c = Consumer({"bootstrap.servers": "localhost:9092", "group.id": "g", "auto.offset.reset": "earliest", "enable.auto.commit": False})
c.subscribe(["orders"])
m = c.poll(1.0)
if m and not m.error():
    handle(m); c.commit(message=m)
```

Everything was run against a real Apache Kafka 4.3.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Task | Solution |
|---|---|
| Scale processing | a group, partitions with headroom |
| Independent handlers of the same data | different groups |
| Do not lose records on a failure | a manual commit after processing |
| Re-read data | `seek`/`assign` or a new group with `earliest` |

- ❌ Do not process longer than `max.poll.interval.ms` without returning to `poll`.
- ❌ Do not commit before processing.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | a group, dividing partitions, committing, a repeat, reading from a position |
| [2. Advanced theory](advanced.en.md) | rebalancing, strategies, the new protocol, `max.poll.interval.ms`, committing |
| [3. Expert theory](expert.en.md) | reference of parameters and the API, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kafka 03-consumers-and-groups level 5` |
| 6. Tests | not created yet — `/learn kafka 03-consumers-and-groups level 6` |

## Related materials

- Prerequisites: [01 · Kafka basics](../01-kafka-basics/overview.en.md), [02 · Producers](../02-producers/overview.en.md)
- Next: [04 · Topics, retention and compaction](../04-topics-and-retention/overview.en.md)
- Analogues in other tracks: [RabbitMQ: queues and delivery](../../../rabbitmq/modules/03-queues-and-delivery/overview.en.md)
