---
title: "Quiz: multi-region and recovery"
lang: en
track: system-design
kind: module
doc: quiz
slug: 16-multi-region-and-dr
part: 3
tags: [system-design, disaster-recovery, multi-region, failover, rpo-rto, backups, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Quiz: multi-region and recovery

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does RPO measure?

- A) Recovery time
- B) Acceptable data loss in time
- C) The number of regions
- D) Network latency

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 2. 🟢 The cheapest strategy?

- A) Backup and restore
- B) Pilot light
- C) Warm standby
- D) Active-active

<details><summary>Answer</summary>

**A.** → [Basics](basics.en.md)

</details>

## 3. 🟢 A developer dropped a table by accident. What helps?

- A) A replica
- B) Point-in-time recovery
- C) A load balancer
- D) A cache

<details><summary>Answer</summary>

**B.** The replica has already deleted the same thing. → [Basics](basics.en.md)

</details>

## 4. 🟢 What already runs in pilot light?

- A) Everything at full size
- B) Data replication and core infrastructure
- C) Nothing, not even backups
- D) Only the load balancer

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 5. 🟡 Why return after a longer streak of successes?

- A) To avoid switching back and forth
- B) To save memory
- C) For encryption
- D) For the cache

<details><summary>Answer</summary>

**A.** → [Advanced](advanced.en.md)

</details>

## 6. 🟡 Select ways to reduce conflicts in active-active

- [ ] A) A user's home region
- [ ] B) One region for writes
- [ ] C) Suitable CRDTs
- [ ] D) Trusting server clocks

<details><summary>Answer</summary>

**A, B, C.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 What does this code print?

```csharp
var c = new FailoverController("eu", "us", failAfter: 3, recoverAfter: 5);
c.Probe(false); c.Probe(false); c.Probe(true); c.Probe(false);
Console.WriteLine(c.Active);
```

<details><summary>Answer</summary>

```text
eu
```

The success reset the failure streak. → [Advanced](advanced.en.md)

</details>

## 8. 🟡 What makes up the downtime?

- A) Only copying
- B) Detection, decision, promotion, redirection
- C) Only DNS
- D) Replication lag

<details><summary>Answer</summary>

**B.** Lag affects data loss. → [Basics](basics.en.md)

</details>

## 9. 🔴 What does this code print?

```csharp
var t = new DateTime(2026, 3, 29, 12, 0, 0, DateTimeKind.Unspecified);
Console.WriteLine(JsonSerializer.Serialize(t));
```

<details><summary>Answer</summary>

```text
"2026-03-29T12:00:00"
```

There is no offset, the value is ambiguous. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

Draw up a DR plan for an online shop.

<details><summary>Key points of a good answer</summary>

- RPO and RTO per function: orders and the catalog differ.
- Strategy, backups, isolation.
- Who switches and how, failback.
- Drills and measurements.

</details>
