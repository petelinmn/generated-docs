---
title: "CLAUDE.md и память"
lang: ru
track: claude
kind: module
doc: overview
slug: 10-memory
part: 2
tags: [claude, claude-code, claude-md, memory, context]
versions: "Claude Code 2.1.332"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# CLAUDE.md и память

🌐 **Русский** · [English](overview.en.md)

## Коротко

Как Claude Code помнит проект между сессиями: файлы **CLAUDE.md**, правила, импорты, **автопамять** и что из этого попадает в контекст.

## Ключевые идеи

- `CLAUDE.md` пишете вы, автопамять — Claude.
- Всё загруженное занимает контекст.
- Правила с `paths` и вложенные файлы грузятся по требованию.
- Это инструкции, а не принуждение.
- Из `MEMORY.md` грузятся первые 200 строк или 25 КБ.

## Минимальный пример

```markdown
# Project

- Build: `npm run build`. Tests: `npm test` (run them before committing).
- Use 2-space indentation.
- API handlers live in `src/api/`.

See @docs/conventions.md
```

## Когда применять и когда нет

Для устойчивых фактов о проекте и соглашений. Разовые указания говорите в чате, обязательные запреты задавайте правами.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude 10-memory level 5` |
| 6. Тесты | — | `/learn claude 10-memory level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [09 · Claude Code](../09-claude-code/overview.ru.md)
- [05 · Prompt caching](../05-prompt-caching/overview.ru.md)
