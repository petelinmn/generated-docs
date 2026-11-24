---
title: "Delivery semantics: advanced level"
lang: en
track: kafka
kind: module
doc: advanced
slug: 05-delivery-semantics
part: 2
tags: [kafka, exactly-once, consume-process-produce, send-offsets-to-transaction, zombie-fencing, transactional-id]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Delivery semantics: advanced level

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run on Apache Kafka 4.3.1 (one broker) with `confluent-kafka` 2.16.2.

## Consume-process-produce, exactly once

A typical task: read a record from the topic `in`, transform it and write the result to `out`. "Exactly once" requires that **writing the result and committing the read position** happen atomically: in one transaction. `send_offsets_to_transaction` adds the consumer's offsets to the transaction for that:

```python
def run_once(crash_after=False, txid="eos-app"):
    c = Consumer({..., "group.id": group, "enable.auto.commit": False, "isolation.level": "read_committed"})
    c.subscribe(["in"])
    tpp = Producer({"bootstrap.servers": BS, "transactional.id": txid}); tpp.init_transactions()
    got = poll_up_to_3_messages(c)
    tpp.begin_transaction()
    for m in got: tpp.produce("out", value=m.value().upper())
    offs = [TopicPartition("in", got[-1].partition(), got[-1].offset() + 1)]
    tpp.send_offsets_to_transaction(offs, c.consumer_group_metadata())
    if crash_after: tpp.abort_transaction(); return -len(got)      # simulated crash
    tpp.commit_transaction(); return len(got)
```

The consumer has `enable.auto.commit=False` and `isolation.level=read_committed`, and the offset to commit is the **next** one to read (`offset + 1`, [module 03](../03-consumers-and-groups/basics.en.md#the-committed-offset-is-the-next-one-to-read)). The topic `in` holds six records `m0…m5`:

```text
batch 1 aborted (simulated crash): -3
output visible after the abort   : []
batch 1 retried                  : 3
batch 2                          : 3
batch 3 (nothing left)           : 0
output (read_committed): ['M0', 'M1', 'M2', 'M3', 'M4', 'M5']
```

- The first batch (3 records) "crashed" — the transaction was aborted: nothing is visible in `out` and the read position did not move.
- A retry of the same batch ran again, then the second, then there is nothing to read.
- The result is six records `M0…M5` in the output, **each once**: the aborted result is not visible, and the input position and the result are confirmed together.

Without a transaction, a failure between writing the result and committing the position leaves the result written but the position not: after a restart the record is processed again and `out` has a duplicate.

## Zombies and fencing

If a process hangs and an orchestrator starts its replacement with the same `transactional.id`, the old process — a "zombie" — may wake up and write extra. The broker protects against this: a new `init_transactions()` **fences off** the old producer.

```python
a = Producer({..., "transactional.id": "same-id"}); a.init_transactions()
b = Producer({..., "transactional.id": "same-id"}); b.init_transactions()   # fences a
a.begin_transaction(); a.produce("fence", value=b"zombie"); a.commit_transaction()
```

```text
old producer fenced: _FENCED fatal: True | txn_requires_abort: False
```

The old producer got a fatal error `_FENCED`: it cannot continue and the process must be restarted. So the `transactional.id` should be **stable for a logical producer** (for example tied to an input-topic partition) but unique among concurrently running ones.

## Call order

Breaking the order gives a state error:

```text
produce without init_transactions -> _STATE KafkaError{code=_STATE,val=-172,str="Unable to produce message: Local: Erroneous state"}
```

The correct order: `init_transactions()` → (`begin_transaction()` → `produce`/`send_offsets_to_transaction` → `commit_transaction()`/`abort_transaction()`)*.

## The limits of "exactly once"

- The guarantee holds for the chain **Kafka → processing → Kafka**. If processing writes to an external system (a database, an email, HTTP) there is no atomicity with Kafka: you need an idempotent receiver or the outbox scheme ([a recipe in the roadmap](../../overview.en.md)).
- Consumers of the result must read with `isolation.level=read_committed`, otherwise they see aborted records ([basics](basics.en.md#producer-transactions)).
- Transactions add latency and load: the acknowledgement comes only after the transaction ends.

> ⚠️ Verify: the effect of transactions on latency/throughput and the parameters `transaction.timeout.ms`, `transactional.id.expiration.ms` were not measured here.

## Best practices

- For Kafka→Kafka pipelines use transactions with `send_offsets_to_transaction` and `read_committed` readers.
- `transactional.id` is stable per logical instance.
- Fatal errors (`_FENCED`) mean restarting the process, not retrying.
- External effects are idempotent or go through an outbox.
- Short transactions: an open transaction delays `read_committed` readers.

## Common mistakes

- ❌ Committing the consumer position separately from the transaction → ✅ `send_offsets_to_transaction`.
- ❌ One `transactional.id` for different concurrent instances → ✅ they fence each other.
- ❌ Reading the result in `read_uncommitted` → ✅ `read_committed`.
- ❌ Expecting "exactly once" for an email → ✅ idempotence on the receiver's side.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [03 · Consumers and groups](../03-consumers-and-groups/advanced.en.md) — commit strategies
- Next module: [06 · Schemas and serialization](../06-schemas-and-serialization/overview.en.md)

## Sources

- [Apache Kafka: Transactions](https://kafka.apache.org/documentation/#design_transactions)
- [KIP-98: Exactly Once Delivery and Transactional Messaging](https://cwiki.apache.org/confluence/display/KAFKA/KIP-98+-+Exactly+Once+Delivery+and+Transactional+Messaging)
- [confluent-kafka-python: send_offsets_to_transaction](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html#confluent_kafka.Producer.send_offsets_to_transaction)
