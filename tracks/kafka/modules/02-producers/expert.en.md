---
title: "Producers: expert level"
lang: en
track: kafka
kind: module
doc: expert
slug: 02-producers
part: 1
tags: [kafka, producer, acks, idempotence, partitioning, compression, linger-ms, delivery-report]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Producers: expert level

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on a single Apache Kafka 4.3.1 broker with `confluent-kafka` 2.16.2.

## Reference

### Client parameters

| Parameter | Purpose | Verified |
|---|---|---|
| `bootstrap.servers` | addresses for the first connection | yes |
| `acks` (`0`, `1`, `all`) | broker acknowledgements | yes |
| `enable.idempotence` | idempotent sending; requires `acks=all` | yes (refused with `acks=1`) |
| `linger.ms` | waiting for a batch to fill | yes (latency ≈ the value) |
| `batch.size` | batch size | set, effect not isolated |
| `compression.type` (`none`, `gzip`, `snappy`, `lz4`, `zstd`) | batch compression | yes |
| `message.timeout.ms` | a record's delivery deadline; `_MSG_TIMED_OUT` once it passes | yes |
| `message.max.bytes` | the maximum record size on the client | indirectly (2 MB refused) |
| `retries`, `retry.backoff.ms`, `delivery.timeout.ms`, `max.in.flight.requests.per.connection`, `transactional.id`, `partitioner` | retries, order, transactions, partition choice | not tested |

### API

| Element | Behaviour | Verified |
|---|---|---|
| `produce(topic, value=, key=, partition=, headers=, timestamp=, on_delivery=)` | queued in the client | the main arguments — yes |
| `flush(timeout)` | waits for sending; returns the number of records still in the queue | `flush` — yes |
| `poll(timeout)` | runs callbacks | yes |
| an error in the callback | a `KafkaError` with `name()` and `str()` | yes |

### Errors (verified)

| Situation | Where it shows | Code/name |
|---|---|---|
| a record above the limit | at `produce` | `MSG_SIZE_TOO_LARGE` (code 10) |
| the broker is unreachable | in the callback by timeout | `_MSG_TIMED_OUT` |
| `enable.idempotence=True` and `acks=1` | when creating the `Producer` | `_INVALID_ARG` |

### Measured ratios (one machine)

| Experiment | Result |
|---|---|
| 20,000 records, `acks` 0 / 1 / all | 0.15 / 0.16 / 0.13 s (no difference) |
| latency of one record, `linger.ms` 0 / 100 | ≈1 ms / ≈110 ms |
| compressing 37 MB of repetitive data | none 37.2; gzip 0.2; snappy 1.9; lz4 0.3; zstd 0.1 MB |

## Internals

- **A client queue and a background thread.** `produce` puts the record into a queue; a separate thread gathers batches per partition and sends them to the leaders.
- **A batch is the unit of compression and sending.** The whole batch is compressed; so records with repetitive content compress better (by orders of magnitude in the experiment).
- **An acknowledgement comes per batch:** the offsets of a batch's records are consecutive.
- **Idempotence rests on record numbers** (a producer id and per-partition sequence numbers): the broker sees a retry and does not append it.

> ⚠️ Verify: the details of idempotence (PID, epochs, sequence numbers), the default batch size, the algorithm of "sticky" partition choice without a key and the effect of `max.in.flight` on ordering are described from the documentation and were not verified here.

## Gotchas

### 1. `acks=0` does not know the offset

The offset in the delivery report is `None` (verified); a lost record goes unnoticed.

### 2. `produce` does not send

`flush()`/`poll()` are needed.

### 3. Without a key there is no order and no evenness

Distribution is unpredictable (verified: two partitions of three).

### 4. Idempotence and `acks`

`enable.idempotence=True` with `acks=1` is refused at creation (verified).

### 5. A large record

Refused at once at `produce` (verified, 2 MB); there is a limit on the broker too.

### 6. `linger.ms` is a delay

A record's latency at `linger.ms=100` is about 110 ms (verified).

### 7. Compression on test data

Repetitive data compresses unrealistically well (verified): down to 0.1 MB from 37 MB.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| `acks=all` + idempotence | durability and no duplicates from retries | more latency and load |
| `acks=1` | faster | loss if the leader fails before replication |
| `acks=0` | maximum speed | losses without a trace |
| a larger `linger.ms` | more full batches | rising latency |
| compression | less disk and network | CPU load |

## Interview questions

<details><summary>1. How does a producer choose a partition?</summary>

An explicit `partition`, otherwise the key's hash; without a key the client spreads records.

→ [Basics](basics.en.md#how-the-partition-is-chosen)

</details>

<details><summary>2. What do `acks=0/1/all` mean?</summary>

Do not wait, wait for the leader, wait for the leader and all in-sync replicas.

→ [Basics](basics.en.md#broker-acknowledgements-acks)

</details>

<details><summary>3. Trick: with `acks=0` what offset is in the delivery report?</summary>

`None`: the broker does not acknowledge.

→ [Gotcha 1](#1-acks0-does-not-know-the-offset)

</details>

<details><summary>4. Why an idempotent producer?</summary>

So retried sends do not create duplicate records in a partition.

→ [Advanced theory](advanced.en.md#the-idempotent-producer)

</details>

<details><summary>5. Trick: can you enable idempotence with `acks=1`?</summary>

No: `_INVALID_ARG`, `all` is needed.

→ [Gotcha 4](#4-idempotence-and-acks)

</details>

<details><summary>6. What does `linger.ms` do?</summary>

Sets how long the client waits for a batch to fill; increases delivery latency.

→ [Advanced theory](advanced.en.md#batches-and-delay-lingerms)

</details>

<details><summary>7. Why compression and what does it pay with?</summary>

It saves disk and network; it pays with CPU. A whole batch is compressed.

→ [Advanced theory](advanced.en.md#compression)

</details>

<details><summary>8. What happens when the broker is unreachable?</summary>

After `message.timeout.ms` the callback gets `_MSG_TIMED_OUT`.

→ [Basics](basics.en.md#errors)

</details>

<details><summary>9. Trick: `produce` returned with no error. Did the record arrive?</summary>

Unknown until the callback; `flush()`/`poll()` and an `err` check are needed.

→ [Gotcha 2](#2-produce-does-not-send)

</details>

<details><summary>10. How do you keep one customer's records in order?</summary>

Use a key (the customer id): one key's records are in one partition.

→ [Basics](basics.en.md#how-the-partition-is-chosen)

</details>

<details><summary>11. Trick: are the compression numbers from the experiment realistic?</summary>

No: the data was extremely repetitive; on real data the gain is several times.

→ [Gotcha 7](#7-compression-on-test-data)

</details>

<details><summary>12. What do you check in the callback?</summary>

`err`: on an error the record was not delivered; retry it or alert.

→ [Basics](basics.en.md#the-delivery-callback)

</details>

## Cheat sheet

```text
Producer({"bootstrap.servers":..., "acks":"all", "enable.idempotence":True, "compression.type":"lz4", "linger.ms":5})
produce(topic, key=, value=, on_delivery=cb) ; poll() ; flush()        # cb(err, msg): err is None on success
partition: explicit > hash(key) > client-chosen (no key)
errors: MSG_SIZE_TOO_LARGE (at produce) ; _MSG_TIMED_OUT (callback) ; idempotence needs acks=all
```

- A key for order, `acks=all` and idempotence for reliability.
- Check `err`, call `flush()`.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [01 · Kafka basics](../01-kafka-basics/expert.en.md)
- Next module: [03 · Consumers and groups](../03-consumers-and-groups/overview.en.md)

## Sources

- [Apache Kafka: Producer configs](https://kafka.apache.org/documentation/#producerconfigs)
- [librdkafka: Configuration](https://github.com/confluentinc/librdkafka/blob/master/CONFIGURATION.md)
- [Apache Kafka: Design: the producer](https://kafka.apache.org/documentation/#design_producer)
