---
title: "MCP and connectors: quiz"
lang: en
track: claude
kind: module
doc: quiz
slug: 08-mcp
part: 2
tags: [claude, mcp, mcp-connector, tool-use, claude-code]
versions: "MCP specification 2026-07-28; Python SDK mcp 2.3.0; anthropic 2.14.0; anthropic-beta mcp-client-2025-11-20"
status: draft
updated: 2026-11-25
---

# MCP and connectors: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which does the model normally choose?

- A) prompts
- B) the transport
- C) tools
- D) resources

<details><summary>Answer</summary>

**C.** Tools are model-controlled: the model decides when to call them. Users pick prompts, the application attaches resources, and the transport is configuration.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 The transport for a local server started as a process?

- A) stdio
- B) Streamable HTTP
- C) SSE
- D) WebSocket

<details><summary>Answer</summary>

**A.** With stdio the client spawns the server and talks over stdin and stdout. Streamable HTTP is for remote servers, the old HTTP+SSE transport is deprecated, and WebSocket is not a standard transport.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 Where should a stdio server write debug messages?

- A) stdout
- B) the response body
- C) `_meta`
- D) stderr

<details><summary>Answer</summary>

**D.** stdout carries protocol messages, so a stray line breaks the exchange. stderr is free for logs. Debug output does not belong in the response body or `_meta`.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 Where does a tool's schema come from in `MCPServer`?

- A) the `README`
- B) the function's type hints
- C) the file name
- D) the model writes it

<details><summary>Answer</summary>

**B.** The SDK builds the JSON Schema from the parameter type hints. The README and file name play no part, and the model only reads the schema.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 What can the MCP connector use?

- A) any transport
- B) only tool calls over HTTPS
- C) local stdio servers
- D) prompts and resources

<details><summary>Answer</summary>

**B.** The connector reaches a remote server over HTTPS from the API side and supports only tools. It cannot reach a local stdio process and does not carry prompts or resources.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 How do you allow just one tool of a server?

- A) delete the others on the server
- B) `tool_choice: none`
- C) list it in `mcp_servers.url`
- D) `default_config.enabled: false` and enable it in `configs`

<details><summary>Answer</summary>

**D.** This is an allowlist: disable by default and enable what you need. Changing the server is unnecessary and often impossible, `tool_choice: none` forbids every call, and a URL does not list tools.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 Which `claude mcp add` scope goes into git?

- A) `project`
- B) `local`
- C) `user`
- D) any

<details><summary>Answer</summary>

**A.** `project` writes `.mcp.json` in the project root, which is committed. `local` (the default) and `user` live in your personal configuration.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What is true of a `.mcp.json` server in a new project?

- [ ] A) it connects without asking
- [ ] B) it is available in the API only
- [ ] C) it waits for the user's approval
- [ ] D) its tools are named `mcp__<server>__<tool>`

<details><summary>Answer</summary>

**C, D.** A server from a repository runs someone's code, so it needs approval, and tool names get a server prefix. It does not connect silently, and this is Claude Code behaviour rather than the API.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 What is true of revision 2026-07-28?

- [ ] A) the version travels in each request's `_meta`
- [ ] B) sessions became mandatory
- [ ] C) `ping` was added
- [ ] D) there is no `initialize` handshake

<details><summary>Answer</summary>

**A, D.** This revision made the protocol stateless: no handshake, and version and capabilities travel with every request. Sessions did not become mandatory, and `ping` was removed, not added.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 A tool raised `ValueError("secret detail")`. What does the SDK client get?

- A) a dropped connection
- B) a generic message without the exception text
- C) the full traceback
- D) `secret detail`

<details><summary>Answer</summary>

**B.** The SDK withholds the text of an unexpected exception so internals do not leak; the client gets an error result with a generic message. The connection stays up, and neither traceback nor secret is sent.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 The error code for an unsupported protocol version?

- A) `-32700`
- B) `404`
- C) `-32022`
- D) `-32601`

<details><summary>Answer</summary>

**C.** `-32022` is the unsupported protocol version error. `-32700` is a parse error, `-32601` an unknown method, and 404 is an HTTP status rather than a JSON-RPC code.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: a colleague wants to connect an MCP server found online to an agent with repository access. What do you check?

<details><summary>Answer</summary>

Who wrote it and what the code does; which tools and what they change; what data leaves; allowlist them; pass no secrets; treat results as untrusted; sandbox and confirm dangerous actions; pin the version.
→ [Advanced theory](advanced.en.md)

</details>
