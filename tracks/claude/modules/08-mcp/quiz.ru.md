---
title: "MCP и connectors: квиз"
lang: ru
track: claude
kind: module
doc: quiz
slug: 08-mcp
part: 2
tags: [claude, mcp, mcp-connector, tool-use, claude-code]
versions: "MCP specification 2026-07-28; Python SDK mcp 2.3.0; anthropic 2.14.0; anthropic-beta mcp-client-2025-11-20"
status: draft
updated: 2026-11-25
---

# MCP и connectors: квиз

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Что из перечисленного обычно выбирает модель?

- A) prompts
- B) транспорт
- C) tools
- D) resources

<details><summary>Ответ</summary>

**C.**
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Транспорт для локального сервера, запускаемого процессом?

- A) stdio
- B) Streamable HTTP
- C) SSE
- D) WebSocket

<details><summary>Ответ</summary>

**A.**
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Где stdio-серверу писать отладочные сообщения?

- A) в stdout
- B) в тело ответа
- C) в `_meta`
- D) в stderr

<details><summary>Ответ</summary>

**D.**
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Откуда берётся схема инструмента в `MCPServer`?

- A) из `README`
- B) из типов параметров функции
- C) из имени файла
- D) её пишет модель

<details><summary>Ответ</summary>

**B.**
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Что умеет MCP connector?

- A) любые транспорты
- B) только вызовы инструментов по HTTPS
- C) локальные stdio-серверы
- D) prompts и resources

<details><summary>Ответ</summary>

**B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Как разрешить только один инструмент сервера?

- A) удалить остальные на сервере
- B) `tool_choice: none`
- C) перечислить в `mcp_servers.url`
- D) `default_config.enabled: false` и включить его в `configs`

<details><summary>Ответ</summary>

**D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Область `claude mcp add`, попадающая в git?

- A) `project`
- B) `local`
- C) `user`
- D) любая

<details><summary>Ответ</summary>

**A.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что верно о сервере из `.mcp.json` в новом проекте?

- [ ] A) подключается без вопросов
- [ ] B) доступен только в API
- [ ] C) ждёт одобрения пользователя
- [ ] D) его инструменты называются `mcp__<server>__<tool>`

<details><summary>Ответ</summary>

**C, D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что верно о ревизии 2026-07-28?

- [ ] A) версия передаётся в `_meta` каждого запроса
- [ ] B) сессии стали обязательными
- [ ] C) добавлен `ping`
- [ ] D) нет рукопожатия `initialize`

<details><summary>Ответ</summary>

**A, D.**
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Инструмент бросил `ValueError("secret detail")`. Что получит клиент SDK?

- A) разрыв соединения
- B) общее сообщение без текста исключения
- C) полный traceback
- D) `secret detail`

<details><summary>Ответ</summary>

**B.**
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Код ошибки для неподдерживаемой версии протокола?

- A) `-32700`
- B) `404`
- C) `-32022`
- D) `-32601`

<details><summary>Ответ</summary>

**C.**
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: коллега предлагает подключить найденный в интернете MCP-сервер к агенту с доступом к репозиторию. Что проверить?

<details><summary>Ответ</summary>

Кто автор и что делает код; какие инструменты и что они меняют; какие данные уходят наружу; включать по списку разрешённых; секреты не передавать; результаты считать недоверенными; изолированная среда и подтверждение опасных действий; фиксировать версию.
→ [Продвинутая теория](advanced.ru.md)

</details>
