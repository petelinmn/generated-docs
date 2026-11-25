---
title: "Agents and subagents: exercises"
lang: en
track: claude
kind: module
doc: exercises
slug: 12-agents
part: 2
tags: [claude, agents, subagents, agent-sdk, managed-agents]
versions: "Claude Code 2.1.332; claude-agent-sdk 0.2.189; anthropic 2.14.0; managed-agents-2026-04-01"
status: draft
updated: 2026-11-25
---

# Agents and subagents: exercises

🌐 [Русский](exercises.ru.md) · **English**

> Use a practice repository; you need Claude Code, and `claude-agent-sdk` for exercises 3–5.

## 1. A read-only agent

An agent that explains a module and cannot change anything.

<details><summary>Hint</summary>

List the tools.

</details>

<details><summary>Solution</summary>

```markdown
---
name: explainer
description: Explains how a module works.
tools: Read, Grep, Glob
---

Explain the module: entry points, data flow, risks. Cite files.
```

</details>

## 2. A one-off agent

Define an agent from the command line.

<details><summary>Hint</summary>

`--agents`.

</details>

<details><summary>Solution</summary>

```bash
claude --agents '{"tmp":{"description":"counts files","prompt":"Count the files.","tools":["Glob"],"maxTurns":3}}'
```

</details>

## 3. Your own tool

Add a `list_orders` tool to `agent.py`.

<details><summary>Hint</summary>

Remember `allowed_tools`.

</details>

<details><summary>Solution</summary>

```python
@tool("list_orders", "List order ids.", {})
async def list_orders(args):
    return {"content": [{"type": "text", "text": "A-1, A-2"}]}
# add it to create_sdk_mcp_server(tools=[order_status, list_orders])
# and "mcp__shop__list_orders" to allowed_tools
```

</details>

## 4. Deny with a hook

Forbid commands with `curl` in the SDK.

<details><summary>Hint</summary>

A `HookMatcher` on `Bash`.

</details>

<details><summary>Solution</summary>

```python
async def deny_curl(input_data, tool_use_id, context):
    if "curl" in input_data["tool_input"].get("command", ""):
        return {"hookSpecificOutput": {"hookEventName": "PreToolUse", "permissionDecision": "deny",
                                       "permissionDecisionReason": "network calls are not allowed"}}
    return {}
```

</details>

## 5. Isolate the product

Make sure machine settings do not affect the agent.

<details><summary>Hint</summary>

One parameter.

</details>

<details><summary>Solution</summary>

```python
options = ClaudeAgentOptions(setting_sources=[])
```

</details>
