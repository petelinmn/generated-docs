---
title: "Skills и hooks: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 11-skills-and-hooks
part: 2
tags: [claude, claude-code, skills, hooks, automation]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Skills и hooks: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [09 · Claude Code](../09-claude-code/overview.ru.md), [10 · CLAUDE.md](../10-memory/overview.ru.md)

## Как проверялся материал

Skills, hooks и их коды выхода запускались в Claude Code 2.1.332 в отдельном каталоге; вместо API отвечала локальная заглушка, которая по сценарию запрашивала инструменты. Поэтому проверено поведение программы, а не то, когда настоящая модель решит применить skill.

## Два способа расширить Claude Code

| | Skill | Hook |
|---|---|---|
| Что это | инструкция, которую читает модель | ваша команда, которую запускает программа |
| Когда срабатывает | по решению модели или по `/имя` | всегда на своём событии |
| Для чего | повторяемые процедуры и знания | проверки, запреты, автоматика |

## Первый skill

Файл `.claude/skills/greet/SKILL.md` (для всех проектов — `~/.claude/skills/`):

```markdown
---
description: Greet a person by name. Use when the user asks for a greeting.
---

Greet $0 politely. Mention the current branch: !`git branch --show-current`
```

- Имя каталога становится командой: `/greet Ann`.
- `description` попадает в список доступных skills — по нему модель решает, брать ли его.
- Тело загружается только при вызове, поэтому длинная процедура не занимает контекст постоянно.
- `$0` — первый аргумент; строка `` !`команда` `` заменяется выводом команды до того, как текст увидит модель.

## Первый hook

Запретим `rm -rf` независимо от того, что решит модель. `.claude/settings.json`:

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [{"type": "command", "command": "python \"$CLAUDE_PROJECT_DIR/.claude/hooks/guard.py\""}]
      }
    ]
  }
}
```

```python
# .claude/hooks/guard.py
import json
import sys

data = json.load(sys.stdin)
command = data.get("tool_input", {}).get("command", "")

if "rm -rf" in command:
    print("rm -rf is blocked in this project; delete specific files instead", file=sys.stderr)
    sys.exit(2)          # 2 blocks the call; stderr goes to Claude
```

Hook получает JSON на stdin: событие, имя инструмента, его аргументы. Код выхода 2 блокирует вызов, и текст из stderr видит модель.

Это защита от случайности, а не граница безопасности: ту же операцию можно записать иначе.

## Частые ошибки новичков

- Выходить с кодом 1, чтобы запретить: это не блокирует.
- Расплывчатое `description` у skill.
- Запускать чужие hooks и skills, не прочитав: это код с вашими правами.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [09 · Claude Code](../09-claude-code/advanced.ru.md)

## Источники

- [Skills](https://code.claude.com/docs/en/skills)
- [Hooks guide](https://code.claude.com/docs/en/hooks-guide)
