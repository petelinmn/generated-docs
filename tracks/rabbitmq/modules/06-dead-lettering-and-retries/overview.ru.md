---
title: "Dead-lettering и повторы"
lang: ru
track: rabbitmq
kind: module
doc: overview
slug: 06-dead-lettering-and-retries
part: 2
levels: [1, 2, 3, 4]
tags: [rabbitmq, dead-letter-exchange, dlx, x-death, retry, poison-message]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Dead-lettering и повторы

🌐 **Русский** · [English](overview.en.md)

## Коротко

Сообщение, которое нельзя обработать, не должно ни зацикливаться, ни пропадать. **Dead letter exchange (DLX)** — обменник, куда брокер отправляет «мёртвые» сообщения очереди: отклонённые без возврата (`rejected`), просроченные (`expired`) и вытесненные по длине (`maxlen`). Из них собираются очереди разбора и повторы с задержкой.

## Ключевые идеи

- **DLX — аргумент очереди** (`x-dead-letter-exchange`, `x-dead-letter-routing-key`); без него `reject` с `requeue=False` просто теряет сообщение.
- **Заголовок `x-death`** хранит историю: очередь, причину, `count`.
- **Три проверенные причины:** `rejected`, `expired`, `maxlen` (при `drop-head` «умирает» самое старое).
- **Повтор с задержкой:** `main` → DLX → очередь `retry` без потребителей с TTL → DLX обратно в `main`; после предела попыток — в `parking`.
- **DLX без маршрута** теряет сообщение молча и без ошибки.
- **Считайте попытки по `x-death`**, фильтруя по очереди и причине.
- **`x-delivery-limit` quorum-очереди** считает `reject(requeue=True)` и обрыв канала, но не `nack(requeue=True)`.

## Минимальный пример

```python
ch.exchange_declare("dlx", "direct")
ch.queue_declare("dead", durable=True); ch.queue_bind("dead", "dlx", "dead")
ch.queue_declare("work", durable=True, arguments={
    "x-dead-letter-exchange": "dlx", "x-dead-letter-routing-key": "dead"})
# a consumer that fails for good does: ch.basic_reject(tag, requeue=False)
```

Всё запускалось на настоящем RabbitMQ 4.4.1 в Docker (см. [базовую теорию](basics.ru.md)).

## Когда применять и когда нет

| Ситуация | Что делать |
|---|---|
| Неисправимая ошибка (формат) | `reject(requeue=False)` → DLX → разбор |
| Временная ошибка (сеть) | очередь `retry` с TTL, предел попыток |
| Устаревшие сообщения | TTL + DLX для аудита |
| Переполнение очереди | `x-max-length` + DLX |

- ❌ Не возвращайте падающее сообщение с `requeue=True`.
- ❌ Не оставляйте DLX без очереди и привязки.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | DLX, причины, `x-death`, потеря без DLX |
| [2. Продвинутая теория](advanced.ru.md) | повтор с задержкой, подсчёт попыток, DLX без маршрута, delivery limit |
| [3. Экспертная теория](expert.ru.md) | справочник аргументов и заголовков, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn rabbitmq 06-dead-lettering-and-retries level 5` |
| 6. Тесты | пока не созданы — `/learn rabbitmq 06-dead-lettering-and-retries level 6` |

## Связанные материалы

- Пререквизиты: [03 · Очереди и доставка](../03-queues-and-delivery/overview.ru.md), [05 · Надёжность](../05-reliability/overview.ru.md)
- Дальше: [07 · Типы очередей](../07-queue-types/overview.ru.md)
- Аналоги в других треках: [фоновые задачи в C#](../../../csharp/recipes/background-jobs/overview.ru.md)
- [System design: сообщения и события](../../../system-design/modules/08-messaging-and-events/overview.ru.md)
