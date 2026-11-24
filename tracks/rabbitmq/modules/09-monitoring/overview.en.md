---
title: "Monitoring and operations"
lang: en
track: rabbitmq
kind: module
doc: overview
slug: 09-monitoring
part: 2
levels: [1, 2, 3, 4]
tags: [rabbitmq, monitoring, management-api, prometheus, alarms, health-check]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Monitoring and operations

🌐 [Русский](overview.ru.md) · **English**

## In short

Watch queues (depth, unacknowledged, consumers), connections and channels, and nodes (memory, disk, alarms). The tools: the management plugin (a web UI and an HTTP API on 15672), `rabbitmqctl`/`rabbitmq-diagnostics` and Prometheus metrics on 15692.

## Key ideas

- **`messages = messages_ready + messages_unacknowledged`**; read them separately.
- **The HTTP API:** `/api/queues/%2F/<queue>`, `/api/connections`, `/api/channels`, `/api/consumers`.
- **Name connections** (`connection_name`) — they are easy to find.
- **Prometheus:** `/metrics` is aggregated node metrics, `/metrics/per-object` per queue.
- **A memory/disk alarm** blocks publishers: `Connection.Blocked` (`low on memory`), the health check answers `503`.
- **`check_local_alarms` in 4.4.1 is a deprecated stub;** use `rabbitmq-diagnostics alarms` or `/api/health/checks/alarms`.
- **Management statistics lag** by seconds.

## Minimal example

```python
q = api("/queues/%2F/mon")
print(q["messages_ready"], q["messages_unacknowledged"], q["consumers"])   # 7 3 1
```

Everything was run against a real RabbitMQ 4.4.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Need | Tool |
|---|---|
| A quick look at queues | management UI / `rabbitmqctl list_queues` |
| History, graphs, alerts | Prometheus + Grafana |
| Liveness/readiness checks | `/api/health/checks/alarms`, `check_running` |
| A client's reaction to resource shortage | `Connection.Blocked` |

- ❌ Do not rely on the deprecated `check_local_alarms`.
- ❌ Do not leave `guest`/`guest` in production.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | the HTTP API, connections and channels, the CLI, the health check |
| [2. Advanced theory](advanced.en.md) | Prometheus, memory and disk alarms, what to alert on |
| [3. Expert theory](expert.en.md) | reference of the API, CLI and metrics, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn rabbitmq 09-monitoring level 5` |
| 6. Tests | not created yet — `/learn rabbitmq 09-monitoring level 6` |

## Related materials

- Prerequisites: [03 · Queues and delivery](../03-queues-and-delivery/overview.en.md), [08 · Clustering](../08-clustering/overview.en.md)
- Next: [10 · Flow control and performance](../../overview.en.md)
- Analogues in other tracks: [Linux](../../../linux/overview.en.md)
