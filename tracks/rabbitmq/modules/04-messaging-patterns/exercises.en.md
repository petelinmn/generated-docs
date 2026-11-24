---
title: "Exercises: messaging patterns"
lang: en
track: rabbitmq
kind: module
doc: exercises
slug: 04-messaging-patterns
part: 1
tags: [rabbitmq, work-queue, publish-subscribe, routing, rpc]
status: draft
updated: 2026-11-24
---

# Exercises: messaging patterns

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The solutions were run against a real RabbitMQ 4.4.1 with pika 1.5.0 using the helper `P()` from the [basic theory](basics.en.md#how-the-code-was-verified). Delete the queues and exchanges you create.

## 1. 🟢 Work queue

**Task:** create a queue `tasks`, two workers (`prefetch_count=1`; the first spends 0.5 s per task, the second 0.05 s) and publish ten tasks. After 2 s print how many tasks each completed.

**Expected output** (exact numbers may differ slightly, but the second worker does almost everything):

```text
work queue: {'w1': 1, 'w2': 9} total 10
```

<details><summary>Hint</summary>

Ack the message after `time.sleep(delay)`.

</details>

<details><summary>Solution</summary>

See the code in the [basic theory](basics.en.md#work-queue).

</details>

## 2. 🟢 Pub/sub

**Task:** create a fanout exchange `logs`, two subscribers with temporary queues and publish three messages. Then attach a third subscriber and publish one more.

**Expected output:** the first two received `log0..log2`, the late one only `['log3']`.

<details><summary>Hint</summary>

`queue_declare("", exclusive=True)`, then `queue_bind(q, "logs")`.

</details>

<details><summary>Solution</summary>

See the code in the [basic theory](basics.en.md#publishsubscribe).

</details>

## 3. 🟡 Routing

**Task:** a direct exchange `sev`; subscriber `errors` listens only for `error`, subscriber `all` for `info`, `warning`, `error`. Publish one message per level.

**Expected output:** `errors` got one message, `all` three.

<details><summary>Hint</summary>

One queue — several `queue_bind` calls with different keys.

</details>

<details><summary>Solution</summary>

See the code in the [basic theory](basics.en.md#routing-by-key).

</details>

## 4. 🟡 RPC with correlation_id

**Task:** implement an RPC server (returns `fib(n)`) and a client with a temporary reply queue. Send requests 10, 20 and 5 and match replies to requests by `correlation_id`.

**Expected output:**

```text
rpc: [(10, '55'), (20, '6765'), (5, '5')]
```

<details><summary>Hint</summary>

The server publishes the reply to `p.reply_to` and copies `p.correlation_id`.

</details>

<details><summary>Solution</summary>

See the code in the [basic theory](basics.en.md#rpc).

</details>

## 5. 🔴 RPC with a timeout and expiration

**Task:** write `call(body, timeout)` using direct reply-to that raises `TimeoutError` when there is no reply and sets `expiration` on the request. Check: (a) with no server the call fails by timeout and the request disappears from the queue; (b) after the server starts a new call gets a reply and the server processed **only** it.

**Expected output:**

```text
timeout: no reply in 0.3s
stale request still queued: 0
reply: cba
processed by server: ['abc']
```

<details><summary>Hint</summary>

Keep pending requests in a dictionary `correlation_id → (Event, list)`. `expiration` is a string in milliseconds.

</details>

<details><summary>Solution</summary>

The `RpcClient` class from the [advanced theory](advanced.en.md#an-rpc-client-with-a-timeout) with a server that replies with the reversed body:

```python
def on_req(chn, m, p, body):
    processed.append(body.decode())
    chn.basic_publish("", p.reply_to, body[::-1], pika.BasicProperties(correlation_id=p.correlation_id))
    chn.basic_ack(m.delivery_tag)
sx.basic_consume("rpc", on_req)
print("reply:", cli.call(b"abc"))
```

</details>
