---
title: "Упражнения: типы очередей"
lang: ru
track: rabbitmq
kind: module
doc: exercises
slug: 07-queue-types
part: 2
tags: [rabbitmq, queue-types, quorum-queue, stream, delivery-limit]
status: draft
updated: 2026-11-24
---

# Упражнения: типы очередей

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Решения запускались на одном узле RabbitMQ 4.4.1 с pika 1.5.0. Удаляйте созданные очереди.

## 1. 🟢 Три типа

**Задача:** объявите по одной durable-очереди каждого типа (`qc`, `qq`, `qs`) и выведите их типы командой `docker exec rabbit rabbitmqctl list_queues name type`.

**Ожидаемый результат:** `qc classic`, `qq quorum`, `qs stream`.

<details><summary>Подсказка</summary>

`arguments={"x-queue-type": "quorum"}` и `"stream"`; classic — без аргумента.

</details>

<details><summary>Решение</summary>

См. код в [основах](basics.ru.md#три-типа-очередей).

</details>

## 2. 🟢 Ограничения

**Задача:** попробуйте объявить quorum-очередь (а) с `exclusive=True`, (б) с `x-max-priority: 5`, (в) с `durable=False`. Запишите код и тип ошибки для каждого случая; каждую попытку делайте на новом соединении.

**Ожидаемый результат:** (а) `406` (канал), (б) `406` (канал), (в) `541` (соединение).

<details><summary>Подсказка</summary>

Ошибка уровня канала закрывает канал, ошибка `541` — всё соединение.

</details>

<details><summary>Решение</summary>

См. таблицу в [основах](basics.ru.md#ограничения-quorum-очередей-и-stream) и функцию `try_declare`: она открывает отдельное соединение, объявляет очередь и печатает `type(e).__name__` и начало текста ошибки.

</details>

## 3. 🟡 Чтение stream

**Задача:** опубликуйте `e0…e4` в stream, прочитайте с `first`, затем подпишите второго потребителя с `next` и опубликуйте `e5`. Затем прочитайте журнал заново с начала.

**Ожидаемый результат:**

```text
stream first: ['e0', 'e1', 'e2', 'e3', 'e4']
stream next: ['e5']
stream re-read: ['e0', 'e1', 'e2', 'e3', 'e4', 'e5']
```

<details><summary>Подсказка</summary>

Нужны `basic_qos(prefetch_count=10)` и ручной `basic_ack`.

</details>

<details><summary>Решение</summary>

См. код в [основах](basics.ru.md#stream-журнал-вместо-очереди).

</details>

## 4. 🟡 Смещения

**Задача:** опубликуйте шесть сообщений и прочитайте stream со смещениями `first`, `last`, `next` и `3`. Объясните результат для `last`.

**Ожидаемый результат:** `first` и `last` — все шесть, `next` — пусто, `3` — `e3, e4, e5`.

<details><summary>Подсказка</summary>

`last` начинает с последнего чанка.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#смещения-в-stream).

</details>

## 5. 🔴 Delivery limit: reject против nack

**Задача:** создайте quorum-очередь с `x-delivery-limit: 3` и DLX и опубликуйте сообщение. Для каждого из двух вариантов (`basic_reject(requeue=True)` и `basic_nack(requeue=True)`) через `basic_get` верните сообщение до 8 раз и выведите число доставок и глубину очереди разбора.

**Ожидаемый результат:** для `reject` 4 доставки и 1 сообщение в очереди разбора; для `nack` 8 доставок (остановили сами) и 0.

<details><summary>Подсказка</summary>

Между попытками `time.sleep(0.15)`; после каждой серии пересоздавайте очереди.

</details>

<details><summary>Решение</summary>

Выводы из проверки: `basic_get+reject(requeue): [(0, None, False), (1, 1, True), (2, 2, True), (3, 3, True)]`, `dead: 1`; `A get+nack: deliveries 8 dead 0`. Код — в [продвинутой теории](advanced.ru.md#delivery-limit).

</details>
