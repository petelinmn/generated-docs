---
title: "MCP SDK: квиз"
lang: ru
track: claude
kind: library
doc: quiz
slug: mcp-sdk
part: 2
tags: [claude, mcp, mcp-sdk, tool-use, json-rpc]
versions: "mcp 2.3.0 (Python)"
status: draft
updated: 2026-11-25
---

# MCP SDK: квиз

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Каким декоратором объявляют то, что вызывает модель?

- A) `@mcp.prompt()`
- B) `@mcp.completion()`
- C) `@mcp.tool()`
- D) `@mcp.resource()`

<details><summary>Ответ</summary>

**C.** Инструментами управляет модель. Ресурсы подключает приложение, prompts выбирает пользователь, completion подсказывает значения аргументов.
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Транспорт `mcp.run()` по умолчанию?

- A) stdio
- B) Streamable HTTP
- C) SSE
- D) WebSocket

<details><summary>Ответ</summary>

**A.** Без аргумента сервер работает по stdio. HTTP и SSE указывают явно, WebSocket среди вариантов `run` нет.
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Откуда берётся описание инструмента?

- A) из имени файла
- B) его придумывает модель
- C) из `pyproject.toml`
- D) из docstring

<details><summary>Ответ</summary>

**D.** SDK берёт docstring функции, поэтому его пишут для модели. Остальное на описание не влияет.
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Как проще всего протестировать сервер?

- A) никак
- B) `Client(mcp)` в процессе
- C) только через Claude Desktop
- D) curl по stdio

<details><summary>Ответ</summary>

**B.** Клиент принимает объект сервера и проходит протокол без сети и процессов. `curl` по stdio не работает.
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Инструмент бросил `RuntimeError('password=…')`. Что получит клиент?

- A) успешный результат
- B) общее сообщение без текста
- C) полный текст исключения
- D) разрыв соединения

<details><summary>Ответ</summary>

**B.** `MCPServer` скрывает текст непредвиденного исключения и отдаёт `is_error`. В логе сервера он остаётся.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Как узнать, что вызов инструмента не удался?

- A) ждать исключения
- B) по пустому `content`
- C) по HTTP-коду
- D) проверить `result.is_error`

<details><summary>Ответ</summary>

**D.** Ошибка инструмента — обычный результат с флагом. Исключения нет, `content` не пуст, а транспорт отвечает успешно.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что делает `print()` в stdio-сервере?

- A) портит поток протокола
- B) пишет в лог клиента как задумано
- C) ничего
- D) останавливает сервер всегда

<details><summary>Ответ</summary>

**A.** Клиент читает stdout как JSON-RPC и получает ошибку разбора. Для отладки есть stderr.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Где виден ресурс `orders://{order_id}`?

- A) в `list_tools`
- B) в `list_prompts`
- C) в `list_resource_templates`
- D) в `list_resources`

<details><summary>Ответ</summary>

**C.** URI с параметром — это шаблон. `list_resources` перечисляет только конкретные URI.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Какую версию протокола дал клиент 2.3.0 по умолчанию?

- A) `2025-11-25`
- B) `1.0`
- C) `2024-01-01`
- D) `2026-07-28`

<details><summary>Ответ</summary>

**D.** `2025-11-25` получается в режиме `legacy`. Остальные значения версиями этой проверки не были.
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Что ответил локальный HTTP-сервер на чужой `Host`?

- A) 500
- B) 421
- C) 200
- D) 401

<details><summary>Ответ</summary>

**B.** 421 Misdirected Request — защита от DNS rebinding. Это не 401: никакой аутентификации здесь нет.
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Что вернётся в `structured_content`, если инструмент возвращает `str`?

- A) `None` всегда
- B) список символов
- C) объект с ключом `result`
- D) сама строка

<details><summary>Ответ</summary>

**C.** Примитив оборачивается в объект, потому что структурированный результат описан схемой объекта. Pydantic-модель отдаётся своими полями.
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: сервер с доступом к заказам нужно опубликовать для команды. Что сделать?

<details><summary>Ответ</summary>

Streamable HTTP за TLS; OAuth или проверка токенов; права на уровне пользователя, а не сервера; `ToolError` без внутренних деталей; ограничения размеров и частоты; журнал вызовов; тесты через `Client`; фиксированная версия SDK.
→ [Продвинутая теория](advanced.ru.md)

</details>
