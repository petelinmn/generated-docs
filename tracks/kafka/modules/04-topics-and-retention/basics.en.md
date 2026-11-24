---
title: "Topics, retention and compaction: basic level"
lang: en
track: kafka
kind: module
doc: basics
slug: 04-topics-and-retention
part: 1
tags: [kafka, retention, segment, log-compaction, topic-config, delete-records]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Topics, retention and compaction: basic level

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · Kafka basics](../01-kafka-basics/basics.en.md), [03 · Consumers and groups](../03-consumers-and-groups/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

The code was run on Apache Kafka 4.3.1 (one broker, KRaft, `apache/kafka:4.3.1`) in Docker with `confluent-kafka` 2.16.2; outputs are copied from the runs. **To avoid waiting minutes the broker was started with accelerated checks:** `log.retention.check.interval.ms=2000` and `log.cleaner.backoff.ms=1000` (the defaults are 300000 and 15000 ms). For this a `server.properties` file with two added lines was copied into the container before it started (`docker cp … kafka:/mnt/shared/config/server.properties`); the image's defaults differ, and `log.retention.check.interval.ms` cannot be changed dynamically (`Cannot update these configs dynamically`). Helpers: `fresh(topic, parts=1, **cfg)` recreates a topic with settings (`retention_ms=…` becomes `retention.ms`); `wm(topic)` returns `(low, high)`; `read(topic, secs)` reads a topic from the start with a new group and returns `(offset, key, value)`. The reader needs a few seconds to join the group: a short window gives an empty result.

## Retention

Kafka does not delete records on reading ([module 01](../01-kafka-basics/basics.en.md#a-log-instead-of-a-queue)); a **retention policy** deletes them. The most common is by time: `retention.ms` (the image's default is `log.retention.hours=168`, that is 7 days). What is deleted is not single records but whole log **segments** ([module 01](../01-kafka-basics/basics.en.md#how-it-lies-on-disk)).

A topic with `retention.ms=4000` and `segment.ms=1500` (a segment closes in about 1.5 s):

```python
fresh("ret", retention_ms=4000, segment_ms=1500)
for i in range(5): p.produce("ret", value=f"old{i}".encode())
```

```text
t=0   watermarks: (0, 5)
t=8s  (one active segment, no roll yet) watermarks: (5, 5) readable: []
after roll +3s watermarks: (5, 7) readable: []
after roll +6s watermarks: (7, 7) readable: []
```

- After 8 seconds **all five records were deleted**: `low` reached 5 and `high` is 5 too — the log is empty.
- New records continued numbering from 5 (offsets are **not reused**): after two new records `high` is 7; then they expired too (`low` 7).

> In the label "one active segment, no roll yet" I assumed an active segment is not deleted. The observation **refuted** that: the segment closed by `segment.ms` and was deleted whole. Do not count on a topic's last records outliving the retention period.

## Deleting records by hand

`kafka-delete-records.sh` moves a partition log's lower bound:

```text
Records delete operation completed:
partition: dr-0	low_watermark: 6
dr before: (0, 10)
dr after : (6, 10)
```

The records at offsets 0–5 became unavailable (`low` = 6), and `high` stayed 10.

## Topic settings

Topic settings are given at creation or changed later; `kafka-configs.sh --describe --topic …` shows them:

```text
Dynamic configs for topic cmp are:
  cleanup.policy=compact sensitive=false synonyms={DYNAMIC_TOPIC_CONFIG:cleanup.policy=compact, DEFAULT_CONFIG:log.cleanup.policy=delete}
```

A topic setting (`DYNAMIC_TOPIC_CONFIG`) overrides the broker default (`DEFAULT_CONFIG`): for `cleanup.policy` the default is `delete`.

### A segment size limit

```text
Invalid value 100000 for configuration segment.bytes: Value must be at least 1048576
```

A segment cannot be smaller than 1 MiB. That is why the experiments limited segments by time (`segment.ms`).

## Searching by time

Each record has a timestamp; you can find an offset by it:

```python
res = c.offsets_for_times([TopicPartition("tsq", 0, mid)], timeout=10)
```

```text
first offset with timestamp >= mid: 1
```

Records `a` (before `mid`) and `b` (after): the first offset with a timestamp not earlier than `mid` is 1, that is record `b`. This is how you rewind reading to "an hour ago".

## Common beginner mistakes

- ❌ Assuming a topic keeps data forever → ✅ `retention.ms` applies (168 h by default).
- ❌ Expecting the last record to outlive the period → ✅ segments are deleted whole, the active one closes by `segment.ms` too.
- ❌ Expecting offsets to be reused → ✅ numbering does not roll back.
- ❌ Expecting deletion "exactly on time" → ✅ the check is periodic (every 5 minutes by default).
- ❌ Reading with a just-created group in a short window → ✅ give it time to join the group.

## Related materials

- Next: [advanced theory](advanced.en.md) — compaction, adding partitions
- Before: [03 · Consumers and groups](../03-consumers-and-groups/basics.en.md)
- Next module: [05 · Delivery semantics](../05-delivery-semantics/overview.en.md)
- Analogue in another track: [RabbitMQ: TTL and length limits](../../../rabbitmq/modules/03-queues-and-delivery/advanced.en.md)

## Sources

- [Apache Kafka: Topic configs](https://kafka.apache.org/documentation/#topicconfigs)
- [Apache Kafka: Log](https://kafka.apache.org/documentation/#log)
- [Apache Kafka: Basic Kafka operations](https://kafka.apache.org/documentation/#basic_ops)
