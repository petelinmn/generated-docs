---
title: "Kafka basics: basic level"
lang: en
track: kafka
kind: module
doc: basics
slug: 01-kafka-basics
part: 1
tags: [kafka, topic, partition, offset, producer, consumer, log, kraft]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Kafka basics: basic level

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [the Python track](../../../python/overview.en.md) (basic syntax is enough) · **Time:** ~1.5 h

## How the code was verified

All code was run against a real single-node Apache Kafka 4.3.1 broker (image `apache/kafka:4.3.1`, KRaft mode) in Docker with `confluent-kafka` 2.16.2 for Python; outputs are copied from the runs. Helpers: `BS = "localhost:9092"`; `admin = AdminClient({"bootstrap.servers": BS})`; `read_all(group, reset="earliest", n=6, topic="orders")` reads up to `n` records and returns a sorted list of `(partition, offset, key, value)`; `kcli(*cmd)` runs a command inside the container (`docker exec kafka …`). Kafka's scripts live in `/opt/kafka/bin/`. The broker was started like this:

```bash
docker run -d --name kafka -p 9092:9092 apache/kafka:4.3.1
```

> In Git Bash on Windows set `MSYS_NO_PATHCONV=1` before `docker exec … /opt/kafka/bin/…`: otherwise the path turns into `C:/Program Files/Git/…`.

## A log instead of a queue

Kafka is built on a **log**: a file to which records are only appended. Each record gets a sequence number — an **offset**. A reader remembers the offset it has read up to and continues from there.

| | A queue (for example [RabbitMQ](../../../rabbitmq/overview.en.md)) | A Kafka log |
|---|---|---|
| a read message | is deleted after the ack | stays until retention expires |
| re-reading | needs a republish | the reader just goes back to the offset it wants |
| several independent readers | need separate queues | each reads the same log at its own position |
| the read position | at the broker | at the reader (stored in Kafka as a group offset) |

## A topic and partitions

A **topic** is a named stream of records. A topic is divided into **partitions**: each is a separate ordered log. Partitions let writing and reading scale because different partitions are processed in parallel.

```python
fs = admin.create_topics([NewTopic("orders", num_partitions=3, replication_factor=1)])
print("create:", {t: f.result() for t, f in fs.items()})
md = admin.list_topics(timeout=10)
print("partitions of orders:", sorted(md.topics["orders"].partitions))
```

```text
create: {'orders': None}
partitions of orders: [0, 1, 2]
```

`None` as the result of `create_topics` means success. `replication_factor=1` is allowed only because there is one broker; replication is in [module 07](../07-replication-and-availability/overview.en.md).

## Writing

```python
p = Producer({"bootstrap.servers": BS})
def on_delivery(err, msg):
    reports.append((msg.topic(), msg.partition(), msg.offset(), msg.key(), msg.value()))
for i in range(6):
    p.produce("orders", key=f"user{i % 2}".encode(), value=f"order-{i}".encode(), on_delivery=on_delivery)
p.flush()
```

```text
delivered: ('orders', 1, 0, b'user1', b'order-1')
delivered: ('orders', 1, 1, b'user1', b'order-3')
delivered: ('orders', 1, 2, b'user1', b'order-5')
delivered: ('orders', 2, 0, b'user0', b'order-0')
delivered: ('orders', 2, 1, b'user0', b'order-2')
delivered: ('orders', 2, 2, b'user0', b'order-4')
```

- `produce` is **asynchronous**: it puts the record into the client's buffer; the result arrives in the `on_delivery` callback, which fires inside `flush()` or `poll()`.
- The report holds the record's partition and offset.
- Records with the same **key** landed in the same partition: `user1` in partition 1, `user0` in partition 2. Within a partition the offsets go 0, 1, 2.

Partition 0 stayed empty: with two keys two partitions were enough. More on keys in [module 02](../02-producers/overview.en.md).

## Reading and offsets

```python
a = read_all("g-1")
b = read_all("g-2")
print("reader A:", a)
print("reader B sees the same:", a == b)
```

```text
reader A: [(1, 0, 'user1', 'order-1'), (1, 1, 'user1', 'order-3'), (1, 2, 'user1', 'order-5'), (2, 0, 'user0', 'order-0'), (2, 1, 'user0', 'order-2'), (2, 2, 'user0', 'order-4')]
reader B sees the same: True
```

The second reader (another group) got the **same** records: reading deletes nothing. In Kafka readers do not "take" messages, they read the log.

A partition log's bounds are **low** (the earliest available offset) and **high** (the offset the next record will get):

```text
partition 0: low=0 high=0
partition 1: low=0 high=3
partition 2: low=0 high=3
```

## Where to read from when a group has no stored position

The parameter `auto.offset.reset` decides what a new group does: `earliest` — from the start of the log, `latest` — only new records.

```text
latest, existing data: []
latest, after a new write: [(2, 3, 'new-1')]
```

A group with `latest` did not see the old records but saw the new one (`new-1`, partition 2, offset 3). Once a group has committed a position, a restart begins from it; see [module 03](../03-consumers-and-groups/overview.en.md).

## How it lies on disk

Each partition is a directory `<topic>-<number>` in `log.dirs`; inside are the segment's files:

```text
00000000000000000000.index
00000000000000000000.log
00000000000000000000.timeindex
leader-epoch-checkpoint
partition.metadata
```

The file name is the offset of the segment's first record (here 0). `.log` holds the records, `.index` and `.timeindex` speed up lookups by offset and time. Details in the [expert theory](expert.en.md).

## Common beginner mistakes

- ❌ Thinking a read message disappears → ✅ the log is kept per its retention.
- ❌ Expecting order across the whole topic → ✅ order is guaranteed only within a partition.
- ❌ Reading with `latest` and wondering at an empty result → ✅ choose `earliest` for a new group.
- ❌ Not calling `flush()` in a short program → ✅ records may stay in the client buffer.
- ❌ Creating a topic with `replication_factor` above the number of brokers → ✅ it cannot exceed the number of brokers.

## Related materials

- Next: [advanced theory](advanced.en.md) — the broker, KRaft, configuration, group positions
- Next module: [02 · Producers](../02-producers/overview.en.md)
- Analogue in another track: [RabbitMQ: AMQP basics](../../../rabbitmq/modules/01-messaging-basics/overview.en.md)

## Sources

- [Apache Kafka: Introduction](https://kafka.apache.org/intro)
- [Apache Kafka: Design](https://kafka.apache.org/documentation/#design)
- [confluent-kafka-python: API](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html)
