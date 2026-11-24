---
title: "Dead-lettering and retries: advanced"
lang: en
track: rabbitmq
kind: module
doc: advanced
slug: 06-dead-lettering-and-retries
part: 2
tags: [rabbitmq, retry, delayed-retry, x-death, parking-lot, poison-message, delivery-limit]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Dead-lettering and retries: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run on RabbitMQ 4.4.1 with pika 1.5.0.

## A delayed retry

Returning a failed message at once (`requeue=True`) hits the same fault in the same millisecond. A **delay** is needed. It is built from two queues and two DLXs:

```text
main --(reject, requeue=False)--> rx --> retry (TTL 300 ms) --(expired)--> default exchange --> main
```

```python
ch.exchange_declare("rx", "direct")
ch.queue_declare("main", durable=True, arguments={
    "x-dead-letter-exchange": "rx", "x-dead-letter-routing-key": "retry"})
ch.queue_declare("retry", durable=True, arguments={
    "x-message-ttl": 300,
    "x-dead-letter-exchange": "", "x-dead-letter-routing-key": "main"})
ch.queue_bind("retry", "rx", "retry")
ch.queue_declare("parking", durable=True)
```

`retry` has no consumers: a message sits there for `x-message-ttl` ms, "dies" by expiry and returns to `main` through the default exchange. The consumer counts attempts from `x-death` and after the third sends the message to a queue for manual inspection (`parking`):

```python
def on_msg(chn, m, p, b):
    deaths = p.headers.get("x-death", []) if p.headers else []
    n = sum(d["count"] for d in deaths if d["queue"] == "main")   # rejections so far
    attempts.append((time.perf_counter(), n))
    if n >= 3:
        chn.basic_publish("", "parking", b, pika.BasicProperties(headers={"attempts": n + 1}))
        chn.basic_ack(m.delivery_tag)
    else:
        chn.basic_reject(m.delivery_tag, requeue=False)
```

```text
attempts: [(0.04, 0), (0.35, 1), (0.65, 2), (0.95, 3)]
parked: 1
parking headers: {'attempts': 4}
```

Four attempts about 0.3 s apart (the TTL of `retry`): the counter `n` grows 0, 1, 2, 3, and on the third retry the message goes to `parking`.

### What accumulates in `x-death`

After two cycles the message has two records, each with its own `count`:

```text
{'count': 2, 'reason': 'expired', 'queue': 'retry', 'routing-keys': ['retry']}
{'count': 2, 'reason': 'rejected', 'queue': 'main', 'routing-keys': ['main']}
{'x-first-death-exchange': '', 'x-first-death-queue': 'main', 'x-first-death-reason': 'rejected', 'x-last-death-exchange': 'rx', 'x-last-death-queue': 'retry', 'x-last-death-reason': 'expired'}
```

A record is grouped by the pair (queue, reason) and `count` says how many times. The number of rejections in `main` is the `count` of the record with `queue == "main"` and `reason == "rejected"`. Summing all records of queue `main` mixes reasons, so in production code filter by `reason` too.

## A DLX with no route

If the named DLX does not exist or has no matching binding, the message **vanishes** with no error:

```text
orphan DLX depth: 0 open: True
```

The channel stayed open and the message is nowhere. So create the DLX and the inspection queue in advance and monitor the inspection queue's depth.

## A variable delay

The TTL of `retry` is the same for everyone. For growing delays (backoff) create several queues with different TTLs (for example `retry.1s`, `retry.10s`, `retry.60s`) and choose the routing key from the `x-death` counter. A per-message TTL (`expiration`) works only at the head of a queue ([module 03](../03-queues-and-delivery/advanced.en.md#message-ttl)), so it does not suit a delay queue with mixed values.

## Delivery limit of quorum queues

For quorum queues the documentation describes `x-delivery-limit` (and the policy key `delivery-limit`): once the number of deliveries exceeds it the message is dropped or dead-lettered. Test: a quorum queue with `x-delivery-limit: 3` and a DLX, and a consumer that loops on `nack(requeue=True)`.

```text
q1 deliveries: 140 dead-lettered: None None
```

The limit did not trigger: 140 deliveries in 2 s and nothing reached the inspection queue. The cause was found later ([module 07](../07-queue-types/advanced.en.md#delivery-limit)): **`basic_nack(requeue=True)` does not increase the delivery count, while `basic_reject(requeue=True)` and a channel crash do**. With `reject` the same test gives 4 deliveries and the message in the DLX with reason `delivery_limit`:

```text
C consumer+reject: deliveries 4 [None, 1, 2, 3] dead 1
```

> ⚠️ Verify: why `nack` is not counted was not established here (an observation, not a documented guarantee). To guard against poison messages use `reject` or `requeue=False` with a DLX.

## Best practices

- A DLX and an inspection queue (`parking`/`dead`) for every work queue.
- Unfixable errors (format, validation) go straight to the DLX; transient ones (network, lock) get a delayed retry.
- Count attempts from `x-death` (filter by queue **and** reason) or with your own header.
- After attempts run out, send to a manual inspection queue with the error context in headers.
- Monitor the depth of inspection queues and `retry`.
- The handler is idempotent: a retry may come after a partial success.

## Common mistakes

- ❌ A retry without a delay → ✅ a `retry` queue with a TTL.
- ❌ An endless main → retry → main cycle → ✅ an attempt limit and `parking`.
- ❌ Summing `count` over all records → ✅ filter by queue and reason.
- ❌ Relying on the delivery limit untested → ✅ test it on your version.
- ❌ A DLX with no queue → ✅ create the topology in advance.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [03 · Queues and delivery](../03-queues-and-delivery/advanced.en.md) — TTL and length limits
- [05 · Reliability](../05-reliability/advanced.en.md) — the idempotent consumer
- Recipe `task-queue-with-retries`: [roadmap](../../overview.en.md)

## Sources

- [Dead Letter Exchanges](https://www.rabbitmq.com/docs/dlx)
- [Quorum Queues: Poison Message Handling](https://www.rabbitmq.com/docs/quorum-queues)
- [Time-To-Live and Expiration](https://www.rabbitmq.com/docs/ttl)
