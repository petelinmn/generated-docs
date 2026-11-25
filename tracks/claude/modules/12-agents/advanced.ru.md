---
title: "Агенты и subagents: продвинутый уровень"
lang: ru
track: claude
kind: module
doc: advanced
slug: 12-agents
part: 2
tags: [claude, agents, subagents, agent-sdk, managed-agents]
versions: "Claude Code 2.1.332; claude-agent-sdk 0.2.189; anthropic 2.14.0; managed-agents-2026-04-01"
status: draft
updated: 2026-11-25
---

# Агенты и subagents: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Права и пределы subagent

- Запрет действует и внутри: отказ subagent в правах попал в `permission_denials` основного запуска.
- Запретить тип целиком: `deny: ["Agent(reviewer)"]`.
- `maxTurns` ограничивает ходы; `--agents '{...}'` задаёт агента на один запуск.
- Вложенность по умолчанию — до трёх уровней ниже основного разговора.
- `isolation: worktree` даёт subagent копию репозитория; `memory` — постоянные заметки.

## Когда параллельно

Независимые части (поиск в разных модулях, несколько точек зрения на ревью) можно раздать нескольким. Не раздавайте правку одних и тех же файлов без изоляции. Каждый исполнитель — отдельный расход.

## Claude Agent SDK

SDK даёт тот же агентный цикл, инструменты, права и hooks, что и Claude Code, из вашей программы на Python или TypeScript.

```python
# agent.py: pip install claude-agent-sdk
import asyncio

from claude_agent_sdk import ClaudeAgentOptions, ResultMessage, create_sdk_mcp_server, query, tool


@tool("order_status", "Return the status of an order by id.", {"order_id": str})
async def order_status(args):
    return {"content": [{"type": "text", "text": f"{args['order_id']}: shipped"}]}


async def main():
    options = ClaudeAgentOptions(
        mcp_servers={"shop": create_sdk_mcp_server(name="shop", version="1.0.0", tools=[order_status])},
        allowed_tools=["mcp__shop__order_status"],
        setting_sources=[],          # do not read settings or CLAUDE.md from disk
        max_turns=5,
    )
    async for message in query(prompt="Where is order A-1?", options=options):
        if isinstance(message, ResultMessage):
            print(message.subtype, message.num_turns)


asyncio.run(main())
```

Против заглушки, сценарно запрашивавшей этот инструмент, запуск прошёл так: вызов `mcp__shop__order_status`, результат `A-1: shipped`, `success` за 2 хода. Без `allowed_tools` тот же вызов получил отказ в правах.

Hooks — обычные функции:

```python
from claude_agent_sdk import HookMatcher


async def deny_touch(input_data, tool_use_id, context):
    if "touch" in input_data["tool_input"].get("command", ""):
        return {"hookSpecificOutput": {"hookEventName": "PreToolUse", "permissionDecision": "deny",
                                       "permissionDecisionReason": "no touching"}}
    return {}

options = ClaudeAgentOptions(allowed_tools=["Bash"],
                             hooks={"PreToolUse": [HookMatcher(matcher="Bash", hooks=[deny_touch])]})
```

- `setting_sources` не задан — читаются настройки и `CLAUDE.md` с диска; `[]` — ничего.
- Для своих продуктов используйте ключ API: вход claude.ai третьим сторонам предлагать нельзя без разрешения.

## Managed Agents (бета)

Другой вариант: цикл и песочницу размещает Anthropic. Четыре понятия: **Agent** (модель, промпт, инструменты), **Environment** (где выполняется), **Session** (запуск), **Events** (сообщения).

```python
agent = client.beta.agents.create(name="Coder", model="claude-opus-5-5", system="You are helpful.",
                                  tools=[{"type": "agent_toolset_20260401"}])          # create once
environment = client.beta.environments.create(name="env",
                                              config={"type": "cloud", "networking": {"type": "unrestricted"}})
session = client.beta.sessions.create(agent=agent.id, environment_id=environment.id, title="t")
client.beta.sessions.events.send(session.id, events=[
    {"type": "user.message", "content": [{"type": "text", "text": "hi"}]}])
```

> ⚠️ Проверить: проверены только пути и тела запросов; поведение сервиса, поток событий и стоимость не проверялись.

Managed Agents хранят состояние сессий и сейчас не подходят под Zero Data Retention.

## Что выбрать

| Нужно | Выбор |
|---|---|
| полный контроль цикла, свои инструменты | Messages API + [tool use](../07-tool-use/basics.ru.md) |
| готовый агент с файлами и командами на своей машине | Agent SDK |
| без своей инфраструктуры, долгие задачи | Managed Agents |

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [08 · MCP](../08-mcp/advanced.ru.md)

## Источники

- [Agent SDK overview](https://code.claude.com/docs/en/agent-sdk/overview)
- [Agent SDK: Python](https://code.claude.com/docs/en/agent-sdk/python)
- [Managed Agents overview](https://platform.claude.com/docs/en/managed-agents/overview)
