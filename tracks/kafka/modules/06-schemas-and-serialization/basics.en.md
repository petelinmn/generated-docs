---
title: "Schemas and serialization: basic level"
lang: en
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

# Schemas and serialization: basic level

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Producers](../02-producers/basics.en.md), [03 · Consumers and groups](../03-consumers-and-groups/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

The code was run on a cluster of three Apache Kafka 4.3.1 brokers (KRaft, image `apache/kafka:4.3.1`, ports 9091–9093) and Confluent Schema Registry 8.4.1 (image `confluentinc/cp-schema-registry`, address `http://localhost:18081`) in Docker with `confluent-kafka` 2.16.2 and `fastavro` 1.13.1; outputs are copied from the runs. Installation: `pip install "confluent-kafka[avro,schemaregistry]" fastavro`. Helpers: `BS = "localhost:9091"`, `sr = SchemaRegistryClient({"url": "http://localhost:18081"})`; the schema `V1` is a record `Order` with fields `id` (long), `customer` (string), `amount` (double). Schema Registry is a **separate Confluent service**, not part of Apache Kafka (check the licensing terms in Confluent's documentation: they were not established here).

## Why schemas

Kafka stores and passes **bytes**: it does not care what is in them. The agreement on a record's format rests on producers and consumers. Without an explicit agreement a format change (a renamed field) breaks consumers. A **schema** describes a record's structure, and a **schema registry** stores schemas and checks that new versions do not break old readers.

## JSON and Avro

JSON is human-readable but heavy: field names repeat in every record. **Apache Avro** is a binary format: the schema is separate, the record holds only values.

```python
j = json.dumps(rec).encode()
fastavro.schemaless_writer(buf, fastavro.parse_schema(json.loads(V1)), rec)
```

```text
JSON bytes: 50 | Avro bytes (no schema): 16
```

The same record `{"id": 1001, "customer": "alice", "amount": 19.99}`: 50 bytes in JSON and 16 in Avro (without the schema). Avro cannot be read without a schema — hence the registry.

## The record format with a registry

The Avro serializer registers the schema in the registry (if absent) and writes a **schema id** at the start of the value:

```python
ser = AvroSerializer(sr, V1)
payload = ser(rec, SerializationContext(topic, MessageField.VALUE))
```

```text
wire: magic byte = 0 | schema id = 1 | total bytes = 21
subjects: ['orders-av-value'] | versions: [1]
```

- The first byte is a "magic" zero (the Confluent format), the next four are the schema's number in the registry, then comes the Avro data (16 bytes): 21 in total.
- The schema is registered under a **subject** `orders-av-value` — by default `<topic>-value`; version 1.

## A round trip through Kafka

```python
p.produce(topic, value=payload); p.flush()
des = AvroDeserializer(sr)
got = des(m.value(), SerializationContext(topic, MessageField.VALUE))
```

```text
decoded: {'id': 1001, 'customer': 'alice', 'amount': 19.99}
```

The deserializer reads the schema number from the record, downloads the schema from the registry (and caches it) and parses the data. The consumer does not need to know the producer's schema in advance.

## Common beginner mistakes

- ❌ Assuming Kafka checks a record's format → ✅ it stores bytes; the registry and clients provide checks.
- ❌ Changing a JSON format "by agreement" without a schema → ✅ a schema and compatibility checks.
- ❌ Reading Avro data without the schema number → ✅ write the format with a registry.
- ❌ Mixing Schema Registry up with part of Kafka → ✅ it is a separate service.
- ❌ Forgetting that the registry is an availability dependency → ✅ clients cache schemas, but the first write/read contacts the registry.

## Related materials

- Next: [advanced theory](advanced.en.md) — compatibility and schema evolution
- Before: [05 · Delivery semantics](../05-delivery-semantics/basics.en.md)
- Next module: [07 · Replication and availability](../07-replication-and-availability/overview.en.md)

## Sources

- [Apache Avro: specification](https://avro.apache.org/docs/current/specification/)
- [Confluent Schema Registry: documentation](https://docs.confluent.io/platform/current/schema-registry/index.html)
- [Confluent Schema Registry: wire format](https://docs.confluent.io/platform/current/schema-registry/fundamentals/serdes-develop/index.html#wire-format)
