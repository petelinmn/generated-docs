---
title: "Polly: advanced"
lang: en
track: csharp
kind: library
doc: advanced
slug: polly
part: 2
tags: [polly, resilience-pipeline, dependency-injection, hedging, rate-limiter, outcome]
versions: ".NET 11, C# 15; Polly 8.9.1, Polly.Extensions, Polly.RateLimiting"
status: draft
updated: 2026-11-22
---

# Polly: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~45 min.

## Composing strategies

Strategies are added to the pipeline in order; **the first is the outer one**, it wraps the rest:

```csharp
var composed = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 2, Delay = TimeSpan.Zero,
        ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>() })
    .AddTimeout(TimeSpan.FromMilliseconds(50))
    .Build();
```

Verified: `retry` outside + `timeout` inside: the operation body started **3 times** (each attempt got its own 50 ms timeout). If you swap the order (`timeout` outside), one timeout covers all attempts at once. A typical order: total timeout → retry → circuit breaker → attempt timeout.

## Dependency injection

The `Polly.Extensions` package registers named pipelines:

```csharp
services.AddResiliencePipeline("my-pipeline", b => b.AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 2, Delay = TimeSpan.FromMilliseconds(10) }));

var provider = sp.GetRequiredService<ResiliencePipelineProvider<string>>();
var pipeline = provider.GetPipeline("my-pipeline");
```

Verified: the pipeline was obtained from the provider and retried the operation (the result after 3 attempts). Pipelines are reused — do not create a new one per call (the circuit breaker's state lives in the pipeline).

## Rate limiter

The `Polly.RateLimiting` package wraps `System.Threading.RateLimiting`:

```csharp
var rl = new ResiliencePipelineBuilder().AddRateLimiter(new SlidingWindowRateLimiter(
    new SlidingWindowRateLimiterOptions { PermitLimit = 2, Window = TimeSpan.FromSeconds(5), SegmentsPerWindow = 1 })).Build();
```

Verified: three calls — `ok, ok, rejected` (`RateLimiterRejectedException` on the third). This limits **outgoing** calls; for the server side see [Rate limiting](../../recipes/rate-limiting/overview.en.md).

## Hedging

If the first call is slow, after `Delay` a second one is started and the first to finish is taken:

```csharp
new ResiliencePipelineBuilder<string>().AddHedging(new HedgingStrategyOptions<string>
{
    MaxHedgedAttempts = 1, Delay = TimeSpan.FromMilliseconds(50),
    ShouldHandle = new PredicateBuilder<string>().Handle<Exception>(),
    ActionGenerator = a => () => a.Callback(a.ActionContext)
}).Build();
```

Verified: the first call slept 500 ms, the second (after 50 ms) returned in 10 ms — the result `answer from call 2`, 2 calls started. Hedging multiplies load and suits only idempotent operations.

## A result without exceptions

`ExecuteOutcomeAsync` returns an `Outcome<T>` instead of throwing:

```csharp
var outcome = await p.ExecuteOutcomeAsync<int, string>(async (ctx, s) => { … }, ResilienceContextPool.Shared.Get(), "state");
// outcome.Exception, outcome.Result
```

Verified: `outcome.Exception` holds `InvalidOperationException` without throwing it outward.

## Observability

Log events in `OnRetry`, `OnOpened`, `OnClosed`, `OnHalfOpened`; Polly also publishes telemetry (metrics through `System.Diagnostics.Metrics`) when `AddTelemetry` is wired up.

> ⚠️ Verify: wiring up Polly telemetry (`ConfigureTelemetry`/metrics) was not run in this material.

## Best practices

| Rule | Why |
|---|---|
| A pipeline is a singleton/from DI | the breaker's state is shared |
| Order: timeout → retry → breaker → attempt timeout | predictable behavior |
| `ShouldHandle` for transient failures | do not retry logic errors |
| Jitter | no synchronized spikes |
| Idempotency for retry/hedging | no side effects |
| Log the triggers | see failures being "healed" |

## Related materials

- Back: [basics](basics.en.md). Next: [expert](expert.en.md).
- [Resilient HttpClient](../../recipes/http-client-resilience/overview.en.md) — the standard handler built on Polly.
- [Rate limiting](../../recipes/rate-limiting/overview.en.md) — the server-side limit.
- [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md) — registering services.

## Sources

- [Polly: Resilience pipelines](https://www.pollydocs.org/pipelines/index.html)
- [Polly: Dependency injection](https://www.pollydocs.org/advanced/dependency-injection.html)
- [Polly: Hedging](https://www.pollydocs.org/strategies/hedging.html)
- [Polly: Rate limiter](https://www.pollydocs.org/strategies/rate-limiter.html)
