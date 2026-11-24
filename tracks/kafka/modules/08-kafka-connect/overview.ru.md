---
title: "Kafka Connect"
lang: ru
track: kafka
kind: module
doc: overview
slug: 08-kafka-connect
part: 2
levels: [1, 2, 3, 4]
tags: [kafka-connect, connector, source-connector, sink-connector, smt, rest-api]
versions: "Apache Kafka 4.3.1 (Kafka Connect, режим distributed)"
status: draft
updated: 2026-11-24
---

# Kafka Connect

🌐 **Русский** · [English](overview.en.md)

## Коротко

Kafka Connect — часть Apache Kafka для интеграции с внешними системами без собственного кода. **Источники** (source) читают из внешней системы в Kafka, **приёмники** (sink) пишут из Kafka наружу; работу выполняют **задачи** на **воркерах**. Коннекторами управляют через REST API, а состояние воркеров хранится в темах Kafka.

## Ключевые идеи

- **Источник и приёмник:** у источника `topic`, у приёмника `topics`.
- **REST API воркера:** `POST /connectors` (201), `status`, `pause`/`resume` (202), `DELETE` (204); неверная конфигурация — `400`.
- **Пауза не теряет данных:** источник продолжает с места остановки.
- **SMT:** цепочка простых преобразований записей (`HoistField`, `InsertField`).
- **Конвертеры** переводят данные Connect в байты Kafka (в опыте JSON без схем).
- **Состояние воркеров** в темах `connect-offsets`, `connect-configs`, `connect-status` (RF=3 в рабочей системе).
- **`FileStream*` — только учебные примеры;** для данных нужны специализированные коннекторы.

## Минимальный пример

```python
rest("POST", "/connectors", {"name": "file-src", "config": {
    "connector.class": "org.apache.kafka.connect.file.FileStreamSourceConnector",
    "file": "/tmp/in.txt", "topic": "connect-test"}})
```

Всё запускалось на настоящем воркере Kafka Connect 4.3.1 в Docker (см. [базовую теорию](basics.ru.md)).

## Когда применять и когда нет

| Задача | Выбор |
|---|---|
| Перенос данных между Kafka и БД/хранилищем | Kafka Connect |
| Простые правки записей | SMT |
| Сложная обработка, агрегации | потоковая обработка |

- ❌ Не используйте `FileStream*` для рабочих данных.
- ❌ Не запускайте один воркер в рабочей системе.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | что такое Connect, REST API, источник, приёмник |
| [2. Продвинутая теория](advanced.ru.md) | жизненный цикл, SMT, проверка конфигурации, конвертеры |
| [3. Экспертная теория](expert.ru.md) | справочник REST и конфигураций, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn kafka 08-kafka-connect level 5` |
| 6. Тесты | пока не созданы — `/learn kafka 08-kafka-connect level 6` |

## Связанные материалы

- Пререквизиты: [01 · Основы Kafka](../01-kafka-basics/overview.ru.md), [06 · Схемы и сериализация](../06-schemas-and-serialization/overview.ru.md)
- Дальше: [09 · Мониторинг и эксплуатация](../../overview.ru.md)
- Аналоги в других треках: [RabbitMQ: Federation и Shovel](../../../rabbitmq/modules/12-federation-and-shovel/overview.ru.md)
