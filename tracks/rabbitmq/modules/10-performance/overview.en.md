---
title: "Flow control and performance"
lang: en
track: rabbitmq
kind: module
doc: overview
slug: 10-performance
part: 3
levels: [1, 2, 3, 4]
tags: [rabbitmq, performance, flow-control, credit-flow, prefetch, disk-alarm, benchmark]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Flow control and performance

🌐 [Русский](overview.ru.md) · **English**

## In short

RabbitMQ's speed is set by publisher confirms, `prefetch` and consumer acks, message size, queue type and disk/memory settings. The broker protects itself from overload with flow control (credit flow) and memory and disk alarms that block publishers. The numbers in the material are **ratios** on one machine, not a forecast for your system.

## Key ideas

- **Pipelined confirms** instead of one by one; without confirms you measure the client, not the broker.
- **`prefetch_count=1` is tens of times slower** (1.8 against 29.4k messages/s); for long tasks it is the price of fairness.
- **A multiple ack** doubles reading speed (66.8k) but needs idempotency.
- **Size:** larger messages — fewer messages/s and more MB/s.
- **Quorum is slower than classic** (23k against 32k) — the price of replication.
- **Alarms** on memory/disk block publishers: `Connection.Blocked` (`low on memory`/`low on disk`).
- **The disk limit** fires when free space is below the limit.

## Minimal example

```python
y.basic_qos(prefetch_count=1000)
y.basic_consume("perf", cb)   # cb acks every 100th message with multiple=True
```

Everything was run against a real RabbitMQ 4.4.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Task | Setting |
|---|---|
| Many fast short tasks | prefetch in the hundreds, multiple acks |
| Long unequal tasks | prefetch 1 to a few |
| Important data | confirms + quorum, with the cost assessed |
| Huge data | storage + a reference in the message |

- ❌ Do not decide from other people's benchmarks.
- ❌ Do not tune blindly: measure first.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | what determines speed, publishing, size, prefetch |
| [2. Advanced theory](advanced.en.md) | multiple acks, queue types, flow control, the disk alarm |
| [3. Expert theory](expert.en.md) | defaults, a table of measurements, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn rabbitmq 10-performance level 5` |
| 6. Tests | not created yet — `/learn rabbitmq 10-performance level 6` |

## Related materials

- Prerequisites: [05 · Reliability](../05-reliability/overview.en.md), [09 · Monitoring and operations](../09-monitoring/overview.en.md)
- Next: [11 · Security](../11-security/overview.en.md)
- Analogues in other tracks: [Linux](../../../linux/overview.en.md)
