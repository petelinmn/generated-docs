---
title: "Агенты и subagents: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 12-agents
part: 2
tags: [claude, agents, subagents, agent-sdk, managed-agents]
versions: "Claude Code 2.1.332; claude-agent-sdk 0.2.189; anthropic 2.14.0; managed-agents-2026-04-01"
status: draft
updated: 2026-11-25
---

# Агенты и subagents: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [07 · Tool use](../07-tool-use/overview.ru.md), [09 · Claude Code](../09-claude-code/overview.ru.md)

## Как проверялся материал

Subagents запускались в Claude Code 2.1.332, Agent SDK — пакетом `claude-agent-sdk` 0.2.189. Вместо API отвечала локальная заглушка: она по сценарию запрашивала инструменты и записывала запросы. Managed Agents требуют настоящего API: проверены только формы запросов SDK.

## Что такое агент

Агент — это модель в цикле с инструментами: она сама решает следующий шаг, пока задача не сделана. Не всё должно быть агентом: если шаги известны заранее, обычный код дешевле и предсказуемее.

## Subagent

**Subagent** — помощник со своим контекстом, которому основной агент поручает часть работы и получает назад только отчёт. Так длинный поиск не засоряет основной разговор.

`.claude/agents/reviewer.md`:

```markdown
---
name: reviewer
description: Reviews code for bugs. Use after code changes.
tools: Read, Grep, Glob
model: haiku
---

You are a careful reviewer. Report concrete defects with file and line.
```

- `description` говорит основному агенту, когда его звать.
- `tools` ограничивает инструменты, `model` выбирает модель.
- Попросить явно: «пусть reviewer проверит изменения».

## Что видит subagent

| Получает | Не получает |
|---|---|
| свой системный промпт и задание | историю основного разговора |
| `CLAUDE.md` (кроме встроенных Explore и Plan) | |

Значит, всё нужное должно быть в самом задании.

## Встроенные

- `Explore` — поиск по коду без правок
- `Plan` — исследование для плана
- `general-purpose` — все инструменты

## Частые ошибки новичков

- Делегировать пустяк: у каждого subagent свой расход токенов.
- Задание без контекста: «проверь это».
- Верить отчёту без проверки.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [11 · Skills и hooks](../11-skills-and-hooks/basics.ru.md)

## Источники

- [Subagents](https://code.claude.com/docs/en/sub-agents)
