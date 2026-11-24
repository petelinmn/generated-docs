---
title: "Quiz: messaging patterns"
lang: en
track: rabbitmq
kind: module
doc: quiz
slug: 04-messaging-patterns
part: 1
tags: [rabbitmq, work-queue, publish-subscribe, routing, rpc, reply-to]
status: draft
updated: 2026-11-24
---

# Quiz: messaging patterns

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 A queue holds ten messages and three workers read it. How are messages delivered?

- A) Each to all three workers
- B) Each to exactly one worker
- C) The first to all, the rest round-robin
- D) It depends on a fanout exchange being bound to the queue

<details><summary>Answer</summary>

**B.** This is a work queue: the queue divides messages. Only subscribers with separate queues receive copies.
→ [Basic theory](basics.en.md#work-queue)

</details>

## 2. 🟢 Which exchange suits pub/sub "to everyone"?

- A) default
- B) direct with one key
- C) fanout
- D) Any: the exchange type does not affect copying

<details><summary>Answer</summary>

**C.** A fanout copies the message to all bound queues regardless of the key. Default and direct deliver on a key match.
→ [Basic theory](basics.en.md#publishsubscribe)

</details>

## 3. 🟢 What does `queue_declare("", exclusive=True)` do?

- A) Creates a queue with an empty name
- B) The broker assigns a name, and the queue is deleted with the connection
- C) Creates a durable queue
- D) Error: a name is required

<details><summary>Answer</summary>

**B.** A name like `amq.gen-…`.
→ [Basic theory](basics.en.md#publishsubscribe)

</details>

## 4. 🟢 Why `correlation_id` in RPC?

- A) So the broker routes the reply
- B) So the client can match the reply to the request
- C) So the message is persistent
- D) So the reply queue is durable

<details><summary>Answer</summary>

**B.** The broker does not interpret this property.
→ [Basic theory](basics.en.md#rpc)

</details>

## 5. 🟡 What happens when you consume `amq.rabbitmq.reply-to` with `auto_ack=False`?

- A) It works
- B) `406 PRECONDITION_FAILED - reply consumer cannot acknowledge`
- C) The subscription is silently ignored
- D) `404 NOT_FOUND`

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#direct-reply-to)

</details>

## 6. 🟡 An RPC client publishes a request with `reply_to="amq.rabbitmq.reply-to"` without consuming first. Result?

- A) The reply is lost
- B) The channel closes
- C) The broker creates a queue
- D) The request is not published and the client gets an exception at once

<details><summary>Answer</summary>

**B.** In the test `is_open` became `False`.
→ [Advanced theory](advanced.en.md#direct-reply-to)

</details>

## 7. 🟡 Why `expiration` on an RPC request?

- A) So the server replies faster
- B) So a stale request is not executed when the server returns
- C) So the reply is persistent
- D) So the request queue is length-limited

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#an-rpc-client-with-a-timeout)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) Publishing to a fanout with no bound queues loses the message
- [ ] B) A pub/sub subscriber that arrives later receives earlier messages
- [ ] C) Several bindings of one queue with different keys are allowed
- [ ] D) A fast worker with `prefetch_count=1` usually gets more tasks

<details><summary>Answer</summary>

**A, C, D.** B is wrong: an exchange stores nothing.
→ [Basic theory](basics.en.md)

</details>

## 9. 🟡 Two subscribers read one named queue bound to a fanout. What results?

- A) Each gets all messages
- B) Messages are divided between them
- C) The broker refuses the second consumer
- D) Messages are duplicated

<details><summary>Answer</summary>

**B.** It is a work queue behind a fanout.
→ [Gotcha 1](expert.en.md#1-fanout-and-a-shared-queue)

</details>

## 10. 🔴 Find the mistake

```python
def on_req(ch, m, p, body):
    ch.basic_publish("", p.reply_to, b"ok")
    ch.basic_ack(m.delivery_tag)
```

<details><summary>Answer</summary>

The reply has no `correlation_id`: the client cannot tie it to the request. Pass `pika.BasicProperties(correlation_id=p.correlation_id)`.
→ [Basic theory](basics.en.md#rpc)

</details>

## 11. 🔴 An RPC client stores every reply in a dictionary and takes whatever sits under its id. What can go wrong?

- A) Nothing
- B) "Foreign" replies end up in the dictionary too; accept only expected ids and remove handled ones
- C) The broker sends replies to all clients
- D) Replies are lost

<details><summary>Answer</summary>

**B.** In the test a reply with id `other` landed in the dictionary.
→ [Gotcha 4](expert.en.md#4-a-foreign-correlation_id)

</details>

## 12. 🔴 Open question

An order service must learn a cart's price from a pricing service without depending on its availability. How would you design it: RPC or otherwise? Explain the trade-off.

<details><summary>Key points of a good answer</summary>

- RPC couples the client's availability to the server; you need a timeout, `expiration`, idempotency.
- Alternative: a command plus a result event, the client receives the price asynchronously.
- If the answer is needed right now — direct reply-to and a timeout with degradation (cache/default value).
- Duplicates: `message_id` and an idempotent handler.

</details>

→ [Advanced theory](advanced.en.md#choosing-a-pattern)
