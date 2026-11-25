---
title: "CLAUDE.md и память: продвинутый уровень"
lang: ru
track: claude
kind: module
doc: advanced
slug: 10-memory
part: 2
tags: [claude, claude-code, claude-md, memory, context]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# CLAUDE.md и память: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Что загружается и когда

```text
at start            ~/.claude/CLAUDE.md, repo/CLAUDE.md, its import, rules/general.md, CLAUDE.local.md
after reading       sub/CLAUDE.md and rules/python.md are added
sub/a.py
```

Порядок в запросе: пользовательский файл, проектный, его импорты, правила, локальный. Всё это идёт сообщением пользователя, а не системным промптом. При запуске из подкаталога `sub/` загрузились и файлы верхних каталогов, и `sub/CLAUDE.md`.

## Правила по путям

`.claude/rules/python.md`:

```markdown
---
paths:
  - "**/*.py"
---

- Use type hints in public functions.
```

Правило без `paths` грузится всегда. С `paths` — когда Claude читает, пишет или правит подходящий файл: так инструкции не занимают контекст зря.

## Импорты

- `@path` подставляет содержимое файла; относительный путь считается от файла с импортом.
- Не больше четырёх переходов: в цепочке из пяти импортов пятый файл не загрузился.
- Внутри `обратных кавычек` и блоков кода `@` не импортируется.
- Импорт — это организация, а не экономия: содержимое всё равно попадает в контекст.
- Импорт файла вне проекта из проектной памяти требует одобрения.

HTML-комментарии `<!-- ... -->` вырезаются до отправки: это заметки для людей.

## AGENTS.md

Если в рабочем каталоге и выше нет `CLAUDE.md` и `CLAUDE.local.md`, загружается `AGENTS.md`. Если есть — `AGENTS.md` пропускается; чтобы использовать оба, добавьте в `CLAUDE.md` строку `@AGENTS.md`. Пользовательский `~/.claude/CLAUDE.md` на это не влияет.

## Автопамять

Claude сам ведёт `MEMORY.md` (оглавление) и тематические файлы. В начале сессии грузится только `MEMORY.md`: первые 200 строк или 25 КБ; тематические файлы читаются по необходимости. Это обычный markdown — его можно править и удалять.

На файле в 205 строк загрузились 200, а в конец добавилось предупреждение:

```text
> WARNING: MEMORY.md is 205 lines (limit: 200). Only part of it was loaded: 5 of 205 lines were cut off, ...
```

Отключить: переключатель в `/memory` или `CLAUDE_CODE_DISABLE_AUTO_MEMORY=1`.

## Длинные сессии и сжатие

| Что | После сжатия (по документации) |
|---|---|
| корневой `CLAUDE.md` и правила без `paths` | перечитываются с диска |
| правила с `paths` и вложенные `CLAUDE.md` | вернутся, когда снова понадобятся |
| сказанное только в чате | остаётся лишь в пересказе |

Если инструкция должна пережить сжатие, запишите её в корневой файл.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [09 · Claude Code](../09-claude-code/advanced.ru.md)

## Источники

- [Memory](https://code.claude.com/docs/en/memory)
- [Context window](https://code.claude.com/docs/en/context-window)
