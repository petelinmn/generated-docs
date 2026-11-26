---
title: "Quiz: replication and partitioning"
lang: en
track: system-design
kind: module
doc: quiz
slug: 06-replication-and-partitioning
part: 2
tags: [system-design, replication, partitioning, shard, quorum, replication-lag, version-vector]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Quiz: replication and partitioning

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is sharding primarily for?

- A) Copies in case of failure
- B) Splitting volume and writes
- C) Backups
- D) Encryption

<details><summary>Answer</summary>

**B.** A is replication. → [Basics](basics.en.md)

</details>

## 2. 🟢 What happens with asynchronous replication?

- A) A replica may lag
- B) Writes are impossible
- C) Replicas are always ahead
- D) Conflicts are inevitable

<details><summary>Answer</summary>

**A.** → [Basics](basics.en.md)

</details>

## 3. 🟢 The key grows with time, partitions are by range. Where do new writes go?

- A) Evenly
- B) To the last partition
- C) To the first one
- D) To all

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 4. 🟢 Which key is the worst?

- A) User identifier
- B) Order identifier
- C) Order status with three values
- D) Device identifier

<details><summary>Answer</summary>

**C.** Three values make three groups, the load cannot be split. → [Basics](basics.en.md)

</details>

## 5. 🟡 N = 5. Which settings give an overlap?

- [ ] A) W=3, R=3
- [ ] B) W=2, R=2
- [ ] C) W=5, R=1
- [ ] D) W=1, R=5

<details><summary>Answer</summary>

**A, C, D.** In B the sum 4 is not greater than 5. → [Advanced](advanced.en.md)

</details>

## 6. 🟡 N=3, W=2, R=2. How many unavailable replicas do both operations survive?

- A) 0
- B) 1
- C) 2
- D) 3

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 How do you split a write-hot key?

- A) Add a salt and read all buckets
- B) Increase the TTL
- C) Enable synchronous replication
- D) Rename the key

<details><summary>Answer</summary>

**A.** D leaves one key on one node. → [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does this code print?

```csharp
var leader = new Leader();
var follower = new Follower(leader);
leader.Write("name", "old");
follower.Replicate();
leader.Write("name", "new");
Console.WriteLine(follower.Read("name"));
```

<details><summary>Answer</summary>

```text
old
```

The second write has not been applied by the replica yet. → [Advanced](advanced.en.md)

</details>

## 9. 🔴 What does this code print?

```csharp
var r = new QuorumRegister(3);
r.Write(1, "v1", 0, 1, 2);
r.Write(2, "v2", 0);
Console.WriteLine($"{r.Read(0, 1).Value} {r.Read(1, 2).Value}");
```

<details><summary>Answer</summary>

```text
v2 v1
```

The write did not reach W=2 and was not rolled back. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

How would you pick a shard key for a messenger?

<details><summary>Key points of a good answer</summary>

- Frequent queries: chat history, a user's chat list.
- A key by chat keeps history together; a large chat may become hot.
- The chat list needs a second, denormalized structure.
- A rebalancing plan.

</details>
