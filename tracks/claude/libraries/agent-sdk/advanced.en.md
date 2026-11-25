---
title: "Claude Agent SDK: advanced"
lang: en
track: claude
kind: library
doc: advanced
slug: agent-sdk
part: 2
tags: [claude, agent-sdk, agents, tool-use, hooks]
versions: "claude-agent-sdk 0.2.189"
status: draft
updated: 2026-11-25
---

# Claude Agent SDK: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Permissions

| Means | Role |
|---|---|
| `allowed_tools` | approve without asking |
| `disallowed_tools` | forbid |
| `permission_mode` | the overall mode |
| `can_use_tool` | decide in code what rules leave open |
| `PreToolUse` hook | check every call |

**`allowed_tools` shadows `can_use_tool`:** for a tool allowed outright the callback is not called and the SDK warns `CanUseToolShadowedWarning`. To check every call use a hook.

## Hooks

```python
# guard.py
from claude_agent_sdk import ClaudeAgentOptions, HookMatcher

from shop_tools import NAME, SERVER


async def freeze(input_data, tool_use_id, context):
    if input_data["tool_input"].get("order_id", "").startswith("X-"):
        return {"hookSpecificOutput": {"hookEventName": "PreToolUse", "permissionDecision": "deny",
                                       "permissionDecisionReason": "frozen orders"}}
    return {}


OPTIONS = ClaudeAgentOptions(setting_sources=[], mcp_servers={"shop": SERVER}, allowed_tools=[NAME],
                             hooks={"PreToolUse": [HookMatcher(matcher=NAME, hooks=[freeze])]})
```

A hook denial reaches the tool result with your reason and appears in `permission_denials`, even for a tool listed in `allowed_tools`.

## Errors and limits

```python
import asyncio

from claude_agent_sdk import ClaudeAgentOptions, ResultError, ResultMessage, query


async def run(prompt, **options):
    result = None
    try:
        async for message in query(prompt=prompt, options=ClaudeAgentOptions(setting_sources=[], **options)):
            if isinstance(message, ResultMessage):
                result = message
    except ResultError:
        if result is None:      # the error result is yielded first, then query() raises
            raise
    return result


result = asyncio.run(run("hello", max_turns=3))
print(result.subtype, result.is_error, len(result.permission_denials or []))
```

| Case | What you get |
|---|---|
| `max_turns` exhausted | `ResultMessage` `error_max_turns`, then `ResultError` |
| unknown `resume` ID | `ResultError` |
| CLI not found | `CLINotFoundError` |
| a permission denial | `success`, listed in `permission_denials` |

`success` does not mean the task got done: check denials and the outcome. `max_budget_usd` bounds spending.

## Sessions

- `resume=<session_id>` continues with the same ID; adding `fork_session=True` branches under a new ID.
- `continue_conversation=True` takes the directory's latest session.
- Functions: `list_sessions`, `get_session_messages`, `fork_session`, `rename_session`, `delete_session`.
- Sessions are files under `~/.claude/projects/` (or `$CLAUDE_CONFIG_DIR/projects/`), tied to the working directory. Another server cannot `resume` them without copying or a `session_store`.

## Structured results and streaming

`output_format={"type": "json_schema", "schema": ...}` puts the object in `structured_output`. `include_partial_messages=True` adds `StreamEvent`s with partial replies.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [11 · Skills and hooks](../../modules/11-skills-and-hooks/advanced.en.md) · [16 · Security](../../modules/16-security/advanced.en.md)

## Sources

- [Permissions](https://code.claude.com/docs/en/agent-sdk/permissions)
- [Hooks](https://code.claude.com/docs/en/agent-sdk/hooks)
- [Sessions](https://code.claude.com/docs/en/agent-sdk/sessions)
