---
title: "Exercises: Kafka Connect"
lang: en
track: kafka
kind: module
doc: exercises
slug: 08-kafka-connect
part: 2
tags: [kafka-connect, connector, smt, rest-api]
status: draft
updated: 2026-11-24
---

# Exercises: Kafka Connect

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. You need a Kafka cluster and a Connect worker in distributed mode with `connect-file-*.jar` in `plugin.path` (see [how the code was verified](basics.en.md#how-the-code-was-verified)); REST at `http://localhost:18083`. The solutions were run on Kafka 4.3.1. Afterwards remove the `connect` container.

## 1. 🟢 A source

**Task:** create a `FileStreamSourceConnector` for the file `/tmp/in.txt` (two lines) and the topic `connect-test`. Print the status and the topic's records.

**Expected output:** code `201`, states `RUNNING`, two records (`"line one"`, `"line two"`).

<details><summary>Hint</summary>

`POST /connectors` with the body `{"name": …, "config": {…}}`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#a-source-a-file--a-topic).

</details>

## 2. 🟢 A sink

**Task:** create a `FileStreamSinkConnector` from the topic `connect-test` into the file `/tmp/out.txt`. Check the file's contents.

**Expected output:** the lines match those of the source file.

<details><summary>Hint</summary>

The topic parameter is `topics`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#a-sink-a-topic--a-file).

</details>

## 3. 🟡 Pause and resume

**Task:** pause the source, append a line to the file, look at the topic, then resume and look again.

**Expected output:** during the pause there is no new record, after `resume` it appears.

<details><summary>Hint</summary>

`PUT /connectors/<name>/pause` and `/resume`; pause and resume are asynchronous (`202`).

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#a-connectors-lifecycle).

</details>

## 4. 🟡 Transforms

**Task:** create a source with the chain `HoistField$Value` (field `line`) and `InsertField$Value` (a static field `origin`). Print the topic's record.

**Expected output:** `{"line":"secret-1","origin":"in2.txt"}`.

<details><summary>Hint</summary>

`transforms=wrap,addsrc`; parameters `transforms.<name>.…`.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#single-message-transforms-smt).

</details>

## 5. 🔴 An invalid configuration

**Task:** create a `FileStreamSourceConnector` without the `topic` parameter. Which code and message does the worker return, and is the connector created?

**Expected output:** `400`, a message about the missing `topic`, the connector not created.

<details><summary>Hint</summary>

Check `GET /connectors` after the attempt.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#configuration-validation).

</details>
