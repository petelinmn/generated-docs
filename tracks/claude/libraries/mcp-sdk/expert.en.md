---
title: "MCP SDK: expert"
lang: en
track: claude
kind: library
doc: expert
slug: mcp-sdk
part: 2
tags: [claude, mcp, mcp-sdk, tool-use, json-rpc]
versions: "mcp 2.3.0 (Python)"
status: draft
updated: 2026-11-25
---

# MCP SDK: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## `MCPServer`

| Group | Methods and parameters |
|---|---|
| Registration | `tool`, `resource`, `prompt`, `completion`, `add_tool`, `add_resource`, `add_prompt`, `remove_tool`, `remove_prompt` |
| Running | `run("stdio" / "streamable-http" / "sse")`, `run_stdio_async`, `run_streamable_http_async`, `streamable_http_app`, `custom_route` |
| Configuration | `instructions`, `version`, `lifespan`, `middleware`, `extensions`, `log_level`, `cache_hints`, `subscriptions` |
| Security | `auth`, `token_verifier`, `auth_server_provider`, `resource_security`, `request_state_security` |

## `Client`

`list_tools`, `call_tool`, `list_resources`, `list_resource_templates`, `read_resource`, `list_prompts`, `get_prompt`, `complete`, `listen`, `subscribe_resource`

Parameters: `read_timeout_seconds`, `raise_exceptions`, `mode`, `logging_callback`, `elicitation_callback`, `sampling_callback`, `list_roots_callback`, `input_required_max_rounds` (10), `cache`.

Names come from the installed package; not all were exercised. For full protocol control there is the low-level `mcp.server.lowlevel`.

## Anatomy of a call

The client sends JSON-RPC `tools/call`; the server validates arguments with a pydantic model, calls your function and turns the result into `content` and `structured_content`. A primitive return gives `{"result": …}`, a pydantic model its fields.

## Gotchas

1. `print()` in a stdio server corrupts the protocol.
2. A tool error is not an exception: miss `is_error` and you miss the failure.
3. A plain exception tells the model nothing: use `ToolError`.
4. The [Agent SDK](../agent-sdk/advanced.en.md) behaves the opposite way: exception text reaches the model.
5. A resource template is absent from `list_resources`.
6. v2 differs substantially from v1: `FastMCP` examples target the old version.

> ⚠️ Verify: authorisation, elicitation, sampling, subscriptions, completions, the TypeScript SDK and specific host compatibility were not run.

## Trade-offs

| Option | When |
|---|---|
| An MCP server | several applications need the tool |
| Plain tool use | only your application needs it |
| SDK MCP server ([Agent SDK](../agent-sdk/overview.en.md)) | a tool inside the agent process |
| MCP connector | the API calls a remote server itself |

## Interview questions

1. How do tools, resources and prompts differ?
<details><summary>Answer</summary>

By who decides to use them: model, application, user.
</details>

2. stdio or HTTP?
<details><summary>Answer</summary>

stdio is a local single-user process; HTTP a remote multi-user service with authorisation.
</details>

3. Where does a tool's schema come from?
<details><summary>Answer</summary>

From type hints and the docstring.
</details>

4. How do you return a useful error to the model?
<details><summary>Answer</summary>

`ToolError` with a message; other exceptions are hidden.
</details>

5. Why no exception on the client for a tool error?
<details><summary>Answer</summary>

It is a result with `is_error`, part of a normal reply, so the model can react.
</details>

6. How do you test a server?
<details><summary>Answer</summary>

An in-process `Client(mcp)`.
</details>

7. Where do you open database connections?
<details><summary>Answer</summary>

In the lifespan.
</details>

8. Does the Origin check stop unauthorised clients?
<details><summary>Answer</summary>

No, only browser-based attacks; you need authentication.
</details>

9. What is the risk of a third-party MCP server?
<details><summary>Answer</summary>

It runs its own code and returns text that may carry instructions for the model.
</details>

10. What changed in revision 2026-07-28?
<details><summary>Answer</summary>

The protocol became stateless: no `initialize`, version and capabilities travel with every request.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [08 · MCP](../../modules/08-mcp/expert.en.md) · [Anthropic SDK](../anthropic-sdk/overview.en.md)

## Sources

- [What's new in v2](https://py.sdk.modelcontextprotocol.io/whats-new/)
- [Protocol versions](https://py.sdk.modelcontextprotocol.io/protocol-versions/)
- [MCP specification](https://modelcontextprotocol.io/specification/2026-07-28)
