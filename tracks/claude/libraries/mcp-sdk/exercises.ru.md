---
title: "MCP SDK: упражнения"
lang: ru
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

# MCP SDK: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Нужны `shop.py` и `jobs.py` из теории. Сеть не нужна.

## 1. Список инструментов

Напечатайте схему входа `get_order`.

<details><summary>Подсказка</summary>

`list_tools`.

</details>

<details><summary>Решение</summary>

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

## 2. Ошибки

Сравните `ToolError` и вызов с неверным типом.

<details><summary>Подсказка</summary>

`is_error` и начало текста.

</details>

<details><summary>Решение</summary>

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

## 3. Прогресс

Соберите значения прогресса `run_job`.

<details><summary>Подсказка</summary>

`progress_callback`.

</details>

<details><summary>Решение</summary>

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

## 4. Неизвестный ресурс

Перехватите ошибку чтения несуществующего URI.

<details><summary>Подсказка</summary>

`MCPError`.

</details>

<details><summary>Решение</summary>

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
