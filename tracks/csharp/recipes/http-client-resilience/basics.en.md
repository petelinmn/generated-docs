---
title: "Resilient HttpClient: the minimal solution"
lang: en
track: csharp
kind: recipe
doc: basics
slug: http-client-resilience
part: 2
tags: [httpclient, ihttpclientfactory, retry, resilience, polly, timeout]
versions: ".NET 11, C# 15; Microsoft.Extensions.Http.Resilience 11.0.1"
status: draft
updated: 2026-11-22
---

# Resilient HttpClient: the minimal solution

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Minimal** · [2. Production](advanced.en.md) · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [10 · async/await](../../modules/10-async-await/overview.en.md), [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md). **Time:** ~30 min.

## The task

The application calls an external HTTP service. The network is unreliable: the service answers 503, hangs, fails for a while. You need to call it so that one-off failures do not break our request, and long failures do not take down the whole application. .NET has `IHttpClientFactory` and the `Microsoft.Extensions.Http.Resilience` package (built on Polly v8).

## The minimal solution

```bash
dotnet add package Microsoft.Extensions.Http.Resilience
```

```csharp
builder.Services.AddHttpClient("api", c => c.BaseAddress = new Uri("https://api.example.com"))
    .AddStandardResilienceHandler();

// usage
var client = factory.CreateClient("api");
var response = await client.GetAsync("/items");
```

`AddStandardResilienceHandler()` inserts a ready pipeline into the chain: a total timeout, retries, a circuit breaker, an attempt timeout (and a rate limiter on the outgoing queue). Verified against a server that answers 503 twice and then 200 (the retry delay was reduced to 50 ms to keep the example fast):

| Client | Result |
|---|---|
| plain `AddHttpClient` | `503` after 1 call |
| `AddStandardResilienceHandler` | `200` after 3 calls (2 retries) |

## Defaults

Verified by reading `HttpStandardResilienceOptions`:

| Strategy | Defaults |
|---|---|
| Retry | `MaxRetryAttempts = 3`, `Delay = 2 s`, `BackoffType = Exponential`, `UseJitter = true` |
| Attempt timeout | `10 s` |
| Total request timeout | `30 s` |
| Circuit breaker | `FailureRatio = 0.1`, `MinimumThroughput = 100`, `SamplingDuration = 30 s`, `BreakDuration = 5 s` |

Retries apply to "transient" responses: network exceptions, timeouts, `5xx`, `408` and `429`. Verified: `408`, `429` and `501` — 4 calls each (1 + 3 retries); `400` and `404` are not retried (1 call); on a constant `500` — 4 calls and then the final `500`.

## The timeout

The default `HttpClient` timeout is 100 seconds (verified: `00:01:40`): far too much. The pipeline limits each attempt and the whole request. Verified with `AttemptTimeout = 300 ms` and a slow (3 s) service: there were 2 attempts (1 + 1 retry), and the result was `TimeoutRejectedException` after about 630 ms.

## Beginner mistakes

- ❌ `new HttpClient()` per request → ✅ socket exhaustion; use `IHttpClientFactory`.
- ❌ One static `HttpClient` forever → ✅ it does not refresh DNS; the factory rotates handlers ([edges](expert.en.md#edges-and-gotchas)).
- ❌ Retries on `POST` without idempotency → ✅ duplicates ([production](advanced.en.md#idempotency-and-retries)).
- ❌ Keeping the default timeout (100 s) → ✅ a pipeline with timeouts.
- ❌ Retries without a pause and jitter → ✅ exponential delay with jitter (the default in the standard handler).

## Related materials

- Next: [production](advanced.en.md) — typed clients, a custom pipeline, circuit breaker, tests.
- [10 · async/await](../../modules/10-async-await/overview.en.md) — cancellation and timeouts.
- [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md) — registering clients.
- [Rate limiting](../rate-limiting/overview.en.md) — the server side: what to do with `429`.
- [Python](../../../python/overview.en.md) — httpx with retries.

## Sources

- [Make HTTP requests with `IHttpClientFactory`](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory)
- [Build resilient HTTP apps](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- [Polly: resilience strategies](https://www.pollydocs.org/strategies/index.html)
