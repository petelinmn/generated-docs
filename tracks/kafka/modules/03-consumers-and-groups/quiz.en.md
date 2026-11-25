---
title: "Quiz: consumers and groups"
lang: en
track: kafka
kind: module
doc: quiz
slug: 03-consumers-and-groups
part: 1
tags: [kafka, consumer, consumer-group, rebalance, offset-commit]
status: draft
updated: 2026-11-25
---

# Quiz: consumers and groups

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 How many consumers of one group receive the same record?

- A) All
- B) Exactly one
- C) Half
- D) Depends on the key

<details><summary>Answer</summary>

**B.** A partition is assigned to one group member.
→ [Basics](basics.en.md#a-consumer-group)

</details>

## 2. 🟢 A topic has 3 partitions, the group has 4 consumers. What happens?

- A) All four read
- B) One is left without partitions
- C) The broker refuses the fourth
- D) The partitions are split in half

<details><summary>Answer</summary>

**B.** In the test: `[[1], [2], [0], []]`.
→ [Basics](basics.en.md#more-consumers-than-partitions)

</details>

## 3. 🟢 What is committed after processing the record at offset 0?

- A) 0
- B) 1
- C) -1
- D) Nothing

<details><summary>Answer</summary>

**B.** The next offset to read is committed.
→ [Basics](basics.en.md#the-committed-offset-is-the-next-one-to-read)

</details>

## 4. 🟢 What does `committed` return for a partition with no stored position?

- A) 0
- B) `-1001` (`OFFSET_INVALID`)
- C) An exception
- D) The end of the log

<details><summary>Answer</summary>

**B.** A missing position is the special value `-1001`. Zero would be a real position, no exception is raised, and the log end is not substituted.
→ [Basics](basics.en.md#committing-offsets)

</details>

## 5. 🟡 A consumer processed six records and closed without `commit` (auto-commit off). What happens on restart?

- A) Nothing comes
- B) The same records come again
- C) Only the last one
- D) An error

<details><summary>Answer</summary>

**B.** At-least-once delivery.
→ [Basics](basics.en.md#what-happens-without-a-commit)

</details>

## 6. 🟡 What distinguishes cooperative rebalancing from eager?

- A) It revokes only the moving partitions
- B) It revokes all partitions
- C) It is faster on disk
- D) It needs a new broker

<details><summary>Answer</summary>

**A.** In the test only partition 0 was revoked from A.
→ [Advanced theory](advanced.en.md#cooperative-rebalancing)

</details>

## 7. 🟡 A pause between `poll` calls is longer than `max.poll.interval.ms`. What do you get?

- A) Nothing
- B) `_MAX_POLL_EXCEEDED` on the next `poll`
- C) Automatic commit
- D) New partitions

<details><summary>Answer</summary>

**B.** The client treats processing as stuck and leaves the group, which the next `poll` reports. Nothing is committed and no partitions are gained.
→ [Advanced theory](advanced.en.md#member-liveness-maxpollintervalms)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) Eager rebalancing revokes partitions from all members
- [ ] B) `commit(message=m)` commits `offset + 1`
- [ ] C) `assign` + `seek` allow reading without a group
- [ ] D) Two applications with one `group.id` read the topic independently and fully

<details><summary>Answer</summary>

**A, B, C.** D is wrong: they will share the partitions.
→ [Gotchas](expert.en.md#gotchas)

</details>

## 9. 🟡 Which parameter enables the new server-side group protocol?

- A) `group.protocol=consumer`
- B) `enable.idempotence`
- C) `acks=all`
- D) `compression.type`

<details><summary>Answer</summary>

**A.** In the test only partition 2 was revoked from A.
→ [Advanced theory](advanced.en.md#the-new-group-protocol-groupprotocolconsumer)

</details>

## 10. 🔴 Find the mistake

```python
while True:
    m = c.poll(1.0)
    if m is None: continue
    c.commit(message=m, asynchronous=False)
    process(m)   # may fail
```

<details><summary>Answer</summary>

The commit comes **before** processing: if `process` fails the record counts as handled and is lost. Commit after successful processing and make the handler idempotent.
→ [Basics](basics.en.md#what-happens-without-a-commit)

</details>

## 11. 🔴 Handling one record takes 10 minutes, `max.poll.interval.ms` is the default. What is wrong and what do you do?

- A) Nothing: Kafka will wait
- B) The consumer will drop out of the group with `_MAX_POLL_EXCEEDED`; process in portions or in separate threads without blocking `poll`
- C) Increase the number of partitions
- D) Enable compression

<details><summary>Answer</summary>

**B.** The default interval is 5 minutes, shorter than the processing time, so the group considers the consumer gone. Kafka does not wait forever, and more partitions or compression do not shorten processing.
→ [Gotcha 5](expert.en.md#5-long-processing--_max_poll_exceeded)

</details>

## 12. 🔴 Open question

A service must scale quickly by adding instances, and one order's record order must be preserved. How do you choose the number of partitions, the key and the rebalance strategy?

<details><summary>Key points of a good answer</summary>

- The key is the order id; the number of partitions with headroom (consumers no more than partitions).
- All instances are one group; `cooperative-sticky` or the new protocol for soft rebalances.
- A manual commit after processing, an idempotent handler.
- Do not block `poll`; close the consumer with `close()`.
- Not verified: static membership, `session.timeout.ms`.

</details>

→ [Advanced theory](advanced.en.md)
