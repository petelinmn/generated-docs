---
title: "Агенты и subagents: квиз"
lang: ru
track: claude
kind: module
doc: quiz
slug: 12-agents
part: 2
tags: [claude, agents, subagents, agent-sdk, managed-agents]
versions: "Claude Code 2.1.332; claude-agent-sdk 0.2.189; anthropic 2.14.0; managed-agents-2026-04-01"
status: draft
updated: 2026-11-25
---

# Агенты и subagents: квиз

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Что возвращается основному агенту от subagent?

- A) его системный промпт
- B) ничего
- C) отчёт
- D) вся его история

<details><summary>Ответ</summary>

**C.** Subagent работает в своём окне и отдаёт только отчёт — ради этого основной контекст и остаётся чистым. Его история и системный промпт не возвращаются.
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Где проектный subagent?

- A) `.claude/agents/`
- B) `.claude/skills/`
- C) `.claude/hooks/`
- D) `agents.json`

<details><summary>Ответ</summary>

**A.** Описания проектных subagents лежат в `.claude/agents/`. `skills` и `hooks` относятся к другим механизмам, а `agents.json` для этого не служит.
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Видит ли subagent историю основного разговора?

- A) да, всю
- B) только последние 10 сообщений
- C) только при `model: inherit`
- D) нет

<details><summary>Ответ</summary>

**D.** Обычный subagent начинает с чистого окна и знает только то, что передано в задании и его настройке. `model: inherit` выбирает модель, а не историю.
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Встроенный агент для поиска без правок?

- A) `Deploy`
- B) `Explore`
- C) `reviewer`
- D) `Edit`

<details><summary>Ответ</summary>

**B.** `Explore` — встроенный агент только для поиска и чтения. `reviewer` — пример пользовательского агента, `Edit` — инструмент, а `Deploy` встроенным не является.
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Как запретить запуск агента `reviewer`?

- A) `model: none`
- B) `deny: Agent(reviewer)`
- C) `tools: []` у основного
- D) удалить `description`

<details><summary>Ответ</summary>

**B.** Правило `Agent(имя)` в `deny` запрещает запуск конкретного агента. Удаление описания и смена модели запретом не являются, а пустые `tools` у основного агента отключили бы всё остальное.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что делает `setting_sources=[]` в Agent SDK?

- A) отключает инструменты
- B) запрещает сеть
- C) выключает hooks из кода
- D) не читает настройки и `CLAUDE.md` с диска

<details><summary>Ответ</summary>

**D.** Пустой список источников означает не читать настройки и инструкции с диска. Инструменты, сеть и hooks, заданные в коде, от этого не отключаются.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что нужно, чтобы свой инструмент SDK выполнялся без вопроса?

- A) указать `mcp__<server>__<tool>` в `allowed_tools`
- B) достаточно `@tool`
- C) `max_turns`
- D) `setting_sources`

<details><summary>Ответ</summary>

**A.** Объявить инструмент мало: вызов требует разрешения, пока полное имя не внесено в `allowed_tools`. `max_turns` и `setting_sources` прав не дают.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что верно о Managed Agents?

- [ ] A) они подходят под Zero Data Retention
- [ ] B) агента создают на каждый запрос
- [ ] C) цикл и песочницу размещает Anthropic
- [ ] D) они в бете

<details><summary>Ответ</summary>

**C, D.** Managed Agents — это размещённый Anthropic цикл с песочницей, сервис в бете. Под ZDR он не подпадает, а агент создаётся один раз и переиспользуется в сессиях.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Какой встроенный агент не получает `CLAUDE.md`?

- A) `general-purpose`
- B) свой из `.claude/agents`
- C) все получают
- D) `Explore`

<details><summary>Ответ</summary>

**D.** `Explore` (как и встроенный `Plan`) не получает `CLAUDE.md`, поэтому правила проекта передают в задании. `general-purpose` и свои агенты его получают.
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Subagent получил отказ в правах. Где это видно?

- A) в коде выхода 2
- B) в `permission_denials` основного запуска
- C) нигде
- D) только в файле агента

<details><summary>Ответ</summary>

**B.** Отказ subagent попадает в общий список запуска. Кодом выхода 2 он не обозначается, а в файле агента ничего не записывается.
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Вложенность subagents по умолчанию:

- A) не ограничена
- B) ровно один
- C) до трёх уровней
- D) запрещена

<details><summary>Ответ</summary>

**C.** По умолчанию вложенность допускается до трёх уровней ниже основного разговора: она возможна, но не бесконечна.
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: как устроить ревью большого PR агентами?

<details><summary>Ответ</summary>

Разделить по областям или аспектам; read-only subagents с узкими инструментами; передать каждому контекст и критерии; параллельно; отдельная проверка найденного; сводит основной агент; лимиты ходов и бюджета; человек принимает решение.
→ [Продвинутая теория](advanced.ru.md)

</details>
