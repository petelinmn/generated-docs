---
title: "Exercises: exchanges and routing"
lang: en
track: rabbitmq
kind: module
doc: exercises
slug: 02-exchanges-and-routing
part: 1
tags: [rabbitmq, exchange, routing-key, topic-exchange, alternate-exchange]
status: draft
updated: 2026-11-24
---

# Exercises: exchanges and routing

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The solutions ran against a real RabbitMQ 4.4.1 with pika 1.5.0 using the helpers `drain` and `setup` from the [basic theory](basics.en.md#how-the-code-was-checked). Delete the exchanges you create.

## 1. 🟢 Predict the topic matches

**Task:** declare a topic exchange `t` with four queues bound with `*.orange.*`, `*.*.rabbit`, `lazy.#` and `#`. Publish `quick.orange.rabbit`, `lazy.orange.elephant`, `lazy`, `quick.brown.fox`, `lazy.pink.rabbit`. **Before running**, write down what each queue receives, then compare.

**Expected result:**

```text
topic *.orange.* -> ['quick.orange.rabbit', 'lazy.orange.elephant']
topic *.*.rabbit -> ['quick.orange.rabbit', 'lazy.pink.rabbit']
topic lazy.# -> ['lazy.orange.elephant', 'lazy', 'lazy.pink.rabbit']
topic # -> ['quick.orange.rabbit', 'lazy.orange.elephant', 'lazy', 'quick.brown.fox', 'lazy.pink.rabbit']
```

<details><summary>Hint</summary>

`*` is exactly one word, `#` is zero or more.

</details>

<details><summary>Solution</summary>

```python
pats = ["*.orange.*", "*.*.rabbit", "lazy.#", "#"]
qs = setup("t", "topic", pats)
for k in ["quick.orange.rabbit", "lazy.orange.elephant", "lazy", "quick.brown.fox", "lazy.pink.rabbit"]:
    ch.basic_publish("t", k, k.encode())
for p, q in zip(pats, qs):
    print("topic", p, "->", drain(q))
```

</details>

## 2. 🟢 Direct with a shared key

**Task:** a direct exchange `d` with three queues bound with `info`, `error`, `error`. Publish `info`, `error`, `warn` and print what each queue got.

**Expected result:** `[['info'], ['error'], ['error']]`.

<details><summary>Hint</summary>

Both `error` queues receive it; `warn` matches nothing.

</details>

<details><summary>Solution</summary>

```python
qs = setup("d", "direct", ["info", "error", "error"])
for k in ("info", "error", "warn"):
    ch.basic_publish("d", k, k.encode())
print([drain(q) for q in qs])
```

</details>

## 3. 🟡 Headers: all or any

**Task:** a headers exchange `h` with two queues: `x-match: all` with `format=pdf, type=report`, and `x-match: any` with the same headers. Publish messages with headers `{format: pdf, type: report}`, `{format: pdf}`, `{format: zip}`. Print what each queue got.

**Expected result:**

```text
headers all: ["{'format': 'pdf', 'type': 'report'}"] any: ["{'format': 'pdf', 'type': 'report'}", "{'format': 'pdf'}"]
```

<details><summary>Hint</summary>

Pass headers with `pika.BasicProperties(headers=...)`; bind with `arguments`.

</details>

<details><summary>Solution</summary>

```python
args = [{"x-match": "all", "format": "pdf", "type": "report"},
        {"x-match": "any", "format": "pdf", "type": "report"}]
qs = setup("h", "headers", args)
for hd in ({"format": "pdf", "type": "report"}, {"format": "pdf"}, {"format": "zip"}):
    ch.basic_publish("h", "", str(hd).encode(), pika.BasicProperties(headers=hd))
print("headers all:", drain(qs[0]), "any:", drain(qs[1]))
```

</details>

## 4. 🟡 Get the unroutable message back

**Task:** publish to the direct exchange `d` with key `nokey` and `mandatory=True`, collecting returned messages through a callback. Print what you collected.

**Expected result:** `[(312, 'NO_ROUTE', 'nokey', b'x')]`.

<details><summary>Hint</summary>

`ch.add_on_return_callback(lambda c, m, p, b: …)`, then wait a moment.

</details>

<details><summary>Solution</summary>

```python
returns = []
ch.add_on_return_callback(lambda c, m, p, b: returns.append((m.reply_code, m.reply_text, m.routing_key, b)))
ch.basic_publish("d", "nokey", b"x", mandatory=True)
time.sleep(0.5)
print(returns)
```

</details>

## 5. 🔴 Alternate exchange and a failed redeclare

**Task:** (a) build `main` (direct, alternate exchange `unrouted`) with one queue bound with `ok`; publish `ok` and `bad`; print what the main queue and the alternate queue got. (b) On another connection redeclare `d` as `fanout` and print the error.

**Expected result:**

```text
main: ['routed'] alt: ['unrouted']
redeclare: 406 PRECONDITION_FAILED - inequivalent arg 'type' for exchange 'd' in vhost '/': received 'fanout' but current is 'direct'
```

<details><summary>Hint</summary>

`arguments={"alternate-exchange": "unrouted"}`; catch `pika.exceptions.ChannelClosedByBroker`.

</details>

<details><summary>Solution</summary>

```python
ch.exchange_declare("unrouted", exchange_type="fanout")
uq = ch.queue_declare("", exclusive=True).method.queue
ch.queue_bind(uq, "unrouted")
ch.exchange_declare("main", exchange_type="direct", arguments={"alternate-exchange": "unrouted"})
mq = ch.queue_declare("", exclusive=True).method.queue
ch.queue_bind(mq, "main", routing_key="ok")
ch.basic_publish("main", "ok", b"routed")
ch.basic_publish("main", "bad", b"unrouted")
print("main:", drain(mq), "alt:", drain(uq))

c2 = pika.Connection(pika.ConnectionParameters("localhost"))
try:
    c2.channel().exchange_declare("d", exchange_type="fanout")
except pika.exceptions.ChannelClosedByBroker as e:
    print("redeclare:", e.reply_code, e.reply_text)
c2.close()
```

</details>
