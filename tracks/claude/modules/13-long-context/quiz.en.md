---
title: "Compaction and long context: quiz"
lang: en
track: claude
kind: module
doc: quiz
slug: 13-long-context
part: 3
tags: [claude, context-window, compaction, context-editing, token-counting]
versions: "Claude 5.5 family; anthropic 2.14.0; beta compact-2026-09-04, compact-2026-01-12, context-management-2025-06-27; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Compaction and long context: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What takes up window space?

- A) input only
- B) everything except cached tokens
- C) everything in the request and the output
- D) user messages only

<details><summary>Answer</summary>

**C.** The window holds the system prompt, tools, the whole history and what the model is generating. Caching lowers the price but frees no space.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 The input alone is longer than the window:

- A) 400 `prompt is too long`
- B) silent truncation
- C) `end_turn`
- D) an automatic summary

<details><summary>Answer</summary>

**A.** When the input alone does not fit, the API rejects the request. It neither truncates nor summarises silently, and `end_turn` belongs to a successful reply.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 What is true of token counting?

- A) priced like generation
- B) always exact
- C) beta only
- D) free and an estimate

<details><summary>Answer</summary>

**D.** Token counting is free but is an estimate that may differ slightly from actual use. It is not a beta and generates nothing.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 Why is trimming by message count risky?

- A) no risk
- B) it can split a `tool_use`/`tool_result` pair
- C) the window grows
- D) the model changes

<details><summary>Answer</summary>

**B.** Every `tool_use` needs its `tool_result`; cutting by count can leave half a pair and the API returns an error. Neither window nor model changes.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 What does an on-demand compaction request return?

- A) a token count
- B) a single `compaction` block
- C) a reply plus a summary
- D) the whole cleaned history

<details><summary>Answer</summary>

**B.** This request only compacts: the reply is a `compaction` block with the summary, which you put into history. It does not answer the question, count tokens or return a cleaned history.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 The default threshold compaction trigger?

- A) 50,000
- B) 1M
- C) 80% of the window
- D) 150,000 input tokens

<details><summary>Answer</summary>

**D.** The default trigger is 150,000 input tokens, an absolute number rather than a share of the window. The other values listed are not the default.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What is true of tool result clearing?

- [ ] A) it happens server-side
- [ ] B) it breaks the cached prefix
- [ ] C) you must delete blocks yourself
- [ ] D) it keeps 10 pairs by default

<details><summary>Answer</summary>

**A, B.** Clearing happens on the API side before the model sees the input, so you delete nothing yourself. It changes the prefix and breaks the cache; by default the 3 most recent tool uses are kept, not 10.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What does `instructions` do in `compact_20260112`?

- A) sets the conversation system prompt
- B) limits tokens
- C) replaces the summarisation prompt
- D) extends it

<details><summary>Answer</summary>

**C.** Your text replaces the default summarisation instruction entirely rather than adding to it. The field sets neither the conversation's system prompt nor limits.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 The on-demand compaction header?

- A) `compact-2026-01-12`
- B) `context-management-2025-06-27`
- C) none
- D) `compact-2026-09-04`

<details><summary>Answer</summary>

**D.** On-demand compaction uses `compact-2026-09-04`. `compact-2026-01-12` is for threshold compaction and `context-management-2025-06-27` for clearing tool results.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 The summary request's `stop_reason` is not `compaction`. What now?

- A) retry for ever
- B) keep the old history
- C) replace it anyway
- D) delete half

<details><summary>Answer</summary>

**B.** Without a `compaction` block there is no summary, so replacing or deleting history would lose context. Endless retries only spend money.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 Which of these have a 1M window?

- [ ] A) Haiku 4.5
- [ ] B) every model
- [ ] C) Opus 5.5
- [ ] D) Haiku 5.5

<details><summary>Answer</summary>

**C, D.** Opus 5.5 and Haiku 5.5 have a 1M window. Haiku 4.5 has 200k, so "any model" is wrong.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: an agent runs for hours and forgets the requirements after compaction. What do you do?

<details><summary>Answer</summary>

Keep requirements in the system prompt or a project file; keep an external decision log (memory tool, files); give the summary a focus; keep recent turns; clear bulky tool results before compaction is needed; delegate reading to subagents.
→ [Advanced theory](advanced.en.md)

</details>
