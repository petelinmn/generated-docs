---
title: "Messaging and the AMQP model: expert"
lang: en
track: rabbitmq
kind: module
doc: expert
slug: 01-messaging-basics
part: 1
tags: [rabbitmq, amqp, delivery-tag, channel-max, frame-max, heartbeat, default-exchange]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Messaging and the AMQP model: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. Values in "Observed defaults" were read from the live broker (`rabbitmqctl eval 'application:get_env(rabbit, Key).'`); the protocol parts come from the AMQP 0-9-1 model description.

## Reference

### Observed defaults (RabbitMQ 4.4.1, Docker image)

| Setting | Value |
|---|---|
| `max_message_size` | 16 777 216 bytes (16 MiB) |
| `channel_max` | 256 |
| `frame_max` | 131 072 bytes |
| `heartbeat` | 60 s |
| `consumer_timeout` | 1 800 000 ms (30 min) |
| `default_vhost` / `default_user` | `/` / `guest` (with the `administrator` tag) |

### Listeners in the image

| Port | Protocol |
|---|---|
| 5672 | AMQP 0-9-1 and AMQP 1.0 |
| 15672 | HTTP API and management UI |
| 15692 | Prometheus exporter |
| 25672 | inter-node and CLI communication |

### Built-in exchanges (`rabbitmqctl list_exchanges name type`)

| Name | Type |
|---|---|
| (empty — the default exchange) | direct |
| `amq.direct` | direct |
| `amq.fanout` | fanout |
| `amq.headers`, `amq.match` | headers |
| `amq.topic` | topic |
| `amq.rabbitmq.log`, `amq.rabbitmq.trace` | topic |

### Protocol classes

AMQP 0-9-1 groups operations into classes: `connection`, `channel`, `exchange`, `queue`, `basic` (publish, get, consume, ack, reject, nack, qos, return), `confirm` (publisher confirms), `tx` (transactions). Client methods map to them: `queue_declare` → `queue.declare`, `basic_publish` → `basic.publish` (a method frame, a content header frame and body frames).

### Close codes seen so far

| Code | Name | Closes |
|---|---|---|
| 403 | `ACCESS_REFUSED` | the connection (login) |
| 404 | `NOT_FOUND` | the channel |
| 405 | `RESOURCE_LOCKED` | the channel |
| 406 | `PRECONDITION_FAILED` | the channel |
| 530 | `NOT_ALLOWED` | the connection (bad vhost) |
| 541 | `INTERNAL_ERROR` | the connection (transient queue refused) |

## Internals

- **Delivery tags** are per channel, start at 1 and grow; an acknowledgement refers to a tag on the same channel. In a test, acking a tag on a different channel closed that other channel (the original one stayed open).
- **A queue is a process** inside the Erlang runtime; publishing routes to the queue's process, which explains why counts lag a publish.
- **Frames.** Everything is sent in frames up to `frame_max`; a large body is split into body frames. In a test, a body of 16 MiB + 1 byte closed the channel (the connection stayed open).
- **Heartbeats** run at the transport level: the effective interval is negotiated between client and broker at connection start.
- **Channels multiplex** one TCP connection; up to `channel_max` per connection (256 here).

## Gotchas

### 1. `queue_declare` without `durable` on 4.4

The connection (not just the channel) is closed with 541. Old tutorials break.

### 2. A passive declare kills the channel

A passive declare of a missing queue returns 404 and closes the channel: a poor existence probe on a working channel.

### 3. A publish to a missing queue succeeds

Through the default exchange it is silently dropped, with no error.

### 4. The count lags

`message_count` read right after a publish returned `1 1 2` for three publishes; after a second, `3`.

### 5. `expiration` is a string

A per-message TTL goes into `BasicProperties(expiration="200")`, a string of milliseconds.

### 6. `user_id` is validated

A `user_id` different from the connecting user closes the channel, asynchronously.

### 7. `delivery_mode` reads back as an integer

`delivery_mode=pika.DeliveryMode.Persistent` came back as `2`.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| `basic_consume` | push, low latency | needs prefetch tuning ([module 03](../03-queues-and-delivery/expert.en.md)) |
| `basic_get` | simple | polls, one round trip per message |
| One channel per thread | isolated errors | more channels |
| One shared channel | fewer resources | one error kills all |
| RabbitMQ vs a log system (Kafka) | flexible routing, per-message ack | no replay by default (streams add it) |

## Interview questions

<details><summary>1. What is the path of a message in AMQP 0-9-1?</summary>

Producer → exchange → (bindings, routing key) → queue → consumer.

→ [Basic theory](basics.en.md#the-amqp-model)

</details>

<details><summary>2. Why do channels exist if there are connections?</summary>

A TCP connection is expensive; channels multiplex many logical sessions over one connection.

→ [Basic theory](basics.en.md#connection-and-channel)

</details>

<details><summary>3. Trick: you publish to the default exchange with a routing key naming a queue that does not exist. What happens?</summary>

Nothing is raised: the message is dropped, because no queue matches.

→ [Gotcha 3](#3-a-publish-to-a-missing-queue-succeeds)

</details>

<details><summary>4. What happens when a consumer dies without acking?</summary>

The broker requeues the message and marks it `redelivered`.

→ [Basic theory](basics.en.md#acknowledgements)

</details>

<details><summary>5. Trick: what closes on a failed passive declare — the channel or the connection?</summary>

The channel (404); the connection stays open.

→ [Gotcha 2](#2-a-passive-declare-kills-the-channel)

</details>

<details><summary>6. Why does `queue_declare("q")` fail on 4.4?</summary>

Transient non-exclusive queues are refused; the connection closes with 541. Use `durable=True` or `exclusive=True`.

→ [Basic theory](basics.en.md#queues-must-be-durable-or-exclusive)

</details>

<details><summary>7. How does `basic_get` differ from `basic_consume`?</summary>

`basic_get` polls one message per call; `basic_consume` subscribes and the broker pushes messages.

→ [Basic theory](basics.en.md#receiving-get-and-consume)

</details>

<details><summary>8. Trick: why can `message_count` after a publish be wrong?</summary>

Publishing is asynchronous and the queue updates its count later.

→ [Gotcha 4](#4-the-count-lags)

</details>

<details><summary>9. What is a virtual host?</summary>

A namespace for exchanges, queues and permissions on one broker; default `/`.

→ [Advanced theory](advanced.en.md#connection-parameters)

</details>

<details><summary>10. How are authentication and vhost errors told apart?</summary>

By the close code: 403 `ACCESS_REFUSED` (login) versus 530 `NOT_ALLOWED` (vhost not found); pika raises different exceptions.

→ [Advanced theory](advanced.en.md#connection-parameters)

</details>

<details><summary>11. Trick: a publish raised no exception. Was it accepted?</summary>

Not necessarily: errors arrive asynchronously by closing the channel; use confirms.

→ [Advanced theory](advanced.en.md#errors-are-asynchronous)

</details>

<details><summary>12. What is a delivery tag, and where is it valid?</summary>

A per-channel counter identifying a delivery; acks must use it on the same channel.

→ [Reference](#internals)

</details>

## Cheat sheet

```text
conn = pika.Connection(pika.ConnectionParameters(host, virtual_host="/", credentials=..., heartbeat=30))
ch = conn.channel()
ch.queue_declare(q, durable=True)            # 4.4: durable or exclusive
ch.basic_publish(exchange="", routing_key=q, body=b"...", properties=pika.BasicProperties(...))
ch.basic_consume(q, callback)  /  ch.basic_get(q)
callback(ch, method, props, body): ch.basic_ack(method.delivery_tag)
errors: 403/530/541 close the connection; 404/405/406 close the channel
```

- Ack after processing.
- Properties on every message.
- One purpose per channel.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Next module: [02 · Exchanges and routing](../02-exchanges-and-routing/expert.en.md)

## Sources

- [AMQP 0-9-1 Model Explained](https://www.rabbitmq.com/tutorials/amqp-concepts)
- [AMQP 0-9-1 Complete Reference](https://www.rabbitmq.com/amqp-0-9-1-reference)
- [Configuration](https://www.rabbitmq.com/docs/configure)
- [Networking](https://www.rabbitmq.com/docs/networking)
