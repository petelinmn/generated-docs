---
title: "Обменники и маршрутизация"
lang: ru
track: rabbitmq
kind: module
doc: overview
slug: 02-exchanges-and-routing
part: 1
levels: [1, 2, 3, 4]
tags: [rabbitmq, exchange, routing-key, binding, topic-exchange, fanout, direct-exchange, headers-exchange, alternate-exchange]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Обменники и маршрутизация

🌐 **Русский** · [English](overview.en.md)

## Коротко

Обменник (exchange) принимает сообщения и решает, какие очереди получат копию. Решение зависит от **типа** обменника и его **привязок** (bindings): `direct` сравнивает ключ маршрутизации точно, `fanout` копирует во все привязанные очереди, `topic` сопоставляет шаблоны через точку с `*` и `#`, `headers` сопоставляет заголовки сообщения. Сообщение, не попавшее ни в одну очередь, отбрасывается — если не попросить вернуть его (`mandatory`) или отправить в **alternate exchange**.

## Ключевые идеи

- **Четыре типа:** `direct`, `fanout`, `topic`, `headers`; плюс безымянный **обменник по умолчанию** — `direct` с неявной привязкой для каждой очереди.
- **Привязка** — правило «обменник → очередь» с ключом привязки (или аргументами для `headers`).
- **`direct`:** ключ маршрутизации равен ключу привязки. Несколько очередей с одним ключом получают сообщение все.
- **`fanout`:** ключ игнорируется.
- **`topic`:** `*` — ровно одно слово, `#` — ноль или больше слов; слова разделены точками. Ключ `lazy` подходит под `lazy.#`.
- **`headers`:** `x-match: all` требует все заголовки, `x-match: any` — любой.
- **Очередь получает одну копию**, даже если подходят несколько её привязок.
- **Немаршрутизируемые сообщения исчезают молча** — если не опубликовать с `mandatory=True` (вернётся `312 NO_ROUTE`) или не задать обменнику `alternate-exchange`.
- **Повторное объявление обменника с другими аргументами падает** (`406 PRECONDITION_FAILED`).

## Минимальный пример

```python
ch.exchange_declare("logs", exchange_type="topic")
q = ch.queue_declare("", exclusive=True).method.queue
ch.queue_bind(q, "logs", routing_key="app.*.error")
ch.basic_publish("logs", "app.db.error", b"disk full")      # delivered to q
ch.basic_publish("logs", "app.db.warn", b"slow query")      # dropped: no match
```

Все примеры запускались на настоящем RabbitMQ 4.4.1 в Docker (см. [базовую теорию](basics.ru.md)).

## Когда применять и когда нет

| Задача | Обменник |
|---|---|
| Отправить в одну именованную очередь | обменник по умолчанию |
| Маршрутизация по фиксированной категории (`info`, `error`) | `direct` |
| Рассылка всем подписчикам | `fanout` |
| Маршрутизация по иерархии (`region.service.level`) | `topic` |
| Маршрутизация по нескольким атрибутам, а не по ключу | `headers` |

- ❌ Не кладите в ключ маршрутизации бизнес-данные сверх нужд маршрутизации.
- ❌ Не полагайтесь на `headers` на критичных по скорости путях: они сопоставляют таблицу.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | четыре типа и обменник по умолчанию с проверенными выводами |
| [2. Продвинутая теория](advanced.ru.md) | немаршрутизируемые сообщения, `mandatory`, alternate exchange, привязки обменник-обменник, internal и auto-delete обменники |
| [3. Экспертная теория](expert.ru.md) | справочник параметров объявления и правил, зарезервированные имена, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn rabbitmq 02-exchanges-and-routing level 5` |
| 6. Тесты | пока не созданы — `/learn rabbitmq 02-exchanges-and-routing level 6` |

## Связанные материалы

- Пререквизиты: [01 · Обмен сообщениями и модель AMQP](../01-messaging-basics/overview.ru.md)
- Дальше: [03 · Очереди и доставка](../03-queues-and-delivery/overview.ru.md)
- Аналоги в других треках: [маршрутизация Services в Kubernetes](../../../kubernetes/overview.ru.md)
