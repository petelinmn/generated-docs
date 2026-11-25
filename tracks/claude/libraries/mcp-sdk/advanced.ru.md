---
title: "MCP SDK: продвинутый уровень"
lang: ru
track: claude
kind: library
doc: advanced
slug: mcp-sdk
part: 2
tags: [claude, mcp, mcp-sdk, tool-use, json-rpc]
versions: "mcp 2.3.0 (Python)"
status: draft
updated: 2026-11-25
---

# MCP SDK: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Ошибки инструментов

| Что случилось | Что получит клиент |
|---|---|
| `raise ToolError("…")` | `is_error`, ваш текст |
| любое другое исключение | `is_error`, только `Error executing tool <имя>` |
| аргументы не по схеме | `is_error`, подробности проверки |
| неизвестный инструмент | `is_error`, `Unknown tool: …` |
| неизвестный ресурс | исключение `MCPError`, `-32602` |

Скрытый текст исключения остаётся в логе сервера (stderr). Проверяйте `result.is_error`: вызов инструмента не бросает исключение.

## Контекст, прогресс, ресурсы на время жизни

```python
# jobs.py
from contextlib import asynccontextmanager

from mcp.server import MCPServer
from mcp.server.mcpserver import Context


@asynccontextmanager
async def lifespan(server):
    resources = {"unit": "step"}                 # open pools here, close them after yield
    yield resources


mcp = MCPServer("jobs", lifespan=lifespan)


@mcp.tool()
async def run_job(steps: int, ctx: Context) -> str:
    """Run a job and report progress."""
    unit = ctx.request_context.lifespan_context["unit"]
    for i in range(steps):
        await ctx.report_progress(i + 1, steps)
    return f"done {steps} {unit}s"
```

Параметр типа `Context` не попадает в схему входа. Прогресс клиент получает через `progress_callback`.

## Шаблоны URI и защита путей

`orders://{order_id}` — шаблон: он виден в `list_resource_templates`, а не в `list_resources`. По умолчанию значения с выходом из каталога (`..`), абсолютными путями и нулевыми байтами отклоняются (`resource_security`). Это не заменяет проверку пути в вашем коде.

## stdout в stdio

stdout занят протоколом. Один `print()` в инструменте дал на клиенте ошибку разбора `Invalid JSON`; вызов в проверке всё же прошёл, но полагаться на это нельзя. Пишите в stderr или через `logging`.

## HTTP: проверки Origin и Host

Локальный сервер на `127.0.0.1` отклонил запрос с чужим `Origin` (403) и чужим `Host` (421) — защита от DNS rebinding. Это не аутентификация: для удалённого сервера нужна авторизация (OAuth, `token_verifier`, `auth`).

## Встроить в существующее приложение

`mcp.streamable_http_app()` возвращает ASGI-приложение Starlette с маршрутом `/mcp`.

## Тесты

`Client(mcp)` в процессе — быстрый тест всего протокольного пути без сети и подпроцессов.

## Версии протокола

По умолчанию клиент 2.x договорился на `2026-07-28`; `Client(..., mode="legacy")` дал `2025-11-25`.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [08 · MCP](../../modules/08-mcp/advanced.ru.md) · [16 · Безопасность](../../modules/16-security/advanced.ru.md)

## Источники

- [Handling errors](https://py.sdk.modelcontextprotocol.io/servers/handling-errors/)
- [The Context](https://py.sdk.modelcontextprotocol.io/handlers/context/)
- [Add to an existing app](https://py.sdk.modelcontextprotocol.io/run/asgi/)
- [Authorization](https://py.sdk.modelcontextprotocol.io/run/authorization/)
