---
title: "Monitoring and operations: advanced"
lang: en
track: rabbitmq
kind: module
doc: advanced
slug: 09-monitoring
part: 2
tags: [rabbitmq, prometheus, alarms, memory-alarm, connection-blocked, health-check, alerting]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Monitoring and operations: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run on a single RabbitMQ 4.4.1 node with pika 1.5.0.

## Prometheus

The `rabbitmq_prometheus` plugin serves metrics in the Prometheus text format on port 15692. In the `rabbitmq:4-management` image it is already enabled (`rabbitmq-plugins enable rabbitmq_prometheus` answered "Plugin configuration unchanged").

```text
metric rabbitmq_queue_messages_ready -> rabbitmq_queue_messages_ready 7
metric rabbitmq_queue_messages_unacked -> rabbitmq_queue_messages_unacked 3
metric rabbitmq_queue_consumers -> rabbitmq_queue_consumers 1
metric rabbitmq_connections -> rabbitmq_connections 2
metric rabbitmq_channels -> rabbitmq_channels 2
metric rabbitmq_process_resident_memory_bytes -> rabbitmq_process_resident_memory_bytes 149659648
metric rabbitmq_disk_space_available_bytes -> rabbitmq_disk_space_available_bytes 993590013952
metric rabbitmq_identity_info -> rabbitmq_identity_info{rabbitmq_node="rabbit@4ff649c7c4dc",rabbitmq_cluster="rabbit@4ff649c7c4dc",ra…
```

- The default address `/metrics` returns **aggregated** values per node (sums over queues): `rabbitmq_queue_messages_ready 7`.
- The address `/metrics/per-object` returns values **per queue** with labels: `rabbitmq_queue_messages_ready{vhost="/",queue="mon"} 7`. There are more series, so on large installations use it deliberately.
- The labels `rabbitmq_node` and `rabbitmq_cluster` in `rabbitmq_identity_info` tell nodes and clusters apart.

Prometheus scrapes metrics on a schedule and keeps history, and alerts and graphs are built in Alertmanager and Grafana.

> ⚠️ Verify: ready-made Grafana dashboards for RabbitMQ and the Prometheus/Alertmanager configuration were not verified here.

## Memory and disk alarms

The broker protects itself: when memory or disk space runs low it raises an **alarm** and blocks publishing connections. Experiment: the memory threshold was lowered to 10 MB (`rabbitmqctl set_vm_memory_high_watermark absolute 10MB`) and a message was published.

```text
health alarms: (503, {'reason': 'There are alarms in effect in the cluster', 'status': 'failed', 'alarms': [{'node': 'rabbit@4ff649c7c4dc', 'resource': 'memory'}]})
blocked callback reasons: [<METHOD([... "method=<Connection.Blocked(['reason=low on memory'])>"])>]
conn states: ['blocking']
```

- `GET /api/health/checks/alarms` answered `503` and listed the node and resource (`memory`).
- The client received `Connection.Blocked` with the reason `low on memory`; the broker tells a publishing connection through the callback `add_on_connection_blocked_callback`.
- The connection state in the API is `blocking`.

After the threshold was restored (`set_vm_memory_high_watermark 0.6`):

```text
health alarms after: (200, {'status': 'ok'})
conn states after: ['running']
```

The client should react to `blocked`: not pile messages up in memory and republish after `unblocked`.

> In 4.4.1 `rabbitmq-diagnostics check_local_alarms` prints "This command is DEPRECATED and is a no-op" and does not show an alarm even when one is active. During an active memory alarm (verified) the HTTP check answered `503` and `rabbitmq-diagnostics alarms` printed `Memory alarm on node …`. Use those.

## What to alert on

Practice (from the documentation and operating experience); each needs thresholds tuned to your load:

| Signal | Reason to alert |
|---|---|
| `messages_ready` grows | consumers cannot keep up |
| `messages_unacknowledged` is large and does not fall | a stuck consumer or too large a prefetch |
| `consumers = 0` on an important queue | nobody reads |
| memory/disk alarms | the broker blocks publishers |
| connections/channels keep growing | a leak in the application |
| the dead-letter queue is non-empty | problem messages ([module 06](../06-dead-lettering-and-retries/advanced.en.md)) |

> ⚠️ Verify: concrete thresholds, recommended rule sets and the list of critical metrics were not verified here and depend on load.

## Best practices

- Monitor queues, nodes and applications together; name connections.
- Collect metrics in Prometheus; on large installations enable per-object deliberately.
- Check `/api/health/checks/alarms` in liveness and readiness probes.
- React to `Connection.Blocked` in the client.
- Alert on the trend (growing depth), not only on a threshold.

## Common mistakes

- ❌ Relying on `check_local_alarms` → ✅ the HTTP health check (verified: the command is a no-op).
- ❌ Reading per-object metrics with a huge number of queues needlessly → ✅ aggregated ones.
- ❌ Ignoring `Connection.Blocked` in the client → ✅ pause publishing.
- ❌ Alerting only on depth → ✅ also on `consumers`, unacked, DLQ.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [05 · Reliability](../05-reliability/advanced.en.md) — publisher behaviour when blocked
- Next module: [10 · Flow control and performance](../../overview.en.md)

## Sources

- [Monitoring with Prometheus and Grafana](https://www.rabbitmq.com/docs/prometheus)
- [Alarms](https://www.rabbitmq.com/docs/alarms)
- [Memory Alarms / Memory Use](https://www.rabbitmq.com/docs/memory)
- [Connection Blocked notifications](https://www.rabbitmq.com/docs/connection-blocked)
