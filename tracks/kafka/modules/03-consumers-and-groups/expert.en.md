---
title: "Consumers and groups: expert level"
lang: en
track: kafka
kind: module
doc: expert
slug: 03-consumers-and-groups
part: 1
tags: [kafka, consumer, consumer-group, rebalance, offset-commit, cooperative-sticky, seek]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Consumers and groups: expert level

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on a single Apache Kafka 4.3.1 broker with `confluent-kafka` 2.16.2.

## Reference

### Consumer parameters

| Parameter | Purpose | Verified |
|---|---|---|
| `group.id` | the group name | yes |
| `auto.offset.reset` (`earliest`, `latest`) | the starting position without a stored one | yes |
| `enable.auto.commit` | auto-commit | switched off explicitly |
| `partition.assignment.strategy` (`cooperative-sticky`) | the assignment strategy | yes (cooperative) |
| `group.protocol` (`consumer`) | the new group protocol | yes (works) |
| `max.poll.interval.ms` | the limit on a pause between `poll` calls | yes (`_MAX_POLL_EXCEEDED`) |
| `session.timeout.ms`, `heartbeat.interval.ms`, `max.partition.fetch.bytes`, `fetch.min.bytes`, `group.instance.id` | liveness, fetching, static membership | not tested |

### API

| Element | Behaviour | Verified |
|---|---|---|
| `subscribe(topics, on_assign=, on_revoke=)` | joining the group, rebalance handlers | yes |
| `poll(timeout)` | a message, `None` or a message with `error()` | yes |
| `assignment()` | the list of assigned `TopicPartition`s | yes |
| `commit(asynchronous=False)` | commits what was read | yes |
| `commit(message=m)` | commits `offset + 1` | yes |
| `committed([TopicPartition…], timeout)` | stored offsets; `-1001` — none | yes |
| `assign([TopicPartition(t, p, off)])`, `seek(tp)` | reading from a given position without a group | yes |
| `close()` | leaves the group | yes |

### Values and codes (verified)

| What | Value |
|---|---|
| no stored offset | `-1001` (`OFFSET_INVALID`) |
| a pause between `poll` longer than the limit | `_MAX_POLL_EXCEEDED` on the next `poll` |
| committed after reading offset 0 | 1 |
| members 1 / 2 / 4 on 3 partitions | `[0,1,2]` / `[0,1],[2]` / the fourth empty |

## Internals

- **A partition belongs to one group member** — the basis of parallelism and ordering.
- **The group coordinator** is the broker that keeps the group's membership and offsets (the internal topic `__consumer_offsets`, [module 01](../01-kafka-basics/advanced.en.md#internal-topics)).
- **Eager rebalancing revokes all partitions** from all members, then assigns anew (verified by the `revoke`/`assign` events).
- **Cooperative and new server-side rebalancing revoke only the partitions that move** (verified).
- **The committed offset is "the next to read"**; hence `offset + 1`.

> ⚠️ Verify: the role of the coordinator and group leader, the record format in `__consumer_offsets`, static membership (`group.instance.id`) and how `session.timeout.ms` works were not verified here.

## Gotchas

### 1. Extra consumers idle

A fourth member on three partitions got none (verified).

### 2. Eager rebalancing stops everyone

Everyone loses all partitions, even unchanged ones (verified).

### 3. Committing `offset` instead of `offset + 1`

It leads to the last record being processed again after a restart.

### 4. No commit means a repeat

What was read but not processed comes again without a commit (verified).

### 5. Long processing → `_MAX_POLL_EXCEEDED`

The consumer drops out of the group, the partitions go to others (verified by the error).

### 6. `assignment()` after the error

After `_MAX_POLL_EXCEEDED` the list of assigned partitions stayed the same (verified) until a redistribution passes; do not treat it as current.

### 7. One `group.id` for different applications

The applications share partitions and each sees only part of the data.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| auto-commit | simpler | unprocessed records may be lost |
| a manual commit after processing | at least once | duplicates, idempotence needed |
| eager rebalancing | simple | a pause for everyone |
| `cooperative-sticky` / the new protocol | soft rebalances | more complex, compatibility needed |
| `assign` without a group | full control of the position | no balancing between instances |

## Interview questions

<details><summary>1. What is a consumer group?</summary>

A set of consumers with one `group.id`; a topic's partitions are divided among them, each to one member.

→ [Basics](basics.en.md#a-consumer-group)

</details>

<details><summary>2. Trick: a topic has 3 partitions, the group has 4 consumers. What happens?</summary>

One is left without partitions.

→ [Gotcha 1](#1-extra-consumers-idle)

</details>

<details><summary>3. What is a rebalance and when does it happen?</summary>

A redistribution of partitions when the group's membership or the number of partitions changes.

→ [Advanced theory](advanced.en.md#rebalancing)

</details>

<details><summary>4. Why is cooperative rebalancing better than eager?</summary>

Only the moving partitions are revoked; the others are read without a pause.

→ [Advanced theory](advanced.en.md#cooperative-rebalancing)

</details>

<details><summary>5. What is committed after reading the record at offset 0?</summary>

Offset 1: the next one to read is committed.

→ [Basics](basics.en.md#the-committed-offset-is-the-next-one-to-read)

</details>

<details><summary>6. Trick: what if you process records and close the consumer without `commit` with auto-commit off?</summary>

After a restart the same records come again.

→ [Gotcha 4](#4-no-commit-means-a-repeat)

</details>

<details><summary>7. What does `-1001` mean in `committed`?</summary>

There is no stored position (`OFFSET_INVALID`).

→ [Basics](basics.en.md#committing-offsets)

</details>

<details><summary>8. What is `max.poll.interval.ms` for?</summary>

To detect a stuck consumer: after too long a pause between `poll` calls it drops out (`_MAX_POLL_EXCEEDED`).

→ [Advanced theory](advanced.en.md#member-liveness-maxpollintervalms)

</details>

<details><summary>9. Trick: can you read without a group?</summary>

Yes: `assign` with a partition and offset and `seek`; there is no balancing between instances.

→ [Basics](basics.en.md#reading-from-a-given-position)

</details>

<details><summary>10. Auto-commit or manual?</summary>

For critical data manual after processing (at least once and idempotence); auto-commit is simpler but may lose unprocessed records.

→ [Advanced theory](advanced.en.md#commit-strategies)

</details>

<details><summary>11. What does a consumer's `close()` do?</summary>

It tells the broker about the exit, speeding up the rebalance.

→ [Advanced theory](advanced.en.md#best-practices)

</details>

<details><summary>12. Trick: two different applications use one `group.id`. What happens?</summary>

They share the partitions, and each gets only part of the records.

→ [Gotcha 7](#7-one-groupid-for-different-applications)

</details>

## Cheat sheet

```text
Consumer({"group.id":g, "auto.offset.reset":"earliest", "enable.auto.commit":False, "partition.assignment.strategy":"cooperative-sticky"})
subscribe([t], on_assign=, on_revoke=) ; poll(1.0) ; commit(message=m) | commit() ; committed([...]) ; close()
committed offset = next to read (offset+1) ; -1001 = none ; _MAX_POLL_EXCEEDED = poll pause too long
assign([TopicPartition(t,p,off)]) ; seek(tp)  # no group balancing
```

- A partition to one group member; consumers no more than partitions.
- Commit after processing, `offset + 1`.
- Do not block `poll`.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [02 · Producers](../02-producers/expert.en.md)
- Next module: [04 · Topics, retention and compaction](../../overview.en.md)

## Sources

- [Apache Kafka: Consumer configs](https://kafka.apache.org/documentation/#consumerconfigs)
- [Apache Kafka: Consumer rebalance protocol](https://kafka.apache.org/documentation/#consumer_rebalance_protocol)
- [librdkafka: Configuration](https://github.com/confluentinc/librdkafka/blob/master/CONFIGURATION.md)
