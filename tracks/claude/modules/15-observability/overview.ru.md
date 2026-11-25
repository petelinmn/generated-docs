---
title: "Observability"
lang: ru
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

🌐 **Русский** · [English](overview.en.md)

## Коротко

Как видеть, что делает приложение на Claude: **стоимость**, задержка, исходы, телеметрия Claude Code по **OpenTelemetry** и расход организации.

## Ключевые идеи

- `usage` и `request-id` есть в каждом ответе.
- Три вида входных токенов стоят по-разному.
- Claude Code шлёт OTel в ваш сборщик.
- Тексты по умолчанию скрыты.
- Оценка на клиенте — не счёт.

## Минимальный пример

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

## Когда применять и когда нет

С первого дня, когда появились настоящие пользователи или заметные расходы.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude 15-observability level 5` |
| 6. Тесты | — | `/learn claude 15-observability level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [05 · Prompt caching](../05-prompt-caching/overview.ru.md)
- [14 · Batch, evals и надёжность](../14-evals/overview.ru.md)
- [16 · Безопасность и compliance](../16-security/overview.ru.md)
