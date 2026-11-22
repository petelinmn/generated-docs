---
title: "Resilient HttpClient: production"
lang: en
track: csharp
kind: recipe
doc: advanced
slug: http-client-resilience
part: 2
tags: [httpclient, typed-client, circuit-breaker, polly, idempotency, resilience]
versions: ".NET 11, C# 15; Microsoft.Extensions.Http.Resilience 11.0.1"
status: draft
updated: 2026-11-22
---

# Resilient HttpClient: production

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · **2. Production** · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [the minimal solution](basics.en.md). **Time:** ~45 min.

## A typed client

Instead of a string name — a class into which the factory injects a configured `HttpClient`:

```csharp
services.AddHttpClient<GitClient>(c => c.BaseAddress = new Uri(baseUrl))
        .AddStandardResilienceHandler();

class GitClient(HttpClient http)
{
    public async Task<int> GetAsync() { var r = await http.GetAsync("/dep"); return (int)r.StatusCode; }
}
```

The client is registered as transient; do not keep it in a singleton longer than the handler lifetime (otherwise DNS rotation is lost) — inject `GitClient` where it is needed or use `IHttpClientFactory`.

## Circuit breaker

If a service is unavailable for a long time, retries only add load to it. A circuit breaker "opens the circuit": after a series of failures calls are rejected at once without reaching the server. Verified (`MinimumThroughput = 2`, `FailureRatio = 0.5`, `BreakDuration = 1 s`; the server answers `500`; one retry):

| Call | Result |
|---|---|
| 1 | `500`, the server received the request (both attempts failed — the circuit opened) |
| 2–5 | `BrokenCircuitException`, **the server did not receive a request** |
| after 1.2 s (the server is healed) | `200` — the half-open state let a trial call through, the circuit closed |
| next | `200` |

Catch `BrokenCircuitException` (`Polly.CircuitBreaker`) and respond with degradation: a cache, a default value, a clear error.

## A custom pipeline

The standard handler suits most cases. For your own set of strategies use `AddResilienceHandler`:

```csharp
services.AddHttpClient("custom", c => c.BaseAddress = new Uri(baseUrl))
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

Verified: on a constant `500` — `500` after 3 calls (1 + 2 retries). The order of strategies matters: the outer one wraps the inner ones.

The standard handler is configured through a lambda: `AddStandardResilienceHandler(o => { o.Retry.MaxRetryAttempts = 5; o.AttemptTimeout.Timeout = …; })`. The options are validated when the client is first created: verified that `Retry.MaxRetryAttempts = 0` is an `OptionsValidationException` ("must be between 1 and …"). To disable retries take your own pipeline without `AddRetry`.

## Idempotency and retries

A retry is safe only for idempotent operations: `GET`, `PUT`, `DELETE`. The standard handler by default also retries `POST` (verified: `POST` with `503` — 4 calls), which can create duplicates (the server performed the request but the response was lost). For unsafe operations:

- pass an idempotency key (`Idempotency-Key`) that the server de-duplicates;
- or disable retries for such requests (`DisableForUnsafeHttpMethods()` on `HttpRetryStrategyOptions`, or a separate client without retries).

> ⚠️ Verify: the name and behavior of `DisableForUnsafeHttpMethods()` — check against the documentation for your package version.

## Testing

Do not go to the real network: substitute a stub `HttpMessageHandler` or start a test server ([17 · Integration tests](../../modules/17-testing-integration/overview.en.md)). A server answering `503` N times and then `200` is the simplest way to check retries (the examples above are built that way).

## Best practices

| Rule | Why |
|---|---|
| `IHttpClientFactory`, not `new HttpClient()` | connection pools and DNS rotation |
| An attempt timeout and a total timeout | do not hang for 100 s |
| Retries with an exponent and jitter | do not amplify a storm |
| A circuit breaker | do not finish off a failing service |
| Retry only idempotent calls | no duplicates |
| Degrade instead of failing | resilience of the user scenario |
| Metrics and logs of retries | see that the service is being "healed" by retries ([22](../../modules/22-observability/overview.en.md)) |

## Related materials

- Back: [minimal](basics.en.md). Next: [scale and edges](expert.en.md).
- [17 · Integration tests](../../modules/17-testing-integration/overview.en.md)
- [22 · Observability](../../modules/22-observability/overview.en.md)
- [Background jobs](../background-jobs/overview.en.md) — calls to external services from workers.

## Sources

- [Build resilient HTTP apps](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- [Polly: circuit breaker](https://www.pollydocs.org/strategies/circuit-breaker.html)
- [Polly: retry](https://www.pollydocs.org/strategies/retry.html)
