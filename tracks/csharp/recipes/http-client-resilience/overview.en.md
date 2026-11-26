---
title: "Resilient HttpClient"
lang: en
track: csharp
kind: recipe
doc: overview
slug: http-client-resilience
part: 2
levels: [1, 2, 3, 4]
tags: [httpclient, ihttpclientfactory, retry, circuit-breaker, polly, resilience, timeout]
versions: ".NET 11, C# 15; Microsoft.Extensions.Http.Resilience 11.0.1"
status: draft
updated: 2026-11-22
---

# Resilient HttpClient

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A recipe: call external HTTP services so that one-off failures do not break a request and long ones do not take down the application. `IHttpClientFactory` manages connections and DNS, `AddStandardResilienceHandler()` (Microsoft.Extensions.Http.Resilience, Polly v8) adds timeouts, retries with an exponent and jitter, and a circuit breaker. Verified by running against a server with artificial failures.

## Key ideas

- `IHttpClientFactory`, not `new HttpClient()` and not a forever-static one.
- `AddStandardResilienceHandler()`: 3 retries, attempt timeout 10 s, total 30 s, a circuit breaker.
- Transient failures are retried (`5xx`, `408`, `429`, network), other `4xx` are not.
- An open circuit → `BrokenCircuitException`, the request does not go out; a timeout → `TimeoutRejectedException`.
- Retries only for idempotent calls (or with an `Idempotency-Key`).
- Your own pipeline — `AddResilienceHandler`.
- Retry storms and cascading failures are a system-level matter.

## Minimal example

```csharp
builder.Services.AddHttpClient("api", c => c.BaseAddress = new Uri("https://api.example.com"))
    .AddStandardResilienceHandler();
```

## When to use and when not

- ✅ Any calls to external HTTP services.
- ✅ Microservice interactions that may fail.
- ❌ Retrying non-idempotent operations without duplicate protection.
- ❌ Blind retries on `4xx` and validation errors.
- ❌ Retries where the failure is a permanent configuration error.
- ❌ As a replacement for SLA expectations: retries increase latency.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Minimal](basics.en.md) | `AddStandardResilienceHandler`, defaults, timeouts |
| [2. Production](advanced.en.md) | typed clients, circuit breaker, a custom pipeline, idempotency |
| [3. Scale and edges](expert.en.md) | retry storms, bulkhead, hedging, gotchas, 10 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp http-client-resilience level 5` |
| 6. Tests | not generated yet — `/learn csharp http-client-resilience level 6` |

## Related materials

- Prerequisites: [10 · async/await](../../modules/10-async-await/overview.en.md), [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md)
- Nearby: [Rate limiting](../rate-limiting/overview.en.md), [Background jobs](../background-jobs/overview.en.md), [22 · Observability](../../modules/22-observability/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — httpx with retries; [Kubernetes · Service and networking](../../../kubernetes/modules/06-services/overview.en.md) — Service names and `Connection refused` during a rollout
- Recipes and libraries: [Polly](../../libraries/polly/overview.en.md), [Refit](../../libraries/refit/overview.en.md), [Caching](../caching/overview.en.md), [gRPC service](../grpc-service/overview.en.md)
- [Scaling and load balancing](../../../system-design/modules/02-scalability-and-load-balancing/overview.en.md)
- [System design: API design](../../../system-design/modules/05-api-design/overview.en.md)
