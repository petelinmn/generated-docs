---
title: "Quiz: queues and delivery"
lang: en
track: rabbitmq
kind: module
doc: quiz
slug: 03-queues-and-delivery
part: 1
tags: [rabbitmq, queue, acknowledgement, prefetch, durability, message-ttl]
status: draft
updated: 2026-11-24
---

# Quiz: queues and delivery

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 When should a consumer ack a message?

- A) As soon as it is received
- B) After the work is done
- C) Never, the broker does it
- D) Before it is parsed

<details><summary>Answer</summary>

**B.** Acking early loses the message if the worker crashes.
→ [Basic theory](basics.en.md#ack-nack-reject)

</details>

## 2. 🟢 What does `basic_nack(tag, requeue=True)` do?

- A) Drops the message
- B) Returns it to the queue, marked redelivered
- C) Acks it
- D) Closes the channel

<details><summary>Answer</summary>

**B.**
→ [Basic theory](basics.en.md#ack-nack-reject)

</details>

## 3. 🟢 A durable queue holds a message published without `delivery_mode=2`. The broker restarts. What is left?

- A) The queue and the message
- B) The queue without the message
- C) Nothing
- D) The message without the queue

<details><summary>Answer</summary>

**B.** Durability of the queue and persistence of the message are separate.
→ [Basic theory](basics.en.md#durable-and-persistent)

</details>

## 4. 🟢 What does `basic_qos(prefetch_count=1)` limit?

- A) Messages per queue
- B) Unacknowledged messages per consumer
- C) Publishes per second
- D) Consumers per queue

<details><summary>Answer</summary>

**B.**
→ [Basic theory](basics.en.md#prefetch-and-fairness)

</details>

## 5. 🟡 A queue has `x-message-ttl` 5000 and a message `expiration="200"`. Which TTL applies?

- A) 5000
- B) 200 (the smaller)
- C) Their sum
- D) Neither

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#message-ttl)

</details>

## 6. 🟡 `x-max-length: 2` with the default overflow; four messages published. What remains?

- A) The first two
- B) The last two
- C) None
- D) All four

<details><summary>Answer</summary>

**B.** `drop-head` removes the oldest. With `reject-publish` the first two would remain.
→ [Advanced theory](advanced.en.md#length-limits-and-overflow)

</details>

## 7. 🟡 Another connection declares your `exclusive` queue. Result?

- A) It shares the queue
- B) `405 RESOURCE_LOCKED`
- C) A new queue is made
- D) `404`

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#exclusive-and-auto-delete-queues)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) A per-message TTL is enforced at the head of the queue
- [ ] B) You can change `x-message-ttl` by redeclaring the queue
- [ ] C) A policy can apply a length limit to queues matching a name pattern
- [ ] D) `auto_ack=True` loses a message if the consumer crashes

<details><summary>Answer</summary>

**A, C, D.** B is wrong: 406 `PRECONDITION_FAILED`.
→ [Expert](expert.en.md#policies)

</details>

## 9. 🟡 Two consumers, no prefetch limit, the slow one subscribed first. What can happen?

- A) Fair distribution
- B) The slow one is handed everything
- C) The fast one is refused
- D) The queue is deleted

<details><summary>Answer</summary>

**B.** In the test consumer A received all six messages.
→ [Gotcha 2](expert.en.md#2-unlimited-prefetch)

</details>

## 10. 🔴 Find the mistake

```python
def on_message(ch, method, props, body):
    ch.basic_ack(method.delivery_tag)
    send_email(body)       # may fail
```

<details><summary>Answer</summary>

The ack comes before the work: if `send_email` fails or the worker crashes, the message is gone. Ack after success; on failure `basic_nack` (preferably to a dead-letter queue).
→ [Basic theory](basics.en.md#ack-nack-reject)

</details>

## 11. 🔴 A message fails every time and the handler nacks it with `requeue=True`. What happens?

- A) It is dropped after three attempts
- B) It loops forever
- C) It goes to a dead-letter queue by default
- D) The queue is deleted

<details><summary>Answer</summary>

**B.** A classic queue has no delivery limit by default (not tested for other queue types): use `requeue=False` with a dead-letter exchange.
→ [Gotcha 7](expert.en.md#7-requeue-loops)

</details>

## 12. 🔴 Open question

Design the queue and consumer settings for a worker pool processing slow image jobs that must not be lost and must not overload memory.

<details><summary>Key points of a good answer</summary>

- A durable queue and persistent messages; manual ack after success.
- A small `prefetch_count` (1) for fair distribution of slow jobs.
- A length limit (policy) and alerting on depth; TTL if old jobs are useless.
- `requeue=False` plus a dead-letter queue for poison messages; idempotent handlers.
- Publisher confirms for the producers.

</details>

→ [Advanced theory](advanced.en.md)
