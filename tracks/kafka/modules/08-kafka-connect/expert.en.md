---
title: "Kafka Connect: expert level"
lang: en
track: kafka
kind: module
doc: expert
slug: 08-kafka-connect
part: 2
tags: [kafka-connect, rest-api, connector, task, converter, smt, worker-config]
versions: "Apache Kafka 4.3.1 (Kafka Connect, distributed mode)"
status: draft
updated: 2026-11-24
---

# Kafka Connect: expert level

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on a Connect 4.3.1 worker (distributed, one worker) next to a three-broker cluster.

## Reference

### The worker's REST API

| Request | Purpose | Verified |
|---|---|---|
| `GET /` | version, cluster id | yes |
| `GET /connector-plugins` | the available classes | yes |
| `GET /connectors` | the list of connectors | yes |
| `POST /connectors` | create (`201`); an invalid configuration — `400` | yes |
| `GET /connectors/{name}/status` | connector and task state | yes |
| `PUT /connectors/{name}/pause` / `resume` | pause/resume (`202`) | yes |
| `GET /connectors/{name}/offsets` | the connector's positions | yes (empty for FileStream) |
| `DELETE /connectors/{name}` | delete (`204`) | yes |
| `PUT …/config`, `POST …/restart`, `GET …/tasks`, `PUT …/stop`, `DELETE …/offsets` | other | not tested |

### Worker configuration (distributed)

| Parameter | Purpose | Verified |
|---|---|---|
| `bootstrap.servers`, `group.id` | the cluster and the worker group | yes |
| `key.converter`, `value.converter` | converters | yes (`JsonConverter`) |
| `*.converter.schemas.enable` | whether to include the schema in JSON | yes (`false`) |
| `offset.storage.topic`, `config.storage.topic`, `status.storage.topic` | internal topics | yes |
| `*.storage.replication.factor` | the RF of internal topics | yes (3) |
| `plugin.path` | the plugin directory | yes |
| `rest.advertised.host.name`, `rest.port` | the REST address | yes |

### Connector configuration

| Parameter | Purpose | Verified |
|---|---|---|
| `connector.class` | the connector class | yes |
| `tasks.max` | maximum tasks | set (1) |
| `topic` (source) / `topics` (sink) | the topic(s) | yes |
| `file` | a file path (FileStream) | yes |
| `transforms`, `transforms.<name>.type` and parameters | an SMT chain | yes (`HoistField$Value`, `InsertField$Value`) |
| `errors.tolerance`, `errors.deadletterqueue.*`, connector-level `key.converter` | errors and converters | not tested |

### States (verified)

`RUNNING`, `PAUSED` (connector and task).

## Internals

- **The worker group** uses Kafka's group protocol to distribute connectors and tasks; state is kept in the topics `connect-configs`, `connect-offsets`, `connect-status`.
- **A source** keeps a position ("where I am in the file") to continue after a pause/restart (verified by the pause behaviour).
- **A converter** turns Connect's internal data into bytes (verified: JSON strings in the topic).
- **SMTs** apply to a record in order between the connector and the converter (verified by the chain's result).
- **Configuration validation** happens before creation (verified: `400`).

> ⚠️ Verify: task distribution among workers when they are added/fail, exactly-once for sources, error handling and a sink's dead-letter topic, third-party connector plugins were not verified here (one worker ran).

## Gotchas

### 1. `topic` and `topics`

A source has `topic`, a sink `topics` (verified).

### 2. A pause does not lose source data

The file was appended during the pause; the record appeared after `resume` (verified).

### 3. `FileStream*` are only examples

Production data needs other connectors (per the documentation).

### 4. Empty positions

`GET …/offsets` returned an empty list (verified), the cause was not established.

### 5. The configuration is checked in advance

A missing required parameter gives `400`, the connector is not created (verified).

### 6. An SMT's result depends on order

First `HoistField`, then `InsertField` (verified). `InsertField` for a bare string needs a structure; the reverse order was not tested here.

### 7. One worker is not fault tolerance

Fault tolerance needs several workers.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| Kafka Connect | ready connectors, management over REST | separate infrastructure |
| your own consumer/producer | full control | more code and operations |
| SMTs | simple edits with no code | not for complex logic |
| stream processing (Kafka Streams) | arbitrary transformations | more code |

## Interview questions

<details><summary>1. What is Kafka Connect?</summary>

A framework for streaming integration of Kafka with external systems: sources, sinks, tasks, workers.

→ [Basics](basics.en.md#what-kafka-connect-is)

</details>

<details><summary>2. How does a source differ from a sink?</summary>

A source reads from an external system into Kafka, a sink writes from Kafka to an external system.

→ [Basics](basics.en.md#what-kafka-connect-is)

</details>

<details><summary>3. How do you manage a connector?</summary>

Through the worker's REST API: create, status, pause, resume, delete.

→ [Basics](basics.en.md#the-rest-api)

</details>

<details><summary>4. Trick: do file lines get lost after `pause`?</summary>

No: they appear after `resume`.

→ [Gotcha 2](#2-a-pause-does-not-lose-source-data)

</details>

<details><summary>5. What is an SMT and where does it apply?</summary>

Transforms of single records in a chain between the connector and the converter.

→ [Advanced theory](advanced.en.md#single-message-transforms-smt)

</details>

<details><summary>6. Where does a worker keep state?</summary>

In the topics `connect-configs`, `connect-offsets`, `connect-status`.

→ [Reference](#worker-configuration-distributed)

</details>

<details><summary>7. Trick: what is the topic parameter called on a source and a sink?</summary>

`topic` and `topics` respectively.

→ [Gotcha 1](#1-topic-and-topics)

</details>

<details><summary>8. What does creating a connector with an incomplete configuration return?</summary>

`400` with an error description; the connector is not created.

→ [Gotcha 5](#5-the-configuration-is-checked-in-advance)

</details>

<details><summary>9. What is a converter for?</summary>

It turns Connect's data into Kafka's bytes and back (JSON, Avro and so on).

→ [Advanced theory](advanced.en.md#converters)

</details>

<details><summary>10. Trick: is one worker enough for a production system?</summary>

No: you need several workers and RF=3 for the internal topics.

→ [Gotcha 7](#7-one-worker-is-not-fault-tolerance)

</details>

<details><summary>11. When do you need stream processing instead of an SMT?</summary>

For complex transformations, aggregations and joins.

→ [Trade-offs](#trade-offs-and-alternatives)

</details>

<details><summary>12. Why are `FileStream*` unfit for production data?</summary>

They are teaching examples shipped with Kafka (per the documentation).

→ [Gotcha 3](#3-filestream-are-only-examples)

</details>

## Cheat sheet

```text
POST /connectors {"name":..., "config": {"connector.class":..., "tasks.max":..., "topic"|"topics":..., ...}}   # 201 | 400
GET /connectors/N/status ; PUT /connectors/N/pause|resume (202) ; DELETE /connectors/N (204)
SMT: "transforms":"a,b", "transforms.a.type":"org.apache.kafka.connect.transforms.HoistField$Value", ...
worker: connect-distributed.sh worker.properties (internal topics RF=3, plugin.path)
```

- A source takes `topic`, a sink `topics`.
- Several workers and RF=3 in production.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [07 · Replication and availability](../07-replication-and-availability/expert.en.md)
- Next module: [09 · Monitoring and operations](../../overview.en.md)

## Sources

- [Apache Kafka: Kafka Connect](https://kafka.apache.org/documentation/#connect)
- [Apache Kafka: Connect REST API](https://kafka.apache.org/documentation/#connect_rest)
- [Apache Kafka: Connect transformations](https://kafka.apache.org/documentation/#connect_transforms)
