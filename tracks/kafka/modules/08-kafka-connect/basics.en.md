---
title: "Kafka Connect: basic level"
lang: en
track: kafka
kind: module
doc: basics
slug: 08-kafka-connect
part: 2
tags: [kafka-connect, connector, source-connector, sink-connector, rest-api, distributed-mode]
versions: "Apache Kafka 4.3.1 (Kafka Connect, distributed mode)"
status: draft
updated: 2026-11-24
---

# Kafka Connect: basic level

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · Kafka basics](../01-kafka-basics/basics.en.md), [06 · Schemas and serialization](../06-schemas-and-serialization/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

A Kafka Connect worker (4.3.1) ran in **distributed** mode in a container based on the image `apache/kafka:4.3.1` with the command `connect-distributed.sh` on the Docker network `kfk` next to the three-broker cluster ([module 07](../07-replication-and-availability/basics.en.md#how-the-code-was-verified)); the REST API is published at `http://localhost:18083`. The worker's configuration file: `bootstrap.servers=kafka1:19092,…`, `group.id=connect-cluster`, `JsonConverter` for keys and values (`schemas.enable=false`), internal topics `connect-offsets`, `connect-configs`, `connect-status` with RF=3, `plugin.path=/tmp/plugins` (with `connect-file-*.jar` copied from `/opt/kafka/libs`). Outputs are copied from the runs. The helper `rest(method, path, body)` calls the REST API and returns `(code, JSON)`; `sh(cmd)` runs a command in the `connect` container; `read(topic, n)` reads a topic. **The `FileStream*` connectors are teaching examples shipped with Kafka**, not production solutions for data (per the documentation).

## What Kafka Connect is

**Kafka Connect** is a part of Apache Kafka for streaming integration with external systems without writing code: **connectors** move data between Kafka and a database, files, storage and so on.

| Kind | Direction | Example |
|---|---|---|
| **Source** | an external system → Kafka | `FileStreamSourceConnector`: lines of a file → records of a topic |
| **Sink** | Kafka → an external system | `FileStreamSinkConnector`: records of a topic → lines of a file |

Connect processes are called **workers**; a connector is divided into **tasks** that do the work in parallel. In distributed mode the workers form a group, share the tasks among themselves and keep state in Kafka topics.

## The REST API

Connectors are managed through the worker's REST API (port 8083):

```text
GET /                       -> {"version":"4.3.1", ... "kafka_cluster_id": ...}
GET /connector-plugins      -> the available connector classes
POST /connectors            -> create a connector
```

The available plugins (in our `plugin.path`): `FileStreamSinkConnector`, `FileStreamSourceConnector`, and also `MirrorSourceConnector`, `MirrorCheckpointConnector`, `MirrorHeartbeatConnector` (replication between clusters, built into Kafka).

## A source: a file → a topic

```python
rest("POST", "/connectors", {"name": "file-src", "config": {
    "connector.class": "org.apache.kafka.connect.file.FileStreamSourceConnector", "tasks.max": "1",
    "file": "/tmp/in.txt", "topic": "connect-test"}})
```

```text
create: 201 file-src source
status: {"name": "file-src", "connector": {"state": "RUNNING", ...}, "tasks": [{"id": 0, "state": "RUNNING", ...}], "type": "source"}
topic: ['"line one"', '"line two"']
after append: ['"line one"', '"line two"', '"line three"']
```

- The connector was created (code `201`), it and its task are `RUNNING`.
- The file's lines became records of the topic `connect-test` (values are JSON strings: `"line one"`).
- The line `line three` appended to the file **appeared in the topic** by itself: the source watches the file.

## A sink: a topic → a file

```python
rest("POST", "/connectors", {"name": "file-sink", "config": {
    "connector.class": "org.apache.kafka.connect.file.FileStreamSinkConnector", "tasks.max": "1",
    "file": "/tmp/out.txt", "topics": "connect-test"}})
```

```text
sink file: ['line one', 'line two', 'line three']
```

The sink wrote the topic's contents to a file. Note: the source has the parameter `topic` (one), the sink `topics` (a list).

## Common beginner mistakes

- ❌ Writing code for a simple integration → ✅ first check whether a ready connector exists.
- ❌ Using `FileStream*` in production → ✅ they are examples; use specialised connectors for files and databases.
- ❌ Mixing up `topic` on a source and `topics` on a sink → ✅ different parameters.
- ❌ Not watching the state → ✅ `GET /connectors/<name>/status`.
- ❌ A single worker in production → ✅ several workers and RF=3 for the internal topics.

## Related materials

- Next: [advanced theory](advanced.en.md) — lifecycle, transforms, configuration validation
- Before: [07 · Replication and availability](../07-replication-and-availability/basics.en.md)
- Next module: [09 · Monitoring and operations](../../overview.en.md)

## Sources

- [Apache Kafka: Kafka Connect](https://kafka.apache.org/documentation/#connect)
- [Apache Kafka: Connect REST API](https://kafka.apache.org/documentation/#connect_rest)
- [Apache Kafka: Connect configs](https://kafka.apache.org/documentation/#connectconfigs)
