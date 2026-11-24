---
title: "Типы очередей: продвинутый уровень"
lang: ru
track: rabbitmq
kind: module
doc: advanced
slug: 07-queue-types
part: 2
tags: [rabbitmq, quorum-queue, delivery-limit, poison-message, stream, stream-offset]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Типы очередей: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Код запускался на RabbitMQ 4.4.1 (один узел) с pika 1.5.0.

## Delivery limit

Quorum-очередь считает, сколько раз сообщение возвращали в очередь, и по превышении `x-delivery-limit` отправляет его в DLX ([модуль 06](../06-dead-lettering-and-retries/basics.ru.md)) или удаляет. Это защита от «ядовитых» сообщений.

```python
ch.queue_declare("qqd", durable=True)
ch.queue_declare("qq", durable=True, arguments={
    "x-queue-type": "quorum", "x-delivery-limit": 3,
    "x-dead-letter-exchange": "", "x-dead-letter-routing-key": "qqd"})
```

Одно и то же сообщение возвращали в очередь четырьмя способами (лимит 3):

| Способ | Доставок | В DLX |
|---|---|---|
| `basic_get` + `basic_reject(requeue=True)` | 4 | да |
| потребитель + `basic_reject(requeue=True)` | 4 | да |
| обрыв соединения до подтверждения | 4 | да |
| `basic_get` + `basic_nack(requeue=True)` | 8 (остановили сами) | нет |
| потребитель + `basic_nack(requeue=True)` | 582 за 2 с | нет |

Вывод фрагментов:

```text
basic_get+reject(requeue): [(0, None, False), (1, 1, True), (2, 2, True), (3, 3, True)]
dead: 1 qq: 0
crash rounds delivered: 4 dead: 1 qq: 0
A get+nack: deliveries 8 dead 0
C consumer+reject: deliveries 4 [None, 1, 2, 3] dead 1
```

Что видно:

- Заголовок `x-delivery-count` появляется у повторных доставок и растёт: `1, 2, 3`.
- После превышения лимита (четвёртая доставка — это три возврата) сообщение уходит в DLX с причиной `delivery_limit`:

```text
dead header: {'x-death': '...', 'x-first-death-exchange': '', 'x-first-death-queue': 'qq', 'x-first-death-reason': 'delivery_limit', 'x-last-death-exchange': '', 'x-last-death-queue': 'qq', 'x-last-death-reason': 'delivery_limit'}
```

- **`basic_nack(requeue=True)` в этом тесте счётчик не увеличивал**, и сообщение бесконечно ходило по кругу. Это объясняет результат модуля 06: там потребитель делал `nack`, и лимит «не сработал».

> ⚠️ Проверить: причина, по которой `nack` с `requeue=True` не учитывается (документированное поведение или особенность версии 4.4.1), не выяснена. Для защиты от «ядовитых» сообщений используйте `reject` либо `requeue=False` с DLX.

## Смещения в stream

Позицию чтения задаёт аргумент `x-stream-offset` при `basic_consume`. Журнал: `e0…e5`.

| Значение | Результат (проверено) |
|---|---|
| `"first"` | `e0…e5` |
| `"last"` | `e0…e5` (последний записанный **чанк**, а не последнее сообщение) |
| `"next"` | пусто, пока не придут новые сообщения |
| число `3` | `e3, e4, e5` |

```text
offset 'first' ['e0', 'e1', 'e2', 'e3', 'e4', 'e5']
offset 'last' ['e0', 'e1', 'e2', 'e3', 'e4', 'e5']
offset 'next' []
offset 3 ['e3', 'e4', 'e5']
```

`last` начинает с последнего **чанка** — блока сообщений, записанных вместе; шесть сообщений, опубликованных подряд, оказались в одном чанке, поэтому прочиталось всё.

> ⚠️ Проверить: как формируются чанки и значение `timestamp` у `x-stream-offset` здесь не проверялись.

## Quorum-очередь и привычные возможности

В тесте quorum-очередь приняла `x-message-ttl` и DLX. Приоритеты (`x-max-priority`) и `exclusive` не поддерживаются ([основы](basics.ru.md#ограничения-quorum-очередей-и-stream)). Остальной список возможностей по типам — в [экспертной теории](expert.ru.md#справочник).

## Лучшие практики

- Для важных задач — quorum-очередь с `x-delivery-limit` и DLX; в потребителе при неисправимой ошибке — `reject(requeue=False)`, а не `nack(requeue=True)`.
- Не меняйте тип на лету: планируйте миграцию (новая очередь, перенос, переключение).
- Stream — для событий, которые читают разные потребители с разных позиций; удаление сообщений — по политике хранения, а не по ack.
- Следите за размером журнала stream (политика хранения по времени или размеру).

## Частые ошибки

- ❌ Полагаться на delivery limit при `nack(requeue=True)` → ✅ `reject` или DLX.
- ❌ Читать stream с `auto_ack` → ✅ prefetch и ack.
- ❌ Считать, что `last` вернёт одно сообщение → ✅ это последний чанк.
- ❌ Объявлять quorum без `durable` → ✅ `durable=True`.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- [06 · Dead-lettering и повторы](../06-dead-lettering-and-retries/advanced.ru.md) — причина `delivery_limit`
- Следующий модуль: [08 · Кластеризация и высокая доступность](../08-clustering/overview.ru.md)

## Источники

- [Quorum Queues: Poison Message Handling](https://www.rabbitmq.com/docs/quorum-queues)
- [Streams and Super Streams](https://www.rabbitmq.com/docs/streams)
- [Dead Letter Exchanges](https://www.rabbitmq.com/docs/dlx)
