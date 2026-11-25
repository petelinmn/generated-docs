---
title: "MCP SDK"
lang: en
track: claude
kind: library
doc: overview
slug: mcp-sdk
part: 2
tags: [claude, mcp, mcp-sdk, tool-use, json-rpc]
versions: "mcp 2.3.0 (Python)"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# MCP SDK

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

**Libraries for Model Context Protocol servers and clients:** a few decorators turn Python functions into tools, resources and prompts.

## Key ideas

- Schemas come from types and docstrings.
- Transports: stdio locally, Streamable HTTP remotely.
- A tool error is a result with `is_error`.
- `ToolError` reaches the model; other exceptions are hidden.
- Test a server with an in-process client.

## Minimal example

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

## When to use it and when not

When several MCP-capable applications need the same tools. For one application of your own, plain tool use is often simpler.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude mcp-sdk level 5` |
| 6. Tests | — | `/learn claude mcp-sdk level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [08 · MCP](../../modules/08-mcp/overview.en.md)
- [Claude Agent SDK](../agent-sdk/overview.en.md)
- [Anthropic SDK](../anthropic-sdk/overview.en.md)
