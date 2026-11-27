---
title: "Quiz: uRL shortener"
lang: en
track: system-design
kind: recipe
doc: quiz
slug: url-shortener
part: 2
tags: [url-shortener, caching, redirect, rate-limiting, minimal-api]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-27
---

# Quiz: uRL shortener

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which status does `Results.Redirect(url)` send by default?

- A) 301
- B) 302
- C) 307
- D) 200

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 Why are random codes better than 1, 2, 3 for unlisted links?

- A) They are shorter
- B) They cannot be walked in order
- C) They never collide
- D) They need no storage

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 What resolves two requests that got the same code?

- A) A check before the insert
- B) An atomic insert with a unique key
- C) A longer timeout
- D) A cache

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟡 Select all addresses the recipe rejects

- [ ] A) `ftp://example.com`
- [ ] B) `https://a@evil.example/`
- [ ] C) `https://example.com/a?b=1`
- [ ] D) An address on its own host

<details><summary>Answer</summary>

**A, B, D.**

→ [Advanced](advanced.en.md)

</details>

## 5. 🟡 Why does creation remove the cache key?

- A) To free memory
- B) The code may be cached as missing
- C) To count clicks
- D) SQLite requires it

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 What happens when the click queue is full?

- A) The redirect waits
- B) The new click is dropped and counted
- C) The service crashes
- D) The oldest code is disabled

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Find the bug

```csharp
var link = await cache.GetOrCreateAsync(code, async _ => await store.FindAsync(code));
return link?.Url;
```

<details><summary>Answer</summary>

A cached link is returned after it has expired or been disabled elsewhere; check `IsLive` after the cache.

→ [Advanced](advanced.en.md)

</details>

## 8. 🔴 Instance A disables a link. Instance B has it in its local cache. B answers

- A) 404 at once
- B) a redirect until its local entry expires
- C) 500
- D) it asks A

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 9. 🔴 Three instances, 30 creations per minute each. A client can create up to

- A) 30
- B) 90
- C) 10
- D) unlimited

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 10. 🔴 Which code is permanent and keeps the method?

- A) 301
- B) 302
- C) 307
- D) 308

<details><summary>Answer</summary>

**D.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Open question: redirects grow to 50 000 per second. What changes?

<details><summary>Key points</summary>

- Measure the hit ratio; add instances and a distributed cache level.
- Partition or replicate the store.
- Move click events to a log.
- Serve close to users; protect the store from misses.

→ [Expert](expert.en.md)

</details>
