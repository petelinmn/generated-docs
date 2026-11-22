---
title: "Exercises: Polly"
lang: en
track: csharp
kind: library
doc: exercises
slug: polly
part: 2
tags: [polly, retry, timeout, circuit-breaker, fallback, hedging]
status: draft
updated: 2026-11-22
---

# Exercises: Polly

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself. The solutions were verified by running (Polly 8.9.1, Polly.Extensions, Polly.RateLimiting). There are no runnable versions in `code/` (level 5 does not exist).

## 1. 🟢 Retry

**Task:** an operation fails twice (`InvalidOperationException`) and succeeds the third time. Configure 3 retries with an exponential delay starting at 20 ms.

**Expected result:** `ok after 3`; delays of 20 and 40 ms.

<details><summary>Hint</summary>

`AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3, Delay = …, BackoffType = DelayBackoffType.Exponential, UseJitter = false, ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>() })`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: retry with exponential backoff.
var retry = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    MaxRetryAttempts = 3, Delay = TimeSpan.FromMilliseconds(20),
    BackoffType = DelayBackoffType.Exponential, UseJitter = false,
    ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>()
}).Build();
var r = await retry.ExecuteAsync(async ct => { attempts++; if (attempts < 3) throw new InvalidOperationException(); return "ok after " + attempts; });
```

Verified: `retry #1 after 20ms`, `retry #2 after 40ms`, `ok after 3`; on a constant failure — the exception after 4 attempts.

</details>

## 2. 🟡 Timeout

**Task:** interrupt a 2 s operation after 100 ms. Then check an operation with `Thread.Sleep(300)`.

**Expected result:** `TimeoutRejectedException` after ~100 ms; the second operation finishes normally in ~300 ms.

<details><summary>Hint</summary>

`AddTimeout(TimeSpan.FromMilliseconds(100))`; the timeout cancels the token — code that ignores the token is not interrupted.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: a cooperative timeout.
var to = new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromMilliseconds(100)).Build();
await to.ExecuteAsync(async ct => await Task.Delay(2000, ct));                   // TimeoutRejectedException
await to.ExecuteAsync(async ct => { Thread.Sleep(300); await Task.CompletedTask; });   // not interrupted
```

Verified: `timeout: TimeoutRejectedException in ~100ms`, `non-cooperative: finished normally in 300ms (token ignored)`.

</details>

## 3. 🟡 Circuit breaker

**Task:** configure a breaker (a threshold of 2 calls, ratio 0.5, a 500 ms pause). Make 4 calls of an always-failing operation, wait and make a successful call.

**Expected result:** calls 1–2 — the exception, 3–4 — `BrokenCircuitException` without running; after the pause — success.

<details><summary>Hint</summary>

`AddCircuitBreaker(new CircuitBreakerStrategyOptions { FailureRatio = 0.5, MinimumThroughput = 2, SamplingDuration = …, BreakDuration = … })`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: circuit breaker opens after two failures.
var cb = new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5, MinimumThroughput = 2,
    SamplingDuration = TimeSpan.FromSeconds(10), BreakDuration = TimeSpan.FromMilliseconds(500),
    ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>()
}).Build();
```

Verified: `call 1/2: failure passed through (calls=1/2)`, `OPENED`, `call 3/4: BrokenCircuitException (calls=2)`, then `HALF-OPEN`, `CLOSED`, `recovered`.

</details>

## 4. 🔴 Fallback and hedging

**Task:** (a) return `default value` instead of an exception; (b) start a duplicate call after 50 ms if the first (500 ms) is slow.

**Expected result:** `fallback: default value`; `hedging: answer from call 2`.

<details><summary>Hint</summary>

`ResiliencePipelineBuilder<string>().AddFallback(…)` and `.AddHedging(new HedgingStrategyOptions<string> { MaxHedgedAttempts = 1, Delay = …, ActionGenerator = a => () => a.Callback(a.ActionContext) })`; call `ExecuteAsync<string>(…)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: fallback value and hedged call.
var fb = new ResiliencePipelineBuilder<string>().AddFallback(new FallbackStrategyOptions<string>
{
    ShouldHandle = new PredicateBuilder<string>().Handle<InvalidOperationException>(),
    FallbackAction = a => Outcome.FromResultAsValueTask("default value")
}).Build();

var hedge = new ResiliencePipelineBuilder<string>().AddHedging(new HedgingStrategyOptions<string>
{
    MaxHedgedAttempts = 1, Delay = TimeSpan.FromMilliseconds(50),
    ShouldHandle = new PredicateBuilder<string>().Handle<Exception>(),
    ActionGenerator = a => () => a.Callback(a.ActionContext)
}).Build();
```

Verified: `fallback: default value`, `hedging: answer from call 2 (total calls started: 2)`.

</details>
