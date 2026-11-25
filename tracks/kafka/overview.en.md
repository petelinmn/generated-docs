---
title: "Apache Kafka"
lang: en
track: kafka
kind: track
doc: overview
slug: kafka
tags: [kafka, event-streaming, log, partition, consumer-group, message-broker]
versions: "Apache Kafka 4.3; Python confluent-kafka 2.16"
default_level: 4
status: draft
updated: 2026-11-24
---

# Apache Kafka

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Apache Kafka is a distributed event-streaming platform. It is built on a **log**: topics are split into partitions, and each partition is an ordered sequence of records that are only appended to. Producers write records, consumers read them by offset and decide themselves where to start and how fast to read. Messages are not deleted on reading: they are kept until the retention period expires (or compaction squeezes them), so the same data can be read by many independent consumers and re-read when needed.

Kafka differs from a classic queue broker (see [RabbitMQ](../rabbitmq/overview.en.md)) in its model: a log with re-reading and scaling by partitions instead of routing through exchanges and deleting acknowledged messages. The track starts with the model (topic, partition, offset, consumer group), then covers delivery guarantees, replication, data schemas, integration and operations.

**Prerequisites:** basic networking and one programming language; the [Python track](../python/overview.en.md) is enough for the examples.

## Environment

| What | Tool |
|---|---|
| Broker | `apache/kafka:4.3.1` in Docker (KRaft mode, no ZooKeeper): `docker run -d --name kafka -p 9092:9092 apache/kafka:4.3.1` |
| CLI | the scripts `/opt/kafka/bin/kafka-*.sh` inside the container: `docker exec kafka /opt/kafka/bin/kafka-topics.sh --bootstrap-server localhost:9092 --list` |
| Example language | Python 3.12+ with the `confluent-kafka` client |
| Module code | as in the Python track: `code/` with `pyproject.toml`, `examples/`, `solutions/`, `tests/` |
| Run module tests | `uv run pytest` inside `code/` |
| Tests and the broker | tests that need a live broker are marked `integration` and skipped when `localhost:9092` is unavailable; the rest use no network |
| Disposable broker | `docker rm -f kafka` removes the broker and its data |

> On Windows in Git Bash set `MSYS_NO_PATHCONV=1` before `docker exec … /opt/kafka/…` commands, otherwise the path is mangled. A single broker and port 9092 without TLS are for a local disposable experiment only; security setup is in the corresponding module.

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Default detail level for modules: **4** (theory and self-check), see [detail levels](../../README.en.md#detail-levels).

### Part 1. Fundamentals

- [01 · Kafka basics](modules/01-kafka-basics/overview.en.md) — the log, topic, partition, offset, broker, KRaft, a first write and read
- [02 · Producers](modules/02-producers/overview.en.md) — keys and partitioning, `acks`, batching, idempotence
- [03 · Consumers and groups](modules/03-consumers-and-groups/overview.en.md) — consumer groups, offset commits, rebalancing
- [04 · Topics, retention and compaction](modules/04-topics-and-retention/overview.en.md) — partitions, retention, segments, compaction, record order

### Part 2. Advanced topics

- [05 · Delivery semantics](modules/05-delivery-semantics/overview.en.md) — at-most-once, at-least-once, exactly-once, transactions
- [06 · Schemas and serialization](modules/06-schemas-and-serialization/overview.en.md) — JSON, Avro, Protobuf, Schema Registry, schema evolution
- [07 · Replication and availability](modules/07-replication-and-availability/overview.en.md) — leaders, ISR, `min.insync.replicas`, leader election
- [08 · Kafka Connect](modules/08-kafka-connect/overview.en.md) — sources and sinks, connectors, transforms
- [09 · Monitoring and operations](modules/09-monitoring/overview.en.md) — consumer lag, metrics, the CLI, alerts

### Part 3. Expert topics

- [10 · Performance](modules/10-performance/overview.en.md) — throughput, latency, compression, client and broker tuning
- [11 · Security](modules/11-security/overview.en.md) — TLS, SASL, ACLs, quotas
- [12 · Stream processing](modules/12-stream-processing/overview.en.md) — Kafka Streams, ksqlDB, windows, state
- [13 · Internals](modules/13-internals/overview.en.md) — log segments, indexes, the KRaft cluster, the page cache
- [14 · Kafka on Kubernetes](modules/14-kubernetes/overview.en.md) — Strimzi, storage, upgrades

## Libraries and tools

- [confluent-kafka](libraries/confluent-kafka/overview.en.md) — the Python client built on librdkafka
- [Kafka CLI](tools/kafka-cli/overview.en.md) — the scripts `kafka-topics`, `kafka-console-*`, `kafka-consumer-groups` and others

## Recipes

- [A consumer with retries and a dead-letter topic](recipes/consumer-with-retries/overview.en.md) — handling failures without stopping a partition
- [An exactly-once pipeline](recipes/exactly-once-pipeline/overview.en.md) — consume-process-produce with transactions
- [Transactional outbox](recipes/outbox/overview.en.md) — reliable event publication from a database transaction

## Projects

- [Clickstream pipeline](projects/clickstream-pipeline/overview.en.md) — event ingestion, processing and aggregates across several topics

## Related tracks

- [RabbitMQ](../rabbitmq/overview.en.md) — a classic queue broker: comparing the models
- [OpenSearch](../opensearch/overview.en.md) — indexing an event stream for search
- [Python](../python/overview.en.md) — the language of the examples
- [Kubernetes](../kubernetes/overview.en.md) — running Kafka with an operator
- [Linux](../linux/overview.en.md) — the file system, the page cache and diagnostics
- [C# background jobs](../csharp/recipes/background-jobs/overview.en.md) — consuming work in a .NET service

## Sources

- [Apache Kafka documentation](https://kafka.apache.org/documentation/)
- [Apache Kafka: Introduction](https://kafka.apache.org/intro)
- [confluent-kafka-python documentation](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html)
