---
title: "Quiz: Kafka Connect"
lang: en
track: kafka
kind: module
doc: quiz
slug: 08-kafka-connect
part: 2
tags: [kafka-connect, connector, smt, rest-api]
status: draft
updated: 2026-11-24
---

# Quiz: Kafka Connect

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does a source connector do?

- A) Reads from Kafka and writes to an external system
- B) Reads from an external system and writes to Kafka
- C) Stores schemas
- D) Chooses a partition leader

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#what-kafka-connect-is)

</details>

## 2. 🟢 What code did the REST API answer with on a successful connector creation?

- A) 200
- B) 201
- C) 202
- D) 204

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#a-source-a-file--a-topic)

</details>

## 3. 🟢 What is the topic parameter called on a sink?

- A) `topic`
- B) `topics`
- C) `target`
- D) `destination`

<details><summary>Answer</summary>

**B.** A source has `topic`.
→ [Basics](basics.en.md#a-sink-a-topic--a-file)

</details>

## 4. 🟢 What happens to a line appended to a file that a running source reads?

- A) It is lost
- B) It appears in the topic
- C) The connector stops
- D) A restart is needed

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#a-source-a-file--a-topic)

</details>

## 5. 🟡 A line is appended to a file while the source is paused. What after `resume`?

- A) The line is lost
- B) The line appears in the topic
- C) The connector is deleted
- D) The line appears twice

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#a-connectors-lifecycle)

</details>

## 6. 🟡 What did the worker return on creating a connector without a required parameter?

- A) 201 and an error in the status
- B) 400 with a description, the connector not created
- C) 500
- D) 202

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#configuration-validation)

</details>

## 7. 🟡 What did the chain `HoistField$Value` + `InsertField$Value` give for the line `secret-1`?

- A) The line unchanged
- B) The JSON object `{"line":"secret-1","origin":"in2.txt"}`
- C) An error
- D) An empty record

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#single-message-transforms-smt)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) A connector pause does not lose source data
- [ ] B) `FileStream*` are connectors for production data
- [ ] C) Worker state is stored in Kafka topics
- [ ] D) SMTs apply to records in order

<details><summary>Answer</summary>

**A, C, D.** B is wrong: they are examples.
→ [Gotchas](expert.en.md#gotchas)

</details>

## 9. 🟡 What is a converter for?

- A) Encryption
- B) Turning Connect's data into Kafka's bytes and back
- C) Choosing a partition
- D) Checking compatibility

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#converters)

</details>

## 10. 🔴 Find the mistake

> "For reliability we will run one Connect worker and put the internal topics at RF=1."

<details><summary>Answer</summary>

One worker is a single point of failure, and internal topics with RF=1 lose state when a broker fails. You need several workers and RF=3 for `connect-offsets`, `connect-configs`, `connect-status`.
→ [Gotcha 7](expert.en.md#7-one-worker-is-not-fault-tolerance)

</details>

## 11. 🔴 You need complex aggregations and stream joins. What do you choose?

- A) An SMT chain
- B) Stream processing (Kafka Streams and the like)
- C) Raise `tasks.max`
- D) Another converter

<details><summary>Answer</summary>

**B.**
→ [Trade-offs](expert.en.md#trade-offs-and-alternatives)

</details>

## 12. 🔴 Open question

You need to move changes from a database into Kafka and on into a store. How does Kafka Connect organise it, and what must a production system provide?

<details><summary>Key points of a good answer</summary>

- A source connector for the database, a sink connector for the store; a format with a schema registry.
- Several workers, RF=3 for the internal topics; configurations in a repository.
- Monitor connector and task states; alert on `FAILED`.
- Error handling and a dead-letter topic (not verified here).
- SMTs for simple edits, anything complex handled separately.

</details>

→ [Basics](basics.en.md)
