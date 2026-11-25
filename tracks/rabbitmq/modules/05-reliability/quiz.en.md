---
title: "Quiz: reliability"
lang: en
track: rabbitmq
kind: module
doc: quiz
slug: 05-reliability
part: 2
tags: [rabbitmq, publisher-confirms, mandatory, transactions, idempotency]
status: draft
updated: 2026-11-25
---

# Quiz: reliability

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does an `ack` from the broker on a publish mean?

- A) A consumer processed the message
- B) The broker took responsibility for the message
- C) The message is in a queue
- D) The message was delivered to all subscribers

<details><summary>Answer</summary>

**B.** It is not processing (A) nor a guarantee of a route (C): without `mandatory` an unroutable message is also confirmed.
→ [Basic theory](basics.en.md#publisher-confirms)

</details>

## 2. 🟢 What does `mandatory=True` do?

- A) Makes the message persistent
- B) Returns an unroutable message to the publisher (`312 NO_ROUTE`)
- C) Blocks publishing until confirmed
- D) Creates the missing queue

<details><summary>Answer</summary>

**B.** `mandatory` stops an unroutable message from vanishing silently: it is returned to the publisher. It is not persistence or confirms, and creates no queue.
→ [Basic theory](basics.en.md#mandatory-and-basicreturn)

</details>

## 3. 🟢 What does "at least once" delivery give?

- A) The message is neither lost nor duplicated
- B) The message is not lost but may arrive twice
- C) The message may be lost but is never duplicated
- D) Ordering is guaranteed

<details><summary>Answer</summary>

**B.** Retrying after failures prevents loss at the price of possible duplicates, so make handlers idempotent. The guarantee promises neither uniqueness nor order.
→ [Basic theory](basics.en.md#what-at-least-once-guarantees)

</details>

## 4. 🟡 A queue with `x-overflow: reject-publish` is full. What does a publisher with confirms get?

- A) `ack`
- B) `nack`
- C) An exception at publish time
- D) Nothing, silently lost

<details><summary>Answer</summary>

**B.** In the test: `first: ack second: nack`.
→ [Advanced theory](advanced.en.md#a-nack-from-the-broker)

</details>

## 5. 🟡 Publishing to a nonexistent exchange with confirms on. What happens?

- A) `nack`
- B) `ack`
- C) The channel closes and no confirm arrives
- D) The broker creates the exchange

<details><summary>Answer</summary>

**C.** In the test: `missing exchange: timeout channel open: False`.
→ [Advanced theory](advanced.en.md#errors-that-produce-no-confirm)

</details>

## 6. 🟡 Why is one-confirm-per-message slow?

- A) The broker encrypts each confirm
- B) Each publish waits for a network round trip
- C) Confirms work only one at a time
- D) A transaction is opened per message

<details><summary>Answer</summary>

**B.** 300 messages: 13.4 s one by one against 0.05 s pipelined (numbers depend on the environment).
→ [Advanced theory](advanced.en.md#throughput)

</details>

## 7. 🟡 Can you call `tx_select` in `pika.Connection`?

- A) Yes
- B) No, the channel has no `tx_*`; transactions exist only in the deprecated `BlockingConnection`
- C) Yes, but only with confirms
- D) No, AMQP has no transactions

<details><summary>Answer</summary>

**B.** AMQP 0-9-1 has transactions but this client does not expose them.
→ [Advanced theory](advanced.en.md#transactions)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) An `ack` for a publish with no route is possible
- [ ] B) A confirm says a consumer processed the message
- [ ] C) `multiple=True` in a confirm covers all tags up to the given one
- [ ] D) Consumer idempotency is needed even with confirms

<details><summary>Answer</summary>

**A, C, D.** B is wrong: a confirm is a broker property.
→ [Expert](expert.en.md#reference)

</details>

## 9. 🟡 A persistent message in a durable queue; the broker restarts. What does a confirm guarantee?

- A) That the message survives in every case
- B) That the broker accepted the message; survival without persistent and durable is not ensured by it alone
- C) That a consumer will receive it
- D) Nothing

<details><summary>Answer</summary>

**B.** Reliability is a chain: confirm, durable, persistent, ack.
→ [Basics](basics.en.md#where-a-message-can-be-lost)

</details>

## 10. 🔴 Find the mistake

```python
pub.publish("orders", "created", body)   # returns "ack"
print("delivered to the queue!")
```

<details><summary>Answer</summary>

An `ack` does not prove a route: without `mandatory` a message with no binding is also confirmed. Add `mandatory=True` and a return handler.
→ [Gotcha 1](expert.en.md#1-an-ack-for-a-lost-message)

</details>

## 11. 🔴 A publisher got no confirm and sent the message again. What can happen?

- A) Nothing: the messages are identical
- B) The consumer receives a duplicate if the first one did arrive
- C) The broker rejects the repeat
- D) The first message disappears

<details><summary>Answer</summary>

**B.** Hence `message_id` and an idempotent consumer.
→ [Gotcha 5](expert.en.md#5-duplicates-on-retry)

</details>

## 12. 🔴 Open question

A payment service publishes a `payment.completed` event that must not be lost. Describe how to ensure reliability from publisher to consumer.

<details><summary>Key points of a good answer</summary>

- A durable queue, persistent messages; confirms with a timeout and republishing of the unconfirmed.
- `mandatory` and a return handler (or a topology check at startup).
- Manual ack after the work; an idempotent consumer by `message_id` in a database.
- Consider an outbox to align publishing with the DB transaction.
- Monitor queue depth and unacknowledged messages.

</details>

→ [Advanced theory](advanced.en.md)
