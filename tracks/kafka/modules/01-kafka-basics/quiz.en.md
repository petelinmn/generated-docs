---
title: "Quiz: Kafka basics"
lang: en
track: kafka
kind: module
doc: quiz
slug: 01-kafka-basics
part: 1
tags: [kafka, topic, partition, offset, consumer-group]
status: draft
updated: 2026-11-25
---

# Quiz: Kafka basics

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What happens to a record in Kafka after a consumer has read it?

- A) It is deleted
- B) It stays in the log until retention expires
- C) It is moved to another topic
- D) It is marked as read for all groups

<details><summary>Answer</summary>

**B.** Another group reads the same records (in the test, the same six).
→ [Basics](basics.en.md#reading-and-offsets)

</details>

## 2. 🟢 What is an offset?

- A) The size of a record in bytes
- B) A record's sequence number within a partition
- C) A partition number
- D) The time of writing

<details><summary>Answer</summary>

**B.** An offset is the record's position number in its partition's log. It is not a size or a time, and the partition number is a separate value.
→ [Basics](basics.en.md#a-log-instead-of-a-queue)

</details>

## 3. 🟢 What ordering guarantee can you rely on?

- A) Across the whole topic
- B) Within one partition
- C) Across all topics
- D) There is no ordering at all

<details><summary>Answer</summary>

**B.** A partition is an ordered log, so order is guaranteed within it. There is no common order across partitions or topics, yet saying there is no order at all is wrong.
→ [Basics](basics.en.md#a-topic-and-partitions)

</details>

## 4. 🟢 What does `None` mean in the result of `create_topics`?

- A) The topic was not created
- B) The topic was created successfully
- C) The topic already exists
- D) You must retry

<details><summary>Answer</summary>

**B.** `create_topics` returns futures; a successful `result()` carries no value, hence `None`. Failures, including "already exists", raise an exception.
→ [Basics](basics.en.md#a-topic-and-partitions)

</details>

## 5. 🟡 Two records with key `user1` are published in a row. Where do they end up?

- A) In different partitions
- B) In one partition, with consecutive offsets
- C) In partition 0
- D) In random partitions

<details><summary>Answer</summary>

**B.** One key — one partition (in the test `user1` → partition 1, offsets 0, 1, 2).
→ [Basics](basics.en.md#writing)

</details>

## 6. 🟡 A new group with `auto.offset.reset=latest` reads a topic that already has data. What does it get?

- A) Everything from the start
- B) Only records that arrived after the subscription
- C) An error
- D) Only the last record

<details><summary>Answer</summary>

**B.** In the test: an empty result, then one new record.
→ [Basics](basics.en.md#where-to-read-from-when-a-group-has-no-stored-position)

</details>

## 7. 🟡 What does `LAG` mean in `kafka-consumer-groups --describe`?

- A) Network latency
- B) The difference between the end of the log and the group's position
- C) The number of consumers
- D) The size of records

<details><summary>Answer</summary>

**B.** Lag is how many records the group still has to read: log end minus committed position. It is not network latency, a consumer count or a size.
→ [Advanced theory](advanced.en.md#consumer-group-positions)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) Group positions are stored in the internal topic `__consumer_offsets`
- [ ] B) `auto.offset.reset` applies every time a group starts
- [ ] C) `produce` is asynchronous, the result arrives in a callback
- [ ] D) `process.roles=broker,controller` means a combined mode

<details><summary>Answer</summary>

**A, C, D.** B is wrong: only if the group has no stored position.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🟡 Can you reduce a topic's partitions from 3 to 2?

- A) Yes
- B) No, `InvalidPartitionsException`
- C) Yes, but only for an empty topic
- D) Yes, with `--delete`

<details><summary>Answer</summary>

**B.** Partitions can only be added: shrinking would discard or move records, so the broker refuses, even for an empty topic. `--delete` removes the whole topic.
→ [Gotcha 1](expert.en.md#1-partitions-cannot-be-reduced)

</details>

## 10. 🔴 Find the mistake

```python
p = Producer({"bootstrap.servers": "localhost:9092"})
p.produce("orders", key=b"k", value=b"v")
print("sent!")
```

<details><summary>Answer</summary>

There is no `flush()`: the record is only in the client buffer and the program may exit before sending; the result is learned in the `on_delivery` callback.
→ [Gotcha 5](expert.en.md#5-produce-is-not-sending)

</details>

## 11. 🔴 A group read the whole topic and is restarted with `auto.offset.reset=earliest`. What does reading return?

- A) The whole topic again
- B) Nothing: the stored position is at the end of the log
- C) Only the last record
- D) An error

<details><summary>Answer</summary>

**B.** `auto.offset.reset` applies only when there is no committed position. The group has one, at the end, so there is nothing to read and no error.
→ [Advanced theory](advanced.en.md#consumer-group-positions)

</details>

## 12. 🔴 Open question

A service needs order events in order per customer, and analytics needs to re-read the last day. How does that map onto topics, keys and groups?

<details><summary>Key points of a good answer</summary>

- The key is the customer id: a customer's events in one partition, order preserved.
- The order handler and analytics are separate consumer groups with independent positions.
- Retention long enough (168 h by default), analytics re-reads from the needed offset when necessary.
- Plan the number of partitions in advance: it cannot be reduced; increasing it changes the key mapping.

</details>

→ [Basics](basics.en.md)
