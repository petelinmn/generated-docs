---
title: "Compaction and long context: fundamentals"
lang: en
track: claude
kind: module
doc: basics
slug: 13-long-context
part: 3
tags: [claude, context-window, compaction, context-editing, token-counting]
versions: "Claude 5.5 family; anthropic 2.14.0; beta compact-2026-09-04, compact-2026-01-12, context-management-2025-06-27; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Compaction and long context: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Messages API](../02-messages-api/overview.en.md), [05 · Prompt caching](../05-prompt-caching/overview.en.md)

## How this was checked

There is no API key. The code ran with the `anthropic` 2.14.0 SDK against a local stand-in: request shapes and client-side logic were checked, not summary quality or real token counts. `/compact` was checked in Claude Code 2.1.332 the same way. Limits and service behaviour come from the documentation.

## The context window

The window is everything the model works with in one request, the reply included: system prompt, all messages, tool results, images, documents, tool definitions and the output with thinking. The API is stateless: you resend the history each time and it grows.

| Models | Window |
|---|---|
| Fable 5–5.2, Mythos 5–5.2, Mythos Preview, Opus 4.6–5.5, Sonnet 4.6–5.5, Haiku 5.5 | 1M tokens |
| others | 200k |

On 1M models the window is the default at standard pricing; the exception is Haiku 5.5, where prompts over 100,000 tokens cost more. More context is not automatically better: quality degrades as it grows.

## Counting in advance

```python
import anthropic

client = anthropic.Anthropic()
count = client.messages.count_tokens(model="claude-opus-5-5", messages=messages, system=system, tools=tools)
print(count.input_tokens)          # an estimate; counting itself is free
```

## What happens on overflow

| Situation | Result |
|---|---|
| the input alone exceeds the window | 400 `invalid_request_error`, `prompt is too long` |
| input + `max_tokens` exceeds it (4.5 and newer) | accepted; on reaching the limit `stop_reason: "model_context_window_exceeded"` |

## The simplest way: trim the history

```python
# history.py
def trim(messages, keep_last):
    """Keep the last `keep_last` messages without starting on an orphaned tool_result."""
    start = max(0, len(messages) - keep_last)
    while start > 0 and is_tool_result(messages[start]):
        start -= 1
    return messages[start:]


def is_tool_result(message):
    content = message["content"]
    return isinstance(content, list) and any(block.get("type") == "tool_result" for block in content)
```

Cutting blindly is wrong: a `tool_result` without its `tool_use` is an error. The function steps back to the start of the pair. Trimming loses information and breaks the cache — the [next level](advanced.en.md) shows better ways.

## Common beginner mistakes

- Thinking caching frees room: cached tokens occupy the window just the same.
- Putting huge tool results into history.
- Handling only `end_turn`.

## Related materials

- [Advanced theory](advanced.en.md)
- [06 · Documents](../06-documents-and-citations/overview.en.md)

## Sources

- [Context windows](https://platform.claude.com/docs/en/build-with-claude/context-windows)
- [Token counting](https://platform.claude.com/docs/en/build-with-claude/token-counting)
