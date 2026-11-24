---
title: "RabbitMQ"
lang: en
track: rabbitmq
kind: track
doc: overview
slug: rabbitmq
tags: [rabbitmq, amqp, message-broker, messaging, queues]
versions: "RabbitMQ 4.4; AMQP 0-9-1; Python pika"
default_level: 4
status: draft
updated: 2026-11-24
---

# RabbitMQ

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

RabbitMQ is an open-source message broker. Producers send messages to the broker, the broker routes them through exchanges, bindings and queues, and consumers take them from the queues. This decouples services in time (the consumer may be down), in speed (a queue absorbs a burst) and in knowledge (the producer does not know who reads the message).

The classic protocol is AMQP 0-9-1; RabbitMQ also speaks AMQP 1.0, MQTT, STOMP and has its own log-like queue type, streams. The track starts with the AMQP model (connection, channel, exchange, queue, binding), then covers delivery guarantees, queue types, patterns, operations and security.

**Prerequisites:** basic networking and one programming language; the [Python track](../python/overview.en.md) is enough for the examples.

## Environment

| What | Tool |
|---|---|
| Broker | `rabbitmq:4-management` in Docker: `docker run -d --name rabbit -p 5672:5672 -p 15672:15672 rabbitmq:4-management` |
| Management UI | `http://localhost:15672`, user `guest`, password `guest` (only from localhost) |
| CLI | `rabbitmqctl` and `rabbitmq-diagnostics` inside the container: `docker exec rabbit rabbitmqctl status` |
| Example language | Python 3.12+ with the `pika` client |
| Module code | as in the Python track: `code/` with `pyproject.toml`, `examples/`, `solutions/`, `tests/` |
| Run all module tests | `uv run pytest` inside `code/` |
| Tests and the broker | tests that need a live broker are marked `integration` and skipped when `localhost:5672` is unavailable; the rest use no network |
| Disposable broker | `docker rm -f rabbit` removes the broker and its data |

> The `guest` account and the open ports are for a disposable local broker only; see the security module for production.

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Default detail level for modules: **4** (theory and self-check), see [detail levels](../../README.en.md#detail-levels).

### Part 1. Fundamentals

- [01 · Messaging and the AMQP model](modules/01-messaging-basics/overview.en.md) — broker, connection, channel, exchange, queue, binding, first publish and consume
- [02 · Exchanges and routing](modules/02-exchanges-and-routing/overview.en.md) — default, direct, fanout, topic and headers exchanges, alternate exchange
- [03 · Queues and delivery](modules/03-queues-and-delivery/overview.en.md) — acknowledgements, prefetch, durability, TTL, exclusive and auto-delete queues
- [04 · Messaging patterns](modules/04-messaging-patterns/overview.en.md) — work queue, publish/subscribe, routing, RPC

### Part 2. Advanced topics

- [05 · Reliability](modules/05-reliability/overview.en.md) — publisher confirms, persistence, the mandatory flag, transactions, idempotent consumers
- [06 · Dead-lettering and retries](modules/06-dead-lettering-and-retries/overview.en.md) — DLX, TTL, delayed retries, poison messages
- [07 · Queue types](modules/07-queue-types/overview.en.md) — classic, quorum queues, streams
- [08 · Clustering and high availability](modules/08-clustering/overview.en.md) — nodes, quorum, partitions, upgrades
- [09 · Monitoring and operations](modules/09-monitoring/overview.en.md) — management UI, metrics, Prometheus, alarms

### Part 3. Expert topics

- [10 · Flow control and performance](modules/10-performance/overview.en.md) — credit flow, memory and disk alarms, tuning
- [11 · Security](modules/11-security/overview.en.md) — vhosts, permissions, TLS, authentication
- [12 · Federation and Shovel](modules/12-federation-and-shovel/overview.en.md) — links between brokers and data centres
- [13 · Internals](modules/13-internals/overview.en.md) — Erlang runtime, metadata store, message store
- [14 · RabbitMQ on Kubernetes](modules/14-kubernetes/overview.en.md) — the Cluster Operator, storage and upgrades

## Libraries and tools

- [pika](libraries/pika/overview.en.md) — the synchronous Python client
- [aio-pika](libraries/aio-pika/overview.en.md) — the asyncio client
- [MassTransit (C#)](../csharp/libraries/masstransit/overview.en.md) — a client library over RabbitMQ for .NET
- [rabbitmqctl](tools/rabbitmqctl/overview.en.md) — node and cluster management from the command line

## Recipes

- [Task queue with retries](recipes/task-queue-with-retries/overview.en.md) — acknowledgements, DLX and backoff
- [RPC over RabbitMQ](recipes/rpc/overview.en.md) — reply queues and correlation ids
- [Transactional outbox](recipes/outbox/overview.en.md) — reliable publication from a database transaction

## Projects

- [Order pipeline](projects/order-pipeline/overview.en.md) — several services exchanging events through topic exchanges

## Related tracks

- [Kafka](../kafka/overview.en.md) — an event log instead of a queue: comparing the models
- [Python](../python/overview.en.md) — language of the examples
- [Kubernetes](../kubernetes/overview.en.md) — running the broker with the operator
- [Linux](../linux/overview.en.md) — sockets, file descriptors and diagnostics
- [C# background jobs](../csharp/recipes/background-jobs/overview.en.md) — consuming work in a .NET service

## Sources

- [RabbitMQ documentation](https://www.rabbitmq.com/docs)
- [AMQP 0-9-1 model explained](https://www.rabbitmq.com/tutorials/amqp-concepts)
- [RabbitMQ tutorials](https://www.rabbitmq.com/tutorials)
- [pika documentation](https://pika.readthedocs.io/en/stable/)
