---
title: "Exercises: Rate limiting"
lang: en
track: csharp
kind: recipe
doc: exercises
slug: rate-limiting
part: 2
tags: [rate-limiting, fixed-window, token-bucket, partitioned-limiter, concurrency-limiter]
status: draft
updated: 2026-11-22
---

# Exercises: Rate limiting

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions were verified by running (Kestrel + `HttpClient`; the built-in middleware, no packages needed). There are no runnable versions in `code/` (level 5 does not exist).

## 1. 🟢 A fixed window

**Task:** limit `/fixed` to three requests per 2 seconds. Send 5 simultaneous requests and one after the window.

**Expected result:** among the 5 — three `200` and two `429`; after the window — `200`.

<details><summary>Hint</summary>

`AddFixedWindowLimiter("fixed", f => { f.PermitLimit = 3; f.Window = TimeSpan.FromSeconds(2); f.QueueLimit = 0; })` and `RequireRateLimiting("fixed")`; `app.UseRateLimiter()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: a fixed window policy attached to an endpoint.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddFixedWindowLimiter("fixed", f => { f.PermitLimit = 3; f.Window = TimeSpan.FromSeconds(2); f.QueueLimit = 0; });
});
app.UseRateLimiter();
app.MapGet("/fixed", () => "ok").RequireRateLimiting("fixed");
```

Verified: `429 200 200 429 200` (the order of parallel responses is undefined), after the window — `200`.

</details>

## 2. 🟡 A per-user limit with Retry-After

**Task:** a policy `per-user`: 2 requests per 5 seconds per user (the `X-User` header). Rejections must carry `Retry-After`.

**Expected result:** `ann`: `200 200 429(Retry-After=…)`, `bob`: `200`.

<details><summary>Hint</summary>

`o.AddPolicy("per-user", ctx => RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions { … }))` and `OnRejected`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: a partitioned limiter and the Retry-After header.
o.AddPolicy("per-user", ctx => RateLimitPartition.GetFixedWindowLimiter(
    ctx.Request.Headers["X-User"].FirstOrDefault() ?? "anonymous",
    _ => new FixedWindowRateLimiterOptions { PermitLimit = 2, Window = TimeSpan.FromSeconds(5), QueueLimit = 0 }));
o.OnRejected = (ctx, ct) =>
{
    if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra))
        ctx.HttpContext.Response.Headers.RetryAfter = ((int)ra.TotalSeconds).ToString();
    return ValueTask.CompletedTask;
};
```

Verified: `ann: 200 200 429(Retry-After=4)`, `bob: 200`.

</details>

## 3. 🟡 A token bucket

**Task:** a capacity of 2 tokens, refill +1 per second. Send three requests in a row, wait 1.1 s and send one more.

**Expected result:** `200 200 429 200`.

<details><summary>Hint</summary>

`AddTokenBucketLimiter("bucket", t => { t.TokenLimit = 2; t.TokensPerPeriod = 1; t.ReplenishmentPeriod = TimeSpan.FromSeconds(1); t.QueueLimit = 0; t.AutoReplenishment = true; })`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: a burst of 2 is allowed, then one request per second on average.
o.AddTokenBucketLimiter("bucket", t =>
{
    t.TokenLimit = 2; t.TokensPerPeriod = 1; t.ReplenishmentPeriod = TimeSpan.FromSeconds(1);
    t.QueueLimit = 0; t.AutoReplenishment = true;
});
```

Verified: `200 200 429(Retry-After=1) 200`.

</details>

## 4. 🔴 Protecting a heavy operation

**Task:** the endpoint `/slow` (300 ms of work) must serve at most one request at a time, the rest get 429. Check with three simultaneous requests.

**Expected result:** one `200`, two `429`.

<details><summary>Hint</summary>

`AddConcurrencyLimiter("concurrent", c => { c.PermitLimit = 1; c.QueueLimit = 0; })`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: a concurrency limiter protects an expensive endpoint.
o.AddConcurrencyLimiter("concurrent", c => { c.PermitLimit = 1; c.QueueLimit = 0; });
app.MapGet("/slow", async () => { await Task.Delay(300); return "done"; }).RequireRateLimiting("concurrent");
```

Verified: `200 429 429`. With `QueueLimit > 0` the extra requests would wait rather than be rejected.

</details>
