---
title: "Monitoring and operations: basics"
lang: en
track: rabbitmq
kind: module
doc: basics
slug: 09-monitoring
part: 2
tags: [rabbitmq, monitoring, management-api, rabbitmqctl, queue-depth, unacked-messages]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Monitoring and operations: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [03 · Queues and delivery](../03-queues-and-delivery/basics.en.md), [08 · Clustering](../08-clustering/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

One RabbitMQ 4.4.1 node in Docker (`rabbitmq:4-management`, ports 5672, 15672 and 15692) and pika 1.5.0; outputs are copied from the runs. The helper `api(path)` calls the management HTTP API (`http://localhost:15672/api…`, user `guest`/`guest` of the teaching image) and returns parsed JSON, `sh(*cmd)` runs a command inside the container (`docker exec rabbit …`). For the experiment 10 messages were published to the queue `mon` and a consumer with `prefetch_count=3` took three and did not ack them.

## What to watch first

| What | Why |
|---|---|
| queue depth (`messages_ready`) | whether work is piling up |
| unacknowledged (`messages_unacknowledged`) | how many messages are "in progress" and whether a consumer is stuck |
| consumers (`consumers`) | whether anyone reads the queue at all |
| connections and channels | leaks, who is connected |
| alarms | memory and disk |

## The management HTTP API

The management plugin gives a web UI on port 15672 and an HTTP API at the same address:

```python
q = api("/queues/%2F/mon")        # %2F is the default vhost "/"
print({k: q.get(k) for k in ("messages", "messages_ready", "messages_unacknowledged", "consumers", "state", "type")})
```

```text
{'messages': 10, 'messages_ready': 7, 'messages_unacknowledged': 3, 'consumers': 1, 'state': 'running', 'type': 'classic'}
```

`messages = messages_ready + messages_unacknowledged`: 7 ready plus 3 held by the consumer. The same numbers at vhost level:

```text
vhost totals: {'messages': 10, 'messages_ready': 7, 'messages_unacknowledged': 3}
```

A queue's `message_stats` holds counters and rates (`ack`, `deliver`, `deliver_get`, `get` …).

> Management statistics are updated with a delay: in the experiment we had to wait a few seconds before reading. Do not compare API numbers with numbers you just obtained another way.

## Connections, channels, consumers

```text
connections: [('172.17.0.1:48330 -> 17', None), ('172.17.0.1:48346 -> 17', 'monitor-demo')]
channels: [(1, 0, 0), (1, 3, 3)]
consumers: [('mon', 3, True)]
```

- A connection can carry a readable name (`client_properties={"connection_name": "monitor-demo"}`): it makes the application easy to find.
- A channel shows `prefetch_count` (3) and the unacknowledged count (3): the consumer holds three messages.
- The consumer is on queue `mon`, prefetch 3, `ack_required=True`.

## The command line

```text
$ rabbitmqctl list_queues name messages_ready messages_unacknowledged consumers
name  messages_ready  messages_unacknowledged  consumers
mon   7               3                        1

$ rabbitmq-diagnostics check_running
RabbitMQ on node rabbit@4ff649c7c4dc is fully booted and running
```

`rabbitmqctl list_queues` and `list_connections`/`list_channels`/`list_consumers` work without HTTP; `rabbitmq-diagnostics` checks the node's state.

## Health check

```text
healthcheck: {'status': 'ok'}
```

`GET /api/health/checks/alarms` returned `ok` with no alarms; during an alarm it answers with code 503 ([advanced theory](advanced.en.md#memory-and-disk-alarms)).

## Common beginner mistakes

- ❌ Looking only at `messages` → ✅ `messages_ready` and `messages_unacknowledged` separately.
- ❌ Leaving connections unnamed → ✅ `connection_name` in `client_properties`.
- ❌ Treating API numbers as instantaneous → ✅ statistics update with a delay.
- ❌ Leaving `guest`/`guest` → ✅ a separate user with minimal permissions ([roadmap](../../overview.en.md), module 11).
- ❌ Monitoring only queues → ✅ nodes too: memory, disk, alarms.

## Related materials

- Next: [advanced theory](advanced.en.md) — Prometheus, alarms
- Before: [08 · Clustering](../08-clustering/basics.en.md), [03 · Queues and delivery](../03-queues-and-delivery/basics.en.md)
- Next module: [10 · Flow control and performance](../../overview.en.md)

## Sources

- [Management Plugin](https://www.rabbitmq.com/docs/management)
- [Monitoring](https://www.rabbitmq.com/docs/monitoring)
- [rabbitmqctl](https://www.rabbitmq.com/docs/man/rabbitmqctl.8)
