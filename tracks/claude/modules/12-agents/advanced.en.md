---
title: "Agents and subagents: advanced"
lang: en
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

# Agents and subagents: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Subagent permissions and limits

- Restrictions hold inside: a subagent's refused call appeared in the main run's `permission_denials`.
- Deny a type altogether: `deny: ["Agent(reviewer)"]`.
- `maxTurns` caps turns; `--agents '{...}'` defines an agent for one run.
- By default nesting goes up to three layers below the main conversation.
- `isolation: worktree` gives a subagent its own repository copy; `memory` gives persistent notes.

## When to run in parallel

Independent parts (searching different modules, several review angles) can go to several workers. Do not hand out edits of the same files without isolation. Every worker costs separately.

## Claude Agent SDK

The SDK gives your Python or TypeScript program the same agent loop, tools, permissions and hooks as Claude Code.

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

Against the stand-in scripted to request this tool, the run was: a `mcp__shop__order_status` call, the result `A-1: shipped`, `success` in 2 turns. Without `allowed_tools` the same call was refused permission.

Hooks are ordinary functions:

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

- With `setting_sources` unset, settings and `CLAUDE.md` are read from disk; `[]` reads nothing.
- Use an API key for your products: third parties may not offer claude.ai login without approval.

## Managed Agents (beta)

The other option: Anthropic hosts the loop and the sandbox. Four concepts: **Agent** (model, prompt, tools), **Environment** (where it runs), **Session** (a run), **Events** (messages).

```python
agent = client.beta.agents.create(name="Coder", model="claude-opus-5-5", system="You are helpful.",
                                  tools=[{"type": "agent_toolset_20260401"}])          # create once
environment = client.beta.environments.create(name="env",
                                              config={"type": "cloud", "networking": {"type": "unrestricted"}})
session = client.beta.sessions.create(agent=agent.id, environment_id=environment.id, title="t")
client.beta.sessions.events.send(session.id, events=[
    {"type": "user.message", "content": [{"type": "text", "text": "hi"}]}])
```

> ⚠️ Verify: only request paths and bodies were checked; service behaviour, the event stream and cost were not.

Managed Agents keep session state and are currently not eligible for Zero Data Retention.

## Which to choose

| Need | Choice |
|---|---|
| full loop control, own tools | Messages API + [tool use](../07-tool-use/basics.en.md) |
| a ready agent with files and commands on your machine | Agent SDK |
| no infrastructure of your own, long tasks | Managed Agents |

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [08 · MCP](../08-mcp/advanced.en.md)

## Sources

- [Agent SDK overview](https://code.claude.com/docs/en/agent-sdk/overview)
- [Agent SDK: Python](https://code.claude.com/docs/en/agent-sdk/python)
- [Managed Agents overview](https://platform.claude.com/docs/en/managed-agents/overview)
