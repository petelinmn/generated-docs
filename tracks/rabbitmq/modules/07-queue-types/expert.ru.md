---
title: "Типы очередей: экспертный уровень"
lang: ru
track: rabbitmq
kind: module
doc: expert
slug: 07-queue-types
part: 2
tags: [rabbitmq, queue-types, quorum-queue, stream, classic-queue, delivery-limit, x-stream-offset]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Типы очередей: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. «Проверено» означает наблюдалось на одном узле RabbitMQ 4.4.1 с pika 1.5.0.

## Справочник

### Возможности по типам

| Возможность | classic | quorum | stream | Проверено |
|---|---|---|---|---|
| `x-queue-type` по умолчанию | да | нет | нет | да |
| `durable=False` | отказ `541` (если не exclusive) | `541` | `541` | да |
| `exclusive=True` | да | `406` | не проверялось | quorum — да |
| `x-max-priority` | не проверялось | `406` | не проверялось | quorum — да |
| `x-message-ttl` | да ([модуль 03](../03-queues-and-delivery/advanced.ru.md#ttl-сообщений)) | допустимо | не проверялось | classic, quorum — да |
| DLX | да ([модуль 06](../06-dead-lettering-and-retries/basics.ru.md)) | да | не проверялось | classic, quorum — да |
| `x-delivery-limit` | — | да (`reject`, обрыв) | — | да |
| неразрушающее чтение, offset | нет | нет | да | да |
| `auto_ack=True` | да | да | `406` | stream — да |
| смена типа повторным объявлением | — | — | — | `406` |

### Аргументы и их результат

| Аргумент | Тип | Проверено |
|---|---|---|
| `x-queue-type` | `classic`/`quorum`/`stream` | да; неизвестное значение → `406` |
| `x-delivery-limit` | quorum | да, см. [продвинутую теорию](advanced.ru.md#delivery-limit) |
| `x-stream-offset` | при `basic_consume` на stream | `first`, `last`, `next`, число — да; `timestamp` — нет |
| `x-max-length-bytes`, `x-stream-max-segment-size-bytes`, `x-max-age` (retention stream) | stream | не проверялись |

> ⚠️ Проверить: аргументы хранения stream (`x-max-length-bytes`, `x-max-age`, `x-stream-max-segment-size-bytes`), лимиты длины quorum-очередей, поведение `x-overflow` и наличие у типов других `x-…` аргументов здесь не проверялись.

### Коды ошибок объявления (проверено)

| Ситуация | Код | Закрывается |
|---|---|---|
| несовместимое свойство (`exclusive` у quorum) | `406` | канал |
| недопустимый аргумент (`x-max-priority` у quorum) | `406` | канал |
| неизвестный тип | `406` | канал |
| смена типа | `406` | канал |
| не-durable и не exclusive | `541` | соединение |

## Внутреннее устройство

- **Classic** — очередь живёт на одном узле; при потере узла сообщения недоступны (и потеряны, если не persistent).
- **Quorum** — реплицируемый журнал на основе консенсуса; запись подтверждается после того, как её принимает большинство реплик ([модуль 08](../08-clustering/overview.ru.md)). В этом модуле, с одним узлом, репликация не проверялась.
- **Stream** — журнал только на добавление; потребители читают по смещению, ack не удаляет сообщение (проверено), удаление идёт по политике хранения.
- **Счётчик доставок** — свойство quorum-очереди: `x-delivery-count` в заголовках повторных доставок; `nack(requeue=True)` в тесте счётчик не увеличивал.

> ⚠️ Проверить: детали Raft, репликации и формат чанков stream взяты из документации и здесь не проверялись.

## Подвохи

### 1. `nack(requeue=True)` и delivery limit

Сообщение ходило по кругу 582 раза за 2 с и в DLX не попало (проверено). `reject(requeue=True)` — 4 доставки и DLX.

### 2. Тип нельзя поменять

Повторное объявление с другим `x-queue-type` даёт `406`.

### 3. Quorum без durable

Соединение закрывается с `541`, а не канал.

### 4. Stream без prefetch

`406 … consumer prefetch count is not set` и `auto_ack` запрещён.

### 5. `last` — это чанк

В тесте `last` вернул все шесть сообщений.

### 6. Приоритеты и exclusive

У quorum не поддерживаются (`406`).

### 7. Stream не удаляет по ack

Сообщения остаются; читать можно повторно (проверено). Размер контролируйте политикой хранения.

## Компромиссы и альтернативы

| Выбор | Плюсы | Минусы |
|---|---|---|
| classic | быстро, просто | один узел, нет репликации |
| quorum | сохранность при отказе узла | нет приоритетов и exclusive, больше накладных расходов |
| stream | повторное чтение, много потребителей | другая модель, нужны prefetch и политика хранения |
| Kafka вместо stream | экосистема журналов | отдельная система |

## Вопросы с собеседований

<details><summary>1. Какие типы очередей есть в RabbitMQ 4.4?</summary>

classic, quorum и stream; тип задаётся `x-queue-type`, по умолчанию classic.

→ [Основы](basics.ru.md#три-типа-очередей)

</details>

<details><summary>2. Чем quorum отличается от classic?</summary>

Quorum реплицируется на основе консенсуса и переживает отказ узла; classic живёт на одном узле.

→ [Основы](basics.ru.md#три-типа-очередей)

</details>

<details><summary>3. Подвох: можно ли объявить quorum-очередь с `exclusive=True`?</summary>

Нет: `406 PRECONDITION_FAILED - invalid property 'exclusive-owner'`.

→ [Основы](basics.ru.md#ограничения-quorum-очередей-и-stream)

</details>

<details><summary>4. Что произойдёт с `durable=False` у quorum?</summary>

Брокер закроет соединение с `541` (не-durable не-exclusive очереди запрещены на 4.4).

→ [Подвох 3](#3-quorum-без-durable)

</details>

<details><summary>5. Чем stream отличается от очереди?</summary>

Неразрушающий журнал: чтение не удаляет сообщения, потребитель выбирает смещение; удаление по политике хранения.

→ [Основы](basics.ru.md#stream-журнал-вместо-очереди)

</details>

<details><summary>6. Подвох: как прочитать stream с начала?</summary>

`x-stream-offset: "first"` при `basic_consume`, с `basic_qos(prefetch_count=N)` и ручным ack.

→ [Основы](basics.ru.md#stream-журнал-вместо-очереди)

</details>

<details><summary>7. Зачем нужен delivery limit?</summary>

Чтобы «ядовитое» сообщение не ходило по кругу: после лимита оно уходит в DLX с причиной `delivery_limit`.

→ [Продвинутая теория](advanced.ru.md#delivery-limit)

</details>

<details><summary>8. Подвох: считает ли delivery limit `nack(requeue=True)`?</summary>

В нашем тесте нет: 582 доставки без DLX. `reject(requeue=True)` и обрыв канала считаются.

→ [Подвох 1](#1-nackrequeuetrue-и-delivery-limit)

</details>

<details><summary>9. Что вернёт `x-stream-offset: "next"`?</summary>

Только сообщения, пришедшие после подписки.

→ [Продвинутая теория](advanced.ru.md#смещения-в-stream)

</details>

<details><summary>10. Можно ли сменить тип очереди?</summary>

Нет (`406`): создайте новую и перенесите сообщения.

→ [Подвох 2](#2-тип-нельзя-поменять)

</details>

<details><summary>11. Подвох: `x-stream-offset: "last"` вернул все сообщения. Почему?</summary>

Он начинает с последнего чанка, а в нём были все шесть сообщений.

→ [Продвинутая теория](advanced.ru.md#смещения-в-stream)

</details>

<details><summary>12. Когда выбрать quorum, а когда stream?</summary>

Quorum — для задач, которые нельзя терять; stream — для событий, читаемых многими с разных позиций.

→ [Основы](basics.ru.md#что-выбрать)

</details>

## Шпаргалка

```text
x-queue-type  : classic (default) | quorum | stream
quorum        : durable only; no exclusive / x-max-priority; x-delivery-limit counts reject, not nack
stream        : durable; basic_qos required; no auto_ack; x-stream-offset first|last|next|N
change type   : impossible (406) -> new queue + migrate
```

- `reject` или DLX вместо `nack(requeue=True)` для защиты от «ядовитых» сообщений.
- Stream хранит сообщения, пока не сработает политика хранения.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Раньше: [06 · Dead-lettering и повторы](../06-dead-lettering-and-retries/expert.ru.md)
- Следующий модуль: [08 · Кластеризация и высокая доступность](../08-clustering/overview.ru.md)

## Источники

- [Quorum Queues](https://www.rabbitmq.com/docs/quorum-queues)
- [Streams and Super Streams](https://www.rabbitmq.com/docs/streams)
- [Classic Queues](https://www.rabbitmq.com/docs/classic-queues)
