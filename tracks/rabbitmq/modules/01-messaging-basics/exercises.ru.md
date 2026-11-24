---
title: "Упражнения: обмен сообщениями и модель AMQP"
lang: ru
track: rabbitmq
kind: module
doc: exercises
slug: 01-messaging-basics
part: 1
tags: [rabbitmq, amqp, queue, message-properties, pika]
status: draft
updated: 2026-11-24
---

# Упражнения: обмен сообщениями и модель AMQP

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Решения запускались на настоящем RabbitMQ 4.4.1 на `localhost:5672` с pika 1.5.0 (см. «Как проверялся код» в [базовой теории](basics.ru.md)). Удаляйте созданные очереди.

Общий помощник, используемый решениями:

```python
import json, time
import pika

def open_channel():
    conn = pika.Connection(pika.ConnectionParameters("localhost"))
    return conn, conn.channel()
```

## 1. 🟢 send и recv

**Задача:** напишите `send(ch, queue, text)` и `recv(ch, queue, timeout=1.0)`. `recv` опрашивает `basic_get(auto_ack=True)` и возвращает текст или `None`, если за время ничего не пришло. Объявите durable-очередь `ex1`, отправьте `a` и `b`, затем получите три раза (третий — с `timeout=0.2`).

**Ожидаемый результат:** `a b None`.

<details><summary>Подсказка</summary>

Публикация асинхронна, поэтому опрашивайте до дедлайна, а не читайте один раз.

</details>

<details><summary>Решение</summary>

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

## 2. 🟢 Глубина очереди

**Задача:** напишите `wait_depth(ch, queue, expected, timeout=2.0)`, которая читает глубину пассивным объявлением, пока она не станет равна `expected` или не истечёт время, и возвращает последнее значение. Отправьте 5 сообщений в очередь `ex2`, выведите глубину; очистите её `queue_purge`, снова выведите глубину.

**Ожидаемый результат:** `5`, затем `0`.

<details><summary>Подсказка</summary>

Счётчик согласуется не сразу: крутите цикл.

</details>

<details><summary>Решение</summary>

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

## 3. 🟡 JSON со свойствами

**Задача:** напишите `publish_json(ch, queue, obj, message_id)`, которая отправляет объект как JSON с `content_type="application/json"`, persistent-доставкой и заданным `message_id`. Получите его и выведите декодированный объект, тип содержимого, режим доставки и идентификатор сообщения.

**Ожидаемый результат:** `{'id': 7} application/json 2 m-1`.

<details><summary>Подсказка</summary>

`pika.BasicProperties(...)` и `pika.DeliveryMode.Persistent`.

</details>

<details><summary>Решение</summary>

```python
def publish_json(ch, queue, obj, message_id):
    ch.basic_publish("", queue, json.dumps(obj).encode(), pika.BasicProperties(
        content_type="application/json", delivery_mode=pika.DeliveryMode.Persistent, message_id=message_id))

# receive
m, p, b = ch.basic_get("ex3", auto_ack=True)
print(json.loads(b), p.content_type, p.delivery_mode, p.message_id)
```

</details>

## 4. 🟡 Существует ли очередь?

**Задача:** напишите `queue_exists(conn, name)`, которая проверяет пассивным объявлением на **новом** канале и возвращает `True` или `False` (проверяя, что код — 404). Затем покажите, что соединение и ваш рабочий канал живы.

**Ожидаемый результат:** `True False`, затем `True` для `conn.is_open`.

<details><summary>Подсказка</summary>

Ловите `pika.exceptions.ChannelClosedByBroker`; умирает пробный канал, а не ваш.

</details>

<details><summary>Решение</summary>

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

## 5. 🔴 Предскажите сбой

**Задача:** на новом соединении вызовите `queue_declare("transient-q")` без аргументов. Поймайте исключение, выведите код закрытия и первую строку причины, затем выведите, закрыто ли соединение.

**Ожидаемый результат:**

```text
541 INTERNAL_ERROR - Feature `transient_nonexcl_queues` is deprecated.
True
```

<details><summary>Подсказка</summary>

В 4.4 это закрывает всё соединение: `ConnectionClosedByBroker`.

</details>

<details><summary>Решение</summary>

```python
c2, ch2 = open_channel()
try:
    ch2.queue_declare("transient-q")
except pika.exceptions.ConnectionClosedByBroker as e:
    print(e.reply_code, e.reply_text.splitlines()[0])
print(c2.is_closed)
```

</details>
