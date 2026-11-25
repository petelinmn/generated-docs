---
title: "Агенты и subagents: упражнения"
lang: ru
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

# Агенты и subagents: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Выполняйте в учебном репозитории; нужен Claude Code, для упражнений 3–5 — `claude-agent-sdk`.

## 1. Агент только для чтения

Агент, объясняющий модуль и не способный ничего менять.

<details><summary>Подсказка</summary>

Перечислите инструменты.

</details>

<details><summary>Решение</summary>

```markdown
---
name: explainer
description: Explains how a module works.
tools: Read, Grep, Glob
---

Explain the module: entry points, data flow, risks. Cite files.
```

</details>

## 2. Агент на один запуск

Задайте агента из командной строки.

<details><summary>Подсказка</summary>

`--agents`.

</details>

<details><summary>Решение</summary>

```bash
claude --agents '{"tmp":{"description":"counts files","prompt":"Count the files.","tools":["Glob"],"maxTurns":3}}'
```

</details>

## 3. Свой инструмент

Добавьте в `agent.py` инструмент `list_orders`.

<details><summary>Подсказка</summary>

Не забудьте `allowed_tools`.

</details>

<details><summary>Решение</summary>

```python
@tool("list_orders", "List order ids.", {})
async def list_orders(args):
    return {"content": [{"type": "text", "text": "A-1, A-2"}]}
# add it to create_sdk_mcp_server(tools=[order_status, list_orders])
# and "mcp__shop__list_orders" to allowed_tools
```

</details>

## 4. Запрет через hook

Запретите в SDK команды с `curl`.

<details><summary>Подсказка</summary>

`HookMatcher` на `Bash`.

</details>

<details><summary>Решение</summary>

```python
async def deny_curl(input_data, tool_use_id, context):
    if "curl" in input_data["tool_input"].get("command", ""):
        return {"hookSpecificOutput": {"hookEventName": "PreToolUse", "permissionDecision": "deny",
                                       "permissionDecisionReason": "network calls are not allowed"}}
    return {}
```

</details>

## 5. Изолировать продукт

Сделайте так, чтобы настройки машины не влияли на агента.

<details><summary>Подсказка</summary>

Один параметр.

</details>

<details><summary>Решение</summary>

```python
options = ClaudeAgentOptions(setting_sources=[])
```

</details>
