---
title: "Reliability patterns: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 09-reliability-patterns
part: 2
tags: [system-design, resilience, retry, circuit-breaker, bulkhead, rate-limiting, load-shedding]
versions: ".NET 11, C# 15; Polly.Core 8.9.1; System.Threading.RateLimiting 11.0.0"
status: draft
updated: 2026-11-26
---

# Reliability patterns: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** async/await, `CancellationToken` · **Time:** ~2 h

## Why it matters

One dependency slows down, callers wait and pile up threads, connections and memory, and soon the whole system stops answering. This is a cascading failure.

## Timeouts

Without a timeout, waiting can last indefinitely. A timeout in Polly cancels a `CancellationToken`, so the code must accept and respect it.

```csharp
new ResiliencePipelineBuilder { TimeProvider = clock }
    .AddTimeout(new TimeoutStrategyOptions { Timeout = timeout })
    .Build();
```

On expiry `TimeoutRejectedException` is thrown.

## Retries

```csharp
.AddRetry(new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<TransientException>(),
    MaxRetryAttempts = maxRetries, // retries, not counting the first call
    Delay = delay,
    BackoffType = DelayBackoffType.Exponential,
})
```

`MaxRetryAttempts = 3` gives four calls. An exception not listed in `ShouldHandle` is not retried. File: [Ex01_Pipelines.cs](code/Examples/Ex01_Pipelines.cs).

## What to retry

| Retry | Do not retry |
|---|---|
| a dropped connection, a timeout | a validation error |
| 408, 429, 5xx | most other 4xx |
| idempotent operations | a POST without an idempotency key |

## Backoff and jitter

The delay grows exponentially to give the dependency time to recover. A random component (jitter) keeps thousands of clients from retrying at the same moment.

## Beginner mistakes

- ❌ No timeout → ✅ always set one.
- ❌ Retrying everything → ✅ only transient errors.
- ❌ Immediate retries → ✅ backoff with jitter.
- ❌ Ignoring the `CancellationToken` → ✅ pass it on.

## Related materials

- [Advanced theory](advanced.en.md)
- [Polly](../../../csharp/libraries/polly/overview.en.md)

## Sources

- [Polly: retry](https://www.pollydocs.org/strategies/retry.html)
- [Polly: timeout](https://www.pollydocs.org/strategies/timeout.html)
