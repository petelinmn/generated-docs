---
title: "Observability: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 15-observability
part: 3
tags: [claude, observability, opentelemetry, usage, cost]
versions: "Claude Code 2.1.332; anthropic 2.14.0; Usage and Cost Analytics API"
status: draft
updated: 2026-11-25
---

# Observability: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Telemetry variables

| Variable | Purpose |
|---|---|
| `CLAUDE_CODE_ENABLE_TELEMETRY` | enable (required) |
| `OTEL_METRICS_EXPORTER`, `OTEL_LOGS_EXPORTER`, `OTEL_TRACES_EXPORTER` | `otlp`, `console`, `none`; metrics also `prometheus` |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | `grpc`, `http/json`, `http/protobuf` |
| `OTEL_EXPORTER_OTLP_ENDPOINT`, `..._HEADERS` | endpoint and headers |
| `OTEL_METRIC_EXPORT_INTERVAL`, `OTEL_LOGS_EXPORT_INTERVAL` | milliseconds |
| `OTEL_LOG_USER_PROMPTS`, `OTEL_LOG_ASSISTANT_RESPONSES`, `OTEL_LOG_TOOL_DETAILS`, `OTEL_LOG_TOOL_CONTENT` | reveal content |
| `OTEL_LOG_RAW_API_BODIES` | full request and response bodies, the whole history included |
| `OTEL_METRICS_INCLUDE_SESSION_ID` and others | metric cardinality |

## Further metrics and events per the documentation

`claude_code.lines_of_code.count`, `pull_request.count`, `commit.count`, `code_edit_tool.decision`; `api_error`, `api_refusal`, `safety_event`, `permission_mode_changed`, `auth`, `mcp_server_connection`, `internal_error`, `plugin_installed`.

## Gotchas

1. `DISABLE_TELEMETRY=1` did not stop the export to the local collector.
2. Client-side cost is a list-price estimate (`costBasis: list`); take the bill from the Console or Analytics API.
3. `OTEL_LOG_USER_PROMPTS` sends the texts to the collector, which changes how it must be protected.
4. A short run may end before the export interval; the check lowered intervals to 500 ms.
5. `user.id` and `session.id` raise metric cardinality.

> ⚠️ Verify: the Analytics API, Console dashboards, `grpc` and `http/protobuf`, mTLS and the documentation-only events were not checked.

## Interview questions

1. Which `usage` fields matter for cost?
<details><summary>Answer</summary>

`input_tokens`, `cache_creation_input_tokens`, `cache_read_input_tokens`, `output_tokens`.
</details>

2. What is TTFT?
<details><summary>Answer</summary>

Time to first token; what a user of a streaming UI feels.
</details>

3. Why not just mean latency?
<details><summary>Answer</summary>

The tail (p95, p99) shapes experience; the mean hides it.
</details>

4. Cost per request or per task?
<details><summary>Answer</summary>

Per task: a cheap request needing retries may cost more.
</details>

5. What do you keep out of logs?
<details><summary>Answer</summary>

Secrets, personal data, texts without need and consent.
</details>

6. How do you notice the cache stopped working?
<details><summary>Answer</summary>

`cache_read_input_tokens` drops to zero despite a repeating prefix.
</details>

7. Does `DISABLE_TELEMETRY` stop your OTel export?
<details><summary>Answer</summary>

No: it opts out of operational telemetry to Anthropic; your export has its own variables.
</details>

8. How do you tie app logs to Anthropic support?
<details><summary>Answer</summary>

Keep the `request-id`.
</details>

9. What do you alert on?
<details><summary>Answer</summary>

Error rate, 429/529, refusals and truncations, p95 latency, daily cost.
</details>

10. Observability vs evals?
<details><summary>Answer</summary>

The former shows what happens in production; the latter whether it is good, on known examples.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [16 · Security](../16-security/basics.en.md)

## Sources

- [Claude Code monitoring](https://code.claude.com/docs/en/monitoring-usage)
- [Claude Code data usage](https://code.claude.com/docs/en/data-usage)
