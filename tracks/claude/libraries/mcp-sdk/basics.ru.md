---
title: "MCP SDK: основы"
lang: ru
track: claude
kind: library
doc: basics
slug: mcp-sdk
part: 2
tags: [claude, mcp, mcp-sdk, tool-use, json-rpc]
versions: "mcp 2.3.0 (Python)"
status: draft
updated: 2026-11-25
---

# MCP SDK: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [08 · MCP](../../modules/08-mcp/overview.ru.md)

## Как проверялся материал

`mcp` 2.3.0: серверы и клиенты запускались в процессе, по stdio и по Streamable HTTP на localhost. Авторизация, настоящие MCP-хосты и TypeScript SDK не проверялись.

## Что это

Официальные библиотеки [Model Context Protocol](../../modules/08-mcp/overview.ru.md): на них пишут серверы, отдающие инструменты, ресурсы и prompts, и клиентов к ним. Python: `pip install mcp` (2.3.0). TypeScript: пакеты `@modelcontextprotocol/server` (2.4.0) и прежний `@modelcontextprotocol/sdk` (1.32.2).

## Сервер

```python
# shop.py
from mcp.server import MCPServer
from mcp.server.mcpserver.exceptions import ToolError
from pydantic import BaseModel

mcp = MCPServer("shop", instructions="Order lookup.")
ORDERS = {"A-1": 10.0}


class Order(BaseModel):
    order_id: str
    total: float


@mcp.tool()
def get_order(order_id: str) -> Order:
    """Return one order."""
    if order_id not in ORDERS:
        raise ToolError(f"unknown order {order_id}")      # this text reaches the model
    return Order(order_id=order_id, total=ORDERS[order_id])


@mcp.resource("orders://{order_id}")
def order_text(order_id: str) -> str:
    return f"order {order_id}"


@mcp.prompt()
def review(order_id: str) -> str:
    return f"Review order {order_id}"


if __name__ == "__main__":
    mcp.run()                                   # stdio by default
```

| Декоратор | Кто выбирает |
|---|---|
| `@mcp.tool()` | модель |
| `@mcp.resource(uri)` | приложение |
| `@mcp.prompt()` | пользователь |

Схема входа строится из аннотаций типов, описание — из docstring, схема выхода — из типа возврата.

## Клиент

```python
import asyncio

from mcp import Client

from shop import mcp


async def main():
    async with Client(mcp) as client:           # in-process, no subprocess or network
        print(client.protocol_version, [t.name for t in (await client.list_tools()).tools])
        ok = await client.call_tool("get_order", {"order_id": "A-1"})
        print(ok.structured_content)
        bad = await client.call_tool("get_order", {"order_id": "Z"})
        print(bad.is_error, bad.content[0].text)
        print((await client.read_resource("orders://A-1")).contents[0].text)
        print((await client.get_prompt("review", {"order_id": "A-1"})).messages[0].content.text)


asyncio.run(main())
```

```text
2026-07-28 ['get_order']
{'order_id': 'A-1', 'total': 10.0}
True Error executing tool get_order: unknown order Z
order A-1
Review order A-1
```

`Client` принимает объект сервера, `StdioServerParameters` или URL.

## Транспорты

- **stdio** — по умолчанию: клиент запускает сервер дочерним процессом.
- **Streamable HTTP** — для удалённого сервера:

```python
from shop import mcp

mcp.run("streamable-http", port=8000)           # serves http://127.0.0.1:8000/mcp
```

- SSE — старый транспорт, для новых серверов не берите.

## Подключить к Claude Code

`claude mcp add shop -- python shop.py` — подробности и области в [модуле 08](../../modules/08-mcp/basics.ru.md).

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [08 · MCP](../../modules/08-mcp/basics.ru.md)

## Источники

- [MCP Python SDK: first steps](https://py.sdk.modelcontextprotocol.io/get-started/first-steps/)
- [Tools](https://py.sdk.modelcontextprotocol.io/servers/tools/)
- [Running your server](https://py.sdk.modelcontextprotocol.io/run/)
