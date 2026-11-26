---
title: "Quiz: event sourcing and CQRS"
lang: en
track: system-design
kind: module
doc: quiz
slug: 13-event-sourcing-and-cqrs
part: 3
tags: [system-design, event-sourcing, cqrs, events, projection, snapshot, upcasting]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Quiz: event sourcing and CQRS

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is the source of truth?

- A) The read model
- B) The event stream
- C) The snapshot
- D) The cache

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 2. 🟢 How should an event be named?

- A) `Withdraw`
- B) `Withdrawn`
- C) `DoWithdraw`
- D) `UpdateRow`

<details><summary>Answer</summary>

**B.** Past tense. → [Basics](basics.en.md)

</details>

## 3. 🟢 What does CQRS do?

- A) Encrypts events
- B) Separates write and read models
- C) Merges databases
- D) Deletes history

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 4. 🟢 What does this code print?

```csharp
Console.WriteLine(Account.Replay([new Deposited(100), new Withdrawn(30)]).Balance);
```

<details><summary>Answer</summary>

```text
70
```

→ [Basics](basics.en.md)

</details>

## 5. 🟡 Why a checkpoint?

- A) To resume from a place and skip duplicates
- B) For encryption
- C) For an aggregate snapshot
- D) To delete events

<details><summary>Answer</summary>

**A.** → [Advanced](advanced.en.md)

</details>

## 6. 🟡 What is upcasting?

- A) Rewriting the store
- B) Converting an old shape on read
- C) A type cast in C#
- D) Raising the stream version

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select true statements about snapshots

- [ ] A) They can be regenerated from events
- [ ] B) Old events must be deleted after a snapshot
- [ ] C) They speed up loading
- [ ] D) They replace the stream as the source of truth

<details><summary>Answer</summary>

**A, C.** → [Advanced](advanced.en.md)

</details>

## 8. 🟡 What is crypto-shredding?

- A) Deleting the key without which data cannot be read
- B) Compressing the log
- C) Hashing events
- D) Splitting a stream

<details><summary>Answer</summary>

**A.** → [Advanced](advanced.en.md)

</details>

## 9. 🔴 What does this code print?

```csharp
Base e = new Dep(5);
Console.WriteLine(JsonSerializer.Serialize(e));
```

<details><summary>Answer</summary>

```text
{}
```

The declared type `Base` is serialized. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

When would you decline event sourcing?

<details><summary>Key points of a good answer</summary>

- Plain CRUD where history has no value.
- Frequent invariants across aggregates.
- No readiness for versions and eventual consistency.
- Auditing is enough.

</details>
