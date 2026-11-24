---
title: "Federation and Shovel: advanced"
lang: en
track: rabbitmq
kind: module
doc: advanced
slug: 12-federation-and-shovel
part: 3
tags: [rabbitmq, shovel, federation, reconnect, shovel-status, federation-status, policy]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Federation and Shovel: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md) (the same two-broker rig). The code was run on RabbitMQ 4.4.1 with pika 1.5.0.

## A source failure

What does a shovel do when the source is unavailable? The broker `rmqa` was stopped (`docker stop rmqa`):

```text
s1 | dynamic | terminated |  |  | / | 2026-11-24 16:09:09 | "failed to connect to source"
```

The shovel went to the `terminated` state with the reason `failed to connect to source`. After `docker start rmqa` and publishing the message `after-restart` to `q.src`, a few seconds later the message appeared in `q.dst`:

```text
after A restart, B q.dst: 1
```

So the shovel restored the link by itself and continued moving messages. To make sure it works, watch `rabbitmqctl shovel_status` (state `running`, the `forwarded` counter).

> ⚠️ Verify: the reconnect interval (`reconnect-delay`, 5 seconds per the documentation), the acknowledgement modes (`ack-mode`: `on-confirm` by default, `on-publish`, `no-ack`) and message loss/duplication on a failure between the confirm and the removal from the source were not verified here. A shovel gives at-least-once delivery, so the destination must tolerate duplicates ([module 05](../05-reliability/advanced.en.md#idempotent-consumer)).

## Managing a shovel

| Action | Command |
|---|---|
| create | `rabbitmqctl set_parameter shovel <name> '<json>'` |
| state | `rabbitmqctl shovel_status` |
| delete | `rabbitmqctl clear_parameter shovel <name>` |

After `clear_parameter` the shovel is stopped and removed:

```text
Clearing runtime parameter "s1" for component "shovel" on vhost "/" ...
```

A shovel is stored as a parameter in the host broker, so it survives a restart (as the documentation says; a restart of `rmqb` was not tried here).

## Federation: the link and its state

`federation_status` shows the state of links:

```text
Listing federation links on node rabbit@rmqb...
[#{error => <<>>, id => <<"945470fc">>, status => running, type => exchange,
   exchange => <<"fed.ex">>, queue => <<>>, uri => <<"amqp://rmqa">>, vhost => <<"/">>, ...}]
```

A link is created for an exchange matching the policy pattern (`^fed\.`) and shows `status => running`. A message published directly on the downstream broker landed in its local queue: `fed.q` held two (one from `rmqa`, one local). Whether it reached `rmqa` was not verified here (no queue is bound there):

```text
B fed.q after publishing on A: 1
A fed.ex received downstream publish? A has no queue bound; B fed.q: 2
```

By default federation is one-directional: from the upstream broker to the downstream one.

> ⚠️ Verify: bidirectional federation (upstreams in both directions), federated queues, an upstream set with several members (`federation-upstream-set`), `max-hops` and behaviour on a link break were not verified here.

## What to choose across data centres

| Scenario | Approach |
|---|---|
| Move an accumulated queue to another broker | Shovel (then delete) |
| Continuous delivery from one data centre to another | Shovel queue to queue or Federation |
| Events that several data centres need | An exchange federation |
| One data centre | a cluster |

## Best practices

- A separate user with minimal permissions for the link; TLS between brokers.
- The destination is idempotent: duplicates are possible.
- Monitor `shovel_status` and `federation_status` and queue depth at both ends ([module 09](../09-monitoring/advanced.en.md)).
- The topology (exchanges, queues, bindings) is created in advance at both ends.
- Do not stretch a cluster across data centres.

## Common mistakes

- ❌ `guest` in the link URI → ✅ a separate user.
- ❌ Treating a shovel as "replication" → ✅ it moves messages.
- ❌ Ignoring the `terminated` state → ✅ alert on `shovel_status`.
- ❌ Expecting bidirectionality from one federation link → ✅ configure both sides (not verified here).
- ❌ Forgetting duplicates → ✅ an idempotent destination.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [05 · Reliability](../05-reliability/advanced.en.md), [09 · Monitoring](../09-monitoring/advanced.en.md)
- Next module: [13 · Internals](../../overview.en.md)

## Sources

- [Shovel Plugin](https://www.rabbitmq.com/docs/shovel)
- [Federation Plugin](https://www.rabbitmq.com/docs/federation)
- [Federated Exchanges](https://www.rabbitmq.com/docs/federated-exchanges)
