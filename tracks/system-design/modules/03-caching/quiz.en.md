---
title: "Quiz: caching"
lang: en
track: system-design
kind: module
doc: quiz
slug: 03-caching
part: 1
tags: [system-design, caching, cache-aside, cache-invalidation, cache-stampede, lru-cache, hybrid-cache]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: verified
updated: 2026-11-26
---

# Quiz: caching

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 On a miss in cache-aside

- A) the cache reads the database itself
- B) the application reads the source and puts the value into the cache
- C) the request fails
- D) the write goes to the cache first

<details><summary>Answer</summary>

**B.** A is read-through, D is about write strategies. → [Basics](basics.en.md)

</details>

## 2. 🟢 10,000 requests per second, 95% hit ratio. Database load?

- A) 50
- B) 500
- C) 950
- D) 9500

<details><summary>Answer</summary>

**B.** 5% of 10,000. → [Basics](basics.en.md)

</details>

## 3. 🟢 What does the second call return?

```csharp
service.GetPrice(1);          // the store has no price yet
store.Write(1, 10m);          // bypasses the service
Console.WriteLine(service.GetPrice(1));
```

<details><summary>Answer</summary>

```text
0
```

The cache does not know about a write that bypassed the service; the value refreshes after the TTL. → [Basics](basics.en.md)

</details>

## 4. 🟢 What must the key include?

- A) Only the identifier
- B) Everything that affects the result
- C) The current time
- D) The server name

<details><summary>Answer</summary>

**B.** Otherwise a user gets someone else's variant of the response. C and D would destroy hits. → [Basics](basics.en.md)

</details>

## 5. 🟡 Select all remedies against a stampede

- [ ] A) Single-flight
- [ ] B) TTL jitter
- [ ] C) The same TTL for all keys
- [ ] D) Serving the stale value during a refresh

<details><summary>Answer</summary>

**A, B, D.** C synchronizes expirations and makes the problem worse. → [Advanced](advanced.en.md)

</details>

## 6. 🟡 What does `Cache-Control: no-cache` mean?

- A) Do not store
- B) May be stored, but revalidate before use
- C) Store only in a CDN
- D) Store forever

<details><summary>Answer</summary>

**B.** A is `no-store`. → [Advanced](advanced.en.md)

</details>

## 7. 🟡 LRU of capacity 2: set a, set b, get a, set c. What is evicted?

- A) a
- B) b
- C) c
- D) nothing

<details><summary>Answer</summary>

**B.** Accessing `a` made `b` the least recently used. → [Advanced](advanced.en.md)

</details>

## 8. 🟡 What is the risk of write-behind?

- A) Slow reads
- B) Loss of data not yet written to the source
- C) No way to set a TTL
- D) Double reads

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 9. 🔴 What does this code print?

```csharp
mc.Set("list", new List<int> { 1 });
mc.Get<List<int>>("list")!.Add(2);
Console.WriteLine(mc.Get<List<int>>("list")!.Count);
```

<details><summary>Answer</summary>

```text
2
```

`IMemoryCache` stores a reference. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

How can an old value remain in the cache if the key is deleted after every write?

<details><summary>Key points of a good answer</summary>

- A reader read v1 from the database before the write and stored it after the delete.
- Remedies: versions, leases, a delayed second delete.
- A TTL bounds the duration.

</details>
