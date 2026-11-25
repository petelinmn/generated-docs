---
title: "Claude Code"
lang: ru
track: claude
kind: module
doc: overview
slug: 09-claude-code
part: 2
tags: [claude, claude-code, permissions, headless, ci]
versions: "Claude Code 2.1.332"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Claude Code

🌐 **Русский** · [English](overview.en.md)

## Коротко

**Claude Code** — агент для работы с кодом в терминале, редакторе и CI. Модуль: установка, права, сессии, неинтерактивный режим и настройки.

## Ключевые идеи

- Права решают, что агент может сделать без вас.
- `deny` сильнее `ask`, `ask` сильнее `allow`.
- `claude -p` превращает агента в команду для скриптов.
- Настройки есть пользовательские, проектные, локальные и управляемые.
- Правила команд — не песочница.

## Минимальный пример

```bash
cd my-project
claude                       # interactive session
claude -p "Explain what this repository does"      # one answer, then exit
```

## Когда применять и когда нет

Задачи в репозитории с проверяемым результатом. Не давайте широких прав там, где ошибку нельзя откатить.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude 09-claude-code level 5` |
| 6. Тесты | — | `/learn claude 09-claude-code level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [07 · Tool use](../07-tool-use/overview.ru.md)
- [08 · MCP](../08-mcp/overview.ru.md)
- [10 · CLAUDE.md и память](../10-memory/overview.ru.md)
