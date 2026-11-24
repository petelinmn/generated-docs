---
title: "Упражнения: очереди и доставка"
lang: ru
track: rabbitmq
kind: module
doc: exercises
slug: 03-queues-and-delivery
part: 1
tags: [rabbitmq, queue, acknowledgement, message-ttl, queue-length-limit, policy]
status: draft
updated: 2026-11-24
---

# Упражнения: очереди и доставка

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Решения запускались на настоящем RabbitMQ 4.4.1 с pika 1.5.0 с помощниками `ch`, `count` и `fresh` из [базовой теории](basics.ru.md#как-проверялся-код). Удаляйте созданные очереди.

## 1. 🟢 Ack, nack, reject

**Задача:** опубликуйте `m0`, `m1`, `m2` в новую durable-очередь `jobs` и заберите все три через `basic_get`. Выведите число готовых. Подтвердите первое, отклоните второе с возвратом, третье — без возврата. Выведите число готовых, а также тело и флаг `redelivered` следующего полученного сообщения.

**Ожидаемый результат:**

```text
0
1
b'm1' True
```

<details><summary>Подсказка</summary>

Полученные, но неподтверждённые сообщения не считаются готовыми.

</details>

<details><summary>Решение</summary>

См. код в [базовой теории](basics.ru.md#ack-nack-reject).

</details>

## 2. 🟢 Durable — не persistent

**Задача:** объявите durable-очередь `dur`, опубликуйте одно transient- и одно persistent-сообщение, перезапустите брокер (`docker restart rabbit`), затем выведите глубину и уцелевшее тело.

**Ожидаемый результат:** `2` до перезапуска, `1` и `[b'persistent']` после.

<details><summary>Подсказка</summary>

`pika.BasicProperties(delivery_mode=pika.DeliveryMode.Persistent)`.

</details>

<details><summary>Решение</summary>

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

## 3. 🟡 Чей TTL победит?

**Задача:** (а) `x-message-ttl` очереди 5000 и `expiration="200"` сообщения; (б) TTL очереди 200 и expiration сообщения 5000; (в) очередь без TTL: опубликуйте сообщение на 5000 мс, затем на 200 мс. Через 0,8 с выведите три счётчика.

**Ожидаемый результат:** `0 0 2`.

<details><summary>Подсказка</summary>

Действует меньший TTL, но TTL сообщения применяется только у головы.

</details>

<details><summary>Решение</summary>

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

**Задача:** две очереди с `x-max-length: 2`, одна с overflow по умолчанию, другая с `reject-publish`. Опубликуйте в каждую `c0`…`c3` и прочитайте по два сообщения.

**Ожидаемый результат:**

```text
[b'c2', b'c3']
[b'c0', b'c1']
```

<details><summary>Подсказка</summary>

`drop-head` оставляет новейшие, `reject-publish` — самые старые.

</details>

<details><summary>Решение</summary>

См. код в [продвинутой теории](advanced.ru.md#ограничения-длины-и-overflow).

</details>

## 5. 🔴 Политика вместо кода

**Задача:** командой `rabbitmqctl set_policy cap-pol "^pol\." '{"max-length":2}' --apply-to queues` создайте политику, объявите очередь `pol.q` **без** аргументов, опубликуйте четыре сообщения и выведите глубину. Затем удалите политику (`rabbitmqctl clear_policy cap-pol`).

**Ожидаемый результат:** `2`.

<details><summary>Подсказка</summary>

Запускайте `rabbitmqctl` внутри контейнера: `docker exec rabbit rabbitmqctl …`. Ключи политики без префикса `x-`.

</details>

<details><summary>Решение</summary>

```python
ch.queue_declare("pol.q", durable=True)
for i in range(4):
    ch.basic_publish("", "pol.q", str(i).encode())
time.sleep(0.5)
print(ch.queue_declare("pol.q", passive=True).method.message_count)
ch.queue_delete("pol.q")
```

</details>
