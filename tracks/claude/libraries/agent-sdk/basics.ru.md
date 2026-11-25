---
title: "Claude Agent SDK: основы"
lang: ru
track: claude
kind: library
doc: basics
slug: agent-sdk
part: 2
tags: [claude, agent-sdk, agents, tool-use, hooks]
versions: "claude-agent-sdk 0.2.189"
status: draft
updated: 2026-11-25
---

# Claude Agent SDK: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [12 · Агенты и subagents](../../modules/12-agents/overview.ru.md), [09 · Claude Code](../../modules/09-claude-code/overview.ru.md)

## Как проверялся материал

`claude-agent-sdk` 0.2.189 со встроенным CLI запускался против локальной имитации модели в изолированной конфигурации. Поведение настоящей модели и TypeScript-версия не проверялись.

## Что это

Агентный цикл Claude Code как библиотека: чтение и правка файлов, команды, поиск, права, hooks, subagents, MCP. Пакеты: `claude-agent-sdk` (Python) и `@anthropic-ai/claude-agent-sdk` (TypeScript, 0.3.332). Он запускает CLI Claude Code как отдельный процесс.

Отличие от [Anthropic SDK](../anthropic-sdk/overview.ru.md): там цикл и инструменты пишете вы, здесь они готовы.

## `query()`

```python
import asyncio

from claude_agent_sdk import ClaudeAgentOptions, ResultMessage, query


async def main():
    options = ClaudeAgentOptions(setting_sources=[], max_turns=5)
    async for message in query(prompt="hello", options=options):
        if isinstance(message, ResultMessage):
            print(message.subtype, message.num_turns, message.result)


asyncio.run(main())
```

`query()` возвращает поток сообщений: `SystemMessage`, `AssistantMessage`, `UserMessage` (результаты инструментов), в конце `ResultMessage`. `setting_sources=[]` не читает настройки и `CLAUDE.md` с диска — поведение не зависит от машины.

## `ClaudeSDKClient`

```python
import asyncio

from claude_agent_sdk import ClaudeAgentOptions, ClaudeSDKClient, ResultMessage


async def main():
    async with ClaudeSDKClient(options=ClaudeAgentOptions(setting_sources=[])) as client:
        ids = []
        for text in ("first question", "second question"):
            await client.query(text)
            async for message in client.receive_response():
                if isinstance(message, ResultMessage):
                    ids.append(message.session_id)
        print(ids[0] == ids[1])


asyncio.run(main())
```

Клиент держит один разговор открытым: следующие `query` видят предыдущие. Он умеет прерывать (`interrupt`), менять модель и режим прав на ходу.

## Свои инструменты

```python
# shop_tools.py
from claude_agent_sdk import create_sdk_mcp_server, tool


@tool("order_status", "Status of an order", {"order_id": str})
async def order_status(args):
    try:
        status = {"A-1": "shipped"}[args["order_id"]]
    except Exception:
        # compose the message yourself: an uncaught exception reaches the model verbatim
        return {"content": [{"type": "text", "text": "unknown order"}], "is_error": True}
    return {"content": [{"type": "text", "text": status}]}


SERVER = create_sdk_mcp_server(name="shop", version="1.0.0", tools=[order_status])
NAME = "mcp__shop__order_status"
```

Инструмент работает в вашем процессе. Полное имя — `mcp__<сервер>__<инструмент>`. Без разрешения вызов отклоняется: добавьте имя в `allowed_tools`.

## Что в результате

| Поле | Смысл |
|---|---|
| `subtype`, `is_error` | `success`, `error_max_turns`, … |
| `result` | итоговый текст |
| `session_id` | для продолжения |
| `num_turns`, `usage`, `total_cost_usd` | расход |
| `permission_denials` | что не разрешили |
| `structured_output` | при `output_format` |

Условия использования: без отдельного разрешения Anthropic нельзя предлагать пользователям вашего продукта вход через claude.ai и его лимиты — используйте API-ключи.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [12 · Агенты](../../modules/12-agents/advanced.ru.md)

## Источники

- [Agent SDK overview](https://code.claude.com/docs/en/agent-sdk/overview)
- [Python reference](https://code.claude.com/docs/en/agent-sdk/python)
- [Custom tools](https://code.claude.com/docs/en/agent-sdk/custom-tools)
