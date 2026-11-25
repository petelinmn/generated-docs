---
title: "Skills и hooks"
lang: ru
track: claude
kind: module
doc: overview
slug: 11-skills-and-hooks
part: 2
tags: [claude, claude-code, skills, hooks, automation]
versions: "Claude Code 2.1.332"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Skills и hooks

🌐 **Русский** · [English](overview.en.md)

## Коротко

Два механизма расширения Claude Code: **skills** — инструкции, подгружаемые по надобности, и **hooks** — ваши команды, выполняемые на событиях.

## Ключевые идеи

- Skill читает модель; hook выполняет программа.
- В контексте лежат только описания skills.
- Код выхода 2 блокирует, 1 — нет.
- `deny` сильнее разрешения от hook.
- Чужие skills и hooks — это исполняемый код.

## Минимальный пример

```markdown
---
description: Greet a person by name. Use when the user asks for a greeting.
---

Greet $0 politely. Mention the current branch: !`git branch --show-current`
```

## Когда применять и когда нет

Skill — для повторяемой процедуры; hook — для того, что обязано происходить всегда. Разовое указание проще написать в чате.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude 11-skills-and-hooks level 5` |
| 6. Тесты | — | `/learn claude 11-skills-and-hooks level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [09 · Claude Code](../09-claude-code/overview.ru.md)
- [10 · CLAUDE.md и память](../10-memory/overview.ru.md)
- [12 · Агенты](../12-agents/overview.ru.md)
