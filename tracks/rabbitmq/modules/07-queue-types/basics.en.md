---
title: "Queue types: basics"
lang: en
track: rabbitmq
kind: module
doc: basics
slug: 07-queue-types
part: 2
tags: [rabbitmq, queue-types, classic-queue, quorum-queue, stream, x-queue-type]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Queue types: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [03 · Queues and delivery](../03-queues-and-delivery/basics.en.md), [06 · Dead-lettering and retries](../06-dead-lettering-and-retries/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

All snippets were run against a real RabbitMQ 4.4.1 in Docker (one node) with pika 1.5.0; outputs are copied from the runs. `P()` opens a connection: `P = lambda: pika.Connection(pika.ConnectionParameters("localhost"))`; `ch` is the channel of the main connection. Properties that depend on several nodes (replication) were not verified here: see [module 08](../08-clustering/overview.en.md).

## Three queue types

The type is chosen at declaration by the argument `x-queue-type`; without it the queue is `classic`.

| Type | Design | Used for |
|---|---|---|
| `classic` | an ordinary queue on one node | fast simple queues where loss on a node failure is acceptable |
| `quorum` | a replicated queue based on consensus (Raft) | important data: survives a node failure |
| `stream` | a non-destructive append-only log | many consumers, re-reading, large volumes |

```python
ch.queue_declare("qc", durable=True)
ch.queue_declare("qq", durable=True, arguments={"x-queue-type": "quorum"})
ch.queue_declare("qs", durable=True, arguments={"x-queue-type": "stream"})
```

```text
declared qc 0
declared qq 0
declared qs 0
```

`rabbitmqctl list_queues name type` shows the types:

```text
plain	classic
qq-ttl	quorum
s	stream
```

## Limits of quorum queues and streams

The new types have rules the broker checks at declaration. All of the following was obtained by running on 4.4.1:

| Attempt | Result |
|---|---|
| quorum + `exclusive=True` | channel closed: `406 PRECONDITION_FAILED - invalid property 'exclusive-owner'` |
| quorum without `durable` | connection closed: `541 … transient_nonexcl_queues is deprecated` |
| stream without `durable` | the same `541` |
| quorum + `x-max-priority` | `406 - invalid arg 'x-max-priority' … of queue type rabbit_quorum_queue` |
| quorum + `x-message-ttl` | allowed |
| an unknown `x-queue-type` | `406 - invalid arg 'x-queue-type' … unsupported queue type` |
| redeclaring with another type | `406 - inequivalent arg 'x-queue-type'` |

You cannot change a queue's type: create a new one and move the messages.

## Stream: a log instead of a queue

In a stream messages are **not deleted** on reading: a consumer reads the log from a chosen position (offset), and an ack only moves its own position. This differs from an ordinary queue, where an ack deletes the message.

```python
# publish e0..e4, then read from the start
x.basic_qos(prefetch_count=10)                       # mandatory for streams
x.basic_consume("qs", cb, arguments={"x-stream-offset": "first"})
```

```text
stream first: ['e0', 'e1', 'e2', 'e3', 'e4']
stream next: ['e5'] | first consumer now: ['e0', 'e1', 'e2', 'e3', 'e4', 'e5']
stream re-read: ['e0', 'e1', 'e2', 'e3', 'e4', 'e5']
```

- `first` starts at the beginning of the log, `next` only new messages (the consumer got just `e5`).
- A second consumer that arrived later read everything again: the messages did not go anywhere.
- A stream consumer needs `prefetch_count` and cannot use `auto_ack=True`: otherwise `406 PRECONDITION_FAILED - consumer prefetch count is not set for stream queue`.

## What to choose

| Need | Type |
|---|---|
| A task queue with no fault-tolerance requirements | classic |
| Tasks that must not be lost when a node fails | quorum |
| Fan-out to many consumers with re-reading, events "as a log" | stream |

## Common beginner mistakes

- ❌ Assuming every queue is replicated → ✅ by default it is a classic queue on one node.
- ❌ Trying to change an existing queue's type → ✅ create a new one (`406`).
- ❌ Expecting a stream read to delete the message → ✅ the log keeps them per its retention policy.
- ❌ Reading a stream without `prefetch_count` or with `auto_ack` → ✅ set prefetch and ack.
- ❌ `x-max-priority` on a quorum queue → ✅ not every type supports priorities.

## Related materials

- Next: [advanced theory](advanced.en.md) — delivery limit, stream offsets
- Before: [06 · Dead-lettering and retries](../06-dead-lettering-and-retries/basics.en.md)
- Next module: [08 · Clustering and high availability](../08-clustering/overview.en.md)

## Sources

- [Classic Queues](https://www.rabbitmq.com/docs/classic-queues)
- [Quorum Queues](https://www.rabbitmq.com/docs/quorum-queues)
- [Streams and Super Streams](https://www.rabbitmq.com/docs/streams)
