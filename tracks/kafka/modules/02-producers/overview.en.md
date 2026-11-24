---
title: "Producers"
lang: en
track: kafka
kind: module
doc: overview
slug: 02-producers
part: 1
levels: [1, 2, 3, 4]
tags: [kafka, producer, partitioning, acks, idempotence, batching, compression]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Producers

🌐 [Русский](overview.ru.md) · **English**

## In short

A producer chooses a record's partition (by an explicit number, by the key's hash, or by itself), gathers records into batches and sends them to the partition's leader. Reliability is set by `acks` and idempotence, latency and throughput by `linger.ms` and compression. Each record's result arrives in a callback.

## Key ideas

- **Partition:** explicit > key hash > the client's choice; without a key there is no order or evenness.
- **`acks`:** `0` — do not wait (the offset is unknown), `1` — the leader, `all` — the leader and in-sync replicas.
- **`produce` is asynchronous:** the result is in `on_delivery`, `flush()` is needed; errors: `MSG_SIZE_TOO_LARGE` at `produce`, `_MSG_TIMED_OUT` in the callback.
- **Idempotence** requires `acks=all`; otherwise `_INVALID_ARG`.
- **`linger.ms`** adds latency (≈100 ms at `100`).
- **Compression** applies to a batch; on real data the gain is several times.

## Minimal example

```python
p = Producer({"bootstrap.servers": "localhost:9092", "acks": "all", "enable.idempotence": True})
p.produce("orders", key=b"user1", value=b"order-1", on_delivery=lambda err, msg: print(err, msg.offset()))
p.flush()
```

Everything was run against a real Apache Kafka 4.3.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Task | Setting |
|---|---|
| A customer's record order | the key is the customer id |
| Must neither lose nor duplicate | `acks=all`, idempotence, an `err` check |
| Low latency | a small `linger.ms` |
| Saving disk and network | `lz4`/`zstd` compression |

- ❌ Do not use `acks=0` for important data.
- ❌ Do not forget `flush()` and the `err` check.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | choosing a partition, the callback, `acks`, errors |
| [2. Advanced theory](advanced.en.md) | idempotence, `linger.ms`, compression, timeouts |
| [3. Expert theory](expert.en.md) | reference of parameters and errors, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kafka 02-producers level 5` |
| 6. Tests | not created yet — `/learn kafka 02-producers level 6` |

## Related materials

- Prerequisites: [01 · Kafka basics](../01-kafka-basics/overview.en.md)
- Next: [03 · Consumers and groups](../03-consumers-and-groups/overview.en.md)
- Analogues in other tracks: [RabbitMQ: reliability and publisher confirms](../../../rabbitmq/modules/05-reliability/overview.en.md)
