---
title: "Claude Agent SDK: quiz"
lang: en
track: claude
kind: library
doc: quiz
slug: agent-sdk
part: 2
tags: [claude, agent-sdk, agents, tool-use, hooks]
versions: "claude-agent-sdk 0.2.189"
status: draft
updated: 2026-11-25
---

# Claude Agent SDK: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does the Agent SDK add over the API client?

- A) its own models
- B) offline use
- C) a ready agent loop with tools
- D) cheaper tokens

<details><summary>Answer</summary>

**C.** It is Claude Code as a library. Models and prices are the same and it still needs the API.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Which message comes last from `query()`?

- A) `ResultMessage`
- B) `SystemMessage`
- C) `UserMessage`
- D) `StreamEvent`

<details><summary>Answer</summary>

**A.** `ResultMessage` sums up the run. `SystemMessage` opens it, `UserMessage` carries tool results and `StreamEvent` partial output.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 The full name of tool `order_status` on server `shop`?

- A) `shop.order_status`
- B) `order_status`
- C) `shop/order_status`
- D) `mcp__shop__order_status`

<details><summary>Answer</summary>

**D.** MCP tools get an `mcp__<server>__` prefix. Permission rules written otherwise will not match.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 Why `setting_sources=[]`?

- A) disable sessions
- B) read no settings from disk
- C) disable tools
- D) block the network

<details><summary>Answer</summary>

**B.** An empty list makes a run machine-independent. It disables neither tools, network nor session writing.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 A tool is in `allowed_tools` and `can_use_tool` is set. What happens?

- A) a startup error
- B) the callback is not called
- C) it is called twice
- D) the call is denied

<details><summary>Answer</summary>

**B.** Rule approval comes before the callback; the SDK only warns. Use a hook to check every call.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 What happens when `max_turns` runs out in `query()`?

- [ ] A) then `ResultError` is raised
- [ ] B) a quiet `success`
- [ ] C) an endless retry
- [ ] D) an `error_max_turns` result arrives

<details><summary>Answer</summary>

**A, D.** The error message is yielded first and the exception follows, so keep the result before catching. It is not a success.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What does `resume` with `fork_session=True` do?

- A) branches under a new ID
- B) overwrites the original
- C) deletes history
- D) merges two sessions

<details><summary>Answer</summary>

**A.** History is copied into a new session and the original stays. Nothing is deleted or merged.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 How do you reliably check every tool call?

- A) a system prompt sentence
- B) `allowed_tools`
- C) a `PreToolUse` hook
- D) `can_use_tool`

<details><summary>Answer</summary>

**C.** A hook fires on every call, approved ones included. The callback can be shadowed, a prompt guarantees nothing and `allowed_tools` approves unchecked.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 A tool raised `ValueError('password=…')`. What does the model see?

- A) only “tool error”
- B) nothing
- C) a traceback without the message
- D) that text

<details><summary>Answer</summary>

**D.** The SDK server forwards the exception message. That is Agent SDK behaviour; the MCP SDK's `MCPServer` hides it.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 What does `allowed_tools` mean?

- A) a call order
- B) calls approved automatically
- C) the only available tools
- D) a logging list

<details><summary>Answer</summary>

**B.** Other tools remain available and go through normal permission checks. Use `disallowed_tools` and `tools` to remove them.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 Why did `resume` fail on another server?

- A) sessions live only in the API
- B) a different SDK key is needed
- C) sessions are local files
- D) the ID lasts a minute

<details><summary>Answer</summary>

**C.** The files sit in that machine's configuration and depend on the directory. Copy them or use a `session_store`.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: an agent inside a web service handles customer tickets. What do you configure?

<details><summary>Answer</summary>

A container per customer; `setting_sources=[]`; narrow `tools`; a hook on risky actions; tools that compose their error messages; `max_turns` and a budget; inspect `permission_denials`; session retention and deletion; an API key rather than claude.ai login.
→ [Advanced theory](advanced.en.md)

</details>
