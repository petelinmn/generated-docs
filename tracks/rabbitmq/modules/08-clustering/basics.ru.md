---
title: "Кластеризация и высокая доступность: основы"
lang: ru
track: rabbitmq
kind: module
doc: basics
slug: 08-clustering
part: 2
tags: [rabbitmq, cluster, high-availability, quorum-queue, node-failure, join-cluster]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Кластеризация и высокая доступность: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [07 · Типы очередей](../07-queue-types/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Кластер из трёх узлов собран в Docker: сеть `rmqnet`, контейнеры `rmq1`, `rmq2`, `rmq3` (образ `rabbitmq:4-management`, RabbitMQ 4.4.1, Erlang 29.1.2) с одинаковым Erlang-cookie. Порты AMQP узлов проброшены как 5681, 5682, 5683. Клиент — pika 1.5.0. Всё ниже — фактические выводы; **это учебный стенд на одной машине**: он показывает логику отказов, но не настоящие сетевые сбои между машинами. Помощник `conn(port)` открывает соединение с узлом, `Pub` — публикация с подтверждением из [модуля 05](../05-reliability/basics.ru.md#как-проверялся-код).

```bash
docker network create rmqnet
docker run -d --name rmq1 --hostname rmq1 --network rmqnet \
  -e RABBITMQ_ERLANG_COOKIE=secretcookie -e RABBITMQ_NODENAME=rabbit@rmq1 \
  -p 5681:5672 rabbitmq:4-management
# the same for rmq2 (5682) and rmq3 (5683)
```

> Cookie в примере учебный. В реальной системе это секрет: кто его знает, тот управляет узлом.

## Что такое кластер

Кластер — несколько узлов RabbitMQ, объединённых в единую логическую систему: у них общие метаданные (пользователи, vhost-ы, обменники, очереди, привязки), и клиент может подключиться к любому узлу. Узлы присоединяются командами:

```bash
docker exec rmq2 rabbitmqctl stop_app
docker exec rmq2 rabbitmqctl join_cluster rabbit@rmq1
docker exec rmq2 rabbitmqctl start_app
```

```text
Clustering node rabbit@rmq2 with rabbit@rmq1
```

`rabbitmqctl cluster_status` показывает состав:

```text
Disk Nodes
rabbit@rmq1
rabbit@rmq2
rabbit@rmq3

Running Nodes
rabbit@rmq1
rabbit@rmq2
rabbit@rmq3
```

## Где живёт очередь

Метаданные общие, но **сама очередь** живёт на конкретных узлах. Classic-очередь — на одном (где объявлена); quorum — на нескольких, список видно в `list_queues`:

```text
name  type     leader        members
cl    classic  rabbit@rmq1   [rabbit@rmq1]
qu    quorum   rabbit@rmq2   [rabbit@rmq2, rabbit@rmq3, rabbit@rmq1]
```

Клиент может работать с очередью через любой узел: подключение к `rmq2` показало глубину classic-очереди с `rmq1`:

```text
via node2 depth: 1
```

Узел-посредник пересылает операции на узел, где живёт очередь.

## Отказ узла

Остановим `rmq1` (`docker stop rmq1`), где живёт classic-очередь `cl`, а quorum-очередь `qu` реплицирована на все три узла.

```text
classic while node1 down: ChannelClosedByBroker (404, "NOT_FOUND - queue 'cl' in vhost '/' process is stopped by supervisor")
quorum depth with node1 down: 2
publish to quorum with node1 down: ack
consumed: ['q1', 'q2', 'q3']
```

- **Classic:** очередь недоступна, пока жив только `rmq3` (404). После `docker start rmq1` persistent-сообщение вернулось: `classic message back: 1`.
- **Quorum:** публикация и чтение продолжают работать на двух оставшихся узлах, потерянных сообщений нет (`q1`, `q2` и новое `q3`).

Вот в чём смысл quorum-очередей: они переживают отказ узла.

## Большинство и кворум

Quorum-очередь работает, пока доступно **большинство** реплик (2 из 3). Остановим ещё и `rmq2`, оставив 1 узел из 3:

```text
publish with 1 of 3 nodes: timeout
```

Подтверждения нет: без большинства очередь не принимает записи. После запуска узлов:

```text
publish after recovery: ack
```

## Типичные ошибки новичков

- ❌ Считать, что кластер автоматически реплицирует все очереди → ✅ classic остаётся на одном узле, реплицируются quorum и stream.
- ❌ Кластер из двух узлов «для надёжности» → ✅ при отказе одного из двух большинства нет; нужны три и более.
- ❌ Подключать клиентов только к одному узлу → ✅ список узлов или балансировщик, реконнект.
- ❌ Оставлять учебный Erlang-cookie → ✅ секрет, одинаковый на узлах кластера.
- ❌ Считать, что кластер защищает от потери данных, если сообщения не persistent → ✅ durable + persistent + quorum.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — обслуживание узлов, критичность кворума
- Раньше: [07 · Типы очередей](../07-queue-types/basics.ru.md)
- Следующий модуль: [09 · Мониторинг и эксплуатация](../09-monitoring/overview.ru.md)
- Аналог в другом треке: [Kubernetes](../../../kubernetes/overview.ru.md)

## Источники

- [Clustering Guide](https://www.rabbitmq.com/docs/clustering)
- [Quorum Queues](https://www.rabbitmq.com/docs/quorum-queues)
- [Reliability Guide](https://www.rabbitmq.com/docs/reliability)
