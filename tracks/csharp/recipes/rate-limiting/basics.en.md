---
title: "Rate limiting: the minimal solution"
lang: en
track: csharp
kind: recipe
doc: basics
slug: rate-limiting
part: 2
tags: [rate-limiting, throttling, aspnet-core, fixed-window, http-429]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# Rate limiting: the minimal solution

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Minimal** · [2. Production](advanced.en.md) · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md). **Time:** ~30 min.

## The task

Limit the request rate to an API: protect against password guessing, accidental client "loops" and overload. Exceeding the limit is answered with **429 Too Many Requests**. ASP.NET Core has a built-in middleware (`Microsoft.AspNetCore.RateLimiting`), no packages needed.

## The minimal solution

```csharp
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddFixedWindowLimiter("fixed", f => { f.PermitLimit = 3; f.Window = TimeSpan.FromSeconds(2); f.QueueLimit = 0; });
});

app.UseRateLimiter();
app.MapGet("/fixed", () => "ok").RequireRateLimiting("fixed");
app.MapGet("/free", () => "ok");                       // no limit
```

The `fixed` policy allows 3 requests per 2-second window. Verified on 5 simultaneous requests: 3 got `200`, 2 got `429`; after the window ends a request passes again (`200`). The `/free` endpoint without a policy is unlimited: ten requests — ten `200`s. `UseRateLimiter()` is called after routing (in minimal APIs — before `Map...`); the limit is attached to specific endpoints through `RequireRateLimiting`.

## Algorithms

| Algorithm | The idea | When |
|---|---|---|
| Fixed window | N requests per a fixed window | simple and cheap; allows a burst at the window boundary |
| Sliding window | the window slides by segments | smooths the window boundary |
| Token bucket | a bucket of tokens refilled at a steady rate | allows bursts but limits the average rate |
| Concurrency | at most N at once | protecting heavy operations |

Token bucket verified: capacity 2, refill +1 per second — the first two requests `200`, the third at once `429`, after 1.1 s `200` again. Concurrency with a limit of 1: of three simultaneous slow requests one got `200`, two got `429`.

## Beginner mistakes

- ❌ Forgetting `UseRateLimiter()` → ✅ policies do not apply by themselves.
- ❌ One global limit for everyone together → ✅ a key per user or IP ([production](advanced.en.md#a-limit-per-user)).
- ❌ Not saying when to retry → ✅ the `Retry-After` header.
- ❌ A limit only in the application behind a balancer with several nodes → ✅ see [scale](expert.en.md#scale-several-instances).
- ❌ An unbounded queue → ✅ a small `QueueLimit` or 0.

## Related materials

- Next: [production](advanced.en.md) — keys, `Retry-After`, queues, nested policies.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md) — the middleware pipeline.
- [JWT authentication](../jwt-auth/overview.en.md) — protecting sign-in from brute force.
- [Python](../../../python/overview.en.md) — rate limiting in FastAPI.

## Sources

- [Rate limiting middleware](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [`System.Threading.RateLimiting`](https://learn.microsoft.com/dotnet/api/system.threading.ratelimiting)
