---
title: "Tool use: advanced"
lang: en
track: claude
kind: module
doc: advanced
slug: 07-tool-use
part: 1
tags: [claude, tool-use, server-tools, tool-choice, strict-tools, prompt-caching, token-cost, tool-runner]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-25
---

# Tool use: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code ran against the local mock server (see "How the code was checked" in the basics); the pages on strict tool use, web search and code execution were not read, so what depends on them is marked.

## Client tools and server tools

| | Client tool | Server tool |
|---|---|---|
| Who runs the code | your application | Anthropic's servers |
| Block in the reply | `tool_use` | `server_tool_use` (+ a result block) |
| Your part | run it, return `tool_result` | nothing: the result arrives in the same reply |
| Examples | your functions, `bash`, `text_editor`, `memory` | web search, web fetch, code execution, tool search |
| Errors | `is_error: true` in `tool_result` | do not send `is_error`; the error is inside the result block |

A tool is declared with a typed entry, for example `{"type": "web_search_20260209", "name": "web_search"}`. A reply may mix both kinds: `stop_reason: "tool_use"` with a `server_tool_use` block and your `tool_use`. Type names and versions are in the [expert reference](expert.en.md#server-tool-types).

> ⚠️ Verify: request and response shapes of the individual server tools (search, fetch, code execution) were not read in this material; only the type names and the general rules from the overview pages are given.

### `pause_turn`

Server tools run in a loop on the server. When it hits its iteration limit the reply has `stop_reason: "pause_turn"`. Resend the conversation with that reply's `content` appended unchanged as an `assistant` message (the documentation summary says the default limit is 10 iterations, and that no user message follows). If the reply has a `container`, send its id back. A call to **your** tool always ends with `tool_use`, never `pause_turn`.

## Forced tool use and models

`any` and `tool` force a tool call: the reply starts with `tool_use`, with no text before it.

- With manual extended thinking (`thinking: {type: "enabled"}`) they return an error: use `auto` or `none`.
- With adaptive thinking they are allowed, but a forced call skips thinking.
- On Claude Fable 5.2, Mythos 5.2, Opus 5.5, Sonnet 5.5, Fable 5.1 and Mythos 5.1 they return a 400; `none` still works. The alternatives are `auto` with `strict` tools or [structured outputs](../04-structured-output/advanced.en.md).

The mock reproduced this for the Opus 5.5 id and let `claude-opus-5` through:

```text
claude-opus-5-5 400 Forced tool use is not supported for this model.
claude-opus-5 tool_use
```

The 400 text above is the mock's wording, not the real one. To get text before the call, use `auto` and ask for it in the prompt. Changing `tool_choice` invalidates cached message blocks (tool definitions and the system prompt stay cached).

## Writing good descriptions

The model chooses tools by reading names and descriptions, so they are your prompt:

- say **what** the tool does, **when** to use it and **when not**, and what it returns;
- explain every parameter (`description` in the schema), with units and formats;
- keep names distinct and verb-like (`get_weather`, not `weather2`);
- put long guidance in the description, not in dozens of tiny tools;
- `input_examples` (a list of example inputs that must validate against the schema) cost about 100 tokens for a flat example and about 200 for a nested one; they work on user-defined tools, not on server tools.

For `strict: true` (guaranteed schema conformance of names and inputs) the documentation says it is not allowed on `mcp_toolset` and the computer/browser toolsets.

> ⚠️ Verify: the full semantics of `strict` (supported schema subset, limits) are on the strict tool use page, which was not read here.

## Cost in tokens

Tools add input tokens in three ways: the **definitions** (the `tools` array), an automatic **system prompt** for tool use, and the `tool_use`/`tool_result` blocks in the history. The documented system-prompt overhead:

| Model | `auto`, `none` | `any`, `tool` |
|---|---|---|
| Opus 5.5 | 286 | forced use not supported |
| Opus 5 | 286 | 406 |
| Sonnet 5.5 | 286 | forced use not supported |
| Sonnet 5 | 354 | 474 |
| Haiku 5.5 | 286 | 406 |

With no `tools`, `tool_choice: none` adds nothing. Server tools have their own usage-based pricing on their pages.

## Caching tools

`cache_control` can sit on a tool definition (`{"type": "ephemeral"}`), caching the prefix up to and including it; order of the prefix is tools → system → messages ([module 05](../05-prompt-caching/advanced.en.md)). A tool with `defer_loading: true` cannot carry `cache_control`. Keep definitions stable and sorted: any change in the `tools` array invalidates what follows.

## The tool runner in the SDK

The Python SDK can run the loop for you. A function with a docstring becomes a tool through `@beta_tool`; `client.beta.messages.tool_runner` sends requests, runs the functions and appends results until the reply is final:

```python
from anthropic import beta_tool

@beta_tool
def get_weather(location: str) -> str:
    """Get the current weather in a city.

    Args:
        location: City name.
    """
    return f"15 degrees in {location}"

runner = client.beta.messages.tool_runner(model="claude-opus-5-5", max_tokens=1024,
                                          tools=[get_weather],
                                          messages=[{"role": "user", "content": "Weather in Paris?"}])
for msg in runner:
    print("runner", msg.stop_reason)
print(get_weather.to_dict()["input_schema"])
```

```text
runner tool_use
runner end_turn
{'additionalProperties': False, 'properties': {'location': {'description': 'City name.', 'title': 'Location', 'type': 'string'}}, 'required': ['location'], 'type': 'object'}
```

The schema was generated from the type hints and the docstring. A hand-written loop gives control (limits, logging, approval of risky calls); the runner is shorter for simple cases.

> In `anthropic` 2.14.0 the runner on the Claude API and Foundry keeps the conversation server-side by default (Message Threads, beta, not ZDR-eligible). Pass `thread=False` to send the full history yourself. See [Anthropic SDK](../../libraries/anthropic-sdk/advanced.en.md).

## Safety

Tool results are **untrusted data**: a web page or a file may contain text that looks like instructions (indirect prompt injection). Keep them inside `tool_result` blocks and never promote them into the system prompt. Check the arguments of every call against permissions, limit what a tool can touch, and ask a human to approve irreversible actions.

## Best practices

- Cap steps and total tokens in the loop; log every call.
- Return compact results: the model pays attention to everything you send.
- Write errors the model can act on.
- Keep the `tools` array stable between requests for caching.
- Run independent calls in parallel (`asyncio.gather`, threads), but return them in one message.
- Validate `input` yourself: even a good schema does not make the argument safe.

## Common mistakes

- ❌ Forcing a tool on Opus 5.5 / Sonnet 5.5 → ✅ `auto` with `strict` tools or structured outputs.
- ❌ `is_error` for server tools → ✅ read the error from the result block.
- ❌ Treating `pause_turn` as the end → ✅ resend the reply and continue.
- ❌ Copying tool output into the system prompt → ✅ keep it in `tool_result`.
- ❌ A new tool order on every request → ✅ a stable array.

## Related materials

- Next: [expert theory](expert.en.md)
- Caching: [05 · Prompt caching](../05-prompt-caching/advanced.en.md)
- Structured output: [04 · Structured outputs](../04-structured-output/advanced.en.md)

## Sources

- [Tool use overview](https://platform.claude.com/docs/en/agents-and-tools/tool-use/overview)
- [Tool reference](https://platform.claude.com/docs/en/agents-and-tools/tool-use/tool-reference)
- [Define tools](https://platform.claude.com/docs/en/agents-and-tools/tool-use/define-tools)
- [Handling stop reasons](https://platform.claude.com/docs/en/build-with-claude/handling-stop-reasons)
