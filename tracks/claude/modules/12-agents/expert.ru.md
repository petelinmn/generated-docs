---
title: "Агенты и subagents: экспертный уровень"
lang: ru
track: claude
kind: module
doc: expert
slug: 12-agents
part: 2
tags: [claude, agents, subagents, agent-sdk, managed-agents]
versions: "Claude Code 2.1.332; claude-agent-sdk 0.2.189; anthropic 2.14.0; managed-agents-2026-04-01"
status: draft
updated: 2026-11-25
---

# Агенты и subagents: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Поля subagent

| Поле | Смысл |
|---|---|
| `name`, `description` | обязательные |
| `tools`, `disallowedTools` | разрешённые и исключённые инструменты |
| `model`, `effort` | `sonnet`, `opus`, `haiku`, `fable`, ID, `inherit` |
| `permissionMode`, `maxTurns` | режим прав и предел ходов |
| `skills`, `mcpServers`, `hooks` | предзагруженные skills, серверы, hooks |
| `memory`, `isolation`, `background`, `omitClaudeMd` | память, worktree, фон, без CLAUDE.md |

## Что показали запросы

| Агент | Инструменты | `CLAUDE.md` |
|---|---|---|
| свой `reviewer` | `Read`, `Grep`, `Glob`; модель haiku | да |
| `Explore` | без `Edit`, `Write`, `Agent` | нет |
| `general-purpose` | включая `Edit`, `Write`, `Agent` | да |

Тело файла агента попадает в его системный промпт. Основному агенту отчёт приходит в `tool_result` с пометкой, что это вывод модели без полномочий пользователя, и с `agentId` для продолжения.

## Подвохи

1. Неизвестный `subagent_type` — ошибка инструмента со списком доступных, а не падение.
2. Subagent на другой модели тратит по её цене: в `modelUsage` результата видны обе.
3. `Explore` не видит `CLAUDE.md`: правила проекта передавайте в задании.
4. В Agent SDK без `setting_sources=[]` локальные настройки и `CLAUDE.md` влияют на ваш продукт.
5. Свой инструмент SDK без `allowed_tools` получает отказ: объявить мало.

> ⚠️ Проверить: фоновые и вложенные subagents, `isolation`, `memory`, agent teams и продолжение по `agentId` описаны по документации.

## Вопросы с собеседований

1. Когда нужен агент?
<details><summary>Ответ</summary>

Задача многошаговая и заранее неописуемая, результат стоит затрат, ошибки обнаружимы и обратимы.
</details>

2. Зачем subagent?
<details><summary>Ответ</summary>

Изолировать контекст, ограничить инструменты, выбрать модель, распараллелить.
</details>

3. Что subagent не знает?
<details><summary>Ответ</summary>

Историю основного разговора.
</details>

4. Subagent и skill?
<details><summary>Ответ</summary>

Skill — инструкции в тот же или (fork) отдельный контекст; subagent — отдельный исполнитель со своими настройками.
</details>

5. Tool Runner и Agent SDK?
<details><summary>Ответ</summary>

Tool Runner крутит цикл ваших инструментов в обычном API; Agent SDK — харнесс Claude Code со встроенными инструментами.
</details>

6. Как ограничить расход?
<details><summary>Ответ</summary>

`max_turns`, бюджет, дешёвые модели для простых частей, отбор инструментов.
</details>

7. Как не доверять отчёту слепо?
<details><summary>Ответ</summary>

Проверяемые критерии, тесты, второй проверяющий, ссылки на файлы.
</details>

8. Чем опасен отчёт, читавший чужой текст?
<details><summary>Ответ</summary>

В нём могут быть инструкции из прочитанного: это данные, не приказы.
</details>

9. Что такое agent teams?
<details><summary>Ответ</summary>

Экспериментальный режим нескольких сессий, обменивающихся сообщениями; по умолчанию выключен.
</details>

10. Foreground и background?
<details><summary>Ответ</summary>

Первый блокирует до отчёта, второй работает параллельно и сообщает позже.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [13 · Длинный контекст](../13-long-context/basics.ru.md)

## Источники

- [Subagents](https://code.claude.com/docs/en/sub-agents)
- [Agent teams](https://code.claude.com/docs/en/agent-teams)
