---
title: "Polly: basics"
lang: en
track: csharp
kind: library
doc: basics
slug: polly
part: 2
tags: [polly, resilience, retry, timeout, circuit-breaker, fallback]
versions: ".NET 11, C# 15; Polly 8.9.1"
status: draft
updated: 2026-11-22
---

# Polly: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [10 · async/await](../../modules/10-async-await/overview.en.md), [08 · Exceptions](../../modules/08-exceptions/overview.en.md). **Time:** ~30 min.

## What it is and why

Polly is a resilience library: it wraps an operation (a service call, a DB query) in strategies that cope with transient failures. In version 8 strategies are assembled into a **pipeline** (`ResiliencePipeline`): retry, timeout, circuit breaker, fallback, hedging, rate limiter. Polly underlies `Microsoft.Extensions.Http.Resilience` ([the HttpClient recipe](../../recipes/http-client-resilience/overview.en.md)).

```bash
dotnet add package Polly
```

## Retry

```csharp
var retry = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    MaxRetryAttempts = 3, Delay = TimeSpan.FromMilliseconds(20),
    BackoffType = DelayBackoffType.Exponential, UseJitter = false,
    ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>()
}).Build();

var r = await retry.ExecuteAsync(async ct => { attempts++; if (attempts < 3) throw new InvalidOperationException(); return attempts; });
```

Verified: the operation failed twice and returned a result on the third attempt (`ok after 3`); the delays between attempts were `20 ms`, `40 ms` (exponential).

- If all attempts are exhausted the exception is propagated: on a constant failure `InvalidOperationException` after **4** attempts (1 + 3 retries) was verified.
- `ShouldHandle` says which failures to retry: an exception of another type (`ArgumentException`) was not retried — **1 attempt**.

## Timeout

```csharp
var to = new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromMilliseconds(100)).Build();
await to.ExecuteAsync(async ct => await Task.Delay(2000, ct));    // TimeoutRejectedException
```

Verified: a 2 s operation was interrupted after ~100 ms with `TimeoutRejectedException`. **Important:** the timeout is "cooperative" — it cancels the token `ct`. An operation that ignores the token (`Thread.Sleep(300)`) is not interrupted: verified — it finished normally in ~300 ms.

## Circuit breaker

```csharp
var cb = new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5, MinimumThroughput = 2, SamplingDuration = TimeSpan.FromSeconds(10), BreakDuration = TimeSpan.FromMilliseconds(500),
    ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>()
}).Build();
```

Verified on an always-failing operation: calls 1 and 2 — the exception passes through (the operation ran), the circuit **opened** (`OPENED`); calls 3 and 4 — `BrokenCircuitException`, **the body did not run** (the call counter stayed 2). After `BreakDuration` the circuit became `HALF-OPEN`, a successful trial call closed it (`CLOSED`).

## Fallback

```csharp
var fb = new ResiliencePipelineBuilder<string>().AddFallback(new FallbackStrategyOptions<string>
{
    ShouldHandle = new PredicateBuilder<string>().Handle<InvalidOperationException>(),
    FallbackAction = a => Outcome.FromResultAsValueTask("default value")
}).Build();
```

Verified: `default value` instead of an exception. A pipeline with a result needs `ResiliencePipelineBuilder<T>`, and the call needs an explicit type: `fb.ExecuteAsync<string>(…)` (without it — compile error CS0411).

## Beginner mistakes

- ❌ Retrying everything → ✅ `ShouldHandle` only for transient failures.
- ❌ Retries without a delay and jitter → ✅ an exponent + jitter.
- ❌ Expecting a timeout to stop code that does not look at the token → ✅ pass `ct` to every wait.
- ❌ Retrying non-idempotent operations → ✅ idempotency ([HttpClient: idempotency](../../recipes/http-client-resilience/advanced.en.md#idempotency-and-retries)).
- ❌ Not handling `BrokenCircuitException` and `TimeoutRejectedException` → ✅ plan for degradation.

## Related materials

- Next: [advanced](advanced.en.md) — composition, DI, hedging, rate limiter, a result without exceptions.
- [Resilient HttpClient](../../recipes/http-client-resilience/overview.en.md) — Polly on top of `HttpClient`.
- [10 · async/await](../../modules/10-async-await/overview.en.md) — cancellation.
- [Python](../../../python/overview.en.md) — tenacity and analogues.

## Sources

- [Polly: documentation](https://www.pollydocs.org/)
- [Polly: Retry strategy](https://www.pollydocs.org/strategies/retry.html)
- [Polly: Circuit breaker](https://www.pollydocs.org/strategies/circuit-breaker.html)
- [Polly: Timeout](https://www.pollydocs.org/strategies/timeout.html)
