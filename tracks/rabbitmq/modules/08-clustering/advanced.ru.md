---
title: "Кластеризация и высокая доступность: продвинутый уровень"
lang: ru
track: rabbitmq
kind: module
doc: advanced
slug: 08-clustering
part: 2
tags: [rabbitmq, cluster, maintenance-mode, rolling-upgrade, quorum, network-partition]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Кластеризация и высокая доступность: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md) (тот же трёхузловой стенд в Docker). Всё ниже получено запуском на RabbitMQ 4.4.1, если не сказано иное.

## Сколько узлов нужно

Quorum-очередь продолжает работу, пока доступно большинство реплик: больше половины от числа реплик.

| Узлов | Большинство | Переживает отказов |
|---|---|---|
| 1 | 1 | 0 |
| 2 | 2 | 0 |
| 3 | 2 | 1 |
| 5 | 3 | 2 |

Двухузловой кластер не лучше одного: отказ любого узла отнимает большинство. Чётное число узлов ничего не добавляет по сравнению с предыдущим нечётным. В тесте с тремя узлами остановка одного не мешала публикации (`ack`), а остановка двух останавливала запись (`timeout`) до возвращения узлов.

## Режим обслуживания

Перед обновлением или перезагрузкой узел переводят в режим обслуживания: он перестаёт принимать клиентов и отдаёт лидерство.

```text
$ rabbitmq-upgrade drain
Will put node rabbit@rmq3 into maintenance mode. The node will no longer serve any client traffic!

Node: rabbit@rmq3, status: marked for maintenance

$ rabbitmq-upgrade revive
Will put node rabbit@rmq3 back into regular operating mode. The node will again serve client traffic and host primary queue replicas.

Node: rabbit@rmq3, status: not under maintenance
```

Статус виден в `cluster_status` в разделе `Maintenance status`. Типовое последовательное обновление: для каждого узла `drain`, остановка, обновление, запуск, `revive`, затем следующий.

> ⚠️ Проверить: полная процедура последовательного (rolling) обновления между версиями и требования к совместимости версий здесь не проверялись; воспроизведён только `drain`/`revive`.

## Критичность узла для кворума

Перед остановкой узла проверьте, не лишит ли это какую-нибудь очередь большинства:

```text
$ rabbitmq-diagnostics check_if_node_is_quorum_critical
Checking if node rabbit@rmq3 is critical for quorum of any queues/streams ...
Node rabbit@rmq3 reported no queues/streams with minimum quorum
```

Если у очередей есть `minimum quorum`, остановка узла лишит их большинства: сначала верните остальные узлы или перенесите реплики.

## Состояние реплик

`rabbitmq-queues quorum_status <очередь>` печатает таблицу: узел, состояние Raft (`leader`/`follower`), тип членства (`voter`), индексы журнала (`Last Log Index`, `Commit Index` …) и `Term`:

```text
Node Name     Raft State  Membership  Last Log Index  Last Written  Last Applied  Commit Index  Snapshot Index  Term  Machine Version
rabbit@rmq1   leader      voter       2               2             2             2             -1              1     9
rabbit@rmq2   follower    voter       2               2             2             2             -1              1     9
```

(Строка третьего узла опущена.) Совпадающие индексы означают, что реплики синхронны.

## Клиенты и отказ узла

После отказа узла соединения с ним рвутся. Клиент обязан переподключиться к другому узлу и повторить неподтверждённые операции ([модуль 05](../05-reliability/advanced.ru.md)): готового «автоматического переподключения» в `pika.Connection` мы не проверяли. Типичные схемы: список узлов в клиенте или балансировщик перед кластером.

> ⚠️ Проверить: автоматический реконнект pika и поведение балансировщика при отказе узла здесь не проверялись.

## Сетевые разделения

Разделение сети (network partition) делит кластер на группы узлов, которые не видят друг друга. Quorum-очереди продолжают работать только в группе, где есть большинство.

> ⚠️ Проверить: сетевое разделение здесь не воспроизводилось (на одной машине остановка контейнера — не то же самое), настройка `cluster_partition_handling` и поведение classic-очередей при разделении не проверялись.

## Лучшие практики

- Три или пять узлов (нечётное число), на разных хостах или зонах.
- Важные очереди — quorum, durable-сообщения persistent.
- Перед обслуживанием — `check_if_node_is_quorum_critical`, затем `drain`.
- Клиенты знают все узлы и умеют переподключаться.
- Обновления по одному узлу, наблюдая за синхронностью реплик.

## Частые ошибки

- ❌ Два узла → ✅ три и больше.
- ❌ Остановка узла без проверки критичности → ✅ `check_if_node_is_quorum_critical`.
- ❌ Все узлы на одном хосте/в одной зоне → ✅ раздельные домены отказа.
- ❌ classic-очередь как «высокодоступная» → ✅ quorum.
- ❌ Ждать, что клиент сам «починится» → ✅ реконнект и повтор.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- [07 · Типы очередей](../07-queue-types/advanced.ru.md)
- [05 · Надёжность](../05-reliability/advanced.ru.md) — повтор неподтверждённых публикаций
- Следующий модуль: [09 · Мониторинг и эксплуатация](../09-monitoring/overview.ru.md)

## Источники

- [Clustering Guide](https://www.rabbitmq.com/docs/clustering)
- [Upgrading RabbitMQ](https://www.rabbitmq.com/docs/upgrade)
- [Quorum Queues](https://www.rabbitmq.com/docs/quorum-queues)
