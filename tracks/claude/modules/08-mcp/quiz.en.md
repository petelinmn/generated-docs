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

**C.**
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 The transport for a local server started as a process?

- A) stdio
- B) Streamable HTTP
- C) SSE
- D) WebSocket

<details><summary>Answer</summary>

**A.**
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 Where should a stdio server write debug messages?

- A) stdout
- B) the response body
- C) `_meta`
- D) stderr

<details><summary>Answer</summary>

**D.**
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 Where does a tool's schema come from in `MCPServer`?

- A) the `README`
- B) the function's type hints
- C) the file name
- D) the model writes it

<details><summary>Answer</summary>

**B.**
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 What can the MCP connector use?

- A) any transport
- B) only tool calls over HTTPS
- C) local stdio servers
- D) prompts and resources

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 How do you allow just one tool of a server?

- A) delete the others on the server
- B) `tool_choice: none`
- C) list it in `mcp_servers.url`
- D) `default_config.enabled: false` and enable it in `configs`

<details><summary>Answer</summary>

**D.**
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 Which `claude mcp add` scope goes into git?

- A) `project`
- B) `local`
- C) `user`
- D) any

<details><summary>Answer</summary>

**A.**
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What is true of a `.mcp.json` server in a new project?

- [ ] A) it connects without asking
- [ ] B) it is available in the API only
- [ ] C) it waits for the user's approval
- [ ] D) its tools are named `mcp__<server>__<tool>`

<details><summary>Answer</summary>

**C, D.**
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 What is true of revision 2026-07-28?

- [ ] A) the version travels in each request's `_meta`
- [ ] B) sessions became mandatory
- [ ] C) `ping` was added
- [ ] D) there is no `initialize` handshake

<details><summary>Answer</summary>

**A, D.**
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 A tool raised `ValueError("secret detail")`. What does the SDK client get?

- A) a dropped connection
- B) a generic message without the exception text
- C) the full traceback
- D) `secret detail`

<details><summary>Answer</summary>

**B.**
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 The error code for an unsupported protocol version?

- A) `-32700`
- B) `404`
- C) `-32022`
- D) `-32601`

<details><summary>Answer</summary>

**C.**
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: a colleague wants to connect an MCP server found online to an agent with repository access. What do you check?

<details><summary>Answer</summary>

Who wrote it and what the code does; which tools and what they change; what data leaves; allowlist them; pass no secrets; treat results as untrusted; sandbox and confirm dangerous actions; pin the version.
→ [Advanced theory](advanced.en.md)

</details>
