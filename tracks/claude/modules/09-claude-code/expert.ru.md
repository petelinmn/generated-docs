---
title: "Claude Code: экспертный уровень"
lang: ru
track: claude
kind: module
doc: expert
slug: 09-claude-code
part: 2
tags: [claude, claude-code, permissions, headless, ci]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Claude Code: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Флаги

| Флаг | Назначение |
|---|---|
| `-p`, `--print` | ответить и выйти |
| `--output-format text\|json\|stream-json`, `--input-format` | форматы |
| `--json-schema` | структурированный ответ в `structured_output` |
| `--max-turns`, `--max-budget-usd` | пределы |
| `--allowedTools`, `--disallowedTools`, `--permission-mode` | права |
| `-c`, `-r`, `--fork-session`, `--no-session-persistence` | сессии |
| `--model`, `--effort`, `--fallback-model` | модель |
| `--system-prompt`, `--append-system-prompt` | системный промпт |
| `--mcp-config`, `--strict-mcp-config` | MCP |
| `--settings`, `--setting-sources`, `--add-dir` | настройки и каталоги |
| `--bare`, `--safe-mode` | запуск без настроек и расширений |
| `--bg` | фоновая сессия |

Подкоманды: `mcp`, `agents`, `attach`, `logs`, `stop`, `plugin`, `doctor`, `auth`, `install`, `update` и другие; полный список — `claude --help`. `--max-turns` в этом выводе не показан, но принимается.

## Модели

Псевдонимы на ключе API в 2.1.332: `opus` → `claude-opus-5-5`, `sonnet` → `claude-sonnet-5-5`, `haiku` → `claude-haiku-5-5`, `fable` → `claude-fable-5-2`. Псевдоним со временем указывает на новую модель.

## Что уходит в API

В перехваченном запросе: системный промпт, список инструментов (`Read`, `Edit`, `Write`, `Bash`, `Glob`, `Grep`, `Agent`, `Skill`, `ToolSearch` и др.), а сведения о git и `CLAUDE.md` — сообщениями пользователя в `<system-reminder>`. Инструменты MCP по умолчанию отложены: сначала передаются имена, схемы подгружаются через `ToolSearch`.

## Подвохи

1. Запуск `-p` с отказом в правах завершается кодом 0 и `subtype: success`.
2. `stream-json` без `--verbose` — ошибка.
3. Переменные `CLAUDE_*`, унаследованные из другой сессии, меняют поведение дочернего запуска; в скриптах задавайте окружение явно.
4. `CLAUDE.md` из каталога, похожего на scratchpad сессии (`claude/<project>/<session>/scratchpad`), не загружается; причина видна в `--debug-file`.
5. `allow` не делает исключения из `deny`.
6. В каталоге, которому ещё не доверяли, `-p` проигнорировал `allow` из `.claude/settings.json` (с предупреждением в выводе ошибок); `deny` из того же файла действовал.

> ⚠️ Проверить: режим `auto`, sandbox, `/rewind`, фоновые сессии, плагины, IDE и GitHub Actions не запускались.

## Вопросы с собеседований

1. Чем Claude Code отличается от чата?
<details><summary>Ответ</summary>

Это агент с инструментами: сам читает проект, правит и проверяет, в рамках прав.
</details>

2. Как запустить в CI безопасно?
<details><summary>Ответ</summary>

`-p`, узкий `--allowedTools`, `dontAsk`, лимиты ходов и бюджета, минимальные права токена, изолированный раннер, секреты из хранилища.
</details>

3. `--allowedTools` и режим прав?
<details><summary>Ответ</summary>

Первый добавляет разрешающие правила, второй задаёт, что делать с остальным.
</details>

4. Что сильнее: allow или deny?
<details><summary>Ответ</summary>

Deny.
</details>

5. Почему правило Bash — не защита?
<details><summary>Ответ</summary>

Оно сравнивает строку; ту же операцию можно выразить иначе.
</details>

6. Как понять в скрипте, что что-то запретили?
<details><summary>Ответ</summary>

`permission_denials` в JSON: код выхода может быть 0.
</details>

7. Что делать при переполнении контекста?
<details><summary>Ответ</summary>

Сжатие происходит автоматически; помогают `/compact` с фокусом, `/clear` между задачами, subagents для чтения.
</details>

8. `--system-prompt` и `--append-system-prompt`?
<details><summary>Ответ</summary>

Первый заменяет системный промпт, второй дополняет; `CLAUDE.md` в обоих случаях загружается.
</details>

9. Что делает `--bare`?
<details><summary>Ответ</summary>

Минимальный режим без hooks, автопамяти и поиска `CLAUDE.md`; удобен для воспроизводимых запусков.
</details>

10. Как подключить свои инструменты?
<details><summary>Ответ</summary>

MCP-серверы; повторяемые сценарии — skills и hooks.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [10 · CLAUDE.md и память](../10-memory/expert.ru.md)

## Источники

- [CLI reference](https://code.claude.com/docs/en/cli-reference)
- [Permissions](https://code.claude.com/docs/en/permissions)
- [Context window](https://code.claude.com/docs/en/context-window)
