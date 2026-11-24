---
title: "Messaging patterns: expert"
lang: en
track: rabbitmq
kind: module
doc: expert
slug: 04-messaging-patterns
part: 1
tags: [rabbitmq, work-queue, publish-subscribe, rpc, direct-reply-to, correlation-id]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Messaging patterns: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on RabbitMQ 4.4.1 with pika 1.5.0.

## Reference

### Pattern → AMQP constructs

| Pattern | Exchange | Queue | Key message properties | Verified |
|---|---|---|---|---|
| Work queue | default (`""`), key = queue name | durable, shared | `delivery_mode=2` | yes |
| Pub/sub | `fanout` | `exclusive`, server-named (`amq.gen-…`) | — | yes |
| Routing | `direct` | one binding per key | routing key | yes |
| Topics | `topic` | by binding patterns | dotted routing key | [module 02](../02-exchanges-and-routing/expert.en.md) |
| RPC | default | request queue + reply | `reply_to`, `correlation_id`, `expiration` | yes |

### Message properties relevant to patterns

| Property | Purpose | Set by |
|---|---|---|
| `reply_to` | reply address | RPC client |
| `correlation_id` | tie reply to request | client sets, server copies |
| `message_id` | identifier for deduplication | publisher |
| `expiration` | request TTL in ms (a string) | RPC client |
| `delivery_mode` | 2 = persistent | publisher |
| `headers` | arbitrary fields | publisher |

The broker does **not interpret** `reply_to` and `correlation_id` (except the pseudo-queue `amq.rabbitmq.reply-to`): they are a convention between client and server.

### Direct reply-to

| Requirement | Result of breaking it (verified) |
|---|---|
| consume with `auto_ack=True` | `406 PRECONDITION_FAILED - reply consumer cannot acknowledge` |
| consume before publishing with that `reply_to` | the channel closed |

## Internals

- **Work-queue distribution follows readiness.** The broker picks the next consumer that has room (prefetch). That is why the fast worker got 9 of 10 tasks (verified); there is no equal split.
- **A fanout copies at the broker level**, but to you these are independent messages in independent queues: an ack in one queue does not affect another.
- **Pub/sub with no queue is nothing.** An exchange stores no messages; publishing with no bindings is silently lost (verified, when not mandatory).
- **RPC has no state on the broker.** The broker sees two independent messages; the link is on the client side (a dictionary of pending requests).

> ⚠️ Verify: internal details of direct reply-to (pseudo address, channel binding) come from the documentation and were not verified here.

## Gotchas

### 1. Fanout and a shared queue

If two subscribers read **one** named queue, messages are divided, not copied: that is a work queue, not pub/sub.

### 2. A hanging RPC

Without a timeout and `expiration` a request lives until a server appears. Verified: after the client timed out, a request with `expiration` vanished from the queue (`stale request still queued: 0`).

### 3. A late reply

A reply may arrive after the timeout. A client that has removed the `correlation_id` from its pending set should simply ignore it (verified: `pending after late reply: 0`).

### 4. A foreign `correlation_id`

A client that puts **all** replies into a dictionary without checking will accept "foreign" ones too (verified: a reply with id `other` landed in the dictionary). Accept only expected ids.

### 5. Reply-to without a consumer

Publishing with `reply_to="amq.rabbitmq.reply-to"` without consuming closes the channel (verified).

### 6. A slow worker without prefetch

Round-robin without prefetch ignores task duration: a slow worker piles up a backlog. See [module 03](../03-queues-and-delivery/expert.en.md).

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| Work queue | simple, scales horizontally | no ordering across workers |
| Pub/sub | decouples publisher and subscribers | temporary queues lose messages when disconnected |
| RPC over a broker | ready transport, load balancing | couples in time, needs timeouts and deduplication |
| HTTP/gRPC | synchronous contract, easier debugging | no buffer when the server is down |
| Command + result event | resilient and asynchronous | harder client |

## Interview questions

<details><summary>1. How does a work queue differ from pub/sub?</summary>

In a work queue each message goes to one worker from a shared queue; in pub/sub each subscriber has its own queue and receives a copy.

→ [Basic theory](basics.en.md#work-queue)

</details>

<details><summary>2. How does a temporary queue get its name and when does it vanish?</summary>

`queue_declare("", exclusive=True)`: the broker gives the name (`amq.gen-…`); the queue is deleted with the connection.

→ [Basic theory](basics.en.md#publishsubscribe)

</details>

<details><summary>3. Trick: two consumers read one named queue bound to a fanout. Do both get every message?</summary>

No: there is one queue, so messages are divided between the consumers.

→ [Gotcha 1](#1-fanout-and-a-shared-queue)

</details>

<details><summary>4. Why `correlation_id` in RPC?</summary>

One reply queue serves many requests; the id lets the client match a reply to its request.

→ [Basic theory](basics.en.md#rpc)

</details>

<details><summary>5. What does direct reply-to give you?</summary>

No per-client reply queue: the reply is delivered to the channel via the pseudo-queue `amq.rabbitmq.reply-to`.

→ [Advanced theory](advanced.en.md#direct-reply-to)

</details>

<details><summary>6. Trick: can you consume `amq.rabbitmq.reply-to` with manual acknowledgement?</summary>

No: `406 PRECONDITION_FAILED - reply consumer cannot acknowledge`.

→ [Reference](#direct-reply-to)

</details>

<details><summary>7. Why set `expiration` on an RPC request?</summary>

So a request from a departed client does not wait for a server and run for nothing.

→ [Advanced theory](advanced.en.md#an-rpc-client-with-a-timeout)

</details>

<details><summary>8. What does the client do with a reply that arrives after the timeout?</summary>

Ignores it: the id has already been removed from the pending set.

→ [Gotcha 3](#3-a-late-reply)

</details>

<details><summary>9. Trick: what happens if you publish to a fanout before any subscriber exists?</summary>

The message is lost: an exchange stores nothing.

→ [Basic theory](basics.en.md#publishsubscribe)

</details>

<details><summary>10. When is RPC over a broker a bad idea?</summary>

When the answer is not needed immediately: RPC couples the client's availability to the server; prefer a command plus a result event.

→ [Advanced theory](advanced.en.md#choosing-a-pattern)

</details>

<details><summary>11. How did the fast worker get 9 tasks of 10?</summary>

The broker hands a message to whoever has room (prefetch); the fast one frees up more often.

→ [Basic theory](basics.en.md#work-queue)

</details>

<details><summary>12. Trick: does the broker itself tie a reply to its request via `correlation_id`?</summary>

No: it is a convention between client and server; the broker does not interpret the property.

→ [Reference](#message-properties-relevant-to-patterns)

</details>

## Cheat sheet

```text
work queue : durable queue, basic_qos(1), ack after work, delivery_mode=2
pub/sub    : fanout exchange + queue_declare("", exclusive=True) per subscriber
routing    : direct exchange, one binding per key
rpc        : reply_to + correlation_id (+ expiration); amq.rabbitmq.reply-to needs auto_ack=True
```

- One queue — divide; a queue per subscriber — copy.
- Always a timeout in RPC.
- Consume reply-to before publishing.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [03 · Queues and delivery](../03-queues-and-delivery/expert.en.md)
- Next module: [05 · Reliability](../05-reliability/overview.en.md)

## Sources

- [RabbitMQ tutorials](https://www.rabbitmq.com/tutorials)
- [Direct Reply-to](https://www.rabbitmq.com/docs/direct-reply-to)
- [Exchanges](https://www.rabbitmq.com/docs/exchanges)
