---
title: "RabbitMQ"
lang: ru
track: rabbitmq
kind: track
doc: overview
slug: rabbitmq
tags: [rabbitmq, amqp, message-broker, messaging, queues]
versions: "RabbitMQ 4.4; AMQP 0-9-1; Python pika"
default_level: 4
status: draft
updated: 2026-11-24
---

# RabbitMQ

🌐 **Русский** · [English](overview.en.md)

## Коротко

RabbitMQ — брокер сообщений (message broker) с открытым исходным кодом. Производители (producers) отправляют сообщения брокеру, он маршрутизирует их через обменники (exchanges), привязки (bindings) и очереди (queues), а потребители (consumers) забирают их из очередей. Так сервисы развязываются по времени (потребитель может быть недоступен), по скорости (очередь сглаживает всплеск) и по знаниям (производитель не знает, кто читает сообщение).

Классический протокол — AMQP 0-9-1; RabbitMQ также поддерживает AMQP 1.0, MQTT, STOMP и имеет собственный тип очередей в виде журнала — streams. Трек начинается с модели AMQP (соединение, канал, обменник, очередь, привязка), затем разбирает гарантии доставки, типы очередей, шаблоны, эксплуатацию и безопасность.

**Пререквизиты:** базовые сети и один язык программирования; для примеров достаточно [трека Python](../python/overview.ru.md).

## Окружение

| Что | Инструмент |
|---|---|
| Брокер | `rabbitmq:4-management` в Docker: `docker run -d --name rabbit -p 5672:5672 -p 15672:15672 rabbitmq:4-management` |
| Management UI | `http://localhost:15672`, пользователь `guest`, пароль `guest` (только с localhost) |
| CLI | `rabbitmqctl` и `rabbitmq-diagnostics` внутри контейнера: `docker exec rabbit rabbitmqctl status` |
| Язык примеров | Python 3.12+ с клиентом `pika` |
| Код модуля | как в треке Python: `code/` с `pyproject.toml`, `examples/`, `solutions/`, `tests/` |
| Запустить все тесты модуля | `uv run pytest` в папке `code/` |
| Тесты и брокер | тесты, которым нужен живой брокер, помечены `integration` и пропускаются, если `localhost:5672` недоступен; остальные не используют сеть |
| Одноразовый брокер | `docker rm -f rabbit` удаляет брокер вместе с данными |

> Учётная запись `guest` и открытые порты — только для одноразового локального брокера; для продакшена см. модуль о безопасности.

## Дорожная карта

Прогресс изучения ведётся в [PROGRESS.md](../../PROGRESS.md). Уровень детализации модулей по умолчанию: **4** (теория и самопроверка), см. [уровни детализации](../../README.ru.md#уровни-детализации).

### Часть 1. Основы

- [01 · Обмен сообщениями и модель AMQP](modules/01-messaging-basics/overview.ru.md) — брокер, соединение, канал, обменник, очередь, привязка, первая отправка и получение
- [02 · Обменники и маршрутизация](modules/02-exchanges-and-routing/overview.ru.md) — default, direct, fanout, topic и headers, alternate exchange
- [03 · Очереди и доставка](modules/03-queues-and-delivery/overview.ru.md) — подтверждения, prefetch, долговечность, TTL, exclusive и auto-delete очереди
- [04 · Шаблоны обмена сообщениями](modules/04-messaging-patterns/overview.ru.md) — work queue, publish/subscribe, routing, RPC

### Часть 2. Продвинутые темы

- [05 · Надёжность](modules/05-reliability/overview.ru.md) — publisher confirms, персистентность, флаг mandatory, транзакции, идемпотентные потребители
- [06 · Dead-lettering и повторы](modules/06-dead-lettering-and-retries/overview.ru.md) — DLX, TTL, отложенные повторы, «ядовитые» сообщения
- [07 · Типы очередей](modules/07-queue-types/overview.ru.md) — classic, quorum, streams
- [08 · Кластеризация и высокая доступность](modules/08-clustering/overview.ru.md) — узлы, кворум, разделение сети, обновления
- [09 · Мониторинг и эксплуатация](modules/09-monitoring/overview.ru.md) — management UI, метрики, Prometheus, тревоги

### Часть 3. Темы для опытных

- [10 · Управление потоком и производительность](modules/10-performance/overview.ru.md) — credit flow, тревоги по памяти и диску, настройка
- [11 · Безопасность](modules/11-security/overview.ru.md) — vhost-ы, права, TLS, аутентификация
- [12 · Federation и Shovel](modules/12-federation-and-shovel/overview.ru.md) — связи между брокерами и дата-центрами
- [13 · Внутреннее устройство](modules/13-internals/overview.ru.md) — среда Erlang, хранилище метаданных, хранилище сообщений
- [14 · RabbitMQ в Kubernetes](modules/14-kubernetes/overview.ru.md) — Cluster Operator, хранилище и обновления

## Библиотеки и инструменты

- [pika](libraries/pika/overview.ru.md) — синхронный клиент для Python
- [aio-pika](libraries/aio-pika/overview.ru.md) — клиент на asyncio
- [MassTransit (C#)](../csharp/libraries/masstransit/overview.ru.md) — клиентская библиотека поверх RabbitMQ для .NET
- [rabbitmqctl](tools/rabbitmqctl/overview.ru.md) — управление узлами и кластером из командной строки

## Рецепты

- [Очередь задач с повторами](recipes/task-queue-with-retries/overview.ru.md) — подтверждения, DLX и backoff
- [RPC поверх RabbitMQ](recipes/rpc/overview.ru.md) — очереди ответов и correlation id
- [Transactional outbox](recipes/outbox/overview.ru.md) — надёжная публикация из транзакции базы данных

## Проекты

- [Конвейер заказов](projects/order-pipeline/overview.ru.md) — несколько сервисов обмениваются событиями через topic-обменники

## Связанные треки

- [Kafka](../kafka/overview.ru.md) — журнал событий вместо очереди: сравнение моделей
- [Python](../python/overview.ru.md) — язык примеров
- [Kubernetes](../kubernetes/overview.ru.md) — запуск брокера через оператор
- [Linux](../linux/overview.ru.md) — сокеты, файловые дескрипторы и диагностика
- [Фоновые задачи в C#](../csharp/recipes/background-jobs/overview.ru.md) — обработка работы в сервисе .NET

## Источники

- [Документация RabbitMQ](https://www.rabbitmq.com/docs)
- [AMQP 0-9-1 model explained](https://www.rabbitmq.com/tutorials/amqp-concepts)
- [Учебники RabbitMQ](https://www.rabbitmq.com/tutorials)
- [Документация pika](https://pika.readthedocs.io/en/stable/)
