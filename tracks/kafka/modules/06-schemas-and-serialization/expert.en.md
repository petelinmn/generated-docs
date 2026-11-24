---
title: "Schemas and serialization: expert level"
lang: en
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

# Schemas and serialization: expert level

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on a three-broker Apache Kafka 4.3.1 cluster and Confluent Schema Registry 8.4.1.

## Reference

### The registry client (`confluent_kafka.schema_registry`)

| Element | Purpose | Verified |
|---|---|---|
| `SchemaRegistryClient({"url": …})` | the client | yes |
| `get_subjects()`, `get_versions(subject)` | subjects and versions | yes |
| `register_schema(subject, Schema(text, "AVRO"))` | registration; returns an id | yes |
| `test_compatibility(subject, Schema)` | a check without registering | yes |
| `get_compatibility(subject=None)` / `set_compatibility(subject, level)` | the compatibility level | yes |
| `delete_subject(subject, permanent=False/True)` | soft/hard deletion | yes (cleanup) |
| `Schema(text, "JSON")`, `Schema(text, "PROTOBUF")` | other formats | not tested |

### Serializers (`schema_registry.avro`)

| Element | Behaviour | Verified |
|---|---|---|
| `AvroSerializer(sr, schema_str, conf=…)` | the writer's schema; registers the schema | yes |
| `AvroDeserializer(sr, reader_schema_str=None)` | without a reader schema — the writer's schema; with one — schema resolution | yes |
| `conf["auto.register.schemas"]` | automatic registration | yes (`False` → 404) |
| `SerializationContext(topic, MessageField.VALUE/KEY)` | determines the subject `<topic>-value`/`-key` | yes (`-value`) |
| `use.latest.version`, subject-name strategies (`TopicNameStrategy`, `RecordNameStrategy`, `TopicRecordNameStrategy`) | version/subject selection | not tested |

### The record format (verified)

| Bytes | Content |
|---|---|
| 0 | the magic byte `0` |
| 1–4 | the schema id (big-endian) |
| 5… | Avro data |

### Registry responses (verified)

| Situation | Response |
|---|---|
| an incompatible schema | `409`, code `40901` |
| no such subject | `404`, code `40401` |

### Compatibility

| Parameter | Value |
|---|---|
| the global default level | `BACKWARD` (verified) |
| a subject's level | overrides the global one (verified with `NONE`) |
| `FORWARD`, `FULL`, `*_TRANSITIVE` | described from the documentation, not tested |

## Internals

- **Schemas are stored in Kafka:** the registry keeps them in an internal topic and caches them; the client caches schemas by number.
- **The schema id is global** in the registry: version 2 of the subject got id 2 (verified), and it is the id that is encoded in the record.
- **The default subject** is built from the topic name: `orders-av-value` (verified).
- **Avro schema resolution** matches the writer's and reader's schemas by field names (verified by behaviour: the extra field dropped, the missing one taken from the default).
- **The registry is a separate process** and a dependency: its unavailability blocks the first serialization/deserialization of an unknown schema.

> ⚠️ Verify: the name and design of the registry's internal topic (`_schemas`), behaviour when the registry is down, schema references, JSON Schema and Protobuf were not verified here.

## Gotchas

### 1. Kafka does not know the format

A record of any bytes goes through; the check is on clients and in the registry.

### 2. A new field without a default

`409` at registration (verified).

### 3. `NONE` on a subject

Switches protection off for it only; the global level stays (verified).

### 4. A reader pinned to an old schema

It ignores new fields (verified).

### 5. `auto.register.schemas=true` by default

Any producer can register a schema; switch it off in production.

### 6. The registry as a dependency

The first access to an unknown schema goes to the registry.

### 7. Avro cannot be read without the registry

Without a schema number a record cannot be parsed (verified: 16 bytes of data with no schema).

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| JSON without a schema | simple, readable | heavy, no checking, an agreement "by word" |
| Avro + a registry | compact, compatibility checks | a registry dependency |
| JSON Schema / Protobuf + a registry | familiar formats | not tested here |
| a schema in code (no registry) | no service | no central checking |

## Interview questions

<details><summary>1. Does Kafka store structure or bytes?</summary>

Bytes; the format is an agreement between producers and consumers.

→ [Basics](basics.en.md#why-schemas)

</details>

<details><summary>2. Why Schema Registry?</summary>

It stores schemas, gives them numbers and checks the compatibility of new versions.

→ [Basics](basics.en.md#why-schemas)

</details>

<details><summary>3. What is in the first bytes of a record with Avro + a registry?</summary>

The magic byte 0 and four bytes of the schema id.

→ [Basics](basics.en.md#the-record-format-with-a-registry)

</details>

<details><summary>4. Trick: adding a field without a default under `BACKWARD`?</summary>

Not allowed: `409` at registration.

→ [Gotcha 2](#2-a-new-field-without-a-default)

</details>

<details><summary>5. What does `BACKWARD` mean?</summary>

A new reader reads data written with the previous schema.

→ [Advanced theory](advanced.en.md#schema-evolution-and-compatibility)

</details>

<details><summary>6. What happens if an old reader sees a record with a new field?</summary>

A reader pinned to the old schema drops the new field.

→ [Gotcha 4](#4-a-reader-pinned-to-an-old-schema)

</details>

<details><summary>7. Why switch off `auto.register.schemas`?</summary>

So schemas reach the registry only through a reviewed process; with an unknown schema serialization fails (404).

→ [Advanced theory](advanced.en.md#automatic-schema-registration)

</details>

<details><summary>8. Trick: does version 2 of a schema always have id 2?</summary>

No: the id is global in the registry; the match in the experiment is incidental (version 2 of the subject got id 2).

→ [Internals](#internals)

</details>

<details><summary>9. How do you find out whether a schema is compatible without registering it?</summary>

`test_compatibility`.

→ [Advanced theory](advanced.en.md#schema-evolution-and-compatibility)

</details>

<details><summary>10. Trick: may you change the type of `amount` from double to string "for compatibility"?</summary>

No: the registry judged a type change incompatible (`test_compatibility` → `False`); add a new field. Renaming a field is incompatible too, while with an alias (`aliases`) the check passed.

→ [Best practices](advanced.en.md#best-practices)

</details>

<details><summary>11. What is the default subject called?</summary>

`<topic>-value` (and `<topic>-key` for the key).

→ [Reference](#serializers-schema_registryavro)

</details>

<details><summary>12. Why is Avro more compact than JSON?</summary>

It stores only values; field names live in the schema (in the experiment 16 bytes against 50).

→ [Basics](basics.en.md#json-and-avro)

</details>

## Cheat sheet

```text
sr = SchemaRegistryClient({"url": ...}) ; AvroSerializer(sr, schema, conf={"auto.register.schemas": False}) ; AvroDeserializer(sr[, reader_schema])
wire = 0x00 + schema_id(4 bytes BE) + avro payload ; subject = "<topic>-value"
compat: BACKWARD (default) = new reader reads old data -> new fields need defaults ; 409/40901 on violation
sr.test_compatibility(subject, Schema(text, "AVRO")) before registering
```

- A schema is a contract; the registry is its guard.
- New fields get a default.
- Automatic registration only for development.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [05 · Delivery semantics](../05-delivery-semantics/expert.en.md)
- Next module: [07 · Replication and availability](../07-replication-and-availability/overview.en.md)

## Sources

- [Confluent Schema Registry: documentation](https://docs.confluent.io/platform/current/schema-registry/index.html)
- [Apache Avro: specification](https://avro.apache.org/docs/current/specification/)
- [confluent-kafka-python: schema registry](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html#schemaregistry-client)
