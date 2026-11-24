---
title: "Schemas and serialization"
lang: en
track: kafka
kind: module
doc: overview
slug: 06-schemas-and-serialization
part: 2
levels: [1, 2, 3, 4]
tags: [kafka, schema-registry, avro, serialization, compatibility, schema-evolution]
versions: "Apache Kafka 4.3.1; Confluent Schema Registry 8.4.1; Python confluent-kafka 2.16.2, fastavro 1.13.1"
status: draft
updated: 2026-11-24
---

# Schemas and serialization

🌐 [Русский](overview.ru.md) · **English**

## In short

Kafka stores bytes, so the record format is an agreement between producers and consumers. A schema (for example **Avro**) is described explicitly, and **Schema Registry** stores schemas, gives them numbers and checks the **compatibility** of new versions. A record carries a magic byte, a schema number and the data.

## Key ideas

- **Avro is more compact than JSON:** 16 bytes against 50 (without the schema).
- **The record format:** `0x00` + the schema id (4 bytes) + Avro data.
- **The default subject:** `<topic>-value`.
- **`BACKWARD` (the default):** a new reader reads old data; a new field has a default, otherwise `409`.
- **Readers:** on an old schema they ignore new fields, on a new one they fill the default into old records.
- **A rename and a type change** are incompatible; an alias lets a rename through.
- **`auto.register.schemas`:** switch it off in production.
- **The registry is a separate Confluent service,** not part of Kafka.

## Minimal example

```python
ser = AvroSerializer(sr, schema_str)
payload = ser(record, SerializationContext("orders", MessageField.VALUE))
producer.produce("orders", value=payload)
```

Everything was run against a real Apache Kafka 4.3.1 and Schema Registry 8.4.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Task | Choice |
|---|---|
| Many consumers and format evolution | Avro + a registry |
| A quick prototype, one consumer | JSON without a schema (accepting the risk) |
| Central compatibility checking | a registry and CI |

- ❌ Do not change a format without a compatibility check.
- ❌ Do not leave automatic registration on in production.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | why schemas, JSON and Avro, the record format, a round trip |
| [2. Advanced theory](advanced.en.md) | compatibility, readers of different schemas, modes, automatic registration |
| [3. Expert theory](expert.en.md) | reference of the registry client, responses, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kafka 06-schemas-and-serialization level 5` |
| 6. Tests | not created yet — `/learn kafka 06-schemas-and-serialization level 6` |

## Related materials

- Prerequisites: [02 · Producers](../02-producers/overview.en.md), [03 · Consumers and groups](../03-consumers-and-groups/overview.en.md)
- Next: [07 · Replication and availability](../07-replication-and-availability/overview.en.md)
- Analogues in other tracks: [RabbitMQ](../../../rabbitmq/overview.en.md) (the message-body format is up to the application)
