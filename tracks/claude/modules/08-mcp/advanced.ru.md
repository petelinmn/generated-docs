---
title: "MCP и connectors: продвинутый уровень"
lang: ru
track: claude
kind: module
doc: advanced
slug: 08-mcp
part: 2
tags: [claude, mcp, mcp-connector, tool-use, claude-code]
versions: "MCP specification 2026-07-28; Python SDK mcp 2.3.0; anthropic 2.14.0; anthropic-beta mcp-client-2025-11-20"
status: draft
updated: 2026-11-25
---

# MCP и connectors: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Три способа подключить сервер к Claude

| Способ | Кто клиент MCP | Что поддерживает |
|---|---|---|
| свой клиент + tool use | ваш код | stdio и HTTP; tools, resources, prompts |
| MCP connector (Messages API) | серверы Anthropic | только вызовы tools; публичный `https`-адрес |
| Claude Code | Claude Code | локальные и удалённые серверы |

## Свой клиент: мост к Messages API

```python
# bridge.py: needs ANTHROPIC_API_KEY
import asyncio

from anthropic import AsyncAnthropic
from anthropic.lib.tools.mcp import async_mcp_tool
from mcp import Client

from server import mcp


async def main():
    claude = AsyncAnthropic()
    async with Client(mcp) as client:  # in-process connection
        tools = [async_mcp_tool(t, client) for t in (await client.list_tools()).tools]
        runner = claude.beta.messages.tool_runner(
            model="claude-opus-5-5", max_tokens=1024, tools=tools,
            messages=[{"role": "user", "content": "Where is order A-1?"}])
        async for message in runner:
            print(message.stop_reason, [b.type for b in message.content])


asyncio.run(main())
```

```text
tool_use ['tool_use']
end_turn ['text']
```

Помощник из SDK превращает инструмент MCP в инструмент tool runner. Вывод получен на заглушке: она запросила `order_status`, runner вызвал сервер и вернул результат. Вручную достаточно переложить `name`, `description`, `input_schema`.

## MCP connector

```python
response = client.beta.messages.create(
    model="claude-opus-5-5",
    max_tokens=1000,
    messages=[{"role": "user", "content": "Where is order A-1?"}],
    mcp_servers=[{"type": "url", "url": "https://mcp.example.com/mcp", "name": "shop"}],
    tools=[{"type": "mcp_toolset", "mcp_server_name": "shop",
            "default_config": {"enabled": False},
            "configs": {"order_status": {"enabled": True}}}],
    betas=["mcp-client-2025-11-20"],
)
```

- Каждый сервер из `mcp_servers` должен упоминаться ровно в одном `mcp_toolset`.
- `default_config` и `configs` задают список разрешённых инструментов; здесь только `order_status`.
- OAuth-токен передаётся в `authorization_token`; получить и обновлять его — ваша задача.
- Коннектор не покрывается соглашениями о нулевом хранении данных (ZDR).

> ⚠️ Проверить: форма запроса проверена на заглушке; ответ с блоками `mcp_tool_use` и `mcp_tool_result` и подключение к настоящему удалённому серверу не проверялись.

## Streamable HTTP

```python
# http_server.py
from server import mcp

mcp.run(transport="streamable-http", port=8765)   # http://127.0.0.1:8765/mcp
```

Клиенту достаточно адреса: `Client("http://127.0.0.1:8765/mcp")`. Для удалённого сервера нужны TLS и аутентификация; локальный не должен слушать все интерфейсы.

## Claude Code

```bash
claude mcp add shop -- python server.py                                   # stdio, local scope
claude mcp add --scope project --transport http docs https://mcp.example.com/mcp   # writes .mcp.json
claude mcp list
claude mcp get shop
claude mcp remove shop -s local
```

| Область | Где хранится | Кто видит |
|---|---|---|
| `local` (по умолчанию) | `~/.claude.json` | вы в этом проекте |
| `project` | `.mcp.json` | команда, через git |
| `user` | `~/.claude.json` | вы во всех проектах |

Сервер из `.mcp.json` не подключается, пока вы его не одобрите: `list` показывал `Pending approval`. Инструменты называются `mcp__<server>__<tool>`; это имя используют в правилах прав.

## Частые ошибки

- Коммитить токен в `.mcp.json`: используйте подстановку переменных окружения.
- Подключать десятки инструментов без отбора.
- Считать результат инструмента доверенными инструкциями.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [09 · Claude Code](../09-claude-code/overview.ru.md)

## Источники

- [MCP connector](https://platform.claude.com/docs/en/agents-and-tools/mcp-connector)
- [Claude Code: MCP](https://code.claude.com/docs/en/mcp)
- [Streamable HTTP](https://modelcontextprotocol.io/specification/2026-07-28/basic/transports/streamable-http)
