---
title: "Семантика доставки"
lang: ru
track: kafka
kind: module
doc: overview
slug: 05-delivery-semantics
part: 2
levels: [1, 2, 3, 4]
tags: [kafka, delivery-semantics, exactly-once, transactions, isolation-level, idempotence]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Семантика доставки

🌐 **Русский** · [English](overview.en.md)

## Коротко

Kafka позволяет выбрать гарантию: **не более одного раза** (позиция фиксируется до обработки), **минимум один раз** (после обработки) и **ровно один раз** (идемпотентный производитель плюс транзакции). «Ровно один раз» работает в цепочке Kafka → обработка → Kafka: запись результата и фиксация входной позиции входят в одну транзакцию, а читатели результата используют `isolation.level=read_committed`.

## Ключевые идеи

- **Минимум один раз:** фиксация после обработки; повтор записи возможен.
- **Идемпотентный производитель** защищает повторы одного производителя в партиции, но не атомарность между партициями.
- **Транзакция:** `init_transactions`, затем `begin` → `produce` → `commit`/`abort`; `transactional.id` стабильный.
- **`read_committed`** скрывает отменённые и открытые транзакции; по умолчанию читатель — `read_uncommitted`.
- **Метки транзакций занимают смещения** (пропуски в нумерации).
- **Чтение-обработка-запись:** `send_offsets_to_transaction` — позиция и результат подтверждаются вместе (в опыте выход ровно `M0…M5`).
- **Ограждение зомби:** новый `init_transactions()` с тем же `transactional.id` даёт старому `_FENCED` (фатально).
- **Внешние эффекты** вне гарантии: нужны идемпотентность или outbox.

## Минимальный пример

```python
tp = Producer({"bootstrap.servers": "localhost:9092", "transactional.id": "tx-1"})
tp.init_transactions()
tp.begin_transaction(); tp.produce("out", value=b"v"); tp.commit_transaction()
```

Всё запускалось на настоящем Apache Kafka 4.3.1 в Docker (см. [базовую теорию](basics.ru.md)).

## Когда применять и когда нет

| Задача | Выбор |
|---|---|
| Конвейер Kafka→Kafka без дублей | транзакции |
| Простая обработка | at-least-once + идемпотентный обработчик |
| Результат во внешней системе | идемпотентность или outbox |

- ❌ Не считайте идемпотентность транзакциями.
- ❌ Не читайте результат в `read_uncommitted`.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | три семантики, транзакции производителя, `read_committed`, открытая транзакция |
| [2. Продвинутая теория](advanced.ru.md) | чтение-обработка-запись, ограждение зомби, порядок вызовов, границы |
| [3. Экспертная теория](expert.ru.md) | справочник API и ошибок, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn kafka 05-delivery-semantics level 5` |
| 6. Тесты | пока не созданы — `/learn kafka 05-delivery-semantics level 6` |

## Связанные материалы

- Пререквизиты: [02 · Производители](../02-producers/overview.ru.md), [03 · Потребители и группы](../03-consumers-and-groups/overview.ru.md)
- Дальше: [06 · Схемы и сериализация](../06-schemas-and-serialization/overview.ru.md)
- Аналоги в других треках: [RabbitMQ: надёжность](../../../rabbitmq/modules/05-reliability/overview.ru.md)
- [System design: сообщения и события](../../../system-design/modules/08-messaging-and-events/overview.ru.md)
