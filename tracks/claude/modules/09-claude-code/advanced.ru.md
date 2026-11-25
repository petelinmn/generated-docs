---
title: "Claude Code: продвинутый уровень"
lang: ru
track: claude
kind: module
doc: advanced
slug: 09-claude-code
part: 2
tags: [claude, claude-code, permissions, headless, ci]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Claude Code: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Неинтерактивный режим

```bash
git diff | claude -p "Review this diff"                       # piped input becomes part of the prompt
claude -p "List the TODOs" --output-format json               # result, session_id, num_turns, cost, denials
claude -p "..." --output-format stream-json --verbose         # events as they happen
claude -p "Count the files" --max-turns 3                     # stop after 3 turns
claude -p "Extract the version" --json-schema '{"type":"object","properties":{"n":{"type":"integer"}},"required":["n"]}'
claude -p "Run the tests" --allowedTools "Bash(npm test)" --permission-mode dontAsk
claude -p "Next step" --continue                              # the latest session in this directory
claude -p "Next step" --resume "$SESSION_ID"
```

- `stream-json` с `-p` требует `--verbose`.
- Неразрешённое действие не останавливает запуск: инструмент получает отказ, код выхода остаётся 0, отказы перечислены в `permission_denials`.
- Лимит `--max-turns` даёт `subtype: error_max_turns` и код 1; недоступный API — тоже код 1.
- `-p` не показывает диалог доверия к каталогу: запускайте только в репозиториях, которым доверяете.

## Настройки и их приоритет

| Файл | Для кого |
|---|---|
| `~/.claude/settings.json` | вы во всех проектах |
| `.claude/settings.json` | команда, в git |
| `.claude/settings.local.json` | вы в этом проекте |
| managed settings | политика организации |

Проверено на ключе `model`: проектный файл перекрыл пользовательский, локальный — проектный, флаг `--model` — всё.

## Правила прав

```json
{
  "permissions": {
    "allow": ["Bash(npm run test *)", "Bash(git diff *)"],
    "ask": ["Bash(git push *)"],
    "deny": ["Read(./.env)", "Bash(curl *)"]
  }
}
```

Разрешения `allow` из файла в репозитории начинают действовать только после того, как вы приняли диалог доверия к каталогу.

Порядок: `deny`, затем `ask`, затем `allow`; точность правила порядок не меняет. `deny` действует и в `bypassPermissions`.

- Составная команда проверяется по частям: при разрешённом `Bash(touch *)` команда `touch a && rm a` всё равно спросила про `rm`.
- Правила Bash сравнивают текст команды: `/bin/rm` или `sh -c` их обходят. Для настоящих границ нужен sandbox или hook.
- `Read(./.env)` в `deny` остановил и Read, и `cat .env`, но не защищает от скрипта, открывающего файл сам.

## Сессии и откат

- `--continue` и `--resume` вернули историю; новый запуск без них начинается пустым.
- `/rewind` (или `Esc Esc` на пустом вводе) откатывает код, разговор или оба. Файл, изменённый только командами Bash, не отслеживается; внешние изменения тоже.

## Редакторы

- VS Code (1.94.0+): расширение с inline-диффами, `@`-упоминаниями и просмотром плана; выделенный текст виден Claude.
- JetBrains: плагин поверх установленного CLI, диффы в IDE и диагностика кода.

## CI: GitHub Actions

```yaml
name: Claude Code
on:
  issue_comment:
    types: [created]
  pull_request_review_comment:
    types: [created]
jobs:
  claude:
    if: contains(github.event.comment.body, '@claude')
    runs-on: ubuntu-latest
    permissions:
      contents: write
      pull-requests: write
      issues: write
      id-token: write
      actions: read
    steps:
      - uses: actions/checkout@v6
        with:
          fetch-depth: 1
      - uses: anthropics/claude-code-action@v1
        with:
          anthropic_api_key: ${{ secrets.ANTHROPIC_API_KEY }}
```

Проще всего настроить командой `/install-github-app` внутри `claude`. Без входа `prompt` action ждёт упоминания `@claude`.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [08 · MCP](../08-mcp/advanced.ru.md)

## Источники

- [Headless mode](https://code.claude.com/docs/en/headless)
- [Settings](https://code.claude.com/docs/en/settings)
- [Permissions](https://code.claude.com/docs/en/permissions)
- [Checkpointing](https://code.claude.com/docs/en/checkpointing)
- [GitHub Actions](https://code.claude.com/docs/en/github-actions)
