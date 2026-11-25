---
title: "Compaction and long context"
lang: en
track: claude
kind: module
doc: overview
slug: 13-long-context
part: 3
tags: [claude, context-window, compaction, context-editing, token-counting]
versions: "Claude 5.5 family; anthropic 2.14.0; beta compact-2026-09-04, compact-2026-01-12, context-management-2025-06-27; Claude Code 2.1.332"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Compaction and long context

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

What to do when a conversation or an agent task outgrows the **context window**: counting tokens, trimming, **compacting** and clearing.

## Key ideas

- The window covers input and output.
- A large context costs money and quality.
- Compaction replaces history with a summary.
- Clearing removes old tool results.
- These are beta features with their own headers.

## Minimal example

```python
# compact.py: on-demand compaction (beta)
def compact(client, history, model="claude-opus-5-5", system=None):
    """Return a new history that starts with the compaction block, or the old one if none came back."""
    extra = {"system": system} if system else {}
    summary = client.beta.messages.create(
        model=model, max_tokens=8192, messages=history,
        betas=["compact-2026-09-04"], compaction={"type": "summarize"}, **extra)
    if summary.stop_reason != "compaction":
        return history
    return [{"role": "assistant", "content": summary.content}]
```

## When to use it and when not

Long dialogues and agent tasks. A short request needs none of it.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude 13-long-context level 5` |
| 6. Tests | — | `/learn claude 13-long-context level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [05 · Prompt caching](../05-prompt-caching/overview.en.md)
- [10 · CLAUDE.md and memory](../10-memory/overview.en.md)
- [12 · Agents](../12-agents/overview.en.md)
