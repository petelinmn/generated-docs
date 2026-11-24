---
title: "Apache Kafka"
lang: ru
track: kafka
kind: track
doc: overview
slug: kafka
tags: [kafka, event-streaming, log, partition, consumer-group, message-broker]
versions: "Apache Kafka 4.3; Python confluent-kafka 2.16"
default_level: 4
status: draft
updated: 2026-11-24
---

# Apache Kafka

🌐 **Русский** · [English](overview.en.md)

## Коротко

Apache Kafka — распределённая платформа потоковой передачи событий. В основе — **журнал** (log): темы (topics) делятся на партиции (partitions), каждая партиция — упорядоченная последовательность записей, которые только добавляются в конец. Производители (producers) пишут записи, потребители (consumers) читают их по смещению (offset) и сами решают, откуда и как быстро читать. Сообщения не удаляются при чтении: они хранятся, пока не истечёт срок хранения (или пока их не «сожмёт» компактация), поэтому одни и те же данные могут читать многие независимые потребители и перечитывать при необходимости.

От классического брокера очередей (см. [RabbitMQ](../rabbitmq/overview.ru.md)) Kafka отличается моделью: журнал с повторным чтением и масштабирование через партиции вместо маршрутизации по обменникам и удаления подтверждённых сообщений. Трек начинается с модели (тема, партиция, смещение, группа потребителей), затем — гарантии доставки, репликация, схемы данных, интеграция и эксплуатация.

**Пререквизиты:** базовые сети и один язык программирования; для примеров достаточно [трека Python](../python/overview.ru.md).

## Окружение

| Что | Инструмент |
|---|---|
| Брокер | `apache/kafka:4.3.1` в Docker (режим KRaft, без ZooKeeper): `docker run -d --name kafka -p 9092:9092 apache/kafka:4.3.1` |
| CLI | скрипты `/opt/kafka/bin/kafka-*.sh` внутри контейнера: `docker exec kafka /opt/kafka/bin/kafka-topics.sh --bootstrap-server localhost:9092 --list` |
| Язык примеров | Python 3.12+ с клиентом `confluent-kafka` |
| Код модулей | как в треке Python: `code/` с `pyproject.toml`, `examples/`, `solutions/`, `tests/` |
| Тесты модуля | `uv run pytest` внутри `code/` |
| Тесты и брокер | тесты, которым нужен живой брокер, помечены `integration` и пропускаются без `localhost:9092`; остальные не используют сеть |
| Одноразовый брокер | `docker rm -f kafka` удаляет брокер и его данные |

> На Windows в Git Bash перед командами `docker exec … /opt/kafka/…` задайте `MSYS_NO_PATHCONV=1`, иначе путь будет искажён. Одиночный брокер и порт 9092 без TLS — только для локального одноразового опыта; настройка безопасности — в соответствующем модуле.

## Дорожная карта

Прогресс изучения отмечается в [PROGRESS.md](../../PROGRESS.md). Уровень детализации модулей по умолчанию: **4** (теория и самопроверка), см. [уровни детализации](../../README.ru.md#уровни-детализации).

### Часть 1. Основы

- [01 · Основы Kafka](modules/01-kafka-basics/overview.ru.md) — журнал, тема, партиция, смещение, брокер, KRaft, первая запись и чтение
- [02 · Производители](modules/02-producers/overview.ru.md) — ключи и партиционирование, `acks`, пакетирование, идемпотентность
- [03 · Потребители и группы](modules/03-consumers-and-groups/overview.ru.md) — группы потребителей, фиксация смещений, ребалансировка
- [04 · Темы, хранение и компактация](modules/04-topics-and-retention/overview.ru.md) — партиции, срок хранения, сегменты, компактация, порядок записей

### Часть 2. Продвинутые темы

- [05 · Семантика доставки](modules/05-delivery-semantics/overview.ru.md) — at-most-once, at-least-once, exactly-once, транзакции
- [06 · Схемы и сериализация](modules/06-schemas-and-serialization/overview.ru.md) — JSON, Avro, Protobuf, Schema Registry, эволюция схем
- [07 · Репликация и доступность](modules/07-replication-and-availability/overview.ru.md) — лидеры, ISR, `min.insync.replicas`, выбор лидера
- [08 · Kafka Connect](modules/08-kafka-connect/overview.ru.md) — источники и приёмники, коннекторы, преобразования
- [09 · Мониторинг и эксплуатация](modules/09-monitoring/overview.ru.md) — отставание потребителей, метрики, CLI, тревоги

### Часть 3. Темы для опытных

- [10 · Производительность](modules/10-performance/overview.ru.md) — пропускная способность, задержка, сжатие, настройка клиентов и брокера
- [11 · Безопасность](modules/11-security/overview.ru.md) — TLS, SASL, ACL, квоты
- [12 · Потоковая обработка](modules/12-stream-processing/overview.ru.md) — Kafka Streams, ksqlDB, окна, состояние
- [13 · Внутреннее устройство](modules/13-internals/overview.ru.md) — сегменты журнала, индексы, кластер KRaft, страничный кэш
- [14 · Kafka в Kubernetes](modules/14-kubernetes/overview.ru.md) — Strimzi, хранилище, обновления

## Библиотеки и инструменты

- [confluent-kafka](libraries/confluent-kafka/overview.ru.md) — клиент Python на основе librdkafka
- [Kafka CLI](tools/kafka-cli/overview.ru.md) — скрипты `kafka-topics`, `kafka-console-*`, `kafka-consumer-groups` и другие

## Рецепты

- [Потребитель с повторами и dead-letter-темой](recipes/consumer-with-retries/overview.ru.md) — обработка сбоев без остановки партиции
- [Конвейер exactly-once](recipes/exactly-once-pipeline/overview.ru.md) — чтение-обработка-запись с транзакциями
- [Transactional outbox](recipes/outbox/overview.ru.md) — надёжная публикация событий из транзакции базы данных

## Проекты

- [Конвейер кликов](projects/clickstream-pipeline/overview.ru.md) — приём событий, обработка и агрегаты через несколько тем

## Связанные треки

- [RabbitMQ](../rabbitmq/overview.ru.md) — классический брокер очередей: сравнение моделей
- [Python](../python/overview.ru.md) — язык примеров
- [Kubernetes](../kubernetes/overview.ru.md) — запуск Kafka оператором
- [Linux](../linux/overview.ru.md) — файловая система, страничный кэш и диагностика
- [Фоновые задачи в C#](../csharp/recipes/background-jobs/overview.ru.md) — обработка очереди работ в .NET-сервисе

## Источники

- [Apache Kafka documentation](https://kafka.apache.org/documentation/)
- [Apache Kafka: Introduction](https://kafka.apache.org/intro)
- [confluent-kafka-python documentation](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html)
