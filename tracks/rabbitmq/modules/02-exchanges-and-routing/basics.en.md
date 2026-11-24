---
title: "Exchanges and routing: basics"
lang: en
track: rabbitmq
kind: module
doc: basics
slug: 02-exchanges-and-routing
part: 1
tags: [rabbitmq, exchange, routing-key, binding, topic-exchange, fanout, direct-exchange, headers-exchange]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Exchanges and routing: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · Messaging and the AMQP model](../01-messaging-basics/basics.en.md) · **Time:** ~1.5 h

## How the code was checked

Everything ran against a real RabbitMQ 4.4.1 in Docker with pika 1.5.0; outputs are copied from the runs. The experiments share two helpers:

```python
import pika, time
conn = pika.Connection(pika.ConnectionParameters("localhost"))
ch = conn.channel()

def drain(q):                       # read everything from a queue
    time.sleep(0.3)                 # publishing is asynchronous
    out = []
    while True:
        m, p, b = ch.basic_get(q, auto_ack=True)
        if m is None:
            return out
        out.append(b.decode())

def setup(ex, typ, bindings):       # one exclusive queue per binding
    ch.exchange_declare(ex, exchange_type=typ)
    names = []
    for key in bindings:
        q = ch.queue_declare("", exclusive=True).method.queue
        if isinstance(key, str):
            ch.queue_bind(q, ex, routing_key=key)
        else:
            ch.queue_bind(q, ex, arguments=key)
        names.append(q)
    return names
```

## What an exchange does

A producer publishes **(exchange, routing key, message)**. The exchange looks at its bindings and copies the message into every queue whose binding matches. The match rule is the exchange **type**. The exchange itself stores nothing: if no queue matches, the message disappears.

## The default exchange

The nameless exchange `""` is a `direct` exchange the broker binds to every queue by the queue's name. That is why `basic_publish("", "work", …)` lands in the queue `work`:

```python
ch.queue_declare("work", durable=True)
ch.basic_publish("", "work", b"to work")
print("default:", drain("work"))
```

```text
default: ['to work']
```

You cannot declare, delete or bind to it by hand.

## Direct

The message goes to queues whose binding key **equals** the routing key.

```python
qs = setup("d", "direct", ["info", "error", "error"])
for k in ("info", "error", "warn"):
    ch.basic_publish("d", k, k.encode())
print("direct:", [drain(q) for q in qs])
```

```text
direct: [['info'], ['error'], ['error']]
```

Two queues bound with `error` both got it; `warn` matched nothing and vanished.

## Fanout

Every bound queue gets a copy; the routing key is ignored.

```python
qs = setup("f", "fanout", ["a", "b"])
ch.basic_publish("f", "whatever", b"hi")
print("fanout:", [drain(q) for q in qs])
```

```text
fanout: [['hi'], ['hi']]
```

## Topic

The routing key is words separated by dots. In a binding key, `*` stands for exactly one word and `#` for zero or more.

```python
pats = ["*.orange.*", "*.*.rabbit", "lazy.#", "#"]
qs = setup("t", "topic", pats)
for k in ["quick.orange.rabbit", "lazy.orange.elephant", "lazy", "quick.brown.fox", "lazy.pink.rabbit"]:
    ch.basic_publish("t", k, k.encode())
for p, q in zip(pats, qs):
    print("topic", p, "->", drain(q))
```

```text
topic *.orange.* -> ['quick.orange.rabbit', 'lazy.orange.elephant']
topic *.*.rabbit -> ['quick.orange.rabbit', 'lazy.pink.rabbit']
topic lazy.# -> ['lazy.orange.elephant', 'lazy', 'lazy.pink.rabbit']
topic # -> ['quick.orange.rabbit', 'lazy.orange.elephant', 'lazy', 'quick.brown.fox', 'lazy.pink.rabbit']
```

Note that `lazy` (a single word) matches `lazy.#`, and `#` alone matches everything (like a fanout). A binding without wildcards behaves like `direct`.

## Headers

Routing looks at the message **headers**, ignoring the routing key. A binding carries a table of headers and `x-match`: `all` requires every header to match, `any` requires at least one.

```python
args = [{"x-match": "all", "format": "pdf", "type": "report"},
        {"x-match": "any", "format": "pdf", "type": "report"}]
qs = setup("h", "headers", args)
for hd in ({"format": "pdf", "type": "report"}, {"format": "pdf"}, {"format": "zip"}):
    ch.basic_publish("h", "", str(hd).encode(), pika.BasicProperties(headers=hd))
print("headers all:", drain(qs[0]), "any:", drain(qs[1]))
```

```text
headers all: ["{'format': 'pdf', 'type': 'report'}"] any: ["{'format': 'pdf', 'type': 'report'}", "{'format': 'pdf'}"]
```

## Choosing a type

| Need | Type |
|---|---|
| Straight into a queue by name | default exchange |
| Exact category | `direct` |
| Everybody | `fanout` |
| Hierarchical pattern | `topic` |
| Several attributes | `headers` |

Topic exchanges cover most real needs; `direct` and `fanout` are their special cases.

## Beginner mistakes

- ❌ Publishing to a named exchange but never binding a queue → ✅ bind first; otherwise the message is dropped.
- ❌ Expecting `*` to match several words → ✅ `*` is one word, `#` is many.
- ❌ Using a routing key with a fanout exchange to filter → ✅ it is ignored; use `direct` or `topic`.
- ❌ Forgetting `x-match` → ✅ set `all` or `any` explicitly.
- ❌ Declaring an exchange again with a different type → ✅ `406 PRECONDITION_FAILED`.

## Related materials

- Next: [advanced theory](advanced.en.md) — unroutable messages, alternate exchange
- Earlier: [01 · Messaging and the AMQP model](../01-messaging-basics/basics.en.md)
- Next module: [03 · Queues and delivery](../03-queues-and-delivery/basics.en.md)

## Sources

- [AMQP 0-9-1 Model Explained](https://www.rabbitmq.com/tutorials/amqp-concepts)
- [Exchanges](https://www.rabbitmq.com/docs/exchanges)
- [Tutorial 4: Routing](https://www.rabbitmq.com/tutorials/tutorial-four-python)
- [Tutorial 5: Topics](https://www.rabbitmq.com/tutorials/tutorial-five-python)
