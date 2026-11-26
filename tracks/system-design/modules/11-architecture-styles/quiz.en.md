---
title: "Quiz: architecture styles"
lang: en
track: system-design
kind: module
doc: quiz
slug: 11-architecture-styles
part: 2
tags: [system-design, architecture, modular-monolith, microservices, fitness-function, adr]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Quiz: architecture styles

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is a monolith?

- A) Bad code
- B) One deployment unit
- C) One table
- D) One developer

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 2. 🟢 What does Web-Queue-Worker consist of?

- A) A web front end, a queue, a background worker
- B) Three databases
- C) Only functions
- D) Two load balancers

<details><summary>Answer</summary>

**A.** → [Basics](basics.en.md)

</details>

## 3. 🟢 What most often breaks service independence?

- A) HTTP
- B) A shared database
- C) Containers
- D) Logs

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 4. 🟢 How do the modules of a modular monolith talk?

- A) Through internal classes
- B) Through contracts
- C) Through global variables
- D) They do not

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 5. 🟡 `Ca = 4`, `Ce = 1`. Instability?

- A) 0.2
- B) 0.25
- C) 0.8
- D) 4

<details><summary>Answer</summary>

**A.** → [Advanced](advanced.en.md)

</details>

## 6. 🟡 What is a strangler fig?

- A) Rewriting everything at once
- B) Gradually moving paths to a new implementation
- C) Deleting the old database
- D) Freezing changes

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select valid reasons to extract a service

- [ ] A) Independent releases
- [ ] B) A different load
- [ ] C) Failure isolation
- [ ] D) "Everyone does it"

<details><summary>Answer</summary>

**A, B, C.** → [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does this code print?

```csharp
var (latency, availability) = NetworkCost.Chain(5, TimeSpan.FromMilliseconds(20), 0.999);
Console.WriteLine(latency.TotalMilliseconds);
```

<details><summary>Answer</summary>

```text
100
```

Sequential latencies add up. → [Advanced](advanced.en.md)

</details>

## 9. 🔴 What does this code print?

```csharp
Action handlers = () => Console.WriteLine("first");
handlers += () => throw new InvalidOperationException();
handlers += () => Console.WriteLine("third");
try { handlers(); } catch (InvalidOperationException) { Console.WriteLine("caught"); }
```

<details><summary>Answer</summary>

```text
first
caught
```

→ [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

A team of six is building a new product. Which style do you choose?

<details><summary>Key points of a good answer</summary>

- A modular monolith as the starting point.
- Explicit contracts and boundary tests.
- Conditions for extracting a service.
- An ADR with alternatives.

</details>
