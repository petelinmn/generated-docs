---
title: "Kafka Connect"
lang: en
track: kafka
kind: module
doc: overview
slug: 08-kafka-connect
part: 2
levels: [1, 2, 3, 4]
tags: [kafka-connect, connector, source-connector, sink-connector, smt, rest-api]
versions: "Apache Kafka 4.3.1 (Kafka Connect, distributed mode)"
status: draft
updated: 2026-11-24
---

# Kafka Connect

🌐 [Русский](overview.ru.md) · **English**

## In short

Kafka Connect is a part of Apache Kafka for integrating with external systems without your own code. **Sources** read from an external system into Kafka, **sinks** write from Kafka outwards; the work is done by **tasks** on **workers**. Connectors are managed through a REST API, and worker state lives in Kafka topics.

## Key ideas

- **A source and a sink:** a source has `topic`, a sink `topics`.
- **The worker's REST API:** `POST /connectors` (201), `status`, `pause`/`resume` (202), `DELETE` (204); an invalid configuration gives `400`.
- **A pause loses no data:** a source continues from where it stopped.
- **SMTs:** a chain of simple record transforms (`HoistField`, `InsertField`).
- **Converters** turn Connect's data into Kafka's bytes (in the experiment JSON without schemas).
- **Worker state** is in the topics `connect-offsets`, `connect-configs`, `connect-status` (RF=3 in production).
- **`FileStream*` are only teaching examples;** data needs specialised connectors.

## Minimal example

```python
rest("POST", "/connectors", {"name": "file-src", "config": {
    "connector.class": "org.apache.kafka.connect.file.FileStreamSourceConnector",
    "file": "/tmp/in.txt", "topic": "connect-test"}})
```

Everything was run against a real Kafka Connect 4.3.1 worker in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Task | Choice |
|---|---|
| Moving data between Kafka and a database/store | Kafka Connect |
| Simple record edits | SMTs |
| Complex processing, aggregations | stream processing |

- ❌ Do not use `FileStream*` for production data.
- ❌ Do not run a single worker in production.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | what Connect is, the REST API, a source, a sink |
| [2. Advanced theory](advanced.en.md) | the lifecycle, SMTs, configuration validation, converters |
| [3. Expert theory](expert.en.md) | reference of REST and configurations, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kafka 08-kafka-connect level 5` |
| 6. Tests | not created yet — `/learn kafka 08-kafka-connect level 6` |

## Related materials

- Prerequisites: [01 · Kafka basics](../01-kafka-basics/overview.en.md), [06 · Schemas and serialization](../06-schemas-and-serialization/overview.en.md)
- Next: [09 · Monitoring and operations](../../overview.en.md)
- Analogues in other tracks: [RabbitMQ: Federation and Shovel](../../../rabbitmq/modules/12-federation-and-shovel/overview.en.md)
