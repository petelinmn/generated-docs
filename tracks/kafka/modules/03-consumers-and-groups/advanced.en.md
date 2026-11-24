---
title: "Consumers and groups: advanced level"
lang: en
track: kafka
kind: module
doc: advanced
slug: 03-consumers-and-groups
part: 1
tags: [kafka, rebalance, cooperative-sticky, max-poll-interval, group-protocol, offset-commit]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Consumers and groups: advanced level

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run on Apache Kafka 4.3.1 (one broker) with `confluent-kafka` 2.16.2.

## Rebalancing

When a group's membership changes (a consumer joins, leaves or hangs) or the number of partitions changes, Kafka **redistributes the partitions**. While a rebalance runs, the affected partitions are not read. The handlers `on_assign` and `on_revoke` tell you what happened:

```python
a.subscribe(["g1"], on_assign=on_assign, on_revoke=on_revoke)
```

Member A's events while others joined and left (the default strategy):

```text
member A events: [('assign', [0, 1, 2]), ('revoke', [0, 1, 2]), ('assign', [0, 1]), ('revoke', [0, 1]), ('assign', [1]), ('revoke', [1]), ('assign', [0, 1])]
```

On every rebalance A **loses all its partitions** (`revoke` of all current ones) and gets them again (`assign`): that is how the eager strategy works. During this pause reading stops for every member, even if their partitions did not change.

### Cooperative rebalancing

The `cooperative-sticky` strategy revokes only those partitions that really move to another member:

```python
Consumer({..., "partition.assignment.strategy": "cooperative-sticky"})
```

```text
cooperative-sticky A events: [('assign', [0, 1, 2]), ('revoke', [0]), ('assign', []), ('assign', [])] | A: [1, 2] B: [0]
```

When B joined, only partition 0 was revoked from A (it moved to B), while partitions 1 and 2 stayed with A and were read without a pause.

### The new group protocol (`group.protocol=consumer`)

Kafka 4.x offers a new consumer-group protocol: the broker drives the rebalance, and it is incremental.

```python
Consumer({..., "group.protocol": "consumer"})
```

```text
group.protocol=consumer A events: [('assign', [0, 1, 2]), ('revoke', [2]), ('assign', [])] | A: [0, 1] B: [2]
```

With this protocol only partition 2 was revoked from A, and partitions 0 and 1 stayed.

> ⚠️ Verify: the new protocol has a different set of settings (some client parameters, for example `session.timeout.ms` and `partition.assignment.strategy`, are managed by the broker or do not apply), and not every client version supports it; none of this was verified here. Check the documentation of your client and broker versions.

## Member liveness: `max.poll.interval.ms`

A consumer must call `poll` regularly. If the pause between calls exceeds `max.poll.interval.ms`, the broker considers the consumer stuck and takes its partitions away. Experiment: `max.poll.interval.ms=6000`, the consumer did not call `poll` for 9 seconds:

```text
assignment before stall: [0, 1, 2]
poll errors after a 9 s stall: ['_MAX_POLL_EXCEEDED'] | assignment: [0, 1, 2]
```

The next `poll` returned the error `_MAX_POLL_EXCEEDED`. So **heavy processing between `poll` calls must not exceed `max.poll.interval.ms`** (by default 5 minutes per the librdkafka documentation): process in portions or in separate threads without blocking the `poll` call.

> ⚠️ Verify: the defaults of `max.poll.interval.ms`, `session.timeout.ms`, `heartbeat.interval.ms` and their interplay were not verified here; the error was observed with lowered values.

## Commit strategies

| Strategy | How | Guarantee |
|---|---|---|
| auto-commit (`enable.auto.commit=true`) | the client commits what was read periodically | a record may be lost: read and committed but not processed |
| commit after each record | `commit(message=m)` | at least once; expensive in requests |
| commit in batches | `commit()` after a batch | at least once; a repeat up to the batch size |
| synchronous / asynchronous | `commit(asynchronous=False/True)` | synchronous is more reliable, asynchronous faster |

The experiment showed: without a commit records come again; the committed offset is the next one to read ([basics](basics.en.md#what-happens-without-a-commit)).

> ⚠️ Verify: the auto-commit interval and the default value of `enable.auto.commit` in librdkafka were not verified here (auto-commit was switched off explicitly in the examples).

## Best practices

- Commit offsets after processing; the handler is idempotent.
- A group's consumers number no more than the partitions.
- For smooth rebalances use `cooperative-sticky` or the new protocol (after checking compatibility).
- Do not block `poll` longer than `max.poll.interval.ms`.
- Close the consumer with `close()`: it tells the broker about the exit and speeds up the rebalance.
- `on_revoke` is the place to commit positions if processing runs in batches.

## Common mistakes

- ❌ Long processing between `poll` calls → ✅ `_MAX_POLL_EXCEEDED` and a rebalance.
- ❌ Auto-commit with critical processing → ✅ a manual commit after processing.
- ❌ Not closing the consumer → ✅ `close()` on shutdown.
- ❌ Assuming a rebalance is instant → ✅ it stops reading.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [05 · Delivery semantics](../../overview.en.md)
- Next module: [04 · Topics, retention and compaction](../../overview.en.md)

## Sources

- [Apache Kafka: Consumer rebalance protocol](https://kafka.apache.org/documentation/#consumer_rebalance_protocol)
- [Apache Kafka: Consumer configs](https://kafka.apache.org/documentation/#consumerconfigs)
- [librdkafka: Configuration](https://github.com/confluentinc/librdkafka/blob/master/CONFIGURATION.md)
