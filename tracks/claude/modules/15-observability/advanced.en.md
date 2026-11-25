---
title: "Observability: advanced"
lang: en
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

# Observability: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Aggregate the logs

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

Watch the share of replies not ending in `end_turn`/`tool_use` (truncations, refusals), cache hit rate, and cost per completed task rather than per request.

## Claude Code telemetry

Claude Code emits metrics and events over OpenTelemetry to **your** collector:

```bash
export CLAUDE_CODE_ENABLE_TELEMETRY=1
export OTEL_METRICS_EXPORTER=otlp
export OTEL_LOGS_EXPORTER=otlp
export OTEL_EXPORTER_OTLP_PROTOCOL=http/json
export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318
claude
```

The check received:

| Metrics | Events |
|---|---|
| `claude_code.session.count`, `claude_code.cost.usage` (USD), `claude_code.token.usage`, `claude_code.active_time.total` | `user_prompt`, `api_request`, `tool_decision`, `tool_result`, `assistant_response`, `plugin_loaded`, `managed_settings_resolved` |

- `api_request` carries model, tokens, `cost_usd`, `duration_ms`, `ttft_ms`.
- `tool_decision` shows what allowed a tool (`decision`, `source`).
- The prompt text is `<REDACTED>` by default and commands and arguments are absent. `OTEL_LOG_USER_PROMPTS=1` and `OTEL_LOG_TOOL_DETAILS=1` send them to the collector.
- The default export interval is 60 s for metrics and 5 s for events.

## Traces (beta)

```bash
export CLAUDE_CODE_ENHANCED_TELEMETRY_BETA=1
export OTEL_TRACES_EXPORTER=otlp
```

The spans received were `claude_code.interaction`, `claude_code.llm_request`, `claude_code.tool`, `claude_code.tool.execution`, `claude_code.tool.blocked_on_user`.

## Organisation usage

```bash
# needs an Admin API credential (sk-ant-admin01-...), not an ordinary API key
curl --globoff "https://api.anthropic.com/v1/organizations/analytics/usage?starting_at=2026-11-01T00:00:00Z" \
  -H "x-api-key: $ANTHROPIC_ADMIN_KEY" -H "anthropic-version: 2026-10-21"
```

The Usage and Cost Analytics API: `/v1/organizations/analytics/usage` and `/analytics/cost`. The older `usage_report/messages` and `cost_report` remain as legacy. It needs admin rights; the Admin API is not available to individual accounts.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [11 · Hooks](../11-skills-and-hooks/advanced.en.md)

## Sources

- [Claude Code monitoring](https://code.claude.com/docs/en/monitoring-usage)
- [Usage and Cost Analytics API](https://platform.claude.com/docs/en/manage-claude/usage-cost-analytics-api)
