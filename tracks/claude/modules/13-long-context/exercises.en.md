---
title: "Compaction and long context: exercises"
lang: en
track: claude
kind: module
doc: exercises
slug: 13-long-context
part: 3
tags: [claude, context-window, compaction, context-editing, token-counting]
versions: "Claude 5.5 family; anthropic 2.14.0; beta compact-2026-09-04, compact-2026-01-12, context-management-2025-06-27; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Compaction and long context: exercises

🌐 [Русский](exercises.ru.md) · **English**

> Exercises 1, 3 and 4 need only Python; 2 and 5 need the `anthropic` SDK and an API key.

## 1. Check the trim

Make sure `trim` never starts the history with a `tool_result`.

<details><summary>Hint</summary>

A `tool_use` and `tool_result` pair.

</details>

<details><summary>Solution</summary>

```python
from history import trim

messages = [
    {"role": "user", "content": "q"},
    {"role": "assistant", "content": [{"type": "tool_use", "id": "t1", "name": "f", "input": {}}]},
    {"role": "user", "content": [{"type": "tool_result", "tool_use_id": "t1", "content": "r"}]},
    {"role": "assistant", "content": "done"},
]
print([m["role"] for m in trim(messages, 2)])
```

</details>

## 2. A budget before sending

Do not send a request over a given token count.

<details><summary>Hint</summary>

`count_tokens`.

</details>

<details><summary>Solution</summary>

```python
def fits(client, messages, limit):
    return client.messages.count_tokens(model="claude-opus-5-5", messages=messages).input_tokens <= limit
```

</details>

## 3. Remaining room

Compute what is left of a 1M window given input `n` and `max_tokens`.

<details><summary>Hint</summary>

Input plus output.

</details>

<details><summary>Solution</summary>

```python
def headroom(input_tokens, max_tokens, window=1_000_000):
    return window - input_tokens - max_tokens

print(headroom(900_000, 128_000))
```

</details>

## 4. Compaction with a fake

Test `compact` without the API.

<details><summary>Hint</summary>

An object with `beta.messages.create`.

</details>

<details><summary>Solution</summary>

```python
from types import SimpleNamespace

from compact import compact


def fake(stop_reason):
    reply = SimpleNamespace(stop_reason=stop_reason, content=["BLOCK"])
    return SimpleNamespace(beta=SimpleNamespace(messages=SimpleNamespace(create=lambda **kw: reply)))


history = [{"role": "user", "content": "hi"}]
print(compact(fake("compaction"), history))
print(compact(fake("max_tokens"), history) is history)
```

</details>

## 5. Clearing

Configure clearing that keeps the last 5 pairs and `search_docs` results.

<details><summary>Hint</summary>

`keep`, `exclude_tools`.

</details>

<details><summary>Solution</summary>

```python
edit = {"type": "clear_tool_uses_20250919",
        "keep": {"type": "tool_uses", "value": 5},
        "exclude_tools": ["search_docs"]}
```

</details>
