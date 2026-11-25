---
title: "Observability: основы"
lang: ru
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

# Observability: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Messages API](../02-messages-api/overview.ru.md), [14 · Evals](../14-evals/overview.ru.md)

## Как проверялся материал

Функция логирования запускалась с SDK `anthropic` 2.14.0 против локальной заглушки. Телеметрия Claude Code 2.1.332 проверена по-настоящему: она отправлялась в локальный OTLP-приёмник. Analytics API требует ключа администратора и не вызывался.

## Что наблюдать

| Вопрос | Откуда ответ |
|---|---|
| сколько стоит | `usage` |
| насколько быстро | время запроса и до первого токена |
| чем закончилось | `stop_reason`, код ошибки |
| какой это был запрос | `request-id` |

## Записывать каждый запрос

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

Каждая строка — один JSON: такие логи легко искать и суммировать. Тексты промптов и ответов здесь намеренно не пишутся: в них могут быть персональные данные.

Все входные токены складываются из трёх полей: `input_tokens` (после последней точки кэша), `cache_creation_input_tokens` и `cache_read_input_tokens`. Они стоят по-разному.

Цены в коде — снимок на момент подготовки (USD за миллион токенов); они меняются, а модели с длинным контекстом могут иметь свои правила.

## Console

Расход и стоимость по организации видны в Claude Console без своего кода.

## Частые ошибки новичков

- Считать только `input_tokens` и терять кэшированную часть.
- Не сохранять `request-id`.
- Писать в лог промпты и ключи.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [05 · Prompt caching](../05-prompt-caching/overview.ru.md)

## Источники

- [Prompt caching](https://platform.claude.com/docs/en/build-with-claude/prompt-caching)
- [Errors: request id](https://platform.claude.com/docs/en/api/errors)
