---
title: "Delivery semantics: basic level"
lang: en
track: kafka
kind: module
doc: basics
slug: 05-delivery-semantics
part: 2
tags: [kafka, delivery-semantics, exactly-once, transactions, isolation-level, at-least-once]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Delivery semantics: basic level

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Producers](../02-producers/basics.en.md), [03 · Consumers and groups](../03-consumers-and-groups/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

The code was run on Apache Kafka 4.3.1 (one broker, KRaft, `apache/kafka:4.3.1`) in Docker with `confluent-kafka` 2.16.2; outputs are copied from the runs. Helpers: `fresh(topic)` recreates a topic with one partition; `read(topic, isolation, secs=8)` reads a topic from the start with a new group using an `isolation.level` and returns `(offset, value)`; `hw(topic)` returns `(low, high)`. The reader needs a few seconds to join the group, so the read window is at least 5–8 seconds.

## Three delivery semantics

What a system guarantees on failures between sending, processing and committing a position:

| Semantics | Meaning | How to get it |
|---|---|---|
| at-most-once | a record is not duplicated but may be lost | commit the position **before** processing |
| at-least-once | a record is not lost but may come again | commit the position **after** processing |
| exactly-once | one effect, no losses and no duplicates | an idempotent producer + transactions |

You have already seen at-least-once in [module 03](../03-consumers-and-groups/basics.en.md#what-happens-without-a-commit): without a commit the records came again. Exactly-once is possible in Kafka but **within Kafka**: reading from Kafka, processing and writing back to Kafka as one atomic operation. External actions (an email, a database request) are not part of it.

## The idempotent producer — the first step

`enable.idempotence=true` protects against duplicates from send retries inside one partition ([module 02](../02-producers/advanced.en.md#the-idempotent-producer)). But it does not make a write to **several** partitions or topics atomic and does not tie a write to a consumer's position commit. For that you need transactions.

## Producer transactions

A transaction groups records into several partitions/topics: all are visible to consumers or none.

```python
tp = Producer({"bootstrap.servers": BS, "transactional.id": "tx-1"})
tp.init_transactions()
tp.begin_transaction(); tp.produce("tx2", value=b"committed-0"); tp.commit_transaction()
tp.begin_transaction(); tp.produce("tx2", value=b"aborted-0");   tp.flush(); tp.abort_transaction()
```

- `transactional.id` is a stable name of the producer; the broker uses it to connect restarts of one logical producer.
- `init_transactions()` is called once after creation.
- Then cycles of `begin_transaction()` → `produce(...)` → `commit_transaction()` or `abort_transaction()`.

```text
watermarks: (0, 8)
read_committed  : [(0, 'committed-0'), (1, 'committed-1'), (6, 'committed-2')]
read_uncommitted: [(0, 'committed-0'), (1, 'committed-1'), (3, 'aborted-0'), (4, 'aborted-1'), (6, 'committed-2')]
```

(The experiment: two messages in the first transaction, two in the aborted one, one in the third.)

- **`isolation.level=read_committed`** shows only the records of committed transactions; aborted ones (`aborted-*`) are hidden.
- **`read_uncommitted`** (the reader's default mode) shows the aborted ones too.
- A transaction takes up offsets: for each `commit`/`abort` the broker writes a service **marker** into the partition. Here the first transaction's records are offsets 0 and 1, its marker 2, the aborted records 3 and 4, the abort marker 5, `committed-2` 6, its marker 7. So `high` is 8: 5 records and 3 markers.

> `abort_transaction()` **discards records not yet sent to the broker**. In the first version of the experiment, without `flush()` before `abort_transaction()`, the aborted records never reached the broker: `read_uncommitted` did not show them and `high` was 5 (no abort marker). To demonstrate aborted records you need `flush()`; in real code it is not required before an abort.

## An open transaction

While a transaction is not finished, `read_committed` does not see its records:

```text
open txn, read_committed  : ['committed-1', 'committed-2']
open txn, read_uncommitted: ['committed-2', 'in-flight']
after commit, read_committed: ['committed-2', 'in-flight']
```

A `read_committed` reader does not advance past the **oldest open transaction**, so a long transaction delays all such readers (per the documentation; only the invisibility was observed).

## Common beginner mistakes

- ❌ Assuming idempotence gives "exactly once" everywhere → ✅ it only protects one producer's retries within a partition.
- ❌ Reading a transactional topic in `read_uncommitted` and seeing the aborted → ✅ `isolation.level=read_committed`.
- ❌ Expecting "exactly once" to cover an email or a database write → ✅ only Kafka→Kafka.
- ❌ Being surprised by gaps in offsets → ✅ transaction markers take offsets.
- ❌ Not setting a stable `transactional.id` → ✅ a restart cannot fence off a "zombie".

## Related materials

- Next: [advanced theory](advanced.en.md) — consume-process-produce, "zombies"
- Before: [03 · Consumers and groups](../03-consumers-and-groups/basics.en.md), [02 · Producers](../02-producers/basics.en.md)
- Next module: [06 · Schemas and serialization](../06-schemas-and-serialization/overview.en.md)
- Analogue in another track: [RabbitMQ: reliability and idempotence](../../../rabbitmq/modules/05-reliability/advanced.en.md)

## Sources

- [Apache Kafka: Message delivery semantics](https://kafka.apache.org/documentation/#semantics)
- [Apache Kafka: Transactions](https://kafka.apache.org/documentation/#design_transactions)
- [confluent-kafka-python: Producer transactions](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html#confluent_kafka.Producer.init_transactions)
