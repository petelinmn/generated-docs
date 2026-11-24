---
title: "Kafka Connect: экспертный уровень"
lang: ru
track: kafka
kind: module
doc: expert
slug: 08-kafka-connect
part: 2
tags: [kafka-connect, rest-api, connector, task, converter, smt, worker-config]
versions: "Apache Kafka 4.3.1 (Kafka Connect, режим distributed)"
status: draft
updated: 2026-11-24
---

# Kafka Connect: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. «Проверено» означает наблюдалось на воркере Connect 4.3.1 (distributed, один воркер) рядом с кластером из трёх брокеров.

## Справочник

### REST API воркера

| Запрос | Назначение | Проверено |
|---|---|---|
| `GET /` | версия, идентификатор кластера | да |
| `GET /connector-plugins` | доступные классы | да |
| `GET /connectors` | список коннекторов | да |
| `POST /connectors` | создать (`201`); некорректная конфигурация — `400` | да |
| `GET /connectors/{имя}/status` | состояние коннектора и задач | да |
| `PUT /connectors/{имя}/pause` / `resume` | пауза/возобновление (`202`) | да |
| `GET /connectors/{имя}/offsets` | позиции коннектора | да (пусто для FileStream) |
| `DELETE /connectors/{имя}` | удалить (`204`) | да |
| `PUT …/config`, `POST …/restart`, `GET …/tasks`, `PUT …/stop`, `DELETE …/offsets` | другое | не проверялись |

### Конфигурация воркера (distributed)

| Параметр | Назначение | Проверено |
|---|---|---|
| `bootstrap.servers`, `group.id` | кластер и группа воркеров | да |
| `key.converter`, `value.converter` | конвертеры | да (`JsonConverter`) |
| `*.converter.schemas.enable` | включать ли схему в JSON | да (`false`) |
| `offset.storage.topic`, `config.storage.topic`, `status.storage.topic` | внутренние темы | да |
| `*.storage.replication.factor` | RF внутренних тем | да (3) |
| `plugin.path` | каталог плагинов | да |
| `rest.advertised.host.name`, `rest.port` | адрес REST | да |

### Конфигурация коннектора

| Параметр | Назначение | Проверено |
|---|---|---|
| `connector.class` | класс коннектора | да |
| `tasks.max` | максимум задач | задавался (1) |
| `topic` (source) / `topics` (sink) | тема(ы) | да |
| `file` | путь к файлу (FileStream) | да |
| `transforms`, `transforms.<имя>.type` и параметры | цепочка SMT | да (`HoistField$Value`, `InsertField$Value`) |
| `errors.tolerance`, `errors.deadletterqueue.*`, `key.converter` на уровне коннектора | ошибки и конвертеры | не проверялись |

### Состояния (проверено)

`RUNNING`, `PAUSED` (коннектор и задача).

## Внутреннее устройство

- **Группа воркеров** использует протокол групп Kafka для распределения коннекторов и задач; состояние хранится в темах `connect-configs`, `connect-offsets`, `connect-status`.
- **Источник** хранит позицию («где я в файле»), чтобы продолжить после паузы/перезапуска (проверено поведением паузы).
- **Конвертер** превращает внутренние данные Connect в байты (проверено: JSON-строки в теме).
- **SMT** применяются к записи по порядку между коннектором и конвертером (проверено результатом цепочки).
- **Проверка конфигурации** выполняется до создания (проверено: `400`).

> ⚠️ Проверить: распределение задач между воркерами при их добавлении/отказе, `exactly-once` для источников, обработка ошибок и dead-letter-тема приёмника, плагины сторонних коннекторов здесь не проверялись (работал один воркер).

## Подвохи

### 1. `topic` и `topics`

У источника `topic`, у приёмника `topics` (проверено).

### 2. Пауза не теряет данные источника

Файл дописывался во время паузы; запись появилась после `resume` (проверено).

### 3. `FileStream*` — только примеры

Для рабочих данных нужны другие коннекторы (по документации).

### 4. Пустые позиции

`GET …/offsets` вернул пустой список (проверено), причина не выяснена.

### 5. Конфигурация проверяется заранее

Отсутствие обязательного параметра — `400`, коннектор не создаётся (проверено).

### 6. Результат SMT зависит от порядка

Сначала `HoistField`, затем `InsertField` (проверено). `InsertField` для строки без объекта требует структуры; обратный порядок здесь не проверялся.

### 7. Один воркер — не отказоустойчивость

Для отказоустойчивости нужны несколько воркеров.

## Компромиссы и альтернативы

| Выбор | Плюсы | Минусы |
|---|---|---|
| Kafka Connect | готовые коннекторы, управление через REST | отдельная инфраструктура |
| собственный потребитель/производитель | полный контроль | больше кода и эксплуатации |
| SMT | простые правки без кода | сложная логика не годится |
| потоковая обработка (Kafka Streams) | произвольные преобразования | больше кода |

## Вопросы с собеседований

<details><summary>1. Что такое Kafka Connect?</summary>

Фреймворк потоковой интеграции Kafka с внешними системами: источники, приёмники, задачи, воркеры.

→ [Основы](basics.ru.md#что-такое-kafka-connect)

</details>

<details><summary>2. Чем источник отличается от приёмника?</summary>

Источник читает из внешней системы в Kafka, приёмник пишет из Kafka во внешнюю систему.

→ [Основы](basics.ru.md#что-такое-kafka-connect)

</details>

<details><summary>3. Как управлять коннектором?</summary>

Через REST API воркера: создание, статус, пауза, возобновление, удаление.

→ [Основы](basics.ru.md#rest-api)

</details>

<details><summary>4. Подвох: после `pause` строки файла теряются?</summary>

Нет: появятся после `resume`.

→ [Подвох 2](#2-пауза-не-теряет-данные-источника)

</details>

<details><summary>5. Что такое SMT и где применяются?</summary>

Преобразования отдельных записей по цепочке между коннектором и конвертером.

→ [Продвинутая теория](advanced.ru.md#преобразования-одиночных-записей-smt)

</details>

<details><summary>6. Где воркер хранит состояние?</summary>

В темах `connect-configs`, `connect-offsets`, `connect-status`.

→ [Справочник](#конфигурация-воркера-distributed)

</details>

<details><summary>7. Подвох: как называется параметр темы у источника и приёмника?</summary>

`topic` и `topics` соответственно.

→ [Подвох 1](#1-topic-и-topics)

</details>

<details><summary>8. Что вернёт создание коннектора с неполной конфигурацией?</summary>

`400` с описанием ошибки; коннектор не создаётся.

→ [Подвох 5](#5-конфигурация-проверяется-заранее)

</details>

<details><summary>9. Зачем конвертер?</summary>

Превращает данные Connect в байты Kafka и обратно (JSON, Avro и т. д.).

→ [Продвинутая теория](advanced.ru.md#конвертеры)

</details>

<details><summary>10. Подвох: достаточно ли одного воркера для рабочей системы?</summary>

Нет: нужны несколько воркеров и RF=3 у внутренних тем.

→ [Подвох 7](#7-один-воркер--не-отказоустойчивость)

</details>

<details><summary>11. Когда вместо SMT нужна потоковая обработка?</summary>

Для сложных преобразований, агрегаций и соединений.

→ [Компромиссы](#компромиссы-и-альтернативы)

</details>

<details><summary>12. Почему `FileStream*` не годятся для рабочих данных?</summary>

Это учебные примеры из поставки Kafka (по документации).

→ [Подвох 3](#3-filestream--только-примеры)

</details>

## Шпаргалка

```text
POST /connectors {"name":..., "config": {"connector.class":..., "tasks.max":..., "topic"|"topics":..., ...}}   # 201 | 400
GET /connectors/N/status ; PUT /connectors/N/pause|resume (202) ; DELETE /connectors/N (204)
SMT: "transforms":"a,b", "transforms.a.type":"org.apache.kafka.connect.transforms.HoistField$Value", ...
worker: connect-distributed.sh worker.properties (internal topics RF=3, plugin.path)
```

- Источник — `topic`, приёмник — `topics`.
- Несколько воркеров и RF=3 в рабочей системе.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Раньше: [07 · Репликация и доступность](../07-replication-and-availability/expert.ru.md)
- Следующий модуль: [09 · Мониторинг и эксплуатация](../../overview.ru.md)

## Источники

- [Apache Kafka: Kafka Connect](https://kafka.apache.org/documentation/#connect)
- [Apache Kafka: Connect REST API](https://kafka.apache.org/documentation/#connect_rest)
- [Apache Kafka: Connect transformations](https://kafka.apache.org/documentation/#connect_transforms)
