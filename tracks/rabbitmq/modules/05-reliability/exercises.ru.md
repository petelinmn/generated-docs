---
title: "Упражнения: надёжность"
lang: ru
track: rabbitmq
kind: module
doc: exercises
slug: 05-reliability
part: 2
tags: [rabbitmq, publisher-confirms, mandatory, idempotency]
status: draft
updated: 2026-11-24
---

# Упражнения: надёжность

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Решения запускались на настоящем RabbitMQ 4.4.1 с pika 1.5.0 с помощником `Confirmed` из [базовой теории](basics.ru.md#как-проверялся-код). Удаляйте созданные очереди и обменники.

## 1. 🟢 Первое подтверждение

**Задача:** объявите durable-очередь `rel`, включите confirms и опубликуйте persistent-сообщение. Выведите вердикт брокера.

**Ожидаемый результат:** `ack` (время зависит от машины).

<details><summary>Подсказка</summary>

`pub = Confirmed(ch)`, затем `pub.publish("", "rel", b"a", pika.BasicProperties(delivery_mode=2))`.

</details>

<details><summary>Решение</summary>

См. код в [базовой теории](basics.ru.md#publisher-confirms).

</details>

## 2. 🟢 Mandatory

**Задача:** создайте direct-обменник `d` без привязок и опубликуйте в него сообщение дважды: с `mandatory=True` и без. Выведите вердикт и список возвратов для каждого случая.

**Ожидаемый результат:**

```text
unroutable mandatory: ack [(312, 'NO_ROUTE')]
unroutable no mandatory: ack []
```

<details><summary>Подсказка</summary>

Очистите `pub.returned` между попытками.

</details>

<details><summary>Решение</summary>

См. код в [базовой теории](basics.ru.md#mandatory-и-basicreturn).

</details>

## 3. 🟡 Nack от переполненной очереди

**Задача:** объявите очередь `lim` с `x-max-length: 1` и `x-overflow: reject-publish` и опубликуйте два сообщения. Выведите оба вердикта.

**Ожидаемый результат:** `first: ack second: nack`.

<details><summary>Подсказка</summary>

Второе сообщение не помещается, потому что очередь полна.

</details>

<details><summary>Решение</summary>

См. код в [продвинутой теории](advanced.ru.md#nack-от-брокера).

</details>

## 4. 🟡 Идемпотентный потребитель

**Задача:** опубликуйте три сообщения с одним `message_id="order-42"` в очередь `idem` и напишите потребителя, который «выполняет работу» только один раз.

**Ожидаемый результат:** `processed: ['order-42']`.

<details><summary>Подсказка</summary>

Храните множество увиденных `message_id`; подтверждайте **все** три сообщения.

</details>

<details><summary>Решение</summary>

См. код в [продвинутой теории](advanced.ru.md#идемпотентный-потребитель).

</details>

## 5. 🔴 Конвейерные подтверждения

**Задача:** опубликуйте 300 сообщений без ожидания подтверждения после каждого и дождитесь, пока обратный вызов насчитает 300 `ack`. Сравните время с вариантом «по одному».

**Ожидаемый результат:** все 300 подтверждены; конвейер на порядки быстрее (числа зависят от окружения: в проверке 0,048 с против 13,4 с).

<details><summary>Подсказка</summary>

`multiple=True` означает «все теги до указанного включительно»; считайте `delivery_tag - acked`.

</details>

<details><summary>Решение</summary>

См. код в [продвинутой теории](advanced.ru.md#пропускная-способность). Для боевого кода храните множество неподтверждённых тегов (из `on_publish`) и повторяйте публикацию при `nack` или обрыве.

</details>
