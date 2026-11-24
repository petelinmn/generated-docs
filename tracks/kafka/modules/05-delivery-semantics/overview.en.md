---
title: "Delivery semantics"
lang: en
track: kafka
kind: module
doc: overview
slug: 05-delivery-semantics
part: 2
levels: [1, 2, 3, 4]
tags: [kafka, delivery-semantics, exactly-once, transactions, isolation-level, idempotence]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Delivery semantics

🌐 [Русский](overview.ru.md) · **English**

## In short

Kafka lets you choose a guarantee: **at most once** (the position is committed before processing), **at least once** (after processing) and **exactly once** (an idempotent producer plus transactions). "Exactly once" works in the chain Kafka → processing → Kafka: writing the result and committing the input position share one transaction, and readers of the result use `isolation.level=read_committed`.

## Key ideas

- **At least once:** commit after processing; a record may repeat.
- **An idempotent producer** protects one producer's retries in a partition but not atomicity across partitions.
- **A transaction:** `init_transactions`, then `begin` → `produce` → `commit`/`abort`; a stable `transactional.id`.
- **`read_committed`** hides aborted and open transactions; a reader's default is `read_uncommitted`.
- **Transaction markers take offsets** (gaps in numbering).
- **Consume-process-produce:** `send_offsets_to_transaction` — the position and the result are confirmed together (in the experiment the output was exactly `M0…M5`).
- **Fencing a zombie:** a new `init_transactions()` with the same `transactional.id` gives the old one `_FENCED` (fatal).
- **External effects** are outside the guarantee: you need idempotence or an outbox.

## Minimal example

```python
tp = Producer({"bootstrap.servers": "localhost:9092", "transactional.id": "tx-1"})
tp.init_transactions()
tp.begin_transaction(); tp.produce("out", value=b"v"); tp.commit_transaction()
```

Everything was run against a real Apache Kafka 4.3.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Task | Choice |
|---|---|
| A Kafka→Kafka pipeline without duplicates | transactions |
| Simple processing | at-least-once + an idempotent handler |
| A result in an external system | idempotence or an outbox |

- ❌ Do not treat idempotence as transactions.
- ❌ Do not read the result in `read_uncommitted`.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | the three semantics, producer transactions, `read_committed`, an open transaction |
| [2. Advanced theory](advanced.en.md) | consume-process-produce, fencing a zombie, call order, limits |
| [3. Expert theory](expert.en.md) | reference of the API and errors, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kafka 05-delivery-semantics level 5` |
| 6. Tests | not created yet — `/learn kafka 05-delivery-semantics level 6` |

## Related materials

- Prerequisites: [02 · Producers](../02-producers/overview.en.md), [03 · Consumers and groups](../03-consumers-and-groups/overview.en.md)
- Next: [06 · Schemas and serialization](../../overview.en.md)
- Analogues in other tracks: [RabbitMQ: reliability](../../../rabbitmq/modules/05-reliability/overview.en.md)
