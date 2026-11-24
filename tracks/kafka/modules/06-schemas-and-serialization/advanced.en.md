---
title: "Schemas and serialization: advanced level"
lang: en
track: kafka
kind: module
doc: advanced
slug: 06-schemas-and-serialization
part: 2
tags: [kafka, schema-registry, schema-evolution, compatibility, backward-compatibility, avro]
versions: "Apache Kafka 4.3.1; Confluent Schema Registry 8.4.1; Python confluent-kafka 2.16.2, fastavro 1.13.1"
status: draft
updated: 2026-11-24
---

# Schemas and serialization: advanced level

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md) (the same rig). The schema `V1` is `Order(id, customer, amount)`; `V2_OK` adds a field `currency` with the default `"USD"`; `V2_BAD` adds `currency` **without** a default.

## Schema evolution and compatibility

Schemas change over time while old records stay in the topic, so you need rules about which changes keep old and new readers and writers working. The registry checks **compatibility** when a new version is registered. The default level:

```text
global compatibility: BACKWARD
```

**BACKWARD** means: a reader with the **new** schema can read data written with the **previous** one. Allowed changes: adding a field **with a default**, removing a field.

```python
sr.test_compatibility("orders-av-value", Schema(V2_OK, "AVRO"))
sr.test_compatibility("orders-av-value", Schema(V2_BAD, "AVRO"))
```

```text
v2 with default field compatible: True
v2 without default compatible   : False
register incompatible -> 409 40901 Schema being registered is incompatible with an earlier schema for subject "orders-av-valu…
registered v2 id: 2 | versions: [1, 2]
```

- A field with a default passes the check, one without does not: the new reader does not know what to put for old records.
- An attempt to register an incompatible schema ends with a `409` response (code `40901`).
- The compatible version got id 2 and became version 2 of the subject.

## What readers with different schemas see

A record was written with `V2_OK` (with `currency="EUR"`), while `V1` is the old schema:

```text
reader with writer's schema sees: {'id': 1001, 'customer': 'alice', 'amount': 19.99, 'currency': 'EUR'}
reader pinned to V1 sees       : {'id': 1001, 'customer': 'alice', 'amount': 19.99}
reader pinned to V2 reads old  : {'id': 1001, 'customer': 'alice', 'amount': 19.99, 'currency': 'USD'}
```

- By default the deserializer uses the **writer's** schema (taken from the registry by the number in the record).
- A reader pinned to `V1` read the new record and **ignored** the new field.
- A reader on `V2` read an **old** record (written with `V1`) and filled `currency` = `USD` by default — which is exactly what "backward compatibility" means.

## Compatibility modes

| Mode | What it guarantees |
|---|---|
| `BACKWARD` (default) | a new reader reads old data |
| `FORWARD` | an old reader reads new data |
| `FULL` | both |
| `NONE` | checking is off |
| `*_TRANSITIVE` modes | checking against all earlier versions, not just the previous one |

> ⚠️ Verify: only `BACKWARD` and `NONE` were tested in the experiment; the behaviour of `FORWARD`, `FULL` and `*_TRANSITIVE` is described from the documentation.

The mode can be set for a subject separately from the global one:

```python
sr.set_compatibility("orders-av-value", "NONE")
```

```text
after NONE, bad v3 compatible: True
global stays: BACKWARD
```

After `NONE` an incompatible schema passes the subject's check while the global mode is unchanged. It is easy to "switch off the protection" for one topic: do so deliberately.

## Automatic schema registration

By default the serializer **registers** the schema itself (`auto.register.schemas`). In production this is switched off so schemas reach the registry only through a reviewed process (CI):

```python
AvroSerializer(sr, schema, conf={"auto.register.schemas": False})
```

```text
auto.register=False, unknown schema -> SchemaRegistryError Subject 'other-topic-value' not found. (HTTP status code 404, SR code 40401)
```

If the schema is not in the registry, serialization fails with an error — a record with an unknown format does not reach the topic.

## Best practices

- Choose the compatibility mode deliberately (`BACKWARD` by default) and check new schemas in CI (`test_compatibility`) before deployment.
- New fields get defaults; do not rename or change a field's type — add a new one.
- Switch off automatic registration in production.
- Do not turn off the compatibility check (`NONE`) without need.
- Rollout order under `BACKWARD`: readers first (with the new schema), then writers.

## Common mistakes

- ❌ Adding a required field without a default → ✅ `409` at registration.
- ❌ Renaming a field → ✅ a new field and a gradual retirement of the old one.
- ❌ Leaving `auto.register.schemas=true` in production → ✅ registration through CI.
- ❌ Switching the mode to `NONE` "temporarily" → ✅ incompatible data stays in the topic.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [04 · Topics, retention and compaction](../04-topics-and-retention/advanced.en.md) — old records stay in the topic
- Next module: [07 · Replication and availability](../07-replication-and-availability/overview.en.md)

## Sources

- [Confluent Schema Registry: schema evolution and compatibility](https://docs.confluent.io/platform/current/schema-registry/fundamentals/schema-evolution.html)
- [Apache Avro: schema resolution](https://avro.apache.org/docs/current/specification/#schema-resolution)
- [confluent-kafka-python: schema registry](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html#schemaregistry-client)
