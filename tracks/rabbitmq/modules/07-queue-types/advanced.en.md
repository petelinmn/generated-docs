---
title: "Queue types: advanced"
lang: en
track: rabbitmq
kind: module
doc: advanced
slug: 07-queue-types
part: 2
tags: [rabbitmq, quorum-queue, delivery-limit, poison-message, stream, stream-offset]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Queue types: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run on RabbitMQ 4.4.1 (one node) with pika 1.5.0.

## Delivery limit

A quorum queue counts how many times a message has been returned to the queue and, once `x-delivery-limit` is exceeded, sends it to the DLX ([module 06](../06-dead-lettering-and-retries/basics.en.md)) or drops it. It is a guard against poison messages.

```python
ch.queue_declare("qqd", durable=True)
ch.queue_declare("qq", durable=True, arguments={
    "x-queue-type": "quorum", "x-delivery-limit": 3,
    "x-dead-letter-exchange": "", "x-dead-letter-routing-key": "qqd"})
```

The same message was returned to the queue in several ways (limit 3):

| Way | Deliveries | In the DLX |
|---|---|---|
| `basic_get` + `basic_reject(requeue=True)` | 4 | yes |
| consumer + `basic_reject(requeue=True)` | 4 | yes |
| connection dropped before the ack | 4 | yes |
| `basic_get` + `basic_nack(requeue=True)` | 8 (we stopped) | no |
| consumer + `basic_nack(requeue=True)` | 582 in 2 s | no |

Output of the snippets:

```text
basic_get+reject(requeue): [(0, None, False), (1, 1, True), (2, 2, True), (3, 3, True)]
dead: 1 qq: 0
crash rounds delivered: 4 dead: 1 qq: 0
A get+nack: deliveries 8 dead 0
C consumer+reject: deliveries 4 [None, 1, 2, 3] dead 1
```

What you can see:

- The header `x-delivery-count` appears on repeat deliveries and grows: `1, 2, 3`.
- Once the limit is exceeded (the fourth delivery is three returns) the message goes to the DLX with reason `delivery_limit`:

```text
dead header: {'x-death': '...', 'x-first-death-exchange': '', 'x-first-death-queue': 'qq', 'x-first-death-reason': 'delivery_limit', 'x-last-death-exchange': '', 'x-last-death-queue': 'qq', 'x-last-death-reason': 'delivery_limit'}
```

- **`basic_nack(requeue=True)` did not increase the count in this test**, and the message went around forever. This explains the result in module 06: the consumer there used `nack`, so the limit "did not trigger".

> ⚠️ Verify: why a `nack` with `requeue=True` is not counted (documented behaviour or a quirk of 4.4.1) was not established. To guard against poison messages use `reject` or `requeue=False` with a DLX.

## Stream offsets

The reading position is set by the `x-stream-offset` argument of `basic_consume`. The log is `e0…e5`.

| Value | Result (verified) |
|---|---|
| `"first"` | `e0…e5` |
| `"last"` | `e0…e5` (the last written **chunk**, not the last message) |
| `"next"` | empty until new messages arrive |
| the number `3` | `e3, e4, e5` |

```text
offset 'first' ['e0', 'e1', 'e2', 'e3', 'e4', 'e5']
offset 'last' ['e0', 'e1', 'e2', 'e3', 'e4', 'e5']
offset 'next' []
offset 3 ['e3', 'e4', 'e5']
```

`last` starts at the last **chunk** — a block of messages written together; six messages published in a row ended up in one chunk, so everything was read.

> ⚠️ Verify: how chunks are formed and the `timestamp` value of `x-stream-offset` were not verified here.

## A quorum queue and familiar features

In the test a quorum queue accepted `x-message-ttl` and a DLX. Priorities (`x-max-priority`) and `exclusive` are not supported ([basics](basics.en.md#limits-of-quorum-queues-and-streams)). The rest of the feature list by type is in the [expert theory](expert.en.md#reference).

## Best practices

- For important tasks use a quorum queue with `x-delivery-limit` and a DLX; on an unfixable error the consumer should `reject(requeue=False)`, not `nack(requeue=True)`.
- Do not change the type on the fly: plan a migration (a new queue, move, switch).
- Use a stream for events that different consumers read from different positions; messages are removed by retention policy, not by ack.
- Watch a stream's size (a retention policy by time or size).

## Common mistakes

- ❌ Relying on the delivery limit with `nack(requeue=True)` → ✅ `reject` or a DLX.
- ❌ Reading a stream with `auto_ack` → ✅ prefetch and ack.
- ❌ Assuming `last` returns one message → ✅ it is the last chunk.
- ❌ Declaring a quorum queue without `durable` → ✅ `durable=True`.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [06 · Dead-lettering and retries](../06-dead-lettering-and-retries/advanced.en.md) — the `delivery_limit` reason
- Next module: [08 · Clustering and high availability](../08-clustering/overview.en.md)

## Sources

- [Quorum Queues: Poison Message Handling](https://www.rabbitmq.com/docs/quorum-queues)
- [Streams and Super Streams](https://www.rabbitmq.com/docs/streams)
- [Dead Letter Exchanges](https://www.rabbitmq.com/docs/dlx)
