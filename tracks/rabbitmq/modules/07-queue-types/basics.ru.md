---
title: "Типы очередей: основы"
lang: ru
track: rabbitmq
kind: module
doc: basics
slug: 07-queue-types
part: 2
tags: [rabbitmq, queue-types, classic-queue, quorum-queue, stream, x-queue-type]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Типы очередей: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [03 · Очереди и доставка](../03-queues-and-delivery/basics.ru.md), [06 · Dead-lettering и повторы](../06-dead-lettering-and-retries/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Все фрагменты запускались на настоящем RabbitMQ 4.4.1 в Docker (один узел) с pika 1.5.0; выводы скопированы из запусков. `P()` открывает соединение: `P = lambda: pika.Connection(pika.ConnectionParameters("localhost"))`; `ch` — канал основного соединения. Свойства, зависящие от нескольких узлов (репликация), здесь не проверялись: см. [модуль 08](../08-clustering/overview.ru.md).

## Три типа очередей

Тип выбирается при объявлении аргументом `x-queue-type`; без него очередь — `classic`.

| Тип | Устройство | Для чего |
|---|---|---|
| `classic` | обычная очередь на одном узле | быстрые простые очереди, где допустима потеря при сбое узла |
| `quorum` | реплицируемая очередь на основе консенсуса (Raft) | важные данные: сохранность при отказе узла |
| `stream` | неразрушающий журнал только на добавление | много потребителей, повторное чтение, большие объёмы |

```python
ch.queue_declare("qc", durable=True)
ch.queue_declare("qq", durable=True, arguments={"x-queue-type": "quorum"})
ch.queue_declare("qs", durable=True, arguments={"x-queue-type": "stream"})
```

```text
declared qc 0
declared qq 0
declared qs 0
```

Список типов показывает `rabbitmqctl list_queues name type`:

```text
plain	classic
qq-ttl	quorum
s	stream
```

## Ограничения quorum-очередей и stream

У новых типов есть правила, которые брокер проверяет при объявлении. Всё ниже получено запуском на 4.4.1:

| Попытка | Результат |
|---|---|
| quorum + `exclusive=True` | канал закрыт: `406 PRECONDITION_FAILED - invalid property 'exclusive-owner'` |
| quorum без `durable` | соединение закрыто: `541 … transient_nonexcl_queues is deprecated` |
| stream без `durable` | то же `541` |
| quorum + `x-max-priority` | `406 - invalid arg 'x-max-priority' … of queue type rabbit_quorum_queue` |
| quorum + `x-message-ttl` | допустимо |
| неизвестный `x-queue-type` | `406 - invalid arg 'x-queue-type' … unsupported queue type` |
| повторное объявление с другим типом | `406 - inequivalent arg 'x-queue-type'` |

Тип очереди изменить нельзя: придётся создать новую и перенести сообщения.

## Stream: журнал вместо очереди

В stream сообщения **не удаляются** при чтении: потребитель читает журнал с выбранной позиции (offset), а подтверждение лишь двигает его собственную позицию. Это отличие от обычной очереди, где ack удаляет сообщение.

```python
# publish e0..e4, then read from the start
x.basic_qos(prefetch_count=10)                       # mandatory for streams
x.basic_consume("qs", cb, arguments={"x-stream-offset": "first"})
```

```text
stream first: ['e0', 'e1', 'e2', 'e3', 'e4']
stream next: ['e5'] | first consumer now: ['e0', 'e1', 'e2', 'e3', 'e4', 'e5']
stream re-read: ['e0', 'e1', 'e2', 'e3', 'e4', 'e5']
```

- `first` — с начала журнала, `next` — только новые сообщения (потребитель получил лишь `e5`).
- Второй потребитель, пришедший позже, снова прочитал всё: сообщения никуда не делись.
- У потребителя stream обязателен `prefetch_count`, а `auto_ack=True` недопустим: иначе `406 PRECONDITION_FAILED - consumer prefetch count is not set for stream queue`.

## Что выбрать

| Нужно | Тип |
|---|---|
| Очередь задач без требований к отказоустойчивости | classic |
| Задачи, которые нельзя потерять при сбое узла | quorum |
| Рассылка многим потребителям с повторным чтением, события «как журнал» | stream |

## Типичные ошибки новичков

- ❌ Считать, что все очереди реплицируются → ✅ по умолчанию это classic на одном узле.
- ❌ Пытаться сменить тип существующей очереди → ✅ создать новую (`406`).
- ❌ Ждать, что чтение из stream удалит сообщение → ✅ журнал хранит их по политике хранения.
- ❌ Читать stream без `prefetch_count` или с `auto_ack` → ✅ задать prefetch и подтверждать.
- ❌ `x-max-priority` у quorum → ✅ приоритеты поддерживают не все типы.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — delivery limit, смещения stream
- Раньше: [06 · Dead-lettering и повторы](../06-dead-lettering-and-retries/basics.ru.md)
- Следующий модуль: [08 · Кластеризация и высокая доступность](../08-clustering/overview.ru.md)

## Источники

- [Classic Queues](https://www.rabbitmq.com/docs/classic-queues)
- [Quorum Queues](https://www.rabbitmq.com/docs/quorum-queues)
- [Streams and Super Streams](https://www.rabbitmq.com/docs/streams)
