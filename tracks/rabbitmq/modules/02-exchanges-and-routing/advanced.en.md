---
title: "Exchanges and routing: advanced"
lang: en
track: rabbitmq
kind: module
doc: advanced
slug: 02-exchanges-and-routing
part: 1
tags: [rabbitmq, exchange, mandatory, alternate-exchange, unroutable-messages, exchange-binding]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Exchanges and routing: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md) (same helpers `drain` and `setup`). All outputs come from a real RabbitMQ 4.4.1.

## Unroutable messages and `mandatory`

A message that matches no binding is dropped without an error. If losing it is unacceptable, publish with `mandatory=True`: the broker then **returns** the message to the publisher with `basic.return`. In pika, register a return callback:

```python
returns = []
ch.add_on_return_callback(lambda c, m, p, b: returns.append((m.reply_code, m.reply_text, m.routing_key, b)))
ch.basic_publish("d", "nokey", b"x", mandatory=True)
time.sleep(0.5)
print("returned:", returns)
```

```text
returned: [(312, 'NO_ROUTE', 'nokey', b'x')]
```

The return is asynchronous, so a publisher needs a callback, and to know that a message *was* routed it needs publisher confirms (module 05 in the [roadmap](../../overview.en.md)).

## Alternate exchange

An exchange may name an **alternate exchange**: messages it cannot route go there instead of vanishing. This collects "dead" traffic in one queue for inspection:

```python
ch.exchange_declare("unrouted", exchange_type="fanout")
uq = ch.queue_declare("", exclusive=True).method.queue
ch.queue_bind(uq, "unrouted")
ch.exchange_declare("main", exchange_type="direct", arguments={"alternate-exchange": "unrouted"})
mq = ch.queue_declare("", exclusive=True).method.queue
ch.queue_bind(mq, "main", routing_key="ok")
ch.basic_publish("main", "ok", b"routed")
ch.basic_publish("main", "bad", b"unrouted")
print("main:", drain(mq), "alt:", drain(uq))
```

```text
main: ['routed'] alt: ['unrouted']
```

Use a `fanout` alternate exchange so nothing is lost again at the second hop.

## Exchange-to-exchange bindings

Exchanges can be bound to exchanges, building a routing graph: a coarse exchange fans out into topic exchanges, and so on.

```python
ch.exchange_declare("src", exchange_type="fanout")
ch.exchange_declare("dst", exchange_type="topic")
ch.exchange_bind("dst", "src")
q = ch.queue_declare("", exclusive=True).method.queue
ch.queue_bind(q, "dst", routing_key="#")
ch.basic_publish("src", "k", b"hop")
print("e2e:", drain(q))
```

```text
e2e: ['hop']
```

## One queue, several matching bindings

If several bindings of one queue match, the queue still receives **one** copy. A test with `a.b` and `a.#` bound to one queue and the key `a.b` left exactly one message in it.

## Internal and auto-delete exchanges

- **`internal=True`:** producers cannot publish to it; it is a target only for other exchanges' bindings. Publishing to one closed the channel in a test.
- **`auto_delete=True`:** the exchange is deleted after its last binding is removed. After the last unbind, a passive declare returned `404`.

## Declaration rules

- Redeclaring with a different type fails: `406 PRECONDITION_FAILED - inequivalent arg 'type' for exchange 'd' … received 'fanout' but current is 'direct'`. The same holds for other arguments.
- An unknown type is refused: `406 PRECONDITION_FAILED - unknown exchange type 'nonsense'`.
- Both errors close the channel.
- Names starting with `amq.` cannot be declared by clients: `403 ACCESS_REFUSED - exchange name 'amq.custom' contains reserved prefix 'amq.*'`.
- The default exchange cannot be deleted or bound to: `403 ACCESS_REFUSED - operation not permitted on the default exchange`.

## Best practices

- Declare exchanges and bindings in one start-up routine shared by producers and consumers.
- Prefer `topic` with disciplined keys (`<domain>.<entity>.<event>`) over many `direct` exchanges.
- Give important exchanges an alternate exchange or publish with `mandatory`.
- Keep routing keys short and lower-case, without spaces.

## Common mistakes

- ❌ Assuming a dropped message raises an error → ✅ `mandatory` or an alternate exchange.
- ❌ Declaring the exchange only in the consumer → ✅ the producer may start first and publish into nothing.
- ❌ Binding a queue twice and expecting two copies → ✅ one copy.

## Related materials

- Next: [expert theory](expert.en.md)
- Earlier: [01 · Messaging and the AMQP model](../01-messaging-basics/advanced.en.md)
- Next module: [03 · Queues and delivery](../03-queues-and-delivery/advanced.en.md)

## Sources

- [Exchanges](https://www.rabbitmq.com/docs/exchanges)
- [Alternate Exchanges](https://www.rabbitmq.com/docs/ae)
- [Publishers](https://www.rabbitmq.com/docs/publishers)
