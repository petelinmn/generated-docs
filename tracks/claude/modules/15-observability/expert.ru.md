---
title: "Observability: экспертный уровень"
lang: ru
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

# Observability: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Переменные телеметрии

| Переменная | Назначение |
|---|---|
| `CLAUDE_CODE_ENABLE_TELEMETRY` | включить (обязательно) |
| `OTEL_METRICS_EXPORTER`, `OTEL_LOGS_EXPORTER`, `OTEL_TRACES_EXPORTER` | `otlp`, `console`, `none`; метрики ещё `prometheus` |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | `grpc`, `http/json`, `http/protobuf` |
| `OTEL_EXPORTER_OTLP_ENDPOINT`, `..._HEADERS` | адрес и заголовки |
| `OTEL_METRIC_EXPORT_INTERVAL`, `OTEL_LOGS_EXPORT_INTERVAL` | миллисекунды |
| `OTEL_LOG_USER_PROMPTS`, `OTEL_LOG_ASSISTANT_RESPONSES`, `OTEL_LOG_TOOL_DETAILS`, `OTEL_LOG_TOOL_CONTENT` | раскрыть содержимое |
| `OTEL_LOG_RAW_API_BODIES` | полные тела запросов и ответов, включая всю историю |
| `OTEL_METRICS_INCLUDE_SESSION_ID` и другие | кардинальность метрик |

## Ещё метрики и события по документации

`claude_code.lines_of_code.count`, `pull_request.count`, `commit.count`, `code_edit_tool.decision`; `api_error`, `api_refusal`, `safety_event`, `permission_mode_changed`, `auth`, `mcp_server_connection`, `internal_error`, `plugin_installed`.

## Подвохи

1. `DISABLE_TELEMETRY=1` не остановил экспорт в свой сборщик.
2. Стоимость от клиента — оценка по прейскуранту (`costBasis: list`), счёт берите из Console или Analytics API.
3. `OTEL_LOG_USER_PROMPTS` отправляет в сборщик сами тексты: это меняет требования к его защите.
4. Короткий запуск может завершиться раньше интервала экспорта; в проверке интервалы уменьшались до 500 мс.
5. `user.id` и `session.id` повышают кардинальность метрик.

> ⚠️ Проверить: Analytics API, дашборды Console, протоколы `grpc` и `http/protobuf`, mTLS и события из списка по документации не проверялись.

## Вопросы с собеседований

1. Какие поля `usage` важны для стоимости?
<details><summary>Ответ</summary>

`input_tokens`, `cache_creation_input_tokens`, `cache_read_input_tokens`, `output_tokens`.
</details>

2. Что такое TTFT?
<details><summary>Ответ</summary>

Время до первого токена; его чувствует пользователь потокового интерфейса.
</details>

3. Почему не только средняя задержка?
<details><summary>Ответ</summary>

Хвост (p95, p99) определяет опыт; среднее его прячет.
</details>

4. Стоимость на запрос или на задачу?
<details><summary>Ответ</summary>

На задачу: дешёвый запрос с повторами может быть дороже.
</details>

5. Что не писать в логи?
<details><summary>Ответ</summary>

Секреты, персональные данные, тексты без необходимости и согласия.
</details>

6. Как заметить, что кэш перестал работать?
<details><summary>Ответ</summary>

`cache_read_input_tokens` падает к нулю при повторяющемся префиксе.
</details>

7. Отключает ли `DISABLE_TELEMETRY` ваш экспорт OTel?
<details><summary>Ответ</summary>

Нет: он отключает операционную телеметрию в сторону Anthropic; экспорт управляется своими переменными.
</details>

8. Как связать логи приложения с поддержкой Anthropic?
<details><summary>Ответ</summary>

Сохранять `request-id`.
</details>

9. Что алертить?
<details><summary>Ответ</summary>

Долю ошибок, 429/529, отказы и обрезки, p95 задержки, дневную стоимость.
</details>

10. Наблюдаемость и eval?
<details><summary>Ответ</summary>

Первая показывает, что происходит в работе; вторые — хорошо ли это, на известных примерах.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [16 · Безопасность](../16-security/basics.ru.md)

## Источники

- [Claude Code monitoring](https://code.claude.com/docs/en/monitoring-usage)
- [Claude Code data usage](https://code.claude.com/docs/en/data-usage)
