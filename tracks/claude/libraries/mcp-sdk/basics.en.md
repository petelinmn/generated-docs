---
title: "MCP SDK: fundamentals"
lang: en
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

# MCP SDK: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [08 · MCP](../../modules/08-mcp/overview.en.md)

## How this was checked

`mcp` 2.3.0: servers and clients ran in-process, over stdio and over Streamable HTTP on localhost. Authorisation, real MCP hosts and the TypeScript SDK were not checked.

## What it is

The official [Model Context Protocol](../../modules/08-mcp/overview.en.md) libraries for servers that expose tools, resources and prompts, and for clients of them. Python: `pip install mcp` (2.3.0). TypeScript: `@modelcontextprotocol/server` (2.4.0) and the earlier `@modelcontextprotocol/sdk` (1.32.2).

## A server

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

| Decorator | Chosen by |
|---|---|
| `@mcp.tool()` | the model |
| `@mcp.resource(uri)` | the application |
| `@mcp.prompt()` | the user |

The input schema comes from type hints, the description from the docstring, the output schema from the return type.

## A client

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

`Client` accepts a server object, `StdioServerParameters` or a URL.

## Transports

- **stdio**, the default: the client spawns the server.
- **Streamable HTTP** for a remote server:

```python
from shop import mcp

mcp.run("streamable-http", port=8000)           # serves http://127.0.0.1:8000/mcp
```

- SSE is the old transport; do not use it for new servers.

## Connect to Claude Code

`claude mcp add shop -- python shop.py`; details and scopes are in [module 08](../../modules/08-mcp/basics.en.md).

## Related materials

- [Advanced theory](advanced.en.md)
- [08 · MCP](../../modules/08-mcp/basics.en.md)

## Sources

- [MCP Python SDK: first steps](https://py.sdk.modelcontextprotocol.io/get-started/first-steps/)
- [Tools](https://py.sdk.modelcontextprotocol.io/servers/tools/)
- [Running your server](https://py.sdk.modelcontextprotocol.io/run/)
