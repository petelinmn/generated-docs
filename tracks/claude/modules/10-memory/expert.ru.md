---
title: "CLAUDE.md и память: экспертный уровень"
lang: ru
track: claude
kind: module
doc: expert
slug: 10-memory
part: 2
tags: [claude, claude-code, claude-md, memory, context]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# CLAUDE.md и память: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Все источники инструкций

| Уровень | Расположение |
|---|---|
| managed | Windows `C:\Program Files\ClaudeCode\CLAUDE.md`; macOS `/Library/Application Support/ClaudeCode/CLAUDE.md`; Linux/WSL `/etc/claude-code/CLAUDE.md` |
| user | `~/.claude/CLAUDE.md`, `~/.claude/rules/` |
| project | `./CLAUDE.md`, `./.claude/CLAUDE.md`, `.claude/rules/` |
| local | `./CLAUDE.local.md` |

## Управление загрузкой

| Средство | Наблюдение |
|---|---|
| `claudeMdExcludes: ["**/proj/CLAUDE.md"]` | исчезли файл и его импорт; правила и локальный файл остались |
| `--setting-sources user` | только пользовательский файл |
| `--bare` | ничего не загрузилось |
| `CLAUDE_CODE_DISABLE_AUTO_MEMORY=1` | `MEMORY.md` не загружен |
| `--system-prompt` | `CLAUDE.md` всё равно загружен |

Настройка **Project instructions** выбирает: `claude-md-or-agents-md` (по умолчанию), `claude-md-and-agents-md`, только `CLAUDE.md` или только управляемые файлы.

## Как это выглядит в запросе

```text
<system-reminder>
Codebase and user instructions are shown below. Be sure to adhere to these instructions. ...

Contents of .../CLAUDE.md (user's private global instructions for all projects):
Contents of .../CLAUDE.md (project instructions, checked into the codebase):
Contents of .../CLAUDE.local.md (user's private project instructions, not checked in):
</system-reminder>
```

Это фрагмент перехваченного запроса версии 2.1.332 с сокращёнными путями; формулировки могут меняться между версиями.

## Подвохи

1. `CLAUDE.local.md` в проекте, где был только `AGENTS.md`, отключает чтение `AGENTS.md` (по документации).
2. Строки `MEMORY.md` после 200-й молча не действуют, если не читать предупреждение.
3. Длинные строки упираются в предел 25 КБ раньше предела строк: из 60 строк по 600 символов загрузилась лишь часть.
4. `claudeMdExcludes` не затрагивает правила в `.claude/rules/`, не подходящие под шаблон.
5. `CLAUDE.md` в каталоге вида scratchpad сессии игнорируется.

> ⚠️ Проверить: managed-файл, диалог одобрения внешних импортов, сжатие и то, когда Claude сам пишет в автопамять, не запускались.

## Вопросы с собеседований

1. Чем CLAUDE.md отличается от системного промпта?
<details><summary>Ответ</summary>

Он доставляется сообщением пользователя после системного промпта и не является механизмом принуждения.
</details>

2. Как гарантировать запрет?
<details><summary>Ответ</summary>

Правами или hook, а не фразой в файле.
</details>

3. Как не раздувать контекст в монорепозитории?
<details><summary>Ответ</summary>

Правила с `paths`, вложенные `CLAUDE.md`, `claudeMdExcludes`, skills для редких процедур.
</details>

4. Импорты экономят токены?
<details><summary>Ответ</summary>

Нет, содержимое разворачивается в контекст.
</details>

5. Что положить в CLAUDE.local.md?
<details><summary>Ответ</summary>

Личные адреса стендов и предпочтения для проекта; файл в `.gitignore`.
</details>

6. Когда `AGENTS.md` не читается?
<details><summary>Ответ</summary>

Когда есть `CLAUDE.md` или `CLAUDE.local.md` в рабочем каталоге или выше (настройка по умолчанию).
</details>

7. Что такое managed CLAUDE.md?
<details><summary>Ответ</summary>

Файл организации в системном каталоге; его нельзя исключить.
</details>

8. Чем рискует чужой репозиторий с CLAUDE.md?
<details><summary>Ответ</summary>

Это инструкции, которые прочтёт агент с вашими правами; читайте перед запуском.
</details>

9. Как проверить, что загрузилось?
<details><summary>Ответ</summary>

`/context` и `/memory`; `--debug-file` покажет пропущенные файлы.
</details>

10. Что писать, а что нет?
<details><summary>Ответ</summary>

Команды, соглашения, неочевидные факты — да; очевидное из кода, большие справочники и секреты — нет.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [05 · Prompt caching](../05-prompt-caching/advanced.ru.md)

## Источники

- [Memory](https://code.claude.com/docs/en/memory)
- [Settings](https://code.claude.com/docs/en/settings)
