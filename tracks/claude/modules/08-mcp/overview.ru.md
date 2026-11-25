---
title: "MCP и connectors"
lang: ru
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

# MCP и connectors

🌐 **Русский** · [English](overview.en.md)

## Коротко

**MCP** — открытый протокол, по которому приложение подключается к серверам с инструментами, данными и шаблонами. Модуль: свой сервер и клиент на Python, подключение к Claude через API, коннектор и Claude Code.

## Ключевые идеи

- Сервер даёт tools, resources и prompts.
- Транспорты: stdio локально, Streamable HTTP удалённо.
- Ревизия 2026-07-28 — без состояния и без рукопожатия.
- MCP connector вызывает только инструменты публичных HTTPS-серверов.
- Чужой сервер — это недоверенный код и недоверенный текст.

## Минимальный пример

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

## Когда применять и когда нет

Когда один набор инструментов нужен нескольким приложениям. Для одной функции внутри своего приложения хватит обычного tool use.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude 08-mcp level 5` |
| 6. Тесты | — | `/learn claude 08-mcp level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [07 · Tool use](../07-tool-use/overview.ru.md)
- [09 · Claude Code](../09-claude-code/overview.ru.md)
- [MCP SDK](../../libraries/mcp-sdk/overview.ru.md)
