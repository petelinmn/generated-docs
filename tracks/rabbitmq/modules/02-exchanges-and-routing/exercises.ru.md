---
title: "Упражнения: обменники и маршрутизация"
lang: ru
track: rabbitmq
kind: module
doc: exercises
slug: 02-exchanges-and-routing
part: 1
tags: [rabbitmq, exchange, routing-key, topic-exchange, alternate-exchange]
status: draft
updated: 2026-11-24
---

# Упражнения: обменники и маршрутизация

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Решения запускались на настоящем RabbitMQ 4.4.1 с pika 1.5.0 с помощниками `drain` и `setup` из [базовой теории](basics.ru.md#как-проверялся-код). Удаляйте созданные обменники.

## 1. 🟢 Предскажите совпадения topic

**Задача:** объявите topic-обменник `t` с четырьмя очередями, привязанными с `*.orange.*`, `*.*.rabbit`, `lazy.#` и `#`. Опубликуйте `quick.orange.rabbit`, `lazy.orange.elephant`, `lazy`, `quick.brown.fox`, `lazy.pink.rabbit`. **До запуска** запишите, что получит каждая очередь, затем сравните.

**Ожидаемый результат:**

```text
topic *.orange.* -> ['quick.orange.rabbit', 'lazy.orange.elephant']
topic *.*.rabbit -> ['quick.orange.rabbit', 'lazy.pink.rabbit']
topic lazy.# -> ['lazy.orange.elephant', 'lazy', 'lazy.pink.rabbit']
topic # -> ['quick.orange.rabbit', 'lazy.orange.elephant', 'lazy', 'quick.brown.fox', 'lazy.pink.rabbit']
```

<details><summary>Подсказка</summary>

`*` — ровно одно слово, `#` — ноль или больше.

</details>

<details><summary>Решение</summary>

```python
pats = ["*.orange.*", "*.*.rabbit", "lazy.#", "#"]
qs = setup("t", "topic", pats)
for k in ["quick.orange.rabbit", "lazy.orange.elephant", "lazy", "quick.brown.fox", "lazy.pink.rabbit"]:
    ch.basic_publish("t", k, k.encode())
for p, q in zip(pats, qs):
    print("topic", p, "->", drain(q))
```

</details>

## 2. 🟢 Direct с общим ключом

**Задача:** direct-обменник `d` с тремя очередями, привязанными с `info`, `error`, `error`. Опубликуйте `info`, `error`, `warn` и выведите, что получила каждая очередь.

**Ожидаемый результат:** `[['info'], ['error'], ['error']]`.

<details><summary>Подсказка</summary>

Обе очереди `error` получают сообщение; `warn` ни с чем не совпадает.

</details>

<details><summary>Решение</summary>

```python
qs = setup("d", "direct", ["info", "error", "error"])
for k in ("info", "error", "warn"):
    ch.basic_publish("d", k, k.encode())
print([drain(q) for q in qs])
```

</details>

## 3. 🟡 Headers: all или any

**Задача:** headers-обменник `h` с двумя очередями: `x-match: all` с `format=pdf, type=report` и `x-match: any` с теми же заголовками. Опубликуйте сообщения с заголовками `{format: pdf, type: report}`, `{format: pdf}`, `{format: zip}`. Выведите, что получила каждая очередь.

**Ожидаемый результат:**

```text
headers all: ["{'format': 'pdf', 'type': 'report'}"] any: ["{'format': 'pdf', 'type': 'report'}", "{'format': 'pdf'}"]
```

<details><summary>Подсказка</summary>

Заголовки передавайте через `pika.BasicProperties(headers=...)`; привязывайте через `arguments`.

</details>

<details><summary>Решение</summary>

```python
args = [{"x-match": "all", "format": "pdf", "type": "report"},
        {"x-match": "any", "format": "pdf", "type": "report"}]
qs = setup("h", "headers", args)
for hd in ({"format": "pdf", "type": "report"}, {"format": "pdf"}, {"format": "zip"}):
    ch.basic_publish("h", "", str(hd).encode(), pika.BasicProperties(headers=hd))
print("headers all:", drain(qs[0]), "any:", drain(qs[1]))
```

</details>

## 4. 🟡 Вернуть немаршрутизируемое сообщение

**Задача:** опубликуйте в direct-обменник `d` с ключом `nokey` и `mandatory=True`, собирая возвращённые сообщения обработчиком. Выведите собранное.

**Ожидаемый результат:** `[(312, 'NO_ROUTE', 'nokey', b'x')]`.

<details><summary>Подсказка</summary>

`ch.add_on_return_callback(lambda c, m, p, b: …)`, затем немного подождите.

</details>

<details><summary>Решение</summary>

```python
returns = []
ch.add_on_return_callback(lambda c, m, p, b: returns.append((m.reply_code, m.reply_text, m.routing_key, b)))
ch.basic_publish("d", "nokey", b"x", mandatory=True)
time.sleep(0.5)
print(returns)
```

</details>

## 5. 🔴 Alternate exchange и неудачное повторное объявление

**Задача:** (а) постройте `main` (direct, альтернативный обменник `unrouted`) с одной очередью, привязанной с `ok`; опубликуйте `ok` и `bad`; выведите, что получили основная и альтернативная очереди. (б) На другом соединении повторно объявите `d` как `fanout` и выведите ошибку.

**Ожидаемый результат:**

```text
main: ['routed'] alt: ['unrouted']
redeclare: 406 PRECONDITION_FAILED - inequivalent arg 'type' for exchange 'd' in vhost '/': received 'fanout' but current is 'direct'
```

<details><summary>Подсказка</summary>

`arguments={"alternate-exchange": "unrouted"}`; ловите `pika.exceptions.ChannelClosedByBroker`.

</details>

<details><summary>Решение</summary>

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
