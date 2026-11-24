---
title: "Queue types: expert"
lang: en
track: rabbitmq
kind: module
doc: expert
slug: 07-queue-types
part: 2
tags: [rabbitmq, queue-types, quorum-queue, stream, classic-queue, delivery-limit, x-stream-offset]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Queue types: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on a single RabbitMQ 4.4.1 node with pika 1.5.0.

## Reference

### Features by type

| Feature | classic | quorum | stream | Verified |
|---|---|---|---|---|
| default `x-queue-type` | yes | no | no | yes |
| `durable=False` | refused `541` (unless exclusive) | `541` | `541` | yes |
| `exclusive=True` | yes | `406` | not tested | quorum — yes |
| `x-max-priority` | not tested | `406` | not tested | quorum — yes |
| `x-message-ttl` | yes ([module 03](../03-queues-and-delivery/advanced.en.md#message-ttl)) | allowed | not tested | classic, quorum — yes |
| DLX | yes ([module 06](../06-dead-lettering-and-retries/basics.en.md)) | yes | not tested | classic, quorum — yes |
| `x-delivery-limit` | — | yes (`reject`, a drop) | — | yes |
| non-destructive read, offset | no | no | yes | yes |
| `auto_ack=True` | yes | yes | `406` | stream — yes |
| changing the type by redeclaring | — | — | — | `406` |

### Arguments and their result

| Argument | Type | Verified |
|---|---|---|
| `x-queue-type` | `classic`/`quorum`/`stream` | yes; an unknown value → `406` |
| `x-delivery-limit` | quorum | yes, see the [advanced theory](advanced.en.md#delivery-limit) |
| `x-stream-offset` | on `basic_consume` to a stream | `first`, `last`, `next`, a number — yes; `timestamp` — no |
| `x-max-length-bytes`, `x-stream-max-segment-size-bytes`, `x-max-age` (stream retention) | stream | not tested |

> ⚠️ Verify: stream retention arguments (`x-max-length-bytes`, `x-max-age`, `x-stream-max-segment-size-bytes`), quorum queue length limits, `x-overflow` behaviour and any other `x-…` arguments per type were not verified here.

### Declaration error codes (verified)

| Situation | Code | Closes |
|---|---|---|
| incompatible property (`exclusive` on quorum) | `406` | channel |
| invalid argument (`x-max-priority` on quorum) | `406` | channel |
| unknown type | `406` | channel |
| changing the type | `406` | channel |
| not durable and not exclusive | `541` | connection |

## Internals

- **Classic** — the queue lives on one node; if the node is lost its messages are unavailable (and lost unless persistent).
- **Quorum** — a replicated log based on consensus; a write is confirmed once a majority of replicas accept it ([module 08](../08-clustering/overview.en.md)). With one node in this module, replication was not verified.
- **Stream** — an append-only log; consumers read by offset, an ack does not delete the message (verified), removal follows a retention policy.
- **The delivery counter** — a quorum queue property: `x-delivery-count` in the headers of repeat deliveries; `nack(requeue=True)` did not increase it in the test.

> ⚠️ Verify: details of Raft, replication and the stream chunk format come from the documentation and were not verified here.

## Gotchas

### 1. `nack(requeue=True)` and the delivery limit

The message went around 582 times in 2 s and never reached the DLX (verified). `reject(requeue=True)` — 4 deliveries and the DLX.

### 2. The type cannot be changed

Redeclaring with another `x-queue-type` gives `406`.

### 3. A quorum queue without durable

The connection closes with `541`, not the channel.

### 4. A stream without prefetch

`406 … consumer prefetch count is not set` and `auto_ack` is forbidden.

### 5. `last` is a chunk

In the test `last` returned all six messages.

### 6. Priorities and exclusive

Not supported on quorum (`406`).

### 7. A stream does not delete on ack

Messages stay; you can re-read (verified). Control size with a retention policy.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| classic | fast, simple | one node, no replication |
| quorum | durability on node failure | no priorities or exclusive, more overhead |
| stream | re-reading, many consumers | a different model, needs prefetch and a retention policy |
| Kafka instead of a stream | the log ecosystem | a separate system |

## Interview questions

<details><summary>1. Which queue types does RabbitMQ 4.4 have?</summary>

classic, quorum and stream; the type is set by `x-queue-type`, classic by default.

→ [Basics](basics.en.md#three-queue-types)

</details>

<details><summary>2. How does quorum differ from classic?</summary>

Quorum is replicated via consensus and survives a node failure; classic lives on one node.

→ [Basics](basics.en.md#three-queue-types)

</details>

<details><summary>3. Trick: can you declare a quorum queue with `exclusive=True`?</summary>

No: `406 PRECONDITION_FAILED - invalid property 'exclusive-owner'`.

→ [Basics](basics.en.md#limits-of-quorum-queues-and-streams)

</details>

<details><summary>4. What happens with `durable=False` on a quorum queue?</summary>

The broker closes the connection with `541` (non-durable non-exclusive queues are forbidden on 4.4).

→ [Gotcha 3](#3-a-quorum-queue-without-durable)

</details>

<details><summary>5. How does a stream differ from a queue?</summary>

A non-destructive log: reading does not delete messages, the consumer picks an offset; removal follows a retention policy.

→ [Basics](basics.en.md#stream-a-log-instead-of-a-queue)

</details>

<details><summary>6. Trick: how do you read a stream from the beginning?</summary>

`x-stream-offset: "first"` on `basic_consume`, with `basic_qos(prefetch_count=N)` and a manual ack.

→ [Basics](basics.en.md#stream-a-log-instead-of-a-queue)

</details>

<details><summary>7. Why a delivery limit?</summary>

So a poison message does not go round forever: after the limit it goes to the DLX with reason `delivery_limit`.

→ [Advanced theory](advanced.en.md#delivery-limit)

</details>

<details><summary>8. Trick: does the delivery limit count `nack(requeue=True)`?</summary>

In our test no: 582 deliveries without the DLX. `reject(requeue=True)` and a channel crash are counted.

→ [Gotcha 1](#1-nackrequeuetrue-and-the-delivery-limit)

</details>

<details><summary>9. What does `x-stream-offset: "next"` return?</summary>

Only messages that arrive after the subscription.

→ [Advanced theory](advanced.en.md#stream-offsets)

</details>

<details><summary>10. Can you change a queue's type?</summary>

No (`406`): create a new one and move the messages.

→ [Gotcha 2](#2-the-type-cannot-be-changed)

</details>

<details><summary>11. Trick: `x-stream-offset: "last"` returned all messages. Why?</summary>

It starts at the last chunk, and that chunk held all six messages.

→ [Advanced theory](advanced.en.md#stream-offsets)

</details>

<details><summary>12. When do you choose quorum and when a stream?</summary>

Quorum for tasks that must not be lost; a stream for events read by many consumers from different positions.

→ [Basics](basics.en.md#what-to-choose)

</details>

## Cheat sheet

```text
x-queue-type  : classic (default) | quorum | stream
quorum        : durable only; no exclusive / x-max-priority; x-delivery-limit counts reject, not nack
stream        : durable; basic_qos required; no auto_ack; x-stream-offset first|last|next|N
change type   : impossible (406) -> new queue + migrate
```

- `reject` or a DLX instead of `nack(requeue=True)` as a guard against poison messages.
- A stream keeps messages until the retention policy fires.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [06 · Dead-lettering and retries](../06-dead-lettering-and-retries/expert.en.md)
- Next module: [08 · Clustering and high availability](../08-clustering/overview.en.md)

## Sources

- [Quorum Queues](https://www.rabbitmq.com/docs/quorum-queues)
- [Streams and Super Streams](https://www.rabbitmq.com/docs/streams)
- [Classic Queues](https://www.rabbitmq.com/docs/classic-queues)
