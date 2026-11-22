---
title: "Quiz: Rate limiting"
lang: en
track: csharp
kind: recipe
doc: quiz
slug: rate-limiting
part: 2
tags: [rate-limiting, throttling, http-429, token-bucket]
status: draft
updated: 2026-11-22
---

# Quiz: Rate limiting

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [minimal](basics.en.md) · 🟡 [production](advanced.en.md) · 🔴 [scale and edges](expert.en.md).

## 1. 🟢 Which response code means the limit was exceeded?

- A) 401
- B) 403
- C) 429
- D) 503

<details><summary>Answer</summary>

**C.** `429 Too Many Requests`; the code is set by `RejectionStatusCode` (in the example — `StatusCodes.Status429TooManyRequests`).
→ [Minimal](basics.en.md#the-task)

</details>

## 2. 🟢 How many of 5 simultaneous requests pass with `PermitLimit = 3` in a 2 s window?

- A) all 5
- B) 3, the rest get 429
- C) 1
- D) none

<details><summary>Answer</summary>

**B.** Verified: three `200`, two `429`; after the window ends a request passes again.
→ [Minimal](basics.en.md#the-minimal-solution)

</details>

## 3. 🟢 What must you do for a policy to actually apply to an endpoint?

- A) nothing: `AddRateLimiter` is enough
- B) call `app.UseRateLimiter()` and add `RequireRateLimiting("policy")`
- C) restart the server
- D) install a NuGet package

<details><summary>Answer</summary>

**B.** Without `UseRateLimiter()` the middleware does not run, and without `RequireRateLimiting` (or a global limiter) the endpoint is unlimited: `/free` let 10 of 10 through.
→ [Minimal](basics.en.md#the-minimal-solution)

</details>

## 4. 🟢 Which algorithm allows a burst but limits the average rate?

- A) fixed window
- B) token bucket
- C) concurrency
- D) there is none

<details><summary>Answer</summary>

**B.** Verified: capacity 2, +1 token/s — two requests at once, the third rejected, after 1.1 s `200` again.
→ [Minimal](basics.en.md#algorithms)

</details>

## 5. 🟡 How do you make the limit separate for each user?

- A) several policies by hand
- B) a policy with a `RateLimitPartition` by a key (user/API key/IP)
- C) you cannot
- D) through a cache

<details><summary>Answer</summary>

**B.** Verified: `ann` — `200 200 429`, `bob` — `200`.
→ [Production](advanced.en.md#a-limit-per-user)

</details>

## 6. 🟡 Which header tells the client when to retry?

- A) `Retry-After`
- B) `Expires`
- C) `Cache-Control`
- D) `X-Wait`

<details><summary>Answer</summary>

**A.** It is set in `OnRejected` from the lease metadata (`MetadataName.RetryAfter`); verified: `Retry-After=1` and `4`.
→ [Production](advanced.en.md#the-response-to-the-client-retry-after)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) a concurrency limiter limits the number of simultaneous requests
- [ ] B) a large `QueueLimit` is harmless
- [ ] C) health checks should be excluded from limits
- [ ] D) behind a proxy without `UseForwardedHeaders` all clients may share one IP

<details><summary>Answer</summary>

**A, C, D.** B is wrong: a queue holds connections and memory.
→ [Production](advanced.en.md#queues-and-choosing-an-algorithm)

</details>

## 8. 🟡 Open question: what limits would you set on `/login` and on reading a catalog?

<details><summary>Key points of a good answer</summary>

- `/login`: a strict limit by IP and login (a few attempts per minute), `Retry-After`, logging rejections;
- reading: a soft limit per user/key that allows bursts (token bucket);
- different policies for different risks, exceptions for health checks;
- metrics and alerts on the number of 429s.

</details>

## 9. 🔴 Why is the built-in limiter not enough with 4 instances of the application?

- A) it does not work with several instances
- B) the counters are in each instance's memory: a client may get up to 4 × the limit; you need a gateway or a shared store
- C) it works only in Development
- D) because of HTTPS

<details><summary>Answer</summary>

**B.** The limiter is local to the process. For an exact quota — a limit at the gateway or counters in Redis.
→ [Scale and edges](expert.en.md#scale-several-instances)

</details>

## 10. 🔴 A client makes 6 requests in a row with a limit of 3 per window and gets no rejections. What is possible?

- A) a client bug
- B) the fixed-window boundary (3 at the end and 3 at the start of the next) or several instances with independent counters
- C) the limit is off forever
- D) 429 does not exist

<details><summary>Answer</summary>

**B.** A fixed window allows up to 2N at the boundary; sliding window/token bucket smooth this.
→ [Scale and edges](expert.en.md#edges-and-gotchas)

</details>
