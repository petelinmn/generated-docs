---
title: "Kafka basics: expert level"
lang: en
track: kafka
kind: module
doc: expert
slug: 01-kafka-basics
part: 1
tags: [kafka, topic, partition, offset, kraft, consumer-group, auto-offset-reset, log-segment]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Kafka basics: expert level

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on a single Apache Kafka 4.3.1 broker (KRaft) with `confluent-kafka` 2.16.2.

## Reference

### The model

| Concept | Meaning | Verified |
|---|---|---|
| record | key, value, headers, timestamp | key and value — yes |
| topic | a named stream of records | yes |
| partition | an ordered log inside a topic | yes |
| offset | a record's number within a partition | yes |
| low / high watermark | the first available offset / the offset of the next record | yes |
| consumer group | a set of readers sharing a stored position | yes |
| broker, controller | partition storage / cluster metadata (KRaft) | yes |

### The client API (`confluent-kafka` 2.16.2)

| Element | Behaviour | Verified |
|---|---|---|
| `Producer.produce(topic, key=, value=, on_delivery=)` | asynchronously puts a record into the buffer | yes |
| `Producer.flush()` | waits for sending and invokes callbacks | yes |
| `msg.partition()`, `msg.offset()` in the callback | where the record landed | yes |
| `Consumer.subscribe([topic])`, `poll(timeout)` | reading with partitions chosen by the broker | yes |
| `Consumer.get_watermark_offsets(TopicPartition, timeout)` | `(low, high)` | yes |
| `Consumer.commit(asynchronous=False)` | commits the group's position | yes |
| `AdminClient.create_topics([NewTopic(...)])` | a dict "topic → future"; `None` on success | yes |
| `AdminClient.list_topics(timeout)` | metadata, a topic's partitions | yes |

### Configuration and defaults (image `apache/kafka:4.3.1`)

| Parameter | Value | Source |
|---|---|---|
| `process.roles` | `broker,controller` | `server.properties` |
| `num.partitions` | 1 | `server.properties` |
| `log.retention.hours` | 168 | `server.properties` |
| `log.segment.bytes` | 1073741824 | `server.properties` |
| `offsets.topic.replication.factor` | 1 | `server.properties` |
| topic auto-creation | enabled (a write to a nonexistent topic created it with 1 partition) | verified |
| the client's `auto.offset.reset` | set explicitly in the examples; `earliest` and `latest` verified, the default value was not | partly |

### Errors (verified)

| Situation | Result |
|---|---|
| reduce the number of partitions | `InvalidPartitionsException: The topic orders currently has 3 partition(s); 2 would not be an increase.` |
| increase the number of partitions (3 → 4) | done |
| `produce` to a nonexistent topic | the topic is created automatically, partition 0 |

## Internals

- **A partition is a directory of segments.** A segment file's name is the offset of its first record; `.log` holds the records, `.index` a sparse offset index, `.timeindex` a time index (verified by the files being present).
- **Order is guaranteed within a partition.** Offsets grow by 1; there is no order across partitions (verified: offsets 0, 1, 2 in each partition independently).
- **A reader's position is stored in Kafka** in the internal topic `__consumer_offsets` (verified by the directories being present; the topic's contents were not examined here).
- **The key determines the partition.** One key — one partition (verified with two keys); the function itself (murmur2 hash by default in the Java client) is implemented in librdkafka for `confluent-kafka`.
- **Writing is asynchronous:** `produce` merely queues the record in the client; the confirmation is in the callback.

> ⚠️ Verify: the exact formula for choosing a partition by key in librdkafka (which hash is the default in 2.16.0), the index structure and the on-disk record format were not verified here; in particular, the binding of the keys `user0` and `user1` to partitions 2 and 1 was observed, not derived.

## Gotchas

### 1. Partitions cannot be reduced

Only increased; reducing gives `InvalidPartitionsException` (verified).

### 2. Adding partitions changes the key mapping

A new partition changes the number by which the hash remainder is taken, so keys may change partition; the order of one key's records relative to old records is not preserved (per the documentation; not verified here).

### 3. Topic auto-creation

A typo in a topic name creates a new topic with 1 partition (verified) instead of an error. In production auto-creation is usually disabled.

### 4. `auto.offset.reset` does not apply with a stored position

A restarted group did not go back to the start (verified).

### 5. `produce` is not sending

Without `flush()`/`poll()` callbacks do not fire, and the record may not leave.

### 6. An empty partition

Partition 0 stayed empty with two keys (verified): distribution by key can be uneven.

### 7. Data in `/tmp`

In the image data lives in `/tmp/kraft-combined-logs`: removing the container removes everything.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| A Kafka log | re-reading, many readers, scaling | no routing through exchanges, order only within a partition |
| A queue ([RabbitMQ](../../../rabbitmq/overview.en.md)) | flexible routing, deletion after processing | no re-reading (except streams) |
| More partitions | parallelism | cannot be reduced, order only within |
| Fewer partitions | simpler ordering | less parallelism |

## Interview questions

<details><summary>1. How does a Kafka log differ from a queue?</summary>

Records are not deleted on reading but kept per retention; readers are independent and keep their own position.

→ [Basics](basics.en.md#a-log-instead-of-a-queue)

</details>

<details><summary>2. What is an offset?</summary>

A record's sequence number within a partition.

→ [Basics](basics.en.md#a-log-instead-of-a-queue)

</details>

<details><summary>3. Trick: is record order guaranteed across a whole topic?</summary>

No: only within a partition.

→ [Gotcha 6](#6-an-empty-partition)

</details>

<details><summary>4. Why divide topics into partitions?</summary>

For parallel writing and reading and for scaling; partitions are processed independently.

→ [Basics](basics.en.md#a-topic-and-partitions)

</details>

<details><summary>5. How does a key affect the partition?</summary>

Records with one key land in one partition (verified), which preserves their order.

→ [Basics](basics.en.md#writing)

</details>

<details><summary>6. Trick: `produce` returned with no error. Is the record on the broker?</summary>

Not necessarily: `produce` only puts the record into a buffer; the result is in the callback after `flush()`/`poll()`.

→ [Gotcha 5](#5-produce-is-not-sending)

</details>

<details><summary>7. What does `auto.offset.reset` do?</summary>

It sets the starting position for a group with no stored position: `earliest` or `latest`.

→ [Basics](basics.en.md#where-to-read-from-when-a-group-has-no-stored-position)

</details>

<details><summary>8. Trick: a group with `auto.offset.reset=earliest` is restarted after reading the whole topic. Does it read from the start?</summary>

No: it has a stored position, so `auto.offset.reset` does not apply.

→ [Advanced theory](advanced.en.md#consumer-group-positions)

</details>

<details><summary>9. Where does Kafka store group positions?</summary>

In the internal topic `__consumer_offsets`.

→ [Advanced theory](advanced.en.md#internal-topics)

</details>

<details><summary>10. Can you reduce the number of partitions?</summary>

No (`InvalidPartitionsException`); only increase, which may change the key mapping.

→ [Gotcha 1](#1-partitions-cannot-be-reduced)

</details>

<details><summary>11. Trick: what happens on a write to a nonexistent topic?</summary>

On this broker the topic was created automatically with 1 partition.

→ [Gotcha 3](#3-topic-auto-creation)

</details>

<details><summary>12. What is KRaft?</summary>

A built-in controller quorum based on Raft that stores the cluster metadata without ZooKeeper.

→ [Advanced theory](advanced.en.md#what-a-broker-and-kraft-mode-are)

</details>

## Cheat sheet

```text
produce(topic, key, value, on_delivery) ; flush()           # async; result in the callback
subscribe([topic]) ; poll(t) ; commit()                      # group position stored in __consumer_offsets
get_watermark_offsets(tp) -> (low, high)
auto.offset.reset applies only if the group has no committed position
partitions: can grow, cannot shrink ; order only within a partition ; same key -> same partition
```

- A log, not a queue: you can re-read.
- Order only within a partition.
- `flush()` before exiting.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Next module: [02 · Producers](../02-producers/overview.en.md)

## Sources

- [Apache Kafka: Introduction](https://kafka.apache.org/intro)
- [Apache Kafka: Design](https://kafka.apache.org/documentation/#design)
- [Apache Kafka: Configuration](https://kafka.apache.org/documentation/#configuration)
