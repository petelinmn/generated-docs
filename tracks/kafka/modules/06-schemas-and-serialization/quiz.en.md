---
title: "Quiz: schemas and serialization"
lang: en
track: kafka
kind: module
doc: quiz
slug: 06-schemas-and-serialization
part: 2
tags: [kafka, schema-registry, avro, compatibility, serialization]
status: draft
updated: 2026-11-24
---

# Quiz: schemas and serialization

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does Kafka store in a record's value?

- A) A structured object with type checking
- B) A set of bytes
- C) Only JSON
- D) The schema and the data together

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#why-schemas)

</details>

## 2. 🟢 What is in the first five bytes of a record with Avro and the Confluent registry?

- A) The topic name
- B) The magic byte 0 and four bytes of the schema id
- C) The record length
- D) A checksum

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#the-record-format-with-a-registry)

</details>

## 3. 🟢 What is the default subject for the value of records in topic `orders-av`?

- A) `orders-av`
- B) `orders-av-value`
- C) `value-orders-av`
- D) `schema-orders-av`

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#the-record-format-with-a-registry)

</details>

## 4. 🟢 Why is Avro more compact than JSON?

- A) It compresses data with gzip
- B) It stores only values, field names live in the schema
- C) It drops numbers
- D) It uses Base64

<details><summary>Answer</summary>

**B.** 16 bytes against 50 in the experiment.
→ [Basics](basics.en.md#json-and-avro)

</details>

## 5. 🟡 A field without a default is added under `BACKWARD`. What does the registry return at registration?

- A) `200`, the schema is accepted
- B) `409` (code `40901`)
- C) `404`
- D) `500`

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#schema-evolution-and-compatibility)

</details>

## 6. 🟡 A reader on `V2` (with `currency` defaulting to `USD`) got a record written with `V1`. What is in `currency`?

- A) A read error
- B) `None`
- C) `USD` (the default)
- D) An empty string

<details><summary>Answer</summary>

**C.**
→ [Advanced theory](advanced.en.md#what-readers-with-different-schemas-see)

</details>

## 7. 🟡 A reader pinned to `V1` got a `V2` record with the field `currency`. What does it see?

- A) An error
- B) The record without the field `currency`
- C) The record with `currency`
- D) `None` instead of the record

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#what-readers-with-different-schemas-see)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) A subject's mode can be set separately from the global one
- [ ] B) With `auto.register.schemas=False` an unknown schema gives a 404 error
- [ ] C) `BACKWARD` allows adding required fields without a default
- [ ] D) The registry is a separate service, not part of Kafka

<details><summary>Answer</summary>

**A, B, D.** C is wrong.
→ [Gotchas](expert.en.md#gotchas)

</details>

## 9. 🟡 What does `test_compatibility` do?

- A) Registers a schema
- B) Checks compatibility without registering
- C) Deletes a schema
- D) Changes the compatibility level

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#schema-evolution-and-compatibility)

</details>

## 10. 🔴 Find the mistake

> "We renamed the field `customer` to `client`: Avro will sort it out itself because the structure is the same."

<details><summary>Answer</summary>

Avro schema resolution matches fields by **name**: a rename is removing one field and adding another with no default. In the check the registry judged the rename incompatible (`test_compatibility` → `False`). With an alias (`"aliases": ["customer"]` on the new field) the check passed (`True`); without an alias add a new field with a default and migrate gradually.
→ [Best practices](advanced.en.md#best-practices)

</details>

## 11. 🔴 In production `auto.register.schemas=true`. What is the risk?

- A) No risk
- B) Any producer can register (and, with `NONE`, wreck) a schema outside review
- C) The registry stops caching
- D) Kafka slows down

<details><summary>Answer</summary>

**B.**
→ [Gotcha 5](expert.en.md#5-autoregisterschemastrue-by-default)

</details>

## 12. 🔴 Open question

How do you safely add a field `currency` to an order event without stopping producers and consumers?

<details><summary>Key points of a good answer</summary>

- A new schema with the field `currency` and a default; check `test_compatibility` in CI.
- Order under `BACKWARD`: roll out readers on the new schema first, then writers.
- Automatic registration is off; registration through CI.
- Old records in the topic remain, readers fill in the default.
- Not verified: `FORWARD`/`FULL`, JSON Schema/Protobuf.

</details>

→ [Advanced theory](advanced.en.md)
