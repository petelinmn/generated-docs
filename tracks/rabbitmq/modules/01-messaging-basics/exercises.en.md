---
title: "Exercises: messaging and the AMQP model"
lang: en
track: rabbitmq
kind: module
doc: exercises
slug: 01-messaging-basics
part: 1
tags: [rabbitmq, amqp, queue, message-properties, pika]
status: draft
updated: 2026-11-24
---

# Exercises: messaging and the AMQP model

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The solutions ran against a real RabbitMQ 4.4.1 on `localhost:5672` with pika 1.5.0 (see "How the code was checked" in the [basic theory](basics.en.md)). Clean up the queues you create.

Common helper used by the solutions:

```python
import json, time
import pika

def open_channel():
    conn = pika.Connection(pika.ConnectionParameters("localhost"))
    return conn, conn.channel()
```

## 1. 🟢 send and recv

**Task:** write `send(ch, queue, text)` and `recv(ch, queue, timeout=1.0)`. `recv` polls with `basic_get(auto_ack=True)` and returns the text, or `None` if nothing arrives in time. Declare a durable queue `ex1`, send `a` and `b`, then receive three times (the third with `timeout=0.2`).

**Expected result:** `a b None`.

<details><summary>Hint</summary>

Publishing is asynchronous, so poll until a deadline instead of reading once.

</details>

<details><summary>Solution</summary>

```python
def send(ch, queue, text):
    ch.basic_publish(exchange="", routing_key=queue, body=text.encode())

def recv(ch, queue, timeout=1.0):
    deadline = time.monotonic() + timeout
    while True:
        method, _, body = ch.basic_get(queue, auto_ack=True)
        if method is not None:
            return body.decode()
        if time.monotonic() > deadline:
            return None
        time.sleep(0.05)
```

</details>

## 2. 🟢 Queue depth

**Task:** write `wait_depth(ch, queue, expected, timeout=2.0)` that reads the depth with a passive declare until it equals `expected` or the time runs out, and returns the last value. Send 5 messages to a queue `ex2`, print the depth; purge it with `queue_purge`, print the depth again.

**Expected result:** `5` then `0`.

<details><summary>Hint</summary>

The count is eventually consistent: loop.

</details>

<details><summary>Solution</summary>

```python
def wait_depth(ch, queue, expected, timeout=2.0):
    deadline = time.monotonic() + timeout
    while True:
        n = ch.queue_declare(queue, passive=True).method.message_count
        if n == expected or time.monotonic() > deadline:
            return n
        time.sleep(0.05)
```

</details>

## 3. 🟡 JSON with properties

**Task:** write `publish_json(ch, queue, obj, message_id)` that sends the object as JSON with `content_type="application/json"`, persistent delivery and the given `message_id`. Receive it and print the decoded object, content type, delivery mode and message id.

**Expected result:** `{'id': 7} application/json 2 m-1`.

<details><summary>Hint</summary>

`pika.BasicProperties(...)` and `pika.DeliveryMode.Persistent`.

</details>

<details><summary>Solution</summary>

```python
def publish_json(ch, queue, obj, message_id):
    ch.basic_publish("", queue, json.dumps(obj).encode(), pika.BasicProperties(
        content_type="application/json", delivery_mode=pika.DeliveryMode.Persistent, message_id=message_id))

# receive
m, p, b = ch.basic_get("ex3", auto_ack=True)
print(json.loads(b), p.content_type, p.delivery_mode, p.message_id)
```

</details>

## 4. 🟡 Does a queue exist?

**Task:** write `queue_exists(conn, name)` that checks with a passive declare on a **fresh** channel and returns `True` or `False` (asserting the code is 404). Then show that the connection and your working channel survive.

**Expected result:** `True False`, then `True` for `conn.is_open`.

<details><summary>Hint</summary>

Catch `pika.exceptions.ChannelClosedByBroker`; the probe channel dies, not yours.

</details>

<details><summary>Solution</summary>

```python
def queue_exists(conn, name):
    probe = conn.channel()
    try:
        probe.queue_declare(name, passive=True)
        return True
    except pika.exceptions.ChannelClosedByBroker as e:
        assert e.reply_code == 404
        return False
```

</details>

## 5. 🔴 Predict the failure

**Task:** on a fresh connection call `queue_declare("transient-q")` without arguments. Catch the exception, print the close code and the first line of the reason, then print whether the connection is closed.

**Expected result:**

```text
541 INTERNAL_ERROR - Feature `transient_nonexcl_queues` is deprecated.
True
```

<details><summary>Hint</summary>

In 4.4 this closes the whole connection: `ConnectionClosedByBroker`.

</details>

<details><summary>Solution</summary>

```python
c2, ch2 = open_channel()
try:
    ch2.queue_declare("transient-q")
except pika.exceptions.ConnectionClosedByBroker as e:
    print(e.reply_code, e.reply_text.splitlines()[0])
print(c2.is_closed)
```

</details>
