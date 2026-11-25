---
title: "Tool use"
lang: en
track: claude
kind: module
doc: overview
slug: 07-tool-use
part: 1
levels: [1, 2, 3, 4]
tags: [claude, tool-use, function-calling, tool-choice, parallel-tool-calls, agent-loop]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Tool use

🌐 [Русский](overview.ru.md) · **English**

## In short

Claude cannot call your code on its own. You describe tools (a name, a description and a JSON Schema of the input), and when the model decides it needs one it answers with a `tool_use` block and `stop_reason: "tool_use"`. Your program runs the function and sends the result back in a `tool_result` block; Claude then continues. Repeating this until the reply is a normal `end_turn` is the **agent loop**: the basis of every assistant that does anything beyond chatting.

## Key ideas

- **A tool is a definition:** `name`, `description`, `input_schema` (JSON Schema); optionally `strict`, `input_examples`, `cache_control`.
- **The model only asks.** A `tool_use` block has `id`, `name` and `input`; the code is executed by your application (a *client tool*) or by Anthropic's servers (a *server tool*, such as web search).
- **The loop:** append the reply as an `assistant` message, run every `tool_use`, send one `user` message with all `tool_result` blocks, repeat.
- **Order matters:** `tool_result` blocks come first in the user message and directly follow the assistant turn; text goes after them or not at all.
- **Parallel calls:** one reply may contain several `tool_use` blocks; return all the results in **one** message.
- **Errors:** return `is_error: true` with an instructive text; do not drop the call.
- **`tool_choice`:** `auto` (default), `any`, `tool`, `none`; `disable_parallel_tool_use` limits calls to one. Forced `any`/`tool` returns a 400 on the Claude 5.5 models listed in the documentation.
- **Cost:** a system prompt for tools adds about 286 tokens on Opus 5.5 and Sonnet 5.5, plus the definitions themselves.
- **Always cap the number of loop steps.**

## Minimal example

```python
tools = [{"name": "get_weather", "description": "Get the current weather in a city.",
          "input_schema": {"type": "object", "properties": {"location": {"type": "string"}}, "required": ["location"]}}]
r = client.messages.create(model="claude-opus-5-5", max_tokens=1024, tools=tools,
                           messages=[{"role": "user", "content": "Weather in Paris?"}])
print(r.stop_reason)   # tool_use: run the function and send a tool_result
```

The full loop and its output are in the [basic theory](basics.en.md). The code was checked against a local mock server, not against a real model.

## When to use and when not

| Task | Choice |
|---|---|
| The model needs fresh data or an action (database, API, files) | client tools |
| Web search, page fetching, code execution without your own sandbox | server tools |
| You only need a structured reply, no side effects | [structured outputs](../04-structured-output/overview.en.md), not tools |
| Strictly one tool must be called | `strict` tools and structured output (forced `tool_choice` is limited on 5.5 models) |
| A fixed sequence of steps known in advance | an ordinary workflow in code; tools are for when the model decides |

- ❌ Do not run a tool from model input without checking arguments and permissions.
- ❌ Do not leave the loop without a step limit.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | definition, the loop, `tool_result`, parallel calls, errors, `tool_choice` |
| [2. Advanced theory](advanced.en.md) | server tools, `pause_turn`, descriptions, forced use and models, token cost, caching, the SDK tool runner |
| [3. Expert theory](expert.en.md) | a reference of fields, stop reasons and tool types, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn claude 07-tool-use level 5` |
| 6. Tests | not created yet — `/learn claude 07-tool-use level 6` |

## Related materials

- Prerequisites: [02 · Messages API](../02-messages-api/overview.en.md), [04 · Structured outputs](../04-structured-output/overview.en.md), [05 · Prompt caching](../05-prompt-caching/overview.en.md)
- Previous: [06 · Documents and citations](../06-documents-and-citations/overview.en.md)
- Next: [08 · MCP and connectors](../08-mcp/overview.en.md)
- Analogues in other tracks: [LLM](../../../llm/overview.en.md), [Python](../../../python/overview.en.md)
- [12 · Agents and subagents](../12-agents/overview.en.md)
- [16 · Security and compliance](../16-security/overview.en.md)
- [Anthropic SDK](../../libraries/anthropic-sdk/overview.en.md)
