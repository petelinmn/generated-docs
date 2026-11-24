---
title: "Надёжность"
lang: ru
track: rabbitmq
kind: module
doc: overview
slug: 05-reliability
part: 2
levels: [1, 2, 3, 4]
tags: [rabbitmq, publisher-confirms, mandatory, transactions, at-least-once, idempotency]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Надёжность

🌐 **Русский** · [English](overview.en.md)

## Коротко

Надёжная доставка — цепочка из четырёх звеньев: **издатель → брокер** (publisher confirms), **маршрут** (`mandatory`), **хранение** (durable-очередь и persistent-сообщения) и **потребитель** (ручной ack после работы). Вместе они дают доставку «минимум один раз»: сообщение не теряется, но может прийти дважды, поэтому потребитель должен быть идемпотентным.

## Ключевые идеи

- **Confirm — это «брокер принял», а не «обработано».**
- **`ack` приходит и для сообщения без маршрута**, если нет `mandatory`; с `mandatory` сначала возврат `312 NO_ROUTE`.
- **`nack` от брокера** — например, переполненная очередь с `reject-publish`.
- **Ошибка канала** (несуществующий обменник) закрывает канал, подтверждения нет: ждите с таймаутом.
- **Конвейерные confirms** в сотни раз быстрее «по одному» (0,048 с против 13,4 с на 300 сообщений в тесте).
- **Транзакции** есть только в `BlockingConnection` и дороги; используйте confirms.
- **Идемпотентность по `message_id`**, хранимая в базе, закрывает дубликаты.

## Минимальный пример

```python
pub = Confirmed(ch)        # helper from the basics, wraps confirm_delivery
verdict = pub.publish("", "rel", b"a", pika.BasicProperties(delivery_mode=2), mandatory=True)
print(verdict)             # ack
```

Всё запускалось на настоящем RabbitMQ 4.4.1 в Docker (см. [базовую теорию](basics.ru.md)).

## Когда применять и когда нет

| Нужно | Механизм |
|---|---|
| Знать, что брокер принял сообщение | confirms |
| Знать, что у сообщения был маршрут | `mandatory` |
| Пережить перезапуск брокера | durable + persistent |
| Не терять при падении потребителя | ручной ack |
| Пережить повтор | идемпотентность |

- ❌ Не включайте confirms «по одному» на высокой нагрузке: используйте конвейер.
- ❌ Не полагайтесь на транзакции вместо confirms.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | звенья надёжности, confirms, mandatory, «минимум один раз» |
| [2. Продвинутая теория](advanced.ru.md) | nack, ошибки, пропускная способность, транзакции, идемпотентность |
| [3. Экспертная теория](expert.ru.md) | справочник ответов брокера и API pika, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn rabbitmq 05-reliability level 5` |
| 6. Тесты | пока не созданы — `/learn rabbitmq 05-reliability level 6` |

## Связанные материалы

- Пререквизиты: [03 · Очереди и доставка](../03-queues-and-delivery/overview.ru.md), [04 · Шаблоны обмена сообщениями](../04-messaging-patterns/overview.ru.md)
- Дальше: [06 · Dead-lettering и повторы](../06-dead-lettering-and-retries/overview.ru.md)
- Аналоги в других треках: [фоновые задачи в C#](../../../csharp/recipes/background-jobs/overview.ru.md)
