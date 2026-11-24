---
title: "Replication and availability: expert level"
lang: en
track: kafka
kind: module
doc: expert
slug: 07-replication-and-availability
part: 2
tags: [kafka, replication, isr, min-insync-replicas, leader-election, kraft, acks]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Replication and availability: expert level

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on a three-node (combined nodes) Apache Kafka 4.3.1 cluster.

## Reference

### Fields of `kafka-topics.sh --describe`

| Field | Meaning | Verified |
|---|---|---|
| `Leader` | the partition's leader broker | yes |
| `Replicas` | brokers with copies; the first is the preferred leader | yes |
| `Isr` | in-sync replicas | yes |
| `Elr`, `LastKnownElr` | fields of the "eligible leader replicas" mechanism | printed empty, meaning not verified |

### Settings

| Setting | Where | Purpose | Verified |
|---|---|---|---|
| `replication.factor` | topic (at creation) | number of copies | yes (3) |
| `min.insync.replicas` | topic | minimum in-sync replicas for `acks=all` | yes (2) |
| `acks` | producer | `1`, `all` | yes |
| `unclean.leader.election.enable` | topic/broker | electing a lagging replica leader | not tested |
| `replica.lag.time.max.ms` | broker | how long a lag lasts before leaving the ISR | not tested |
| `auto.leader.rebalance.enable`, `leader.imbalance.check.interval.seconds` | broker | returning preferred leaders | not tested |
| `broker.rack`, `default.replication.factor` | broker | racks and the default RF | not tested |

### Experiment results (3 brokers, RF=3, `min.insync.replicas=2`)

| Brokers alive | `acks=all` | `acks=1` | Metadata (creating a topic) |
|---|---|---|---|
| 3 | `ok` | `ok` | work |
| 2 | `ok` | — | — |
| 1 | `_MSG_TIMED_OUT` | `ok` | timeout (no controller quorum) |

### Tools (verified)

| Command | Result |
|---|---|
| `kafka-topics.sh --describe --topic T` | leaders, replicas, ISR |
| `kafka-topics.sh --describe --under-replicated-partitions` | empty on a healthy cluster |
| `kafka-metadata-quorum.sh describe --status` | quorum leader, epoch, observers |
| `kafka-leader-election.sh`, `--unavailable-partitions` | leader election, partitions with no leader | not tested |

## Internals

- **Followers pull data from the leader**; the ISR holds those that keep up (the ISR was seen to shrink and recover).
- **The leader is chosen from the ISR** (observed: broker 1 from the ISR `[1, 2]`).
- **An `acks=all` write is accepted if the ISR holds at least `min.insync.replicas`**; otherwise the producer gets an error (in the experiment, a message timeout).
- **Metadata is managed by the controller quorum** (KRaft): without a majority it does not change (verified by the topic-creation timeout), although writing to existing partitions with a live leader continues (verified with `acks=1`).
- **The preferred leader does not return at once:** after recovery broker 2 stayed the leader of all partitions.

> ⚠️ Verify: the replication mechanism (the high watermark, leader epochs), eligible leader replicas, `acks=all` behaviour without a controller quorum and unclean elections were not verified here.

## Gotchas

### 1. `acks=all` without `min.insync.replicas`

With `min.insync.replicas=1` an `acks=all` write is accepted even from a single replica.

### 2. A timeout instead of a clear error

With too few in-sync replicas the client sees `_MSG_TIMED_OUT` (verified), not an immediate refusal.

### 3. `acks=1` goes through with a live leader

But it is not protected against losing it (verified: `ok` with one broker).

### 4. Metadata needs a controller quorum

With two of three combined nodes lost, creating a topic hangs (verified).

### 5. Leaders do not return by themselves

After recovery all partitions stayed with broker 2 (verified over 25 seconds).

### 6. Combined nodes tie fates together

Stopping a broker stops a controller too.

### 7. Two brokers with RF=3

`min.insync.replicas=2` tolerates the loss of only one.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| RF=3, `min.insync.replicas=2`, `acks=all` | durability, survives losing one broker | writes stop on two failures |
| `min.insync.replicas=1` | availability | a write may stay on one copy |
| `acks=1` | speed | loss when the leader fails |
| unclean elections | availability | records may be lost |
| dedicated controllers | metadata fault tolerance | more nodes |

## Interview questions

<details><summary>1. What are a leader and followers?</summary>

The leader serves a partition's writes and reads, followers copy data from it.

→ [Basics](basics.en.md#replication)

</details>

<details><summary>2. What is the ISR?</summary>

The replicas that keep up with the leader; a new leader is chosen from them.

→ [Basics](basics.en.md#replication)

</details>

<details><summary>3. What do `acks=all` and `min.insync.replicas=2` guarantee at RF=3?</summary>

A write is confirmed on at least two brokers; the cluster survives losing one.

→ [Basics](basics.en.md#acks-and-mininsyncreplicas)

</details>

<details><summary>4. Trick: `acks=all` but `min.insync.replicas=1`. Is it protected?</summary>

Weakly: a write may stay on one copy.

→ [Gotcha 1](#1-acksall-without-mininsyncreplicas)

</details>

<details><summary>5. What does a producer see with two of three brokers stopped?</summary>

With `acks=all` — `_MSG_TIMED_OUT`, with `acks=1` — success while the leader is alive.

→ [Basics](basics.en.md#acks-and-mininsyncreplicas)

</details>

<details><summary>6. Who chooses the new leader?</summary>

The KRaft quorum's controller, from the ISR.

→ [Advanced theory](advanced.en.md#leader-election)

</details>

<details><summary>7. Trick: can you create a topic if 1 of 3 combined nodes is alive?</summary>

No: there is no controller quorum (a timeout).

→ [Gotcha 4](#4-metadata-needs-a-controller-quorum)

</details>

<details><summary>8. What is the preferred leader?</summary>

The first broker in the replica list; leaders may not return to it after recovery.

→ [Advanced theory](advanced.en.md#the-preferred-leader)

</details>

<details><summary>9. Which output checks replication health?</summary>

`--under-replicated-partitions`: empty means healthy.

→ [Advanced theory](advanced.en.md#watching-the-state)

</details>

<details><summary>10. Trick: does RF=3 protect against losing two brokers?</summary>

With `acks=all` + `min.insync.replicas=2` data survives only the loss of one; with two, writes stop.

→ [Gotcha 7](#7-two-brokers-with-rf3)

</details>

<details><summary>11. What is an unclean leader election?</summary>

Electing a lagging replica leader: availability at the price of possibly losing records (off by default — per the documentation).

→ [Advanced theory](advanced.en.md#leader-election)

</details>

<details><summary>12. Why is the combined mode risky in production?</summary>

Losing a broker means losing a controller; dedicated controllers are independent.

→ [Gotcha 6](#6-combined-nodes-tie-fates-together)

</details>

## Cheat sheet

```text
RF=3, min.insync.replicas=2, producer acks=all (+ idempotence)  -> survives 1 broker loss
kafka-topics.sh --describe [--under-replicated-partitions|--unavailable-partitions]
leader = from ISR ; preferred leader = first in Replicas (not restored automatically in our run)
metadata changes need a controller majority (KRaft)
```

- RF=3 and `min.insync.replicas=2` for important topics.
- Watch under-replicated partitions.
- Upgrade brokers one at a time.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [06 · Schemas and serialization](../06-schemas-and-serialization/expert.en.md)
- Next module: [08 · Kafka Connect](../08-kafka-connect/overview.en.md)

## Sources

- [Apache Kafka: Replication](https://kafka.apache.org/documentation/#replication)
- [Apache Kafka: KRaft](https://kafka.apache.org/documentation/#kraft)
- [Apache Kafka: Topic configs](https://kafka.apache.org/documentation/#topicconfigs)
