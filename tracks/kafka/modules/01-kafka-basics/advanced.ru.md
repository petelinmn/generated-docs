---
title: "Основы Kafka: продвинутый уровень"
lang: ru
track: kafka
kind: module
doc: advanced
slug: 01-kafka-basics
part: 1
tags: [kafka, kraft, broker, consumer-group, committed-offset, lag, internal-topics]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Основы Kafka: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Код запускался на Apache Kafka 4.3.1 (KRaft, один узел) с `confluent-kafka` 2.16.2.

## Что такое брокер и режим KRaft

**Брокер** (broker) — процесс Kafka, хранящий партиции и обслуживающий клиентов. Кластер — несколько брокеров. Метаданные кластера (какие темы, где партиции, кто лидер) хранит **контроллер**; в современных версиях это встроенный кворум **KRaft** (на основе Raft), без отдельного ZooKeeper.

Конфигурация образа `apache/kafka:4.3.1` (из `/opt/kafka/config/server.properties`):

```text
process.roles=broker,controller
node.id=1
listeners=PLAINTEXT://:9092,CONTROLLER://:9093
log.dirs=/tmp/kraft-combined-logs
num.partitions=1
log.retention.hours=168
log.segment.bytes=1073741824
offsets.topic.replication.factor=1
```

- `process.roles=broker,controller` — один процесс выполняет обе роли (учебный «комбинированный» режим).
- Клиенты подключаются к `9092`, а контроллер слушает `9093`.
- Тема по умолчанию получает 1 партицию, записи хранятся 168 часов (7 суток), сегмент журнала — 1 ГиБ.
- `/tmp/kraft-combined-logs` — каталог данных: при удалении контейнера данные исчезнут.

Состояние кворума:

```text
$ kafka-metadata-quorum.sh --bootstrap-server localhost:9092 describe --status
ClusterId:              5L6g3nShT-eMCtK--X86sw
LeaderId:               1
LeaderEpoch:            1
HighWatermark:          70
MaxFollowerLag:         0
MaxFollowerLagTimeMs:   0
```

> ⚠️ Проверить: назначение `LeaderEpoch` и `HighWatermark` кворума (в общих чертах — номер «эпохи» выбора лидера и позиция подтверждённых записей метаданных) взято из документации и здесь не проверялось.

## Внутренние темы

В каталоге данных, кроме `orders-*`, лежат служебные каталоги:

```text
__cluster_metadata-0
__consumer_offsets-0
__consumer_offsets-1
...
```

- `__cluster_metadata-0` — журнал метаданных кластера (KRaft).
- `__consumer_offsets-N` — внутренняя тема, в которую Kafka записывает **зафиксированные смещения групп потребителей**. Позиция читателя хранится в самой Kafka.

## Позиции групп потребителей

Группа `grp-d78c2` прочитала тему с `earliest` и зафиксировала смещения (commit). `kafka-consumer-groups.sh --describe`:

```text
GROUP      TOPIC   PARTITION  CURRENT-OFFSET  LOG-END-OFFSET  LAG  CONSUMER-ID  HOST  CLIENT-ID
grp-d78c2  orders  1          3               3               0    -            -     -
grp-d78c2  orders  2          4               4               0    -            -     -

Consumer group 'grp-d78c2' has no active members.
```

- `CURRENT-OFFSET` — зафиксированная позиция группы (следующая запись к чтению), `LOG-END-OFFSET` — конец журнала, `LAG` — разница: сколько записей группа ещё не прочитала.
- Активных участников нет (`-`), но позиции сохранены: группа, запущенная снова, продолжит с зафиксированных смещений:

```text
same group again (offsets committed): []
```

Повторный запуск не вернул ничего: всё уже прочитано, а позиция не сбросилась в `earliest`. Параметр `auto.offset.reset` действует **только** когда у группы нет сохранённой позиции.

## Описание темы

```text
Topic: orders  TopicId: MtHLzlWzRhyNBS79_B02cg  PartitionCount: 3  ReplicationFactor: 1  Configs: min.insync.replicas=1,segment.bytes=1073741824
  Partition: 0  Leader: 1  Replicas: 1  Isr: 1  Elr:   LastKnownElr:
```

`Leader` — брокер, обслуживающий запись и чтение партиции; `Replicas` — брокеры с копиями; `Isr` (in-sync replicas) — копии, не отстающие от лидера. Подробнее — [модуль 07](../../overview.ru.md).

## Лучшие практики

- Называйте темы по смыслу и соглашению (например, `orders.created`), планируйте число партиций заранее: уменьшить его нельзя ([модуль 04](../04-topics-and-retention/overview.ru.md)).
- Всегда вызывайте `flush()` перед выходом и проверяйте ошибки в обратном вызове доставки.
- Группам задавайте осмысленные `group.id`: позиции хранятся по ним.
- Следите за `LAG` ([модуль 09](../../overview.ru.md)).

## Частые ошибки

- ❌ Полагаться на `auto.offset.reset` для группы с сохранённой позицией → ✅ он на неё не влияет.
- ❌ Общий `group.id` у несвязанных приложений → ✅ они начнут делить партиции.
- ❌ Хранить данные брокера в `/tmp` контейнера в рабочей системе → ✅ том или диск.
- ❌ Не смотреть `LAG` → ✅ мониторить отставание.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Следующий модуль: [02 · Производители](../02-producers/overview.ru.md)

## Источники

- [Apache Kafka: Configuration](https://kafka.apache.org/documentation/#configuration)
- [Apache Kafka: KRaft](https://kafka.apache.org/documentation/#kraft)
- [Apache Kafka: Managing consumer groups](https://kafka.apache.org/documentation/#basic_ops_consumer_group)
