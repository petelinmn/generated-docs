---
title: "Messaging patterns: basics"
lang: en
track: rabbitmq
kind: module
doc: basics
slug: 04-messaging-patterns
part: 1
tags: [rabbitmq, work-queue, publish-subscribe, routing, rpc, correlation-id, reply-to]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Messaging patterns: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Exchanges and routing](../02-exchanges-and-routing/basics.en.md), [03 · Queues and delivery](../03-queues-and-delivery/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

All snippets were run against a real RabbitMQ 4.4.1 in Docker with pika 1.5.0; outputs are copied from the runs. The helper `P()` opens a connection: `P = lambda: pika.Connection(pika.ConnectionParameters("localhost"))`; `ch` is the channel of the main connection. `basic_consume` handlers run on pika's worker thread, so no separate loop is needed: the examples just `time.sleep`.

The four classic patterns are built from the same bricks (exchange, queue, binding) and differ in **who divides the messages and who copies them**.

| Pattern | Exchange | Queues | Meaning |
|---|---|---|---|
| Work queue | default | one shared | each message goes to one worker |
| Publish/subscribe | fanout | one per subscriber | every subscriber gets every message |
| Routing | direct (or topic) | one per interest | the subscriber picks keys |
| RPC | default | request queue + reply queue | request and response tied by `correlation_id` |

## Work queue

One queue, several workers: the broker deals messages round-robin, and `prefetch_count=1` stops a slow worker from hoarding tasks ([module 03](../03-queues-and-delivery/basics.en.md#prefetch-and-fairness)). Test: ten tasks, worker `w1` spends 0.5 s per task, `w2` 0.05 s.

```python
ch.queue_declare("tasks", durable=True)

def worker(name, delay):
    c = P(); x = c.channel(); x.basic_qos(prefetch_count=1)
    def cb(chn, m, p, body):
        time.sleep(delay)                      # pretend to work
        done[name].append(body.decode())
        chn.basic_ack(m.delivery_tag)          # ack after the work
    x.basic_consume("tasks", cb)
    return c

w1 = worker("w1", 0.5); w2 = worker("w2", 0.05)
for i in range(10):
    ch.basic_publish("", "tasks", f"job{i}".encode(),
                     pika.BasicProperties(delivery_mode=pika.DeliveryMode.Persistent))
```

```text
work queue: {'w1': 1, 'w2': 9} total 10
```

The fast worker took nine of ten tasks because it became free more often: distribution follows readiness, not equality. Without `prefetch_count` the broker would have handed each worker half up front.

## Publish/subscribe

A fanout exchange copies every message to all bound queues. Each subscriber has its own **temporary queue**: `queue_declare("", exclusive=True)` asks the broker to invent the name, and the queue disappears with the connection.

```python
ch.exchange_declare("logs", "fanout")

def sub(name):
    c = P(); x = c.channel()
    q = x.queue_declare("", exclusive=True).method.queue   # server-named
    x.queue_bind(q, "logs")
    x.basic_consume(q, lambda chn, m, p, b: got[name].append(b.decode()), auto_ack=True)
    return c, q

for i in range(3):
    ch.basic_publish("logs", "", f"log{i}".encode())
```

```text
server-named queue: True
pub/sub: {'A': ['log0', 'log1', 'log2'], 'B': ['log0', 'log1', 'log2']}
late subscriber: ['log3']
```

A temporary queue name starts with `amq.gen-`. A subscriber that arrives later sees only new messages (`log3`), and a message published while **no** queue is bound is dropped: a fanout stores nothing.

## Routing by key

A direct exchange delivers a message to the queues whose binding key equals the routing key. One subscriber listens only for `error`, another for all three levels:

```python
ch.exchange_declare("sev", "direct")
# s1 binds "error"; s2 binds "info", "warning", "error"
for k in ("info", "warning", "error"):
    ch.basic_publish("sev", k, f"{k}-msg".encode())
```

```text
routing: {'errors': [('error', 'error-msg')], 'all': [('info', 'info-msg'), ('warning', 'warning-msg'), ('error', 'error-msg')]}
```

A queue may have several bindings, and a message lands in **every** matching queue. For hierarchical keys (`orders.eu.created`) use a topic exchange ([module 02](../02-exchanges-and-routing/basics.en.md)).

## RPC

Request-reply over queues: the client sends a request to the `rpc` queue and says in the properties **where** to reply (`reply_to`) and **how to recognise** the reply (`correlation_id`). The server replies to the queue from `reply_to` and copies `correlation_id`.

```python
def on_req(chn, m, p, body):                   # server
    r = str(fib(int(body))).encode()
    chn.basic_publish("", p.reply_to, r, pika.BasicProperties(correlation_id=p.correlation_id))
    chn.basic_ack(m.delivery_tag)

reply_q = cx.queue_declare("", exclusive=True).method.queue   # client
cx.basic_consume(reply_q, lambda chn, m, p, b: replies.__setitem__(p.correlation_id, b.decode()), auto_ack=True)
for n in (10, 20, 5):
    cid = str(uuid.uuid4())
    cx.basic_publish("", "rpc", str(n).encode(), pika.BasicProperties(reply_to=reply_q, correlation_id=cid))
```

```text
rpc: [(10, '55'), (20, '6765'), (5, '5')]
```

One reply queue serves many requests; `correlation_id` ties a reply to its request. If the server is not running, the request waits in the queue and no reply comes:

```text
no reply without server: False queued: 1
```

> RPC over a broker is convenient but not free: the client must set its own timeouts and match replies. See the [advanced theory](advanced.en.md).

## Common beginner mistakes

- ❌ One shared queue for several subscribers in pub/sub → ✅ one queue per subscriber (otherwise messages are divided, not copied).
- ❌ Publishing to a fanout before any subscriber exists → ✅ create queues and bindings first, then publish.
- ❌ Not copying `correlation_id` into the reply → ✅ the client cannot match the reply to the request.
- ❌ A work queue without prefetch and manual ack → ✅ `basic_qos(1)` and `basic_ack` after the work.
- ❌ A named queue for a temporary subscriber → ✅ `queue_declare("", exclusive=True)`.

## Related materials

- Next: [advanced theory](advanced.en.md) — direct reply-to, RPC timeouts, choosing a pattern
- Before: [03 · Queues and delivery](../03-queues-and-delivery/basics.en.md), [02 · Exchanges and routing](../02-exchanges-and-routing/basics.en.md)
- Next module: [05 · Reliability](../05-reliability/overview.en.md)
- Analogue in another track: [C# background jobs](../../../csharp/recipes/background-jobs/overview.en.md)

## Sources

- [RabbitMQ tutorials](https://www.rabbitmq.com/tutorials)
- [Direct Reply-to](https://www.rabbitmq.com/docs/direct-reply-to)
- [Exchanges](https://www.rabbitmq.com/docs/exchanges)
