---
title: "Rate limiting: scale and edges"
lang: en
track: csharp
kind: recipe
doc: expert
slug: rate-limiting
part: 2
tags: [rate-limiting, distributed-rate-limiting, redis, api-gateway, token-bucket, interview-questions]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# Rate limiting: scale and edges

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · [2. Production](advanced.en.md) · **3. Scale and edges** · [4. Quiz](quiz.en.md)

**Prerequisites:** [minimal](basics.en.md) and [production](advanced.en.md). **Time:** ~30 min.

## Scale: several instances

The built-in limiters keep counters **in process memory**. With N instances behind a load balancer a client's real limit is up to N × the limit (or less with sticky sessions). Solutions:

- **A limit at the entry:** an API gateway/balancer/CDN (nginx, Envoy, YARP, cloud gateways) counts centrally; the application is a fallback layer.
- **A shared counter store** (Redis with atomic operations/scripts): an exact limit, but an extra network call per request and a new point of failure — decide what to do when the store is unavailable (fail-open or fail-closed).
- **Dividing the limit** among instances (limit/N): approximate and simple.

The built-in middleware gives only local protection for an instance — enough as a "fuse" against overload, but not as an exact client quota.

## Edges and gotchas

**1. The fixed-window boundary.** A client can make N requests at the end of one window and N at the start of the next — 2N in a short time (sliding window/token bucket smooth this).

**2. The order of parallel requests.** Of 5 simultaneous requests with a limit of 3, two were rejected, but which ones is undefined (verified: `429 200 200 429 200`).

**3. An IP behind a proxy.** Without `UseForwardedHeaders` all clients have the proxy's address and share one limit.

**4. Partitions grow.** Each key is a separate limiter in memory; millions of unique keys (random values from an attacker) are a memory leak; limit the keys (authenticated users) and tie them to a TTL.

**5. A queue holds resources.** A large `QueueLimit` occupies connections/threads.

**6. A 429 must not break infrastructure.** Exclude health checks and metrics from limits.

**7. Requests do not cost the same.** One expensive request is better counted as several "tokens" (a token bucket with a variable number of tokens) or a separate policy.

**8. The client side.** Clients must respect `Retry-After` and use exponential backoff with jitter (see [Resilient HttpClient](../http-client-resilience/overview.en.md)).

## Alternatives and complements

| Approach | What for |
|---|---|
| Built-in `AddRateLimiter` | local protection of an instance |
| An API gateway (YARP, Envoy, cloud) | centralized quotas, API keys |
| A Redis limiter | exact distributed limits |
| Caching | fewer backend requests without rejections |
| Backpressure (queues, `Channel<T>`) | smoothing inside the system |
| Bulkhead (pool isolation) | one part's failure does not sink the rest |

## Interview questions

<details><summary>1. Why is rate limiting needed?</summary>

Protection from brute force, abuse and overload; a fair distribution of resources; honoring plan quotas.

</details>

<details><summary>2. Which algorithms do you know and how do they differ?</summary>

Fixed window (simple, a burst at the boundary), sliding window (smooths), token bucket (allows a burst with a limited average rate), concurrency (limiting simultaneous), leaky bucket (a constant rate).

</details>

<details><summary>3. Which response code and headers on exceeding?</summary>

429 Too Many Requests and `Retry-After` (when to retry). Verified: a response with `Retry-After`.

</details>

<details><summary>4. How do you limit per user?</summary>

Through partitioning (`RateLimitPartition`) by a key — a user, an API key, an IP; each key has its own counter (verified: `ann` rejected, `bob` passes).

</details>

<details><summary>5. What is the problem with in-memory limits with several instances?</summary>

Each instance counts separately: a client's total limit is up to N × the limit. You need a central gateway or a shared store (Redis).

</details>

<details><summary>6. Fixed window and sliding window: what is the difference?</summary>

Fixed counts in fixed windows and allows up to 2N at the boundary; sliding splits the window into segments and shifts it — a smoother limit.

</details>

<details><summary>7. How does a token bucket work?</summary>

A bucket of capacity B is refilled at rate r; a request takes a token. It allows a burst up to B but the average rate is ≤ r (verified at capacity 2, +1/s).

</details>

<details><summary>8. What is a concurrency limiter for?</summary>

It limits the number of simultaneous requests (heavy operations). Verified: of three simultaneous slow requests one passed.

</details>

<details><summary>9. Trick question: the limit is 3 per second, yet a client makes 6 requests in a short time with no rejections. How?</summary>

The fixed-window boundary (3 at the end of the window + 3 at the start of the next) or several instances with independent counters.

</details>

<details><summary>10. Trick question: all users behind a corporate proxy share one limit. Why?</summary>

The key is the IP, and it is the same for all; you need a per-user key or correct forwarded headers.

</details>

## Related materials

- Back: [production](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [Resilient HttpClient](../http-client-resilience/overview.en.md) — the client side.
- [21 · Application architecture](../../modules/21-architecture/overview.en.md) — gateways and service boundaries.
- [Kubernetes](../../../kubernetes/overview.en.md) — limits at the ingress level.

## Sources

- [Rate limiting middleware](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [RFC 6585: status 429](https://www.rfc-editor.org/rfc/rfc6585#section-4)
- [YARP: rate limiting](https://microsoft.github.io/reverse-proxy/articles/rate-limiting.html)
