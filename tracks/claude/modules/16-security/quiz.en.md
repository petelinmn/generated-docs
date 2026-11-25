---
title: "Security and compliance: quiz"
lang: en
track: claude
kind: module
doc: quiz
slug: 16-security
part: 3
tags: [claude, security, prompt-injection, secrets, compliance]
versions: "Claude 5.5 family; Claude Code 2.1.332; anthropic 2.14.0"
status: draft
updated: 2026-11-25
---

# Security and compliance: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 How should tool arguments from the model be treated?

- A) as your own code
- B) as configuration
- C) as untrusted input
- D) as validated

<details><summary>Answer</summary>

**C.** The model produces them and any text it read can steer it, so check them like internet input. They deserve none of the trust of your code or configuration.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Where should an API key live?

- A) a secrets manager or environment variable
- B) the repository
- C) page JavaScript
- D) the system prompt

<details><summary>Answer</summary>

**A.** The key belongs outside the code, readable only by the server process. A repository, page JavaScript and a system prompt can all be read by others.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 What about an irreversible agent action?

- A) do it faster
- B) describe it in the prompt
- C) nothing
- D) require human confirmation

<details><summary>Answer</summary>

**D.** A model mistake in an irreversible action cannot be undone, so a person decides. A prompt description guarantees nothing, and speed only adds damage.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 A suspected key leak:

- A) wait
- B) disable or delete the key
- C) change the model
- D) clear the cache

<details><summary>Answer</summary>

**B.** A leaked key works until disabled, so disable it at once. Waiting, changing models or clearing a cache does not do that.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 Where does a stranger's email content go?

- A) `stop_sequences`
- B) a `tool_result`
- C) the system prompt
- D) a tool name

<details><summary>Answer</summary>

**B.** A `tool_result` marks the text as data returned by a tool. In the system prompt it would gain instruction-level trust; tool names and `stop_sequences` are not for content.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 What is true of injection defence?

- [ ] A) limiting privileges is essential
- [ ] B) one prompt sentence solves it
- [ ] C) unneeded for web page reading
- [ ] D) it is built in layers

<details><summary>Answer</summary>

**A, D.** No single measure is reliable, so combine labelling, screening and limited privileges. Web pages are a typical injection source.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 The Claude Code sandbox on native Windows:

- A) commands run without it
- B) on by default
- C) uses Seatbelt
- D) blocks everything

<details><summary>Answer</summary>

**A.** The sandbox runs on macOS, Linux and WSL2; native Windows gets no isolation although commands run. It is off by default, and Seatbelt is a macOS mechanism.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 The default `inference_geo`?

- A) `eu`
- B) the key's region
- C) `global`
- D) `us`

<details><summary>Answer</summary>

**C.** The default is `global`. `us` must be requested, there is no `eu` value, and the key does not decide it.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 What does Workload Identity Federation give?

- A) free requests
- B) ZDR
- C) client-side prompt encryption
- D) short-lived tokens instead of a static key

<details><summary>Answer</summary>

**D.** Instead of a long-lived secret the application exchanges its identity provider's token for a temporary one. It affects neither price, ZDR nor encryption.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 Why is `startswith` bad for path checks?

- A) it needs absolute paths
- B) it lets a sibling with the same prefix through
- C) it is slow
- D) it does not work in Python

<details><summary>Answer</summary>

**B.** `workspace-evil` starts with `workspace` yet lies outside it; check containment of resolved paths. Speed and Python support are not the issue.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 What is not ZDR-eligible?

- [ ] A) the plain Messages API
- [ ] B) token counting
- [ ] C) Managed Agents
- [ ] D) the MCP connector

<details><summary>Answer</summary>

**C, D.** Managed Agents and the MCP connector are not ZDR-eligible. The Messages API and token counting are.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: an agent reads incoming mail, can send replies and search the CRM. What measures?

<details><summary>Answer</summary>

Mail only as JSON in `tool_result`; CRM access scoped to the sender; sending needs confirmation or is limited to templates and the sender; screen input and output; no arbitrary links or attachments outward; an action log; injection tests; limits.
→ [Advanced theory](advanced.en.md)

</details>
