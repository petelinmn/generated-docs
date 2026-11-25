---
title: "Quiz: topics, retention and compaction"
lang: en
track: kafka
kind: module
doc: quiz
slug: 04-topics-and-retention
part: 1
tags: [kafka, retention, log-compaction, tombstone, segment]
status: draft
updated: 2026-11-25
---

# Quiz: topics, retention and compaction

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What removes records from a Kafka topic when the period expires?

- A) A consumer reading
- B) The retention policy: whole segments are deleted
- C) A consumer's ack
- D) The producer

<details><summary>Answer</summary>

**B.** The broker removes old data under its retention policy, whole segments at a time. Reading or committing deletes nothing, and neither does the producer.
→ [Basics](basics.en.md#retention)

</details>

## 2. 🟢 After cleaning the log became `(5, 5)`. Which offset does the next record get?

- A) 0
- B) 5
- C) 6
- D) Depends on the key

<details><summary>Answer</summary>

**B.** Offsets are not reused: numbering continues from `high`.
→ [Basics](basics.en.md#retention)

</details>

## 3. 🟢 What does `kafka-delete-records.sh` with `offset: 6` do?

- A) Deletes the topic
- B) Moves the partition's lower bound (`low_watermark`) to 6
- C) Deletes only the record at offset 6
- D) Copies records

<details><summary>Answer</summary>

**B.** The command removes everything before the given offset by raising `low_watermark`. It does not delete a single record or the topic, and copies nothing.
→ [Basics](basics.en.md#deleting-records-by-hand)

</details>

## 4. 🟢 What is the minimum allowed `segment.bytes`?

- A) 1 KiB
- B) 100000 bytes
- C) 1 MiB (1048576)
- D) There is no limit

<details><summary>Answer</summary>

**C.** The broker accepts no segment smaller than 1 MiB. Smaller values are rejected, so there is a limit.
→ [Basics](basics.en.md#a-segment-size-limit)

</details>

## 5. 🟡 What does a topic with `cleanup.policy=compact` keep?

- A) All records for a period
- B) The latest value of each key (with their old offsets)
- C) Only the topic's last record
- D) Only records without a key

<details><summary>Answer</summary>

**B.** Offsets are preserved, the numbering has gaps.
→ [Advanced theory](advanced.en.md#log-compaction)

</details>

## 6. 🟡 How do you delete a key in a compact topic?

- A) Publish a record with that key and the value `None` (a tombstone)
- B) Delete the partition
- C) Publish an empty key
- D) Change `group.id`

<details><summary>Answer</summary>

**A.** A tombstone is a record with the key and a null value; compaction then drops the key. Deleting a partition removes everything, an empty key is a different key, and `group.id` changes no data.
→ [Advanced theory](advanced.en.md#tombstones)

</details>

## 7. 🟡 The number of partitions was raised from 3 to 6. What about keys?

- A) All stayed in place
- B) Some keys move to other partitions
- C) All keys go to the new partitions
- D) Records vanish

<details><summary>Answer</summary>

**B.** In the test 4 keys of 8 moved.
→ [Advanced theory](advanced.en.md#adding-partitions-and-keys)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) Compaction preserves the offsets of the surviving records
- [ ] B) Deletion by retention works one record at a time
- [ ] C) `log.retention.check.interval.ms` cannot be changed dynamically
- [ ] D) An old producer may not know about new partitions

<details><summary>Answer</summary>

**A, C, D.** B is wrong: whole segments are deleted.
→ [Gotchas](expert.en.md#gotchas)

</details>

## 9. 🟡 What does `offsets_for_times` return for a timestamp `mid`?

- A) The last offset
- B) The first offset with a timestamp not earlier than `mid`
- C) The previous record's offset
- D) An error

<details><summary>Answer</summary>

**B.** Lookup by time returns the earliest offset whose timestamp is at or after the given one. It is neither the log end nor the previous record.
→ [Basics](basics.en.md#searching-by-time)

</details>

## 10. 🔴 Find the mistake

> "The topic has `retention.ms` = 7 days, so the last record will certainly live a week."

<details><summary>Answer</summary>

Whole segments are deleted, and a segment closes by size or `segment.ms`; in the experiment the log emptied entirely, last records included. The period is an upper bound, not a guarantee of keeping.
→ [Gotcha 1](expert.en.md#1-the-last-record-is-deleted-too)

</details>

## 11. 🔴 In a compact topic a consumer crashes on a record with `value is None`. What is wrong?

- A) Nothing, `None` is not allowed
- B) It is a tombstone, a key deletion: it must be handled
- C) It is an empty record, always skip it
- D) The broker corrupted the data

<details><summary>Answer</summary>

**B.** A null value is legal in a compacted topic and means the key is deleted. The consumer must remove it from its state; always skipping it leaves stale data, and nothing is corrupted.
→ [Gotcha 5](expert.en.md#5-tombstones-must-be-handled)

</details>

## 12. 🔴 Open question

A service needs a user's current profile (read by key) and a 30-day change history for audit. How do you organise the topics and policies?

<details><summary>Key points of a good answer</summary>

- The profile — a compact topic keyed by `user_id`, a tombstone for deletion.
- The history — a topic with `delete` and `retention.ms` of 30 days (with headroom).
- Plan the number of partitions in advance: changing it breaks key order.
- Consumers handle `None`.
- Not verified: `compact,delete`, the tombstone's exact lifetime.

</details>

→ [Advanced theory](advanced.en.md)
