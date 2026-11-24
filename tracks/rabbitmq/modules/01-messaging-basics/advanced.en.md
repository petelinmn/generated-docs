---
title: "Messaging and the AMQP model: advanced"
lang: en
track: rabbitmq
kind: module
doc: advanced
slug: 01-messaging-basics
part: 1
tags: [rabbitmq, amqp, message-properties, connection-parameters, heartbeat, virtual-host]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Messaging and the AMQP model: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). All outputs come from a real RabbitMQ 4.4.1 broker with pika 1.5.0.

## Message properties

A message has a body (bytes the broker never looks at) and **properties**, standard metadata carried with it:

```python
props = pika.BasicProperties(
    content_type="application/json", content_encoding="utf-8",
    delivery_mode=pika.DeliveryMode.Persistent, message_id="m-1", correlation_id="c-1",
    reply_to="replies", type="order.created", app_id="shop", timestamp=1700000000,
    priority=3, headers={"retries": 2, "src": "web"})
ch.basic_publish("", "props", json.dumps({"id": 7}).encode(), props)
```

What the consumer received:

```text
b'{"id": 7}'
{'content_type': 'application/json', 'content_encoding': 'utf-8', 'delivery_mode': 2, 'message_id': 'm-1', 'correlation_id': 'c-1', 'reply_to': 'replies', 'type': 'order.created', 'app_id': 'shop', 'timestamp': 1700000000, 'priority': 3, 'headers': {'retries': 2, 'src': 'web'}, 'expiration': None, 'user_id': None}
```

| Property | Use |
|---|---|
| `content_type`, `content_encoding` | how to decode the body (`application/json`) |
| `delivery_mode` | `1` transient, `2` persistent ([module 03](../03-queues-and-delivery/overview.en.md)) |
| `message_id` | an identifier for deduplication and tracing |
| `correlation_id`, `reply_to` | request/response ([module 04](../04-messaging-patterns/overview.en.md)) |
| `type`, `app_id` | the kind of event, the sender |
| `timestamp` | seconds since the epoch, set by the producer |
| `priority` | for priority queues |
| `expiration` | a per-message TTL in milliseconds, **as a string** |
| `headers` | a free-form table; used by headers exchanges |
| `user_id` | must equal the connecting user, else the broker refuses |

The broker treats the body as opaque. Agree on a format (JSON with `content_type`, or Protobuf/Avro) and put a version in `type` or a header so consumers can evolve.

`pika` accepts a `str` body and sends its text; an `int` raises `TypeError: basic_publish body must be bytes or str, not int`. Prefer explicit `bytes`.

## Connection parameters

```python
params = pika.ConnectionParameters(
    "localhost", virtual_host="/",
    credentials=pika.PlainCredentials("guest", "guest"), heartbeat=30)
conn = pika.Connection(params)
```

- **`virtual_host`** — a namespace of exchanges, queues and permissions; one broker can host many. The default is `/`.
- **`heartbeat`** — the interval, in seconds, at which each side proves it is alive; a dead peer is detected after missing heartbeats. The broker's default is 60 s.
- Failures to connect surface as different exceptions:

```text
bad login: ProbableAuthenticationError ConnectionClosedByBroker: (403) 'ACCESS_REFUSED - Login was refused using authentication mechanism P…
no broker: AMQPConnectionError
bad vhost: ProbableAccessDeniedError ConnectionClosedByBroker: (530) 'NOT_ALLOWED - vhost nope not found' (host='localhost', port=5672)
```

Authentication failure (403) and a missing virtual host (530) are different problems and different exceptions; an unreachable port is an `AMQPConnectionError`.

## Errors are asynchronous

`basic_publish` does not wait for the broker. A mistake is reported later, by closing the channel:

```python
ch2.basic_publish("", "uid", b"x", pika.BasicProperties(user_id="someone"))   # connected as guest
time.sleep(0.5)
print("ch2 closed:", ch2.is_closed)
```

```text
ch2 closed: True
```

The call itself returned normally. Do not treat "no exception" as "accepted": watch the channel close callbacks, use **publisher confirms** ([module 05](../05-reliability/overview.en.md), and for undeliverable messages the `mandatory` flag ([module 02](../02-exchanges-and-routing/overview.en.md)).

## Naming and server-generated queues

- Names are strings up to 255 bytes (AMQP short strings); use dotted lowercase names (`orders.created`).
- An empty name asks the broker to **generate** one: `queue_declare("", exclusive=True)` returned a name starting with `amq.gen`. Such queues are for private reply or fan-out subscriptions and vanish with the connection.
- Names starting with `amq.` are reserved for the broker.

## Threads and channels

`pika.Connection` is thread-safe: several threads may call `basic_publish` at once, and consumer callbacks run on a worker thread per channel, one message at a time, in order. A slow callback does not stall heartbeats (the old `BlockingConnection` had that problem). Still keep a channel to one purpose, because a channel-level error kills everything on it.

> ⚠️ Verify: automatic reconnection and consumer recovery for `pika.Connection` were not examined; assume you must reconnect and re-declare topology yourself after a connection loss.

## Best practices

- One connection per process; separate connections for publishing and consuming if back pressure on one must not block the other.
- Declare topology (exchanges, queues, bindings) at start-up in one place, idempotently.
- Set `content_type`, `message_id` and `timestamp` on every message.
- Handle channel and connection close callbacks; reconnect with a back-off.

## Common mistakes

- ❌ Assuming a publish with no exception was accepted → ✅ confirms and close callbacks.
- ❌ Putting `expiration` as a number → ✅ a string of milliseconds.
- ❌ Hard-coding credentials → ✅ environment variables.
- ❌ Sharing a channel between producers of unrelated traffic → ✅ one purpose per channel.

## Related materials

- Next: [expert theory](expert.en.md)
- Next module: [02 · Exchanges and routing](../02-exchanges-and-routing/advanced.en.md)

## Sources

- [AMQP 0-9-1 Model Explained](https://www.rabbitmq.com/tutorials/amqp-concepts)
- [Connections](https://www.rabbitmq.com/docs/connections)
- [Heartbeats](https://www.rabbitmq.com/docs/heartbeats)
- [Virtual Hosts](https://www.rabbitmq.com/docs/vhosts)
