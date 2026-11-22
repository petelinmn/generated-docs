---
title: "Exercises: Resilient HttpClient"
lang: en
track: csharp
kind: recipe
doc: exercises
slug: http-client-resilience
part: 2
tags: [httpclient, retry, circuit-breaker, timeout, resilience]
status: draft
updated: 2026-11-22
---

# Exercises: Resilient HttpClient

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions were verified by running: a local Kestrel server with the needed responses + the `Microsoft.Extensions.Http.Resilience` 11.0.1 package. There are no runnable versions in `code/` (level 5 does not exist).

## 1. 🟢 Retries on a flaky service

**Task:** the server answers `503` twice, then `200`. Make the client return `200`.

**Expected result:** a plain client — `503` after 1 call, with the handler — `200` after 3 calls.

<details><summary>Hint</summary>

`AddHttpClient(...).AddStandardResilienceHandler()`. For speed reduce `o.Retry.Delay` and disable jitter.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: standard handler retries transient 503.
services.AddHttpClient("c", c => c.BaseAddress = new Uri(baseUrl))
    .AddStandardResilienceHandler(o =>
    {
        o.Retry.Delay = TimeSpan.FromMilliseconds(50);
        o.Retry.UseJitter = false;
    });
```

Verified: `standard flaky: 200 after 3 server calls`.

</details>

## 2. 🟡 The attempt timeout

**Task:** the service answers in 3 s. Limit an attempt to 300 ms, one retry. What does the code get?

**Expected result:** `TimeoutRejectedException` after about 630 ms and 2 calls.

<details><summary>Hint</summary>

`o.AttemptTimeout.Timeout`, `o.TotalRequestTimeout.Timeout`, `o.Retry.MaxRetryAttempts = 1`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: per-attempt timeout.
.AddStandardResilienceHandler(o =>
{
    o.AttemptTimeout.Timeout = TimeSpan.FromMilliseconds(300);
    o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
    o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(1000);
    o.Retry.MaxRetryAttempts = 1;
    o.Retry.Delay = TimeSpan.FromMilliseconds(10);
});
```

Verified: `TimeoutRejectedException after 2 server calls, 633ms`. `SamplingDuration` was raised because the validator requires the window not to be shorter than the attempt timeouts.

> ⚠️ Verify: the exact `SamplingDuration` validation rule — the 1000 s value comes from a working run, the rule was not checked against the documentation.

</details>

## 3. 🟡 The circuit breaker

**Task:** the server answers `500`; configure a breaker (`MinimumThroughput = 2`, `FailureRatio = 0.5`, `BreakDuration = 1 s`). Make 5 calls, then "heal" the server and repeat after a pause.

**Expected result:** call 1 — `500`, 2–5 — `BrokenCircuitException` with no server hit; after the pause — `200`.

<details><summary>Hint</summary>

A retry ≥ 1 is mandatory (0 is forbidden). The first call makes two attempts — enough to reach the threshold. Catch `BrokenCircuitException` from `Polly.CircuitBreaker`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: a short-circuiting breaker.
services.AddHttpClient<GitClient>(c => c.BaseAddress = new Uri(baseUrl))
    .AddStandardResilienceHandler(o =>
    {
        o.Retry.MaxRetryAttempts = 1; o.Retry.Delay = TimeSpan.FromMilliseconds(10); o.Retry.UseJitter = false;
        o.CircuitBreaker.MinimumThroughput = 2;
        o.CircuitBreaker.FailureRatio = 0.5;
        o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        o.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(1);
        o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
        o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
    });
```

Verified: `1: status 500, hit server=True`, `2..5: BrokenCircuitException, hit server=False`, `after break: 200`.

</details>

## 4. 🔴 A custom pipeline without the standard handler

**Task:** a client with a 1 s timeout and two retries with a constant 20 ms pause. How many calls does the server get on a constant `500`?

**Expected result:** a final `500` after 3 calls.

<details><summary>Hint</summary>

`AddResilienceHandler("my", p => { p.AddTimeout(…); p.AddRetry(new HttpRetryStrategyOptions { … }); })`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: custom resilience pipeline.
.AddResilienceHandler("my", p =>
{
    p.AddTimeout(TimeSpan.FromSeconds(1));
    p.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 2, Delay = TimeSpan.FromMilliseconds(20),
        BackoffType = DelayBackoffType.Constant, UseJitter = false
    });
});
```

Verified: `custom pipeline: 500 after 3 calls`.

</details>
