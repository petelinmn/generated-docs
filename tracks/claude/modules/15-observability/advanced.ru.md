---
title: "Observability: продвинутый уровень"
lang: ru
track: claude
kind: module
doc: advanced
slug: 15-observability
part: 3
tags: [claude, observability, opentelemetry, usage, cost]
versions: "Claude Code 2.1.332; anthropic 2.14.0; Usage and Cost Analytics API"
status: draft
updated: 2026-11-25
---

# Observability: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Свести логи

```python
# summary.py
import json
from collections import defaultdict


def summarise(lines):
    totals = defaultdict(lambda: {"requests": 0, "cost_usd": 0.0, "not_end_turn": 0, "latencies": []})
    for line in lines:
        r = json.loads(line)
        t = totals[r["model"]]
        t["requests"] += 1
        t["cost_usd"] += r["cost_usd"]
        t["not_end_turn"] += r["stop_reason"] not in ("end_turn", "tool_use")
        t["latencies"].append(r["latency_ms"])
    return {model: {"requests": t["requests"], "cost_usd": round(t["cost_usd"], 6), "not_end_turn": t["not_end_turn"],
                    "max_latency_ms": max(t["latencies"])} for model, t in totals.items()}
```

Следите за долей ответов, закончившихся не `end_turn`/`tool_use` (обрезки, отказы), за попаданиями в кэш и за стоимостью на выполненную задачу, а не на запрос.

## Телеметрия Claude Code

Claude Code отдаёт метрики и события по OpenTelemetry — в **ваш** сборщик:

```bash
export CLAUDE_CODE_ENABLE_TELEMETRY=1
export OTEL_METRICS_EXPORTER=otlp
export OTEL_LOGS_EXPORTER=otlp
export OTEL_EXPORTER_OTLP_PROTOCOL=http/json
export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318
claude
```

В проверке пришли:

| Метрики | События |
|---|---|
| `claude_code.session.count`, `claude_code.cost.usage` (USD), `claude_code.token.usage`, `claude_code.active_time.total` | `user_prompt`, `api_request`, `tool_decision`, `tool_result`, `assistant_response`, `plugin_loaded`, `managed_settings_resolved` |

- `api_request` несёт модель, токены, `cost_usd`, `duration_ms`, `ttft_ms`.
- `tool_decision` показывает, кто разрешил инструмент (`decision`, `source`).
- Текст промпта по умолчанию `<REDACTED>`, команд и аргументов нет. Включают `OTEL_LOG_USER_PROMPTS=1` и `OTEL_LOG_TOOL_DETAILS=1` — тогда они уходят в сборщик.
- Интервал экспорта метрик по умолчанию 60 с, событий 5 с.

## Трассы (бета)

```bash
export CLAUDE_CODE_ENHANCED_TELEMETRY_BETA=1
export OTEL_TRACES_EXPORTER=otlp
```

Пришли спаны `claude_code.interaction`, `claude_code.llm_request`, `claude_code.tool`, `claude_code.tool.execution`, `claude_code.tool.blocked_on_user`.

## Расход по организации

```bash
# needs an Admin API credential (sk-ant-admin01-...), not an ordinary API key
curl --globoff "https://api.anthropic.com/v1/organizations/analytics/usage?starting_at=2026-11-01T00:00:00Z" \
  -H "x-api-key: $ANTHROPIC_ADMIN_KEY" -H "anthropic-version: 2026-10-21"
```

Usage and Cost Analytics API: `/v1/organizations/analytics/usage` и `/analytics/cost`. Старые `usage_report/messages` и `cost_report` поддерживаются как legacy. Нужны права администратора; для индивидуальных аккаунтов Admin API недоступен.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [11 · Hooks](../11-skills-and-hooks/advanced.ru.md)

## Источники

- [Claude Code monitoring](https://code.claude.com/docs/en/monitoring-usage)
- [Usage and Cost Analytics API](https://platform.claude.com/docs/en/manage-claude/usage-cost-analytics-api)
