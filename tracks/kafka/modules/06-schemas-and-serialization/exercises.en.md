---
title: "Exercises: schemas and serialization"
lang: en
track: kafka
kind: module
doc: exercises
slug: 06-schemas-and-serialization
part: 2
tags: [kafka, schema-registry, avro, compatibility]
status: draft
updated: 2026-11-24
---

# Exercises: schemas and serialization

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. You need a Kafka broker, Confluent Schema Registry (`confluentinc/cp-schema-registry`, port 8081 or another) and `pip install "confluent-kafka[avro,schemaregistry]" fastavro`. The solutions were run on Apache Kafka 4.3.1 and Schema Registry 8.4.1.

## 1. 🟢 JSON against Avro

**Task:** encode the record `{"id": 1001, "customer": "alice", "amount": 19.99}` in JSON and in Avro (`fastavro.schemaless_writer`). Compare the sizes.

**Expected output:** `50` and `16` bytes.

<details><summary>Hint</summary>

`fastavro.parse_schema(json.loads(V1))`, writing to an `io.BytesIO`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#json-and-avro).

</details>

## 2. 🟢 The record format

**Task:** serialize a record with `AvroSerializer` and print the magic byte, the schema id and the length.

**Expected output:** magic byte `0`, a schema id (`1` in the experiment), 21 bytes in total.

<details><summary>Hint</summary>

`struct.unpack(">I", payload[1:5])`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#the-record-format-with-a-registry).

</details>

## 3. 🟡 Compatibility

**Task:** register `V1`, then check (`test_compatibility`) a version with a field `currency` (a) with a default and (b) without. Try to register (b).

**Expected output:** (a) `True`, (b) `False`; registering (b) gives `409`, code `40901`.

<details><summary>Hint</summary>

The error is a `SchemaRegistryError` with the fields `http_status_code` and `error_code`.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#schema-evolution-and-compatibility).

</details>

## 4. 🟡 Readers of different versions

**Task:** write a record with `V2` (with `currency="EUR"`) and read it with a reader with no reader schema and with a reader on `V1`; then read a `V1` record with a reader on `V2`.

**Expected output:** the first sees `currency`, the second does not; the third sees `currency="USD"`.

<details><summary>Hint</summary>

`AvroDeserializer(sr, reader_schema_str)`.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#what-readers-with-different-schemas-see).

</details>

## 5. 🔴 A rename and a type change

**Task:** check `test_compatibility` for (a) renaming the field `customer` → `client`, (b) the same with `aliases`, (c) changing `amount` from double to string.

**Expected output:** (a) `False`, (b) `True`, (c) `False`.

<details><summary>Hint</summary>

The alias is a property of the new-name field: `"aliases": ["customer"]`.

</details>

<details><summary>Solution</summary>

See [quiz question 10](quiz.en.md#10--find-the-mistake) and the [expert theory](expert.en.md#interview-questions).

</details>
