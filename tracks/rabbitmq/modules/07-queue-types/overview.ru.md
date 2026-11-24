---
title: "Типы очередей"
lang: ru
track: rabbitmq
kind: module
doc: overview
slug: 07-queue-types
part: 2
levels: [1, 2, 3, 4]
tags: [rabbitmq, queue-types, classic-queue, quorum-queue, stream, delivery-limit]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Типы очередей

🌐 **Русский** · [English](overview.en.md)

## Коротко

В RabbitMQ 4.4 три типа очередей, выбираемых аргументом `x-queue-type`: **classic** (по умолчанию, один узел), **quorum** (реплицируемая очередь на консенсусе, для данных, которые нельзя потерять) и **stream** (неразрушающий журнал с чтением по смещению). У каждого свои ограничения, а тип очереди изменить нельзя.

## Ключевые идеи

- **Тип задаётся при объявлении**; повторное объявление с другим типом — `406`.
- **Quorum:** только durable (иначе `541`), нет `exclusive` и `x-max-priority` (`406`), есть `x-message-ttl` и DLX.
- **Stream:** чтение не удаляет сообщения; нужны `basic_qos` и ручной ack (`auto_ack` — `406`); `x-stream-offset`: `first`, `last`, `next`, число.
- **`last` — это последний чанк**, а не последнее сообщение.
- **Delivery limit (`x-delivery-limit`)** считает `reject(requeue=True)` и обрыв канала; **`nack(requeue=True)` в тесте не считался** — сообщение ходило по кругу.
- **Репликация не проверялась** (один узел): см. [модуль 08](../08-clustering/overview.ru.md).

## Минимальный пример

```python
ch.queue_declare("qq", durable=True, arguments={
    "x-queue-type": "quorum", "x-delivery-limit": 3,
    "x-dead-letter-exchange": "", "x-dead-letter-routing-key": "qqd"})
```

Всё запускалось на настоящем RabbitMQ 4.4.1 в Docker (см. [базовую теорию](basics.ru.md)).

## Когда применять и когда нет

| Задача | Тип |
|---|---|
| Простые быстрые очереди | classic |
| Задачи, которые нельзя терять при отказе узла | quorum |
| События с повторным чтением и многими потребителями | stream |

- ❌ Не рассчитывайте на `nack(requeue=True)` + delivery limit.
- ❌ Не пытайтесь сменить тип существующей очереди.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | три типа, ограничения, stream как журнал |
| [2. Продвинутая теория](advanced.ru.md) | delivery limit, смещения stream |
| [3. Экспертная теория](expert.ru.md) | справочник возможностей по типам, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn rabbitmq 07-queue-types level 5` |
| 6. Тесты | пока не созданы — `/learn rabbitmq 07-queue-types level 6` |

## Связанные материалы

- Пререквизиты: [03 · Очереди и доставка](../03-queues-and-delivery/overview.ru.md), [06 · Dead-lettering и повторы](../06-dead-lettering-and-retries/overview.ru.md)
- Дальше: [08 · Кластеризация и высокая доступность](../08-clustering/overview.ru.md)
- Аналоги в других треках: [фоновые задачи в C#](../../../csharp/recipes/background-jobs/overview.ru.md)
