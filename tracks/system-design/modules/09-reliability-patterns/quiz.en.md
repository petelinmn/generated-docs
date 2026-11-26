---
title: "Quiz: reliability patterns"
lang: en
track: system-design
kind: module
doc: quiz
slug: 09-reliability-patterns
part: 2
tags: [system-design, resilience, retry, circuit-breaker, bulkhead, rate-limiting, load-shedding]
versions: ".NET 11, C# 15; Polly.Core 8.9.1; System.Threading.RateLimiting 11.0.0"
status: draft
updated: 2026-11-26
---

# Quiz: reliability patterns

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 `MaxRetryAttempts = 3` means how many calls at most?

- A) 2
- B) 3
- C) 4
- D) 6

<details><summary>Answer</summary>

**C.** The first call and three retries. → [Basics](basics.en.md)

</details>

## 2. 🟢 What is usually not retried?

- A) A timeout
- B) 503
- C) A validation error
- D) A dropped connection

<details><summary>Answer</summary>

**C.** A retry gives the same result. → [Basics](basics.en.md)

</details>

## 3. 🟢 Why jitter?

- A) To speed up retries
- B) To spread retries of different clients over time
- C) Encryption
- D) Compression

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 4. 🟢 What does Polly throw when a timeout expires?

- A) `TimeoutException`
- B) `TimeoutRejectedException`
- C) `BrokenCircuitException`
- D) nothing

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 5. 🟡 What does an open circuit breaker do?

- A) Retries more often
- B) Rejects the call without reaching the dependency
- C) Increases the timeout
- D) Writes to a queue

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 6. 🟡 What does this code print?

```csharp
using var limiter = Limits.TokenBucket(burst: 3, perPeriod: 1);
Console.WriteLine(Enumerable.Range(0, 10).Count(_ => limiter.AttemptAcquire().IsAcquired));
```

<details><summary>Answer</summary>

```text
3
```

Replenishment is off, only the initial stock is available. → [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) A concurrency limiter limits simultaneous operations
- [ ] B) An in-memory limit of one instance is cluster-wide
- [ ] C) A token bucket allows a burst
- [ ] D) A bulkhead isolates the resources of dependencies

<details><summary>Answer</summary>

**A, C, D.** → [Advanced](advanced.en.md)

</details>

## 8. 🟡 Which part of the standard handler is closest to the call itself?

- A) rate limiter
- B) retry
- C) attempt timeout
- D) total timeout

<details><summary>Answer</summary>

**C.** → [Advanced](advanced.en.md)

</details>

## 9. 🔴 What happens?

```csharp
var to = new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromMilliseconds(100)).Build();
await to.ExecuteAsync(async ct => await Task.Delay(600));
```

<details><summary>Answer</summary>

The call completes successfully after about 600 ms: the token is ignored. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

A dependency answers in 10 s instead of 50 ms. What do you do?

<details><summary>Key points of a good answer</summary>

- A timeout based on real percentiles.
- Bulkhead and circuit breaker.
- Degradation and load shedding.
- Do not add load with retries; observe.

</details>
