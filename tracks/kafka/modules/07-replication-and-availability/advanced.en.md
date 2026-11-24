---
title: "Replication and availability: advanced level"
lang: en
track: kafka
kind: module
doc: advanced
slug: 07-replication-and-availability
part: 2
tags: [kafka, kraft, controller-quorum, leader-election, unclean-leader-election, under-replicated-partitions]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Replication and availability: advanced level

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md) (the same three-broker cluster). The code was run on Apache Kafka 4.3.1.

## The controller quorum

The cluster's metadata (topics, leaders, ISR) is managed by the KRaft **controller quorum** ([module 01](../01-kafka-basics/advanced.en.md#what-a-broker-and-kraft-mode-are)). In our rig there are three combined nodes: the controllers are the brokers themselves. The quorum needs a **majority** (2 of 3). With two of the three brokers stopped we checked what stays working:

```text
create topic with 1 of 3 controllers: KafkaException KafkaError{code=_TIMED_OUT,val=-185,str="Failed while waiting for response from broker: Local: Timed out"} 8.0 s
```

- **Writing to existing partitions** whose leader is on a live broker kept working (`acks=1`: `['ok', 'ok']` in the [basics](basics.en.md#acks-and-mininsyncreplicas)).
- **Changing metadata** (creating a topic, a `describeMetadataQuorum` request) does **not** complete without a controller majority: the request timed out.
- After the stopped brokers were started the quorum recovered: a controller leader was found, `LeaderEpoch` rose to 5.

Takeaway: a cluster needs an **odd** number of controllers (3 or 5), and in production controllers are separated from brokers so losing a broker does not cost the cluster a controller.

> ⚠️ Verify: dedicated controllers (`process.roles=controller`) and their sizing in production clusters come from the documentation; only the combined mode was verified here. Also not verified: the behaviour of `acks=all` clients and of leader re-election while the quorum is unavailable.

## Leader election

When a partition's leader is unavailable the controller appoints one of the **in-sync** followers:

```text
p0 leader now: 1 replicas: [3, 1, 2] isr: [1, 2]
```

Broker 1 from the ISR `[1, 2]` became the leader. Broker 3 is not in the ISR, so it could not be.

If no in-sync replica remained, by default Kafka does **not** elect a lagging copy (`unclean.leader.election.enable=false`): the partition becomes unavailable but data is not lost. Enabling unclean elections raises availability at the price of possibly losing records.

> ⚠️ Verify: the unclean-election scenario, the default of `unclean.leader.election.enable` and `replica.lag.time.max.ms` (the time after which a lagging follower leaves the ISR) were not reproduced here.

## The preferred leader

In the `replicas` list the first broker is the preferred leader. After recovery the leaders **stayed** on broker 2 (during a 25-second wait). Kafka can return leadership to the preferred brokers (automatically on a schedule or with the command `kafka-leader-election.sh`), but in our experiment that did not happen within the time observed.

> ⚠️ Verify: the settings `auto.leader.rebalance.enable` and `leader.imbalance.check.interval.seconds` and the command `kafka-leader-election.sh` itself were not verified here.

## Watching the state

```text
under-replicated: (none)
```

`kafka-topics.sh --describe --under-replicated-partitions` prints partitions whose ISR is smaller than the number of replicas; on a healthy cluster the output is empty. It is the main indicator of replication health ([module 09](../../overview.en.md)); likewise `--unavailable-partitions` shows partitions with no leader.

## Best practices

- RF=3, `min.insync.replicas=2`, producers with `acks=all` + idempotence for important data.
- Three (or five) controllers; in production separate from the brokers.
- Brokers in different racks/availability zones; watch under-replicated partitions.
- Do not enable unclean leader election without need.
- Upgrade and restart brokers one at a time, waiting for the ISR to return.

## Common mistakes

- ❌ Assuming RF=3 protects against losing two brokers → ✅ with `min.insync.replicas=2` writes stop, and with `acks=1` data is at risk.
- ❌ Restarting several brokers at once → ✅ one at a time, waiting for the ISR.
- ❌ Keeping all brokers in one rack/zone → ✅ a shared failure of the rack.
- ❌ Expecting metadata to change without a quorum → ✅ a timeout.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [01 · Kafka basics](../01-kafka-basics/advanced.en.md) — the broker and KRaft
- Next module: [08 · Kafka Connect](../08-kafka-connect/overview.en.md)

## Sources

- [Apache Kafka: Replication](https://kafka.apache.org/documentation/#replication)
- [Apache Kafka: KRaft](https://kafka.apache.org/documentation/#kraft)
- [Apache Kafka: Broker configs](https://kafka.apache.org/documentation/#brokerconfigs)
