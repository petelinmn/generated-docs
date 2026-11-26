---
title: "Reliability patterns"
lang: en
track: system-design
kind: module
doc: overview
slug: 09-reliability-patterns
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, resilience, retry, circuit-breaker, bulkhead, rate-limiting, load-shedding]
versions: ".NET 11, C# 15; Polly.Core 8.9.1; System.Threading.RateLimiting 11.0.0"
status: draft
updated: 2026-11-26
---

# Reliability patterns

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Dependencies are slow and they break. Reliability patterns stop one failure from spreading: they limit waiting, retries, concurrent work and the incoming flow.

## Key ideas

- Every network call needs a timeout.
- Retry only transient errors and only safe operations.
- Grow the delay between retries and randomize it.
- Retries at several layers multiply.
- A circuit breaker stops calling what does not answer.
- A bulkhead keeps one dependency from taking all resources.
- An overloaded system should refuse quickly.

## Minimal example

```csharp
using RateLimitLease lease = limiter.AttemptAcquire();
if (!lease.IsAcquired) return (false, default);
```

## When to use and when not

- ✅ Any call over the network.
- ✅ A shared resource that can be overloaded.
- ❌ Retrying a non-idempotent operation without a key.
- ❌ A circuit breaker around a local computation.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | timeouts, retries, backoff |
| [2. Advanced](advanced.en.md) | circuit breaker, bulkhead, rate limiting, load shedding |
| [3. Expert](expert.en.md) | retry amplification, defaults, gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | Polly, rate limiters |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [API design](../05-api-design/overview.en.md), [messaging and events](../08-messaging-and-events/overview.en.md)
- In other tracks: [Polly](../../../csharp/libraries/polly/overview.en.md), [resilient HttpClient](../../../csharp/recipes/http-client-resilience/overview.en.md), [rate limiting](../../../csharp/recipes/rate-limiting/overview.en.md)
- Availability and error budget: [design process](../01-design-process/overview.en.md)
- [Observability and SLOs](../14-observability-and-slo/overview.en.md)
- [Security architecture](../15-security-architecture/overview.en.md)
