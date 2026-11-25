---
title: "MCP and connectors: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 08-mcp
part: 2
tags: [claude, mcp, mcp-connector, tool-use, claude-code]
versions: "MCP specification 2026-07-28; Python SDK mcp 2.3.0; anthropic 2.14.0; anthropic-beta mcp-client-2025-11-20"
status: draft
updated: 2026-11-25
---

# MCP and connectors: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## The 2026-07-28 protocol

Messages are JSON-RPC 2.0. The protocol is **stateless**: the `initialize` handshake and sessions are gone, every request carries the client's version and capabilities in `_meta`.

```json
{"jsonrpc": "2.0", "id": 2, "method": "tools/call", "params": {
  "name": "add", "arguments": {"a": 2, "b": 3},
  "_meta": {"io.modelcontextprotocol/protocolVersion": "2026-07-28",
            "io.modelcontextprotocol/clientCapabilities": {}}}}
{"jsonrpc": "2.0", "id": 2, "result": {"content": [{"type": "text", "text": "5"}], "isError": false,
  "resultType": "complete", "structuredContent": {"result": 5}, "_meta": {"...": "..."}}}
```

| Method | Purpose |
|---|---|
| `server/discover` | versions, capabilities, server identity |
| `tools/list`, `tools/call` | tools |
| `resources/list`, `resources/templates/list`, `resources/read` | resources |
| `prompts/list`, `prompts/get` | prompts |
| `subscriptions/listen` | a stream of change notifications |

Results carry `resultType` (`complete` or `input_required`). `input_required` is a multi round-trip request: the server asks for input and the client retries with answers. `ping`, `logging/setLevel` and `resources/subscribe` were removed.

## Errors observed from the SDK server

| Case | Response |
|---|---|
| no `_meta` with the version | `-32602` |
| version `1999-01-01` | `-32022`, `data.supported` |
| `ping` | `-32601 Method not found` |
| unknown resource | `-32602` |
| HTTP: `Mcp-Method` header differs from the body | 400, `-32020` |
| HTTP: a foreign `Origin` | 403 |

## Streamable HTTP

A POST with `MCP-Protocol-Version`, `Mcp-Method` and `Mcp-Name` headers and `Accept: application/json, text/event-stream`. This revision has no `Mcp-Session-Id` and no `Last-Event-ID` stream resumption.

## Compatibility

`Client(..., mode="legacy")` against the same server negotiated `2025-11-25`; the default (`auto`) negotiated `2026-07-28`.

## MCP connector: fields and beta features

| Field | Rule |
|---|---|
| `mcp_servers[].type` | `url` only |
| `mcp_servers[].url` | starts with `https://` |
| `configs.<tool>.enabled` / `defer_loading` | `true` / `false` by default |
| precedence | `configs` > `default_config` > defaults |
| unknown name in `configs` | not an error |

With the `mcp-client-2026-09-15` beta header the response includes an `mcp_tool_listing` block, and the tool list can be pinned in the toolset's `tools` so server changes do not alter a conversation.

**MCP tunnels** — a research preview: `cloudflared` and a proxy inside your network open an outbound-only connection.

> ⚠️ Verify: OAuth authorization, `input_required`, subscriptions, extensions (tasks), list pinning and tunnels are described from the documentation and were not run.

## Gotchas

1. An ordinary exception in a tool reached the client only as `Error executing tool order_status`: the text is hidden. Raise `ToolError` to tell the model something.
2. A `print` at stdio server start-up made the client log `Failed to parse JSONRPC message`.
3. A wrong argument type came back as an `isError` result, not a protocol error; so did a call to an unknown tool in this SDK.
4. Articles about `initialize`, `FastMCP` and `Mcp-Session-Id` describe earlier revisions and SDK versions.

## Interview questions

1. How does MCP differ from tool use?
<details><summary>Answer</summary>

Tool use is the calling mechanism of the model API; MCP is a protocol between an application and a capability server. MCP tools reach the model through tool use.
</details>

2. Tools, resources, prompts?
<details><summary>Answer</summary>

Actions the model picks; data the application picks; templates the user picks.
</details>

3. What changed in 2026-07-28?
<details><summary>Answer</summary>

The protocol became stateless: no `initialize`, no sessions, version and capabilities travel in each request's `_meta`, and `server/discover` appeared.
</details>

4. Protocol error vs tool error?
<details><summary>Answer</summary>

The first is a JSON-RPC `error`; the second is a result with `isError: true` that the model sees.
</details>

5. Why not print to stdout?
<details><summary>Answer</summary>

Under stdio it carries protocol messages.
</details>

6. What are the risks of a third-party server?
<details><summary>Answer</summary>

Leaking arguments, instruction injection in results and descriptions, a changing tool list, dangerous actions.
</details>

7. Connector or own client?
<details><summary>Answer</summary>

The connector for public HTTP servers and tools only; your own client for local servers, resources, prompts and your own approval policy.
</details>

8. Where do Roots, Sampling and Logging stand?
<details><summary>Answer</summary>

Deprecated in 2026-07-28.
</details>

9. How does a server keep state without sessions?
<details><summary>Answer</summary>

It mints an explicit handle that the client passes back as an ordinary tool argument.
</details>

10. What are MCP tunnels?
<details><summary>Answer</summary>

A research preview: Claude reaching servers in a private network over an outbound-only connection.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [07 · Tool use](../07-tool-use/expert.en.md)

## Sources

- [Key changes in 2026-07-28](https://modelcontextprotocol.io/specification/2026-07-28/changelog)
- [Versioning](https://modelcontextprotocol.io/specification/versioning)
- [Streamable HTTP](https://modelcontextprotocol.io/specification/2026-07-28/basic/transports/streamable-http)
- [MCP tunnels](https://platform.claude.com/docs/en/agents-and-tools/mcp-tunnels/overview)
