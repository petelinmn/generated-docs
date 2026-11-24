---
title: "Схемы и сериализация: базовый уровень"
lang: ru
track: kafka
kind: module
doc: basics
slug: 06-schemas-and-serialization
part: 2
tags: [kafka, schema-registry, avro, serialization, wire-format, json]
versions: "Apache Kafka 4.3.1; Confluent Schema Registry 8.4.1; Python confluent-kafka 2.16.2, fastavro 1.13.1"
status: draft
updated: 2026-11-24
---

# Схемы и сериализация: базовый уровень

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Производители](../02-producers/basics.ru.md), [03 · Потребители и группы](../03-consumers-and-groups/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Код запускался на кластере из трёх брокеров Apache Kafka 4.3.1 (KRaft, образ `apache/kafka:4.3.1`, порты 9091–9093) и Confluent Schema Registry 8.4.1 (образ `confluentinc/cp-schema-registry`, адрес `http://localhost:18081`) в Docker с `confluent-kafka` 2.16.2 и `fastavro` 1.13.1; выводы скопированы из запусков. Установка: `pip install "confluent-kafka[avro,schemaregistry]" fastavro`. Помощники: `BS = "localhost:9091"`, `sr = SchemaRegistryClient({"url": "http://localhost:18081"})`; схема `V1` — запись `Order` с полями `id` (long), `customer` (string), `amount` (double). Schema Registry — **отдельный сервис Confluent**, а не часть Apache Kafka (условия лицензирования проверьте в документации Confluent: здесь они не выяснялись).

## Зачем схемы

Kafka хранит и передаёт **байты**: ей всё равно, что в них. Договор о формате записи лежит на производителях и потребителях. Без явного договора изменение формата (переименованное поле) ломает потребителей. **Схема** описывает структуру записи, а **реестр схем** (Schema Registry) хранит схемы и проверяет, что новые версии не ломают старых читателей.

## JSON и Avro

JSON читаем человеком, но тяжёл: имена полей повторяются в каждой записи. **Apache Avro** — бинарный формат: схема отдельно, в записи только значения.

```python
j = json.dumps(rec).encode()
fastavro.schemaless_writer(buf, fastavro.parse_schema(json.loads(V1)), rec)
```

```text
JSON bytes: 50 | Avro bytes (no schema): 16
```

Одна и та же запись `{"id": 1001, "customer": "alice", "amount": 19.99}`: 50 байт в JSON и 16 в Avro (без схемы). Читать Avro без схемы нельзя — отсюда реестр.

## Формат записи с реестром

Сериализатор Avro регистрирует схему в реестре (если её нет) и записывает в начало значения **идентификатор схемы**:

```python
ser = AvroSerializer(sr, V1)
payload = ser(rec, SerializationContext(topic, MessageField.VALUE))
```

```text
wire: magic byte = 0 | schema id = 1 | total bytes = 21
subjects: ['orders-av-value'] | versions: [1]
```

- Первый байт — «магический» нуль (формат Confluent), следующие четыре — номер схемы в реестре, дальше — данные Avro (16 байт): итого 21.
- Схема зарегистрирована под **субъектом** (subject) `orders-av-value` — по умолчанию `<тема>-value`; версия 1.

## Круговой путь через Kafka

```python
p.produce(topic, value=payload); p.flush()
des = AvroDeserializer(sr)
got = des(m.value(), SerializationContext(topic, MessageField.VALUE))
```

```text
decoded: {'id': 1001, 'customer': 'alice', 'amount': 19.99}
```

Десериализатор читает номер схемы из записи, скачивает схему из реестра (и кэширует) и разбирает данные. Потребителю не нужно заранее знать схему производителя.

## Типичные ошибки новичков

- ❌ Считать, что Kafka проверяет формат записи → ✅ она хранит байты; проверку обеспечивает реестр и клиенты.
- ❌ Менять формат JSON «по договорённости» без схемы → ✅ схема и проверка совместимости.
- ❌ Читать данные Avro без номера схемы → ✅ записывайте формат с реестром.
- ❌ Путать Schema Registry с частью Kafka → ✅ это отдельный сервис.
- ❌ Забыть, что реестр — зависимость доступности → ✅ клиенты кэшируют схемы, но первая запись/чтение обращается к реестру.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — совместимость и эволюция схем
- Раньше: [05 · Семантика доставки](../05-delivery-semantics/basics.ru.md)
- Следующий модуль: [07 · Репликация и доступность](../07-replication-and-availability/overview.ru.md)

## Источники

- [Apache Avro: specification](https://avro.apache.org/docs/current/specification/)
- [Confluent Schema Registry: documentation](https://docs.confluent.io/platform/current/schema-registry/index.html)
- [Confluent Schema Registry: wire format](https://docs.confluent.io/platform/current/schema-registry/fundamentals/serdes-develop/index.html#wire-format)
