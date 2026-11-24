---
title: "Topics, retention and compaction: advanced level"
lang: en
track: kafka
kind: module
doc: advanced
slug: 04-topics-and-retention
part: 1
tags: [kafka, log-compaction, tombstone, partitions, key-mapping, producer-metadata]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Topics, retention and compaction: advanced level

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md) (the same broker with accelerated checks). The code was run on Apache Kafka 4.3.1 with `confluent-kafka` 2.16.2.

## Log compaction

The policy `cleanup.policy=compact` keeps not "records for a period" but **the latest value of each key**: older records with the same key are removed. This is how state is stored (a user profile, settings) rather than a history of events.

A topic with `cleanup.policy=compact`, `segment.ms=1000`, `min.cleanable.dirty.ratio=0.01`. Three keys were updated four times (12 records):

```text
before: 12 records
after compaction: [(9, 'k1', 'k1-v3'), (10, 'k2', 'k2-v3'), (11, 'k3', 'k3-v3'), (12, 'k9', 'roll'), (13, 'k9', 'roll2')]
```

- One latest record remained for `k1`, `k2`, `k3` (`v3`).
- **Offsets were preserved** (9, 10, 11): the numbering now has gaps but a record keeps its number.
- Both records of key `k9` remained. The likely reason: they sat in the **active** segment, and per the documentation only closed segments are cleaned (after the later writes `roll` and `roll2` eventually vanished — see below).

Reading still goes by increasing offsets; a consumer just does not see the removed values.

### Tombstones

To delete a key you publish a record with that key and an **empty value** (`None`) — a tombstone:

```python
p.produce("cmp", key=b"k2", value=None)
```

```text
tombstone present: [(14, 'k2', None)]
after delete.retention.ms: [(14, 'k2', None)] all: [(9, 'k1', 'k1-v3'), (11, 'k3', 'k3-v3'), (14, 'k2', None), (15, 'k9', 'roll3'), (16, 'k9', 'roll4')]
```

The old value of `k2` (offset 10) disappeared, the tombstone (offset 14) remained. The broker also removes the tombstone itself after `delete.retention.ms` (3 seconds in the experiment); in our measurements it survived even ~20 s: cleaning applies only to closed segments, and this tombstone lay in the active one.

> ⚠️ Verify: exactly when the broker removes a tombstone (after the next cleaning of the segment holding it and once `delete.retention.ms` has elapsed) is not proven here: it was only observed to survive ~20 s.

Consumers must handle `value is None` as "the key was deleted".

## Compaction or retention

| Need | Policy |
|---|---|
| a history of events for a period | `delete` (by time/size) |
| the current state by key | `compact` |
| both | `compact,delete` |

> ⚠️ Verify: the value `compact,delete` and the parameters `min.compaction.lag.ms`, `max.compaction.lag.ms` were not verified here (only `min.compaction.lag.ms=0` was set).

## Adding partitions and keys

The number of partitions can be increased. But records with one key may start landing in a **different** partition:

```text
old producer after 3->6: True | new producer moved keys: {'user0': (2, 5), 'user2': (2, 5), 'user3': (2, 5), 'user5': (1, 4)}
```

The experiment: a topic of 3 partitions, 8 keys; partitions raised to 6.

- A **new** producer sent four keys to other partitions (`user0`: 2 → 5 and so on), the other four stayed.
- The **old** producer (created before the increase) kept writing by the old scheme: it did not learn of the new partitions, because it does not refresh metadata at once (the parameter `topic.metadata.refresh.interval.ms`, 5 minutes per the librdkafka documentation).

Consequence: one key's record order breaks — old records are in the old partition, new ones in another, and consumers may see them out of order. So plan the number of partitions in advance ([module 01](../01-kafka-basics/expert.en.md#1-partitions-cannot-be-reduced)).

> ⚠️ Verify: the default of `topic.metadata.refresh.interval.ms` comes from the librdkafka documentation and was not verified here; it was only observed that a producer created before the increase did not move to the new partitions during the experiment.

## Best practices

- Choose the policy for the data: events — `delete`, state — `compact`.
- Set `retention.ms` explicitly and keep headroom relative to consumer speed.
- Do not treat the last record as "eternal": segments are deleted whole.
- Plan the number of partitions with headroom; increasing it breaks per-key order.
- For compact topics handle tombstones (`value is None`).
- Do not change policies on a working topic without checking: what is deleted does not come back.

## Common mistakes

- ❌ `compact` for an event history → ✅ it keeps only the latest value of a key.
- ❌ Expecting compaction at once → ✅ it runs in the background, and not for the active segment.
- ❌ Not handling `None` in a compact topic → ✅ it is a key deletion.
- ❌ Adding partitions "on the fly" → ✅ keys will change partitions.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- Next module: [05 · Delivery semantics](../05-delivery-semantics/overview.en.md)

## Sources

- [Apache Kafka: Log compaction](https://kafka.apache.org/documentation/#compaction)
- [Apache Kafka: Topic configs](https://kafka.apache.org/documentation/#topicconfigs)
- [librdkafka: Configuration](https://github.com/confluentinc/librdkafka/blob/master/CONFIGURATION.md)
