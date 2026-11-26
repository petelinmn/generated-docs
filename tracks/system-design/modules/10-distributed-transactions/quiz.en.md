---
title: "Quiz: distributed transactions"
lang: en
track: system-design
kind: module
doc: quiz
slug: 10-distributed-transactions
part: 2
tags: [system-design, distributed-transactions, two-phase-commit, saga, outbox, inbox, idempotency]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-26
---

# Quiz: distributed transactions

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 When does 2PC commit?

- A) If the majority is in favour
- B) If everyone is in favour
- C) If the coordinator is in favour
- D) Always

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 2. 🟢 What does a saga do when a step fails?

- A) A global database rollback
- B) Compensates the completed steps
- C) Deletes services
- D) Nothing

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 3. 🟢 What is saved with the order in an outbox?

- A) The event to publish
- B) A database copy
- C) The broker's reply
- D) An error log

<details><summary>Answer</summary>

**A.** → [Basics](basics.en.md)

</details>

## 4. 🟢 Why is "write to the database, then send" dangerous?

- A) It is slow
- B) A failure between the steps loses the event
- C) No indexing
- D) It breaks column order

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 5. 🟡 In which order do compensations run?

- A) Forward
- B) Reverse
- C) Random
- D) Always in parallel

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 6. 🟡 Select countermeasures against saga anomalies

- [ ] A) A semantic lock
- [ ] B) Commutative updates
- [ ] C) Rereading before writing
- [ ] D) Disabling compensations

<details><summary>Answer</summary>

**A, B, C.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 What does this code print?

```csharp
Participant[] all = [new(), new()];
TwoPhaseCommit.Run(all, crashAfterPrepare: true);
Console.WriteLine(string.Join(" ", all.Select(p => p.State)));
```

<details><summary>Answer</summary>

```text
Prepared Prepared
```

The participants are in doubt. → [Advanced](advanced.en.md)

</details>

## 8. 🟡 The relay crashed after publishing, before marking. What happens?

- A) Loss
- B) Publishing again
- C) The order is rolled back
- D) Nothing

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 9. 🔴 The compensation of the second step threw. Will the first step be compensated in the example?

- A) Yes
- B) No
- C) Twice
- D) Later, automatically

<details><summary>Answer</summary>

**B.** The exception leaves the rollback loop. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

Design order placement: stock reservation, payment, delivery.

<details><summary>Key points of a good answer</summary>

- Steps, compensations, pivot.
- Idempotency and an outbox.
- The order status as a semantic lock.
- Timeouts and manual handling.

</details>
