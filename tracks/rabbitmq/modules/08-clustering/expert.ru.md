---
title: "Кластеризация и высокая доступность: экспертный уровень"
lang: ru
track: rabbitmq
kind: module
doc: expert
slug: 08-clustering
part: 2
tags: [rabbitmq, cluster, quorum, raft, maintenance-mode, rabbitmq-queues, rabbitmqctl]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Кластеризация и высокая доступность: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. «Проверено» означает наблюдалось на трёхузловом стенде в Docker (RabbitMQ 4.4.1).

## Справочник

### Команды

| Команда | Назначение | Проверено |
|---|---|---|
| `rabbitmqctl stop_app` / `start_app` | остановить/запустить приложение RabbitMQ в узле | да |
| `rabbitmqctl join_cluster <узел>` | присоединить остановленный узел | да |
| `rabbitmqctl cluster_status` | состав, версии, статус обслуживания | да |
| `rabbitmqctl list_queues name type leader members online` | где живут очереди | да |
| `rabbitmqctl set_cluster_name <имя>` | задать имя кластера | вызвана |
| `rabbitmq-queues quorum_status <очередь>` | состояние реплик Raft | да |
| `rabbitmq-upgrade drain` / `revive` | режим обслуживания | да |
| `rabbitmq-diagnostics check_if_node_is_quorum_critical` | критичность узла для кворума | да |
| `rabbitmqctl forget_cluster_node`, `rabbitmq-queues add_member`/`delete_member`/`grow`/`shrink`, `reset` | изменение состава кластера и реплик | не проверялись |

### Матрица отказов (трёхузловой стенд)

| Ситуация | Classic (на отказавшем узле) | Quorum (3 реплики) | Проверено |
|---|---|---|---|
| остановлен 1 узел из 3 | `404 … process is stopped by supervisor` | публикация `ack`, чтение работает | да |
| остановлено 2 узла из 3 | недоступна | публикация без подтверждения (`timeout`) | да |
| узлы возвращены | persistent-сообщение на месте | публикация `ack` | да |

### Поля `quorum_status`

| Поле | Смысл |
|---|---|
| `Raft State` | `leader` / `follower` |
| `Membership` | `voter` |
| `Last Log Index`, `Last Written`, `Last Applied`, `Commit Index` | прогресс журнала Raft |
| `Snapshot Index` | позиция снимка |
| `Term` | номер «эпохи» выборов лидера |
| `Machine Version` | версия машины состояний очереди |

## Внутреннее устройство

- **Метаданные в кластере общие,** данные очередей — нет: classic хранится на одном узле, quorum — на репликах с лидером.
- **Запись в quorum-очередь** подтверждается после принятия большинством реплик (наблюдалось косвенно: с 1 узлом из 3 подтверждения нет, с 2 из 3 — есть).
- **Лидер очереди может быть на любом узле:** в тесте лидер quorum-очереди сначала был на `rmq1`, а после отказов и возвращения узлов оказался на `rmq2`.
- **Режим обслуживания** выводит узел из обслуживания клиентов и передаёт лидерство.
- **Клиентский узел-посредник пересылает** операции на узел, где живёт очередь (проверено на глубине classic-очереди).

> ⚠️ Проверить: детали выборов лидера Raft, поведение при разделении сети, `cluster_partition_handling`, peer discovery и работа кластера в Kubernetes здесь не проверялись.

## Подвохи

### 1. Classic не реплицируется

Отказ узла делает classic-очередь недоступной (`404 … process is stopped by supervisor`); сообщения возвращаются вместе с узлом, если они persistent (проверено).

### 2. Два узла из трёх — граница

Остановка двух из трёх узлов останавливает запись в quorum-очередь (`timeout`).

### 3. Таймаут вместо ошибки

Без большинства публикация не отвергается, а просто не подтверждается: клиент должен ждать подтверждение с таймаутом ([модуль 05](../05-reliability/expert.ru.md#подвохи)).

### 4. Cookie

Узлы кластера должны иметь одинаковый Erlang-cookie; он — секрет.

### 5. Порядок `stop_app` → `join_cluster` → `start_app`

Документация требует остановить приложение (`stop_app`) на присоединяемом узле. Мы выполнили `join_cluster` и без этого на свежем узле: команда напечатала только `Clustering node …`, и итоговый результат без `stop_app` мы не выяснили. Следуйте документированному порядку.

> ⚠️ Проверить: что именно делает `join_cluster` без предварительного `stop_app`, здесь не выяснено.

### 6. Чётное число узлов

Четыре узла переживают столько же отказов, сколько три (большинство — 3 из 4).

## Компромиссы и альтернативы

| Выбор | Плюсы | Минусы |
|---|---|---|
| Кластер из трёх узлов | переживает отказ одного узла | три узла, больше ресурсов |
| Один узел | просто | единая точка отказа |
| Federation / Shovel | связь между независимыми брокерами | не кластер, см. [дорожную карту](../../overview.ru.md) |
| Kubernetes-оператор | автоматизация | отдельный слой ([дорожная карта](../../overview.ru.md)) |

## Вопросы с собеседований

<details><summary>1. Что общего у узлов кластера?</summary>

Метаданные: пользователи, vhost-ы, обменники, очереди, привязки. Данные очередей — на конкретных узлах.

→ [Основы](basics.ru.md#что-такое-кластер)

</details>

<details><summary>2. Подвох: classic-очередь в кластере из трёх узлов реплицируется?</summary>

Нет: она живёт на одном узле и недоступна, когда он отказал.

→ [Подвох 1](#1-classic-не-реплицируется)

</details>

<details><summary>3. Сколько отказов переживает кластер из трёх/пяти узлов?</summary>

Одного / двух: нужно большинство.

→ [Продвинутая теория](advanced.ru.md#сколько-узлов-нужно)

</details>

<details><summary>4. Что происходит с публикацией, если большинства нет?</summary>

Подтверждения нет (`timeout`); сообщение не принимается.

→ [Основы](basics.ru.md#большинство-и-кворум)

</details>

<details><summary>5. Зачем режим обслуживания?</summary>

Безопасно вывести узел из обслуживания перед обновлением или перезагрузкой: клиентов он больше не обслуживает, лидерство передаётся.

→ [Продвинутая теория](advanced.ru.md#режим-обслуживания)

</details>

<details><summary>6. Подвох: двухузловой кластер надёжнее одного узла?</summary>

Нет: отказ любого узла лишает большинства.

→ [Продвинутая теория](advanced.ru.md#сколько-узлов-нужно)

</details>

<details><summary>7. Как узнать, можно ли остановить узел?</summary>

`rabbitmq-diagnostics check_if_node_is_quorum_critical`.

→ [Продвинутая теория](advanced.ru.md#критичность-узла-для-кворума)

</details>

<details><summary>8. Как присоединить узел к кластеру?</summary>

`stop_app`, `join_cluster rabbit@первый`, `start_app`.

→ [Основы](basics.ru.md#что-такое-кластер)

</details>

<details><summary>9. Подвох: клиент подключён к `rmq2`, очередь classic на `rmq1`. Это работает?</summary>

Да, узел `rmq2` пересылает операции (глубина прочиталась); но при отказе `rmq1` очередь недоступна.

→ [Основы](basics.ru.md#где-живёт-очередь)

</details>

<details><summary>10. Как посмотреть, где лидер quorum-очереди?</summary>

`rabbitmqctl list_queues name type leader members` или `rabbitmq-queues quorum_status`.

→ [Справочник](#команды)

</details>

<details><summary>11. Почему нечётное число узлов?</summary>

Чётное не добавляет отказоустойчивости: 4 узла переживают столько же, сколько 3.

→ [Подвох 6](#6-чётное-число-узлов)

</details>

<details><summary>12. Подвох: после возвращения узла пропало ли persistent-сообщение classic-очереди?</summary>

Нет: `classic message back: 1`.

→ [Основы](basics.ru.md#отказ-узла)

</details>

## Шпаргалка

```text
join      : stop_app ; join_cluster rabbit@first ; start_app
status    : cluster_status ; list_queues name type leader members ; rabbitmq-queues quorum_status Q
maintain  : check_if_node_is_quorum_critical ; rabbitmq-upgrade drain ; ... ; revive
majority  : nodes/2 + 1  (3 -> 2, 5 -> 3)
```

- Quorum для важного, три и более нечётных узлов.
- Без большинства записи нет: ждите подтверждение с таймаутом.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Раньше: [07 · Типы очередей](../07-queue-types/expert.ru.md)
- Следующий модуль: [09 · Мониторинг и эксплуатация](../09-monitoring/overview.ru.md)

## Источники

- [Clustering Guide](https://www.rabbitmq.com/docs/clustering)
- [Quorum Queues](https://www.rabbitmq.com/docs/quorum-queues)
- [Upgrading RabbitMQ](https://www.rabbitmq.com/docs/upgrade)
