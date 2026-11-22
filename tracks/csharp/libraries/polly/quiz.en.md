---
title: "Quiz: Polly"
lang: en
track: csharp
kind: library
doc: quiz
slug: polly
part: 2
tags: [polly, resilience, retry, circuit-breaker]
status: draft
updated: 2026-11-22
---

# Quiz: Polly

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 How many attempts run with `MaxRetryAttempts = 3` and a constant failure?

- A) 3
- B) 4
- C) 1
- D) infinitely

<details><summary>Answer</summary>

**B.** Verified: 1 initial + 3 retries = 4, then the exception is propagated.
→ [Basics](basics.en.md#retry)

</details>

## 2. 🟢 Is an exception of a type not listed in `ShouldHandle` retried?

- A) yes
- B) no — one attempt and the exception
- C) yes, once
- D) depends on the delay

<details><summary>Answer</summary>

**B.** Verified: `ArgumentException` — 1 attempt.
→ [Basics](basics.en.md#retry)

</details>

## 3. 🟢 Will a timeout interrupt an operation with `Thread.Sleep(300)` that does not look at the token?

- A) yes
- B) no — the timeout is cooperative
- C) yes, always
- D) only in `Debug`

<details><summary>Answer</summary>

**B.** Verified: it finished normally in ~300 ms.
→ [Basics](basics.en.md#timeout)

</details>

## 4. 🟢 What does the code get with an open circuit?

- A) the operation's result
- B) `BrokenCircuitException`, the body does not run
- C) `TimeoutRejectedException`
- D) `null`

<details><summary>Answer</summary>

**B.** Verified: the call counter did not grow.
→ [Basics](basics.en.md#circuit-breaker)

</details>

## 5. 🟡 `AddRetry(...).AddTimeout(50 ms)`: how many times does the body run with a constantly long operation and 2 retries?

- A) 1
- B) 2
- C) 3
- D) 50

<details><summary>Answer</summary>

**C.** Verified: 3 runs — retry is outer, each got its own timeout.
→ [Advanced](advanced.en.md#composing-strategies)

</details>

## 6. 🟡 How do you get a pipeline from DI?

- A) `new ResiliencePipeline()`
- B) `AddResiliencePipeline("name", …)` and `ResiliencePipelineProvider<string>.GetPipeline("name")`
- C) through `static`
- D) you cannot

<details><summary>Answer</summary>

**B.** Verified.
→ [Advanced](advanced.en.md#dependency-injection)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) hedging starts extra calls if the first is slow
- [ ] B) `ExecuteOutcomeAsync` throws on failure
- [ ] C) the rate limiter in Polly is a separate package `Polly.RateLimiting`
- [ ] D) a pipeline can be reused from several threads

<details><summary>Answer</summary>

**A, C, D.** B is wrong: it returns an `Outcome<T>` with an `Exception` (verified).
→ [Advanced](advanced.en.md#a-result-without-exceptions)

</details>

## 8. 🟡 Open question: how do you protect a call to an unstable service?

<details><summary>Key points of a good answer</summary>

- a total timeout → retry with an exponent and jitter for transient failures → circuit breaker → attempt timeout;
- a fallback/degradation on `BrokenCircuitException` and timeout;
- idempotency; logging the triggers;
- a pipeline is a singleton.

</details>

## 9. 🔴 Why does the breaker not open when a pipeline is created per call?

- A) a Polly bug
- B) the breaker's state belongs to the pipeline instance
- C) `Retry` is needed
- D) `Fallback` is needed

<details><summary>Answer</summary>

**B.** A pipeline must be a singleton/from DI.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>

## 10. 🔴 What is dangerous about hedging?

- A) nothing
- B) it multiplies load on the service and suits only idempotent operations
- C) it disables retry
- D) it breaks tokens

<details><summary>Answer</summary>

**B.** Verified: 2 calls were started for one answer.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>
