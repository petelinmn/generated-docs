---
title: "Quiz: monitoring and operations"
lang: en
track: rabbitmq
kind: module
doc: quiz
slug: 09-monitoring
part: 2
tags: [rabbitmq, monitoring, prometheus, alarms, management-api]
status: draft
updated: 2026-11-24
---

# Quiz: monitoring and operations

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 A queue has `messages` = 10, `messages_ready` = 7. What are the other 3?

- A) Expired
- B) Unacknowledged, held by a consumer
- C) In the dead-letter queue
- D) Deleted

<details><summary>Answer</summary>

**B.** `messages = messages_ready + messages_unacknowledged`.
→ [Basics](basics.en.md#the-management-http-api)

</details>

## 2. 🟢 How do you encode the vhost `/` in an API URL?

- A) `/`
- B) `%2F`
- C) `root`
- D) An empty string

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#the-management-http-api)

</details>

## 3. 🟢 Why set `connection_name`?

- A) To speed up the connection
- B) To find the application among the connections in management
- C) To enable TLS
- D) It is a required field

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#connections-channels-consumers)

</details>

## 4. 🟢 Which command shows queue depth without HTTP?

- A) `rabbitmqctl list_queues name messages_ready messages_unacknowledged consumers`
- B) `rabbitmq-plugins list`
- C) `rabbitmqctl cluster_status`
- D) `rabbitmq-diagnostics check_running`

<details><summary>Answer</summary>

**A.**
→ [Basics](basics.en.md#the-command-line)

</details>

## 5. 🟡 What is the Prometheus metrics port and how do `/metrics` and `/metrics/per-object` differ?

- A) 15672; they are the same
- B) 15692; the first is aggregated node metrics, the second is per queue
- C) 5672; the first for queues, the second for connections
- D) 15692; the first for a node, the second for the cluster

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#prometheus)

</details>

## 6. 🟡 What does `GET /api/health/checks/alarms` return on a memory alarm?

- A) `200 ok`
- B) `503` with the list of alarms
- C) `404`
- D) `401`

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#memory-and-disk-alarms)

</details>

## 7. 🟡 What does a publishing client get on a memory alarm?

- A) A refused connection
- B) `Connection.Blocked` with the reason `low on memory`
- C) Automatic deletion of messages
- D) Nothing

<details><summary>Answer</summary>

**B.** The connection's state in the API becomes `blocking`.
→ [Advanced theory](advanced.en.md#memory-and-disk-alarms)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) `rabbitmq-diagnostics check_local_alarms` reliably shows an active alarm in 4.4.1
- [ ] B) Management statistics are updated with a delay
- [ ] C) `/metrics/per-object` carries the label `queue`
- [ ] D) Prometheus obtains metrics by scraping port 15692

<details><summary>Answer</summary>

**B, C, D.** A is wrong: the command is deprecated and does nothing.
→ [Gotcha 1](expert.en.md#1-check_local_alarms-does-nothing)

</details>

## 9. 🟡 What do a channel's `prefetch_count` and `messages_unacknowledged` show?

- A) The limit and the number of unacknowledged messages of the consumer on that channel
- B) The queue size
- C) The number of connections
- D) The publish rate

<details><summary>Answer</summary>

**A.** In the experiment: 3 and 3.
→ [Basics](basics.en.md#connections-channels-consumers)

</details>

## 10. 🔴 Find the mistake

> "We made sure there are no alarms: `rabbitmq-diagnostics check_local_alarms` returned 'no local alarms'."

<details><summary>Answer</summary>

In 4.4.1 the command is deprecated and does nothing (verified: it also said "no local alarms" while a memory alarm was active). Check `/api/health/checks/alarms`.
→ [Gotcha 1](expert.en.md#1-check_local_alarms-does-nothing)

</details>

## 11. 🔴 The queue is growing, `consumers = 1`, `messages_unacknowledged` equals the prefetch and does not change. What is likely?

- A) The consumer is stuck and not acking
- B) The queue overflowed
- C) A disk alarm
- D) A connection leak

<details><summary>Answer</summary>

**A.** The consumer holds a prefetch worth of messages and does not ack; the rest wait ([module 03](../03-queues-and-delivery/basics.en.md#prefetch-and-fairness)).
→ [Advanced theory](advanced.en.md#what-to-alert-on)

</details>

## 12. 🔴 Open question

Design monitoring for a payment-tasks queue: which metrics, alerts and client reactions do you provide?

<details><summary>Key points of a good answer</summary>

- Metrics: `messages_ready`, `messages_unacknowledged`, `consumers`, DLQ, connections, memory/disk (Prometheus).
- Alerts on the growth trend of `ready`, `consumers = 0`, a non-empty DLQ, memory/disk alarms.
- An HTTP health check in probes; in the client a reaction to `Connection.Blocked`.
- Named connections, a Grafana dashboard.
- Tune the thresholds to the load (not verified in this material).

</details>

→ [Advanced theory](advanced.en.md#what-to-alert-on)
