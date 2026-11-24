---
title: "Quiz: flow control and performance"
lang: en
track: rabbitmq
kind: module
doc: quiz
slug: 10-performance
part: 3
tags: [rabbitmq, performance, flow-control, prefetch, publisher-confirms]
status: draft
updated: 2026-11-24
---

# Quiz: flow control and performance

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which `prefetch_count` is the slowest for reading?

- A) 1
- B) 10
- C) 100
- D) 1000

<details><summary>Answer</summary>

**A.** In the test: 1.8k messages/s against 29.4k at 1000.
→ [Basics](basics.en.md#reading-prefetch-and-acks)

</details>

## 2. 🟢 What happens to messages per second and MB/s as message size grows?

- A) Both rise
- B) Messages/s falls, MB/s rises
- C) Both fall
- D) Neither changes

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#message-size)

</details>

## 3. 🟢 Why does publishing without confirms look faster?

- A) The broker processes such messages faster
- B) The client only writes to a socket and does not wait for the broker
- C) Messages are compressed
- D) Confirms disable the disk

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#publishing)

</details>

## 4. 🟢 What do you do with very large data?

- A) Put it in the message
- B) Put it in storage and pass a reference in the message
- C) Split it into 100,000 messages of 1 byte
- D) Enable prefetch 1

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#message-size)

</details>

## 5. 🟡 What does `basic_ack(tag, multiple=True)` do?

- A) Acknowledges all messages up to the tag in one frame
- B) Acknowledges only this message
- C) Rejects all messages
- D) Raises prefetch

<details><summary>Answer</summary>

**A.**
→ [Advanced theory](advanced.en.md#multiple-acks)

</details>

## 6. 🟡 Which queue type was the slowest in the confirmed-publish test?

- A) classic
- B) quorum
- C) stream
- D) All the same

<details><summary>Answer</summary>

**B.** 23k against 31.6k (classic) and 36.9k (stream) — the write is replicated and confirmed by a majority.
→ [Advanced theory](advanced.en.md#queue-type-and-speed)

</details>

## 7. 🟡 Which `Connection.Blocked` reason does a client get when disk space runs low?

- A) `low on memory`
- B) `low on disk`
- C) `flow`
- D) `blocked by policy`

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#the-disk-alarm)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) A multiple ack needs an idempotent handler
- [ ] B) The disk alarm fires when free space is below `disk_free_limit`
- [ ] C) The speed without confirms is the broker's speed
- [ ] D) Persistent is always several times slower than transient

<details><summary>Answer</summary>

**A, B.** C is the client's write speed; D is wrong in the test.
→ [Gotchas](expert.en.md#gotchas)

</details>

## 9. 🟡 A disk limit of 900 GB with ≈990 GB free. What happens?

- A) An alarm
- B) No alarm
- C) The broker stops
- D) The channel closes

<details><summary>Answer</summary>

**B.** The alarm occurs when free space is below the limit.
→ [Gotcha 5](expert.en.md#5-the-disk-limit-must-be-above-the-free-space)

</details>

## 10. 🔴 Find the mistake

> "We measured 62k messages/s without confirms and report that the broker sustains 62k messages/s."

<details><summary>Answer</summary>

That is the client's write speed into a socket. With confirms it was ≈40k, and a measurement on one machine does not transfer to your system.
→ [Gotcha 1](expert.en.md#1-the-no-confirms-speed-is-the-clients)

</details>

## 11. 🔴 A consumer handles short uniform tasks, throughput is low, `prefetch_count=1`. What do you do?

- A) Nothing, it is normal
- B) Raise prefetch to tens–hundreds and ack in batches, ensuring idempotency
- C) Turn acknowledgements off
- D) Move to a stream

<details><summary>Answer</summary>

**B.** It gives an order of magnitude more speed; for long tasks prefetch stays small.
→ [Trade-offs](expert.en.md#trade-offs-and-alternatives)

</details>

## 12. 🔴 Open question

A service publishes 50,000 small events per second and consumers lag. How do you find the bottleneck and what do you change?

<details><summary>Key points of a good answer</summary>

- Measure: queue depth and rates, unacked, `consumers`, alarms, node load.
- Publisher: pipelined confirms, batching; consumer: prefetch, multiple acks.
- Message size, queue type, disk and network; several queues/consumers.
- Measure on the target hardware, change one parameter at a time.

</details>

→ [Advanced theory](advanced.en.md)
