---
title: "Мониторинг и эксплуатация: экспертный уровень"
lang: ru
track: rabbitmq
kind: module
doc: expert
slug: 09-monitoring
part: 2
tags: [rabbitmq, monitoring, management-api, prometheus, alarms, rabbitmq-diagnostics, health-check]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Мониторинг и эксплуатация: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. «Проверено» означает наблюдалось на одном узле RabbitMQ 4.4.1.

## Справочник

### HTTP API управления

| Запрос | Что возвращает | Проверено |
|---|---|---|
| `GET /api/overview` | версия, имя кластера, `queue_totals`, `message_stats` | да |
| `GET /api/queues/{vhost}/{очередь}` | `messages`, `messages_ready`, `messages_unacknowledged`, `consumers`, `state`, `type`, `message_stats` | да |
| `GET /api/connections` | соединения; `client_properties`, `state` | да |
| `GET /api/channels` | каналы; `prefetch_count`, `messages_unacknowledged` | да |
| `GET /api/consumers` | потребители; `queue`, `prefetch_count`, `ack_required` | да |
| `GET /api/nodes` | узлы: память, диск, флаги тревог | вызван |
| `GET /api/health/checks/alarms` | `200 ok` или `503` с перечнем тревог | да |
| другие `GET /api/health/checks/*`, `/api/definitions`, `/api/vhosts`, `/api/users` | проверки, экспорт определений | не проверялись |

Имя vhost `/` кодируется как `%2F`.

### Командная строка

| Команда | Назначение | Проверено |
|---|---|---|
| `rabbitmqctl list_queues name messages_ready messages_unacknowledged consumers` | сводка по очередям | да |
| `rabbitmqctl list_connections` / `list_channels` / `list_consumers` | кто подключён | не проверялись |
| `rabbitmqctl set_vm_memory_high_watermark <доля>` / `absolute <размер>` | порог памяти | да |
| `rabbitmq-diagnostics check_running` | узел запущен | да |
| `rabbitmq-diagnostics check_local_alarms` | **устарела, ничего не делает** в 4.4.1 | да |
| `rabbitmq-diagnostics status`, `memory_breakdown`, `ping` | состояние и разбор памяти | не проверялись |

### Метрики Prometheus (порт 15692)

| Адрес | Содержание |
|---|---|
| `/metrics` | сводные метрики узла: `rabbitmq_queue_messages_ready`, `rabbitmq_queue_messages_unacked`, `rabbitmq_queue_consumers`, `rabbitmq_connections`, `rabbitmq_channels`, `rabbitmq_process_resident_memory_bytes`, `rabbitmq_disk_space_available_bytes`, `rabbitmq_identity_info` (все проверены) |
| `/metrics/per-object` | значения по каждой очереди с метками `vhost`, `queue` (проверен `rabbitmq_queue_messages_ready`) |
| другие пути (`/metrics/detailed`) | расширенные метрики | не проверялись |

## Внутреннее устройство

- **Статистика management собирается периодически,** поэтому числа отстают от реальности на секунды; в опыте пришлось ждать 6 секунд.
- **Оповещения памяти/диска останавливают издателей:** брокер отправляет `Connection.Blocked`, соединение в API — `blocking` (проверено); после снятия оповещения — `Connection.Unblocked` и `running` (состояние проверено).
- **`messages = ready + unacked`:** неподтверждённые остаются у очереди, пока потребитель не подтвердит или не отвалится.
- **Prometheus «вытягивает» метрики** (pull), поэтому брокеру не нужно знать адрес сервера мониторинга.

> ⚠️ Проверить: внутреннее устройство сбора статистики (интервал обновления по умолчанию), точная семантика порогов памяти при процентном/абсолютном значении и поведение `unblocked` как клиентского события здесь не проверялись.

## Подвохи

### 1. `check_local_alarms` — ничего не делает

Команда в 4.4.1 печатает, что она «DEPRECATED and is a no-op», и сообщает «no local alarms» при действующей тревоге (проверено). Используйте `/api/health/checks/alarms` (503) или `rabbitmq-diagnostics alarms` (`Memory alarm on node …`) — обе проверены при активной тревоге.

### 2. Числа API отстают

Сразу после публикации `messages_ready` в management может быть старым.

### 3. `/metrics` не показывает очереди по отдельности

Нужен `/metrics/per-object` (проверено: метка `queue="mon"` только там).

### 4. `guest`/`guest`

Учебный образ принял эти данные через проброшенный порт; в продакшене такой пользователь — дыра ([модуль 11](../../overview.ru.md)).

### 5. Тревога блокирует издателей, а не потребителей

Блокируются публикующие соединения; чтение продолжается ([документация](https://www.rabbitmq.com/docs/alarms)).

### 6. Клиент должен реагировать на `Connection.Blocked`

Pika вызвал обратный вызов с причиной `low on memory` (проверено).

## Компромиссы и альтернативы

| Выбор | Плюсы | Минусы |
|---|---|---|
| Management UI/API | просто, наглядно | нагрузка на узел, задержка статистики |
| Prometheus + Grafana | история, тревоги, стандарт | отдельная инфраструктура |
| per-object метрики | видно каждую очередь | много серий |
| `rabbitmqctl` в скриптах | без HTTP | нужен доступ к узлу |

## Вопросы с собеседований

<details><summary>1. Какие показатели очереди проверить первыми?</summary>

`messages_ready`, `messages_unacknowledged`, `consumers`, а также `state`.

→ [Основы](basics.ru.md#что-смотреть-в-первую-очередь)

</details>

<details><summary>2. Подвох: `messages` = 10, `messages_ready` = 7. Что остальные три?</summary>

Неподтверждённые: их держит потребитель (`messages_unacknowledged`).

→ [Основы](basics.ru.md#http-api-управления)

</details>

<details><summary>3. Как посмотреть очереди без HTTP?</summary>

`rabbitmqctl list_queues name messages_ready messages_unacknowledged consumers`.

→ [Основы](basics.ru.md#командная-строка)

</details>

<details><summary>4. Как найти приложение по соединению?</summary>

Задать `connection_name` в `client_properties` и искать в `/api/connections`.

→ [Основы](basics.ru.md#соединения-каналы-потребители)

</details>

<details><summary>5. В чём разница `/metrics` и `/metrics/per-object`?</summary>

Первый — сводные метрики узла, второй — значения по каждой очереди с метками.

→ [Продвинутая теория](advanced.ru.md#prometheus)

</details>

<details><summary>6. Подвох: `check_local_alarms` показал «no alarms». Значит ли это, что тревог нет?</summary>

Нет: в 4.4.1 команда устарела и ничего не делает. Проверяйте `/api/health/checks/alarms`.

→ [Подвох 1](#1-check_local_alarms--ничего-не-делает)

</details>

<details><summary>7. Что происходит при тревоге по памяти?</summary>

Брокер блокирует публикующие соединения: клиент получает `Connection.Blocked` (`low on memory`), health check отвечает 503.

→ [Продвинутая теория](advanced.ru.md#оповещения-памяти-и-диска)

</details>

<details><summary>8. Как клиенту реагировать на блокировку?</summary>

Подписаться на обратный вызов blocked/unblocked, приостановить публикацию и не копить сообщения в памяти.

→ [Продвинутая теория](advanced.ru.md#оповещения-памяти-и-диска)

</details>

<details><summary>9. Подвох: почему числа в management не совпадают с только что считанными?</summary>

Статистика обновляется периодически, поэтому она отстаёт.

→ [Подвох 2](#2-числа-api-отстают)

</details>

<details><summary>10. На что тревожить в первую очередь?</summary>

Рост `messages_ready`, большой неубывающий `messages_unacknowledged`, `consumers = 0`, тревоги памяти/диска, непустая очередь DLQ.

→ [Продвинутая теория](advanced.ru.md#что-тревожить)

</details>

<details><summary>11. Подвох: имя vhost в URL API?</summary>

`/` кодируется как `%2F`.

→ [Справочник](#http-api-управления)

</details>

<details><summary>12. Почему Prometheus не требует настройки на брокере?</summary>

Он сам обращается к порту 15692 (pull); плагин лишь публикует метрики.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

## Шпаргалка

```text
queue     : GET /api/queues/%2F/<q> -> messages_ready, messages_unacknowledged, consumers, state
health    : GET /api/health/checks/alarms   (200 ok | 503 + alarms)   # check_local_alarms is a no-op in 4.4.1
prom      : :15692/metrics (aggregated)  :15692/metrics/per-object (per queue)
blocked   : Connection.Blocked(reason="low on memory") -> pause publishing
```

- Следите за `ready`, `unacked` и `consumers`.
- Проверка тревог — через HTTP.
- Называйте соединения.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Раньше: [08 · Кластеризация](../08-clustering/expert.ru.md)
- Следующий модуль: [10 · Управление потоком и производительность](../../overview.ru.md)

## Источники

- [Management Plugin](https://www.rabbitmq.com/docs/management)
- [Monitoring](https://www.rabbitmq.com/docs/monitoring)
- [Prometheus](https://www.rabbitmq.com/docs/prometheus)
- [Alarms](https://www.rabbitmq.com/docs/alarms)
