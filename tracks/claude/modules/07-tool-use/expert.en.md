---
title: "Tool use: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 07-tool-use
part: 1
tags: [claude, tool-use, tool-choice, stop-reason, server-tools, parallel-tool-calls, tool-result]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Tool use: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. The reference comes from the documentation pages read at the time of writing (overview, define tools, handle tool calls, tool reference in full; the parallel tool use and stop reasons pages only partly); what depends on a summary is marked.

## Reference

### Tool definition fields

| Field | Rule |
|---|---|
| `name` | `^[a-zA-Z0-9_-]{1,128}$` |
| `description` | plain text; the documentation advises at least 3–4 sentences |
| `input_schema` | a JSON Schema object |
| `input_examples` | optional; each example must validate against `input_schema` (the API usually answers 400 if not; the check is skipped in some cases, e.g. a schema with `pattern`); ~100 tokens (flat) / ~200 (nested) each; not for server tools |
| `strict` | optional; schema validation of names and inputs; not for `mcp_toolset` and the computer/browser toolsets; a `strict` tool cannot list the code-execution caller in `allowed_callers` |
| `cache_control` | optional; a breakpoint at this definition; for toolset entries it goes on the entry itself |
| `defer_loading` | optional; loads the tool on demand via tool search; cannot be combined with `cache_control` |
| `allowed_callers` | optional; `"direct"` (default) or `"code_execution_20260120"` |
| `eager_input_streaming` | optional boolean; user-defined tools only |

### tool_use and tool_result blocks

| Block | Fields |
|---|---|
| `tool_use` | `type`, `id` (e.g. `toolu_…`), `name`, `input`; `caller` for programmatic calls; `toolset_name` (`computer`/`browser`) for toolset members |
| `server_tool_use` | like `tool_use`, id prefix `srvtoolu_`; the result is a separate block |
| `tool_result` | `type`, `tool_use_id`, optional `content` (a string or a list of `text`, `image`, `document`, `search_result` blocks), optional `is_error`; computer/browser results echo `toolset_name` |

An empty result (no `content`) is valid.

### tool_choice

| Value | Effect |
|---|---|
| `auto` | the model decides; default with `tools` |
| `any` | some tool must be called |
| `tool` (+ `name`) | exactly this tool |
| `none` | no tools; default without `tools` |
| `disable_parallel_tool_use: true` | a field **inside** `tool_choice`; with `auto` at most one call, with `any`/`tool` exactly one |

With `any`/`tool` the reply starts with the call (no text). Not supported with manual thinking, and returns a 400 on Fable 5.2, Mythos 5.2, Opus 5.5, Sonnet 5.5, Fable 5.1, Mythos 5.1. Computer/browser toolsets reject `tool`. Changing `tool_choice` invalidates cached message blocks.

### stop_reason values

| Value | Meaning | What you do |
|---|---|---|
| `end_turn` | natural end | use the reply |
| `max_tokens` | hit `max_tokens` | raise it; if the last block is an incomplete `tool_use`, retry with more |
| `stop_sequence` | one of your stop sequences | read `stop_sequence` |
| `tool_use` | a tool of yours is called | run it, return the result |
| `pause_turn` | a server-tool loop reached its limit | resend the reply as the assistant turn |
| `refusal` | declined | read `stop_details` |
| `model_context_window_exceeded` | context full | treat as truncated |

### Server tool types

| Tool | Type strings |
|---|---|
| Web search | `web_search_20260318`, `web_search_20260209`, `web_search_20250305` |
| Web fetch | `web_fetch_20260318`, `web_fetch_20260309`, `web_fetch_20260209`, `web_fetch_20250910` |
| Code execution | `code_execution_20260521`, `code_execution_20260120`, `code_execution_20250825` (legacy `code_execution_20250522`, Python only) |
| Tool search | `tool_search_tool_regex_20251119`, `tool_search_tool_bm25_20251119` (+ undated aliases) |
| Advisor | `advisor_20260301` (beta header `advisor-tool-2026-03-01`) |
| MCP connector | `mcp_toolset` (beta header `mcp-client-2025-11-20`) |

Client-executed Anthropic tools: `memory_20250818`, `bash_20250124`, `text_editor_20250728` (Claude 4+) / `text_editor_20250124`, computer use (`computer_toolset_20260801`, `computer_20251124`, `computer_20250124`), browser use (`browser_toolset_20260801`).

## Internals

- **Stateless.** The API keeps nothing between requests: the whole history, including the assistant's `tool_use` blocks and your `tool_result`s, travels with each request. The pairing by `id` is checked on every call.
- **The tool prompt.** The API adds a system prompt for tool use (hundreds of tokens, depending on the model and `tool_choice`) before your tools; it counts as input.
- **Parallelism is the model's choice.** The API does not prescribe an order of execution; Claude 4 and later call in parallel by default. You may run the calls concurrently or one by one. Return one `tool_result` per `tool_use`, all in the next user message.
- **Skipped calls.** If you do not execute a call, answer it with `is_error: true` ("Not executed: …"): an unanswered id is a 400.
- **Server loops.** Server tools iterate inside one request; when it exceeds its limit the API returns `pause_turn` and expects the reply to be sent back.

> ⚠️ Verify: the default limit of 10 server-loop iterations and the exact `pause_turn` resend procedure come from an abridged summary of the stop reasons page.

## Gotchas

### 1. Text before the results

A user message with text before `tool_result` blocks is rejected: "tool_use ids were found without tool_result blocks immediately after." The mock reproduced it as a 400.

### 2. Text after the results

Allowed when the turn has only client tools, but a trailing text can provoke an empty `end_turn` (2–3 tokens); with a server tool without a result yet the user message must hold only `tool_result` blocks, or the request fails with a 400. Send the results alone.

### 3. Every call needs an answer

A failed or skipped call still gets a `tool_result` (`is_error: true`).

### 4. Forced tool use on 5.5 models

`any` and `tool` return a 400 on Opus 5.5 and Sonnet 5.5; `auto` plus `strict` tools or structured outputs replaces them.

### 5. `tool_choice` and the cache

Changing `tool_choice` between requests invalidates cached message blocks, while tool definitions and the system prompt stay cached.

### 6. The unbounded loop

If the model keeps calling tools, `while True` never ends: cap steps. The check in exercise 5 raises `no final answer after 3 steps`.

### 7. `is_error` for server tools

Do not send it; the error is inside the server tool's result block.

### 8. Truncated `tool_use`

At `max_tokens` the last `tool_use` may be incomplete: do not run it; retry with a larger limit.

### 9. Untrusted results

A result is data, not instructions: keep it in `tool_result`, never in the system prompt.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Hand-written loop | full control: limits, logging, approvals | more code |
| SDK tool runner | short | less visible control |
| Server tools | no code to host | less control over the details |
| Structured outputs | a guaranteed shape, no loop | no side effects, no external data |
| A workflow in code | predictable, cheap | the model does not decide the steps |
| Managed agents | the loop and sandbox hosted for you | a different surface ([agents](../../overview.en.md)) |

## Interview questions

<details><summary>1. What does the model return when it wants to call a tool?</summary>

A reply with a `tool_use` block (`id`, `name`, `input`) and `stop_reason: "tool_use"`.

→ [Basic theory](basics.en.md#the-loop-in-four-steps)

</details>

<details><summary>2. Who runs a client tool?</summary>

Your application. The model only asks.

→ [Basic theory](basics.en.md#why-it-matters)

</details>

<details><summary>3. What goes into the next request after <code>tool_use</code>?</summary>

The reply `content` unchanged as an `assistant` message, then a `user` message with one `tool_result` per call.

→ [Basic theory](basics.en.md#call-and-result)

</details>

<details><summary>4. Trick: can the text come before the <code>tool_result</code> blocks?</summary>

No: results must come first and directly follow the call, otherwise a 400 ("tool_use ids were found without tool_result blocks immediately after").

→ [Gotcha 1](#1-text-before-the-results)

</details>

<details><summary>5. How are parallel results returned?</summary>

All in one `user` message, one `tool_result` per `tool_use`.

→ [Basic theory](basics.en.md#parallel-calls)

</details>

<details><summary>6. What do you do when a tool fails?</summary>

Return a `tool_result` with `is_error: true` and an actionable message.

→ [Basic theory](basics.en.md#errors-in-tools)

</details>

<details><summary>7. Trick: where does <code>disable_parallel_tool_use</code> go?</summary>

Inside the `tool_choice` object, not at the top level of the request.

→ [Reference](#tool_choice)

</details>

<details><summary>8. What are <code>any</code> and <code>tool</code>, and what is their catch?</summary>

They force a tool call (any tool / this tool); the reply starts with the call. They are not supported with manual thinking and return a 400 on the Claude 5.5 models listed in the documentation.

→ [Advanced theory](advanced.en.md#forced-tool-use-and-models)

</details>

<details><summary>9. What is <code>pause_turn</code>?</summary>

A server-tool loop reached its iteration limit; resend the reply as the assistant turn to continue. A call to your tool ends with `tool_use`, never `pause_turn`.

→ [Advanced theory](advanced.en.md#client-tools-and-server-tools)

</details>

<details><summary>10. Trick: should <code>is_error</code> be sent for a web search failure?</summary>

No; server tool errors are inside the result block.

→ [Gotcha 7](#7-is_error-for-server-tools)

</details>

<details><summary>11. How do tools affect the cost?</summary>

Definitions, an automatic tool system prompt (about 286 tokens on Opus 5.5 / Sonnet 5.5 for `auto`/`none`), and the `tool_use`/`tool_result` blocks in the history.

→ [Advanced theory](advanced.en.md#cost-in-tokens)

</details>

<details><summary>12. How do you protect against prompt injection through tool results?</summary>

Keep results inside `tool_result`, never in the system prompt; check arguments against permissions; require human approval for irreversible actions.

→ [Advanced theory](advanced.en.md#safety)

</details>

## Cheat sheet

```text
tool: {"name", "description", "input_schema", "strict"?, "input_examples"?, "cache_control"?}
reply: stop_reason == "tool_use" -> blocks {"type":"tool_use","id","name","input"}
next:  assistant = reply.content (unchanged); user = [tool_result per call (first!), ...]
tool_result: {"type":"tool_result","tool_use_id","content"?,"is_error"?}
tool_choice: auto | any | tool(+name) | none ; disable_parallel_tool_use inside it
any/tool: 400 on Opus 5.5, Sonnet 5.5, Fable 5.2 ...
pause_turn: resend the reply as the assistant turn
```

- Results first, all in one message.
- Every call gets an answer.
- Cap the steps.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Caching: [05 · Prompt caching](../05-prompt-caching/expert.en.md)
- Structured output: [04 · Structured outputs](../04-structured-output/expert.en.md)

## Sources

- [Tool use overview](https://platform.claude.com/docs/en/agents-and-tools/tool-use/overview)
- [Define tools](https://platform.claude.com/docs/en/agents-and-tools/tool-use/define-tools)
- [Handle tool calls](https://platform.claude.com/docs/en/agents-and-tools/tool-use/handle-tool-calls)
- [Parallel tool use](https://platform.claude.com/docs/en/agents-and-tools/tool-use/parallel-tool-use)
- [Tool reference](https://platform.claude.com/docs/en/agents-and-tools/tool-use/tool-reference)
- [Handling stop reasons](https://platform.claude.com/docs/en/build-with-claude/handling-stop-reasons)
