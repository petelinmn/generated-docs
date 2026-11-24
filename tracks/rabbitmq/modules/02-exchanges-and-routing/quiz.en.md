---
title: "Quiz: exchanges and routing"
lang: en
track: rabbitmq
kind: module
doc: quiz
slug: 02-exchanges-and-routing
part: 1
tags: [rabbitmq, exchange, routing-key, topic-exchange, alternate-exchange]
status: draft
updated: 2026-11-24
---

# Quiz: exchanges and routing

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which exchange ignores the routing key?

- A) `direct`
- B) `fanout`
- C) `topic`
- D) the default exchange

<details><summary>Answer</summary>

**B.** (`headers` also ignores it and uses headers instead.)
→ [Basic theory](basics.en.md#fanout)

</details>

## 2. 🟢 What does `basic_publish("", "work", …)` do?

- A) Fails: the exchange name is empty
- B) Publishes via the default exchange into the queue named `work`
- C) Publishes to a fanout
- D) Creates the queue `work`

<details><summary>Answer</summary>

**B.** The default exchange routes by queue name; it does not create queues.
→ [Basic theory](basics.en.md#the-default-exchange)

</details>

## 3. 🟢 Which keys match the binding `lazy.#`?

- A) `lazy` only
- B) `lazy.pink.rabbit` only
- C) `lazy`, `lazy.orange.elephant`, `lazy.pink.rabbit`
- D) every key

<details><summary>Answer</summary>

**C.** `#` matches zero or more words.
→ [Basic theory](basics.en.md#topic)

</details>

## 4. 🟢 In a `topic` binding, `*` means

- A) any number of words
- B) exactly one word
- C) any characters
- D) zero or one word

<details><summary>Answer</summary>

**B.**
→ [Basic theory](basics.en.md#topic)

</details>

## 5. 🟡 A message matches no binding. What happens by default?

- A) The producer gets an exception
- B) It is dropped silently
- C) It goes to the first queue
- D) It waits in the exchange

<details><summary>Answer</summary>

**B.** Use `mandatory=True` or an alternate exchange to catch it.
→ [Advanced theory](advanced.en.md#unroutable-messages-and-mandatory)

</details>

## 6. 🟡 What does a `mandatory` publish with no route produce?

- A) A 404 on the channel
- B) `basic.return` with `312 NO_ROUTE`
- C) A message in a dead-letter queue
- D) Nothing

<details><summary>Answer</summary>

**B.** Delivered to the return callback, asynchronously.
→ [Advanced theory](advanced.en.md#unroutable-messages-and-mandatory)

</details>

## 7. 🟡 A queue is bound to a topic exchange with both `a.b` and `a.#`. How many copies of `a.b`?

- A) 0
- B) 1
- C) 2
- D) Depends on the order of bindings

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#one-queue-several-matching-bindings)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) `x-match: any` needs at least one header to match
- [ ] B) A fanout exchange filters by routing key
- [ ] C) You can bind an exchange to another exchange
- [ ] D) You can delete the default exchange

<details><summary>Answer</summary>

**A, C.** B is wrong (the key is ignored); D is wrong (403 `ACCESS_REFUSED`).
→ [Expert](expert.en.md#rules-and-error-codes)

</details>

## 9. 🟡 What happens if a consumer redeclares an existing `direct` exchange as `fanout`?

- A) The type changes
- B) `406 PRECONDITION_FAILED` and the channel closes
- C) A second exchange is created
- D) Nothing

<details><summary>Answer</summary>

**B.**
→ [Gotcha 6](expert.en.md#6-mismatched-redeclaration)

</details>

## 10. 🔴 Find the mistake

```python
ch.exchange_declare("events", exchange_type="topic")
ch.basic_publish("events", "order.created", b"...")   # no queues bound yet
```

<details><summary>Answer</summary>

No queue is bound, so the message is silently dropped. Bind the consumers' queues before publishing, or make the exchange's alternate exchange catch unrouted messages, or publish with `mandatory=True`.
→ [Advanced theory](advanced.en.md#alternate-exchange)

</details>

## 11. 🔴 What happens when you publish to an `internal` exchange?

- A) It routes normally
- B) The broker closes the channel
- C) The message is returned
- D) It is queued for later

<details><summary>Answer</summary>

**B.** Internal exchanges accept messages only from other exchanges' bindings.
→ [Advanced theory](advanced.en.md#internal-and-auto-delete-exchanges)

</details>

## 12. 🔴 Open question

Design the exchanges for an order system where billing wants all order events, shipping wants only `order.paid`, and an audit service wants everything, including events nobody routes.

<details><summary>Key points of a good answer</summary>

- One `topic` exchange (`orders`) with keys `order.<event>`.
- Billing queue bound with `order.#`; shipping with `order.paid`; audit with `#`.
- An alternate exchange (fanout) with a queue for unrouted messages; `mandatory` for critical publishes.
- Topology declared in one shared start-up routine; durable exchanges and queues.

</details>

→ [Advanced theory](advanced.en.md)
