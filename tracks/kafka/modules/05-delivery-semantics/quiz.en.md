---
title: "Quiz: delivery semantics"
lang: en
track: kafka
kind: module
doc: quiz
slug: 05-delivery-semantics
part: 2
tags: [kafka, exactly-once, transactions, isolation-level, idempotence]
status: draft
updated: 2026-11-24
---

# Quiz: delivery semantics

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 How do you get at-least-once semantics on a consumer?

- A) Commit the position before processing
- B) Commit the position after processing
- C) Never commit
- D) Enable compression

<details><summary>Answer</summary>

**B.** Committing before processing gives at-most-once.
→ [Basics](basics.en.md#three-delivery-semantics)

</details>

## 2. 🟢 What does `isolation.level=read_committed` do?

- A) Shows only records of committed transactions
- B) Encrypts reading
- C) Reads faster
- D) Shows aborted records

<details><summary>Answer</summary>

**A.**
→ [Basics](basics.en.md#producer-transactions)

</details>

## 3. 🟢 What does an idempotent producer protect?

- A) A write to several topics atomically
- B) One producer's send retries inside a partition
- C) External effects (emails)
- D) A consumer's position commit

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#the-idempotent-producer--the-first-step)

</details>

## 4. 🟢 What must you call right after creating a transactional producer?

- A) `begin_transaction()`
- B) `init_transactions()`
- C) `commit_transaction()`
- D) `flush()`

<details><summary>Answer</summary>

**B.** Otherwise `_STATE`.
→ [Advanced theory](advanced.en.md#call-order)

</details>

## 5. 🟡 What does a `read_uncommitted` reader see in a topic after an aborted transaction (the records were sent to the broker)?

- A) Only committed records
- B) Both committed and aborted records
- C) Nothing
- D) Only markers

<details><summary>Answer</summary>

**B.** In the test `aborted-0`, `aborted-1` were visible.
→ [Basics](basics.en.md#producer-transactions)

</details>

## 6. 🟡 How do you atomically write a result and commit the input topic's position?

- A) Call the consumer's `commit()` right after `produce`
- B) `send_offsets_to_transaction` in the same transaction
- C) Two independent `commit`s
- D) Enable auto-commit

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#consume-process-produce-exactly-once)

</details>

## 7. 🟡 Two producers with the same `transactional.id`; the second called `init_transactions()`. What about the first?

- A) It keeps working
- B) It gets a fatal `_FENCED`
- C) It gets `_STATE`
- D) It becomes a reader

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#zombies-and-fencing)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) Transaction markers take offsets
- [ ] B) `abort_transaction()` without `flush()` may discard records not yet sent
- [ ] C) "Exactly once" covers sending an email
- [ ] D) `read_uncommitted` is the reader's default mode

<details><summary>Answer</summary>

**A, B, D.** C is wrong: only Kafka→Kafka.
→ [Gotchas](expert.en.md#gotchas)

</details>

## 9. 🟡 Where do the gaps in a transactional topic's offsets come from?

- A) A broker error
- B) Commit/abort markers of transactions take offsets
- C) Compression
- D) Compaction

<details><summary>Answer</summary>

**B.** `high` = 8 with five records and three markers.
→ [Basics](basics.en.md#producer-transactions)

</details>

## 10. 🔴 Find the mistake

```python
while True:
    m = c.poll(1.0)
    if m is None: continue
    p.produce("out", value=transform(m.value()))   # non-transactional producer
    c.commit(message=m)
```

<details><summary>Answer</summary>

Writing the result and committing the position are not atomic: on a failure between them the result is written but the position is not, so after a restart the record is processed again and `out` has a duplicate. You need transactions with `send_offsets_to_transaction` (or an idempotent receiver).
→ [Advanced theory](advanced.en.md#consume-process-produce-exactly-once)

</details>

## 11. 🔴 An "exactly once" service reads Kafka and sends emails. What is wrong?

- A) Nothing
- B) Emails are an external effect: there is no atomicity with Kafka, you need idempotent sending or an outbox
- C) You need compression
- D) You need `acks=0`

<details><summary>Answer</summary>

**B.**
→ [Gotcha 6](expert.en.md#6-external-effects)

</details>

## 12. 🔴 Open question

A pipeline reads payments from the topic `payments`, enriches them and writes to `enriched`. How do you ensure no losses and no duplicates, and what do you do about a hung instance?

<details><summary>Key points of a good answer</summary>

- Transactions: a stable `transactional.id` per instance, `send_offsets_to_transaction`, readers of the result in `read_committed`.
- A hung instance is fenced by a new `init_transactions()` (`_FENCED` is fatal, restart).
- External effects are idempotent or go through an outbox.
- Short transactions; monitor long ones.
- Not verified: transaction timeouts, the effect on throughput.

</details>

→ [Advanced theory](advanced.en.md)
