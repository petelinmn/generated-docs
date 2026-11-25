---
title: "Observability: exercises"
lang: en
track: claude
kind: module
doc: exercises
slug: 15-observability
part: 3
tags: [claude, observability, opentelemetry, usage, cost]
versions: "Claude Code 2.1.332; anthropic 2.14.0; Usage and Cost Analytics API"
status: draft
updated: 2026-11-25
---

# Observability: exercises

🌐 [Русский](exercises.ru.md) · **English**

> You need `usage_log.py` and `summary.py` from the theory; everything runs without an API key.

## 1. Cost of a request

Compute the cost of 1000 input and 500 output tokens on Opus 5.5.

<details><summary>Hint</summary>

`cost_usd`.

</details>

<details><summary>Solution</summary>

```python
from usage_log import cost_usd

print(cost_usd("claude-opus-5-5", 1000, 500))
```

</details>

## 2. Cache savings

Compare 100,000 tokens as plain input and as cache reads.

<details><summary>Hint</summary>

The `cache_read` parameter.

</details>

<details><summary>Solution</summary>

```python
from usage_log import cost_usd

print(cost_usd("claude-opus-5-5", 100_000, 0), cost_usd("claude-opus-5-5", 0, 0, cache_read=100_000))
```

</details>

## 3. A summary

Aggregate two log lines.

<details><summary>Hint</summary>

`summarise`.

</details>

<details><summary>Solution</summary>

```python
import json

from summary import summarise

lines = [json.dumps({"model": "claude-opus-5-5", "cost_usd": 0.01, "stop_reason": s, "latency_ms": ms})
         for s, ms in (("end_turn", 800), ("max_tokens", 4000))]
print(summarise(lines))
```

</details>

## 4. An unknown model

Make sure an unknown model does not crash the app because of logging.

<details><summary>Hint</summary>

Return `None`.

</details>

<details><summary>Solution</summary>

```python
from usage_log import PRICES, cost_usd


def safe_cost(model, *args):
    return cost_usd(model, *args) if model in PRICES else None

print(safe_cost("claude-unknown", 1, 1))
```

</details>

## 5. Telemetry to the console

Look at Claude Code events without a collector.

<details><summary>Hint</summary>

The `console` exporter.

</details>

<details><summary>Solution</summary>

```bash
CLAUDE_CODE_ENABLE_TELEMETRY=1 OTEL_LOGS_EXPORTER=console OTEL_LOGS_EXPORT_INTERVAL=1000 claude
```

</details>
