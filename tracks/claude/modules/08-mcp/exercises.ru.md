---
title: "MCP и connectors: упражнения"
lang: ru
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

# MCP и connectors: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Нужны `server.py` и `client.py` из [базовой теории](basics.ru.md).

## 1. Новый инструмент

Добавьте `list_orders()`, возвращающий все идентификаторы.

<details><summary>Подсказка</summary>

Тип результата `list[str]`.

</details>

<details><summary>Решение</summary>

```python
@mcp.tool()
def list_orders() -> list[str]:
    """List the ids of all orders."""
    return sorted(ORDERS)
```

</details>

## 2. Проверка без процесса

Вызовите `order_status` из теста, не запуская отдельный процесс.

<details><summary>Подсказка</summary>

`Client` принимает объект сервера.

</details>

<details><summary>Решение</summary>

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

## 3. Схема для API

Преобразуйте инструменты MCP в список `tools` Messages API без помощников SDK.

<details><summary>Подсказка</summary>

Три поля.

</details>

<details><summary>Решение</summary>

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

## 4. Только чтение

Настройте `mcp_toolset`, отключающий инструменты `cancel_order` и `refund` и оставляющий остальные.

<details><summary>Подсказка</summary>

Список запрещённых в `configs`.

</details>

<details><summary>Решение</summary>

```json
{"type": "mcp_toolset", "mcp_server_name": "shop",
 "configs": {"cancel_order": {"enabled": false}, "refund": {"enabled": false}}}
```

</details>

## 5. Найдите ошибку

Сервер stdio при запуске делает `print("started")`, клиент пишет ошибки разбора. Исправьте.

<details><summary>Подсказка</summary>

Куда идёт stdout?

</details>

<details><summary>Решение</summary>

```python
import sys

print("started", file=sys.stderr)
```

</details>
