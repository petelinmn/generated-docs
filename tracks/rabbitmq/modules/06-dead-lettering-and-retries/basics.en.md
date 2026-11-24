---
title: "Dead-lettering and retries: basics"
lang: en
track: rabbitmq
kind: module
doc: basics
slug: 06-dead-lettering-and-retries
part: 2
tags: [rabbitmq, dead-letter-exchange, dlx, x-death, poison-message, message-ttl]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Dead-lettering and retries: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [03 · Queues and delivery](../03-queues-and-delivery/basics.en.md), [05 · Reliability](../05-reliability/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

All snippets were run against a real RabbitMQ 4.4.1 in Docker with pika 1.5.0; outputs are copied from the runs. Helpers: `get(q)` waits up to 2 s and returns `(method, props, body)` of the first message of a queue (via `basic_get` with `auto_ack`), `show(p)` prints the `x-death` header in short form (`queue`, `reason`, `count`, `exchange`, `routing-keys`).

## Why dead-lettering

A message that cannot be processed must neither loop (`requeue=True`, [module 03](../03-queues-and-delivery/expert.en.md#7-requeue-loops)) nor vanish. A **dead letter exchange (DLX)** is an exchange to which the broker redirects a queue's "dead" messages. From there they land in an ordinary queue for inspection.

A message becomes dead-lettered when:

| Reason (`reason`) | When |
|---|---|
| `rejected` | a consumer did `reject`/`nack` with `requeue=False` |
| `expired` | the message or queue TTL ran out |
| `maxlen` | the message was evicted from the queue by `x-max-length` (`drop-head`) |

> A fourth reason, `delivery_limit`, belongs to quorum queues ([module 07 in the roadmap](../../overview.en.md)). The [advanced theory](advanced.en.md#delivery-limit-of-quorum-queues) describes what happened when it was tried.

## Setup

A DLX is set by queue arguments: `x-dead-letter-exchange` and optionally `x-dead-letter-routing-key` (otherwise the original key is kept):

```python
ch.exchange_declare("dlx", "direct")
ch.queue_declare("dead", durable=True)
ch.queue_bind("dead", "dlx", "dead")
ch.queue_declare("work", durable=True, arguments={
    "x-dead-letter-exchange": "dlx",
    "x-dead-letter-routing-key": "dead",
})
```

## A rejected message

```python
ch.basic_publish("", "work", b"bad")
m, _, _ = ch.basic_get("work")
ch.basic_reject(m.delivery_tag, requeue=False)
m, p, b = get("dead")
print("rejected:", b, show(p))
print("first-death:", p.headers.get("x-first-death-reason"), p.headers.get("x-first-death-queue"), p.headers.get("x-last-death-reason"))
print("routing key now:", m.routing_key)
```

```text
rejected: b'bad' [{'count': 1, 'reason': 'rejected', 'queue': 'work', 'exchange': '', 'routing-keys': ['work']}]
first-death: rejected work rejected
routing key now: dead
```

The broker added an `x-death` header — a list of "who, why and how many times" records — and the summary fields `x-first-death-*` and `x-last-death-*`. The routing key became `dead` (from `x-dead-letter-routing-key`).

## Without a DLX the message vanishes

If a queue has no DLX, a `reject` with `requeue=False` simply drops the message:

```text
plain queue depth: 0
```

So every work queue gets a DLX and a queue for inspection.

## Other reasons

An expired (`x-message-ttl` 200 ms) and an evicted (`x-max-length: 1`) message also go to the DLX; the reason is recorded in `x-death`:

```text
expired: b'old' [{'count': 1, 'reason': 'expired', 'queue': 'ttlq', 'exchange': '', 'routing-keys': ['ttlq']}]
maxlen: b'first' [{'count': 1, 'reason': 'maxlen', 'queue': 'mlq', 'exchange': '', 'routing-keys': ['mlq']}]
```

With `maxlen` and `drop-head` the **oldest** message (`first`) becomes dead, not the new one.

## Common beginner mistakes

- ❌ A work queue without a DLX → ✅ messages vanish on `reject`; set up a DLX.
- ❌ `nack(requeue=True)` for an unfixable error → ✅ `requeue=False` and a DLX.
- ❌ A DLX with no bound queue → ✅ the message silently disappears ([advanced theory](advanced.en.md#a-dlx-with-no-route)).
- ❌ Ignoring `x-death` → ✅ it holds the reason and the number of attempts.
- ❌ Expecting the DLX to retry by itself → ✅ a retry must be built (delay + return), see the [advanced theory](advanced.en.md).

## Related materials

- Next: [advanced theory](advanced.en.md) — delayed retries, poison messages
- Before: [05 · Reliability](../05-reliability/basics.en.md), [03 · Queues and delivery](../03-queues-and-delivery/basics.en.md)
- Next modules: [07 · Queue types](../../overview.en.md)

## Sources

- [Dead Letter Exchanges](https://www.rabbitmq.com/docs/dlx)
- [Time-To-Live and Expiration](https://www.rabbitmq.com/docs/ttl)
- [Queue Length Limits](https://www.rabbitmq.com/docs/maxlength)
