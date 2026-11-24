---
title: "Схемы и сериализация: экспертный уровень"
lang: ru
track: kafka
kind: module
doc: expert
slug: 06-schemas-and-serialization
part: 2
tags: [kafka, schema-registry, avro, subject, compatibility, wire-format, serdes]
versions: "Apache Kafka 4.3.1; Confluent Schema Registry 8.4.1; Python confluent-kafka 2.16.2, fastavro 1.13.1"
status: draft
updated: 2026-11-24
---

# Схемы и сериализация: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. «Проверено» означает наблюдалось на кластере из трёх брокеров Apache Kafka 4.3.1 и Confluent Schema Registry 8.4.1.

## Справочник

### Клиент реестра (`confluent_kafka.schema_registry`)

| Элемент | Назначение | Проверено |
|---|---|---|
| `SchemaRegistryClient({"url": …})` | клиент | да |
| `get_subjects()`, `get_versions(subject)` | субъекты и версии | да |
| `register_schema(subject, Schema(text, "AVRO"))` | регистрация; возвращает идентификатор | да |
| `test_compatibility(subject, Schema)` | проверка без регистрации | да |
| `get_compatibility(subject=None)` / `set_compatibility(subject, level)` | уровень совместимости | да |
| `delete_subject(subject, permanent=False/True)` | мягкое/жёсткое удаление | да (очистка) |
| `Schema(text, "JSON")`, `Schema(text, "PROTOBUF")` | другие форматы | не проверялись |

### Сериализаторы (`schema_registry.avro`)

| Элемент | Поведение | Проверено |
|---|---|---|
| `AvroSerializer(sr, schema_str, conf=…)` | схема писателя; регистрирует схему | да |
| `AvroDeserializer(sr, reader_schema_str=None)` | без схемы читателя — схема писателя; с ней — разрешение схем | да |
| `conf["auto.register.schemas"]` | автоматическая регистрация | да (`False` → 404) |
| `SerializationContext(topic, MessageField.VALUE/KEY)` | определяет субъект `<тема>-value`/`-key` | да (`-value`) |
| `use.latest.version`, стратегии имён субъектов (`TopicNameStrategy`, `RecordNameStrategy`, `TopicRecordNameStrategy`) | выбор версии/субъекта | не проверялись |

### Формат записи (проверено)

| Байты | Содержимое |
|---|---|
| 0 | магический байт `0` |
| 1–4 | идентификатор схемы (big-endian) |
| 5… | данные Avro |

### Ответы реестра (проверено)

| Ситуация | Ответ |
|---|---|
| несовместимая схема | `409`, код `40901` |
| субъекта нет | `404`, код `40401` |

### Совместимость

| Параметр | Значение |
|---|---|
| глобальный уровень по умолчанию | `BACKWARD` (проверено) |
| уровень субъекта | перекрывает глобальный (проверено на `NONE`) |
| `FORWARD`, `FULL`, `*_TRANSITIVE` | описаны по документации, не проверялись |

## Внутреннее устройство

- **Схемы хранятся в Kafka:** реестр держит их во внутренней теме и кэширует; клиент кэширует схемы по номеру.
- **Идентификатор схемы глобален** в реестре: версия 2 субъекта получила id 2 (проверено), а в записи кодируется именно id.
- **Субъект по умолчанию** строится из имени темы: `orders-av-value` (проверено).
- **Разрешение схем (schema resolution)** Avro сопоставляет схему писателя и читателя по именам полей (проверено поведением: лишнее поле отброшено, недостающее взято из умолчания).
- **Реестр — отдельный процесс** и зависимость: недоступность реестра блокирует первую сериализацию/десериализацию неизвестной схемы.

> ⚠️ Проверить: имя и устройство внутренней темы реестра (`_schemas`), поведение при падении реестра, режимы «ссылок на схемы», JSON Schema и Protobuf здесь не проверялись.

## Подвохи

### 1. Kafka не знает формата

Запись любых байт пройдёт; проверка — на клиентах и в реестре.

### 2. Новое поле без значения по умолчанию

`409` при регистрации (проверено).

### 3. `NONE` у субъекта

Отключает защиту только для него; глобальный уровень остаётся (проверено).

### 4. Читатель, зафиксированный на старой схеме

Игнорирует новые поля (проверено).

### 5. `auto.register.schemas=true` по умолчанию

Любой производитель может зарегистрировать схему; в рабочей системе отключайте.

### 6. Реестр как зависимость

Первое обращение к неизвестной схеме идёт в реестр.

### 7. Нельзя читать Avro без реестра

Без номера схемы запись не разобрать (проверено: 16 байт данных без схемы).

## Компромиссы и альтернативы

| Выбор | Плюсы | Минусы |
|---|---|---|
| JSON без схемы | просто, читаемо | тяжёлый, нет проверки, договорённость «на словах» |
| Avro + реестр | компактно, проверка совместимости | зависимость от реестра |
| JSON Schema / Protobuf + реестр | знакомые форматы | здесь не проверялись |
| схема в коде (без реестра) | нет сервиса | нет централизованной проверки |

## Вопросы с собеседований

<details><summary>1. Что хранит Kafka: структуру или байты?</summary>

Байты; формат — договорённость производителей и потребителей.

→ [Основы](basics.ru.md#зачем-схемы)

</details>

<details><summary>2. Зачем Schema Registry?</summary>

Хранит схемы, выдаёт им номера и проверяет совместимость новых версий.

→ [Основы](basics.ru.md#зачем-схемы)

</details>

<details><summary>3. Что в первых байтах записи при Avro + реестр?</summary>

Магический байт 0 и четыре байта идентификатора схемы.

→ [Основы](basics.ru.md#формат-записи-с-реестром)

</details>

<details><summary>4. Подвох: добавить поле без значения по умолчанию при `BACKWARD`?</summary>

Нельзя: `409` при регистрации.

→ [Подвох 2](#2-новое-поле-без-значения-по-умолчанию)

</details>

<details><summary>5. Что означает `BACKWARD`?</summary>

Новый читатель читает данные, записанные по предыдущей схеме.

→ [Продвинутая теория](advanced.ru.md#эволюция-схем-и-совместимость)

</details>

<details><summary>6. Что произойдёт, если старый читатель увидит запись с новым полем?</summary>

Читатель, зафиксированный на старой схеме, отбросит новое поле.

→ [Подвох 4](#4-читатель-зафиксированный-на-старой-схеме)

</details>

<details><summary>7. Зачем отключать `auto.register.schemas`?</summary>

Чтобы схемы попадали в реестр только через проверенный процесс; при неизвестной схеме сериализация падает (404).

→ [Продвинутая теория](advanced.ru.md#автоматическая-регистрация-схем)

</details>

<details><summary>8. Подвох: схема версии 2 имеет идентификатор 2 всегда?</summary>

Нет: идентификатор глобален в реестре; совпадение в опыте случайно (версия 2 субъекта получила id 2).

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>9. Как узнать, совместима ли схема, не регистрируя её?</summary>

`test_compatibility`.

→ [Продвинутая теория](advanced.ru.md#эволюция-схем-и-совместимость)

</details>

<details><summary>10. Подвох: можно ли поменять тип поля `amount` с double на string «по совместимости»?</summary>

Нет: реестр признал смену типа несовместимой (`test_compatibility` → `False`); добавляйте новое поле. Переименование поля тоже несовместимо, а с алиасом (`aliases`) проверка прошла.

→ [Лучшие практики](advanced.ru.md#лучшие-практики)

</details>

<details><summary>11. Как называется субъект по умолчанию?</summary>

`<тема>-value` (и `<тема>-key` для ключа).

→ [Справочник](#сериализаторы-schema_registryavro)

</details>

<details><summary>12. Почему Avro компактнее JSON?</summary>

Хранит только значения; имена полей находятся в схеме (в опыте 16 байт против 50).

→ [Основы](basics.ru.md#json-и-avro)

</details>

## Шпаргалка

```text
sr = SchemaRegistryClient({"url": ...}) ; AvroSerializer(sr, schema, conf={"auto.register.schemas": False}) ; AvroDeserializer(sr[, reader_schema])
wire = 0x00 + schema_id(4 bytes BE) + avro payload ; subject = "<topic>-value"
compat: BACKWARD (default) = new reader reads old data -> new fields need defaults ; 409/40901 on violation
sr.test_compatibility(subject, Schema(text, "AVRO")) before registering
```

- Схема — договор; реестр — его страж.
- Новые поля — со значением по умолчанию.
- Автоматическая регистрация — только для разработки.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Раньше: [05 · Семантика доставки](../05-delivery-semantics/expert.ru.md)
- Следующий модуль: [07 · Репликация и доступность](../07-replication-and-availability/overview.ru.md)

## Источники

- [Confluent Schema Registry: documentation](https://docs.confluent.io/platform/current/schema-registry/index.html)
- [Apache Avro: specification](https://avro.apache.org/docs/current/specification/)
- [confluent-kafka-python: schema registry](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html#schemaregistry-client)
