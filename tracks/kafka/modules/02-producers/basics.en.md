---
title: "Producers: basic level"
lang: en
track: kafka
kind: module
doc: basics
slug: 02-producers
part: 1
tags: [kafka, producer, partitioning, message-key, acks, delivery-report]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Producers: basic level

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · Kafka basics](../01-kafka-basics/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

The code was run against a real Apache Kafka 4.3.1 (`apache/kafka:4.3.1`, one broker, KRaft) in Docker with `confluent-kafka` 2.16.2; outputs are copied from the runs. Helpers: `fresh(topic, parts=3)` deletes and creates a topic; `send(p, topic, items, **kw)` publishes records (items are values or "key, value" pairs), calls `flush` and returns a list of `(error, partition, offset)` from the delivery callbacks; `BS = "localhost:9092"`. Speed and sizes depend on the machine and data: look at the ratios.

## What a producer does

A producer is a client that writes records to topics. For each record it picks a **partition**, gathers records into **batches**, sends them to the partition's leader broker and reports the result in a callback.

```python
p = Producer({"bootstrap.servers": BS})
p.produce("orders", key=b"user1", value=b"order-1", on_delivery=cb)
p.flush()
```

`produce` only puts the record into the client's queue; a background thread does the sending and `flush()` waits for it and runs the callbacks ([module 01](../01-kafka-basics/basics.en.md#writing)).

## How the partition is chosen

| Condition | Partition |
|---|---|
| `partition=` is set | the given one |
| there is a key | determined by the key's hash: one key — one partition |
| no key | spread by the client across partitions |

### An explicit partition

```python
r = send(p, "ex", [(b"same-key", b"v")] * 3, partition=2)
print("explicit partition=2:", [x[1] for x in r])
```

```text
explicit partition=2: [2, 2, 2]
```

### Records without a key

```text
keyless 300 in one batch run: {1: 108, 2: 100, 0: 92}
keyless 30 flushed one by one: {2: 12, 0: 18}
```

Without a key records scatter across partitions: 300 records in one stream went to all three, 30 records sent one by one with a `flush()` after each went to two of the three (in this run). Distribution without a key is **neither even nor predictable**: do not rely on it. If you need record order, set a key.

## The delivery callback

The result of each record arrives in `on_delivery(err, msg)`: on success `err is None` and `msg` holds the partition and offset; on failure `err` describes the cause. Callbacks run inside `flush()` or `poll()`.

## Broker acknowledgements: `acks`

`acks` determines when the broker considers a record accepted:

| Value | Meaning |
|---|---|
| `0` | do not wait for an acknowledgement |
| `1` | wait for the leader's write |
| `all` | wait for the leader and all in-sync replicas |

20,000 records of 100 bytes on one broker:

```text
{'acks': '0'} 20000 msgs: 0.15 s; errors: 0 ; offsets reported: {None}
{'acks': '1'} 20000 msgs: 0.16 s; errors: 0 ; offsets reported: {0, 1, 2}
{'acks': 'all'} 20000 msgs: 0.13 s; errors: 0 ; offsets reported: {0, 1, 2}
```

- With `acks=0` the offset in the report is **unknown** (`None`): the broker answered nothing.
- On one broker there is no speed difference: `all` has no other replicas to wait for. In a cluster `all` is slower but more reliable ([module 07](../../overview.en.md)).

## Errors

A record that is too large is refused at once, in `produce`:

```text
2 MB: KafkaError{code=MSG_SIZE_TOO_LARGE,val=10,str="Unable to produce message: Broker: Message size too large"}
```

If the broker is unreachable the record is not lost silently: after `message.timeout.ms` the callback gets an error:

```text
unreachable broker: [('_MSG_TIMED_OUT', 'Local: Message timed out')]
```

(an experiment with `message.timeout.ms=3000` and a nonexistent address `localhost:1`). Always check `err` in the callback.

## Common beginner mistakes

- ❌ Not calling `flush()` before exiting → ✅ records stay in the buffer.
- ❌ Not checking `err` in the callback → ✅ losses go unnoticed.
- ❌ Expecting even distribution without a key → ✅ set a key or a partition.
- ❌ Using `acks=0` for important data → ✅ `all` (see the advanced theory).
- ❌ Publishing records above the limit → ✅ the record size limit (about 1 MB) or storing outside Kafka.

## Related materials

- Next: [advanced theory](advanced.en.md) — idempotence, batches, compression
- Before: [01 · Kafka basics](../01-kafka-basics/basics.en.md)
- Next module: [03 · Consumers and groups](../03-consumers-and-groups/overview.en.md)

## Sources

- [Apache Kafka: Producer configs](https://kafka.apache.org/documentation/#producerconfigs)
- [Apache Kafka: Design: the producer](https://kafka.apache.org/documentation/#design_producer)
- [librdkafka: Configuration](https://github.com/confluentinc/librdkafka/blob/master/CONFIGURATION.md)
