---
title: "CLAUDE.md и память: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 10-memory
part: 2
tags: [claude, claude-code, claude-md, memory, context]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# CLAUDE.md и память: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [09 · Claude Code](../09-claude-code/overview.ru.md)

## Как проверялся материал

Загрузка проверена в Claude Code 2.1.332: в файлы вставлялись метки, а локальная заглушка API записывала запросы. Так видно, **что именно уходит в модель**. Как модель следует инструкциям, этим не проверяется.

## Зачем нужна память

Каждая сессия начинается с чистого контекста. Всё, что агенту надо знать о проекте всегда, записывают в файл — он добавляется в начало каждого разговора.

| | CLAUDE.md | Автопамять |
|---|---|---|
| Кто пишет | вы | Claude |
| Что внутри | инструкции и правила | замеченное по ходу работы |
| Где живёт | репозиторий или домашний каталог | `~/.claude/projects/<project>/memory/` |

## Первый CLAUDE.md

Команда `/init` создаст черновик по коду проекта. Хороший файл короткий и конкретный:

```markdown
# Project

- Build: `npm run build`. Tests: `npm test` (run them before committing).
- Use 2-space indentation.
- API handlers live in `src/api/`.

See @docs/conventions.md
```

- Пишите проверяемое: «2 пробела», а не «форматируй аккуратно».
- Ориентир документации — меньше 200 строк.
- Не пишите то, что видно из кода.

## Где лежат файлы

```text
~/.claude/CLAUDE.md                 yours, every project
repo/CLAUDE.md                      shared through git (or repo/.claude/CLAUDE.md)
repo/CLAUDE.local.md                yours, this project; add to .gitignore
repo/.claude/rules/general.md       loaded at start
repo/.claude/rules/python.md        has `paths:`; loaded when a matching file is opened
repo/sub/CLAUDE.md                  loaded when a file in sub/ is opened
```

## Это не принуждение

`CLAUDE.md` — текст в контексте, а не правило, которое программа обязана выполнить. То, что должно соблюдаться всегда (не читать `.env`, не пушить), задают [правами](../09-claude-code/advanced.ru.md) или hooks.

## Посмотреть и изменить

- `/memory` — открыть файлы инструкций и автопамять, включить или выключить автопамять.
- `/context` — какие файлы загружены и сколько занимают.

## Частые ошибки новичков

- Огромный файл «обо всём».
- Противоречия между пользовательским и проектным файлами.
- Секреты в `CLAUDE.md`: файл уходит в модель и в git.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [05 · Prompt caching](../05-prompt-caching/overview.ru.md)

## Источники

- [Memory](https://code.claude.com/docs/en/memory)
