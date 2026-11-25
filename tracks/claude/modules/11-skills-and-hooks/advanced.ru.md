---
title: "Skills и hooks: продвинутый уровень"
lang: ru
track: claude
kind: module
doc: advanced
slug: 11-skills-and-hooks
part: 2
tags: [claude, claude-code, skills, hooks, automation]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Skills и hooks: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Frontmatter skill

| Поле | Действие |
|---|---|
| `description`, `when_to_use` | когда применять; вместе обрезаются на 1536 символах |
| `disable-model-invocation: true` | только `/имя`; модель не видит его в списке |
| `user-invocable: false` | только для модели; `/имя` отвечает отказом |
| `allowed-tools` | инструменты без вопроса на ход вызова |
| `context: fork`, `agent` | выполнить в отдельном subagent |
| `model`, `effort`, `paths`, `hooks`, `argument-hint` | уточняют модель, усилие, область, hooks, подсказку |

Проверено: skill с `allowed-tools: Bash(touch *)` создал файл без разрешения, тот же вызов без skill получил отказ.

Аргументы: `$ARGUMENTS` — все, `$0`, `$1` — по номеру, с кавычками как в shell. `/greet "Ann Lee" tomorrow` дало `$0` = `Ann Lee`. Номер без аргумента остаётся текстом `$1`. `${{CLAUDE_SKILL_DIR}}` — каталог skill, для приложенных скриптов и справок. `SKILL.md` советуют держать короче 500 строк, подробности — в соседних файлах.

Старые файлы `.claude/commands/имя.md` по-прежнему вызываются как `/имя`.

## События hooks

| Событие | Когда | Что делает код 2 |
|---|---|---|
| `PreToolUse` | перед инструментом | блокирует вызов |
| `PostToolUse` | после успеха | инструмент уже выполнен; Claude получает замечание |
| `UserPromptSubmit` | отправка сообщения | отклоняет сообщение: запрос к API не уходит |
| `Stop` | Claude заканчивает ответ | заставляет продолжить |
| `SessionStart` | начало или возобновление | — |
| `SubagentStart`, `SubagentStop`, `PermissionRequest`, `PreCompact`, `PostCompact`, `SessionEnd`, `Notification` | | |

`matcher` для событий инструментов сравнивается с именем: `Bash`, `Edit|Write`, `mcp__server__.*`.

## Коды выхода и JSON

| Результат hook | Наблюдение для `PreToolUse` |
|---|---|
| код 0 без вывода | обычная проверка прав |
| код 2 | вызов заблокирован, даже в `bypassPermissions` |
| код 1 | не блокирует |
| `permissionDecision: "deny"` | заблокирован с причиной |
| `permissionDecision: "allow"` | без вопроса; но правило `deny` всё равно действует |
| `permissionDecision: "ask"` | спросить; в `-p` это отказ |
| `updatedInput` | подменяет аргументы: вместо `touch` выполнился `echo` |

```json
{"hookSpecificOutput": {"hookEventName": "PreToolUse", "permissionDecision": "deny",
                        "permissionDecisionReason": "use the deploy script instead"}}
```

stdout hook на `SessionStart` и `UserPromptSubmit` добавляется в контекст.

## Не зациклить Stop

При повторном срабатывании во входе `stop_hook_active: true` — проверяйте его и выходите с 0.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [12 · Агенты](../12-agents/basics.ru.md)

## Источники

- [Skills](https://code.claude.com/docs/en/skills)
- [Hooks reference](https://code.claude.com/docs/en/hooks)
