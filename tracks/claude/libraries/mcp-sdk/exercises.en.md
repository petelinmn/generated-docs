---
title: "MCP SDK: exercises"
lang: en
track: claude
kind: library
doc: exercises
slug: mcp-sdk
part: 2
tags: [claude, mcp, mcp-sdk, tool-use, json-rpc]
versions: "mcp 2.3.0 (Python)"
status: draft
updated: 2026-11-25
---

# MCP SDK: exercises

🌐 [Русский](exercises.ru.md) · **English**

> You need `shop.py` and `jobs.py` from the theory. No network.

## 1. List the tools

Print the input schema of `get_order`.

<details><summary>Hint</summary>

`list_tools`.

</details>

<details><summary>Solution</summary>

```python
import asyncio

from mcp import Client

from shop import mcp


async def main():
    async with Client(mcp) as client:
        print((await client.list_tools()).tools[0].input_schema["required"])


asyncio.run(main())
```

</details>

## 2. Errors

Compare a `ToolError` with a wrong-typed call.

<details><summary>Hint</summary>

`is_error` and the start of the text.

</details>

<details><summary>Solution</summary>

```python
import asyncio

from mcp import Client

from shop import mcp


async def main():
    async with Client(mcp) as client:
        for args in ({"order_id": "Z"}, {"order_id": 5}):
            result = await client.call_tool("get_order", args)
            print(result.is_error, result.content[0].text.splitlines()[0])


asyncio.run(main())
```

</details>

## 3. Progress

Collect the progress values of `run_job`.

<details><summary>Hint</summary>

`progress_callback`.

</details>

<details><summary>Solution</summary>

```python
import asyncio

from mcp import Client

from jobs import mcp


async def main():
    seen = []

    async def on_progress(done, total, message):
        seen.append((done, total))

    async with Client(mcp) as client:
        result = await client.call_tool("run_job", {"steps": 3}, progress_callback=on_progress)
    print(result.content[0].text, seen)


asyncio.run(main())
```

</details>

## 4. An unknown resource

Catch the error of reading a missing URI.

<details><summary>Hint</summary>

`MCPError`.

</details>

<details><summary>Solution</summary>

```python
import asyncio

from mcp import Client, MCPError

from shop import mcp


async def main():
    async with Client(mcp) as client:
        try:
            await client.read_resource("missing://x")
        except MCPError as error:
            print(error.code)


asyncio.run(main())
```

</details>
