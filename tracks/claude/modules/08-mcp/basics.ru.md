---
title: "MCP и connectors: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 08-mcp
part: 2
tags: [claude, mcp, mcp-connector, tool-use, claude-code]
versions: "MCP specification 2026-07-28; Python SDK mcp 2.3.0; anthropic 2.14.0; anthropic-beta mcp-client-2025-11-20"
status: draft
updated: 2026-11-25
---

# MCP и connectors: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [07 · Tool use](../07-tool-use/overview.ru.md)

## Как проверялся код

Сервер и клиент MCP запускались по-настоящему: Python-пакет `mcp` 2.3.0, транспорт stdio и Streamable HTTP на локальной машине. Ключа API нет: часть с Claude проверялась на локальной заглушке Messages API, то есть проверены формы запросов, а не решения модели.

## Зачем MCP

Инструмент из [tool use](../07-tool-use/overview.ru.md) живёт внутри вашего приложения. **Model Context Protocol (MCP)** выносит инструменты в отдельный сервер со стандартным протоколом, и один сервер работает с разными приложениями: Claude Code, IDE, вашим кодом.

| Участник | Роль |
|---|---|
| host | приложение с моделью |
| client | соединение приложения с одним сервером |
| server | предоставляет возможности |

| Что даёт сервер | Что это | Кто решает использовать |
|---|---|---|
| tools | действия | модель |
| resources | данные по URI | приложение |
| prompts | шаблоны сообщений | пользователь |

## Сервер

```bash
pip install "mcp==2.3.0"
```

```python
# server.py
from mcp.server.mcpserver import MCPServer
from mcp.server.mcpserver.exceptions import ToolError

mcp = MCPServer("shop", instructions="Look up orders of a demo shop.")
ORDERS = {"A-1": "shipped", "A-2": "pending"}


@mcp.tool()
def order_status(order_id: str) -> str:
    """Return the status of an order by its id, for example A-1."""
    if order_id not in ORDERS:
        raise ToolError(f"unknown order {order_id}; ids look like A-1")
    return ORDERS[order_id]


@mcp.resource("shop://orders/{order_id}")
def order_resource(order_id: str) -> str:
    """One order as text."""
    return f"{order_id}: {ORDERS.get(order_id, 'unknown')}"


@mcp.prompt()
def support_reply(order_id: str) -> str:
    """Draft a reply about an order."""
    return f"Write a short reply about order {order_id}."


if __name__ == "__main__":
    mcp.run()  # stdio by default
```

Типы и docstring становятся схемой и описанием инструмента: модель видит именно их.

## Клиент

```python
# client.py
import asyncio
import sys

from mcp import Client, StdioServerParameters


async def main():
    params = StdioServerParameters(command=sys.executable, args=["server.py"])
    async with Client(params) as client:
        print(client.protocol_version, client.server_info.name)
        tools = (await client.list_tools()).tools
        print([t.name for t in tools])
        for order_id in ("A-1", "Z"):
            r = await client.call_tool("order_status", {"order_id": order_id})
            print(r.is_error, r.content[0].text)
        print((await client.read_resource("shop://orders/A-2")).contents[0].text)
        print((await client.get_prompt("support_reply", {"order_id": "A-1"})).messages[0].content.text)


asyncio.run(main())
```

```text
2026-07-28 shop
['order_status']
False shipped
True Error executing tool order_status: unknown order Z; ids look like A-1
A-2: pending
Write a short reply about order A-1.
```

Ошибка инструмента приходит обычным результатом с `isError`, чтобы модель могла прочитать и исправиться.

## Транспорты

| Транспорт | Как связаны | Когда |
|---|---|---|
| stdio | клиент запускает сервер процессом, сообщения идут через stdin/stdout | локальные инструменты |
| Streamable HTTP | HTTP-запросы на один адрес | общий удалённый сервер |

Старый транспорт HTTP+SSE объявлен устаревшим.

## Частые ошибки новичков

- `print` в stdout у stdio-сервера: в stdout должен идти только протокол. Пишите в stderr.
- Расплывчатые docstring: модель не поймёт, когда вызывать.
- Подключать чужой сервер, не глядя: он получает аргументы и возвращает текст, который читает модель.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [07 · Tool use](../07-tool-use/basics.ru.md)

## Источники

- [Architecture overview](https://modelcontextprotocol.io/docs/learn/architecture)
- [stdio transport](https://modelcontextprotocol.io/specification/2026-07-28/basic/transports/stdio)
- [Tools](https://modelcontextprotocol.io/specification/2026-07-28/server/tools)
