---
title: "Rate limiting: production"
lang: en
track: csharp
kind: recipe
doc: advanced
slug: rate-limiting
part: 2
tags: [rate-limiting, partitioned-limiter, retry-after, token-bucket, concurrency-limiter]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# Rate limiting: production

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · **2. Production** · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [the minimal solution](basics.en.md). **Time:** ~45 min.

## A limit per user

A shared limit for everyone is pointless: one client eats the others' limit. You need a **partition key**: a user, an API key or an IP.

```csharp
o.AddPolicy("per-user", ctx => RateLimitPartition.GetFixedWindowLimiter(
    ctx.Request.Headers["X-User"].FirstOrDefault() ?? "anonymous",
    _ => new FixedWindowRateLimiterOptions { PermitLimit = 2, Window = TimeSpan.FromSeconds(5), QueueLimit = 0 }));
```

Verified: the user `ann` — `200 200 429`, while `bob` at the same time — `200`: each key has its own counter. In a real application the key comes from the authenticated user (`ctx.User.Identity?.Name`), for anonymous callers from the IP (`ctx.Connection.RemoteIpAddress`); behind a proxy a correct `X-Forwarded-For` (`UseForwardedHeaders`) is needed, otherwise everyone has one IP — the proxy's address.

## The response to the client: Retry-After

It is useful for the client to know when to retry:

```csharp
o.OnRejected = (ctx, ct) =>
{
    if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra))
        ctx.HttpContext.Response.Headers.RetryAfter = ((int)ra.TotalSeconds).ToString();
    return ValueTask.CompletedTask;
};
```

Verified: a rejection carries `Retry-After` (for a fixed window — the remaining seconds of the window, for example `1` or `4`). `OnRejected` is the place for logging rejections and a response body (`ProblemDetails`, [14](../../modules/14-aspnet-core-minimal-api/advanced.en.md#validation-and-problemdetails)).

## Queues and choosing an algorithm

`QueueLimit > 0` puts excess requests in a queue instead of rejecting them at once (`QueueProcessingOrder.OldestFirst`/`NewestFirst`): a request waits for the limit to free up. A queue smooths short bursts but holds connections and memory; for most APIs `QueueLimit = 0` and a fast rejection are better.

| Task | Algorithm |
|---|---|
| protecting sign-in from brute force | fixed/sliding window by IP+login |
| a public API with a quota | fixed window by API key |
| allow bursts with a limited average rate | token bucket (verified: 2 tokens, +1/s) |
| a heavy operation (a report, an export) | concurrency (verified: 1 of 3 simultaneous) |

## Global and nested limits

A `GlobalLimiter` applies to all requests; `RequireRateLimiting` adds an endpoint limit on top. For exceptions there is `DisableRateLimiting()` (for example health checks, [22 · Observability](../../modules/22-observability/overview.en.md)). A chain of limiters is combined through `PartitionedRateLimiter.CreateChained`.

## Best practices

| Rule | Why |
|---|---|
| A limit per key, not a shared one | fairness |
| `Retry-After` and a clear 429 body | clients do not hammer repeatedly |
| Stricter on `/login`, softer on reads | different risks |
| Log rejections and count them as a metric | see attacks and wrong limits |
| Exclude health checks | the orchestrator must not get 429 |
| Test the window boundaries | errors at the window boundary |

## Related materials

- Back: [minimal](basics.en.md). Next: [scale and edges](expert.en.md).
- [JWT authentication](../jwt-auth/overview.en.md) — a key per user.
- [22 · Observability](../../modules/22-observability/overview.en.md) — rejection metrics.
- [Caching](../caching/overview.en.md) — reducing load without rejections.

## Sources

- [Rate limiting in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [Forwarded headers](https://learn.microsoft.com/aspnet/core/host-and-deploy/proxy-load-balancer)
