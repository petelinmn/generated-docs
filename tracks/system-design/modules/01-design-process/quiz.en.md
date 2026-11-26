---
title: "Quiz: design process and estimation"
lang: en
track: system-design
kind: module
doc: quiz
slug: 01-design-process
part: 1
tags: [system-design, capacity-estimation, slo, availability, latency, percentiles]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-26
---

# Quiz: design process and estimation

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which of these is a non-functional requirement?

- A) A user can send a message
- B) p99 of sending is at most 300 ms
- C) You can delete your account
- D) The history is searchable

<details><summary>Answer</summary>

**B.** It says "how well". A, C and D describe what the system does. → [Basics](basics.en.md)

</details>

## 2. 🟢 8.64 million requests a day is on average

- A) 10 per second
- B) 100 per second
- C) 1000 per second
- D) 8640 per second

<details><summary>Answer</summary>

**B.** 8,640,000 / 86,400 = 100. → [Basics](basics.en.md)

</details>

## 3. 🟢 How much downtime per 30 days does a 99.9% SLO allow?

- A) 4.32 min
- B) 43.2 min
- C) 432 min
- D) 8.76 h

<details><summary>Answer</summary>

**B.** 43,200 minutes × 0.001. A matches 99.99%, C matches 99%, D is the yearly downtime at 99.9%. → [Basics](basics.en.md)

</details>

## 4. 🟢 What does this code print?

```csharp
Console.WriteLine(2000 * TimeSpan.FromMilliseconds(200).TotalSeconds);
```

<details><summary>Answer</summary>

```text
400
```

Little's law: that many requests are in the system at once. → [Basics](basics.en.md)

</details>

## 5. 🟡 Select all correct statements

- [ ] A) The average can be much larger than the median because of rare slow requests
- [ ] B) The cluster-wide p99 is the average of the nodes' p99
- [ ] C) Fan-out increases the share of slow responses
- [ ] D) The short window of a burn-rate alert helps to clear it sooner

<details><summary>Answer</summary>

**A, C, D.** B is wrong: percentiles cannot be averaged. → [Advanced](advanced.en.md)

</details>

## 6. 🟡 SLO 99.9%, 100 of 10,000 requests failed. Burn rate?

- A) 0.1
- B) 1
- C) 10
- D) 100

<details><summary>Answer</summary>

**C.** The error ratio is 1%, the allowed one is 0.1%. → [Advanced](advanced.en.md)

</details>

## 7. 🟡 Find the mistake

"We have two 99% replicas, both using one database with 99% availability. So the system is 99.99% available."

<details><summary>Answer</summary>

The database is a required shared dependency. With the application and the database in series: `0.9999 × 0.99` ≈ 98.99%. → [Basics](basics.en.md)

</details>

## 8. 🟡 What goes into an ADR?

- A) Only the decision taken
- B) Context, decision, alternatives and consequences
- C) The sprint task list
- D) An incident log

<details><summary>Answer</summary>

**B.** Without the context and alternatives you cannot tell whether it is time to revisit the decision. → [Advanced](advanced.en.md)

</details>

## 9. 🔴 What does this code print?

```csharp
int users = 10_000_000, perUser = 20, bytes = 500;
Console.WriteLine(users * perUser * bytes);
```

<details><summary>Answer</summary>

```text
1215752192
```

`int` overflow; the correct value is 100,000,000,000. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

A customer asks for 99.999% availability. How do you respond?

<details><summary>Key points of a good answer</summary>

- That is 5.26 minutes of downtime a year: manual recovery does not fit.
- You need redundancy without common causes of failure and automatic failover.
- Availability cannot exceed that of the required dependencies.
- Name the price and suggest different SLOs for different operations.

</details>
