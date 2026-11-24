---
title: "Topics, retention and compaction: expert level"
lang: en
track: kafka
kind: module
doc: expert
slug: 04-topics-and-retention
part: 1
tags: [kafka, retention, log-compaction, topic-config, segment, tombstone, delete-records]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Topics, retention and compaction: expert level

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on a single Apache Kafka 4.3.1 broker with accelerated checks (`log.retention.check.interval.ms=2000`, `log.cleaner.backoff.ms=1000`).

## Reference

### Topic settings

| Setting | Purpose | Verified |
|---|---|---|
| `retention.ms` | retention of closed segments | yes |
| `segment.ms` | after how long to close a segment | yes |
| `segment.bytes` | segment size; the minimum is 1048576 | yes (refused at 100000) |
| `cleanup.policy` (`delete`, `compact`, `compact,delete`) | the cleanup policy | `delete` (default), `compact` — yes |
| `min.cleanable.dirty.ratio` | the share of the "dirty" part that starts compaction | yes (0.01) |
| `delete.retention.ms` | tombstone retention | set (3000), removal not confirmed |
| `min.compaction.lag.ms` | the minimum record age before compaction | set (0) |
| `retention.bytes`, `max.compaction.lag.ms`, `min.insync.replicas`, `max.message.bytes`, `compression.type`, `message.timestamp.type`, `flush.*` | others | not tested |

### Broker settings and the image's values

| Parameter | Value in the image | Note |
|---|---|---|
| `log.retention.hours` | 168 | the default retention |
| `log.segment.bytes` | 1073741824 | |
| `log.retention.check.interval.ms` | 300000 (2000 in the experiments) | is **not** changeable dynamically: `Cannot update these configs dynamically` |
| `log.cleaner.backoff.ms` | 15000 (1000 in the experiments) | the cleaner's interval |
| `log.cleanup.policy` | `delete` | the default for `cleanup.policy` |

### Tools (verified)

| Command | Result |
|---|---|
| `kafka-configs.sh --describe --topic T` | topic settings and their synonyms |
| `kafka-delete-records.sh --offset-json-file F` | `low_watermark` moved to 6 |
| `Consumer.offsets_for_times([TopicPartition(t, p, ts)])` | the first offset with a timestamp ≥ `ts` |
| `Consumer.get_watermark_offsets` | `(low, high)` |
| `AdminClient.create_partitions([NewPartitions(t, n)])` | increases the number of partitions |

## Internals

- **Segments are the unit of deletion.** A partition's log is a sequence of segments; a segment is deleted whole once all its records are older than `retention.ms`.
- **Closing a segment by time** (`segment.ms`) means even the "active" segment eventually falls under deletion (in the experiment the log emptied entirely: `low = high = 5`).
- **Numbering does not roll back:** new records continued at 5 and 6 (verified).
- **Compaction preserves the offsets** of the surviving records (9, 10, 11 — verified), so the numbering gets gaps.
- **A tombstone is a record with an empty value** and the same key; the cleaner removes the key's earlier values (verified).
- **Cleaning is periodic,** so deletion does not happen "exactly on time".

> ⚠️ Verify: the cleaner's algorithm (dirty/clean regions, the offset map), the exact logic of choosing segments to delete and the tombstone's lifetime were not verified here.

## Gotchas

### 1. The last record is deleted too

After `retention.ms` and a segment close the log could empty entirely (verified).

### 2. Offsets are not reused

After cleaning `low = high`, and new records get the next numbers (verified).

### 3. The minimum segment size

A `segment.bytes` below 1 MiB is refused (verified).

### 4. Compaction is not instant and does not touch the active segment

Two records of one key remained (verified).

### 5. Tombstones must be handled

A value of `None` is a key deletion.

### 6. Adding partitions breaks key order

A new producer sent four keys of eight to other partitions (verified).

### 7. An old producer does not see new partitions

It kept writing by the old scheme (verified).

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| `delete` by time | a simple event history | old data is lost |
| `compact` | state by key, bounded size | no history, tombstones needed |
| `compact,delete` | state and a bounded period | harder to reason about |
| keep longer | you can re-read | more disk |
| move history to storage | unbounded history | a separate component |

## Interview questions

<details><summary>1. What removes records from a Kafka topic?</summary>

A retention policy (by time/size) or compaction; reading does not remove them.

→ [Basics](basics.en.md#retention)

</details>

<details><summary>2. Trick: are records deleted one by one when the period expires?</summary>

No, in whole segments.

→ [Basics](basics.en.md#retention)

</details>

<details><summary>3. What happens to offsets after cleaning?</summary>

They are not reused: new records continue the numbering.

→ [Gotcha 2](#2-offsets-are-not-reused)

</details>

<details><summary>4. How does compaction differ from `delete`?</summary>

It keeps the latest value of each key instead of records for a period; offsets are preserved.

→ [Advanced theory](advanced.en.md#log-compaction)

</details>

<details><summary>5. How do you delete a key in a compact topic?</summary>

Publish a record with that key and the value `None` (a tombstone).

→ [Advanced theory](advanced.en.md#tombstones)

</details>

<details><summary>6. Trick: is a key's record order preserved after adding partitions?</summary>

No: some keys move to other partitions.

→ [Gotcha 6](#6-adding-partitions-breaks-key-order)

</details>

<details><summary>7. How do you move the start of the log by hand?</summary>

`kafka-delete-records.sh` with JSON (`low_watermark` rose to 6).

→ [Basics](basics.en.md#deleting-records-by-hand)

</details>

<details><summary>8. How do you find an offset by time?</summary>

`offsets_for_times`.

→ [Basics](basics.en.md#searching-by-time)

</details>

<details><summary>9. Trick: can you set `segment.bytes=100000`?</summary>

No: the minimum is 1048576.

→ [Gotcha 3](#3-the-minimum-segment-size)

</details>

<details><summary>10. Why did compaction keep two `k9` records?</summary>

Likely they sat in the active segment, which is not cleaned.

→ [Gotcha 4](#4-compaction-is-not-instant-and-does-not-touch-the-active-segment)

</details>

<details><summary>11. Why did the old producer keep writing to the old partitions?</summary>

It had not refreshed the topic metadata (the refresh interval is 5 minutes by default per the documentation).

→ [Gotcha 7](#7-an-old-producer-does-not-see-new-partitions)

</details>

<details><summary>12. When `compact` and when `delete`?</summary>

`compact` for state by key, `delete` for an event history over a period.

→ [Advanced theory](advanced.en.md#compaction-or-retention)

</details>

## Cheat sheet

```text
retention.ms / segment.ms / segment.bytes(>=1 MiB) ; cleanup.policy = delete | compact | compact,delete
delete-records: low watermark moves, offsets never reused ; compaction keeps offsets (gaps), tombstone = key + None
adding partitions remaps keys ; producers refresh metadata lazily
kafka-configs.sh --describe --topic T ; offsets_for_times ; kafka-delete-records.sh
```

- Segments are deleted whole; the last record too.
- `compact` is state, `delete` is history.
- Plan the number of partitions in advance.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [03 · Consumers and groups](../03-consumers-and-groups/expert.en.md)
- Next module: [05 · Delivery semantics](../05-delivery-semantics/overview.en.md)

## Sources

- [Apache Kafka: Topic configs](https://kafka.apache.org/documentation/#topicconfigs)
- [Apache Kafka: Log compaction](https://kafka.apache.org/documentation/#compaction)
- [Apache Kafka: Broker configs](https://kafka.apache.org/documentation/#brokerconfigs)
