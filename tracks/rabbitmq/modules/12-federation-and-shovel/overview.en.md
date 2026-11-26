---
title: "Federation and Shovel"
lang: en
track: rabbitmq
kind: module
doc: overview
slug: 12-federation-and-shovel
part: 3
levels: [1, 2, 3, 4]
tags: [rabbitmq, shovel, federation, upstream, multi-datacenter]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Federation and Shovel

🌐 [Русский](overview.ru.md) · **English**

## In short

When brokers sit in different data centres or belong to different systems they are not joined into a cluster but linked by plugins. A **Shovel** moves messages from a source on one broker to a destination on another. **Federation** propagates messages published to an upstream broker's exchange to a downstream one.

## Key ideas

- **Brokers stay independent:** each has its own users, vhosts and queues.
- **A shovel moves:** messages are removed from the source; order is preserved (verified).
- **A dynamic shovel is a parameter:** `rabbitmqctl set_parameter shovel …`; the state is `shovel_status`.
- **A source failure:** the shovel is `terminated` ("failed to connect to source") and recovered once the source returned.
- **Federation:** a `federation-upstream` parameter and a policy with `federation-upstream-set`; the state is `federation_status`.
- **Federation is one-directional by default** (upstream to downstream).
- **A separate user and TLS for the link;** `guest` only in a teaching setup.
- **Duplicates are possible** — the destination is idempotent.

## Minimal example

```text
rabbitmqctl set_parameter shovel s1 '{"src-protocol":"amqp091","src-uri":"amqp://U:P@rmqa","src-queue":"q.src",
  "dest-protocol":"amqp091","dest-uri":"amqp://U:P@rmqb","dest-queue":"q.dst"}'
```

Everything was run on two independent RabbitMQ 4.4.1 brokers in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Need | Solution |
|---|---|
| Move messages from a queue to a queue on another broker | Shovel |
| A shared exchange between brokers | Federation |
| One data centre, one system | a cluster ([module 08](../08-clustering/overview.en.md)) |

- ❌ Do not stretch a cluster across data centres.
- ❌ Do not use `guest` for the link in production.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | why link brokers, shovel, federation, choosing |
| [2. Advanced theory](advanced.en.md) | a source failure, management, federation state |
| [3. Expert theory](expert.en.md) | reference of parameters, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn rabbitmq 12-federation-and-shovel level 5` |
| 6. Tests | not created yet — `/learn rabbitmq 12-federation-and-shovel level 6` |

## Related materials

- Prerequisites: [08 · Clustering](../08-clustering/overview.en.md), [11 · Security](../11-security/overview.en.md)
- Next: [13 · Internals](../../overview.en.md)
- Analogues in other tracks: [Kubernetes](../../../kubernetes/overview.en.md)
- [System design: multi-region and recovery](../../../system-design/modules/16-multi-region-and-dr/overview.en.md)
