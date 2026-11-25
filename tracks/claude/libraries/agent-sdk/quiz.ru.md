---
title: "Claude Agent SDK: квиз"
lang: ru
track: claude
kind: library
doc: quiz
slug: agent-sdk
part: 2
tags: [claude, agent-sdk, agents, tool-use, hooks]
versions: "claude-agent-sdk 0.2.189"
status: draft
updated: 2026-11-25
---

# Claude Agent SDK: квиз

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Что Agent SDK даёт сверх клиента API?

- A) свои модели
- B) офлайн-работу
- C) готовый агентный цикл с инструментами
- D) более дешёвые токены

<details><summary>Ответ</summary>

**C.** Это Claude Code как библиотека. Модели и цены те же, и без доступа к API он не работает.
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Какое сообщение приходит последним в `query()`?

- A) `ResultMessage`
- B) `SystemMessage`
- C) `UserMessage`
- D) `StreamEvent`

<details><summary>Ответ</summary>

**A.** `ResultMessage` подводит итог запуска. `SystemMessage` идёт в начале, `UserMessage` несёт результаты инструментов, `StreamEvent` — частичные ответы.
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Полное имя инструмента `order_status` сервера `shop`?

- A) `shop.order_status`
- B) `order_status`
- C) `shop/order_status`
- D) `mcp__shop__order_status`

<details><summary>Ответ</summary>

**D.** Инструменты MCP получают префикс `mcp__<сервер>__`. Правила прав с другими записями к нему не применятся.
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Зачем `setting_sources=[]`?

- A) отключить сессии
- B) не читать настройки с диска
- C) отключить инструменты
- D) запретить сеть

<details><summary>Ответ</summary>

**B.** Пустой список делает запуск независимым от машины. Инструменты, сеть и запись сессий он не отключает.
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Инструмент в `allowed_tools` и задан `can_use_tool`. Что будет?

- A) ошибка запуска
- B) callback не вызовется
- C) вызовется дважды
- D) вызов будет отклонён

<details><summary>Ответ</summary>

**B.** Разрешение правилом наступает раньше callback; SDK лишь предупреждает. Для проверки каждого вызова нужен hook.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что происходит при исчерпании `max_turns` в `query()`?

- [ ] A) затем поднимается `ResultError`
- [ ] B) тихий `success`
- [ ] C) бесконечный повтор
- [ ] D) приходит результат `error_max_turns`

<details><summary>Ответ</summary>

**A, D.** Сначала отдаётся сообщение с ошибкой, потом исключение, поэтому результат надо сохранить до перехвата. Успехом это не считается.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что делает `resume` вместе с `fork_session=True`?

- A) создаёт ветку с новым ID
- B) перезаписывает исходную
- C) удаляет историю
- D) объединяет две сессии

<details><summary>Ответ</summary>

**A.** История копируется в новую сессию, исходная остаётся как была. Ни удаления, ни слияния нет.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Как надёжно проверить каждый вызов инструмента?

- A) фраза в системном промпте
- B) `allowed_tools`
- C) `PreToolUse` hook
- D) `can_use_tool`

<details><summary>Ответ</summary>

**C.** Hook срабатывает на каждом вызове, включая разрешённые. Callback затеняется, промпт не гарантирует, а `allowed_tools` одобряет без проверки.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Инструмент бросил `ValueError('password=…')`. Что увидит модель?

- A) только «ошибка инструмента»
- B) ничего
- C) traceback без текста
- D) этот текст

<details><summary>Ответ</summary>

**D.** SDK-сервер передаёт сообщение исключения в результат. Так ведёт себя именно Agent SDK; `MCPServer` из MCP SDK текст скрывает.
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Что означает `allowed_tools`?

- A) порядок вызова
- B) список автоодобряемых вызовов
- C) единственные доступные инструменты
- D) список для логирования

<details><summary>Ответ</summary>

**B.** Остальные инструменты остаются доступны и проходят обычную проверку прав. Для удаления есть `disallowed_tools` и `tools`.
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Почему `resume` не нашёл сессию на другом сервере?

- A) сессии лежат только в API
- B) нужен другой ключ SDK
- C) сессии — локальные файлы
- D) ID действует минуту

<details><summary>Ответ</summary>

**C.** Файлы лежат в конфигурации той машины и зависят от каталога. Нужны перенос или `session_store`.
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: агент в веб-сервисе чинит тикеты клиентов. Что настроить?

<details><summary>Ответ</summary>

Изоляция на уровне контейнера на клиента; `setting_sources=[]`; узкие `tools`; hook на опасные действия; инструменты с составленными сообщениями об ошибках; `max_turns` и бюджет; разбор `permission_denials`; хранение и удаление сессий; API-ключ, а не вход claude.ai.
→ [Продвинутая теория](advanced.ru.md)

</details>
