---
title: "MCP SDK"
lang: ru
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

🌐 **Русский** · [English](overview.en.md)

## Коротко

**Библиотеки для серверов и клиентов Model Context Protocol:** несколько декораторов превращают функции Python в инструменты, ресурсы и prompts.

## Ключевые идеи

- Схемы строятся из типов и docstring.
- Транспорты: stdio локально, Streamable HTTP удалённо.
- Ошибка инструмента — результат с `is_error`.
- `ToolError` доходит до модели, прочие исключения скрыты.
- Сервер тестируется клиентом в процессе.

## Минимальный пример

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

## Когда применять и когда нет

Когда один набор инструментов нужен нескольким приложениям с поддержкой MCP. Для одного своего приложения часто проще обычный tool use.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude mcp-sdk level 5` |
| 6. Тесты | — | `/learn claude mcp-sdk level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [08 · MCP](../../modules/08-mcp/overview.ru.md)
- [Claude Agent SDK](../agent-sdk/overview.ru.md)
- [Anthropic SDK](../anthropic-sdk/overview.ru.md)
