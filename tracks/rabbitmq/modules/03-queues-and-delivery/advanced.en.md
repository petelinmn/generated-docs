---
title: "Queues and delivery: advanced"
lang: en
track: rabbitmq
kind: module
doc: advanced
slug: 03-queues-and-delivery
part: 1
tags: [rabbitmq, queue, message-ttl, queue-length-limit, overflow, exclusive-queue, auto-delete-queue, queue-expiry]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Queues and delivery: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md) (same helpers). All outputs come from a real RabbitMQ 4.4.1.

## Message TTL

A queue-wide TTL is set with the argument `x-message-ttl` (milliseconds); a single message gets its own with the `expiration` property (a string). An expired message is dropped (or dead-lettered).

```python
fresh("ttl", arguments={"x-message-ttl": 300})
ch.basic_publish("", "ttl", b"short-lived")
print("ttl before:", count("ttl"), end=" ")
time.sleep(0.7)
print("after:", count("ttl"))

fresh("perm")
ch.basic_publish("", "perm", b"x", pika.BasicProperties(expiration="200"))
time.sleep(0.6)
print("per-message expiration:", count("perm"))
```

```text
ttl before: 1 after: 0
per-message expiration: 0
```

When both are set, the **smaller** applies: a queue TTL of 5000 ms with a message `expiration` of 200 left 0 messages after 0.8 s, and a queue TTL of 200 with a message expiration of 5000 also left 0. But a per-message TTL is only enforced at the head of the queue: a `short` (200 ms) message published behind a `long` (5000 ms) one was still counted (2 messages) after 0.8 s.

## Length limits and overflow

`x-max-length` (messages) bounds a queue. When it is full, `x-overflow` decides:

- `drop-head` (default): the oldest message is dropped;
- `reject-publish`: new messages are refused.

```python
fresh("cap", arguments={"x-max-length": 2})
for i in range(4):
    ch.basic_publish("", "cap", f"c{i}".encode())
print([ch.basic_get("cap", auto_ack=True)[2] for _ in range(2)])

fresh("cap2", arguments={"x-max-length": 2, "x-overflow": "reject-publish"})
for i in range(4):
    ch.basic_publish("", "cap2", f"c{i}".encode())
print([ch.basic_get("cap2", auto_ack=True)[2] for _ in range(2)])
```

```text
[b'c2', b'c3']
[b'c0', b'c1']
```

With `drop-head` the newest two stayed; with `reject-publish` the first two stayed. A publisher without confirms is not told about the refusal; with confirms it receives a nack (module 05 in the [roadmap](../../overview.en.md)).

## Queue expiry: `x-expires`

A queue unused for the given number of milliseconds is deleted (no consumers, no redeclare, no `basic_get`):

```python
ch.queue_declare("idle", durable=True, arguments={"x-expires": 500})
time.sleep(1.0)
ch.queue_declare("idle", passive=True)     # ChannelClosedByBroker 404
```

```text
x-expires: ChannelClosedByBroker 404
```

## Exclusive and auto-delete queues

- **`exclusive=True`:** usable by the declaring connection only and deleted when it closes. Another connection's attempt gives `405 RESOURCE_LOCKED - cannot obtain exclusive access to locked queue …`.
- **`auto_delete=True`:** deleted when the last consumer cancels. After a consumer subscribed and cancelled, a passive declare returned `404`.

## Argument mismatch

Redeclaring a queue with arguments different from its current ones fails and closes the channel:

```text
406 PRECONDITION_FAILED - inequivalent arg 'x-message-ttl' for queue 'jobs' in vhost '/': received the value '1000' of type 'signedint' but current is none
```

To change a queue's arguments you must delete and recreate it (or use a policy for settings that policies cover, see the expert level).

## Best practices

- Always set a length or TTL limit on queues that can outgrow memory, and alert on depth.
- Prefer `prefetch_count` of a few messages for fast handlers and 1 for slow ones.
- Use exclusive server-named queues for per-instance subscriptions.
- Never change declare arguments in code without a migration plan.

## Common mistakes

- ❌ Expecting per-message TTL to expire messages behind a long-lived one → ✅ use a queue-level `x-message-ttl` when expiry must be prompt.
- ❌ `x-max-length` with `reject-publish` and publishers without confirms → ✅ confirms.
- ❌ Declaring the same queue in two services with different arguments → ✅ one owner of the declaration.

## Related materials

- Next: [expert theory](expert.en.md)
- Earlier: [02 · Exchanges and routing](../02-exchanges-and-routing/advanced.en.md)
- Next module: 04 · Messaging patterns ([roadmap](../../overview.en.md))

## Sources

- [Time-To-Live and Expiration](https://www.rabbitmq.com/docs/ttl)
- [Queue Length Limits](https://www.rabbitmq.com/docs/maxlength)
- [Queues](https://www.rabbitmq.com/docs/queues)
