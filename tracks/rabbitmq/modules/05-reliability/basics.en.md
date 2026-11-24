---
title: "Reliability: basics"
lang: en
track: rabbitmq
kind: module
doc: basics
slug: 05-reliability
part: 2
tags: [rabbitmq, publisher-confirms, mandatory, basic-return, persistence, at-least-once]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Reliability: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [03 · Queues and delivery](../03-queues-and-delivery/basics.en.md), [04 · Messaging patterns](../04-messaging-patterns/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

All snippets were run against a real RabbitMQ 4.4.1 in Docker with pika 1.5.0; outputs are copied from the runs. In the new `pika.Connection`, `confirm_delivery` takes a **callback** and `basic_publish` returns nothing: publishing is asynchronous. So the code below uses a small helper that turns the broker's verdict into a return value:

```python
class Confirmed:
    """Publish and wait for the broker's verdict: 'ack', 'nack' or 'timeout'."""
    def __init__(self, ch):
        self.ch, self.events, self.verdict = ch, {}, {}
        self.returned, self.lock = [], threading.Lock()
        ch.confirm_delivery(self._on_confirm)
        ch.add_on_return_callback(lambda c, m, p, b: self.returned.append((m.reply_code, m.reply_text)))

    def _on_confirm(self, frame):
        m = frame.method
        kind = "ack" if isinstance(m, pika.spec.Basic.Ack) else "nack"
        with self.lock:
            tags = [t for t in self.events if t <= m.delivery_tag] if m.multiple else [m.delivery_tag]
            for t in tags:
                self.verdict[t] = kind
                if t in self.events: self.events[t].set()

    def publish(self, ex, rk, body, props=None, mandatory=False, timeout=5):
        tagbox, ev = [], threading.Event()
        def on_pub(tag):                       # runs on the IOLoop thread
            with self.lock: self.events[tag] = ev
            tagbox.append(tag)
        self.ch.basic_publish(ex, rk, body, props, mandatory, on_publish=on_pub)
        if not ev.wait(timeout): return "timeout"
        v = self.verdict.pop(tagbox[0]); del self.events[tagbox[0]]
        return v
```

`pub = Confirmed(ch)`; `ch` is the channel of the main connection; the queue `rel` is durable.

## Where a message can be lost

| Stretch | What protects it |
|---|---|
| publisher → broker | publisher confirms |
| broker, no route | `mandatory` + Basic.Return |
| broker → disk | durable queue + `delivery_mode=2` ([module 03](../03-queues-and-delivery/basics.en.md#durable-and-persistent)) |
| broker → consumer | manual ack after the work ([module 03](../03-queues-and-delivery/basics.en.md#ack-nack-reject)) |
| consumer processes twice | idempotency |

Reliability is a chain: the weakest link breaks all of it.

## Publisher confirms

After `confirm_delivery` the broker confirms each publish: `ack` means "accepted" (for a persistent message in a durable queue, written to disk), `nack` means "could not".

```python
t = time.perf_counter()
print("confirmed publish:", pub.publish("", "rel", b"a", pika.BasicProperties(delivery_mode=2), mandatory=True),
      "ms:", round((time.perf_counter() - t) * 1000, 1))
```

```text
confirmed publish: ack ms: 46.4
```

The time depends on the machine and Docker; what matters is that without a confirm the publisher does not know whether the message arrived. A confirm means "the broker took responsibility", not "a consumer processed it".

> ⚠️ Verify: the claim that for a persistent message in a durable queue the `ack` arrives after the disk write comes from the documentation and was not verified here.

## Mandatory and Basic.Return

If a message has no route, the broker by default silently drops it — **and still confirms** the publish. With `mandatory=True` it first returns the message (`Basic.Return`, code 312) and then confirms:

```python
ch.exchange_declare("d", "direct")
r = pub.publish("d", "nokey", b"x", mandatory=True)
print("unroutable mandatory:", r, pub.returned)
pub.returned.clear()
r = pub.publish("d", "nokey", b"x")
print("unroutable no mandatory:", r, pub.returned)
```

```text
unroutable mandatory: ack [(312, 'NO_ROUTE')]
unroutable no mandatory: ack []
```

So an `ack` does not prove the message reached a queue. If routing matters, use `mandatory` and a return handler ([module 02](../02-exchanges-and-routing/basics.en.md)).

## What "at least once" guarantees

Confirms plus consumer acks give **at-least-once** delivery: the message will not be lost but may arrive twice (the publisher retried because it got no confirm; the consumer crashed after the work but before the ack). The defence against duplicates is an idempotent consumer ([advanced theory](advanced.en.md#idempotent-consumer)).

## Common beginner mistakes

- ❌ Treating `basic_publish` without confirms as reliable → ✅ enable confirms.
- ❌ Treating `ack` as proof of routing → ✅ `mandatory`.
- ❌ A persistent message in a non-durable queue (on 4.4 you cannot create one unless it is exclusive) → ✅ durable + persistent.
- ❌ Acking before the work in the consumer → ✅ after.
- ❌ Assuming "exactly once" delivery → ✅ "at least once" plus idempotency.

## Related materials

- Next: [advanced theory](advanced.en.md) — throughput, nack, transactions, idempotency
- Before: [04 · Messaging patterns](../04-messaging-patterns/basics.en.md), [03 · Queues and delivery](../03-queues-and-delivery/basics.en.md)
- Next module: [06 · Dead-lettering and retries](../06-dead-lettering-and-retries/overview.en.md)

## Sources

- [Consumer Acknowledgements and Publisher Confirms](https://www.rabbitmq.com/docs/confirms)
- [Reliability Guide](https://www.rabbitmq.com/docs/reliability)
- [Publishers](https://www.rabbitmq.com/docs/publishers)
