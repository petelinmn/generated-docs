---
title: "Consumers and groups: basic level"
lang: en
track: kafka
kind: module
doc: basics
slug: 03-consumers-and-groups
part: 1
tags: [kafka, consumer, consumer-group, offset-commit, rebalance, at-least-once]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Consumers and groups: basic level

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · Kafka basics](../01-kafka-basics/basics.en.md), [02 · Producers](../02-producers/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

The code was run on Apache Kafka 4.3.1 (one broker, KRaft, image `apache/kafka:4.3.1`) in Docker with `confluent-kafka` 2.16.2; outputs are copied from the runs. Helpers: `fresh(topic, parts=3)` recreates a topic; `fill(topic, n)` writes `n` records evenly into three partitions; `mk(group, **kw)` creates a `Consumer` with `auto.offset.reset=earliest` and auto-commit **off** (`enable.auto.commit=False`); `pump(consumers, secs)` calls `poll` on each for `secs` seconds; `asg(c)` returns the sorted numbers of the assigned partitions. Groups were created with random names (`grp-xxxxx`).

## A consumer group

A **consumer group** is a set of processes with one `group.id` that read a topic together. The rule: **each partition is assigned to exactly one member of the group**, so inside a group a record is handled by one consumer and the partitions are divided among the members.

```python
a = mk(g); a.subscribe(["g1"]); pump([a], 6)
print("one member:", asg(a))
b = mk(g); b.subscribe(["g1"]); pump([a, b], 10)
print("two members:", asg(a), asg(b))
```

```text
one member: [0, 1, 2]
two members: [0, 1] [2]
```

One member got all three partitions, and after a second joined the partitions were divided. That is **scaling reads**: add a consumer and the load splits.

### More consumers than partitions

```python
c3 = mk(g); c3.subscribe(["g1"]); d = mk(g); d.subscribe(["g1"]); pump([a, b, c3, d], 12)
print("four members on 3 partitions:", [asg(x) for x in (a, b, c3, d)])
```

```text
four members on 3 partitions: [[1], [2], [0], []]
```

The fourth member was left **without partitions**: a partition is not shared among a group's consumers, so the number of active readers is at most the number of partitions. The extra consumer is a standby in case a neighbour fails.

### A member leaving

When two members closed, the remaining ones divided the partitions again:

```text
after two leave: [0, 1] [2]
```

Such a redistribution of partitions is called a **rebalance**; details in the [advanced theory](advanced.en.md#rebalancing).

## Committing offsets

To continue from where it stopped after a restart, a group **commits** its position. Until a commit there is no stored position:

```python
print("committed before commit:", [(tp.partition, tp.offset) for tp in c.committed([...])])
c.commit(asynchronous=False)
print("committed after commit :", [(tp.partition, tp.offset) for tp in c.committed([...])])
```

```text
read: 9
committed before commit: [(0, -1001), (1, -1001), (2, -1001)]
committed after commit : [(0, 3), (1, 3), (2, 3)]
```

`-1001` means "no position" (`OFFSET_INVALID`). After `commit` every partition has offset 3 committed: three records (0, 1, 2) were read from each, and what is committed is the number of the **next** record to read.

### The committed offset is the next one to read

```text
read offset 0 partition 2 -> committed [(2, 1)]
```

The record at offset 0 was handled — offset 1 is committed. If you commit manually per message, commit `offset + 1`; the method `commit(message=m)` does that itself.

## What happens without a commit

A consumer read six records, closed **without** a `commit` (auto-commit off) and was started again in the same group:

```text
processed without commit, restarted -> redelivered: 6 6 True
```

The same six records were read again. That is how at-least-once delivery works: if you processed but did not manage to commit, after a failure the record comes again ([module 05](../../overview.en.md)). Commit **after** processing and make the handler idempotent.

## Reading from a given position

You can skip the group and the broker's position and assign a partition and offset yourself:

```python
c.assign([TopicPartition("g6", 0, 2)])     # start at offset 2 of partition 0
c.seek(TopicPartition("g6", 0, 0))          # go back to offset 0
```

```text
assign at offset 2: [(2, 'm6')]
after seek(0): 0 m0
```

Partition 0 held the records `m0`, `m3`, `m6` at offsets 0, 1, 2, so offset 2 is the record `m6`, and after `seek(0)` `m0` is read. This is how you re-read data or skip a problem stretch.

## Common beginner mistakes

- ❌ Expecting two consumers of one group to get the same record → ✅ inside a group one consumer gets it.
- ❌ More consumers than partitions for speed → ✅ the extras idle.
- ❌ Committing the position before processing → ✅ after; otherwise a record is lost on a crash.
- ❌ Forgetting that the committed offset is the next one to read → ✅ `offset + 1`.
- ❌ Different applications with one `group.id` → ✅ they will start sharing partitions.

## Related materials

- Next: [advanced theory](advanced.en.md) — rebalancing, `max.poll.interval.ms`, strategies
- Before: [02 · Producers](../02-producers/basics.en.md)
- Next module: [04 · Topics, retention and compaction](../../overview.en.md)
- Analogue in another track: [RabbitMQ: acknowledgements and prefetch](../../../rabbitmq/modules/03-queues-and-delivery/basics.en.md)

## Sources

- [Apache Kafka: Consumer configs](https://kafka.apache.org/documentation/#consumerconfigs)
- [Apache Kafka: Design: the consumer](https://kafka.apache.org/documentation/#design_consumer)
- [confluent-kafka-python: Consumer](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html#pythonclient-consumer)
