---
title: "Репликация и доступность: базовый уровень"
lang: ru
track: kafka
kind: module
doc: basics
slug: 07-replication-and-availability
part: 2
tags: [kafka, replication, leader, isr, min-insync-replicas, acks, high-availability]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Репликация и доступность: базовый уровень

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Основы Kafka](../01-kafka-basics/basics.ru.md), [02 · Производители](../02-producers/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Код запускался на кластере из **трёх брокеров** Apache Kafka 4.3.1 (KRaft, образ `apache/kafka:4.3.1`, контейнеры `kafka1`, `kafka2`, `kafka3` в сети Docker `kfk`, порты хоста 9091–9093) с `confluent-kafka` 2.16.2; выводы скопированы из запусков. Каждый брокер совмещает роли брокера и контроллера (`process.roles=broker,controller`), кворум контроллеров — три узла. Брокеры останавливались и запускались командами `docker stop`/`docker start`. **Это учебный стенд на одной машине:** он показывает логику отказов, но не сетевые сбои между машинами. Помощники: `fresh(topic, parts=3, rf=3, **cfg)` пересоздаёт тему; `state(topic)` разбирает вывод `kafka-topics.sh --describe`; `send(producer, topic, n)` публикует `n` записей и возвращает результаты из обратных вызовов (`ok` или имя ошибки).

## Репликация

У каждой партиции есть **лидер** (leader) и **фолловеры** (followers): копии на других брокерах. Все записи и чтения идут через лидера, а фолловеры подтягивают данные с него. Число копий задаёт **коэффициент репликации** (replication factor, RF).

```text
p0 leader=3 replicas=3,1,2 isr=3,1,2
p1 leader=1 replicas=1,2,3 isr=1,2,3
p2 leader=2 replicas=2,3,1 isr=2,3,1
```

Тема с тремя партициями и RF=3:

- `leader` — брокер, обслуживающий партицию: лидеры **распределены** по брокерам (3, 1, 2).
- `replicas` — брокеры с копиями; первый в списке — **предпочтительный лидер**.
- `isr` — **синхронные реплики** (in-sync replicas): копии, не отстающие от лидера. Здесь все три.

## Отказ лидера

Остановим брокера 3 — лидера партиции 0:

```text
kafka3
p0 leader now: 1 replicas: [3, 1, 2] isr: [1, 2]
acks=all, 2 of 3 alive: ['ok', 'ok', 'ok']
```

За несколько секунд лидером партиции 0 стал брокер 1, из ISR выбыл брокер 3 (`isr: [1, 2]`), а запись с `acks=all` продолжила работать. Клиенты обновляют метаданные и находят нового лидера сами.

## `acks` и `min.insync.replicas`

Надёжность записи определяют две настройки ([модуль 02](../02-producers/basics.ru.md#подтверждения-брокера-acks)):

- `acks=all` — запись подтверждается, когда её приняли **все синхронные реплики**;
- `min.insync.replicas` (настройка темы; здесь 2) — **минимальное** число синхронных реплик, при котором запись с `acks=all` вообще принимается.

Вместе: при RF=3 и `min.insync.replicas=2` запись подтверждена минимум на двух брокерах, а кластер переживает потерю **одного** брокера без остановки записи.

Остановим второго брокера (остался один из трёх):

```text
acks=all, 1 of 3 alive: ['_MSG_TIMED_OUT', '_MSG_TIMED_OUT']
acks=1 , 1 of 3 alive: ['ok', 'ok']
```

- С `acks=all` записи **не принимаются**: синхронных реплик меньше двух, и клиент в итоге получает `_MSG_TIMED_OUT`.
- С `acks=1` записи проходят — лидер ещё жив, но они **не защищены**: подтвердил только лидер, и его отказ потеряет их.

## Восстановление

После запуска брокеров реплики догнали лидеров и вернулись в ISR:

```text
p0 leader=2 replicas=[3, 1, 2] isr=[1, 2, 3]
p1 leader=2 replicas=[1, 2, 3] isr=[1, 2, 3]
p2 leader=2 replicas=[2, 3, 1] isr=[1, 2, 3]
under-replicated: (none)
```

ISR полный, недореплицированных партиций нет. Лидеры при этом **не вернулись** к предпочтительным: лидером всех трёх партиций остался брокер 2.

## Типичные ошибки новичков

- ❌ RF=1 в рабочей системе → ✅ минимум RF=3 для важных тем.
- ❌ `acks=all` при `min.insync.replicas=1` → ✅ `all` защищает только вместе с `min.insync.replicas` ≥ 2.
- ❌ `acks=1` для важных данных → ✅ запись, подтверждённая лишь лидером, теряется при его отказе.
- ❌ Считать, что после восстановления лидеры вернутся на места → ✅ могут остаться на других брокерах.
- ❌ Два брокера «для надёжности» → ✅ при RF=3 нужны три.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — кворум контроллеров, выбор лидера
- Раньше: [06 · Схемы и сериализация](../06-schemas-and-serialization/basics.ru.md), [02 · Производители](../02-producers/basics.ru.md)
- Следующий модуль: [08 · Kafka Connect](../08-kafka-connect/overview.ru.md)
- Аналог в другом треке: [RabbitMQ: кластеризация и кворумные очереди](../../../rabbitmq/modules/08-clustering/basics.ru.md)

## Источники

- [Apache Kafka: Replication](https://kafka.apache.org/documentation/#replication)
- [Apache Kafka: Topic configs: min.insync.replicas](https://kafka.apache.org/documentation/#topicconfigs_min.insync.replicas)
- [Apache Kafka: Producer configs: acks](https://kafka.apache.org/documentation/#producerconfigs_acks)
