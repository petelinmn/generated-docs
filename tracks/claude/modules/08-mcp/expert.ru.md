---
title: "MCP и connectors: экспертный уровень"
lang: ru
track: claude
kind: module
doc: expert
slug: 08-mcp
part: 2
tags: [claude, mcp, mcp-connector, tool-use, claude-code]
versions: "MCP specification 2026-07-28; Python SDK mcp 2.3.0; anthropic 2.14.0; anthropic-beta mcp-client-2025-11-20"
status: draft
updated: 2026-11-25
---

# MCP и connectors: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Протокол 2026-07-28

Сообщения — JSON-RPC 2.0. Протокол **без состояния**: рукопожатия `initialize` и сессий больше нет, каждый запрос несёт версию и возможности клиента в `_meta`.

```json
{"jsonrpc": "2.0", "id": 2, "method": "tools/call", "params": {
  "name": "add", "arguments": {"a": 2, "b": 3},
  "_meta": {"io.modelcontextprotocol/protocolVersion": "2026-07-28",
            "io.modelcontextprotocol/clientCapabilities": {}}}}
{"jsonrpc": "2.0", "id": 2, "result": {"content": [{"type": "text", "text": "5"}], "isError": false,
  "resultType": "complete", "structuredContent": {"result": 5}, "_meta": {"...": "..."}}}
```

| Метод | Назначение |
|---|---|
| `server/discover` | версии, возможности, сведения о сервере |
| `tools/list`, `tools/call` | инструменты |
| `resources/list`, `resources/templates/list`, `resources/read` | ресурсы |
| `prompts/list`, `prompts/get` | шаблоны |
| `subscriptions/listen` | поток уведомлений об изменениях |

Результаты содержат `resultType` (`complete` или `input_required`). `input_required` — это multi round-trip: сервер просит данные, клиент повторяет запрос с ответами. Удалены `ping`, `logging/setLevel`, `resources/subscribe`.

## Ошибки, виденные на сервере SDK

| Случай | Ответ |
|---|---|
| нет `_meta` с версией | `-32602` |
| версия `1999-01-01` | `-32022`, `data.supported` |
| `ping` | `-32601 Method not found` |
| неизвестный ресурс | `-32602` |
| HTTP: заголовок `Mcp-Method` не совпадает с телом | 400, `-32020` |
| HTTP: посторонний `Origin` | 403 |

## Streamable HTTP

POST с заголовками `MCP-Protocol-Version`, `Mcp-Method` и `Mcp-Name`; `Accept: application/json, text/event-stream`. Заголовка `Mcp-Session-Id` и возобновления потока по `Last-Event-ID` в этой ревизии нет.

## Совместимость

`Client(..., mode="legacy")` с тем же сервером договорился о `2025-11-25`; по умолчанию (`auto`) — о `2026-07-28`.

## MCP connector: поля и бета-возможности

| Поле | Правило |
|---|---|
| `mcp_servers[].type` | только `url` |
| `mcp_servers[].url` | начинается с `https://` |
| `configs.<tool>.enabled` / `defer_loading` | по умолчанию `true` / `false` |
| приоритет | `configs` > `default_config` > значения по умолчанию |
| неизвестное имя в `configs` | не ошибка |

С бета-заголовком `mcp-client-2026-09-15` ответ содержит блок `mcp_tool_listing`, а список инструментов можно зафиксировать в `tools` набора, чтобы изменения сервера не меняли разговор.

**MCP tunnels** — исследовательская предварительная версия: `cloudflared` и прокси внутри вашей сети открывают исходящее соединение.

> ⚠️ Проверить: авторизация OAuth, `input_required`, подписки, расширения (tasks), закрепление списка и туннели описаны по документации и не запускались.

## Подвохи

1. Обычное исключение в инструменте клиент увидел только как `Error executing tool order_status`: текст скрыт. Для сообщения модели бросайте `ToolError`.
2. `print` при старте stdio-сервера вызвал у клиента ошибку разбора `Failed to parse JSONRPC message`.
3. Неверный тип аргумента вернулся результатом с `isError`, а не ошибкой протокола; так же ответил вызов неизвестного инструмента в этом SDK.
4. Сведения из старых статей про `initialize`, `FastMCP` и `Mcp-Session-Id` относятся к прежним ревизиям и версиям SDK.

## Вопросы с собеседований

1. Чем MCP отличается от tool use?
<details><summary>Ответ</summary>

Tool use — механизм вызова в API модели; MCP — протокол между приложением и сервером возможностей. Инструменты MCP передаются модели через tool use.
</details>

2. Tools, resources, prompts?
<details><summary>Ответ</summary>

Действия, выбираемые моделью; данные, выбираемые приложением; шаблоны, выбираемые пользователем.
</details>

3. Что изменилось в 2026-07-28?
<details><summary>Ответ</summary>

Протокол стал без состояния: нет `initialize` и сессий, версия и возможности идут в `_meta` каждого запроса, появился `server/discover`.
</details>

4. Ошибка протокола и ошибка инструмента?
<details><summary>Ответ</summary>

Первая — JSON-RPC `error`; вторая — результат с `isError: true`, который видит модель.
</details>

5. Почему нельзя печатать в stdout?
<details><summary>Ответ</summary>

В stdio по нему идут сообщения протокола.
</details>

6. Какие риски у стороннего сервера?
<details><summary>Ответ</summary>

Утечка аргументов, инъекция инструкций в результатах и описаниях, смена списка инструментов, опасные действия.
</details>

7. Когда коннектор, а когда свой клиент?
<details><summary>Ответ</summary>

Коннектор — для публичных HTTP-серверов и только инструментов; свой клиент — для локальных серверов, resources, prompts, своей политики подтверждений.
</details>

8. Что такое Roots, Sampling, Logging сегодня?
<details><summary>Ответ</summary>

Объявлены устаревшими в 2026-07-28.
</details>

9. Как сервер хранит состояние без сессий?
<details><summary>Ответ</summary>

Выдаёт явный идентификатор, который клиент передаёт обычным аргументом инструмента.
</details>

10. Что такое MCP tunnels?
<details><summary>Ответ</summary>

Исследовательская предварительная версия: доступ Claude к серверам в частной сети через исходящее соединение.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [07 · Tool use](../07-tool-use/expert.ru.md)

## Источники

- [Key changes in 2026-07-28](https://modelcontextprotocol.io/specification/2026-07-28/changelog)
- [Versioning](https://modelcontextprotocol.io/specification/versioning)
- [Streamable HTTP](https://modelcontextprotocol.io/specification/2026-07-28/basic/transports/streamable-http)
- [MCP tunnels](https://platform.claude.com/docs/en/agents-and-tools/mcp-tunnels/overview)
