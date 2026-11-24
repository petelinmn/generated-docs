---
title: "Queues and delivery: basics"
lang: en
track: rabbitmq
kind: module
doc: basics
slug: 03-queues-and-delivery
part: 1
tags: [rabbitmq, queue, acknowledgement, prefetch, durability, redelivery, persistent-message]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Queues and delivery: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · Messaging and the AMQP model](../01-messaging-basics/basics.en.md), [02 · Exchanges and routing](../02-exchanges-and-routing/basics.en.md) · **Time:** ~1.5 h

## How the code was checked

All code ran against a real RabbitMQ 4.4.1 in Docker with pika 1.5.0; outputs are copied from the runs. Helpers used below: `ch` is a channel, `count(q)` waits 0.3 s and returns the number of ready messages of `q` by a passive declare, `fresh(q, **kw)` deletes and redeclares a durable queue.

## Ack, nack, reject

For every delivery the consumer says what happened:

- `basic_ack(tag)` — done; the broker forgets the message.
- `basic_nack(tag, requeue=…)` — failed (may refuse many at once with `multiple=True`).
- `basic_reject(tag, requeue=…)` — failed (one message).

With `requeue=True` the message goes back to the queue; with `requeue=False` it is dropped (or dead-lettered if the queue has a DLX).

```python
fresh("jobs")
for i in range(3):
    ch.basic_publish("", "jobs", f"m{i}".encode())
m1, _, b1 = ch.basic_get("jobs")
m2, _, b2 = ch.basic_get("jobs")
m3, _, b3 = ch.basic_get("jobs")
print("ready while three are unacked:", count("jobs"))
ch.basic_ack(m1.delivery_tag)
ch.basic_nack(m2.delivery_tag, requeue=True)
ch.basic_reject(m3.delivery_tag, requeue=False)
print("after ack / nack(requeue) / reject(drop):", count("jobs"))
m, _, b = ch.basic_get("jobs")
print("requeued message:", b, "redelivered:", m.redelivered)
ch.basic_ack(m.delivery_tag)
```

```text
ready while three are unacked: 0
after ack / nack(requeue) / reject(drop): 1
requeued message: b'm1' redelivered: True
```

Unacknowledged messages are not "ready": the consumer owns them. The three messages were `m0`, `m1`, `m2`: `m0` was acked (gone), `m1` was nacked with requeue (back in the queue, `redelivered: True`), `m2` was rejected without requeue (dropped).

## A dead consumer returns its messages

If the channel or connection closes with messages unacknowledged, the broker puts them back:

```python
ch.basic_publish("", "jobs", b"crash")
c2 = pika.Connection(pika.ConnectionParameters("localhost"))
mm, _, _ = c2.channel().basic_get("jobs")      # takes it, never acks
c2.close()
time.sleep(0.5)
m, _, b = ch.basic_get("jobs")
print("after consumer died:", b, "redelivered:", m.redelivered)
```

```text
after consumer died: b'crash' redelivered: True
```

This is the basis of at-least-once processing: a message may be processed twice, so handlers must tolerate duplicates.

## Prefetch and fairness

By default the broker pushes as many messages as it can. `basic_qos(prefetch_count=N)` limits the number of **unacknowledged** messages per consumer (per channel). Test: six messages, two consumers, consumer A never acknowledges, B acknowledges at once, `prefetch_count=1`:

```text
prefetch=1: {'A': ['t0'], 'B': ['t1', 't2', 't3', 't4', 't5']}
```

A took one message and was then skipped because it held its quota; B did the rest. With no limit:

```text
prefetch=0 (unlimited): {'A': ['t0', 't1', 't2', 't3', 't4', 't5'], 'B': []}
```

A subscribed first and was handed everything at once, then sat on it. Set a small prefetch (often 1 for slow tasks, higher for fast ones) so work spreads.

## Durable and persistent

A broker restart (tested with `docker restart`) wipes what is not persisted:

| What | Survives a restart |
|---|---|
| a non-durable queue | no (and 4.4 will not create one unless it is exclusive) |
| a durable queue | the queue itself, yes |
| a message in it | only if persistent (`delivery_mode=2`) |

```python
ch.queue_declare("dur", durable=True)
ch.basic_publish("", "dur", b"transient")
ch.basic_publish("", "dur", b"persistent", pika.BasicProperties(delivery_mode=pika.DeliveryMode.Persistent))
# docker restart rabbit
```

```text
before restart: 2
after restart: 1
[b'persistent']
```

"Persistent" is not a guarantee of safety by itself: the message may still be lost between the publish and the broker writing it. Publisher confirms close that gap ([module 05](../05-reliability/overview.en.md).

## Beginner mistakes

- ❌ A durable queue with transient messages → ✅ `delivery_mode=2` as well.
- ❌ No prefetch with slow tasks → ✅ `basic_qos(prefetch_count=1)`.
- ❌ Forgetting to ack → ✅ unacked messages pile up and return on disconnect.
- ❌ Requeueing a poison message forever → ✅ `requeue=False` plus a DLX ([module 06](../06-dead-lettering-and-retries/overview.en.md).
- ❌ Acking before the work is done → ✅ ack last.

## Related materials

- Next: [advanced theory](advanced.en.md) — TTL, length limits, expiry
- Earlier: [02 · Exchanges and routing](../02-exchanges-and-routing/basics.en.md)
- Next module: [04 · Messaging patterns](../04-messaging-patterns/overview.en.md)

## Sources

- [Consumer Acknowledgements and Publisher Confirms](https://www.rabbitmq.com/docs/confirms)
- [Consumer Prefetch](https://www.rabbitmq.com/docs/consumer-prefetch)
- [Queues](https://www.rabbitmq.com/docs/queues)
- [Reliability Guide](https://www.rabbitmq.com/docs/reliability)
