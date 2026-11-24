---
title: "Kafka Connect: advanced level"
lang: en
track: kafka
kind: module
doc: advanced
slug: 08-kafka-connect
part: 2
tags: [kafka-connect, smt, single-message-transform, connector-lifecycle, config-validation, offsets]
versions: "Apache Kafka 4.3.1 (Kafka Connect, distributed mode)"
status: draft
updated: 2026-11-24
---

# Kafka Connect: advanced level

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md) (the same Connect 4.3.1 worker in distributed mode).

## A connector's lifecycle

A connector is managed through the REST API: pause, resume, restart, delete.

```python
rest("PUT", "/connectors/file-src/pause")
rest("PUT", "/connectors/file-src/resume")
```

```text
pause: 202
state: PAUSED
topic while paused: 3 records
resume: 202
topic after resume: 4 records
```

- The commands were accepted with code `202` (they run asynchronously).
- While the connector was `PAUSED` the new file line (`while paused`) **did not reach** the topic (3 records); after `resume` it appeared (4 records): the connector continued from where it stopped.

A connector's positions (source offsets) are stored in the topic `connect-offsets`. A request `GET /connectors/file-src/offsets` returned `{"offsets": []}` in our experiment.

> ⚠️ Verify: why the position list is empty for `FileStreamSourceConnector` (a quirk of this teaching connector or the timing of the request) was not established here; the semantics of resetting positions (`DELETE …/offsets`) were not tested.

## Single message transforms (SMT)

**SMTs** modify each record on the fly with no code. A chain is set in the configuration: `transforms=wrap,addsrc`.

```python
"transforms": "wrap,addsrc",
"transforms.wrap.type": "org.apache.kafka.connect.transforms.HoistField$Value", "transforms.wrap.field": "line",
"transforms.addsrc.type": "org.apache.kafka.connect.transforms.InsertField$Value",
"transforms.addsrc.static.field": "origin", "transforms.addsrc.static.value": "in2.txt"
```

```text
smt topic: ['{"line":"secret-1","origin":"in2.txt"}']
```

- `HoistField$Value` wrapped the string into an object `{"line": "secret-1"}`;
- `InsertField$Value` added a static field `origin`.

The result is a JSON object instead of a bare string. Transforms apply in order.

## Configuration validation

The worker validates a connector's configuration **before** creating it:

```text
bad config: 400 {"error_code": 400, "message": "Connector configuration is invalid and contains the following 1 error(s):\nMissing required configuration \"topic\" which has no default value.\nYou can also find ...
```

A missing required parameter `topic` gives `400` with a clear description, and the connector is not created.

## Converters

Converters (`key.converter`, `value.converter`) turn Connect's data into Kafka's bytes and back. The experiment used `JsonConverter` with `schemas.enable=false`, so the topic holds plain JSON values; for schemas and a registry you use the Avro converter ([module 06](../06-schemas-and-serialization/basics.en.md)).

> ⚠️ Verify: the Avro converter and Schema Registry with Connect were not verified here (they need a separate set of libraries in `plugin.path`).

## Best practices

- Several workers, RF=3 for the internal topics (`connect-offsets`, `connect-configs`, `connect-status`).
- Keep connector configurations in a repository and apply them through REST/CI.
- Check `status` and task states; alert on `FAILED`.
- Transforms are for simple changes; move complex logic into stream processing.
- Use specialised connectors for production data, not `FileStream*`.

## Common mistakes

- ❌ Expecting a pause to lose source data → ✅ the source continues from where it stopped (verified).
- ❌ Piling logic into an SMT chain → ✅ SMTs for simple edits.
- ❌ Not checking task `status` → ✅ a task may fail while the connector is alive.
- ❌ Using schemaless JSON when the format evolves → ✅ Avro + a registry.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [06 · Schemas and serialization](../06-schemas-and-serialization/advanced.en.md)
- Next module: [09 · Monitoring and operations](../../overview.en.md)

## Sources

- [Apache Kafka: Connect transformations](https://kafka.apache.org/documentation/#connect_transforms)
- [Apache Kafka: Connect REST API](https://kafka.apache.org/documentation/#connect_rest)
- [Apache Kafka: Kafka Connect](https://kafka.apache.org/documentation/#connect)
