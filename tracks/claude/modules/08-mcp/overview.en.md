---
title: "MCP and connectors"
lang: en
track: claude
kind: module
doc: overview
slug: 08-mcp
part: 2
tags: [claude, mcp, mcp-connector, tool-use, claude-code]
versions: "MCP specification 2026-07-28; Python SDK mcp 2.3.0; anthropic 2.14.0; anthropic-beta mcp-client-2025-11-20"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# MCP and connectors

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

**MCP** is an open protocol an application uses to connect to servers offering tools, data and templates. This module: your own Python server and client, and connecting it to Claude through the API, the connector and Claude Code.

## Key ideas

- A server offers tools, resources and prompts.
- Transports: stdio locally, Streamable HTTP remotely.
- The 2026-07-28 revision is stateless with no handshake.
- The MCP connector calls only tools of public HTTPS servers.
- A third-party server is untrusted code and untrusted text.

## Minimal example

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

## When to use it and when not

When several applications need the same tools. For one function inside your own application plain tool use is enough.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude 08-mcp level 5` |
| 6. Tests | — | `/learn claude 08-mcp level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [07 · Tool use](../07-tool-use/overview.en.md)
- [09 · Claude Code](../09-claude-code/overview.en.md)
