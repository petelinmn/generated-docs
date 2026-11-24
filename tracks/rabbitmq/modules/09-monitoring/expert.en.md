---
title: "Monitoring and operations: expert"
lang: en
track: rabbitmq
kind: module
doc: expert
slug: 09-monitoring
part: 2
tags: [rabbitmq, monitoring, management-api, prometheus, alarms, rabbitmq-diagnostics, health-check]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Monitoring and operations: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on a single RabbitMQ 4.4.1 node.

## Reference

### The management HTTP API

| Request | Returns | Verified |
|---|---|---|
| `GET /api/overview` | version, cluster name, `queue_totals`, `message_stats` | yes |
| `GET /api/queues/{vhost}/{queue}` | `messages`, `messages_ready`, `messages_unacknowledged`, `consumers`, `state`, `type`, `message_stats` | yes |
| `GET /api/connections` | connections; `client_properties`, `state` | yes |
| `GET /api/channels` | channels; `prefetch_count`, `messages_unacknowledged` | yes |
| `GET /api/consumers` | consumers; `queue`, `prefetch_count`, `ack_required` | yes |
| `GET /api/nodes` | nodes: memory, disk, alarm flags | invoked |
| `GET /api/health/checks/alarms` | `200 ok` or `503` with the list of alarms | yes |
| other `GET /api/health/checks/*`, `/api/definitions`, `/api/vhosts`, `/api/users` | checks, definitions export | not tested |

The vhost name `/` is encoded as `%2F`.

### The command line

| Command | Purpose | Verified |
|---|---|---|
| `rabbitmqctl list_queues name messages_ready messages_unacknowledged consumers` | queue summary | yes |
| `rabbitmqctl list_connections` / `list_channels` / `list_consumers` | who is connected | not tested |
| `rabbitmqctl set_vm_memory_high_watermark <fraction>` / `absolute <size>` | memory threshold | yes |
| `rabbitmq-diagnostics check_running` | the node is running | yes |
| `rabbitmq-diagnostics check_local_alarms` | **deprecated, does nothing** in 4.4.1 | yes |
| `rabbitmq-diagnostics status`, `memory_breakdown`, `ping` | state and memory breakdown | not tested |

### Prometheus metrics (port 15692)

| Address | Content |
|---|---|
| `/metrics` | aggregated node metrics: `rabbitmq_queue_messages_ready`, `rabbitmq_queue_messages_unacked`, `rabbitmq_queue_consumers`, `rabbitmq_connections`, `rabbitmq_channels`, `rabbitmq_process_resident_memory_bytes`, `rabbitmq_disk_space_available_bytes`, `rabbitmq_identity_info` (all verified) |
| `/metrics/per-object` | per-queue values with labels `vhost`, `queue` (`rabbitmq_queue_messages_ready` verified) |
| other paths (`/metrics/detailed`) | extended metrics | not tested |

## Internals

- **Management statistics are collected periodically,** so numbers lag reality by seconds; in the experiment we waited 6 seconds.
- **Memory/disk alarms stop publishers:** the broker sends `Connection.Blocked`, the connection in the API is `blocking` (verified); once the alarm clears — `Connection.Unblocked` and `running` (state verified).
- **`messages = ready + unacked`:** unacknowledged messages stay with the queue until the consumer acks or goes away.
- **Prometheus pulls metrics,** so the broker does not need to know the monitoring server's address.

> ⚠️ Verify: the internals of statistics collection (the default update interval), the exact semantics of memory thresholds as a percentage versus an absolute value, and `unblocked` as a client-side event were not verified here.

## Gotchas

### 1. `check_local_alarms` does nothing

In 4.4.1 the command prints that it is "DEPRECATED and is a no-op" and reports "no local alarms" while an alarm is active (verified). Use `/api/health/checks/alarms` (503) or `rabbitmq-diagnostics alarms` (`Memory alarm on node …`) — both verified during an active alarm.

### 2. API numbers lag

Right after a publish `messages_ready` in management may be stale.

### 3. `/metrics` does not show queues individually

You need `/metrics/per-object` (verified: the label `queue="mon"` appears only there).

### 4. `guest`/`guest`

The teaching image accepted these credentials through the published port; in production such a user is a hole ([module 11](../../overview.en.md)).

### 5. An alarm blocks publishers, not consumers

Publishing connections are blocked; reading continues ([documentation](https://www.rabbitmq.com/docs/alarms)).

### 6. The client must react to `Connection.Blocked`

Pika invoked the callback with the reason `low on memory` (verified).

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| Management UI/API | simple, visual | load on the node, statistics delay |
| Prometheus + Grafana | history, alerts, a standard | separate infrastructure |
| per-object metrics | every queue visible | many series |
| `rabbitmqctl` in scripts | no HTTP | needs access to the node |

## Interview questions

<details><summary>1. Which queue indicators do you check first?</summary>

`messages_ready`, `messages_unacknowledged`, `consumers`, and `state`.

→ [Basics](basics.en.md#what-to-watch-first)

</details>

<details><summary>2. Trick: `messages` = 10, `messages_ready` = 7. What are the other three?</summary>

Unacknowledged: a consumer holds them (`messages_unacknowledged`).

→ [Basics](basics.en.md#the-management-http-api)

</details>

<details><summary>3. How do you inspect queues without HTTP?</summary>

`rabbitmqctl list_queues name messages_ready messages_unacknowledged consumers`.

→ [Basics](basics.en.md#the-command-line)

</details>

<details><summary>4. How do you find an application by its connection?</summary>

Set `connection_name` in `client_properties` and look it up in `/api/connections`.

→ [Basics](basics.en.md#connections-channels-consumers)

</details>

<details><summary>5. What is the difference between `/metrics` and `/metrics/per-object`?</summary>

The first is aggregated node metrics, the second is per-queue values with labels.

→ [Advanced theory](advanced.en.md#prometheus)

</details>

<details><summary>6. Trick: `check_local_alarms` said "no alarms". Does that mean there are none?</summary>

No: in 4.4.1 the command is deprecated and does nothing. Check `/api/health/checks/alarms`.

→ [Gotcha 1](#1-check_local_alarms-does-nothing)

</details>

<details><summary>7. What happens on a memory alarm?</summary>

The broker blocks publishing connections: the client gets `Connection.Blocked` (`low on memory`), the health check answers 503.

→ [Advanced theory](advanced.en.md#memory-and-disk-alarms)

</details>

<details><summary>8. How should a client react to blocking?</summary>

Subscribe to the blocked/unblocked callback, pause publishing and not pile messages up in memory.

→ [Advanced theory](advanced.en.md#memory-and-disk-alarms)

</details>

<details><summary>9. Trick: why do management numbers differ from ones just read?</summary>

Statistics are updated periodically, so they lag.

→ [Gotcha 2](#2-api-numbers-lag)

</details>

<details><summary>10. What do you alert on first?</summary>

Growing `messages_ready`, a large non-falling `messages_unacknowledged`, `consumers = 0`, memory/disk alarms, a non-empty DLQ.

→ [Advanced theory](advanced.en.md#what-to-alert-on)

</details>

<details><summary>11. Trick: the vhost name in an API URL?</summary>

`/` is encoded as `%2F`.

→ [Reference](#the-management-http-api)

</details>

<details><summary>12. Why does Prometheus need no configuration on the broker?</summary>

It scrapes port 15692 itself (pull); the plugin only publishes metrics.

→ [Internals](#internals)

</details>

## Cheat sheet

```text
queue     : GET /api/queues/%2F/<q> -> messages_ready, messages_unacknowledged, consumers, state
health    : GET /api/health/checks/alarms   (200 ok | 503 + alarms)   # check_local_alarms is a no-op in 4.4.1
prom      : :15692/metrics (aggregated)  :15692/metrics/per-object (per queue)
blocked   : Connection.Blocked(reason="low on memory") -> pause publishing
```

- Watch `ready`, `unacked` and `consumers`.
- Check alarms over HTTP.
- Name your connections.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [08 · Clustering](../08-clustering/expert.en.md)
- Next module: [10 · Flow control and performance](../../overview.en.md)

## Sources

- [Management Plugin](https://www.rabbitmq.com/docs/management)
- [Monitoring](https://www.rabbitmq.com/docs/monitoring)
- [Prometheus](https://www.rabbitmq.com/docs/prometheus)
- [Alarms](https://www.rabbitmq.com/docs/alarms)
