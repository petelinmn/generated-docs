---
title: "Claude"
lang: ru
track: claude
kind: track
doc: overview
slug: claude
tags: [claude, anthropic, messages-api, claude-code, tool-use]
versions: "Claude 5.5 family; Claude Code, 2026-11"
default_level: 4
status: draft
updated: 2026-11-23
---

# Claude

🌐 **Русский** · [English](overview.en.md)

## Коротко

Claude — семейство языковых моделей Anthropic для анализа текста, изображений и документов, программирования и agentic-работы. Трек охватывает два связанных направления: разработку приложений через Claude Platform API и автономную работу Claude Code в терминале и редакторе.

Основные интерфейсы трека — Messages API, tool use, structured outputs и Claude Code. Подход к обучению строится от первой синхронной или потоковой генерации до инструментов, MCP, навыков (skills), hooks и managed agents.

**Пререквизиты:** базовое программирование и основы HTTP; для SDK-примеров достаточно [трека Python](../python/overview.ru.md).

## Окружение

| Что | Инструмент |
|---|---|
| Язык примеров | Python 3.12+ с официальным Anthropic SDK и `uv` |
| Альтернативные SDK | TypeScript/JavaScript, C#, Java, Go и другие официальные SDK |
| Доступ к API | `ANTHROPIC_API_KEY` в переменной окружения или `.env` |
| Claude Code | терминал, VS Code, JetBrains, Desktop или web |
| Код модуля | как в треке Python: `code/` с `pyproject.toml`, `examples/`, `solutions/`, `tests/` |
| Запустить все тесты модуля | `uv run pytest` в папке `code/` |
| Тесты и API | API-вызовы заменяются фейком или мок-сервером; интеграционные тесты пропускаются без ключа |

## Дорожная карта

Прогресс изучения — в [PROGRESS.md](../../PROGRESS.md). Модули генерируются с уровнем детализации по умолчанию **4** (теория и самопроверка), см. [уровни детализации](../../README.ru.md#уровни-детализации).

### Часть 1. Основы

- [01 · Семейство Claude и выбор модели](modules/01-models/overview.ru.md) — модель, контекст, параметры вывода, стоимости и lifecycle
- [02 · Messages API](modules/02-messages-api/overview.ru.md) — сообщения, system prompt, блоки контента, стриминг и stop reason
- [03 · Prompt engineering](modules/03-prompt-engineering/overview.ru.md) — роли, структура инструкции, примеры, XML-разметка и thinking
- [04 · Structured outputs](modules/04-structured-output/overview.ru.md) — JSON-схемы, валидация и извлечение данных
- [05 · Prompt caching](modules/05-prompt-caching/overview.ru.md) — кэшируемый префикс, breakpoints и диагностика cache miss
- [06 · Работа с документами](modules/06-documents-and-citations/overview.ru.md) — PDF, изображения, файлы, citations и контекстные ограничения

### Часть 2. Продвинутые темы

- [07 · Tool use](modules/07-tool-use/overview.ru.md) — client tools, server tools, tool_choice и цикл tool_result
- [08 · MCP и connectors](modules/08-mcp/overview.ru.md) — MCP-серверы, туннели, remote connectors и Protocol SDK
- [09 · Claude Code](modules/09-claude-code/overview.ru.md) — команды, редакторы, терминал, CI и работа с репозиторием
- [10 · CLAUDE.md и память](modules/10-memory/overview.ru.md) — инструкции проекта, auto memory и управление контекстом
- [11 · Skills и hooks](modules/11-skills-and-hooks/overview.ru.md) — reusable workflows, автоматическая активация и lifecycle hooks
- [12 · Агенты и subagents](modules/12-agents/overview.ru.md) — оркестрация, Agent SDK, managed agents, budgets и параллелизм

### Часть 3. Темы для опытных

- [13 · Компактизация и длинный контекст](modules/13-long-context/overview.ru.md) — 1M-контекст, compaction и бюджет сообщений
- [14 · Batch, evals и надёжность](modules/14-evals/overview.ru.md) — Batches API, regression evals, stop reason и guardrails
- [15 · Observability](modules/15-observability/overview.ru.md) — usage/cost analytics, OpenTelemetry, traces и метрики
- [16 · Security и compliance](modules/16-security/overview.ru.md) — permissions, secrets, ZDR, residency и безопасный tool use
- [17 · Deployment и production](modules/17-production/overview.ru.md) — Bedrock, Foundry, regional endpoints, retries и стоимость

## Библиотеки

- [Anthropic SDK](libraries/anthropic-sdk/overview.ru.md) — клиенты для Python, TypeScript и других языков
- [Claude Agent SDK](libraries/agent-sdk/overview.ru.md) — агенты на инструментах Claude Code
- [MCP SDK](libraries/mcp-sdk/overview.ru.md) — серверы и клиенты Model Context Protocol
- [Claude Code](libraries/claude-code/overview.ru.md) — CLI, IDE-интеграции, desktop и web

## Рецепты

- [Чат с Claude](recipes/chat-with-history/overview.ru.md) — stateless-сессии, история и стриминг
- [Агент с инструментами](recipes/tool-agent/overview.ru.md) — tool_use, tool_result и обработка ошибок
- [Извлечение структурированных данных](recipes/structured-extraction/overview.ru.md) — JSON Schema и citations
- [Автоматизация репозитория](recipes/claude-code-workflow/overview.ru.md) — CLAUDE.md, skills, hooks и CI
- [RAG через Claude](recipes/rag-with-claude/overview.ru.md) — retrieval, citations и prompt caching

## Проекты

- [Claude API service](projects/api-service/overview.ru.md) — небольшой сервис с streaming и structured output
- [Coding agent workflow](projects/coding-agent/overview.ru.md) — Claude Code и MCP для типового проекта
- [Document assistant](projects/document-assistant/overview.ru.md) — анализ PDF с citations и оценкой качества

## Связанные треки

- [LLM](../llm/overview.ru.md) — общая теория языковых моделей, RAG и агентов
- [Python](../python/overview.ru.md) — основной язык SDK-примеров
- [JavaScript](../javascript/overview.ru.md) — JavaScript SDK-примеры и Node.js-интеграции
- [TypeScript](../typescript/overview.ru.md) — типобезопасные клиенты и схемы

## Источники

- [Обзор моделей Claude](https://platform.claude.com/docs/en/models/overview)
- [Messages API](https://platform.claude.com/docs/en/api/messages)
- [Tool use с Claude](https://platform.claude.com/docs/en/agents-and-tools/tool-use/overview)
- [Claude Platform: overview features](https://platform.claude.com/docs/en/build-with-claude/overview)
- [Claude Code overview](https://code.claude.com/docs/en/overview)
