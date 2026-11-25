---
title: "MCP and connectors: exercises"
lang: en
track: claude
kind: module
doc: exercises
slug: 08-mcp
part: 2
tags: [claude, mcp, mcp-connector, tool-use, claude-code]
versions: "MCP specification 2026-07-28; Python SDK mcp 2.3.0; anthropic 2.14.0; anthropic-beta mcp-client-2025-11-20"
status: draft
updated: 2026-11-25
---

# MCP and connectors: exercises

🌐 [Русский](exercises.ru.md) · **English**

> You need `server.py` and `client.py` from the [fundamentals](basics.en.md).

## 1. A new tool

Add `list_orders()` returning every id.

<details><summary>Hint</summary>

Return type `list[str]`.

</details>

<details><summary>Solution</summary>

```python
@mcp.tool()
def list_orders() -> list[str]:
    """List the ids of all orders."""
    return sorted(ORDERS)
```

</details>

## 2. Testing without a process

Call `order_status` from a test without starting a process.

<details><summary>Hint</summary>

`Client` accepts the server object.

</details>

<details><summary>Solution</summary>

```python
import asyncio

from mcp import Client

from server import mcp


async def check():
    async with Client(mcp) as client:
        r = await client.call_tool("order_status", {"order_id": "A-2"})
        assert r.structured_content == {"result": "pending"}
        print("ok")


asyncio.run(check())
```

</details>

## 3. Schema for the API

Convert MCP tools into the Messages API `tools` list without SDK helpers.

<details><summary>Hint</summary>

Three fields.

</details>

<details><summary>Solution</summary>

```python
import asyncio

from mcp import Client

from server import mcp


async def convert():
    async with Client(mcp) as client:
        return [{"name": t.name, "description": t.description or "", "input_schema": t.input_schema}
                for t in (await client.list_tools()).tools]


print([t["name"] for t in asyncio.run(convert())])
```

</details>

## 4. Read only

Configure an `mcp_toolset` that disables `cancel_order` and `refund` and keeps the rest.

<details><summary>Hint</summary>

A denylist in `configs`.

</details>

<details><summary>Solution</summary>

```json
{"type": "mcp_toolset", "mcp_server_name": "shop",
 "configs": {"cancel_order": {"enabled": false}, "refund": {"enabled": false}}}
```

</details>

## 5. Find the bug

A stdio server does `print("started")` on start-up and the client logs parse errors. Fix it.

<details><summary>Hint</summary>

Where does stdout go?

</details>

<details><summary>Solution</summary>

```python
import sys

print("started", file=sys.stderr)
```

</details>
