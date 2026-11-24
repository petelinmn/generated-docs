---
title: "Federation and Shovel: basics"
lang: en
track: rabbitmq
kind: module
doc: basics
slug: 12-federation-and-shovel
part: 3
tags: [rabbitmq, shovel, federation, upstream, downstream, multi-datacenter]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Federation and Shovel: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [08 · Clustering](../08-clustering/basics.en.md), [11 · Security](../11-security/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

Two **independent** (not clustered) RabbitMQ 4.4.1 brokers in Docker: `rmqa` (port 5691) and `rmqb` (port 5692) on one network; the plugins `rabbitmq_shovel`, `rabbitmq_shovel_management`, `rabbitmq_federation`, `rabbitmq_federation_management` are enabled on both. The client is pika 1.5.0. Outputs are copied from the runs; **this is a single-machine teaching rig** — a real wide-area network with latency and breaks was not reproduced. The `rabbitmqctl` commands run inside the container. The teaching `guest` account (reachable over the network in the image) linked the brokers; in a production system create a separate user with minimal permissions and use TLS ([module 11](../11-security/basics.en.md)).

## Why link brokers

A cluster ([module 08](../08-clustering/basics.en.md)) is designed for a fast reliable network inside one data centre. When brokers sit in different data centres, belong to different teams, or messages must move between systems, they are linked by plugins:

| Plugin | What it does |
|---|---|
| **Shovel** | reads messages from a source (a queue or an exchange) and writes them to a destination on another broker |
| **Federation** | a downstream exchange or queue receives copies of messages from an upstream broker |

Both plugins work between **independent** brokers: each has its own users, vhosts and queues.

## Shovel: queue to queue

A dynamic shovel is defined by a runtime parameter. On the broker `rmqb` it moves messages from the queue `q.src` of `rmqa` to the queue `q.dst` of `rmqb`:

```text
$ rabbitmqctl set_parameter shovel s1 '{"src-protocol":"amqp091","src-uri":"amqp://USER:PASS@rmqa","src-queue":"q.src","dest-protocol":"amqp091","dest-uri":"amqp://USER:PASS@rmqb","dest-queue":"q.dst"}'
Setting runtime parameter "s1" for component "shovel" to "{"src-protocol": "amqp091", "src-uri": "amqp://rmqa", "src-queue": "q.src", "dest-protocol": "amqp091", "dest-uri": "amqp://rmqb", "dest-queue": "q.dst"}" in vhost "/" ...
```

(In the broker's output the credentials in the URI are hidden.) Before the shovel was enabled `q.src` held five messages:

```text
A q.src: 0 B q.dst: 5
Shovel status on node rabbit@rmqb
name | pending | type | state | source | destination | vhost | last_changed | termination_reason | ...
s1 | 0 | dynamic | running | amqp://rmqa | amqp://rmqb | / | 2026-11-24 16:09:02 |  | ...
```

All five left `q.src` for `q.dst` and the shovel entered the `running` state. New messages are moved continuously:

```text
after 3 more: A q.src: 0 B q.dst: 8
B received in order: ['m0', 'm1', 'm2', 'm3', 'm4', 'm5', 'm6', 'm7']
```

Order is preserved. Messages are **moved**, not copied: they are removed from the source after the destination acknowledges.

## Federation: a shared exchange

Federation makes a downstream exchange receive messages published to the upstream one. Setup on the downstream broker `rmqb`: the upstream broker and a policy that applies federation to exchanges by name pattern:

```text
$ rabbitmqctl set_parameter federation-upstream up-a '{"uri":"amqp://USER:PASS@rmqa"}'
$ rabbitmqctl set_policy fed "^fed\." '{"federation-upstream-set":"all"}' --apply-to exchanges
```

The exchange `fed.ex` is declared on both brokers; on `rmqb` a queue `fed.q` is bound to it. Publishing on `rmqa`:

```text
B fed.q after publishing on A: 1
```

A message published on `rmqa` ended up in a queue on `rmqb`. The link state is visible in `federation_status` (in the check — `status => running`, `type => exchange`).

## Shovel or federation

| Need | Choice |
|---|---|
| Move messages from a queue to a queue (including between different systems or protocols) | Shovel |
| Propagate published messages between brokers via an exchange | Federation |
| One logical system inside a data centre | a cluster ([module 08](../08-clustering/basics.en.md)) |

## Common beginner mistakes

- ❌ Stretching a cluster across data centres → ✅ separate brokers and Shovel/Federation.
- ❌ Keeping the link's credentials in the open and using `guest` → ✅ a separate user, TLS.
- ❌ Expecting a Shovel to copy messages → ✅ it moves them: they are removed from the source.
- ❌ Forgetting to declare the exchange/queue at both ends → ✅ create the topology in advance.
- ❌ Not watching the link state → ✅ `shovel_status`, `federation_status`.

## Related materials

- Next: [advanced theory](advanced.en.md) — link failure, policy and limits
- Before: [11 · Security](../11-security/basics.en.md), [08 · Clustering](../08-clustering/basics.en.md)
- Next module: [13 · Internals](../../overview.en.md)

## Sources

- [Shovel Plugin](https://www.rabbitmq.com/docs/shovel)
- [Federation Plugin](https://www.rabbitmq.com/docs/federation)
- [Dynamic Shovels](https://www.rabbitmq.com/docs/shovel-dynamic)
