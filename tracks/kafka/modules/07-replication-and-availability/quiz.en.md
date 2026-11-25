---
title: "Quiz: replication and availability"
lang: en
track: kafka
kind: module
doc: quiz
slug: 07-replication-and-availability
part: 2
tags: [kafka, replication, isr, min-insync-replicas, leader-election]
status: draft
updated: 2026-11-25
---

# Quiz: replication and availability

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Through whom do a partition's writes and reads go?

- A) Through any follower
- B) Through the leader
- C) Through the controller
- D) Through the client

<details><summary>Answer</summary>

**B.** The leader takes writes and by default serves reads, while followers copy it. The controller manages metadata and no data flows through it.
→ [Basics](basics.en.md#replication)

</details>

## 2. 🟢 What is the ISR?

- A) All brokers of the cluster
- B) The replicas that keep up with the leader
- C) Replicas on disk
- D) Replicas in another data centre

<details><summary>Answer</summary>

**B.** The ISR is the set of a partition's replicas that are sufficiently caught up. It is not all brokers, anything on disk or another data centre.
→ [Basics](basics.en.md#replication)

</details>

## 3. 🟢 A partition's leader is stopped. What happens?

- A) The partition is unavailable forever
- B) An in-sync follower becomes the new leader
- C) The data is deleted
- D) Clients must switch broker by hand

<details><summary>Answer</summary>

**B.** In the test broker 1 became the leader and the ISR shrank to `[1, 2]`.
→ [Basics](basics.en.md#a-leader-failure)

</details>

## 4. 🟢 Which replication factor is sensible for important topics in production?

- A) 1
- B) 2
- C) 3
- D) 10

<details><summary>Answer</summary>

**C.** With `min.insync.replicas=2`.
→ [Basics](basics.en.md#common-beginner-mistakes)

</details>

## 5. 🟡 RF=3, `min.insync.replicas=2`, 1 broker alive. What about an `acks=all` write?

- A) It goes through
- B) It is not accepted; the client gets `_MSG_TIMED_OUT`
- C) It is lost silently
- D) It goes to another cluster

<details><summary>Answer</summary>

**B.** One replica is below the required two, so no acknowledgement comes and the client eventually gets a delivery timeout. Nothing is silently lost or sent to another cluster.
→ [Basics](basics.en.md#acks-and-mininsyncreplicas)

</details>

## 6. 🟡 The same with `acks=1` and a live leader?

- A) It is not accepted
- B) It goes through but is protected only by the leader
- C) It goes through and is protected by three copies
- D) A `NOT_ENOUGH_REPLICAS` error

<details><summary>Answer</summary>

**B.** `min.insync.replicas` applies only to `acks=all`. The leader acknowledges alone, and until replicas return there is a single copy.
→ [Basics](basics.en.md#acks-and-mininsyncreplicas)

</details>

## 7. 🟡 What happens on trying to create a topic when 1 of 3 combined nodes is alive?

- A) The topic is created
- B) A timeout: there is no controller quorum
- C) The topic is created with RF=1
- D) An authorization error

<details><summary>Answer</summary>

**B.** Creating a topic changes metadata, and one vote in three is no quorum. The topic does not appear even with a lower RF, and permissions are not the issue.
→ [Advanced theory](advanced.en.md#the-controller-quorum)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) A new leader is chosen from the ISR
- [ ] B) After recovery leaders always return to the preferred brokers at once
- [ ] C) `--under-replicated-partitions` is empty on a healthy cluster
- [ ] D) `acks=all` without `min.insync.replicas` ≥ 2 gives weak protection

<details><summary>Answer</summary>

**A, C, D.** B is wrong: in the experiment leaders stayed on broker 2.
→ [Gotchas](expert.en.md#gotchas)

</details>

## 9. 🟡 How do you learn that replication is unhealthy?

- A) `--under-replicated-partitions` shows partitions
- B) `kafka-topics.sh --list`
- C) By disk size
- D) By the number of consumers

<details><summary>Answer</summary>

**A.** This command lists partitions whose ISR is smaller than the replica set. A topic list, disk size or consumer count does not reveal it.
→ [Advanced theory](advanced.en.md#watching-the-state)

</details>

## 10. 🔴 Find the mistake

> "We have RF=3 and `acks=all`, so losing two of three brokers does not scare us."

<details><summary>Answer</summary>

With `min.insync.replicas=2`, after two brokers are lost writes stop (`_MSG_TIMED_OUT`), while with `min.insync.replicas=1` and `acks=all` a write stays on a single copy, and losing it loses data. Losing **one** broker is tolerable.
→ [Gotcha 7](expert.en.md#7-two-brokers-with-rf3)

</details>

## 11. 🔴 All three cluster nodes are combined. What does losing two of them mean?

- A) Nothing, the data is on the third
- B) Brokers and the controller quorum are lost: metadata does not change, writes are possible only to partitions with a live leader
- C) All data is lost
- D) The cluster rebuilds itself automatically

<details><summary>Answer</summary>

**B.** A combined node is both broker and controller, so losing two removes the voting majority too. Data on the surviving node remains, but the cluster does not rebuild itself.
→ [Gotcha 4](expert.en.md#4-metadata-needs-a-controller-quorum)

</details>

## 12. 🔴 Open question

How do you configure a payments topic and producers so as not to lose records when one broker fails and not to accept a write when protection is insufficient?

<details><summary>Key points of a good answer</summary>

- RF=3, `min.insync.replicas=2`; the producer `acks=all`, idempotence.
- A write timeout and handling of `_MSG_TIMED_OUT`: retry/alert.
- Brokers in different racks/zones; dedicated controllers.
- Monitor under-replicated partitions; upgrade one at a time.
- Not verified: unclean elections, `replica.lag.time.max.ms`.

</details>

→ [Basics](basics.en.md)
