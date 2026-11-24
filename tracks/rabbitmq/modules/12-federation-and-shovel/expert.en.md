---
title: "Federation and Shovel: expert"
lang: en
track: rabbitmq
kind: module
doc: expert
slug: 12-federation-and-shovel
part: 3
tags: [rabbitmq, shovel, federation, upstream, policy, shovel-status, multi-datacenter]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Federation and Shovel: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on two independent RabbitMQ 4.4.1 brokers in Docker.

## Reference

### Shovel

| Element | Value | Verified |
|---|---|---|
| plugins | `rabbitmq_shovel`, `rabbitmq_shovel_management` | yes |
| creation | `rabbitmqctl set_parameter shovel <name> '<json>'` | yes |
| JSON keys | `src-protocol`, `src-uri`, `src-queue`, `dest-protocol`, `dest-uri`, `dest-queue` | yes (`amqp091`) |
| state | `rabbitmqctl shovel_status`: `name`, `type`, `state`, `source`, `destination`, `termination_reason`, `forwarded`, … | yes |
| states | `running`, `terminated` | yes |
| termination reason on a source failure | `"failed to connect to source"` | yes |
| deletion | `rabbitmqctl clear_parameter shovel <name>` | yes |
| an exchange as the source (`src-exchange`), `ack-mode`, `reconnect-delay`, `src-delete-after`, AMQP 1.0 | other keys | not tested |
| a static shovel (in configuration) | an alternative to the dynamic one | not tested |

### Federation

| Element | Value | Verified |
|---|---|---|
| plugins | `rabbitmq_federation`, `rabbitmq_federation_management` | yes |
| upstream | `rabbitmqctl set_parameter federation-upstream <name> '{"uri": "…"}'` | yes |
| policy | `set_policy <name> "<pattern>" '{"federation-upstream-set":"all"}' --apply-to exchanges` | yes |
| state | `rabbitmqctl federation_status`: `status`, `type`, `exchange`, `uri`, `vhost` | yes |
| federated queues, `max-hops`, bidirectionality, `expires`, `message-ttl` | other features | not tested |

### What was verified on the rig

| Experiment | Result |
|---|---|
| shovel `A.q.src → B.q.dst`, 5 messages in the source | all 5 in the destination; `forwarded` 5 |
| 3 more messages | source 0, destination 8; order preserved |
| the source stopped | `terminated`, `failed to connect to source` |
| the source started and a publish | the message arrived, the shovel works again |
| a federated exchange `fed.ex` | a message from `rmqa` in a queue on `rmqb`; link `running` |

## Internals

- **A shovel is an AMQP client.** It connects to the source and destination as an ordinary consumer and publisher and moves messages; so it works over any URI reachable from the host broker.
- **A shovel lives as a parameter** (a runtime parameter) on the host broker (in the experiment, on `rmqb`).
- **Federation is a link created by a policy:** a policy by name pattern starts links to the upstream brokers.
- **Both plugins use ordinary AMQP connections** and permissions ([module 11](../11-security/expert.en.md)).
- **Duplication is possible:** on a failure between delivery and acknowledgement a message may arrive again.

> ⚠️ Verify: the internals (acknowledgement handling, behaviour on breaks and restarts, performance limits, the internal connection pool and federation queues) are described from the documentation and were not verified here.

## Gotchas

### 1. A shovel moves, it does not copy

After the transfer the message disappears from the source (verified: `q.src` is empty).

### 2. Topology is needed at both ends

In the experiments the queues and exchanges were created in advance on both brokers; do the same. What happens when they are missing was not verified here.

### 3. Credentials in a parameter

A URI with a password is stored in the broker's parameter; use a separate user with minimal permissions and TLS (the broker hides the password in its output).

### 4. The terminated state

The shovel recovered after the source returned (verified), but without monitoring you will not learn about the downtime.

### 5. Federation is one-directional by default

Upstream → downstream; the reverse direction needs separate configuration (not verified here).

### 6. Duplicates

The destination must be idempotent ([module 05](../05-reliability/expert.en.md)).

### 7. `guest` for the link

It works in the teaching image but is unacceptable in production.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| Shovel | simple queue→queue transfer, any protocols | moves, no "shared" topology |
| Federation | a shared exchange between brokers | policy setup, one-directional by default |
| A cluster | one system | not for a WAN |
| Application-level data replication | full control | more code |

## Interview questions

<details><summary>1. How does a Shovel differ from Federation?</summary>

A Shovel moves messages from a source to a destination; Federation propagates messages published to upstream exchanges to downstream ones.

→ [Basics](basics.en.md#why-link-brokers)

</details>

<details><summary>2. Trick: does a shovel copy or move?</summary>

It moves: messages are removed from the source.

→ [Gotcha 1](#1-a-shovel-moves-it-does-not-copy)

</details>

<details><summary>3. Why not stretch a cluster across data centres?</summary>

A cluster is designed for a fast reliable network; for a WAN use independent brokers and Shovel/Federation.

→ [Basics](basics.en.md#why-link-brokers)

</details>

<details><summary>4. How do you create a dynamic shovel?</summary>

`rabbitmqctl set_parameter shovel <name> '{"src-protocol":…,"src-uri":…,"src-queue":…,"dest-protocol":…,"dest-uri":…,"dest-queue":…}'`.

→ [Basics](basics.en.md#shovel-queue-to-queue)

</details>

<details><summary>5. What happens to a shovel when the source fails?</summary>

The state becomes `terminated` with the reason `failed to connect to source`; after the source returned the shovel recovered.

→ [Advanced theory](advanced.en.md#a-source-failure)

</details>

<details><summary>6. How do you set up an exchange federation?</summary>

A `federation-upstream` parameter with a URI and a policy with `federation-upstream-set` by a pattern of exchange names.

→ [Basics](basics.en.md#federation-a-shared-exchange)

</details>

<details><summary>7. Trick: will a message published on the downstream broker reach the upstream one?</summary>

By default federation goes from upstream to downstream; the reverse direction was not verified here.

→ [Gotcha 5](#5-federation-is-one-directional-by-default)

</details>

<details><summary>8. How do you check the state of the links?</summary>

`rabbitmqctl shovel_status` and `rabbitmqctl federation_status`.

→ [Advanced theory](advanced.en.md#managing-a-shovel)

</details>

<details><summary>9. What delivery guarantees does a shovel have?</summary>

At least once: duplicates are possible, an idempotent destination is needed.

→ [Gotcha 6](#6-duplicates)

</details>

<details><summary>10. Trick: can you use `guest` in the link URI?</summary>

It works in the teaching image but not in production: a separate user and TLS.

→ [Gotcha 7](#7-guest-for-the-link)

</details>

<details><summary>11. What must be created in advance?</summary>

Queues/exchanges and bindings on both brokers.

→ [Gotcha 2](#2-topology-is-needed-at-both-ends)

</details>

<details><summary>12. When do you choose Federation and when a Shovel?</summary>

Federation for a shared exchange and propagating publishes; a Shovel for moving queues and integrating different systems.

→ [Basics](basics.en.md#shovel-or-federation)

</details>

## Cheat sheet

```text
shovel     : set_parameter shovel NAME '{src-uri,src-queue,dest-uri,dest-queue,...}' ; shovel_status ; clear_parameter shovel NAME
federation : set_parameter federation-upstream UP '{"uri":"amqp://..."}' ; set_policy P "^pat" '{"federation-upstream-set":"all"}' --apply-to exchanges ; federation_status
moves vs copies : shovel MOVES a queue's messages ; federation propagates exchange publishes downstream
```

- Independent brokers, a separate user, TLS, an idempotent destination.
- Watch the link state.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [11 · Security](../11-security/expert.en.md)
- Next: [13 · Internals](../../overview.en.md)

## Sources

- [Shovel Plugin](https://www.rabbitmq.com/docs/shovel)
- [Federation Plugin](https://www.rabbitmq.com/docs/federation)
- [Dynamic Shovels](https://www.rabbitmq.com/docs/shovel-dynamic)
