---
title: "Polly: expert"
lang: en
track: csharp
kind: library
doc: expert
slug: polly
part: 2
tags: [polly, resilience, chaos-engineering, circuit-breaker, idempotency, interview-questions]
versions: ".NET 11, C# 15; Polly 8.9.1"
status: draft
updated: 2026-11-22
---

# Polly: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md) and [advanced](advanced.en.md). **Time:** ~30 min.

## Internals

A `ResiliencePipeline` is a chain of "strategies" (middleware): each receives the call and the "next" delegate, performs logic before/after and decides whether to call the next one. It is the same idea as the `HttpClient` handler pipeline. Features of Polly 8:

- a pipeline is **immutable** after `Build()`, thread-safe and meant to be reused;
- an operation's result (a value or an exception) is represented as an `Outcome<T>`; `ShouldHandle` (`PredicateBuilder`) decides whether an outcome counts as a failure;
- a call has a `ResilienceContext` (a cancellation token, properties) from the `ResilienceContextPool`;
- strategies are tuned for low allocations (`ValueTask`, `struct` arguments).

> ⚠️ Verify: implementation details (internal types, pools, allocation benchmarks) were not investigated in this material; check the Polly sources and documentation.

## Edges and gotchas

**1. The timeout is cooperative.** Verified: an operation with `Thread.Sleep(300)` under a 100 ms timeout finished normally in ~300 ms — the token was ignored.

**2. The order of strategies.** Verified: `retry` outside, `timeout` inside — 3 body runs; the reverse order gives one shared timeout.

**3. The circuit breaker's state lives in the pipeline.** A new pipeline per call — the breaker never trips. A pipeline is a singleton/DI.

**4. The breaker and thresholds.** `MinimumThroughput` calls per `SamplingDuration` are needed (verified: with 2 calls the circuit opened; with the defaults more traffic is required).

**5. `BrokenCircuitException` is not the operation.** With an open circuit the body does not run (verified: the call counter does not grow).

**6. Retry and side effects.** Repeating a non-idempotent operation creates duplicates.

**7. Hedging multiplies load.** Verified: 2 calls were started for one answer; for idempotent and latency-expensive requests.

**8. Generic and non-generic pipelines.** `ResiliencePipelineBuilder<T>` is needed for `Fallback` with a result; at the call an explicit `ExecuteAsync<T>` is sometimes required (otherwise CS0411, verified).

**9. `RateLimiter` is a separate package.** `Polly.RateLimiting` (and `System.Threading.RateLimiting`); without it — a compile error (verified).

**10. Exceptions in `OnRetry`.** Event handlers must not throw — they affect pipeline execution.

> ⚠️ Verify: the behavior of exceptions inside strategy event handlers was not checked in this material.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Polly 8 | a rich set of strategies, pipelines, DI, telemetry | you must understand order and state |
| `Microsoft.Extensions.Http.Resilience` | ready settings for `HttpClient` | HTTP only |
| A hand-written retry loop | no dependency | easy to get wrong, no breaker |
| A service mesh | outside the application | does not know business semantics |
| A queue + scheduled retry | reliable, asynchronous | a different architecture |

## Interview questions

<details><summary>1. What is Polly and which strategies does version 8 have?</summary>

A resilience library: retry, timeout, circuit breaker, fallback, hedging, rate limiter; assembled into a `ResiliencePipeline`.

</details>

<details><summary>2. How does retry with exponential delay and jitter work?</summary>

The delay grows by a power (20 → 40 → 80 ms, verified without jitter); jitter adds a random spread so clients do not retry in sync.

</details>

<details><summary>3. What happens when retry attempts are exhausted?</summary>

The last exception is propagated (verified: after 4 attempts).

</details>

<details><summary>4. How does a circuit breaker work?</summary>

Closed → with a sufficient failure ratio Open (calls are rejected without running), after `BreakDuration` Half-Open with a trial call (verified in the example).

</details>

<details><summary>5. Why does a timeout not always interrupt an operation?</summary>

It is cooperative: it cancels a token; code that ignores the token is not interrupted (verified).

</details>

<details><summary>6. How does the order of strategies affect behavior?</summary>

The first is the outer one: `retry`→`timeout` gives a timeout per attempt, `timeout`→`retry` — a shared one.

</details>

<details><summary>7. How do you use Polly with DI?</summary>

`AddResiliencePipeline("name", …)` and `ResiliencePipelineProvider<string>.GetPipeline("name")` (verified).

</details>

<details><summary>8. What is hedging and when is it dangerous?</summary>

Running duplicate calls in parallel when the first is slow; it loads the service, only for idempotent operations (verified: 2 calls).

</details>

<details><summary>9. Trick question: we create a `ResiliencePipeline` inside a method per call — the breaker never opens. Why?</summary>

The breaker's state belongs to the pipeline instance; a new pipeline — new state. You need a singleton.

</details>

<details><summary>10. Trick question: `await fb.ExecuteAsync(async ct => { throw … })` does not compile (CS0411). Why?</summary>

For `ResiliencePipeline<T>` the result type cannot be inferred from a lambda that only throws; specify `ExecuteAsync<string>(…)`.

</details>

<details><summary>11. How do you get a result without exceptions?</summary>

`ExecuteOutcomeAsync` returns an `Outcome<T>` with a `Result` or an `Exception` (verified).

</details>

## Related materials

- Back: [advanced](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [21 · Application architecture](../../modules/21-architecture/overview.en.md) — cascading failures and bulkhead.
- [22 · Observability](../../modules/22-observability/overview.en.md) — resilience metrics.
- [Kubernetes](../../../kubernetes/overview.en.md) — resilience at the infrastructure level.

## Sources

- [Polly docs](https://www.pollydocs.org/)
- [Polly: Resilience context](https://www.pollydocs.org/advanced/resilience-context.html)
- [Polly: Telemetry](https://www.pollydocs.org/advanced/telemetry.html)
