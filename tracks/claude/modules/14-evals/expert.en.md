---
title: "Batch, evals and reliability: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 14-evals
part: 3
tags: [claude, evals, batch-api, retries, reliability]
versions: "Claude 5.5 family; anthropic 2.14.0; Message Batches API"
status: draft
updated: 2026-11-25
---

# Batch, evals and reliability: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Error codes

| Code | `type` |
|---|---|
| 400 | `invalid_request_error` (also your own spend limit) |
| 401 | `authentication_error` |
| 402 | `billing_error` |
| 403 | `permission_error` |
| 404 | `not_found_error` |
| 409 | `conflict_error` |
| 413 | `request_too_large` |
| 429 | `rate_limit_error` |
| 500 | `api_error` |
| 504 | `timeout_error` |
| 529 | `overloaded_error` |

## Rate limits

RPM, ITPM and OTPM per model class, token bucket: capacity refills continuously, and a short burst can still exceed it. ITPM counts `input_tokens` and `cache_creation_input_tokens` but not `cache_read_input_tokens`. `max_tokens` does not affect OTPM.

## More on batches

Cancelling moves to `canceling`. The `output-300k-2026-03-24` beta raises batch output to 300k on several models. The batch discount combines with prompt caching to the extent the cache hits.

## Gotchas

1. A refusal raises nothing and `content` is empty: `response.content[0]` crashes.
2. With defaults one call against an overloaded API is three requests.
3. Your retries on top of the SDK's multiply.
4. Batch result order does not match request order.
5. The SDK does not retry an error that arrives inside a stream (per the documentation).

> ⚠️ Verify: limits, prices, service error texts, `fallbacks`, batch cancellation and extended output were not exercised live.

## Interview questions

1. Which errors do you retry?
<details><summary>Answer</summary>

Network, 408, 409, 429, 5xx subject to `x-should-retry`; not 400, 401, 403, 404.
</details>

2. Why is a 429 not always worth retrying?
<details><summary>Answer</summary>

A tier spend-cap 429 has no `retry-after` and keeps failing until access resumes.
</details>

3. How do you raise input throughput?
<details><summary>Answer</summary>

Cache reads do not count towards ITPM, so caching helps.
</details>

4. What do you give support?
<details><summary>Answer</summary>

The `request-id` header or `request_id`.
</details>

5. When batch?
<details><summary>Answer</summary>

No user waiting, large volume, up to a day of delay acceptable.
</details>

6. How do you match batch results?
<details><summary>Answer</summary>

By `custom_id` only.
</details>

7. How do you detect a refusal?
<details><summary>Answer</summary>

By `stop_reason`, not the HTTP status.
</details>

8. How do you avoid overfitting to an eval?
<details><summary>Answer</summary>

Split into a tuning set and a held-out set.
</details>

9. What besides accuracy?
<details><summary>Answer</summary>

Latency, cost, refusal and truncation rates, run-to-run variance, failures by category.
</details>

10. How do you catch a regression on a model change?
<details><summary>Answer</summary>

Run the same set on both and compare case by case, not just the average.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [15 · Observability](../15-observability/basics.en.md)

## Sources

- [Rate limits](https://platform.claude.com/docs/en/api/rate-limits)
- [Errors](https://platform.claude.com/docs/en/api/errors)
