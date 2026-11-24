---
title: "Quiz: queue types"
lang: en
track: rabbitmq
kind: module
doc: quiz
slug: 07-queue-types
part: 2
tags: [rabbitmq, queue-types, quorum-queue, stream, delivery-limit]
status: draft
updated: 2026-11-24
---

# Quiz: queue types

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What type is a queue declared without `x-queue-type`?

- A) quorum
- B) stream
- C) classic
- D) An error: the type is required

<details><summary>Answer</summary>

**C.**
→ [Basics](basics.en.md#three-queue-types)

</details>

## 2. 🟢 Which type suits events that different consumers read from different positions?

- A) classic
- B) quorum
- C) stream
- D) Any

<details><summary>Answer</summary>

**C.** A stream is a non-destructive log with offsets.
→ [Basics](basics.en.md#stream-a-log-instead-of-a-queue)

</details>

## 3. 🟢 What does a quorum queue give over classic?

- A) Message priorities
- B) Consensus-based replication
- C) Exclusive mode
- D) Less overhead

<details><summary>Answer</summary>

**B.** Priorities and exclusive are not supported on quorum.
→ [Basics](basics.en.md#three-queue-types)

</details>

## 4. 🟢 Can you change an existing queue's type by redeclaring it?

- A) Yes
- B) No, `406 PRECONDITION_FAILED`
- C) Yes, but only classic → quorum
- D) No, the broker silently ignores it

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#limits-of-quorum-queues-and-streams)

</details>

## 5. 🟡 What happens on `queue_declare(..., durable=False, arguments={"x-queue-type": "quorum"})` on 4.4?

- A) The channel closes with `406`
- B) The connection closes with `541`
- C) The queue is created
- D) The type is replaced by classic

<details><summary>Answer</summary>

**B.** Non-durable non-exclusive queues are forbidden.
→ [Gotcha 3](expert.en.md#3-a-quorum-queue-without-durable)

</details>

## 6. 🟡 A stream consumer without `basic_qos`. Result?

- A) It works
- B) `406 … consumer prefetch count is not set for stream queue`
- C) Only one message is read
- D) The broker picks a prefetch itself

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#stream-a-log-instead-of-a-queue)

</details>

## 7. 🟡 A quorum queue with `x-delivery-limit: 3`. A consumer does `basic_reject(requeue=True)`. What happens?

- A) An endless loop
- B) 4 deliveries, then the DLX with reason `delivery_limit`
- C) The message goes straight to the DLX
- D) The channel closes

<details><summary>Answer</summary>

**B.** In the test: `deliveries 4 … dead 1`.
→ [Advanced theory](advanced.en.md#delivery-limit)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) Reading a stream does not delete messages
- [ ] B) `x-stream-offset: "next"` returns only new messages
- [ ] C) A number (for example 3) is allowed as `x-stream-offset`
- [ ] D) `auto_ack=True` is allowed for a stream

<details><summary>Answer</summary>

**A, B, C.** D is wrong: `406`.
→ [Advanced theory](advanced.en.md#stream-offsets)

</details>

## 9. 🟡 What does `x-stream-offset: "last"` return when six messages sit in one chunk?

- A) The last message
- B) All six
- C) Nothing
- D) An error

<details><summary>Answer</summary>

**B.** `last` starts at the last chunk.
→ [Advanced theory](advanced.en.md#stream-offsets)

</details>

## 10. 🔴 Find the mistake

```python
def on_msg(ch, m, p, body):
    try:
        handle(body)
        ch.basic_ack(m.delivery_tag)
    except Exception:
        ch.basic_nack(m.delivery_tag, requeue=True)   # relying on x-delivery-limit
```

<details><summary>Answer</summary>

In the test `nack(requeue=True)` did not increase the delivery count: the message went round and round (582 deliveries in 2 s) and never reached the DLX. Use `reject(requeue=True)` (the count grows) or `requeue=False` with a DLX.
→ [Gotcha 1](expert.en.md#1-nackrequeuetrue-and-the-delivery-limit)

</details>

## 11. 🔴 Which queue suits tasks that must not be lost when a cluster node fails?

- A) classic
- B) quorum
- C) a stream with `auto_ack`
- D) exclusive classic

<details><summary>Answer</summary>

**B.** Quorum is replicated. (Replication was not verified in this module — one node; see [module 08](../08-clustering/overview.en.md).)
→ [Basics](basics.en.md#what-to-choose)

</details>

## 12. 🔴 Open question

A service needs a task queue that must not be lost and an event log for three analytics consumers. Which queue types and settings do you choose?

<details><summary>Key points of a good answer</summary>

- Tasks: a quorum queue, durable, `x-delivery-limit` and a DLX; manual ack, `reject` for unfixable errors.
- Events: a stream, `x-stream-offset` per consumer, prefetch and ack, a retention policy by size/time.
- Migrating existing classic queues: a new queue and a move (the type does not change).
- Watch the stream's size and the DLX depth.

</details>

→ [Advanced theory](advanced.en.md)
