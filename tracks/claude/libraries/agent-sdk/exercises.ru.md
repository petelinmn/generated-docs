---
title: "Claude Agent SDK: упражнения"
lang: ru
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

# Claude Agent SDK: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Нужны `shop_tools.py` и `guard.py` из теории. Модель и ключ не нужны.

## 1. Инструмент без агента

Вызовите обработчик `order_status` напрямую для известного и неизвестного заказа.

<details><summary>Подсказка</summary>

У декорированного объекта есть `.handler`.

</details>

<details><summary>Решение</summary>

```python
import asyncio

from shop_tools import order_status

for order_id in ("A-1", "Z"):
    print(asyncio.run(order_status.handler({"order_id": order_id})))
```

</details>

## 2. Hook

Проверьте функцию `freeze` на замороженном и обычном заказе.

<details><summary>Подсказка</summary>

Передайте словарь с `tool_input`.

</details>

<details><summary>Решение</summary>

```python
import asyncio

from guard import freeze

for order_id in ("X-9", "A-1"):
    print(asyncio.run(freeze({"tool_input": {"order_id": order_id}}, "t1", None)))
```

</details>

## 3. Настройки

Соберите опции только для чтения с пределом ходов.

<details><summary>Подсказка</summary>

`tools` и `max_turns`.

</details>

<details><summary>Решение</summary>

```python
from claude_agent_sdk import ClaudeAgentOptions

options = ClaudeAgentOptions(tools=["Read", "Grep", "Glob"], allowed_tools=["Read", "Grep", "Glob"],
                             max_turns=5, setting_sources=[])
print(options.tools, options.max_turns)
```

</details>

## 4. Список сессий

Напечатайте число сессий текущего каталога.

<details><summary>Подсказка</summary>

`list_sessions(directory=...)`.

</details>

<details><summary>Решение</summary>

```python
import os

from claude_agent_sdk import list_sessions

print(len(list_sessions(directory=os.getcwd())))
```

</details>
