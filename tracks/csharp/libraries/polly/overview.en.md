---
title: "Polly"
lang: en
track: csharp
kind: library
doc: overview
slug: polly
part: 2
levels: [1, 2, 3, 4]
tags: [polly, resilience, retry, circuit-breaker, timeout, fallback, hedging]
versions: ".NET 11, C# 15; Polly 8.9.1"
status: draft
updated: 2026-11-22
---

# Polly

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Polly is a resilience library. Strategies (retry, timeout, circuit breaker, fallback, hedging, rate limiter) are assembled into an immutable, thread-safe `ResiliencePipeline`. It sits under `Microsoft.Extensions.Http.Resilience` but applies to any code: databases, queues, external APIs. The behavior of all the main strategies was verified by running.

## Key ideas

- A pipeline = strategies in order, the first is the outer one.
- Retry: `ShouldHandle`, an exponent, jitter; after exhaustion — an exception.
- The timeout is cooperative (it cancels the token).
- Circuit breaker: Closed → Open → Half-Open; `BrokenCircuitException` without running the body.
- Fallback, hedging, rate limiter (`Polly.RateLimiting`).
- DI: `AddResiliencePipeline`, `ResiliencePipelineProvider`.
- A pipeline is a singleton: the breaker's state lives in it.

## Minimal example

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3, Delay = TimeSpan.FromMilliseconds(20) })
    .AddTimeout(TimeSpan.FromSeconds(1))
    .Build();
var result = await pipeline.ExecuteAsync(async ct => await CallServiceAsync(ct));
```

## When to use and when not

- ✅ Calls to external services, DBs, queues with transient failures.
- ✅ You need timeouts, a breaker and degradation.
- ✅ One resilience policy across the application.
- ❌ Retrying non-idempotent operations without protection.
- ❌ Masking permanent configuration errors.
- ❌ HTTP only and the defaults suit you — the ready standard handler is enough.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | retry, timeout, circuit breaker, fallback |
| [2. Advanced](advanced.en.md) | composition, DI, rate limiter, hedging, `Outcome` |
| [3. Expert](expert.en.md) | internals, gotchas, alternatives, 11 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp polly level 5` |
| 6. Tests | not generated yet — `/learn csharp polly level 6` |

## Related materials

- Prerequisites: [10 · async/await](../../modules/10-async-await/overview.en.md), [08 · Exceptions](../../modules/08-exceptions/overview.en.md)
- Nearby: [Resilient HttpClient](../../recipes/http-client-resilience/overview.en.md), [Rate limiting](../../recipes/rate-limiting/overview.en.md), [Refit](../refit/overview.en.md), [MassTransit](../masstransit/overview.en.md) — retries at a broker endpoint's level
- In other tracks: [Python](../../../python/overview.en.md) — tenacity
- [System design: reliability patterns](../../../system-design/modules/09-reliability-patterns/overview.en.md)
