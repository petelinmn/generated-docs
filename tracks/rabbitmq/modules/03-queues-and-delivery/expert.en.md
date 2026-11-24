---
title: "Queues and delivery: expert"
lang: en
track: rabbitmq
kind: module
doc: expert
slug: 03-queues-and-delivery
part: 1
tags: [rabbitmq, queue, policy, acknowledgement, prefetch, consumer-timeout, redelivery, durability]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Queues and delivery: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Tested" means observed on RabbitMQ 4.4.1.

## Reference

### Queue declare options

| Option | Meaning | Tested |
|---|---|---|
| `durable` | the queue survives a restart | yes |
| `exclusive` | one connection only, deleted with it; 405 for others | yes |
| `auto_delete` | deleted after the last consumer cancels | yes |
| `passive` | check existence only; 404 closes the channel | yes |
| `arguments` | the `x-…` arguments below | partly |

### Queue arguments

| Argument | Effect | Tested |
|---|---|---|
| `x-message-ttl` | queue-wide message TTL (ms) | yes |
| `x-max-length` | maximum number of messages | yes |
| `x-overflow` | `drop-head` (default) or `reject-publish` | yes |
| `x-expires` | delete the queue after this idle time (ms) | yes |
| `x-dead-letter-exchange`, `x-dead-letter-routing-key` | dead-lettering | module 06 in the [roadmap](../../overview.en.md) |
| `x-max-priority`, `x-queue-type`, `x-max-length-bytes` | priorities, queue type, byte limit | not tested here |

### Message properties and delivery

| Item | Behaviour |
|---|---|
| `delivery_mode=2` | persistent; survived a restart in a durable queue (tested) |
| `expiration="200"` | per-message TTL; the smaller of queue and message TTL applies (tested) |
| `redelivered` | `True` after a return to the queue (tested) |
| `basic_nack(multiple=True)` | refuses all deliveries up to the tag |
| `basic_qos(prefetch_count=N)` | max unacked messages per consumer on the channel (tested) |

### Policies

Many arguments can be set by a **policy** instead of in code: a pattern, a definition and a priority, applied to queues or exchanges of a virtual host. In a test, a policy `^pol\.` with `{"max-length":2}` made a queue `pol.q` keep 2 of 4 published messages:

```text
Setting policy "cap-pol" for pattern "^pol\." to "{"max-length":2}" with priority "0" for vhost "/" ...
with policy max-length 2: 2
```

Policies can be changed without redeclaring queues, which is why they are preferred for limits, TTLs and dead-lettering. Policy keys drop the `x-` prefix (`max-length`, not `x-max-length`).

### Observed server defaults

`consumer_timeout` is 1 800 000 ms (30 minutes): a delivery not acknowledged within it closes the channel with `PRECONDITION_FAILED` (the documented behaviour; the default value was read from the broker, the closing was not tested here).

## Internals

- **Ready vs unacked.** A queue tracks messages ready for delivery and messages delivered but unacknowledged; only the former are counted by `message_count` of a declare. Unacked messages return to ready when the channel closes.
- **Prefetch is per consumer on a channel** (for `basic_qos` without the global flag in AMQP 0-9-1 RabbitMQ semantics); the broker stops delivering to a consumer whose unacked count reaches the limit.
- **Per-message TTL is enforced at the head.** The queue checks the head message, so an expired message behind a live one stays counted until it reaches the head (tested: 2 messages after 0.8 s).
- **Persistence is two decisions:** the queue's durability and the message's `delivery_mode`; both are needed.

> ⚠️ Verify: the exact semantics of the `global` flag of `basic_qos`, the closing behaviour at `consumer_timeout`, and details of persistence in each queue type were not tested here.

## Gotchas

### 1. Durable queue, transient message

After a restart the queue exists and the message is gone (tested: 2 before, 1 after).

### 2. Unlimited prefetch

The first consumer may take everything (tested: A got all six).

### 3. Rejecting without a DLX

`basic_reject(requeue=False)` drops the message for good.

### 4. `reject-publish` is silent without confirms

The publisher is not told that a message was refused.

### 5. TTL behind a long head

A short per-message TTL does not fire while a long-lived message is ahead.

### 6. Changing arguments

A declare with different arguments is `406 PRECONDITION_FAILED`; use a policy instead.

### 7. Requeue loops

`requeue=True` on a message that always fails makes it loop forever.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| Manual ack | no loss on crash | duplicates possible, must be idempotent |
| `auto_ack` | fastest | loss on crash |
| Small prefetch | fair, bounded memory | lower throughput on fast tasks |
| Large prefetch | high throughput | unfair, large redelivery on crash |
| Policy-based limits | changeable at runtime | one more place to look |
| Argument-based limits | visible in code | immutable without recreating |

## Interview questions

<details><summary>1. What is the difference between ack, nack and reject?</summary>

Ack confirms; nack and reject refuse (nack may refuse several deliveries with `multiple`); with `requeue=True` the message returns to the queue, otherwise it is dropped or dead-lettered.

→ [Basic theory](basics.en.md#ack-nack-reject)

</details>

<details><summary>2. What happens to unacked messages when a consumer dies?</summary>

They are requeued and marked `redelivered`.

→ [Basic theory](basics.en.md#a-dead-consumer-returns-its-messages)

</details>

<details><summary>3. Trick: a durable queue and a message with no delivery mode. Restart. Is the message there?</summary>

No: the message must be persistent too.

→ [Gotcha 1](#1-durable-queue-transient-message)

</details>

<details><summary>4. What does prefetch do?</summary>

It limits unacknowledged messages per consumer, spreading work fairly.

→ [Basic theory](basics.en.md#prefetch-and-fairness)

</details>

<details><summary>5. Trick: what happens without any prefetch limit and a slow consumer subscribed first?</summary>

It may be handed all the messages and starve the others.

→ [Gotcha 2](#2-unlimited-prefetch)

</details>

<details><summary>6. How do queue TTL and message TTL combine?</summary>

The smaller applies; per-message TTL is enforced at the head of the queue.

→ [Advanced theory](advanced.en.md#message-ttl)

</details>

<details><summary>7. What does `x-overflow` do?</summary>

Chooses between `drop-head` (drop the oldest) and `reject-publish` (refuse new messages) when `x-max-length` is reached.

→ [Advanced theory](advanced.en.md#length-limits-and-overflow)

</details>

<details><summary>8. Trick: can you change `x-message-ttl` of an existing queue by redeclaring it?</summary>

No: `406 PRECONDITION_FAILED`. Use a policy or recreate the queue.

→ [Gotcha 6](#6-changing-arguments)

</details>

<details><summary>9. What is the difference between exclusive and auto-delete?</summary>

Exclusive: one connection, deleted with it. Auto-delete: deleted when the last consumer goes away.

→ [Advanced theory](advanced.en.md#exclusive-and-auto-delete-queues)

</details>

<details><summary>10. What is a policy?</summary>

A runtime-managed set of arguments applied to queues or exchanges whose names match a pattern.

→ [Reference](#policies)

</details>

<details><summary>11. Trick: why should a handler be idempotent?</summary>

At-least-once delivery means a message can be processed twice (for example after a crash before the ack).

→ [Basic theory](basics.en.md#a-dead-consumer-returns-its-messages)

</details>

<details><summary>12. Why is requeueing a failing message dangerous?</summary>

It loops forever and blocks the worker; send it to a dead-letter queue instead.

→ [Gotcha 7](#7-requeue-loops)

</details>

## Cheat sheet

```text
ack/nack/reject(tag, requeue)   basic_qos(prefetch_count=N)
queue args: x-message-ttl, x-max-length, x-overflow(drop-head|reject-publish), x-expires
message: delivery_mode=2 (persistent), expiration="ms"
durable queue + persistent message = survives restart
redeclare with other args -> 406; use policies: rabbitmqctl set_policy NAME PATTERN '{"max-length":2}' --apply-to queues
```

- Ack last.
- Small prefetch for slow work.
- Persistent needs both.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Earlier: [02 · Exchanges and routing](../02-exchanges-and-routing/expert.en.md)
- Next module: 04 · Messaging patterns ([roadmap](../../overview.en.md))

## Sources

- [Consumer Acknowledgements and Publisher Confirms](https://www.rabbitmq.com/docs/confirms)
- [Consumer Prefetch](https://www.rabbitmq.com/docs/consumer-prefetch)
- [Policies](https://www.rabbitmq.com/docs/parameters)
- [Time-To-Live and Expiration](https://www.rabbitmq.com/docs/ttl)
- [Queue Length Limits](https://www.rabbitmq.com/docs/maxlength)
