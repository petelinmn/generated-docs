---
title: "Упражнения: шаблоны обмена сообщениями"
lang: ru
track: rabbitmq
kind: module
doc: exercises
slug: 04-messaging-patterns
part: 1
tags: [rabbitmq, work-queue, publish-subscribe, routing, rpc]
status: draft
updated: 2026-11-24
---

# Упражнения: шаблоны обмена сообщениями

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Решения запускались на настоящем RabbitMQ 4.4.1 с pika 1.5.0 с помощником `P()` из [базовой теории](basics.ru.md#как-проверялся-код). Удаляйте созданные очереди и обменники.

## 1. 🟢 Work queue

**Задача:** создайте очередь `tasks`, двух воркеров (`prefetch_count=1`; первый тратит 0,5 с на задачу, второй 0,05 с) и опубликуйте десять задач. Через 2 с выведите, сколько задач выполнил каждый.

**Ожидаемый результат** (точные числа могут чуть отличаться, но второй воркер делает почти всё):

```text
work queue: {'w1': 1, 'w2': 9} total 10
```

<details><summary>Подсказка</summary>

Подтверждайте сообщение после `time.sleep(delay)`.

</details>

<details><summary>Решение</summary>

См. код в [базовой теории](basics.ru.md#work-queue).

</details>

## 2. 🟢 Pub/sub

**Задача:** создайте fanout-обменник `logs`, двух подписчиков с временными очередями и опубликуйте три сообщения. Затем подключите третьего подписчика и опубликуйте ещё одно.

**Ожидаемый результат:** первые два получили `log0..log2`, поздний — только `['log3']`.

<details><summary>Подсказка</summary>

`queue_declare("", exclusive=True)`, затем `queue_bind(q, "logs")`.

</details>

<details><summary>Решение</summary>

См. код в [базовой теории](basics.ru.md#publishsubscribe).

</details>

## 3. 🟡 Маршрутизация

**Задача:** обменник `sev` типа direct; подписчик `errors` слушает только `error`, подписчик `all` — `info`, `warning`, `error`. Опубликуйте по одному сообщению на каждый уровень.

**Ожидаемый результат:** `errors` получил одно сообщение, `all` — три.

<details><summary>Подсказка</summary>

Одна очередь — несколько вызовов `queue_bind` с разными ключами.

</details>

<details><summary>Решение</summary>

См. код в [базовой теории](basics.ru.md#маршрутизация-по-ключам).

</details>

## 4. 🟡 RPC с correlation_id

**Задача:** реализуйте RPC-сервер (возвращает `fib(n)`) и клиента с временной очередью ответов. Отправьте запросы 10, 20 и 5 и сопоставьте ответы с запросами по `correlation_id`.

**Ожидаемый результат:**

```text
rpc: [(10, '55'), (20, '6765'), (5, '5')]
```

<details><summary>Подсказка</summary>

Сервер публикует ответ в `p.reply_to` и копирует `p.correlation_id`.

</details>

<details><summary>Решение</summary>

См. код в [базовой теории](basics.ru.md#rpc).

</details>

## 5. 🔴 RPC с таймаутом и expiration

**Задача:** напишите `call(body, timeout)` с direct reply-to, который бросает `TimeoutError`, если ответа нет, и ставит запросу `expiration`. Проверьте: (а) без сервера вызов падает по таймауту, а запрос исчезает из очереди; (б) после запуска сервера новый вызов получает ответ, а сервер обработал **только** его.

**Ожидаемый результат:**

```text
timeout: no reply in 0.3s
stale request still queued: 0
reply: cba
processed by server: ['abc']
```

<details><summary>Подсказка</summary>

Храните ожидающие запросы в словаре `correlation_id → (Event, список)`. `expiration` — строка в миллисекундах.

</details>

<details><summary>Решение</summary>

Класс `RpcClient` из [продвинутой теории](advanced.ru.md#rpc-клиент-с-таймаутом) с сервером, который отвечает перевёрнутым телом:

```python
def on_req(chn, m, p, body):
    processed.append(body.decode())
    chn.basic_publish("", p.reply_to, body[::-1], pika.BasicProperties(correlation_id=p.correlation_id))
    chn.basic_ack(m.delivery_tag)
sx.basic_consume("rpc", on_req)
print("reply:", cli.call(b"abc"))
```

</details>
