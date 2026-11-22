---
title: "Quiz: Resilient HttpClient"
lang: en
track: csharp
kind: recipe
doc: quiz
slug: http-client-resilience
part: 2
tags: [httpclient, retry, circuit-breaker, timeout, ihttpclientfactory]
status: draft
updated: 2026-11-22
---

# Quiz: Resilient HttpClient

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [minimal](basics.en.md) · 🟡 [production](advanced.en.md) · 🔴 [scale and edges](expert.en.md).

## 1. 🟢 How should you get an `HttpClient` in an application?

- A) `new HttpClient()` per request
- B) through `IHttpClientFactory` (`AddHttpClient`)
- C) a single static instance forever
- D) through `Activator`

<details><summary>Answer</summary>

**B.** The factory manages connection pools and handler rotation (DNS). `new` per request exhausts sockets, a static client does not pick up DNS changes.
→ [Minimal](basics.en.md#beginner-mistakes)

</details>

## 2. 🟢 How does a call to a server that answers `503` twice and then `200` end with the standard handler?

- A) `503`
- B) `200` after 3 calls
- C) an exception
- D) `200` after 1 call

<details><summary>Answer</summary>

**B.** Verified: two retries, the result `200` after 3 calls; a plain client returned `503` after 1 call.
→ [Minimal](basics.en.md#the-minimal-solution)

</details>

## 3. 🟢 How many retries does the standard handler make by default?

- A) 0
- B) 1
- C) 3
- D) 10

<details><summary>Answer</summary>

**C.** `MaxRetryAttempts = 3`, exponential delay from 2 s with jitter. On a constant `500` verified: 4 calls (1 + 3).
→ [Minimal](basics.en.md#defaults)

</details>

## 4. 🟢 Is a `400` response retried?

- A) yes, 3 times
- B) no, it is returned at once
- C) yes, once
- D) depends on the body size

<details><summary>Answer</summary>

**B.** Transient failures are retried (network errors, timeouts, `5xx`, `408`, `429`); verified: a `400` made one call.
→ [Minimal](basics.en.md#defaults)

</details>

## 5. 🟡 What does the code get when the circuit breaker is open?

- A) an `HttpResponseMessage` with `503`
- B) `BrokenCircuitException`, the request does not reach the server
- C) `TaskCanceledException`
- D) nothing, it hangs

<details><summary>Answer</summary>

**B.** Verified: calls 2–5 — `BrokenCircuitException` with no server hit; after `BreakDuration` a trial call closed the circuit.
→ [Production](advanced.en.md#circuit-breaker)

</details>

## 6. 🟡 What happens with `o.Retry.MaxRetryAttempts = 0` in the standard handler?

- A) retries are disabled
- B) `OptionsValidationException` when the client is created
- C) 1 retry
- D) the handler ignores the value

<details><summary>Answer</summary>

**B.** Allowed from 1 up (verified). To have no retries — your own pipeline through `AddResilienceHandler` without `AddRetry`.
→ [Production](advanced.en.md#a-custom-pipeline)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) retrying `POST` is safe without any measures
- [ ] B) the default `HttpClient.Timeout` is 100 s
- [ ] C) a typed client is registered through `AddHttpClient<T>()`
- [ ] D) a circuit breaker protects a service from extra load when it is down

<details><summary>Answer</summary>

**B, C, D.** A is wrong: without an idempotency key duplicates are possible. The 100 s timeout was verified (`00:01:40`).
→ [Production](advanced.en.md#idempotency-and-retries)

</details>

## 8. 🟡 Open question: how would you organize calling a payment API that sometimes answers `503`?

<details><summary>Key points of a good answer</summary>

- an `Idempotency-Key` per operation, retries with jitter;
- an attempt timeout and a total timeout, a circuit breaker;
- handling `BrokenCircuitException`/`TimeoutRejectedException` and degradation (a "processing" status);
- metrics and logs of retries, alerts on an open circuit.

</details>

## 9. 🔴 Why does the circuit breaker not open in a test although the service always answers `500`?

- A) `Retry-After` is needed
- B) the defaults require at least 100 requests per 30 s — the threshold is not reached
- C) a circuit breaker works only in production
- D) `500` does not count as a failure

<details><summary>Answer</summary>

**B.** In a test lower `MinimumThroughput` and the window (verified: with `MinimumThroughput = 2` the circuit opened).
→ [Scale and edges](expert.en.md#edges-and-gotchas)

</details>

## 10. 🔴 What is dangerous about 100 instances with 3 retries when a dependency fails?

- A) nothing
- B) a retry storm: up to 4× load on an already sick service
- C) retries switch off by themselves
- D) just an extra log line

<details><summary>Answer</summary>

**B.** Protection: jitter, a retry limit, a circuit breaker, `Retry-After`, rate limiting on the service side.
→ [Scale and edges](expert.en.md#scale)

</details>
