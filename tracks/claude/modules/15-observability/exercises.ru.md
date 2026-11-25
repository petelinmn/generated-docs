---
title: "Observability: упражнения"
lang: ru
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

# Observability: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Нужны `usage_log.py` и `summary.py` из теории; всё выполняется без ключа API.

## 1. Стоимость запроса

Посчитайте стоимость 1000 входных и 500 выходных токенов на Opus 5.5.

<details><summary>Подсказка</summary>

`cost_usd`.

</details>

<details><summary>Решение</summary>

```python
from usage_log import cost_usd

print(cost_usd("claude-opus-5-5", 1000, 500))
```

</details>

## 2. Выгода кэша

Сравните 100 000 токенов обычного входа и чтения из кэша.

<details><summary>Подсказка</summary>

Параметр `cache_read`.

</details>

<details><summary>Решение</summary>

```python
from usage_log import cost_usd

print(cost_usd("claude-opus-5-5", 100_000, 0), cost_usd("claude-opus-5-5", 0, 0, cache_read=100_000))
```

</details>

## 3. Сводка

Сведите две строки лога.

<details><summary>Подсказка</summary>

`summarise`.

</details>

<details><summary>Решение</summary>

```python
import json

from summary import summarise

lines = [json.dumps({"model": "claude-opus-5-5", "cost_usd": 0.01, "stop_reason": s, "latency_ms": ms})
         for s, ms in (("end_turn", 800), ("max_tokens", 4000))]
print(summarise(lines))
```

</details>

## 4. Неизвестная модель

Сделайте так, чтобы неизвестная модель не роняла приложение из-за логирования.

<details><summary>Подсказка</summary>

Верните `None`.

</details>

<details><summary>Решение</summary>

```python
from usage_log import PRICES, cost_usd


def safe_cost(model, *args):
    return cost_usd(model, *args) if model in PRICES else None

print(safe_cost("claude-unknown", 1, 1))
```

</details>

## 5. Телеметрия в консоль

Посмотрите события Claude Code без сборщика.

<details><summary>Подсказка</summary>

Экспортёр `console`.

</details>

<details><summary>Решение</summary>

```bash
CLAUDE_CODE_ENABLE_TELEMETRY=1 OTEL_LOGS_EXPORTER=console OTEL_LOGS_EXPORT_INTERVAL=1000 claude
```

</details>
