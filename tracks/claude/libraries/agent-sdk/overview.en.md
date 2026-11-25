---
title: "Claude Agent SDK"
lang: en
track: claude
kind: library
doc: overview
slug: agent-sdk
part: 2
tags: [claude, agent-sdk, agents, tool-use, hooks]
versions: "claude-agent-sdk 0.2.189"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Claude Agent SDK

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

**The Claude Code agent loop as a library** for Python and TypeScript: built-in and custom tools, permissions, hooks and sessions.

## Key ideas

- `query()` is one run, `ClaudeSDKClient` a conversation.
- Permissions are set in code: rules, hooks, a callback.
- Check the `ResultMessage`, not just the text.
- The model sees your tools' error messages.
- `setting_sources=[]` makes behaviour predictable.

## Minimal example

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

## When to use it and when not

When you need an agent working with files and commands in your environment. Single model calls need only the Anthropic SDK.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude agent-sdk level 5` |
| 6. Tests | — | `/learn claude agent-sdk level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [12 · Agents and subagents](../../modules/12-agents/overview.en.md)
- [Anthropic SDK](../anthropic-sdk/overview.en.md)
- [MCP SDK](../mcp-sdk/overview.en.md)
