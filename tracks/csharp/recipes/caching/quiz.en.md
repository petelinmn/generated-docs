---
title: "Quiz: Caching"
lang: en
track: csharp
kind: recipe
doc: quiz
slug: caching
part: 2
tags: [caching, memory-cache, hybrid-cache, cache-stampede, output-cache]
status: draft
updated: 2026-11-22
---

# Quiz: Caching

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [minimal](basics.en.md) · 🟡 [production](advanced.en.md) · 🔴 [scale and edges](expert.en.md).

## 1. 🟢 How many times does the load run on two consecutive `GetOrCreateAsync` calls for one key?

- A) twice
- B) once
- C) never
- D) depends on the value size

<details><summary>Answer</summary>

**B.** Verified: `v1 v1`, one load; after expiry — a new one.
→ [Minimal](basics.en.md#imemorycache)

</details>

## 2. 🟢 What does `CacheOutput` do for an endpoint?

- A) caches DB results
- B) caches the ready HTTP response
- C) enables compression
- D) nothing without Redis

<details><summary>Answer</summary>

**B.** Verified: two requests — `call 1` both times (the handler ran once).
→ [Minimal](basics.en.md#output-caching)

</details>

## 3. 🟢 What must a cache entry always have?

- A) compression
- B) a lifetime and a meaningful key
- C) a tag
- D) a signature

<details><summary>Answer</summary>

**B.** Without a lifetime data goes stale forever; the key must include all inputs (filters, user).
→ [Minimal](basics.en.md#beginner-mistakes)

</details>

## 4. 🟢 Which tool gives an in-memory cache + an optional shared L2?

- A) `IMemoryCache`
- B) `HybridCache`
- C) output caching
- D) `ConcurrentDictionary`

<details><summary>Answer</summary>

**B.** `HybridCache` — L1 in memory and L2 through `IDistributedCache`.
→ [Minimal](basics.en.md#three-tools)

</details>

## 5. 🟡 How many loads do 10 simultaneous requests for an empty key run in `IMemoryCache` and `HybridCache`?

- A) 1 and 1
- B) 10 and 1
- C) 1 and 10
- D) 10 and 10

<details><summary>Answer</summary>

**B.** Verified: `IMemoryCache` — 10 loads, `HybridCache` — 1 (stampede protection).
→ [Production](advanced.en.md#cache-stampede)

</details>

## 6. 🟡 How do you reset a group of `HybridCache` entries?

- A) delete them one by one in a loop
- B) tag them and call `RemoveByTagAsync`
- C) restart the application
- D) you cannot

<details><summary>Answer</summary>

**B.** Verified: both entries with the `products` tag reloaded.
→ [Production](advanced.en.md#tags-and-invalidation)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) `AddDistributedMemoryCache()` gives `HybridCache` a shared L2 between instances
- [ ] B) `RemoveAsync` on one instance does not reset other instances' L1
- [ ] C) with a `SizeLimit` every entry must specify `Size`
- [ ] D) `HybridCache` limits a stampede within a process

<details><summary>Answer</summary>

**B, C, D.** A is wrong: `HybridCache` does not use `MemoryDistributedCache` as L2 (verified: 2 loads in two instances).
→ [Production](advanced.en.md#a-two-level-cache-l1--l2)

</details>

## 8. 🟡 Open question: how would you cache a product catalog that changes once an hour in a service with 4 instances?

<details><summary>Key points of a good answer</summary>

- `HybridCache` with a shared L2 (Redis) and a short L1;
- a lifetime of about an hour or less + tag invalidation on change;
- warm-up in a background job, hit metrics;
- acknowledging that other instances see a change only after L1 expires.

</details>

## 9. 🔴 After `RemoveAsync` on instance A, instance B serves the old value. Why?

- A) a cache bug
- B) instance B's L1 is not invalidated; it serves the entry until `LocalCacheExpiration` elapses
- C) L2 does not work
- D) no tag is specified

<details><summary>Answer</summary>

**B.** Verified: after `RemoveAsync` on the first instance the second returned the old value from its L1 without a new load.
→ [Scale and edges](expert.en.md#edges-and-gotchas)

</details>

## 10. 🔴 What is the risk of negative caching (`null`)?

- A) `null` cannot be cached
- B) an absent entry "sticks" until it expires although it has already appeared in the source
- C) memory is not freed
- D) it does not work in Redis

<details><summary>Answer</summary>

**B.** Verified: `null` is cached (the key exists with the value `null`) — useful against extra calls but delays the appearance of a new entry.
→ [Scale and edges](expert.en.md#edges-and-gotchas)

</details>
