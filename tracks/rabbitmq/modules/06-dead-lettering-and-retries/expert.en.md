---
title: "Dead-lettering and retries: expert"
lang: en
track: rabbitmq
kind: module
doc: expert
slug: 06-dead-lettering-and-retries
part: 2
tags: [rabbitmq, dead-letter-exchange, x-death, retry, poison-message, message-ttl, parking-lot]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Dead-lettering and retries: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on RabbitMQ 4.4.1 with pika 1.5.0.

## Reference

### Queue arguments for dead-lettering

| Argument | Effect | Verified |
|---|---|---|
| `x-dead-letter-exchange` | where to send "dead" messages | yes |
| `x-dead-letter-routing-key` | routing key in the DLX; otherwise the original is kept | yes |
| `x-dead-letter-strategy` (`at-most-once`/`at-least-once`) | dead-lettering guarantee | not tested |
| `x-delivery-limit` (quorum) | delivery cap | works with `reject(requeue=True)` and a channel crash; `nack(requeue=True)` is not counted, see the [advanced theory](advanced.en.md#delivery-limit-of-quorum-queues) |

### Reasons (`reason` in `x-death`)

| Reason | When | Verified |
|---|---|---|
| `rejected` | `reject`/`nack` with `requeue=False` | yes |
| `expired` | message/queue TTL ran out | yes |
| `maxlen` | evicted by `x-max-length` with `drop-head` | yes |
| `delivery_limit` | delivery limit exceeded (quorum) | yes |

### Headers the broker adds

| Header | Content | Verified |
|---|---|---|
| `x-death` | list of records: `queue`, `reason`, `count`, `exchange`, `routing-keys` (and a time) | yes |
| `x-first-death-queue/-reason/-exchange` | the first death | yes |
| `x-last-death-queue/-reason/-exchange` | the last death | yes |

An `x-death` record is grouped by (queue, reason) and accumulates `count`; records are ordered latest first (in the test the `retry/expired` record came before `main/rejected`).

### What happens to the message

| Property | Behaviour | Verified |
|---|---|---|
| routing key | replaced by `x-dead-letter-routing-key` | yes |
| body and properties | preserved | yes |
| `delivery_mode`, `message_id`, custom headers | preserved per the documentation | not tested |
| the DLX does not exist | the message vanishes, no error | yes |
| with `maxlen`+`drop-head` | the oldest becomes dead | yes |

## Internals

- **Dead-lettering is a publish by the broker.** The message is republished to the DLX with the key `x-dead-letter-routing-key`; from there it is an ordinary publish: it can be chained or sent to a fanout.
- **`x-death` is part of the message.** The broker updates the header on each cycle; the consumer uses it as an attempt counter.
- **A delay queue with no consumers.** A message "ages" inside the queue and "dies" by TTL: expiry is just another dead-lettering reason.
- **Message TTL only at the head.** In a queue with mixed TTLs an expired message behind a live one waits ([module 03](../03-queues-and-delivery/expert.en.md#5-ttl-behind-a-long-head)); for delay queues use one TTL per queue.

> ⚠️ Verify: the guarantees of `x-dead-letter-strategy` and how dead-lettering behaves when a node fails were not verified here.

## Gotchas

### 1. Reject without a DLX

The message is gone forever (verified: depth 0).

### 2. A DLX with no route

The message vanishes with no error and no trace (verified).

### 3. Summing `count`

A sum of `count` over all records mixes reasons and queues; filter by queue and reason.

### 4. An endless main → retry → main cycle

Without an attempt cap the message goes around forever; count attempts and send to `parking`.

### 5. The old one is evicted

With `maxlen` and `drop-head` the oldest message goes to the DLX, not the new one (verified: `first`).

### 6. Delivery limit

`basic_nack(requeue=True)` does not increase a quorum queue's delivery count: with `x-delivery-limit: 3` the message went through 140 deliveries and never reached the DLX. `basic_reject(requeue=True)` is counted: 4 deliveries and a DLX with reason `delivery_limit`.

### 7. The delay depends on the queue TTL

One TTL per queue; different delays need different queues.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| DLX + delay queue | standard tools | a queue per delay |
| Own counter in a header | independent of `x-death` | the message must be republished |
| The delayed-message-exchange plugin | any per-message delay | a separate plugin; not tested here |
| Delivery limit (quorum) | the broker counts attempts | did not trigger in the test; needs checking |
| A retry in consumer code | simple | blocks the worker, lost on a crash |

## Interview questions

<details><summary>1. What is a DLX?</summary>

An exchange to which the broker sends a queue's messages that became "dead": rejected without requeue, expired, evicted by length.

→ [Basic theory](basics.en.md#why-dead-lettering)

</details>

<details><summary>2. Trick: what happens on `reject(requeue=False)` in a queue without a DLX?</summary>

The message is dropped forever.

→ [Basics](basics.en.md#without-a-dlx-the-message-vanishes)

</details>

<details><summary>3. What is in `x-death`?</summary>

A list of records (queue, reason, `count`, exchange, routing keys) that accumulate the history of "deaths".

→ [Basics](basics.en.md#a-rejected-message)

</details>

<details><summary>4. How do you build a delayed retry?</summary>

A work queue → DLX → a `retry` queue with no consumers, with a TTL and a DLX back to the work queue.

→ [Advanced theory](advanced.en.md#a-delayed-retry)

</details>

<details><summary>5. How do you count attempts?</summary>

From `x-death`: the `count` of the record with queue `main` and reason `rejected`, or with your own header.

→ [Advanced theory](advanced.en.md#what-accumulates-in-x-death)

</details>

<details><summary>6. Trick: the DLX does not exist. What happens to the message?</summary>

It vanishes; no error, the channel stays open.

→ [Advanced theory](advanced.en.md#a-dlx-with-no-route)

</details>

<details><summary>7. What becomes "dead" with `x-max-length` and `drop-head`?</summary>

The oldest message (reason `maxlen`).

→ [Basics](basics.en.md#other-reasons)

</details>

<details><summary>8. What is a poison message?</summary>

A message whose processing always fails; take it out of the loop instead of requeuing it.

→ [Advanced theory](advanced.en.md#best-practices)

</details>

<details><summary>9. Trick: can you trust a quorum queue's delivery limit?</summary>

For `reject(requeue=True)` and a channel crash, yes (4 deliveries, then the DLX); for `nack(requeue=True)`, no: the count does not grow (140 deliveries without the DLX).

→ [Gotcha 6](#6-delivery-limit)

</details>

<details><summary>10. How do you build a growing delay?</summary>

Several `retry` queues with different TTLs and a routing key chosen by the attempt counter.

→ [Advanced theory](advanced.en.md#a-variable-delay)

</details>

<details><summary>11. Why can't you use per-message `expiration` for a delay queue with mixed values?</summary>

A message TTL fires only at the head of the queue; a short TTL behind a long one waits.

→ [Module 03](../03-queues-and-delivery/expert.en.md#5-ttl-behind-a-long-head)

</details>

<details><summary>12. Trick: after dead-lettering, is the routing key the original or `x-dead-letter-routing-key`?</summary>

If `x-dead-letter-routing-key` is set, that one (verified: `dead`); otherwise the original stays.

→ [Basics](basics.en.md#a-rejected-message)

</details>

## Cheat sheet

```text
queue args : x-dead-letter-exchange, x-dead-letter-routing-key
reasons    : rejected | expired | maxlen | delivery_limit (quorum)
x-death    : [{queue, reason, count, exchange, routing-keys}] + x-first-death-* / x-last-death-*
retry      : main -DLX-> retry(TTL, no consumers) -DLX-> main ; cap attempts -> parking queue
```

- A DLX and an inspection queue for every work queue.
- Retry with a delay and a cap.
- The delivery limit counts `reject` and a channel crash, but not `nack`.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [05 · Reliability](../05-reliability/expert.en.md)
- Next: [07 · Queue types](../07-queue-types/overview.en.md)

## Sources

- [Dead Letter Exchanges](https://www.rabbitmq.com/docs/dlx)
- [Quorum Queues](https://www.rabbitmq.com/docs/quorum-queues)
- [Time-To-Live and Expiration](https://www.rabbitmq.com/docs/ttl)
