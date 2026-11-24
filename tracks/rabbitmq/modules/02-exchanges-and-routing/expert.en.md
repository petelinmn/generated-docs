---
title: "Exchanges and routing: expert"
lang: en
track: rabbitmq
kind: module
doc: expert
slug: 02-exchanges-and-routing
part: 1
tags: [rabbitmq, exchange, routing-key, topic-exchange, headers-exchange, alternate-exchange, mandatory]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Exchanges and routing: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. Behaviour marked "tested" was observed on a RabbitMQ 4.4.1 broker.

## Reference

### Exchange types

| Type | Match rule | Routing key | Tested |
|---|---|---|---|
| default (`""`) | the queue name equals the key | used | yes |
| `direct` | key equals binding key | used | yes |
| `fanout` | every bound queue | ignored | yes |
| `topic` | dotted words; `*` one word, `#` zero or more | used | yes |
| `headers` | headers table plus `x-match` | ignored | yes (`all`, `any`) |

> ⚠️ Verify: the documentation also describes `x-match` values `all-with-x` and `any-with-x` (headers starting with `x-` taken into account), and a default of `all` when `x-match` is absent; these were not tested here.

### Declare options (`exchange_declare`)

| Option | Meaning | Tested |
|---|---|---|
| `exchange_type` | `direct`, `fanout`, `topic`, `headers` (others → 406 `unknown exchange type`) | yes |
| `durable` | survives a broker restart | no |
| `auto_delete` | deleted after the last binding is removed | yes |
| `internal` | not publishable by clients | yes |
| `passive` | only check it exists (404 and a closed channel if not) | yes |
| `arguments` | `alternate-exchange` and plugin arguments | yes |

### Rules and error codes

| Situation | Result |
|---|---|
| Redeclare with a different type or arguments | `406 PRECONDITION_FAILED`, channel closed |
| Unknown exchange type | `406 PRECONDITION_FAILED` |
| Name with prefix `amq.` | `403 ACCESS_REFUSED` |
| Delete or bind the default exchange | `403 ACCESS_REFUSED` |
| Publish to an `internal` exchange | the channel is closed |
| `mandatory=True` and no route | `basic.return` with `312 NO_ROUTE` |

## Internals

- **Routing is a lookup by binding.** For `direct` and `topic` the broker evaluates the key against the exchange's binding table; a queue appears at most once in the result set, which is why several matching bindings still yield one copy.
- **A message is not copied per queue by value on the wire.** The producer sends it once; the broker places a reference into each target queue (the effect you see is one delivery per queue).
- **The default exchange is implicit:** the broker treats every queue as bound by name, which is why it cannot be deleted or re-bound.
- **Alternate exchange chains** (an alternate exchange with its own alternate) are possible; the unroutable message ends at the last exchange that cannot route it.

> ⚠️ Verify: the claim that messages are stored once and referenced by queues describes classic storage design and was not examined here; treat it as background.

## Gotchas

### 1. Silent drop

Publishing to an exchange with no matching binding does nothing visible.

### 2. `*` is one word

`*.orange.*` matched `quick.orange.rabbit` and `lazy.orange.elephant` but not `lazy` or `quick.brown.fox`.

### 3. `#` matches zero words

`lazy.#` matched `lazy` itself.

### 4. Two bindings, one copy

Binding a queue with `a.b` and `a.#` and publishing `a.b` produced exactly one message.

### 5. Fanout ignores the key

A key given to a fanout exchange does nothing.

### 6. Mismatched redeclaration

A consumer declaring `topic` while a producer created `direct` closes its channel with 406.

### 7. Internal exchange publish

Closes the channel, so a wrong target is noticed only through the close callback.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| `topic` with a key scheme | flexible, evolves | needs key discipline |
| Many `direct` exchanges | simple per exchange | topology sprawl |
| `headers` | multi-attribute matching | slower, harder to inspect |
| Alternate exchange | catches dead traffic | one more queue to watch |
| `mandatory` + return callback | per-publish feedback | asynchronous handling in the publisher |

## Interview questions

<details><summary>1. What are the exchange types?</summary>

`direct`, `fanout`, `topic`, `headers`, plus the default (nameless) exchange.

→ [Basic theory](basics.en.md#what-an-exchange-does)

</details>

<details><summary>2. How does the default exchange work?</summary>

It is a direct exchange implicitly bound to every queue by queue name.

→ [Basic theory](basics.en.md#the-default-exchange)

</details>

<details><summary>3. Trick: does `lazy.#` match the key `lazy`?</summary>

Yes, `#` matches zero or more words.

→ [Gotcha 3](#3--matches-zero-words)

</details>

<details><summary>4. Trick: a queue is bound with `a.b` and `a.#`. You publish `a.b`. How many copies?</summary>

One.

→ [Gotcha 4](#4-two-bindings-one-copy)

</details>

<details><summary>5. What happens to an unroutable message?</summary>

It is dropped, unless `mandatory` returns it (`312 NO_ROUTE`) or an alternate exchange takes it.

→ [Advanced theory](advanced.en.md#unroutable-messages-and-mandatory)

</details>

<details><summary>6. What is an alternate exchange for?</summary>

To collect messages that an exchange cannot route.

→ [Advanced theory](advanced.en.md#alternate-exchange)

</details>

<details><summary>7. Does a fanout exchange use the routing key?</summary>

No, it is ignored.

→ [Gotcha 5](#5-fanout-ignores-the-key)

</details>

<details><summary>8. What do `x-match: all` and `any` mean?</summary>

All of the binding's headers must match, or at least one.

→ [Basic theory](basics.en.md#headers)

</details>

<details><summary>9. Trick: what happens when you redeclare an exchange with another type?</summary>

`406 PRECONDITION_FAILED`, and the channel closes.

→ [Gotcha 6](#6-mismatched-redeclaration)

</details>

<details><summary>10. What is an internal exchange?</summary>

One that clients cannot publish to; only other exchanges can route into it.

→ [Advanced theory](advanced.en.md#internal-and-auto-delete-exchanges)

</details>

<details><summary>11. Can you bind exchanges to exchanges?</summary>

Yes (`exchange_bind`), forming a routing graph.

→ [Advanced theory](advanced.en.md#exchange-to-exchange-bindings)

</details>

<details><summary>12. Why prefer `topic` over many direct exchanges?</summary>

One exchange with a key scheme covers many cases, including direct and fanout behaviour.

→ [Basic theory](basics.en.md#choosing-a-type)

</details>

## Cheat sheet

```text
default "" : routes by queue name; cannot declare/delete/bind
direct     : key == binding key
fanout     : key ignored
topic      : * = one word, # = zero or more, words split by "."
headers    : arguments + x-match=all|any
exchange_declare(ex, exchange_type, durable, auto_delete, internal, passive, arguments)
alternate-exchange : arguments on the exchange; mandatory=True -> basic.return 312 NO_ROUTE
```

- Bind before publishing.
- One copy per queue.
- Redeclare with identical arguments.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Earlier: [01 · Messaging and the AMQP model](../01-messaging-basics/expert.en.md)
- Next module: [03 · Queues and delivery](../03-queues-and-delivery/expert.en.md)

## Sources

- [Exchanges](https://www.rabbitmq.com/docs/exchanges)
- [Alternate Exchanges](https://www.rabbitmq.com/docs/ae)
- [AMQP 0-9-1 Complete Reference](https://www.rabbitmq.com/amqp-0-9-1-reference)
