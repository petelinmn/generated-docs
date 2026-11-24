---
title: "Quiz: messaging and the AMQP model"
lang: en
track: rabbitmq
kind: module
doc: quiz
slug: 01-messaging-basics
part: 1
tags: [rabbitmq, amqp, queue, exchange, channel, acknowledgement]
status: draft
updated: 2026-11-24
---

# Quiz: messaging and the AMQP model

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Where does a producer publish a message in AMQP 0-9-1?

- A) Straight into a queue
- B) Into an exchange
- C) Into a consumer
- D) Into a binding

<details><summary>Answer</summary>

**B.** The exchange routes it into queues. The default exchange only makes it look like a direct write to a queue.
→ [Basic theory](basics.en.md#the-amqp-model)

</details>

## 2. 🟢 What is the difference between a connection and a channel?

- A) None
- B) A connection is one TCP connection; channels are cheap virtual connections inside it
- C) A channel is a TCP connection; a connection is a thread
- D) A connection is for publishing, a channel for consuming

<details><summary>Answer</summary>

**B.**
→ [Basic theory](basics.en.md#connection-and-channel)

</details>

## 3. 🟢 A consumer received a message and crashed before acking. What happens?

- A) The message is lost
- B) It is requeued and marked `redelivered`
- C) It goes to another exchange
- D) The producer is told

<details><summary>Answer</summary>

**B.** With `auto_ack=True` it would be lost instead.
→ [Basic theory](basics.en.md#acknowledgements)

</details>

## 4. 🟢 Which declaration fails on RabbitMQ 4.4?

- A) `queue_declare("q", durable=True)`
- B) `queue_declare("", exclusive=True)`
- C) `queue_declare("q")`
- D) `queue_declare("q", durable=True, passive=True)` for an existing queue

<details><summary>Answer</summary>

**C.** A queue that is neither durable nor exclusive is refused and the connection closes with 541.
→ [Basic theory](basics.en.md#queues-must-be-durable-or-exclusive)

</details>

## 5. 🟡 Which part of a `BasicProperties` carries a per-message TTL, and of what type?

- A) `ttl`, an integer
- B) `expiration`, a string of milliseconds
- C) `timestamp`, a float
- D) `headers["ttl"]`

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#message-properties)

</details>

## 6. 🟡 Login fails with a wrong password. Which close code?

- A) 404
- B) 406
- C) 403 `ACCESS_REFUSED`
- D) 530

<details><summary>Answer</summary>

**C.** 530 is a missing virtual host; 404 and 406 close channels.
→ [Advanced theory](advanced.en.md#connection-parameters)

</details>

## 7. 🟡 `basic_publish` returned without an exception. What can you conclude?

- A) The message is in the queue
- B) The message was sent; errors may still arrive later by closing the channel
- C) The consumer received it
- D) The message is persistent

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#errors-are-asynchronous)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) A passive declare of a missing queue closes the channel with 404
- [ ] B) A passive declare of a missing queue closes the connection
- [ ] C) Publishing to a missing queue name through the default exchange raises an error
- [ ] D) `pika.Connection` allows several threads to publish at once

<details><summary>Answer</summary>

**A, D.** B is wrong (only the channel closes); C is wrong (the message is silently dropped).
→ [Expert](expert.en.md#gotchas)

</details>

## 9. 🟡 Why can `message_count` read right after a publish be too small?

- A) A bug in pika
- B) Publishing is asynchronous and the queue updates its count later
- C) The queue is full
- D) The message is persistent

<details><summary>Answer</summary>

**B.**
→ [Gotcha 4](expert.en.md#4-the-count-lags)

</details>

## 10. 🔴 Find the mistake

```python
method, props, body = ch.basic_get("jobs", auto_ack=True)
process(body)          # may crash
```

<details><summary>Answer</summary>

With `auto_ack=True` the broker treats the message as delivered at once; a crash in `process` loses it. Use `auto_ack=False` and `basic_ack` after success.
→ [Basic theory](basics.en.md#acknowledgements)

</details>

## 11. 🔴 What happens if you ack a delivery tag on a different channel?

- A) It works, tags are global
- B) That other channel is closed by the broker
- C) The message is requeued
- D) The connection is closed

<details><summary>Answer</summary>

**B.** Delivery tags are per channel; in a test the other channel closed and the original stayed open.
→ [Expert](expert.en.md#internals)

</details>

## 12. 🔴 Open question

Design how a web application should connect to RabbitMQ: how many connections and channels, where topology is declared, how failures are handled.

<details><summary>Key points of a good answer</summary>

- One connection per process (maybe a second for publishing), channels per task or thread, one purpose per channel.
- Topology declared once at start-up, idempotently, with `durable=True`.
- Close callbacks on channel and connection; reconnect with a back-off and re-declare.
- Publisher confirms and `mandatory` for important messages; ack after processing.
- Credentials from the environment, a dedicated vhost and user.

</details>

→ [Advanced theory](advanced.en.md)
