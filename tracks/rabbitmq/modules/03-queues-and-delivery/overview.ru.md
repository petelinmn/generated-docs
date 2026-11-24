---
title: "Очереди и доставка"
lang: ru
track: rabbitmq
kind: module
doc: overview
slug: 03-queues-and-delivery
part: 1
levels: [1, 2, 3, 4]
tags: [rabbitmq, queue, acknowledgement, prefetch, durability, message-ttl, queue-length-limit, redelivery]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Очереди и доставка

🌐 **Русский** · [English](overview.en.md)

## Коротко

Очередь хранит сообщения, пока их не заберут потребители. Насколько надёжно и честно это происходит, зависит от нескольких переключателей: **подтверждения** (`ack`, `nack`, `reject`, вернуть в очередь или отбросить), **prefetch** (сколько неподтверждённых сообщений может держать потребитель), **долговечность** (durable-очередь и persistent-сообщения переживают перезапуск брокера) и **ограничения** (TTL, максимальная длина, срок жизни самой очереди).

## Ключевые идеи

- **Подтверждайте после обработки.** `basic_ack` убирает сообщение; `basic_nack`/`basic_reject` с `requeue=True` возвращает его, с `requeue=False` отбрасывает (или отправляет в dead-letter, [модуль 06](../06-dead-lettering-and-retries/overview.ru.md).
- **Умерший потребитель возвращает неподтверждённые сообщения** в очередь с пометкой `redelivered`.
- **Prefetch (`basic_qos`) управляет справедливостью.** С `prefetch_count=1` застрявший потребитель держит одно сообщение; без лимита ему могут отдать всё.
- **Durable ≠ persistent.** Durable-очередь переживает перезапуск; сообщение — только если оно ещё и persistent (`delivery_mode=2`). В тесте с перезапуском persistent-сообщение осталось, а transient потерялось.
- **Ограничения:** `x-message-ttl`, `expiration` на сообщение, `x-max-length` с `x-overflow`, `x-expires`.
- **Exclusive**-очереди принадлежат одному соединению; **auto-delete**-очереди удаляются после последнего потребителя.
- **Повторное объявление очереди с другими аргументами падает** (`406`).

## Минимальный пример

```python
ch.queue_declare("jobs", durable=True)
ch.basic_qos(prefetch_count=1)

def on_job(ch_, method, props, body):
    try:
        handle(body)
        ch_.basic_ack(method.delivery_tag)
    except Exception:
        ch_.basic_nack(method.delivery_tag, requeue=False)

ch.basic_consume("jobs", on_job)
```

Всё запускалось на настоящем RabbitMQ 4.4.1 в Docker (см. [базовую теорию](basics.ru.md)).

## Когда применять и когда нет

| Нужно | Переключатель |
|---|---|
| Не терять работу при падении воркера | ручное подтверждение |
| Делить работу поровну между воркерами | `prefetch_count=1` (или малое) |
| Сообщения должны пережить перезапуск | durable-очередь + persistent-сообщения |
| Отбрасывать устаревшие сообщения | TTL |
| Защитить память от безграничной очереди | `x-max-length` |
| Личная очередь ответов | exclusive или очередь с именем от сервера |

- ❌ Не полагайтесь на `auto_ack=True` для важной работы.
- ❌ Не делайте очереди неограниченными без мониторинга.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | ack, nack, reject, повторная доставка, prefetch, долговечность |
| [2. Продвинутая теория](advanced.ru.md) | TTL, лимиты длины и overflow, срок жизни, exclusive и auto-delete очереди, несовпадение аргументов |
| [3. Экспертная теория](expert.ru.md) | справочник аргументов очередей с наблюдаемым поведением, таймаут доставки, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn rabbitmq 03-queues-and-delivery level 5` |
| 6. Тесты | пока не созданы — `/learn rabbitmq 03-queues-and-delivery level 6` |

## Связанные материалы

- Пререквизиты: [01 · Обмен сообщениями и модель AMQP](../01-messaging-basics/overview.ru.md), [02 · Обменники и маршрутизация](../02-exchanges-and-routing/overview.ru.md)
- Дальше: [04 · Шаблоны обмена сообщениями](../04-messaging-patterns/overview.ru.md)
- Аналоги в других треках: [фоновые задачи в C#](../../../csharp/recipes/background-jobs/overview.ru.md)
