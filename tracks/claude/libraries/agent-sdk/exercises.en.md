---
title: "Claude Agent SDK: exercises"
lang: en
track: claude
kind: library
doc: exercises
slug: agent-sdk
part: 2
tags: [claude, agent-sdk, agents, tool-use, hooks]
versions: "claude-agent-sdk 0.2.189"
status: draft
updated: 2026-11-25
---

# Claude Agent SDK: exercises

🌐 [Русский](exercises.ru.md) · **English**

> You need `shop_tools.py` and `guard.py` from the theory. No model or key.

## 1. A tool without an agent

Call the `order_status` handler directly for a known and an unknown order.

<details><summary>Hint</summary>

The decorated object has `.handler`.

</details>

<details><summary>Solution</summary>

```python
import asyncio

from shop_tools import order_status

for order_id in ("A-1", "Z"):
    print(asyncio.run(order_status.handler({"order_id": order_id})))
```

</details>

## 2. The hook

Test `freeze` on a frozen and a normal order.

<details><summary>Hint</summary>

Pass a dict with `tool_input`.

</details>

<details><summary>Solution</summary>

```python
import asyncio

from guard import freeze

for order_id in ("X-9", "A-1"):
    print(asyncio.run(freeze({"tool_input": {"order_id": order_id}}, "t1", None)))
```

</details>

## 3. Options

Build read-only options with a turn limit.

<details><summary>Hint</summary>

`tools` and `max_turns`.

</details>

<details><summary>Solution</summary>

```python
from claude_agent_sdk import ClaudeAgentOptions

options = ClaudeAgentOptions(tools=["Read", "Grep", "Glob"], allowed_tools=["Read", "Grep", "Glob"],
                             max_turns=5, setting_sources=[])
print(options.tools, options.max_turns)
```

</details>

## 4. Listing sessions

Print how many sessions the current directory has.

<details><summary>Hint</summary>

`list_sessions(directory=...)`.

</details>

<details><summary>Solution</summary>

```python
import os

from claude_agent_sdk import list_sessions

print(len(list_sessions(directory=os.getcwd())))
```

</details>
