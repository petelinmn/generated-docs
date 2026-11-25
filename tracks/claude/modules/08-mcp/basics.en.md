---
title: "MCP and connectors: fundamentals"
lang: en
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

# MCP and connectors: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [07 · Tool use](../07-tool-use/overview.en.md)

## How the code was checked

The MCP server and client ran for real: the Python `mcp` 2.3.0 package over stdio and Streamable HTTP on a local machine. There is no API key: the Claude part ran against a local Messages API stand-in, so request shapes were checked, not model decisions.

## Why MCP

A [tool use](../07-tool-use/overview.en.md) tool lives inside your application. The **Model Context Protocol (MCP)** moves tools into a separate server speaking a standard protocol, so one server serves many applications: Claude Code, IDEs, your own code.

| Party | Role |
|---|---|
| host | the application with the model |
| client | the application's connection to one server |
| server | offers capabilities |

| What a server offers | What it is | Who decides to use it |
|---|---|---|
| tools | actions | the model |
| resources | data addressed by URI | the application |
| prompts | message templates | the user |

## A server

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

Type hints and the docstring become the tool's schema and description: that is what the model sees.

## A client

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

A tool failure arrives as an ordinary result with `isError`, so the model can read it and recover.

## Transports

| Transport | How they connect | When |
|---|---|---|
| stdio | the client starts the server as a process; messages go over stdin/stdout | local tools |
| Streamable HTTP | HTTP requests to one endpoint | a shared remote server |

The older HTTP+SSE transport is deprecated.

## Common beginner mistakes

- `print` to stdout in a stdio server: stdout is for the protocol only. Write to stderr.
- Vague docstrings: the model cannot tell when to call.
- Connecting a stranger's server blindly: it receives arguments and returns text the model reads.

## Related materials

- [Advanced theory](advanced.en.md)
- [07 · Tool use](../07-tool-use/basics.en.md)

## Sources

- [Architecture overview](https://modelcontextprotocol.io/docs/learn/architecture)
- [stdio transport](https://modelcontextprotocol.io/specification/2026-07-28/basic/transports/stdio)
- [Tools](https://modelcontextprotocol.io/specification/2026-07-28/server/tools)
