---
title: "Flow control and performance: advanced"
lang: en
track: rabbitmq
kind: module
doc: advanced
slug: 10-performance
part: 3
tags: [rabbitmq, performance, flow-control, credit-flow, disk-alarm, multiple-ack, queue-types]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Flow control and performance: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md) (the same caveats about measurements). The code was run on a single RabbitMQ 4.4.1 node with pika 1.5.0.

## Multiple acks

`basic_ack(tag, multiple=True)` acknowledges every message up to and including the tag in one frame. Experiment: prefetch 1000, an ack every hundred:

```text
consume prefetch 1000, ack each: 29,406 msg/s
consume prefetch 1000, multiple ack/100: 66,795 msg/s
```

Multiple acks more than doubled the reading speed. The price is reprocessing of up to 100 messages if the consumer crashes between acks (all unacknowledged ones return to the queue), so the handler must be idempotent ([module 05](../05-reliability/advanced.en.md#idempotent-consumer)).

## Queue type and speed

5000 persistent messages with pipelined confirms, a durable queue of each type:

```text
classic 5000 persistent pipelined confirms: 31,556 msg/s acked=5000
quorum  5000 persistent pipelined confirms: 23,285 msg/s acked=5000
stream  5000 persistent pipelined confirms: 36,858 msg/s acked=5000
```

A quorum queue is slower than classic (by about a quarter in this test): the write is replicated and confirmed by a majority. Pay for durability deliberately ([module 07](../07-queue-types/basics.en.md)). The sample is small and on one node: in a three-node cluster the difference differs.

> ⚠️ Verify: a comparison of the types in a multi-node cluster and at different message sizes was not done here.

## Flow control

The broker does not let a publisher outrun consumers indefinitely. The mechanisms:

1. **Credit flow** — inside the broker processes pass messages to one another while the receiver has "credit"; if a queue cannot keep up, the pressure propagates back as far as the publisher's connection, which enters the `flow` state and is slowed.
2. **Alarms** for memory and disk — block publishers completely ([module 09](../09-monitoring/advanced.en.md#memory-and-disk-alarms)).
3. **Confirms and prefetch** — limit the client from its side.

Defaults read with `rabbitmqctl environment`:

```text
{credit_flow_default_credit,{400,200}}
{disk_free_limit,50000000}
{vm_memory_high_watermark,0.6}
```

An experiment with 300,000 fast persistent publishes (4.7 s, `published 300k in 4.69 s`) while polling `/api/connections` every 0.2 s:

```text
connection states seen: ['running']
queued: 299880
```

We could not catch the `flow` state: the broker coped without throttling. The `flow` state appears under overload.

> ⚠️ Verify: the credit flow mechanism itself (the meaning of the pair `{400,200}`, the moment a connection enters `flow`) is described from the documentation and was not reproduced here.

## The disk alarm

If free space is below `disk_free_limit` the broker blocks publishers. Experiment: the limit set above the available space (`rabbitmqctl set_disk_free_limit 2000GB`).

```text
blocked reasons: ['low on disk']
Free disk space alarm on node rabbit@cf229f34fc47
```

The client got `Connection.Blocked` with the reason `low on disk`; `rabbitmq-diagnostics alarms` showed the alarm. After the limit was restored (`50000000`) the alarm cleared: `reported no alarms, local or clusterwide`. At 900 GB there was no alarm because the disk had more free space: the limit fires only when free space is below it.

## Best practices

- Pipelined publisher confirms, multiple consumer acks.
- Prefetch per task: tens to hundreds for fast tasks, 1 to a few for long ones.
- Large data goes to storage, a reference in the message.
- Watch queue depth and alarms; keep headroom on disk and memory.
- Quorum for important data, classic for fast simple queues.
- Load-test on the target hardware before launch.

## Common mistakes

- ❌ A multiple ack without idempotency → ✅ an idempotent handler.
- ❌ Comparing queue types from one run on a laptop → ✅ measure in the target environment.
- ❌ Expecting the disk alarm to fire at any limit → ✅ it fires when free space is below the limit.
- ❌ Not reacting to `Connection.Blocked` → ✅ pause publishing.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [07 · Queue types](../07-queue-types/advanced.en.md), [09 · Monitoring](../09-monitoring/advanced.en.md)
- Next module: [11 · Security](../11-security/overview.en.md)

## Sources

- [Flow Control](https://www.rabbitmq.com/docs/flow-control)
- [Alarms](https://www.rabbitmq.com/docs/alarms)
- [Disk Alarms](https://www.rabbitmq.com/docs/disk-alarms)
- [Consumer Acknowledgements and Publisher Confirms](https://www.rabbitmq.com/docs/confirms)
