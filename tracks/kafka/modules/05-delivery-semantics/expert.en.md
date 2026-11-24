---
title: "Delivery semantics: expert level"
lang: en
track: kafka
kind: module
doc: expert
slug: 05-delivery-semantics
part: 2
tags: [kafka, exactly-once, transactions, isolation-level, idempotence, fencing, transactional-id]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Delivery semantics: expert level

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on a single Apache Kafka 4.3.1 broker with `confluent-kafka` 2.16.2.

## Reference

### The producer transaction API

| Method | Purpose | Verified |
|---|---|---|
| `Producer({"transactional.id": id, ...})` | creates a transactional producer | yes |
| `init_transactions()` | registers the `transactional.id`, fences earlier ones | yes |
| `begin_transaction()` | starts a transaction | yes |
| `produce(...)` | a write within a transaction | yes |
| `send_offsets_to_transaction(offsets, group_metadata)` | includes the consumer's offsets in the transaction | yes |
| `commit_transaction()` / `abort_transaction()` | ends it | yes |
| `consumer.consumer_group_metadata()` | group metadata for `send_offsets_to_transaction` | yes |

### Parameters

| Parameter | Value | Verified |
|---|---|---|
| `transactional.id` | a stable producer name | yes |
| the reader's `isolation.level` | `read_uncommitted` / `read_committed` | yes |
| `enable.idempotence` | idempotence (enabled by transactions) | idempotence — [module 02](../02-producers/advanced.en.md#the-idempotent-producer) |
| `transaction.timeout.ms`, `transactional.id.expiration.ms`, `max.in.flight.requests.per.connection` | timeouts and order | not tested |

### Errors (verified)

| Situation | Error |
|---|---|
| `produce` without `init_transactions()` | `_STATE` ("Local: Erroneous state") |
| a producer fenced by a new one with the same `transactional.id` | `_FENCED`, `fatal() == True` |

### What a reader sees (verified)

| Record type | `read_uncommitted` | `read_committed` |
|---|---|---|
| committed | yes | yes |
| aborted (sent to the broker) | yes | no |
| open | yes | no |
| transaction markers | not handed to the application, but take offsets | the same |

## Internals

- **Markers (control records)** in a partition's log mark a transaction's completion; they are not given to the application but take offsets (verified: `high` = 8 with five records).
- **A `read_committed` reader** sees only records of completed transactions (verified: an open one is invisible).
- **Fencing:** a new `init_transactions()` with the same `transactional.id` raises the "epoch" and invalidates the earlier producer (`_FENCED`).
- **Read-write atomicity** is achieved by writing the consumer's offsets to `__consumer_offsets` in the same transaction.
- **`abort_transaction()` discards records not yet sent** (verified: without `flush()` before the abort the records never reached the broker).

> ⚠️ Verify: the transaction coordinator and its log `__transaction_state`, the LSO (last stable offset) mechanism and the timeout of hanging transactions are described from the documentation and were not verified here.

## Gotchas

### 1. Idempotence is not transactions

It protects against one producer's send retries into one partition.

### 2. `read_uncommitted` is the default

A reader without `isolation.level=read_committed` sees aborted records (verified).

### 3. Markers take offsets

Offsets in a transactional topic have gaps (verified).

### 4. `abort` without `flush`

Unsent records are discarded; the aborted transaction is then "empty" (verified).

### 5. `_FENCED` is fatal

You cannot retry: restart the process (verified).

### 6. External effects

"Exactly once" does not extend to databases and emails.

### 7. A long open transaction

It delays `read_committed` readers (per the documentation).

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| at-least-once + an idempotent handler | simple, no transactions | duplicates must be tolerated |
| transactions (exactly-once) | Kafka→Kafka atomicity | more complex, latency, only inside Kafka |
| at-most-once | no duplicates | losses |
| outbox | consistency with the database | an extra table and process |

## Interview questions

<details><summary>1. How does at-least-once differ from exactly-once?</summary>

With at-least-once a record may come again; exactly-once adds transactions so the effect is one.

→ [Basics](basics.en.md#three-delivery-semantics)

</details>

<details><summary>2. What does an idempotent producer give?</summary>

Protection against duplicates from send retries inside a partition.

→ [Basics](basics.en.md#the-idempotent-producer--the-first-step)

</details>

<details><summary>3. What is `read_committed`?</summary>

A reader mode that hides records of aborted and open transactions.

→ [Basics](basics.en.md#producer-transactions)

</details>

<details><summary>4. Trick: why does a transactional topic's offsets have gaps?</summary>

Transaction-completion markers take offsets.

→ [Gotcha 3](#3-markers-take-offsets)

</details>

<details><summary>5. How do you atomically write a result and commit the read position?</summary>

`send_offsets_to_transaction` in the same transaction.

→ [Advanced theory](advanced.en.md#consume-process-produce-exactly-once)

</details>

<details><summary>6. What is a "zombie" and how do you defend against it?</summary>

An old hung process; a new `init_transactions()` with the same `transactional.id` fences it (`_FENCED`).

→ [Advanced theory](advanced.en.md#zombies-and-fencing)

</details>

<details><summary>7. Trick: does "exactly once" cover sending an email?</summary>

No: only Kafka→Kafka.

→ [Gotcha 6](#6-external-effects)

</details>

<details><summary>8. What happens on `produce` without `init_transactions()`?</summary>

A `_STATE` error.

→ [Advanced theory](advanced.en.md#call-order)

</details>

<details><summary>9. Trick: what does `_FENCED` with `fatal=True` mean?</summary>

The producer is invalid; restart the process, do not retry.

→ [Gotcha 5](#5-_fenced-is-fatal)

</details>

<details><summary>10. Why did `abort_transaction()` without `flush()` leave no aborted records?</summary>

It discards records that were not sent.

→ [Gotcha 4](#4-abort-without-flush)

</details>

<details><summary>11. When is at-least-once with an idempotent handler better?</summary>

When simplicity matters or the result goes to an external system.

→ [Trade-offs](#trade-offs-and-alternatives)

</details>

<details><summary>12. Why a stable `transactional.id`?</summary>

So a restart can fence off the earlier instance and continue from the last state.

→ [Advanced theory](advanced.en.md#zombies-and-fencing)

</details>

## Cheat sheet

```text
Producer({"transactional.id": stable_id}) ; init_transactions() ; begin_transaction() ; produce() ; send_offsets_to_transaction(offs, c.consumer_group_metadata()) ; commit_transaction() | abort_transaction()
reader: isolation.level=read_committed (default is read_uncommitted)
errors: _STATE (wrong order), _FENCED (fatal: restart) ; markers take offsets ; EOS = Kafka -> Kafka only
```

- Transactions: atomicity of writes and position.
- `read_committed` for readers of the result.
- External effects — idempotence or an outbox.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [04 · Topics, retention and compaction](../04-topics-and-retention/expert.en.md)
- Next module: [06 · Schemas and serialization](../06-schemas-and-serialization/overview.en.md)

## Sources

- [Apache Kafka: Message delivery semantics](https://kafka.apache.org/documentation/#semantics)
- [Apache Kafka: Transactions](https://kafka.apache.org/documentation/#design_transactions)
- [KIP-98](https://cwiki.apache.org/confluence/display/KAFKA/KIP-98+-+Exactly+Once+Delivery+and+Transactional+Messaging)
