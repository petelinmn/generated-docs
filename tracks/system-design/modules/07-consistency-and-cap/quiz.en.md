---
title: "Quiz: consistency and CAP"
lang: en
track: system-design
kind: module
doc: quiz
slug: 07-consistency-and-cap
part: 2
tags: [system-design, consistency, cap-theorem, isolation-levels, mvcc, write-skew, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Quiz: consistency and CAP

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 When do you have to choose between C and A under CAP?

- A) Always
- B) During a network partition
- C) Under high load
- D) During a schema change

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 2. 🟢 What is a dirty read?

- A) Reading uncommitted data
- B) Reading from a replica
- C) A repeated read
- D) Reading without an index

<details><summary>Answer</summary>

**A.** → [Basics](basics.en.md)

</details>

## 3. 🟢 What does eventual consistency promise?

- A) Fresh data within a second
- B) Copies converge if writes stop
- C) Strict ordering
- D) No conflicts

<details><summary>Answer</summary>

**B.** There is no deadline. → [Basics](basics.en.md)

</details>

## 4. 🟢 A node in the minority in CP mode

- A) accepts writes
- B) refuses
- C) deletes data
- D) becomes the leader

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 5. 🟡 The default level in PostgreSQL?

- A) Read uncommitted
- B) Read committed
- C) Repeatable read
- D) Serializable

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 6. 🟡 Which anomalies does Serializable prevent?

- [ ] A) Dirty read
- [ ] B) Phantom read
- [ ] C) Write skew
- [ ] D) Network failure

<details><summary>Answer</summary>

**A, B, C.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 What does this code print?

```csharp
var session = new Session();
Console.WriteLine(session.Read(new Replica(2, "new")));
Console.WriteLine(session.Read(new Replica(1, "old")) is null);
```

<details><summary>Answer</summary>

```text
new
True
```

→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does E stand for in PACELC?

- A) Error
- B) "Else": when there is no partition
- C) Eventual
- D) Elasticity

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 9. 🔴 What ends up in `d["x"]`?

```csharp
var d = new ConcurrentDictionary<string, int> { ["x"] = 0 };
// two threads: read d["x"], wait for each other, write value + 1
```

<details><summary>Answer</summary>

```text
1
```

A lost update: the pair of operations is not atomic. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

How do you guarantee the last ticket is not sold twice?

<details><summary>Key points of a good answer</summary>

- An atomic conditional update or a database constraint.
- Or serializable with retries.
- Do not rely on replicas for the decision.
- Idempotency of the client request.

</details>
