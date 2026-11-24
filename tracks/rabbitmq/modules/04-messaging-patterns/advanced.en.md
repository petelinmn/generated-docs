---
title: "Messaging patterns: advanced"
lang: en
track: rabbitmq
kind: module
doc: advanced
slug: 04-messaging-patterns
part: 1
tags: [rabbitmq, rpc, direct-reply-to, timeout, request-expiration, competing-consumers]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Messaging patterns: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run on RabbitMQ 4.4.1 with pika 1.5.0.

## Direct reply-to

Instead of creating a reply queue per client you can use the pseudo-queue `amq.rabbitmq.reply-to`: the client consumes from it and names it in `reply_to`; the broker substitutes a unique address itself and delivers the reply to the same channel, bypassing real queues.

```python
cx.basic_consume("amq.rabbitmq.reply-to", lambda chn, m, p, b: out.append((p.correlation_id, b.decode())), auto_ack=True)
cx.basic_publish("", "rpc", b"12", pika.BasicProperties(reply_to="amq.rabbitmq.reply-to", correlation_id="d1"))
```

```text
direct reply-to: [('d1', '144')]
```

Rules confirmed by running:

- The consumer must use `auto_ack=True`: with manual acks the broker closes the channel — `406 PRECONDITION_FAILED - reply consumer cannot acknowledge`.
- You must consume **before** publishing a request with that `reply_to`: publishing without a consumer closed the channel (`is_open` became `False`).
- The reply arrives only on the same channel that sent the request. If the client dies the reply is lost: there is no queue where it could wait.

> ⚠️ Verify: the last statement comes from the documentation and was not verified by running here.

## An RPC client with a timeout

The broker does not know you are waiting for a reply. If the server is down, a client without a timeout hangs. The client below keeps pending requests by `correlation_id`, waits on an event with a timeout and sets the request's `expiration` equal to the timeout:

```python
class RpcClient:
    def __init__(self):
        self.c = P(); self.ch = self.c.channel(); self.pending = {}
        self.ch.basic_consume("amq.rabbitmq.reply-to", self._on_reply, auto_ack=True)

    def _on_reply(self, ch, m, p, body):
        slot = self.pending.pop(p.correlation_id, None)   # unknown id -> ignored
        if slot: slot[1].append(body.decode()); slot[0].set()

    def call(self, body, timeout=1.0):
        cid = str(uuid.uuid4()); ev = threading.Event(); box = []
        self.pending[cid] = (ev, box)
        self.ch.basic_publish("", "rpc", body, pika.BasicProperties(
            reply_to="amq.rabbitmq.reply-to", correlation_id=cid,
            expiration=str(int(timeout * 1000))))          # stale requests expire
        if not ev.wait(timeout):
            self.pending.pop(cid, None)
            raise TimeoutError(f"no reply in {timeout}s")
        return box[0]
```

```text
timeout: no reply in 0.3s
stale request still queued: 0
reply: cba
processed by server: ['abc']
timeout: no reply in 0.3s
pending after late reply: 0
```

Line by line:

1. No server: the client got a `TimeoutError`.
2. `expiration` removed the stale request from the queue: when a server appeared it processed only the new request (`processed by server: ['abc']`), not the stale one.
3. A slow server replied after the timeout: the client had already removed the `correlation_id` from `pending`, so the late reply is silently dropped.

Without `expiration` a request from a long-gone client would wait for the server and be executed for nothing — and dangerous if the request changes state.

## Choosing a pattern

| Task | Pattern | Why |
|---|---|---|
| Spread heavy work | work queue | each message to one worker |
| Notify everyone | pub/sub (fanout) | every subscriber gets its own copy |
| Notify the interested | direct / topic | the subscriber picks keys |
| Get a result | RPC | the reply is tied to the request |
| No result needed | work queue or pub/sub | RPC is overkill, couples services in time |

RPC turns an asynchronous system into a synchronous one: the client waits, so its availability depends on the server. If the answer is not needed immediately, send a command and receive the result as a separate event.

## Best practices

- Work queue: a durable queue, persistent messages, manual ack after the work, `prefetch_count` matched to task duration.
- Pub/sub: temporary exclusive queues for "live" subscribers and named durable queues for those that must not miss messages while disconnected.
- RPC: a client timeout, `expiration` on the request, a `correlation_id` check, an idempotent server.
- One exchange per event type and bindings on the subscriber side: the publisher does not know its consumers.

## Common mistakes

- ❌ RPC without a timeout → ✅ a timeout and `expiration`.
- ❌ Consuming `amq.rabbitmq.reply-to` with manual ack → ✅ `auto_ack=True`.
- ❌ Publishing a request before consuming reply-to → ✅ consume first.
- ❌ A temporary queue for a subscriber that must miss nothing → ✅ a named durable queue.
- ❌ Assuming a reply belongs to the request without checking `correlation_id` → ✅ compare ids.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [03 · Queues and delivery](../03-queues-and-delivery/advanced.en.md) — message TTL and expiration
- Next module: [05 · Reliability](../05-reliability/overview.en.md)

## Sources

- [Direct Reply-to](https://www.rabbitmq.com/docs/direct-reply-to)
- [Time-To-Live and Expiration](https://www.rabbitmq.com/docs/ttl)
- [RabbitMQ tutorial: Remote procedure call](https://www.rabbitmq.com/tutorials/tutorial-six-python)
