---
title: "Producers: advanced level"
lang: en
track: kafka
kind: module
doc: advanced
slug: 02-producers
part: 1
tags: [kafka, producer, idempotence, batching, linger-ms, compression, message-timeout]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Producers: advanced level

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run on Apache Kafka 4.3.1 (one broker) with `confluent-kafka` 2.16.2.

## The idempotent producer

On a network failure the client may **retry** sending, and the record could land in the partition twice. An idempotent producer (`enable.idempotence=true`) numbers records so the broker discards the duplicates of retries within one partition.

```python
p = Producer({"bootstrap.servers": BS, "enable.idempotence": True})
r = send(p, "ak", [b"idem"]); print("idempotent producer ok:", r[0][0] is None)
```

```text
idempotent producer ok: True
```

Idempotence requires `acks=all`; the client refuses an incompatible pair when it is created:

```text
idempotence + acks=1 -> KafkaException KafkaError{code=_INVALID_ARG,val=-186,str="Failed to create producer: `acks` must be set to `all` when `enable.idempoten…
```

> ⚠️ Verify: the default value of `enable.idempotence` in `confluent-kafka` 2.16.2 and protection against duplicates under real retries (a break between the write and its acknowledgement) were not verified here — there were no retries on one rig. Enable idempotence explicitly for important records.

## Batches and delay: `linger.ms`

The client gathers records into batches. `linger.ms` is how long it waits for a batch to fill before sending:

```text
linger.ms=0: delivery latency of one message ms: [1, 1, 0, 0, 1]
linger.ms=100: delivery latency of one message ms: [108, 113, 106, 115, 115]
```

With `linger.ms=0` a single record arrives in a millisecond, with `100` in about 100 ms: the client waited in case more records came. A larger delay raises the share of full batches (and saves requests), a smaller one lowers delivery latency. Which is better depends on the load; measure.

> ⚠️ Verify: the effect of `linger.ms` and `batch.size` on throughput. In our experiment with 50,000 records `linger.ms=0` gave 275k records/s against 145k at 50 ms: the conclusion "a delay slows down" cannot be generalised, because the experiment was short and on one machine.

## Compression

`compression.type` compresses batches on the client; the broker stores them compressed. 20,000 repetitive records of 1850 bytes (37 MB) into one partition:

```text
none    payload 1850B x 20000 = 37 MB raw -> on disk 37.2 MB (0.61s)
gzip    payload 1850B x 20000 = 37 MB raw -> on disk 0.2 MB (0.16s)
snappy  payload 1850B x 20000 = 37 MB raw -> on disk 1.9 MB (0.05s)
lz4     payload 1850B x 20000 = 37 MB raw -> on disk 0.3 MB (0.04s)
zstd    payload 1850B x 20000 = 37 MB raw -> on disk 0.1 MB (0.09s)
```

The size is the sum of the partition's `.log` files. The data here is **extremely repetitive**, so the compression ratio is unrealistic: real data compresses several times, not hundreds. Takeaway: compression saves disk and network almost for free; compare codecs on your own data.

## Timeouts and retries

A record not delivered within `message.timeout.ms` ends with the error `_MSG_TIMED_OUT` ([basics](basics.en.md#errors)). The client itself retries sending on transient errors within that period.

> ⚠️ Verify: the interplay of `retries`, `retry.backoff.ms`, `delivery.timeout.ms` and record ordering under retries without idempotence was not verified here.

## Best practices

- For important data: `acks=all` and `enable.idempotence=true`.
- Always check `err` in the callback and call `flush()` on shutdown.
- The key is the business entity whose order you need (a customer, an order).
- `compression.type=lz4` or `zstd` as a reasonable choice; check on your data.
- `linger.ms` by latency requirements; for interactive flows a small value.

## Common mistakes

- ❌ `acks=1` with idempotence → ✅ the client refuses: `all` is needed.
- ❌ A huge `linger.ms` in an interactive system → ✅ rising latency.
- ❌ Judging compression on repetitive test data → ✅ measure on real data.
- ❌ Ignoring `_MSG_TIMED_OUT` → ✅ retry/alert.

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert theory](expert.en.md)
- [05 · Delivery semantics](../../overview.en.md) — idempotence and transactions
- Next module: [03 · Consumers and groups](../03-consumers-and-groups/overview.en.md)

## Sources

- [Apache Kafka: Producer configs](https://kafka.apache.org/documentation/#producerconfigs)
- [Apache Kafka: Idempotent producer](https://kafka.apache.org/documentation/#producerconfigs_enable.idempotence)
- [librdkafka: Configuration](https://github.com/confluentinc/librdkafka/blob/master/CONFIGURATION.md)
