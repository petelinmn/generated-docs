---
title: "Compaction and long context: advanced"
lang: en
track: claude
kind: module
doc: advanced
slug: 13-long-context
part: 3
tags: [claude, context-window, compaction, context-editing, token-counting]
versions: "Claude 5.5 family; anthropic 2.14.0; beta compact-2026-09-04, compact-2026-01-12, context-management-2025-06-27; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Compaction and long context: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Three tools

| Tool | Does | Who decides when |
|---|---|---|
| on-demand compaction | replaces history with a summary | your code |
| threshold compaction | the same inside an ordinary request | API |
| context editing | removes old tool results or thinking | API |

All three are in beta. The documentation recommends on-demand compaction where available.

## On demand

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

```python
history.append({"role": "user", "content": question})
response = client.beta.messages.create(model="claude-opus-5-5", max_tokens=8192,
                                       betas=["compact-2026-09-04"], messages=history)
history.append({"role": "assistant", "content": response.content})   # the whole content, not just text

used = response.usage.input_tokens + response.usage.output_tokens    # the next request sends the reply too
if used > COMPACT_AT_TOKENS:
    history = compact(client, history)
```

- A summary request produces no reply: one `compaction` block and `stop_reason: "compaction"`.
- The block goes first in `messages` in place of the summarised ones, unchanged with its signature; exactly one.
- The beta header is needed on every later request that carries the block.
- Send the same `system` and `tools`; `max_tokens` caps the whole call including thinking.

## At a threshold

```python
response = client.beta.messages.create(
    model="claude-opus-5-5", max_tokens=4096, messages=messages,
    betas=["compact-2026-01-12"],
    context_management={"edits": [{"type": "compact_20260112",
                                   "trigger": {"type": "input_tokens", "value": 150000}}]})
messages.append({"role": "assistant", "content": response.content})
```

The default trigger is 150,000 input tokens, the minimum 50,000. Append the whole `content`: the API drops what preceded the block. `pause_after_compaction` stops after the summary; `instructions` **replaces** the default summarisation prompt.

## Clearing tool results

```python
response = client.beta.messages.create(
    model="claude-opus-5-5", max_tokens=4096, messages=messages, tools=tools,
    betas=["context-management-2025-06-27"],
    context_management={"edits": [{
        "type": "clear_tool_uses_20250919",
        "trigger": {"type": "input_tokens", "value": 30000},
        "keep": {"type": "tool_uses", "value": 3},
        "clear_at_least": {"type": "input_tokens", "value": 5000},
        "exclude_tools": ["memory"]}]})
```

| Parameter | Default |
|---|---|
| `trigger` | 100 000 input tokens |
| `keep` | 3 tool use/result pairs |
| `clear_at_least` | — |
| `exclude_tools` | — |
| `clear_tool_inputs` | `false` |

The edit happens on the server: you keep the full history. Clearing breaks the cached prefix, so `clear_at_least` prevents doing it for little gain. The other strategy is `clear_thinking_20251015`.

## Claude Code

Compaction is automatic; manually `/compact`, optionally saying what to keep: `/compact focus on the auth bug`. In the check, `PreCompact`, `SessionStart` with source `compact` and `PostCompact` fired; the history was replaced by a summary and `CLAUDE.md` was inserted again.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [10 · CLAUDE.md](../10-memory/advanced.en.md) · [11 · Hooks](../11-skills-and-hooks/advanced.en.md)

## Sources

- [Compaction overview](https://platform.claude.com/docs/en/build-with-claude/compaction)
- [Compaction on demand](https://platform.claude.com/docs/en/build-with-claude/compaction-on-demand)
- [Compaction at a token threshold](https://platform.claude.com/docs/en/build-with-claude/compaction-threshold)
- [Context editing](https://platform.claude.com/docs/en/build-with-claude/context-editing)
- [Claude Code: context window](https://code.claude.com/docs/en/context-window)
