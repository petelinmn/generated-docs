---
title: "Observability: fundamentals"
lang: en
track: claude
kind: module
doc: basics
slug: 15-observability
part: 3
tags: [claude, observability, opentelemetry, usage, cost]
versions: "Claude Code 2.1.332; anthropic 2.14.0; Usage and Cost Analytics API"
status: draft
updated: 2026-11-25
---

# Observability: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Messages API](../02-messages-api/overview.en.md), [14 · Evals](../14-evals/overview.en.md)

## How this was checked

The logging function ran with the `anthropic` 2.14.0 SDK against a local stand-in. Claude Code 2.1.332 telemetry was checked for real: it was sent to a local OTLP receiver. The Analytics API needs an admin key and was not called.

## What to watch

| Question | Where from |
|---|---|
| what does it cost | `usage` |
| how fast | request time and time to first token |
| how did it end | `stop_reason`, error status |
| which request was it | `request-id` |

## Log every request

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

Each line is one JSON object: easy to search and aggregate. Prompt and reply texts are deliberately not logged: they may hold personal data.

Total input is three fields: `input_tokens` (after the last cache breakpoint), `cache_creation_input_tokens` and `cache_read_input_tokens`. They are priced differently.

The prices in the code are a snapshot (USD per million tokens); they change, and some models price long prompts differently.

## Console

Organisation usage and cost are visible in the Claude Console with no code of your own.

## Common beginner mistakes

- Counting `input_tokens` only and losing the cached part.
- Not keeping the `request-id`.
- Logging prompts and keys.

## Related materials

- [Advanced theory](advanced.en.md)
- [05 · Prompt caching](../05-prompt-caching/overview.en.md)

## Sources

- [Prompt caching](https://platform.claude.com/docs/en/build-with-claude/prompt-caching)
- [Errors: request id](https://platform.claude.com/docs/en/api/errors)
