---
title: "Reliability: expert"
lang: en
track: rabbitmq
kind: module
doc: expert
slug: 05-reliability
part: 2
tags: [rabbitmq, publisher-confirms, mandatory, transactions, at-least-once, idempotency, basic-return]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Reliability: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on RabbitMQ 4.4.1 with pika 1.5.0.

## Reference

### What the broker's reply to a publish means

| Situation | Basic.Return | Confirm | Verified |
|---|---|---|---|
| route exists, message accepted | — | `ack` | yes |
| no route, no `mandatory` | — | `ack` | yes |
| no route, with `mandatory` | `312 NO_ROUTE` | `ack` (after the return, per the docs) | yes |
| `reject-publish` queue is full | — | `nack` | yes |
| exchange does not exist | channel closed | no confirm | yes |

### pika 1.5.0 API (`pika.Connection`)

| Element | Behaviour | Verified |
|---|---|---|
| `confirm_delivery(callback)` | enables confirms; the callback receives `Basic.Ack` or `Basic.Nack` | yes |
| `basic_publish(..., on_publish=f)` | `f(tag)` runs on the IOLoop thread after the frame is written; gives the confirm tag | yes |
| `add_on_return_callback(f)` | `f(ch, method, props, body)` for returned messages | yes |
| `tx_select`/`tx_commit`/`tx_rollback` | the `pika.Connection` channel does not have them | yes |
| `multiple=True` in a confirm | confirms every tag up to and including the given one | yes |

### Transactions (`BlockingConnection` only)

| Action | Result |
|---|---|
| publish before `tx_commit` | messages invisible (depth 0) |
| `tx_rollback` | messages discarded |
| `tx_commit` | messages visible |

### Guarantees per stretch

| Stretch | Mechanism | If not enabled |
|---|---|---|
| publisher → broker | confirms | a message can vanish without a trace |
| route | `mandatory` | the message is silently dropped |
| storage | durable + persistent | disappears on restart ([module 03](../03-queues-and-delivery/expert.en.md#gotchas)) |
| consumer | manual ack | loss on a crash |

## Internals

- **Confirms number publishes per channel.** The tag grows with each publish on a channel with confirms on; `multiple=True` collapses a range. Hence the need to keep a set of unconfirmed tags.
- **A confirm is not routing.** An `ack` arrives even for a message that went nowhere (verified).
- **Return precedes ack.** With `mandatory` and no route, `Basic.Return` arrives first, then `Basic.Ack` (as the pika documentation says; running verified only that both arrive).
- **A confirm is a broker property, not a consumer one.** It says nothing about processing.

> ⚠️ Verify: the moment of the confirm relative to the disk write and to the quorum in quorum queues comes from the documentation and was not verified here.

## Gotchas

### 1. An `ack` for a lost message

A message with no route and no `mandatory` gets an `ack` yet lies nowhere (verified).

### 2. No confirm at all

A channel-level error (nonexistent exchange) closes the channel, and pending publishes are never confirmed. A wait without a timeout hangs.

### 3. The IOLoop thread

`on_publish` runs on the IOLoop thread and must return quickly; the confirm callback runs on the channel's worker thread. Long work in either blocks the connection.

### 4. Transactions are costly and unavailable

100 commits take 4.5 s against 0.05 s for 300 messages with pipelined confirms (verified). `pika.Connection` has none.

### 5. Duplicates on retry

Republishing without a confirm can create a duplicate: the first message may have arrived while the confirm was lost. You need `message_id` and an idempotent consumer.

### 6. In-memory idempotency

A `seen` set in memory is lost when the consumer restarts: store it in a database.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| One-by-one confirms | simple | very slow (13 s per 300) |
| Pipelined confirms | fast | tag bookkeeping, retries |
| No confirms | maximum speed | silent loss |
| Transactions | group atomicity | costly, absent in the new pika |
| A database outbox | consistency with the DB | an extra process; see the [roadmap](../../overview.en.md) |

## Interview questions

<details><summary>1. What do publisher confirms guarantee?</summary>

That the broker accepted the message (for a persistent one in a durable queue, wrote it); it does not mean it was processed.

→ [Basic theory](basics.en.md#publisher-confirms)

</details>

<details><summary>2. Trick: an `ack` arrived. Did the message reach a queue?</summary>

Not necessarily: without `mandatory` an unroutable message is also confirmed.

→ [Gotcha 1](#1-an-ack-for-a-lost-message)

</details>

<details><summary>3. What does `mandatory` do?</summary>

Returns an unroutable message to the publisher (`Basic.Return`, 312 NO_ROUTE); the confirm still comes afterwards.

→ [Basic theory](basics.en.md#mandatory-and-basicreturn)

</details>

<details><summary>4. When does a `nack` arrive?</summary>

When the broker cannot accept the message, for example `reject-publish` in a full queue.

→ [Advanced theory](advanced.en.md#a-nack-from-the-broker)

</details>

<details><summary>5. Why is "one → wait" slow?</summary>

Each message pays a network round trip; a pipeline and `multiple=True` amortise it (13.4 s against 0.05 s per 300).

→ [Advanced theory](advanced.en.md#throughput)

</details>

<details><summary>6. Trick: how do you know whether a message arrived if the channel closed?</summary>

You cannot know exactly: unconfirmed publishes are "unknown". Republish them and let the consumer deduplicate.

→ [Advanced theory](advanced.en.md#errors-that-produce-no-confirm)

</details>

<details><summary>7. Why are transactions worse than confirms?</summary>

Orders of magnitude costlier (100 commits take 4.5 s), synchronous; unavailable in `pika.Connection`.

→ [Advanced theory](advanced.en.md#transactions)

</details>

<details><summary>8. What does "at least once" delivery mean?</summary>

The message is not lost but may arrive twice; idempotency is needed.

→ [Basic theory](basics.en.md#what-at-least-once-guarantees)

</details>

<details><summary>9. How do you make a consumer idempotent?</summary>

Store handled `message_id`s (in a database, together with the result) or make the operation idempotent by nature.

→ [Advanced theory](advanced.en.md#idempotent-consumer)

</details>

<details><summary>10. Trick: are persistent messages enough to survive a restart?</summary>

No: you also need a durable queue, and confirms attest delivery to disk.

→ [Basics](basics.en.md#where-a-message-can-be-lost)

</details>

<details><summary>11. Why `on_publish`?</summary>

It supplies the publish tag used to match the `ack`/`nack` from the confirm callback.

→ [Reference](#pika-150-api-pikaconnection)

</details>

<details><summary>12. Trick: can you use `tx_select` in `pika.Connection`?</summary>

No, the channel has no `tx_*`; transactions exist only in `BlockingConnection` (deprecated).

→ [Reference](#transactions-blockingconnection-only)

</details>

## Cheat sheet

```text
publisher  : confirm_delivery(cb) + on_publish(tag) -> track tags; ack/nack/timeout
routing    : mandatory=True + add_on_return_callback  (ack comes AFTER the 312 return)
storage    : durable queue + delivery_mode=2
consumer   : manual ack after the work; idempotent by message_id (stored in the DB)
```

- Confirms plus consumer acks mean "at least once".
- `ack` does not mean "there is a route".
- Never wait without a timeout.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [04 · Messaging patterns](../04-messaging-patterns/expert.en.md)
- Next module: [06 · Dead-lettering and retries](../06-dead-lettering-and-retries/overview.en.md)

## Sources

- [Consumer Acknowledgements and Publisher Confirms](https://www.rabbitmq.com/docs/confirms)
- [Reliability Guide](https://www.rabbitmq.com/docs/reliability)
- [Publishers](https://www.rabbitmq.com/docs/publishers)
