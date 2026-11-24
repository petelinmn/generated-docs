---
title: "Flow control and performance: basics"
lang: en
track: rabbitmq
kind: module
doc: basics
slug: 10-performance
part: 3
tags: [rabbitmq, performance, throughput, publisher-confirms, prefetch, message-size, benchmark]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Flow control and performance: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [05 · Reliability](../05-reliability/basics.en.md), [09 · Monitoring](../09-monitoring/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

One RabbitMQ 4.4.1 node in Docker (Docker Desktop on Windows) and pika 1.5.0 on the same machine; outputs are copied from the runs. **Absolute numbers do not transfer to your hardware and network:** the broker and client share one machine, the disk is fast, and there is one Python client process. The **ratios** between variants are what is useful. Helpers: `publish_confirmed_pipeline(ch, q, n, props, body)` publishes `n` messages with pipelined confirms (see [module 05](../05-reliability/advanced.en.md#throughput)) and returns the time; `rate(n, s)` prints "messages per second".

> A benchmark that was not measured on your system proves nothing. Measure under your own load: message size, number of publishers and consumers, queue type, disk, network.

## What determines speed

| Factor | Effect |
|---|---|
| publisher confirms | one by one — very slow, pipelined — fast |
| consumer acks and prefetch | prefetch 1 — the slowest reading |
| message size | MB/s rises, messages/s falls |
| queue type | classic, quorum and stream differ |
| persistent and durable | extra disk writes |

## Publishing

20,000 messages of 100 bytes into a durable queue, no confirms (measured on the client side):

```text
publish transient no confirms: 62,109 msg/s (client side)
   queued: 20000
publish persistent no confirms: 65,318 msg/s (client side)
   queued: 20000
```

Without confirms the client merely pushes data into a socket: that rate is not the broker's rate. With pipelined confirms:

```text
publish transient pipelined confirms: 40,713 msg/s acked=20000
publish persistent pipelined confirms: 40,306 msg/s acked=20000
```

- Confirms cost throughput (≈40k against ≈62k messages/s) but give certainty of delivery ([module 05](../05-reliability/basics.en.md)).
- Persistent and transient barely differed in this test: the disk is fast and the broker writes in batches. On a slow disk the difference would be larger.

## Message size

3000 persistent messages with confirms:

```text
size     100 B persistent confirmed: 32,759 msg/s = 3.3 MB/s
size   10000 B persistent confirmed: 24,308 msg/s = 243.1 MB/s
size  100000 B persistent confirmed: 3,831 msg/s = 383.1 MB/s
```

As size grows there are fewer messages per second but more MB/s: for small messages the bottleneck is per-message overhead, for large ones bandwidth and disk. Very large messages are better kept out of the broker: put the data in storage and a reference in the message.

## Reading: prefetch and acks

10,000 messages, the consumer acks each:

```text
consume prefetch    1, ack each: 1,764 msg/s
consume prefetch   10, ack each: 9,916 msg/s
consume prefetch  100, ack each: 24,220 msg/s
consume prefetch 1000, ack each: 29,406 msg/s
```

`prefetch_count=1` is the slowest: the broker waits for an ack before sending the next message, so every message pays a network round trip. Raising prefetch to a hundred speeds reading up sharply, beyond that the gain is small. Fair distribution of slow tasks ([module 03](../03-queues-and-delivery/basics.en.md#prefetch-and-fairness)) needs a small prefetch: that is the trade-off.

## Common beginner mistakes

- ❌ Believing other people's benchmarks → ✅ measure on your own system.
- ❌ Publishing without confirms for speed when messages matter → ✅ pipelined confirms.
- ❌ `prefetch_count=1` for fast short tasks → ✅ prefetch of tens or hundreds.
- ❌ Pushing multi-megabyte messages through the broker → ✅ a reference to an object in storage.
- ❌ Measuring the client's speed instead of the broker's → ✅ count confirmed and consumed messages.

## Related materials

- Next: [advanced theory](advanced.en.md) — multiple acks, queue types, flow control
- Before: [09 · Monitoring and operations](../09-monitoring/basics.en.md), [05 · Reliability](../05-reliability/basics.en.md)
- Next module: [11 · Security](../11-security/overview.en.md)

## Sources

- [Reliability Guide](https://www.rabbitmq.com/docs/reliability)
- [Consumer Prefetch](https://www.rabbitmq.com/docs/consumer-prefetch)
- [Publishers](https://www.rabbitmq.com/docs/publishers)
