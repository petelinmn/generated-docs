---
title: "Quiz: producers"
lang: en
track: kafka
kind: module
doc: quiz
slug: 02-producers
part: 1
tags: [kafka, producer, acks, partitioning, idempotence, compression]
status: draft
updated: 2026-11-24
---

# Quiz: producers

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 How is the partition of a record with a key determined?

- A) Randomly
- B) By the key's hash: one key — one partition
- C) Always partition 0
- D) By the time of writing

<details><summary>Answer</summary>

**B.** An explicit `partition=` overrides the key.
→ [Basics](basics.en.md#how-the-partition-is-chosen)

</details>

## 2. 🟢 What does `acks=all` mean?

- A) Do not wait for an acknowledgement
- B) Wait for the leader's write
- C) Wait for the leader and all in-sync replicas
- D) Wait for an acknowledgement from all consumers

<details><summary>Answer</summary>

**C.**
→ [Basics](basics.en.md#broker-acknowledgements-acks)

</details>

## 3. 🟢 Where does a record's delivery result arrive?

- A) In `produce`'s return value
- B) In the `on_delivery` callback after `flush()`/`poll()`
- C) In a separate topic
- D) Nowhere

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#the-delivery-callback)

</details>

## 4. 🟢 What does the delivery report hold in the offset field with `acks=0`?

- A) 0
- B) `None`
- C) -1
- D) The offset of the last record

<details><summary>Answer</summary>

**B.** In the test: `offsets reported: {None}`.
→ [Basics](basics.en.md#broker-acknowledgements-acks)

</details>

## 5. 🟡 What happens with `enable.idempotence=True` and `acks=1`?

- A) Idempotence is switched off silently
- B) The client refuses the configuration (`_INVALID_ARG`)
- C) The record is sent twice
- D) The broker closes the connection

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#the-idempotent-producer)

</details>

## 6. 🟡 The delivery latency of one record at `linger.ms=100`?

- A) ≈ 1 ms
- B) ≈ 100 ms
- C) ≈ 10 s
- D) Depends on the partition

<details><summary>Answer</summary>

**B.** In the test 106–115 ms against 0–1 ms at `linger.ms=0`.
→ [Advanced theory](advanced.en.md#batches-and-delay-lingerms)

</details>

## 7. 🟡 What happens on a 2 MB record with default settings?

- A) The record goes through
- B) `MSG_SIZE_TOO_LARGE` at `produce`
- C) The record is silently lost
- D) The record is split into batches

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#errors)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) Without a key records are spread evenly and predictably over partitions
- [ ] B) Compression applies to a whole batch
- [ ] C) `flush()` waits for sending and runs the callbacks
- [ ] D) An unreachable broker gives `_MSG_TIMED_OUT` in the callback

<details><summary>Answer</summary>

**B, C, D.** A is wrong: distribution without a key is unpredictable.
→ [Gotchas](expert.en.md#gotchas)

</details>

## 9. 🟡 Why is the compression ratio from the experiment (37 MB → 0.1 MB) unrealistic?

- A) A measurement error
- B) The data was extremely repetitive
- C) zstd compresses everything hundreds of times
- D) The broker compresses further itself

<details><summary>Answer</summary>

**B.** On real data it is several times.
→ [Gotcha 7](expert.en.md#7-compression-on-test-data)

</details>

## 10. 🔴 Find the mistake

```python
p = Producer({"bootstrap.servers": BS, "acks": "0"})
p.produce("payments", key=b"order-1", value=b"paid")
p.flush()
print("payment recorded")
```

<details><summary>Answer</summary>

With `acks=0` the broker acknowledges nothing: a lost record goes unnoticed and the offset is unknown. For payments you need `acks=all`, idempotence and an `err` check in the callback.
→ [Gotcha 1](expert.en.md#1-acks0-does-not-know-the-offset)

</details>

## 11. 🔴 One customer's events reach the consumer out of order. What do you check on the producer?

- A) That the records have a common key (the customer id)
- B) That compression is on
- C) That `linger.ms` is 0
- D) That `acks=0`

<details><summary>Answer</summary>

**A.** Without a key records spread over partitions, and order exists only within a partition.
→ [Basics](basics.en.md#records-without-a-key)

</details>

## 12. 🔴 Open question

A service sends payment events that must be neither lost nor duplicated. Which producer settings and what in the callback do you provide?

<details><summary>Key points of a good answer</summary>

- `acks=all`, `enable.idempotence=true`; the key is the payment/order id.
- An `err` check in the callback, retries or alerting, `flush()` on shutdown.
- Compression and `linger.ms` by latency; record size within the limit.
- Duplicates on the consumer side are still possible (module 05).
- Not verified: the default value of idempotence and behaviour under real retries.

</details>

→ [Advanced theory](advanced.en.md)
