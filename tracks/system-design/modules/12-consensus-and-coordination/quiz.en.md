---
title: "Quiz: consensus and coordination"
lang: en
track: system-design
kind: module
doc: quiz
slug: 12-consensus-and-coordination
part: 3
tags: [system-design, consensus, raft, leader-election, distributed-lock, fencing-token, logical-clocks]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Quiz: consensus and coordination

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 The majority in a cluster of 5?

- A) 2
- B) 3
- C) 4
- D) 5

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 2. 🟢 The states of a Raft node?

- A) master, slave
- B) follower, candidate, leader
- C) open, closed
- D) reader, writer

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 3. 🟢 Why does a lease expire?

- A) So a crashed holder does not keep the resource forever
- B) For encryption
- C) For speed
- D) For ordering

<details><summary>Answer</summary>

**A.** → [Basics](basics.en.md)

</details>

## 4. 🟢 The storage has seen token 34. Token 33 arrives. What happens?

- A) Accepted
- B) Rejected
- C) The token becomes 33
- D) The lock is extended

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 5. 🟡 Which log is more up to date?

- A) The one with the larger term of the last entry
- B) Always the longer one
- C) That of whoever started first
- D) A random one

<details><summary>Answer</summary>

**A.** Length decides only with equal terms. → [Advanced](advanced.en.md)

</details>

## 6. 🟡 How many votes per term can a node give?

- A) One
- B) Two
- C) Any number
- D) None

<details><summary>Answer</summary>

**A.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 What does this code print?

```csharp
var voter = new Node(1);
Console.WriteLine(voter.RequestVote(1, 2, 0, 0));
Console.WriteLine(voter.RequestVote(1, 3, 0, 0));
```

<details><summary>Answer</summary>

```text
True
False
```

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does a leader do on seeing a larger term?

- A) Ignores it
- B) Becomes a follower
- C) Increases its own by two
- D) Shuts down

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 9. 🔴 Lamport mark A = 1, B = 5. What is known?

- A) A is definitely earlier than B
- B) B is definitely not a cause of A
- C) The events are concurrent
- D) Nothing at all

<details><summary>Answer</summary>

**B.** If B were a cause of A, B's mark would be smaller. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

How do you guarantee that only one service instance builds a report?

<details><summary>Key points of a good answer</summary>

- A lease with renewal.
- Fencing or a unique key of the result.
- Idempotency of the job.
- Behavior on a pause and loss of connectivity.

</details>
