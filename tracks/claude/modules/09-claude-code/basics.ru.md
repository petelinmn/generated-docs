---
title: "Claude Code: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 09-claude-code
part: 2
tags: [claude, claude-code, permissions, headless, ci]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Claude Code: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Как проверялся материал

Команды запускались в Claude Code 2.1.332 на Windows с изолированным каталогом настроек. Вместо API отвечала локальная заглушка, которая по сценарию «просила» инструменты. Поэтому проверено поведение самой программы (права, коды выхода, форматы), а не ответы модели. Интерактивные команды, IDE и GitHub Actions описаны по документации.

## Что это

Claude Code — агент от Anthropic, работающий с вашим проектом: читает файлы, правит код, запускает команды. Это тот же цикл из [tool use](../07-tool-use/overview.ru.md), где инструменты уже готовы.

## Установка

| Система | Команда |
|---|---|
| macOS, Linux, WSL | `curl -fsSL https://claude.ai/install.sh \| bash` |
| Windows PowerShell | `irm https://claude.ai/install.ps1 \| iex` |
| Homebrew | `brew install --cask claude-code` |
| WinGet | `winget install Anthropic.ClaudeCode` |

Homebrew и WinGet сами не обновляют программу. Нужен вход по подписке или ключ API.

## Первый запуск

```bash
cd my-project
claude                       # interactive session
claude -p "Explain what this repository does"      # one answer, then exit
```

## Права доступа

Перед рискованным действием Claude Code спрашивает разрешение. В режиме по умолчанию чтение и безопасные команды вроде `ls` проходили без вопроса, а `touch` и запись файла требовали одобрения.

| Режим | Запись файла в проекте |
|---|---|
| `default` (в интерфейсе Manual) | спрашивает |
| `acceptEdits` | разрешает |
| `plan` | отклоняет: только исследование и план |
| `dontAsk` | отклоняет всё, что потребовало бы вопроса |
| `auto` | решает классификатор |
| `bypassPermissions` | разрешает; только в изолированной среде |

Режим переключают `Shift+Tab` или флагом `--permission-mode`.

## Полезные команды в сессии

| Команда | Что делает |
|---|---|
| `/init` | создаёт `CLAUDE.md` |
| `/clear`, `/compact` | новый разговор; сжатие текущего |
| `/context` | что занимает контекст |
| `/permissions`, `/model`, `/mcp` | права, модель, серверы MCP |
| `/resume`, `/rewind` | вернуться к сессии; откатить правки и разговор |
| `Esc` | прервать, не теряя сделанного |
| `@file`, `!cmd` | упомянуть файл; выполнить команду самому |

## Как формулировать задачу

- Дайте цель и способ проверки: «почини тест X, запусти `npm test`».
- Для крупных изменений начните с режима plan.
- Работайте в git: так видно и легко откатить всё.

## Частые ошибки новичков

- Включить `bypassPermissions` на рабочей машине.
- Вести всё в одной сессии: старый контекст мешает.
- Считать `/rewind` заменой git.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [10 · CLAUDE.md и память](../10-memory/overview.ru.md)

## Источники

- [Overview and installation](https://code.claude.com/docs/en/overview)
- [Permission modes](https://code.claude.com/docs/en/permission-modes)
- [Commands](https://code.claude.com/docs/en/commands)
