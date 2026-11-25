---
title: "Observability"
lang: en
track: claude
kind: module
doc: overview
slug: 15-observability
part: 3
tags: [claude, observability, opentelemetry, usage, cost]
versions: "Claude Code 2.1.332; anthropic 2.14.0; Usage and Cost Analytics API"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Observability

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

How to see what a Claude application is doing: **cost**, latency, outcomes, Claude Code **OpenTelemetry** and organisation usage.

## Key ideas

- Every response has `usage` and a `request-id`.
- The three kinds of input tokens are priced differently.
- Claude Code sends OTel to your collector.
- Texts are redacted by default.
- A client-side estimate is not the bill.

## Minimal example

```python
# usage_log.py
import json
import time

# USD per million tokens: (input, output, cache read). Check the pricing page before relying on it.
PRICES = {"claude-opus-5-5": (4.00, 20.00, 0.20), "claude-sonnet-5-5": (2.00, 10.00, 0.10)}


def cost_usd(model, input_tokens, output_tokens, cache_read=0, cache_write_5m=0):
    price_in, price_out, price_read = PRICES[model]
    return (input_tokens * price_in + output_tokens * price_out
            + cache_read * price_read + cache_write_5m * price_in * 1.25) / 1_000_000


def logged_create(client, log=print, **params):
    started = time.perf_counter()
    response = client.messages.create(**params)
    u = response.usage
    log(json.dumps({
        "model": response.model, "request_id": response._request_id, "stop_reason": response.stop_reason,
        "latency_ms": round((time.perf_counter() - started) * 1000),
        "input_tokens": u.input_tokens, "output_tokens": u.output_tokens,
        "cache_read": u.cache_read_input_tokens or 0, "cache_write": u.cache_creation_input_tokens or 0,
        "cost_usd": round(cost_usd(response.model, u.input_tokens, u.output_tokens,
                                   u.cache_read_input_tokens or 0, u.cache_creation_input_tokens or 0), 6),
    }))
    return response
```

## When to use it and when not

From the first day with real users or noticeable spend.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude 15-observability level 5` |
| 6. Tests | — | `/learn claude 15-observability level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [05 · Prompt caching](../05-prompt-caching/overview.en.md)
- [14 · Batch, evals and reliability](../14-evals/overview.en.md)
- [16 · Security and compliance](../16-security/overview.en.md)
