---
title: "Exercises: monitoring and operations"
lang: en
track: rabbitmq
kind: module
doc: exercises
slug: 09-monitoring
part: 2
tags: [rabbitmq, monitoring, prometheus, alarms, management-api]
status: draft
updated: 2026-11-24
---

# Exercises: monitoring and operations

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. You need a `rabbitmq:4-management` container with ports 5672, 15672 and 15692 (a disposable teaching setup, user `guest`/`guest`). Do not change memory thresholds on a working broker.

## 1. 🟢 Depth and unacknowledged

**Task:** publish 10 messages to a queue `mon`, start a consumer with `prefetch_count=3` that does not ack and read `/api/queues/%2F/mon`. Print `messages`, `messages_ready`, `messages_unacknowledged`, `consumers`.

**Expected output:** `10`, `7`, `3`, `1` (wait a few seconds: statistics update with a delay).

<details><summary>Hint</summary>

Basic auth `guest:guest`, the vhost `/` is encoded as `%2F`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#the-management-http-api).

</details>

## 2. 🟢 Find the application

**Task:** open a connection with `connection_name="monitor-demo"` and find it in `/api/connections`.

**Expected output:** the list contains a connection with `client_properties.connection_name == "monitor-demo"`.

<details><summary>Hint</summary>

`pika.ConnectionParameters("localhost", client_properties={"connection_name": "monitor-demo"})`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#connections-channels-consumers).

</details>

## 3. 🟡 Prometheus metrics

**Task:** read `http://localhost:15692/metrics` and find `rabbitmq_queue_messages_ready`, `rabbitmq_connections`. Then find the same queue in `/metrics/per-object`.

**Expected output:** `rabbitmq_queue_messages_ready 7` in the aggregated output, `rabbitmq_queue_messages_ready{vhost="/",queue="mon"} 7` in per-object.

<details><summary>Hint</summary>

Filter lines by the metric name prefix.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#prometheus).

</details>

## 4. 🟡 A memory alarm

**Task:** on a disposable broker set `rabbitmqctl set_vm_memory_high_watermark absolute 10MB`, publish a message, capture the answer of `/api/health/checks/alarms` and catch `Connection.Blocked`. Restore the threshold (`0.6`) and make sure the alarm is gone.

**Expected output:** `503`, `reason=low on memory`, after the reset `200` and `running`.

<details><summary>Hint</summary>

In pika a connection has `add_on_connection_blocked_callback`.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#memory-and-disk-alarms). Do not forget to restore the threshold.

</details>

## 5. 🔴 The deprecated-command trap

**Task:** during an active memory alarm run `rabbitmq-diagnostics check_local_alarms` and `rabbitmq-diagnostics alarms`. Compare them and explain the discrepancy.

**Expected output:** the first prints that it is deprecated and does nothing ("no local alarms"), the second `Memory alarm on node …`.

<details><summary>Hint</summary>

Read the first line of the output.

</details>

<details><summary>Solution</summary>

In 4.4.1 `check_local_alarms` is a deprecated stub; to check alarms use `rabbitmq-diagnostics alarms` or the HTTP check `/api/health/checks/alarms`.

</details>
