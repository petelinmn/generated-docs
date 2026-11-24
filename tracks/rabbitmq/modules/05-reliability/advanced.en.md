---
title: "Reliability: advanced"
lang: en
track: rabbitmq
kind: module
doc: advanced
slug: 05-reliability
part: 2
tags: [rabbitmq, publisher-confirms, nack, transactions, idempotency, message-id, throughput]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Reliability: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md) (the `Confirmed` helper). The code was run on RabbitMQ 4.4.1 with pika 1.5.0.

## A nack from the broker

The broker may answer `nack`: for example a queue with `x-overflow: reject-publish` is full ([module 03](../03-queues-and-delivery/advanced.en.md#length-limits-and-overflow)). It tells the publisher to retry later or give up:

```python
ch.queue_declare("lim", durable=True, arguments={"x-max-length": 1, "x-overflow": "reject-publish"})
print("first:", pub.publish("", "lim", b"1"), "second:", pub.publish("", "lim", b"2"))
```

```text
first: ack second: nack
```

Without confirms the publisher would never learn about the rejection.

## Errors that produce no confirm

Publishing to a nonexistent exchange closes the channel (`404 NOT_FOUND`) and no confirm arrives at all:

```text
missing exchange: timeout channel open: False
```

So waiting for a confirm must always have a timeout, and after a channel closes you must open a new one and enable confirms again. Every unconfirmed publish is **unknown**: it may or may not have arrived.

## Throughput

Waiting for a confirm after each message is expensive. Three ways to publish 300 messages (Docker Desktop on Windows; numbers depend on the environment, the ratios matter):

```text
300 one-by-one confirmed: 13.43s
300 pipelined confirms: 0.048s acked=300
300 no confirms (fire-and-forget): 0.005s
```

The pipelined variant sends everything and counts confirms in the callback (`multiple=True` confirms a batch at once):

```python
acked, ev, lock = [0], threading.Event(), threading.Lock()
def on_confirm(frame):
    with lock:
        if isinstance(frame.method, pika.spec.Basic.Ack):
            acked[0] += (frame.method.delivery_tag - acked[0]) if frame.method.multiple else 1
        if acked[0] >= N: ev.set()
ch2.confirm_delivery(on_confirm)
for i in range(N): ch2.basic_publish("", "rel", b"x")
ev.wait(10)
```

Almost 300 times faster, but this counter only handles `ack`: production code must keep a set of unconfirmed tags (from `on_publish`) and handle `nack` and connection loss, republishing the unconfirmed ones.

## Transactions

AMQP has transactions (`tx_select`, `tx_commit`, `tx_rollback`), but the new `pika.Connection` **does not have them**: its channel has no `tx_*` methods. They exist only in the deprecated `BlockingConnection`. Verified:

```text
tx before commit: 0
after rollback: 0
after commit: 1
100 tx commits: 4.50s
```

Messages are invisible until `tx_commit`, and `tx_rollback` discards them. But committing every publish is costly (100 commits take 4.5 s against 0.05 s of pipelined confirms for 300 messages), so use publisher confirms and keep transactions for rare all-or-nothing cases over several messages.

## Idempotent consumer

Duplicates are unavoidable. If a message has a `message_id`, the consumer remembers handled ones and skips repeats:

```python
for i in range(3):
    ch.basic_publish("", "idem", b"pay", pika.BasicProperties(message_id="order-42"))
seen, processed = set(), []
def cb(chn, m, p, b):
    if p.message_id not in seen:
        processed.append(p.message_id); seen.add(p.message_id)   # do the work here
    chn.basic_ack(m.delivery_tag)
```

```text
processed: ['order-42']
```

Three copies, one processing. In memory this works only while the process lives; in production store the ids in a database together with the result of the work **in one transaction** (or make the operation itself idempotent: "set status", not "increment a counter").

## Best practices

- Confirms on every business-critical publisher; a wait timeout.
- Batched or asynchronous sending instead of "one → wait".
- `mandatory` when a route may be missing.
- Republish the unconfirmed after a disconnect; the consumer is idempotent.
- Persistent + durable for data that must not be lost.

## Common mistakes

- ❌ Waiting for a confirm after every message at high volume → ✅ a pipeline with tags.
- ❌ Waiting for a confirm with no timeout → ✅ a timeout and reopening the channel.
- ❌ Transactions instead of confirms → ✅ confirms.
- ❌ Idempotency only in process memory → ✅ in the database, together with the result.
- ❌ Republishing without a `message_id` → ✅ otherwise duplicates cannot be recognised.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- Next module: [06 · Dead-lettering and retries](../06-dead-lettering-and-retries/overview.en.md)

## Sources

- [Consumer Acknowledgements and Publisher Confirms](https://www.rabbitmq.com/docs/confirms)
- [Publishers](https://www.rabbitmq.com/docs/publishers)
- [pika: publisher confirms](https://pika.readthedocs.io/en/stable/)
