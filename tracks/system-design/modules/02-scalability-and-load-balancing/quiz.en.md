---
title: "Quiz: scaling and load balancing"
lang: en
track: system-design
kind: module
doc: quiz
slug: 02-scalability-and-load-balancing
part: 1
tags: [system-design, scaling, scale-out, load-balancing, consistent-hashing, yarp]
versions: ".NET 11, C# 15; Yarp.ReverseProxy 2.3.0"
status: draft
updated: 2026-11-26
---

# Quiz: scaling and load balancing

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What prevents a service from scaling horizontally?

- A) Using HTTP
- B) Sessions in process memory
- C) Asynchronous handlers
- D) An external database

<details><summary>Answer</summary>

**B.** The next request may reach another instance. D, on the contrary, helps move state out. → [Basics](basics.en.md)

</details>

## 2. 🟢 What does an L7 balancer see that L4 does not?

- A) The IP address
- B) The port
- C) The HTTP request path
- D) The TCP protocol

<details><summary>Answer</summary>

**C.** L4 does not parse the contents of a connection. → [Basics](basics.en.md)

</details>

## 3. 🟢 What does this code print?

```csharp
var rr = new RoundRobin(["a", "b", "c"]);
Console.WriteLine(string.Concat(Enumerable.Range(0, 5).Select(_ => rr.Pick())));
```

<details><summary>Answer</summary>

```text
abcab
```

→ [Basics](basics.en.md)

</details>

## 4. 🟢 What does a cluster describe in YARP?

- A) A path matching rule
- B) A set of destinations and the policy for choosing among them
- C) Kubernetes pods
- D) A list of clients

<details><summary>Answer</summary>

**B.** Path matching is a route. → [Basics](basics.en.md)

</details>

## 5. 🟡 Select all correct statements about consistent hashing

- [ ] A) When a node is added, keys move only to the new node
- [ ] B) It removes the hot key problem
- [ ] C) Virtual points even out the shares
- [ ] D) The hash must be the same in every process

<details><summary>Answer</summary>

**A, C, D.** B is wrong: one key still lives on one node. → [Advanced](advanced.en.md)

</details>

## 6. 🟡 Which YARP policy applies if none is specified?

- A) `RoundRobin`
- B) `Random`
- C) `PowerOfTwoChoices`
- D) `LeastRequests`

<details><summary>Answer</summary>

**C.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 By default, health checks in YARP are

- A) both enabled
- B) only passive enabled
- C) both disabled
- D) only active enabled

<details><summary>Answer</summary>

**C.** They have to be enabled explicitly. → [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does this code print?

```csharp
var wrr = new SmoothWeightedRoundRobin(("a", 5), ("b", 1), ("c", 1));
Console.WriteLine(string.Concat(Enumerable.Range(0, 7).Select(_ => wrr.Pick())));
```

<details><summary>Answer</summary>

```text
aabacaa
```

The picks of `b` and `c` are interleaved with the picks of `a` instead of being left to the end. → [Advanced](advanced.en.md)

</details>

## 9. 🔴 Find the bug

```csharp
int i = Interlocked.Increment(ref _next);
return nodes[i % nodes.Count];
```

<details><summary>Answer</summary>

After overflow `i` is negative, so is the remainder, and you get an exception. Cast to `uint`: `unchecked((uint)i) % (uint)nodes.Count`. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

After going from 31 to 100 nodes, throughput dropped. Why does that happen?

<details><summary>Key points of a good answer</summary>

- USL: coordination costs grow as `n(n-1)`.
- Shared resources: database locks, connection count.
- Verify by measuring, shard or remove the shared point.

</details>
