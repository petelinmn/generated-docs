---
title: "Skills и hooks: экспертный уровень"
lang: ru
track: claude
kind: module
doc: expert
slug: 11-skills-and-hooks
part: 2
tags: [claude, claude-code, skills, hooks, automation]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Skills и hooks: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Все события

По документации: `SessionStart`, `Setup`, `InstructionsLoaded`, `UserPromptSubmit`, `UserPromptExpansion`, `PreSkillShellCommand`, `MessageDisplay`, `PreToolUse`, `PermissionRequest`, `PostToolUse`, `PostToolUseFailure`, `PostToolBatch`, `PermissionDenied`, `Notification`, `SubagentStart`, `SubagentStop`, `TaskCreated`, `TaskCompleted`, `Stop`, `StopFailure`, `TeammateIdle`, `ConfigChange`, `CwdChanged`, `DirectoryAdded`, `FileChanged`, `WorktreeCreate`, `WorktreeRemove`, `PreCompact`, `PostCompact`, `PreModelSwitch`, `PostModelSwitch`, `SessionEnd`, `Elicitation`, `ElicitationResult`.

## Обработчики и таймауты

| `type` | Что выполняет | Таймаут по умолчанию, с |
|---|---|---|
| `command` | команду | 600 |
| `http` | HTTP-запрос | 600 |
| `mcp_tool` | инструмент MCP | 600 |
| `prompt` | запрос к модели | 30 |
| `agent` | агента с инструментами | 60 |

У `UserPromptSubmit` и некоторых событий таймаут по умолчанию меньше. Команду можно сделать фоновой (`async: true`).

## Что получает hook

В перехваченном `PreToolUse`: `session_id`, `transcript_path`, `cwd`, `prompt_id`, `permission_mode`, `effort`, `hook_event_name`, `tool_name`, `tool_input`, `tool_use_id`. `PostToolUse` добавляет `tool_response` и `duration_ms`, `Stop` — `stop_hook_active` и `last_assistant_message`.

## Где объявляются

Настройки пользователя, проекта, локальные и управляемые; плагины; frontmatter skill и subagent. Вывод hook ограничен 10 000 символов: длиннее — сохраняется в файл, а Claude получает путь и начало.

## Подвохи

1. `exit 1` не блокирует: инструмент пошёл дальше по обычной проверке прав.
2. Hook с `timeout: 3`, который спал 20 секунд и собирался выйти с 2, был отменён — команда выполнилась.
3. `matcher: Edit` не сработал на `Bash` — опечатка в matcher молча выключает защиту.
4. Skill с `disable-model-invocation` при вызове инструментом `Skill` вернул ошибку, запрещающую воспроизводить его другими средствами; `/имя` работает.
5. Git Bash превращает аргумент `/greet` в путь Windows: в скриптах задайте `MSYS2_ARG_CONV_EXCL='*'`.
6. `PostToolUse` ничего не отменяет: действие уже произошло.

> ⚠️ Проверить: обработчики `http`, `mcp_tool`, `prompt`, `agent`, фоновые hooks, плагины и большинство событий из списка не запускались.

## Вопросы с собеседований

1. Когда skill, а когда CLAUDE.md?
<details><summary>Ответ</summary>

CLAUDE.md — то, что нужно всегда; skill — процедура по случаю, тело грузится при вызове.
</details>

2. Когда hook, а не инструкция?
<details><summary>Ответ</summary>

Когда действие обязано происходить независимо от модели.
</details>

3. Какой код выхода блокирует?
<details><summary>Ответ</summary>

2.
</details>

4. Может ли hook разрешить запрещённое правилом?
<details><summary>Ответ</summary>

Нет: `deny` действует.
</details>

5. Как переписать команду?
<details><summary>Ответ</summary>

`updatedInput` в JSON-ответе `PreToolUse`.
</details>

6. Что будет, если hook не уложился в таймаут?
<details><summary>Ответ</summary>

Он отменяется, и это не блокировка: действие идёт дальше.
</details>

7. Как hooks разных уровней сочетаются?
<details><summary>Ответ</summary>

Складываются; совпавшие запускаются параллельно.
</details>

8. Как отключить hooks?
<details><summary>Ответ</summary>

`disableAllHooks`; управляемые hooks так отключить нельзя.
</details>

9. Как запретить модели самой деплоить?
<details><summary>Ответ</summary>

`disable-model-invocation: true` у skill и права/hooks на сами команды.
</details>

10. Чем опасны `!`-команды в чужом skill?
<details><summary>Ответ</summary>

Выполняются при загрузке skill до того, как модель что-то решит.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [10 · CLAUDE.md](../10-memory/expert.ru.md)

## Источники

- [Hooks reference](https://code.claude.com/docs/en/hooks)
- [Skills](https://code.claude.com/docs/en/skills)
