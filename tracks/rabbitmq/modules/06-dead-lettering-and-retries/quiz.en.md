---
title: "Quiz: dead-lettering and retries"
lang: en
track: rabbitmq
kind: module
doc: quiz
slug: 06-dead-lettering-and-retries
part: 2
tags: [rabbitmq, dead-letter-exchange, x-death, retry, poison-message]
status: draft
updated: 2026-11-25
---

# Quiz: dead-lettering and retries

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is a dead letter exchange?

- A) An exchange that stores all messages
- B) An exchange to which the broker sends a queue's messages that became "dead"
- C) An exchange for deleting queues
- D) A queue type

<details><summary>Answer</summary>

**B.** Rejected, expired and overflowed messages go to the DLX. It is an ordinary exchange: it does not store everything, delete queues or form a queue type.
→ [Basic theory](basics.en.md#why-dead-lettering)

</details>

## 2. 🟢 Which of these makes a message "dead"?

- A) `basic_ack`
- B) `reject` with `requeue=False` in a queue with a DLX
- C) Publishing to a fanout
- D) `basic_qos`

<details><summary>Answer</summary>

**B.** Also an expired TTL and eviction by `x-max-length`.
→ [Basic theory](basics.en.md#why-dead-lettering)

</details>

## 3. 🟢 What happens to a rejected (`requeue=False`) message in a queue without a DLX?

- A) It returns to the queue
- B) It goes to a queue `amq.dead`
- C) It is dropped
- D) The channel closes

<details><summary>Answer</summary>

**C.** In the test the depth became 0.
→ [Basics](basics.en.md#without-a-dlx-the-message-vanishes)

</details>

## 4. 🟢 What is the header called where the broker keeps the history of "deaths"?

- A) `x-history`
- B) `x-death`
- C) `x-dlx`
- D) `x-retry`

<details><summary>Answer</summary>

**B.** The broker maintains the `x-death` header with queue, reason and count. It fills none of the other names.
→ [Basics](basics.en.md#a-rejected-message)

</details>

## 5. 🟡 How do you build a delayed retry?

- A) `basic_nack(requeue=True)` with a pause in the handler
- B) A `retry` queue with no consumers, with a TTL and a DLX back to the work queue
- C) Enable `mandatory`
- D) Raise `prefetch_count`

<details><summary>Answer</summary>

**B.** A blocks the worker and is lost on a crash.
→ [Advanced theory](advanced.en.md#a-delayed-retry)

</details>

## 6. 🟡 A queue's DLX does not exist. What happens to a rejected message?

- A) The channel closes with a 404 error
- B) The message returns to the original queue
- C) The message vanishes with no error
- D) The broker creates the exchange

<details><summary>Answer</summary>

**C.** In the test: depth 0, channel open.
→ [Advanced theory](advanced.en.md#a-dlx-with-no-route)

</details>

## 7. 🟡 `x-max-length: 1`, `drop-head`, `first` and `second` published, the queue has a DLX. What lands in the inspection queue?

- A) `second`
- B) `first`
- C) Both
- D) Nothing

<details><summary>Answer</summary>

**B.** The oldest is evicted.
→ [Basics](basics.en.md#other-reasons)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) An `x-death` record is grouped by the pair (queue, reason) and accumulates `count`
- [ ] B) The routing key after dead-lettering can be replaced by `x-dead-letter-routing-key`
- [ ] C) A DLX retries processing automatically
- [ ] D) A `retry` delay queue needs no consumers

<details><summary>Answer</summary>

**A, B, D.** C is wrong: you build the retry yourself.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🟡 What is more reliable in production code when counting attempts from `x-death`?

- A) Sum `count` over all records
- B) Take the `count` of the record with the right queue and reason `rejected`
- C) Count the length of the records list
- D) Take the `count` of the first record

<details><summary>Answer</summary>

**B.** A sum and a length mix reasons and queues.
→ [Advanced theory](advanced.en.md#what-accumulates-in-x-death)

</details>

## 10. 🔴 Find the mistake

```python
def on_msg(ch, m, p, body):
    try:
        handle(body)
        ch.basic_ack(m.delivery_tag)
    except Exception:
        ch.basic_nack(m.delivery_tag, requeue=True)
```

<details><summary>Answer</summary>

For an unfixable error the message loops forever. Distinguish transient from unfixable errors: unfixable ones go to the DLX with `requeue=False`; transient ones through a delay queue with an attempt cap.
→ [Advanced theory](advanced.en.md#best-practices)

</details>

## 11. 🔴 A consumer of a quorum queue with `x-delivery-limit: 3` loops on `basic_nack(requeue=True)`. What happens?

- A) After four deliveries the message goes to the DLX
- B) The limit does not trigger: `nack(requeue=True)` does not increase the delivery count
- C) The channel closes
- D) The message is dropped at once

<details><summary>Answer</summary>

**B.** In the test: 140 deliveries without the DLX; with `basic_reject(requeue=True)` it would be 4 deliveries and the DLX (we did not establish the cause, this is an observation).
→ [Gotcha 6](expert.en.md#6-delivery-limit)

</details>

## 12. 🔴 Open question

Design handling of payment messages: network faults are transient, an invalid format is unfixable. What do you do with each kind and how do you avoid losing the message in the end?

<details><summary>Key points of a good answer</summary>

- An invalid format — straight to `requeue=False` into the DLX and an inspection queue with the reason.
- A transient fault — a `retry` queue with a TTL (or several for backoff), an attempt cap from `x-death`, then `parking`.
- An idempotent handler (`message_id`), manual ack after success.
- Monitor the depth of `retry` and `parking`, alert.
- A topology created in advance: a DLX with no queue loses messages silently.

</details>

→ [Advanced theory](advanced.en.md)
