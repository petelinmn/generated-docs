---
title: "Шаблоны обмена сообщениями"
lang: ru
track: rabbitmq
kind: module
doc: overview
slug: 04-messaging-patterns
part: 1
levels: [1, 2, 3, 4]
tags: [rabbitmq, work-queue, publish-subscribe, routing, rpc, direct-reply-to, correlation-id]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Шаблоны обмена сообщениями

🌐 **Русский** · [English](overview.en.md)

## Коротко

Из обменников, очередей и привязок строятся четыре типовых шаблона: **work queue** (одно сообщение — одному воркеру), **publish/subscribe** (каждому подписчику своя копия), **routing** (подписчик выбирает ключи) и **RPC** (запрос и ответ, связанные `correlation_id`). Шаблоны различаются тем, кто делит сообщения и кто их копирует.

## Ключевые идеи

- **Одна очередь — делим, очередь на подписчика — копируем.**
- **Work queue** — durable-очередь, `prefetch_count=1`, ack после работы: быстрый воркер забрал 9 задач из 10.
- **Pub/sub** — fanout и временные очереди (`queue_declare("", exclusive=True)`); опубликованное без подписчиков теряется.
- **Routing** — direct-обменник; одна очередь может иметь несколько привязок.
- **RPC** — `reply_to` и `correlation_id`; брокер их не интерпретирует; клиент сам ставит таймаут.
- **Direct reply-to** (`amq.rabbitmq.reply-to`) — без очереди на клиента; нужен `auto_ack=True` и подписка до публикации.
- **`expiration` на запросе** не даёт устаревшему RPC выполниться после возвращения сервера.

## Минимальный пример

```python
ch.exchange_declare("logs", "fanout")
q = ch.queue_declare("", exclusive=True).method.queue   # server-named
ch.queue_bind(q, "logs")
ch.basic_consume(q, lambda c, m, p, b: print(b), auto_ack=True)
ch.basic_publish("logs", "", b"hello")
```

Всё запускалось на настоящем RabbitMQ 4.4.1 в Docker (см. [базовую теорию](basics.ru.md)).

## Когда применять и когда нет

| Нужно | Шаблон |
|---|---|
| Распределить тяжёлую работу | work queue |
| Оповестить всех | pub/sub |
| Оповестить заинтересованных | routing / topics |
| Получить результат сразу | RPC |
| Результат не нужен немедленно | команда + событие-результат |

- ❌ Не используйте RPC там, где ответ не нужен сейчас.
- ❌ Не читайте несколько подписчиков из одной очереди, если им нужны копии.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | work queue, pub/sub, routing, RPC |
| [2. Продвинутая теория](advanced.ru.md) | direct reply-to, RPC с таймаутом, выбор шаблона |
| [3. Экспертная теория](expert.ru.md) | справочник шаблонов и свойств, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn rabbitmq 04-messaging-patterns level 5` |
| 6. Тесты | пока не созданы — `/learn rabbitmq 04-messaging-patterns level 6` |

## Связанные материалы

- Пререквизиты: [02 · Обменники и маршрутизация](../02-exchanges-and-routing/overview.ru.md), [03 · Очереди и доставка](../03-queues-and-delivery/overview.ru.md)
- Дальше: [05 · Надёжность](../05-reliability/overview.ru.md)
- Аналоги в других треках: [фоновые задачи в C#](../../../csharp/recipes/background-jobs/overview.ru.md)
