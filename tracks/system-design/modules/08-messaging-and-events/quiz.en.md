---
title: "Quiz: messaging and events"
lang: en
track: system-design
kind: module
doc: quiz
slug: 08-messaging-and-events
part: 2
tags: [system-design, message-broker, events, delivery-semantics, idempotency, backpressure, poison-message]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Quiz: messaging and events

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which of these is an event?

- A) `SendInvoice`
- B) `InvoiceSent`
- C) `CreateOrder`
- D) `DeleteUser`

<details><summary>Answer</summary>

**B.** A fact in the past tense. → [Basics](basics.en.md)

</details>

## 2. 🟢 What happens to a read event in a log?

- A) It is removed
- B) It stays until retention expires
- C) It moves to the dead letter queue
- D) It is locked

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 3. 🟢 What is an ack for?

- A) Encryption
- B) Saying the message has been processed
- C) Raising the priority
- D) Compressing data

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 4. 🟢 The main benefit of asynchrony?

- A) An instant result
- B) The receiver may be temporarily unavailable
- C) No duplicates
- D) Strict ordering

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 5. 🟡 Acking before processing gives

- A) at-most-once
- B) at-least-once
- C) exactly-once
- D) ordering

<details><summary>Answer</summary>

**A.** → [Advanced](advanced.en.md)

</details>

## 6. 🟡 Which `FullMode` is the default?

- A) `DropOldest`
- B) `Wait`
- C) `DropWrite`
- D) `DropNewest`

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 What does this code print?

```csharp
var (accepted, buffered) = Backpressure.Overflow(BoundedChannelFullMode.DropOldest, 2, 5);
Console.WriteLine($"{accepted}: {string.Join(" ", buffered)}");
```

<details><summary>Answer</summary>

```text
5: 4 5
```

All writes are "successful", the last two remain. → [Advanced](advanced.en.md)

</details>

## 8. 🟡 Find the mistake

"We save the order to the database, then publish the event to the broker."

<details><summary>Answer</summary>

A failure between the steps loses the event. An outbox is needed. → [Advanced](advanced.en.md)

</details>

## 9. 🔴 Why might this loop never end?

```csharp
await foreach (var x in channel.Reader.ReadAllAsync()) Handle(x);
```

<details><summary>Answer</summary>

The writer did not call `Complete()`: the reader waits for more items. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

Design payment message processing without double charges.

<details><summary>Key points of a good answer</summary>

- Ack after processing, a message identifier.
- Deduplication in one transaction with the effect.
- An idempotency key at the external provider.
- Dead letter queue, alerts and reconciliation.

</details>
