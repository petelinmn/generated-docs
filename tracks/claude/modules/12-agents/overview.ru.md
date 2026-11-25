---
title: "Агенты и subagents"
lang: ru
track: claude
kind: module
doc: overview
slug: 12-agents
part: 2
tags: [claude, agents, subagents, agent-sdk, managed-agents]
versions: "Claude Code 2.1.332; claude-agent-sdk 0.2.189; anthropic 2.14.0; managed-agents-2026-04-01"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Агенты и subagents

🌐 **Русский** · [English](overview.en.md)

## Коротко

Как устроены агенты в экосистеме Claude: **subagents** в Claude Code, **Agent SDK** в своей программе и **Managed Agents** на стороне Anthropic.

## Ключевые идеи

- Агент — модель в цикле с инструментами.
- У subagent свой контекст, назад возвращается отчёт.
- Инструменты и модель задаются на каждого.
- Отчёт — это данные, их проверяют.
- Начинайте с простого: часто агент не нужен.

## Минимальный пример

```markdown
---
name: reviewer
description: Reviews code for bugs. Use after code changes.
tools: Read, Grep, Glob
model: haiku
---

You are a careful reviewer. Report concrete defects with file and line.
```

## Когда применять и когда нет

Открытые многошаговые задачи с проверяемым результатом. Фиксированные конвейеры делайте обычным кодом.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude 12-agents level 5` |
| 6. Тесты | — | `/learn claude 12-agents level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [07 · Tool use](../07-tool-use/overview.ru.md)
- [11 · Skills и hooks](../11-skills-and-hooks/overview.ru.md)
- [LLM](../../../llm/overview.ru.md)
- [Claude Agent SDK](../../libraries/agent-sdk/overview.ru.md)
