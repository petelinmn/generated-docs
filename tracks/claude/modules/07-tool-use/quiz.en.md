---
title: "Quiz: tool use"
lang: en
track: claude
kind: module
doc: quiz
slug: 07-tool-use
part: 1
tags: [claude, tool-use, tool-choice, parallel-tool-calls]
status: draft
updated: 2026-11-24
---

# Quiz: tool use

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Who executes a client tool?

- A) Claude, inside the model
- B) Anthropic's servers
- C) Your application
- D) The SDK, automatically always

<details><summary>Answer</summary>

**C.** The model only emits a `tool_use` block. (D is wrong: the SDK runs functions only if you use the tool runner.)
→ [Basic theory](basics.en.md#why-it-matters)

</details>

## 2. 🟢 What signals that a tool should be run?

- A) `stop_reason: "tool_use"` and `tool_use` blocks in `content`
- B) The word "tool" in the text
- C) An HTTP 202
- D) `stop_reason: "end_turn"`

<details><summary>Answer</summary>

**A.**
→ [Basic theory](basics.en.md#the-loop-in-four-steps)

</details>

## 3. 🟢 What must the assistant message in the next request contain?

- A) Only the text of the reply
- B) The reply `content` unchanged, including `tool_use` blocks
- C) Nothing: the API remembers
- D) Only the `tool_use` blocks

<details><summary>Answer</summary>

**B.** The API is stateless and checks the pairing by `id`.
→ [Basic theory](basics.en.md#call-and-result)

</details>

## 4. 🟢 The model made two parallel calls. How do you return the results?

- A) Two user messages, one result each
- B) One user message with two `tool_result` blocks
- C) Only the first one
- D) As text in the system prompt

<details><summary>Answer</summary>

**B.** One `tool_result` per `tool_use`, all together.
→ [Basic theory](basics.en.md#parallel-calls)

</details>

## 5. 🟡 A tool raised an exception. What do you do?

- A) End the conversation
- B) Send nothing and continue
- C) Send a `tool_result` with `is_error: true` and an actionable message
- D) Resend the original request

<details><summary>Answer</summary>

**C.** An unanswered call is a 400, and the model can recover from a clear error.
→ [Basic theory](basics.en.md#errors-in-tools)

</details>

## 6. 🟡 What does `tool_choice: {"type": "none"}` do?

- A) Deletes the tools from the request
- B) Forbids the model to call tools in this request
- C) Forces a tool call
- D) Turns parallelism off

<details><summary>Answer</summary>

**B.** It stays available on all models.
→ [Basic theory](basics.en.md#choosing-a-tool)

</details>

## 7. 🟡 You need a guaranteed tool call on Opus 5.5. What is the right approach?

- A) `tool_choice: any`
- B) `tool_choice: {"type": "tool", ...}`
- C) `auto` with `strict` tools, or structured outputs
- D) `disable_parallel_tool_use`

<details><summary>Answer</summary>

**C.** According to the documentation `any`/`tool` return a 400 on Opus 5.5 and Sonnet 5.5.
→ [Advanced theory](advanced.en.md#forced-tool-use-and-models)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) `disable_parallel_tool_use` is a field inside `tool_choice`
- [ ] B) A server tool error is sent back with `is_error: true`
- [ ] C) `pause_turn` means resending the reply as the assistant turn
- [ ] D) Tool results can be copied into the system prompt

<details><summary>Answer</summary>

**A, C.** B is wrong: server tool errors are inside the result block. D is wrong: results are untrusted data.
→ [Expert](expert.en.md#tool_choice)

</details>

## 9. 🟡 What does changing `tool_choice` between requests do to the cache?

- A) Nothing
- B) Invalidates cached message blocks (definitions and the system prompt stay cached)
- C) Invalidates everything
- D) Turns caching off

<details><summary>Answer</summary>

**B.**
→ [Gotcha 5](expert.en.md#5-tool_choice-and-the-cache)

</details>

## 10. 🔴 Find the mistake

```python
messages.append({"role": "user", "content": [
    {"type": "text", "text": "Here are the results:"},
    {"type": "tool_result", "tool_use_id": b.id, "content": "15"}]})
```

<details><summary>Answer</summary>

Text before `tool_result`: the API answers 400 ("tool_use ids were found without tool_result blocks immediately after"). Put results first; better send them alone.
→ [Gotcha 1](expert.en.md#1-text-before-the-results)

</details>

## 11. 🔴 What happens at `max_tokens` with a half-finished `tool_use`?

- A) Run it with what there is
- B) Do not run it; retry with a larger `max_tokens`
- C) Switch to `pause_turn`
- D) Drop the tool

<details><summary>Answer</summary>

**B.** The input may be incomplete.
→ [Gotcha 8](expert.en.md#8-truncated-tool_use)

</details>

## 12. 🔴 Open question

Design a loop for an assistant with a database tool and web search that is safe to put in production.

<details><summary>Key points of a good answer</summary>

- The loop with a step and token limit; log every call.
- One user message with all `tool_result` blocks, results first; `is_error` with an actionable text.
- `pause_turn` is handled for server tools; `max_tokens` with an incomplete `tool_use` is a retry.
- Results are untrusted: kept in `tool_result`; arguments checked against permissions; a read-only database role; human approval for writes.
- A stable `tools` array and `cache_control` for the cache; compact results.

</details>

→ [Advanced theory](advanced.en.md)
