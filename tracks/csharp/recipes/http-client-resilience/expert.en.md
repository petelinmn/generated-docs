---
title: "Resilient HttpClient: scale and edges"
lang: en
track: csharp
kind: recipe
doc: expert
slug: http-client-resilience
part: 2
tags: [httpclient, retry-storm, hedging, bulkhead, dns, interview-questions]
versions: ".NET 11, C# 15; Microsoft.Extensions.Http.Resilience 11.0.1"
status: draft
updated: 2026-11-22
---

# Resilient HttpClient: scale and edges

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · [2. Production](advanced.en.md) · **3. Scale and edges** · [4. Quiz](quiz.en.md)

**Prerequisites:** [minimal](basics.en.md) and [production](advanced.en.md). **Time:** ~30 min.

## Scale

- **A retry storm.** 100 instances × 3 retries = up to 4× load on an already sick service. Protection: jitter, a retry limit, a circuit breaker, honoring `Retry-After` and (on the service side) [rate limiting](../rate-limiting/overview.en.md).
- **Cascading failures.** Timeouts add up: if A waits for B for 30 s and B waits for C for 30 s, a thread is held for minutes. The time budget of the whole chain must be smaller than the caller's.
- **Bulkhead.** Isolate pools: a separate client and concurrency limit per dependency, so one slow one does not eat all threads/connections.
- **Hedging.** For latency-sensitive idempotent calls — a second request to another instance if the first is slow to answer (`AddStandardHedgingHandler`).

> ⚠️ Verify: `AddStandardHedgingHandler` and its settings were not run in this material — check the documentation before using it.

## Edges and gotchas

**1. `MaxRetryAttempts = 0` is forbidden.** Verified: `OptionsValidationException` ("must be between 1 and …") when the client is first created, not at registration.

**2. The circuit breaker "does not trip" in tests.** The defaults (`MinimumThroughput = 100`, a 30 s window) need a hundred requests; lower the thresholds in tests (verified: with `MinimumThroughput = 2` the circuit opened after the first call with two attempts).

**3. An exception instead of a response.** With an open circuit — `BrokenCircuitException`, on a timeout — `TimeoutRejectedException` (verified), not an `HttpResponseMessage`. Code must catch them, not only look at the status.

**4. The final response after retries are exhausted.** Verified: on a constant `500` the last `500` was returned (4 calls), no exception — `EnsureSuccessStatusCode` is up to you.

**5. Most `4xx` are not retried.** Verified: `400` and `404` — one call, but `408` and `429` are retried (4 calls each). For `429` honor `Retry-After` and do not worsen the overload.

**6. Handler lifetime.** The factory recreates the `HttpMessageHandler` (every 2 minutes by default) to pick up DNS; a client captured by a singleton loses this.

**7. `HttpClient.Timeout` and the pipeline.** The default timeout is 100 s (verified): if it is less than the sum of attempts with delays, the client cuts retries off before the pipeline does.

**8. Re-reading the body.** A request body with `StreamContent` cannot be sent twice; for retries use buffered content.

**9. Retrying `POST` without idempotency.** Duplicate orders/payments ([production](advanced.en.md#idempotency-and-retries)).

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `AddStandardResilienceHandler` | ready sensible values | generic, tune to the service |
| `AddResilienceHandler` | full control, Polly v8 | more code |
| Manual Polly (`ResiliencePipeline`) | not only HTTP | no out-of-the-box factory integration |
| A service mesh (Istio, Linkerd) | retries/CB in the infrastructure | does not see business semantics |
| A queue instead of a synchronous call | a failure is invisible to the user | asynchrony, [background jobs](../background-jobs/overview.en.md) |

## Interview questions

<details><summary>1. Why not create `new HttpClient()` per request?</summary>

Each instance holds its own connection pool: with frequent creation sockets stay in `TIME_WAIT`, ports may be exhausted. Use `IHttpClientFactory`.

</details>

<details><summary>2. And why is a single static `HttpClient` bad?</summary>

Connections live forever and do not pick up a DNS change. The factory rotates handlers; the alternative is `SocketsHttpHandler.PooledConnectionLifetime`.

</details>

<details><summary>3. What does the standard resilience handler include?</summary>

A total timeout, retries, a circuit breaker, an attempt timeout (and a concurrency limiter). Defaults verified: 3 retries, exponential with jitter from 2 s, attempt timeout 10 s, total 30 s.

</details>

<details><summary>4. Which responses are retried by default?</summary>

Network exceptions, timeouts, `5xx`, `408`, `429`. `400` and `404` are not retried (verified).

</details>

<details><summary>5. How does a circuit breaker work?</summary>

Closed → when the failure ratio exceeds the threshold at enough traffic it goes Open: calls are rejected at once; after `BreakDuration` — Half-Open: a trial call; success closes the circuit. Verified in practice.

</details>

<details><summary>6. What does the code get with an open circuit?</summary>

`BrokenCircuitException`; the request does not reach the server (verified).

</details>

<details><summary>7. Why jitter?</summary>

So that clients do not retry in sync (a thundering herd) — a random spread of delays.

</details>

<details><summary>8. Is it safe to retry `POST`?</summary>

Only with an idempotency key: otherwise duplicates when the server executed the request but the response was lost.

</details>

<details><summary>9. Trick question: `MaxRetryAttempts = 0` to disable retries in the standard handler. What happens?</summary>

`OptionsValidationException` when the client is created (verified). You need your own pipeline without `AddRetry`.

</details>

<details><summary>10. Trick question: in a test you cannot open the circuit breaker although the service always answers `500`. Why?</summary>

The defaults (at least 100 requests per 30 s, a 10% failure ratio) are not reached; in a test set a lower `MinimumThroughput` and a smaller window.

</details>

## Related materials

- Back: [production](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [21 · Application architecture](../../modules/21-architecture/overview.en.md) — service boundaries and cascading failures.
- [Rate limiting](../rate-limiting/overview.en.md) — the server side of `429` and `Retry-After`.
- [Kubernetes](../../../kubernetes/overview.en.md) — a service mesh and retries at the infrastructure level.

## Sources

- [Build resilient HTTP apps](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- [Polly: hedging](https://www.pollydocs.org/strategies/hedging.html)
- [Guidelines for using HttpClient](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines)
