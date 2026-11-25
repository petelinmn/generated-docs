---
title: "Compaction and long context: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 13-long-context
part: 3
tags: [claude, context-window, compaction, context-editing, token-counting]
versions: "Claude 5.5 family; anthropic 2.14.0; beta compact-2026-09-04, compact-2026-01-12, context-management-2025-06-27; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Compaction and long context: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Beta headers and parameters

| Feature | Header | Parameter |
|---|---|---|
| on demand | `compact-2026-09-04` | top-level `compaction: {"type": "summarize"}` |
| threshold | `compact-2026-01-12` | `context_management.edits[compact_20260112]` |
| editing | `context-management-2025-06-27` | `clear_tool_uses_20250919`, `clear_thinking_20251015` |

## Other facts from the documentation

- 1M models: up to 128k output tokens; up to 600 images or PDF pages per request (100 at 200k).
- Previous thinking blocks stay in context by default on Opus 4.5+, Sonnet 4.6+, Haiku 5.5 and Fable/Mythos; on older models the API strips them.
- Only Sonnet 4.5 and Haiku 4.5 track the remaining window; for others there are task budgets (beta).
- Token counting is free, has its own rate limits, is an estimate and applies no caching.
- On-demand compaction usage appears in `usage.iterations`.

## Gotchas

1. If the summary request's `stop_reason` is not `compaction`, do not replace the history: the function above returns the old one.
2. Summarised messages must not remain before the block: the documentation says 400 `compaction_block_misplaced`.
3. The two kinds have different headers and opposite rules: an on-demand block precedes new turns; with threshold compaction the API itself drops what precedes it.
4. `instructions` do not extend the default prompt, they replace it.
5. A trim that starts on a `tool_result` breaks the request.

> ⚠️ Verify: service responses (the `compaction` block, `applied_edits`, overflow errors), background compaction and keeping recent turns were not exercised against the real service.

## Interview questions

1. What counts towards the window?
<details><summary>Answer</summary>

Everything in the request and the generated output, thinking and cached tokens included.
</details>

2. Compaction vs clearing?
<details><summary>Answer</summary>

The former summarises, the latter removes selected blocks.
</details>

3. Server compaction vs your own summariser?
<details><summary>Answer</summary>

Server-side needs no code and yields a block the API understands; your own gives full control over the text.
</details>

4. Why not store just the reply text?
<details><summary>Answer</summary>

The `compaction` block is lost, and the compacted state with it.
</details>

5. How does shrinking history affect the cache?
<details><summary>Answer</summary>

A changed prefix is cached anew.
</details>

6. What do you do with a large tool result?
<details><summary>Answer</summary>

Filter and paginate in the tool, return a reference, clear old ones.
</details>

7. RAG or long context?
<details><summary>Answer</summary>

Long context is simpler for a small stable corpus; retrieval is cheaper and scales for large changing ones.
</details>

8. How do you keep what matters through compaction?
<details><summary>Answer</summary>

Write it to external memory or files, give a focus, keep recent turns verbatim.
</details>

9. What is `model_context_window_exceeded`?
<details><summary>Answer</summary>

The `stop_reason` when generation hit the window; the answer is incomplete.
</details>

10. Why count tokens for the specific model?
<details><summary>Answer</summary>

Tokenisers differ: old measurements do not carry over.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [05 · Prompt caching](../05-prompt-caching/expert.en.md)

## Sources

- [Context windows](https://platform.claude.com/docs/en/build-with-claude/context-windows)
- [Task budgets](https://platform.claude.com/docs/en/build-with-claude/task-budgets)
- [Memory tool](https://platform.claude.com/docs/en/agents-and-tools/tool-use/memory-tool)
