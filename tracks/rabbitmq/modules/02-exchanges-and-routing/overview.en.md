---
title: "Exchanges and routing"
lang: en
track: rabbitmq
kind: module
doc: overview
slug: 02-exchanges-and-routing
part: 1
levels: [1, 2, 3, 4]
tags: [rabbitmq, exchange, routing-key, binding, topic-exchange, fanout, direct-exchange, headers-exchange, alternate-exchange]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Exchanges and routing

🌐 [Русский](overview.ru.md) · **English**

## In short

An exchange receives messages and decides which queues get a copy. The decision depends on the exchange **type** and its **bindings**: `direct` matches the routing key exactly, `fanout` copies to every bound queue, `topic` matches dotted patterns with `*` and `#`, `headers` matches message headers. A message that reaches no queue is dropped, unless you ask to have it returned (`mandatory`) or sent to an **alternate exchange**.

## Key ideas

- **Four types:** `direct`, `fanout`, `topic`, `headers`; plus the nameless **default exchange**, a `direct` exchange with an implicit binding for every queue.
- **A binding** is a rule "exchange → queue" with a binding key (or arguments for `headers`).
- **`direct`:** the routing key equals the binding key. Several queues bound with the same key all get the message.
- **`fanout`:** the key is ignored.
- **`topic`:** `*` is exactly one word, `#` is zero or more words; words are separated by dots. The key `lazy` matches `lazy.#`.
- **`headers`:** `x-match: all` needs every header, `x-match: any` needs one.
- **One queue gets one copy** even if several of its bindings match.
- **Unroutable messages vanish silently** — unless published with `mandatory=True` (returned with `312 NO_ROUTE`) or the exchange has an `alternate-exchange`.
- **Redeclaring an exchange with other arguments fails** (`406 PRECONDITION_FAILED`).

## Minimal example

```python
ch.exchange_declare("logs", exchange_type="topic")
q = ch.queue_declare("", exclusive=True).method.queue
ch.queue_bind(q, "logs", routing_key="app.*.error")
ch.basic_publish("logs", "app.db.error", b"disk full")      # delivered to q
ch.basic_publish("logs", "app.db.warn", b"slow query")      # dropped: no match
```

All examples were run against a real RabbitMQ 4.4.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Task | Exchange |
|---|---|
| Send to one named queue | the default exchange |
| Route by a fixed category (`info`, `error`) | `direct` |
| Broadcast to all subscribers | `fanout` |
| Route by a hierarchy (`region.service.level`) | `topic` |
| Route by several attributes, not a key | `headers` |

- ❌ Do not put business data in the routing key beyond routing needs.
- ❌ Do not rely on `headers` exchanges for performance-critical paths: they match against a table.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | the four types and the default exchange with checked outputs |
| [2. Advanced theory](advanced.en.md) | unroutable messages, `mandatory`, alternate exchange, exchange-to-exchange bindings, internal and auto-delete exchanges |
| [3. Expert theory](expert.en.md) | a reference of declare options and rules, reserved names, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn rabbitmq 02-exchanges-and-routing level 5` |
| 6. Tests | not created yet — `/learn rabbitmq 02-exchanges-and-routing level 6` |

## Related materials

- Prerequisites: [01 · Messaging and the AMQP model](../01-messaging-basics/overview.en.md)
- Next: [03 · Queues and delivery](../03-queues-and-delivery/overview.en.md)
- Analogues in other tracks: [Kubernetes Services and routing](../../../kubernetes/overview.en.md)
