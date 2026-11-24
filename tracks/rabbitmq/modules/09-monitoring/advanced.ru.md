---
title: "Мониторинг и эксплуатация: продвинутый уровень"
lang: ru
track: rabbitmq
kind: module
doc: advanced
slug: 09-monitoring
part: 2
tags: [rabbitmq, prometheus, alarms, memory-alarm, connection-blocked, health-check, alerting]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Мониторинг и эксплуатация: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Код запускался на одном узле RabbitMQ 4.4.1 с pika 1.5.0.

## Prometheus

Плагин `rabbitmq_prometheus` отдаёт метрики в текстовом формате Prometheus на порту 15692. В образе `rabbitmq:4-management` он уже включён (`rabbitmq-plugins enable rabbitmq_prometheus` ответил «Plugin configuration unchanged»).

```text
metric rabbitmq_queue_messages_ready -> rabbitmq_queue_messages_ready 7
metric rabbitmq_queue_messages_unacked -> rabbitmq_queue_messages_unacked 3
metric rabbitmq_queue_consumers -> rabbitmq_queue_consumers 1
metric rabbitmq_connections -> rabbitmq_connections 2
metric rabbitmq_channels -> rabbitmq_channels 2
metric rabbitmq_process_resident_memory_bytes -> rabbitmq_process_resident_memory_bytes 149659648
metric rabbitmq_disk_space_available_bytes -> rabbitmq_disk_space_available_bytes 993590013952
metric rabbitmq_identity_info -> rabbitmq_identity_info{rabbitmq_node="rabbit@4ff649c7c4dc",rabbitmq_cluster="rabbit@4ff649c7c4dc",ra…
```

- Адрес `/metrics` по умолчанию отдаёт **сводные** значения по узлу (суммы по очередям): `rabbitmq_queue_messages_ready 7`.
- Адрес `/metrics/per-object` отдаёт значения **по каждой очереди** с метками: `rabbitmq_queue_messages_ready{vhost="/",queue="mon"} 7`. Метрик больше, поэтому на больших установках им пользуются осознанно.
- Метки `rabbitmq_node` и `rabbitmq_cluster` в `rabbitmq_identity_info` позволяют различать узлы и кластеры.

Prometheus собирает метрики по расписанию (scrape) и хранит историю, а тревоги и графики строят в Alertmanager и Grafana.

> ⚠️ Проверить: готовые панели Grafana для RabbitMQ и конфигурация Prometheus/Alertmanager здесь не проверялись.

## Оповещения памяти и диска

Брокер защищает себя: при нехватке памяти или места на диске он включает **оповещение** (alarm) и блокирует публикующие соединения. Опыт: порог памяти опустили до 10 МБ (`rabbitmqctl set_vm_memory_high_watermark absolute 10MB`) и опубликовали сообщение.

```text
health alarms: (503, {'reason': 'There are alarms in effect in the cluster', 'status': 'failed', 'alarms': [{'node': 'rabbit@4ff649c7c4dc', 'resource': 'memory'}]})
blocked callback reasons: [<METHOD([... "method=<Connection.Blocked(['reason=low on memory'])>"])>]
conn states: ['blocking']
```

- `GET /api/health/checks/alarms` ответил `503` и перечислил узел и ресурс (`memory`).
- Клиент получил `Connection.Blocked` с причиной `low on memory`; для публикующего соединения брокер сообщает об этом через обратный вызов `add_on_connection_blocked_callback`.
- Состояние соединения в API — `blocking`.

После возврата порога (`set_vm_memory_high_watermark 0.6`):

```text
health alarms after: (200, {'status': 'ok'})
conn states after: ['running']
```

Клиент должен реагировать на `blocked`: не копить сообщения в памяти и повторять публикацию после `unblocked`.

> `rabbitmq-diagnostics check_local_alarms` в 4.4.1 печатает «This command is DEPRECATED and is a no-op» и не показывает тревогу даже при активной. При активной тревоге по памяти (проверено) HTTP-проверка отвечала `503`, а `rabbitmq-diagnostics alarms` печатала `Memory alarm on node …`. Используйте их.

## Что тревожить

Практика (из документации и опыта эксплуатации), каждое требует настройки порогов под вашу нагрузку:

| Сигнал | Причина тревоги |
|---|---|
| `messages_ready` растёт | потребители не успевают |
| `messages_unacknowledged` велик и не падает | зависший потребитель или слишком большой prefetch |
| `consumers = 0` у важной очереди | никто не читает |
| оповещения памяти/диска | брокер блокирует издателей |
| число соединений/каналов растёт | утечка в приложении |
| очередь dead-letter не пуста | проблемные сообщения ([модуль 06](../06-dead-lettering-and-retries/advanced.ru.md)) |

> ⚠️ Проверить: конкретные пороги, рекомендованные наборы правил и список критичных метрик здесь не проверялись и зависят от нагрузки.

## Лучшие практики

- Мониторить очереди, узлы и приложения вместе; называть соединения.
- Собирать метрики в Prometheus; для больших установок осознанно включать per-object.
- Проверять `/api/health/checks/alarms` в проверках живости и готовности.
- Реагировать в клиенте на `Connection.Blocked`.
- Тревожить по тренду (рост глубины), а не только по порогу.

## Частые ошибки

- ❌ Полагаться на `check_local_alarms` → ✅ HTTP health check (проверено: команда — no-op).
- ❌ Читать per-object метрики с огромным числом очередей без нужды → ✅ сводные.
- ❌ Игнорировать `Connection.Blocked` в клиенте → ✅ приостановить публикацию.
- ❌ Тревога только на глубину → ✅ ещё и на `consumers`, unacked, DLQ.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- [05 · Надёжность](../05-reliability/advanced.ru.md) — поведение издателя при блокировке
- Следующий модуль: [10 · Управление потоком и производительность](../10-performance/overview.ru.md)

## Источники

- [Monitoring with Prometheus and Grafana](https://www.rabbitmq.com/docs/prometheus)
- [Alarms](https://www.rabbitmq.com/docs/alarms)
- [Memory Alarms / Memory Use](https://www.rabbitmq.com/docs/memory)
- [Connection Blocked notifications](https://www.rabbitmq.com/docs/connection-blocked)
