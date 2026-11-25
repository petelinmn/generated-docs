---
title: "MCP SDK: quiz"
lang: en
track: claude
kind: library
doc: quiz
slug: mcp-sdk
part: 2
tags: [claude, mcp, mcp-sdk, tool-use, json-rpc]
versions: "mcp 2.3.0 (Python)"
status: draft
updated: 2026-11-25
---

# MCP SDK: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which decorator declares what the model calls?

- A) `@mcp.prompt()`
- B) `@mcp.completion()`
- C) `@mcp.tool()`
- D) `@mcp.resource()`

<details><summary>Answer</summary>

**C.** Tools are model-controlled. The application attaches resources, users pick prompts, and completion suggests argument values.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 The default transport of `mcp.run()`?

- A) stdio
- B) Streamable HTTP
- C) SSE
- D) WebSocket

<details><summary>Answer</summary>

**A.** With no argument the server uses stdio. HTTP and SSE are explicit, and `run` has no WebSocket option.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 Where does a tool's description come from?

- A) the file name
- B) the model invents it
- C) `pyproject.toml`
- D) the docstring

<details><summary>Answer</summary>

**D.** The SDK takes the function's docstring, so write it for the model. The rest plays no part.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 What is the simplest way to test a server?

- A) you cannot
- B) an in-process `Client(mcp)`
- C) only through Claude Desktop
- D) curl over stdio

<details><summary>Answer</summary>

**B.** The client takes the server object and runs the protocol without network or processes. `curl` cannot speak stdio.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 A tool raised `RuntimeError('password=…')`. What does the client get?

- A) a success
- B) a generic message without the text
- C) the full exception text
- D) a dropped connection

<details><summary>Answer</summary>

**B.** `MCPServer` hides unexpected exception text and returns `is_error`. It stays in the server log.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 How do you know a tool call failed?

- A) wait for an exception
- B) an empty `content`
- C) the HTTP status
- D) check `result.is_error`

<details><summary>Answer</summary>

**D.** A tool error is a normal result with a flag. No exception is raised, `content` is not empty and the transport succeeds.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What does `print()` do in a stdio server?

- A) corrupts the protocol stream
- B) logs to the client as intended
- C) nothing
- D) always stops the server

<details><summary>Answer</summary>

**A.** The client reads stdout as JSON-RPC and hits a parse error. Debug output belongs in stderr.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 Where does the resource `orders://{order_id}` appear?

- A) `list_tools`
- B) `list_prompts`
- C) `list_resource_templates`
- D) `list_resources`

<details><summary>Answer</summary>

**C.** A parameterised URI is a template. `list_resources` lists only concrete URIs.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 Which protocol version did a 2.3.0 client negotiate by default?

- A) `2025-11-25`
- B) `1.0`
- C) `2024-01-01`
- D) `2026-07-28`

<details><summary>Answer</summary>

**D.** `2025-11-25` is what `legacy` mode gave. The other values did not occur.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 What did the local HTTP server answer to a foreign `Host`?

- A) 500
- B) 421
- C) 200
- D) 401

<details><summary>Answer</summary>

**B.** 421 Misdirected Request guards against DNS rebinding. It is not a 401: no authentication is involved.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 What is in `structured_content` when a tool returns `str`?

- A) always `None`
- B) a list of characters
- C) an object with a `result` key
- D) the bare string

<details><summary>Answer</summary>

**C.** A primitive is wrapped because the structured result follows an object schema. A pydantic model yields its fields.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: publish a server with order access for a team. What do you do?

<details><summary>Answer</summary>

Streamable HTTP behind TLS; OAuth or token verification; per-user authorisation rather than per server; `ToolError` without internals; size and rate limits; an audit log; `Client` tests; a pinned SDK version.
→ [Advanced theory](advanced.en.md)

</details>
