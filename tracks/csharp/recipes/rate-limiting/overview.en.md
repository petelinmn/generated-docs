---
title: "Rate limiting"
lang: en
track: csharp
kind: recipe
doc: overview
slug: rate-limiting
part: 2
levels: [1, 2, 3, 4]
tags: [rate-limiting, throttling, aspnet-core, http-429, token-bucket, partitioned-limiter]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# Rate limiting

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A recipe: limit the request rate to an API with the built-in ASP.NET Core middleware. Exceeding the limit is answered with `429 Too Many Requests` and `Retry-After`. The algorithms are fixed/sliding window, token bucket and concurrency; the limit is set per endpoint (`RequireRateLimiting`) and partitioned by a key (user, API key, IP). Counters live in process memory, so with several instances an exact quota is provided by a gateway or a shared store.

## Key ideas

- `AddRateLimiter` + `UseRateLimiter()` + `RequireRateLimiting("policy")`.
- A fixed window is simple but allows up to 2N at the boundary; a token bucket allows a burst with a limited average rate.
- A concurrency limiter protects heavy operations.
- A partition key (`RateLimitPartition`) — fairness between clients.
- `OnRejected` and `Retry-After` — a clear answer to the client.
- In-memory limits: with N instances up to N × the limit.
- Exclude health checks from limits.

## Minimal example

```csharp
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddFixedWindowLimiter("fixed", f => { f.PermitLimit = 3; f.Window = TimeSpan.FromSeconds(2); f.QueueLimit = 0; });
});
app.UseRateLimiter();
app.MapGet("/fixed", () => "ok").RequireRateLimiting("fixed");
```

## When to use and when not

- ✅ Public and authentication endpoints: protection from brute force and abuse.
- ✅ Heavy operations: a concurrency limiter.
- ✅ Quotas per API key (on a gateway or in the application).
- ❌ As the only DDoS protection (the network and a gateway are needed).
- ❌ An exact quota across several instances with only the built-in limiter.
- ❌ A limit without `Retry-After` and monitoring.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Minimal](basics.en.md) | the task, `AddRateLimiter`, a fixed window, algorithms |
| [2. Production](advanced.en.md) | a per-user limit, `Retry-After`, queues, global and nested limits |
| [3. Scale and edges](expert.en.md) | several instances, gateways and Redis, gotchas, alternatives, 10 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp rate-limiting level 5` |
| 6. Tests | not generated yet — `/learn csharp rate-limiting level 6` |

## Related materials

- Prerequisites: [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md)
- Nearby: [JWT authentication](../jwt-auth/overview.en.md), [Resilient HttpClient](../http-client-resilience/overview.en.md), [Caching](../caching/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — rate limiting in FastAPI
- Recipes and libraries: [Polly](../../libraries/polly/overview.en.md), [Background jobs](../background-jobs/overview.en.md), [File upload](../file-upload/overview.en.md), [Pagination and filtering](../pagination-filtering/overview.en.md), [Real-time with SignalR](../signalr-realtime/overview.en.md)
- [System design: reliability patterns](../../../system-design/modules/09-reliability-patterns/overview.en.md)
