---
title: "Exercises: queues and delivery"
lang: en
track: rabbitmq
kind: module
doc: exercises
slug: 03-queues-and-delivery
part: 1
tags: [rabbitmq, queue, acknowledgement, message-ttl, queue-length-limit, policy]
status: draft
updated: 2026-11-24
---

# Exercises: queues and delivery

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The solutions ran against a real RabbitMQ 4.4.1 with pika 1.5.0 using the helpers `ch`, `count` and `fresh` from the [basic theory](basics.en.md#how-the-code-was-checked). Delete the queues you create.

## 1. 🟢 Ack, nack, reject

**Task:** publish `m0`, `m1`, `m2` to a fresh durable queue `jobs` and fetch all three with `basic_get`. Print the ready count. Ack the first, nack the second with requeue, reject the third without requeue. Print the ready count and the body and `redelivered` flag of the message you get next.

**Expected result:**

```text
0
1
b'm1' True
```

<details><summary>Hint</summary>

Fetched-but-unacked messages are not ready.

</details>

<details><summary>Solution</summary>

See the code in the [basic theory](basics.en.md#ack-nack-reject).

</details>

## 2. 🟢 Durable is not persistent

**Task:** declare a durable queue `dur`, publish one transient and one persistent message, restart the broker (`docker restart rabbit`), then print the depth and the surviving body.

**Expected result:** `2` before the restart, `1` and `[b'persistent']` after.

<details><summary>Hint</summary>

`pika.BasicProperties(delivery_mode=pika.DeliveryMode.Persistent)`.

</details>

<details><summary>Solution</summary>

```python
ch.queue_declare("dur", durable=True)
ch.basic_publish("", "dur", b"transient")
ch.basic_publish("", "dur", b"persistent", pika.BasicProperties(delivery_mode=pika.DeliveryMode.Persistent))
time.sleep(0.5)
print("before restart:", ch.queue_declare("dur", passive=True).method.message_count)
# docker restart rabbit; reconnect, then:
print("after restart:", ch.queue_declare("dur", passive=True).method.message_count)
print([ch.basic_get("dur", auto_ack=True)[2] for _ in range(1)])
```

</details>

## 3. 🟡 Which TTL wins?

**Task:** (a) queue `x-message-ttl` 5000 and a message `expiration="200"`; (b) queue TTL 200 and message expiration 5000; (c) a queue without TTL: publish a 5000 ms message, then a 200 ms message. After 0.8 s print the three counts.

**Expected result:** `0 0 2`.

<details><summary>Hint</summary>

The smaller TTL applies, but a per-message TTL is only enforced at the head.

</details>

<details><summary>Solution</summary>

```python
def cnt(q): time.sleep(0.8); return ch.queue_declare(q, passive=True).method.message_count
ch.queue_declare("q-long", durable=True, arguments={"x-message-ttl": 5000})
ch.basic_publish("", "q-long", b"x", pika.BasicProperties(expiration="200"))
ch.queue_declare("q-short", durable=True, arguments={"x-message-ttl": 200})
ch.basic_publish("", "q-short", b"x", pika.BasicProperties(expiration="5000"))
ch.queue_declare("q-head", durable=True)
ch.basic_publish("", "q-head", b"long", pika.BasicProperties(expiration="5000"))
ch.basic_publish("", "q-head", b"short", pika.BasicProperties(expiration="200"))
print(cnt("q-long"), cnt("q-short"), cnt("q-head"))
```

</details>

## 4. 🟡 Overflow

**Task:** two queues with `x-max-length: 2`, one with the default overflow and one with `reject-publish`. Publish `c0`…`c3` to each and read two messages from each.

**Expected result:**

```text
[b'c2', b'c3']
[b'c0', b'c1']
```

<details><summary>Hint</summary>

`drop-head` keeps the newest, `reject-publish` keeps the oldest.

</details>

<details><summary>Solution</summary>

See the code in the [advanced theory](advanced.en.md#length-limits-and-overflow).

</details>

## 5. 🔴 A policy instead of code

**Task:** with `rabbitmqctl set_policy cap-pol "^pol\." '{"max-length":2}' --apply-to queues`, declare a queue `pol.q` **without** arguments, publish four messages and print the depth. Then remove the policy (`rabbitmqctl clear_policy cap-pol`).

**Expected result:** `2`.

<details><summary>Hint</summary>

Run `rabbitmqctl` inside the container: `docker exec rabbit rabbitmqctl …`. Policy keys have no `x-` prefix.

</details>

<details><summary>Solution</summary>

```python
ch.queue_declare("pol.q", durable=True)
for i in range(4):
    ch.basic_publish("", "pol.q", str(i).encode())
time.sleep(0.5)
print(ch.queue_declare("pol.q", passive=True).method.message_count)
ch.queue_delete("pol.q")
```

</details>
