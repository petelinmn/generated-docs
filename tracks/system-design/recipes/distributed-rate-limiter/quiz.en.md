---
title: "Quiz: distributed rate limiter"
lang: en
track: system-design
kind: recipe
doc: quiz
slug: distributed-rate-limiter
part: 2
tags: [rate-limiting, redis, token-bucket, lua, distributed-systems]
versions: ".NET 11, C# 15; StackExchange.Redis 3.4.35; Redis 8"
status: draft
updated: 2026-11-27
---

# Quiz: distributed rate limiter

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Three instances each allow 100 per minute. A client can get

- A) 100
- B) 300
- C) 33
- D) unlimited

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 2. 🟢 What does the capacity of a token bucket limit?

- A) The long-term rate
- B) The size of a burst
- C) The number of clients
- D) The timeout

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 3. 🟢 Which status means too many requests?

- A) 403
- B) 429
- C) 503
- D) 409

<details><summary>Answer</summary>

**B.**

→ [Basics](basics.en.md)

</details>

## 4. 🟡 Why does the script use `TIME`?

- A) It is faster
- B) One clock for all instances
- C) Lua has no numbers
- D) To expire keys

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 5. 🟡 What does `return 3.9` from a script give the client?

- A) 3.9
- B) 3
- C) 4
- D) an error

<details><summary>Answer</summary>

**B.**

→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 Select all true statements about scripts

- [ ] A) They run atomically
- [ ] B) The server is blocked while one runs
- [ ] C) Keys should be passed in `KEYS`
- [ ] D) They run in parallel with other commands

<details><summary>Answer</summary>

**A, B, C.**

→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Find the problem

```csharp
var left = (int)await redis.StringGetAsync(key);
if (left > 0) { await redis.StringSetAsync(key, left - 1); return true; }
return false;
```

<details><summary>Answer</summary>

Two instances can read the same value; do it in one atomic step.

→ [Advanced](advanced.en.md)

</details>

## 8. 🔴 The check timed out. The token was

- A) surely not taken
- B) possibly taken
- C) surely taken
- D) returned automatically

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 9. 🔴 What does a lease of a batch risk?

- A) Data corruption
- B) Refusals while unused tokens sit elsewhere
- C) Deadlock in Redis
- D) Higher latency

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 10. 🔴 For login attempts when Redis is down, the safer policy is usually

- A) fail open
- B) fail closed or a strict local limit
- C) no limit
- D) retry forever

<details><summary>Answer</summary>

**B.**

→ [Expert](expert.en.md)

</details>

## 11. 🔴 Open question: design limits for a public API with free and paid plans.

<details><summary>Key points</summary>

- Key by account, not by address; plan defines capacity and rate.
- Several limits (minute, day) checked together.
- 429 with `Retry-After`; show remaining quota.
- Failure policy per endpoint; monitoring of refusals.

→ [Expert](expert.en.md)

</details>
