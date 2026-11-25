---
title: "Claude Agent SDK: fundamentals"
lang: en
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

# Claude Agent SDK: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [12 · Agents and subagents](../../modules/12-agents/overview.en.md), [09 · Claude Code](../../modules/09-claude-code/overview.en.md)

## How this was checked

`claude-agent-sdk` 0.2.189 with its bundled CLI ran against a local model stand-in in an isolated configuration. A real model's behaviour and the TypeScript version were not checked.

## What it is

The Claude Code agent loop as a library: file reading and editing, commands, search, permissions, hooks, subagents, MCP. Packages: `claude-agent-sdk` (Python) and `@anthropic-ai/claude-agent-sdk` (TypeScript, 0.3.332). It runs the Claude Code CLI as a subprocess.

Unlike the [Anthropic SDK](../anthropic-sdk/overview.en.md), where you write the loop and tools, here they are ready.

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

`query()` yields a stream: `SystemMessage`, `AssistantMessage`, `UserMessage` (tool results), then a `ResultMessage`. `setting_sources=[]` reads no settings or `CLAUDE.md` from disk, so behaviour does not depend on the machine.

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

The client keeps one conversation open: later queries see earlier ones. It can `interrupt` and change model or permission mode on the fly.

## Your own tools

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

The tool runs in your process. Its full name is `mcp__<server>__<tool>`. Without permission the call is denied: add the name to `allowed_tools`.

## What the result holds

| Field | Meaning |
|---|---|
| `subtype`, `is_error` | `success`, `error_max_turns`, … |
| `result` | the final text |
| `session_id` | to resume |
| `num_turns`, `usage`, `total_cost_usd` | consumption |
| `permission_denials` | what was refused |
| `structured_output` | with `output_format` |

Terms: without Anthropic's approval you may not offer claude.ai login or its rate limits in your product: use API keys.

## Related materials

- [Advanced theory](advanced.en.md)
- [12 · Agents](../../modules/12-agents/advanced.en.md)

## Sources

- [Agent SDK overview](https://code.claude.com/docs/en/agent-sdk/overview)
- [Python reference](https://code.claude.com/docs/en/agent-sdk/python)
- [Custom tools](https://code.claude.com/docs/en/agent-sdk/custom-tools)
