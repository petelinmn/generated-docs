---
title: "Flow control and performance: expert"
lang: en
track: rabbitmq
kind: module
doc: expert
slug: 10-performance
part: 3
tags: [rabbitmq, performance, flow-control, credit-flow, prefetch, benchmark, tuning, disk-free-limit]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Flow control and performance: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on a single RabbitMQ 4.4.1 node on a local machine; the numbers are not a forecast for your system.

## Reference

### Defaults read from the broker (`rabbitmqctl environment`)

| Parameter | Value | Meaning |
|---|---|---|
| `heartbeat` | 60 | heartbeat interval, s |
| `frame_max` | 131072 | maximum frame size, bytes |
| `channel_max` | 256 | as printed by `environment` |
| `credit_flow_default_credit` | `{400,200}` | credit flow parameters |
| `disk_free_limit` | 50000000 | free-space threshold, bytes |
| `vm_memory_high_watermark` | 0.6 | memory fraction for the alarm |

> ⚠️ Verify: the actual effect of `channel_max` and the meaning of the fields of `credit_flow_default_credit` come from the output and the documentation; the behaviour when they are changed was not verified here.

### Measured ratios (one machine)

| Experiment | Result |
|---|---|
| publishing without confirms | 62–65k messages/s (client side) |
| publishing with pipelined confirms | ≈40k messages/s |
| message 100 B / 10 KB / 100 KB, persistent with confirms | 32.8 / 24.3 / 3.8k messages/s (3.3 / 243 / 383 MB/s) |
| reading prefetch 1 / 10 / 100 / 1000, ack each | 1.8 / 9.9 / 24.2 / 29.4k messages/s |
| reading prefetch 1000, `multiple` ack every 100 | 66.8k messages/s |
| classic / quorum / stream, 5000 persistent with confirms | 31.6 / 23.3 / 36.9k messages/s |

### Alarms

| Alarm | `Connection.Blocked` reason | Verified |
|---|---|---|
| memory | `low on memory` | yes |
| disk | `low on disk` | yes |
| connection state `flow` | overload inside the broker | not reproduced |

## Internals

- **Per-message overhead** dominates for small messages: so small messages give many messages per second but little MB/s (verified).
- **Prefetch hides network latency:** at prefetch 1 each message waits for a round trip "message — ack"; at larger values the pipeline is full.
- **A multiple ack saves frames:** one frame acknowledges a range of tags.
- **Quorum pays for replication:** the confirm comes after a majority accepts.
- **Load shedding on a queue:** credit flow passes back-pressure to the publisher.

> ⚠️ Verify: the internals of credit flow, batched disk writes and the exact reasons for the difference between queue types are described from the documentation and were not verified here.

## Gotchas

### 1. The "no confirms" speed is the client's

A client without confirms measures how fast it writes to a socket, not the broker (62k against 40k with confirms).

### 2. Persistent is not always slower

On a fast disk the difference from transient is almost zero (verified); on a slow one it is visible.

### 3. `prefetch_count=1` is tens of times slower

1.8k against 29.4k messages/s at prefetch 1000.

### 4. A multiple ack needs idempotency

If a consumer crashes between acks, up to a hundred messages arrive again.

### 5. The disk limit must be above the free space

The disk alarm fires when free space is below the limit; 900 GB with ≈990 GB free caused no alarm, 2000 GB did (verified).

### 6. MB/s and messages/s move in opposite directions

For large messages messages per second fall while MB/s rises.

### 7. The `flow` state is not always visible

On a fast machine 300,000 publishes did not cause `flow` (verified).

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| A large prefetch | throughput | unfair distribution, more redelivery on a crash |
| A small prefetch | fairness | slower |
| Multiple acks | faster | redelivery of up to N messages |
| Quorum | durability | slower than classic |
| Stream | high write rate, re-reading | a different model ([module 07](../07-queue-types/basics.en.md)) |
| Data in storage, a reference in the message | light messages | an extra component |

## Interview questions

<details><summary>1. What affects reading speed most?</summary>

`prefetch_count`: at 1 each message waits for an ack round trip; hundreds speed reading up by an order of magnitude (1.8 against 24k messages/s in the test).

→ [Basics](basics.en.md#reading-prefetch-and-acks)

</details>

<details><summary>2. Trick: publishing without confirms showed 62k messages/s. Is that the broker's speed?</summary>

No: it is the client's write speed into a socket; with confirms it was ≈40k.

→ [Gotcha 1](#1-the-no-confirms-speed-is-the-clients)

</details>

<details><summary>3. What is a multiple ack and what is risky about it?</summary>

`ack(tag, multiple=True)` acknowledges all messages up to the tag; after a crash the unacknowledged ones return, so an idempotent handler is needed.

→ [Advanced theory](advanced.en.md#multiple-acks)

</details>

<details><summary>4. Trick: is persistent always noticeably slower than transient?</summary>

No: on a fast disk the difference in the test is almost zero.

→ [Gotcha 2](#2-persistent-is-not-always-slower)

</details>

<details><summary>5. How does message size matter?</summary>

Larger size — fewer messages per second, more MB/s; very large data is better stored separately.

→ [Basics](basics.en.md#message-size)

</details>

<details><summary>6. Which flow-control mechanisms does the broker have?</summary>

Credit flow (inside the broker and up to the publisher), memory/disk alarms, confirms and prefetch on the client side.

→ [Advanced theory](advanced.en.md#flow-control)

</details>

<details><summary>7. Trick: a disk limit of 900 GB with ≈990 GB free. Will there be an alarm?</summary>

No: the alarm fires when free space is below the limit.

→ [Gotcha 5](#5-the-disk-limit-must-be-above-the-free-space)

</details>

<details><summary>8. What does a client get on a disk alarm?</summary>

`Connection.Blocked` with the reason `low on disk`.

→ [Advanced theory](advanced.en.md#the-disk-alarm)

</details>

<details><summary>9. What does a quorum queue pay for durability with?</summary>

Speed: the write is replicated and confirmed by a majority (23k against 32k messages/s for classic in the test).

→ [Advanced theory](advanced.en.md#queue-type-and-speed)

</details>

<details><summary>10. Trick: how do you fairly compare two configurations?</summary>

Measure in the target environment, with real message sizes and load, repeating the runs; one run on a laptop proves nothing.

→ [Basics](basics.en.md#how-the-code-was-verified)

</details>

<details><summary>11. When is it safe to raise prefetch?</summary>

When tasks are short and uniform and fairness is not critical; long tasks keep a small one.

→ [Trade-offs](#trade-offs-and-alternatives)

</details>

<details><summary>12. Why put large data in storage and a reference in the message?</summary>

Messages stay light, the broker does not hit bandwidth and disk limits, and messages per second rise.

→ [Basics](basics.en.md#message-size)

</details>

## Cheat sheet

```text
publish : pipelined confirms (not one-by-one); persistent only when needed
consume : prefetch 10-1000 for fast tasks, 1-few for slow; multiple=True acks need idempotent handlers
alarms  : memory/disk -> Connection.Blocked (low on memory | low on disk); disk_free_limit < free space to avoid
measure : your hardware, your sizes; numbers here are ratios from one laptop-class machine
```

- Pipelined confirms, prefetch per task.
- Do not trust numbers you have not measured yourself.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [09 · Monitoring and operations](../09-monitoring/expert.en.md)
- Next module: [11 · Security](../11-security/overview.en.md)

## Sources

- [Flow Control](https://www.rabbitmq.com/docs/flow-control)
- [Consumer Prefetch](https://www.rabbitmq.com/docs/consumer-prefetch)
- [Disk Alarms](https://www.rabbitmq.com/docs/disk-alarms)
- [Reliability Guide](https://www.rabbitmq.com/docs/reliability)
