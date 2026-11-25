---
title: "Prompt caching"
lang: en
track: claude
kind: module
doc: overview
slug: 05-prompt-caching
part: 1
levels: [1, 2, 3, 4]
tags: [claude, prompt-caching, cache-control, cost-optimization, latency, cache-diagnostics]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Prompt caching

🌐 [Русский](overview.ru.md) · **English**

## In short

Prompt caching lets you avoid processing the repeating start of a request again. A **prefix** is cached — everything up to a marked point (`cache_control`), in the order `tools` → `system` → `messages`. Reading from the cache costs a fraction of the ordinary input-token price and speeds up the answer, while a write costs slightly more than ordinary processing. The cache is an exact match of the prefix: any change in it (even one byte) resets everything after it.

## Key ideas

- **A prefix is cached** up to the block with `cache_control`; the order is `tools`, `system`, `messages`.
- **Automatic mode** is a top-level `cache_control`: the point is put on the last cacheable block and moves forward as the conversation grows.
- **Explicit breakpoints** are `cache_control` on blocks; no more than 4 in all (automatic mode takes one).
- **TTL:** 5 minutes by default, 1 hour with `"ttl": "1h"`; the lifetime is renewed on every use.
- **Price:** a 5-minute write is 1.25× the base input price, a 1-hour write 2×, a read usually 0.1× (even cheaper on some models).
- **The minimum prefix length** depends on the model (512, 1024, 2048 or 4096 tokens); a short prefix simply is not cached, without an error.
- **The effect is checked in `usage`:** `cache_creation_input_tokens` and `cache_read_input_tokens`.
- **Stability matters most:** time, random identifiers and key order at the start of a prompt break the cache.
- **The cache becomes available after the first response starts:** parallel requests first "warm" the cache.
- **The cache is isolated by organization** (and on a number of platforms by workspace).

## Minimal example

```python
response = client.messages.create(
    model="claude-opus-5-5",
    max_tokens=1024,
    cache_control={"type": "ephemeral"},   # automatic caching
    system=LONG_REFERENCE_TEXT,
    messages=[{"role": "user", "content": "Summarize"}],
)
u = response.usage
print(u.input_tokens, u.cache_creation_input_tokens, u.cache_read_input_tokens)
```

```text
20 2048 0
```

The output came from a local mock server (see "How the code was checked" in the [basic theory](basics.en.md)): the first request wrote 2048 tokens into the cache; a repeated one with the same prefix would show `cache_read_input_tokens` rather than a write.

## When to use and when not

| Task | Choice |
|---|---|
| A large unchanging `system`, a reference, instructions | cache |
| A long conversation where history grows | automatic `cache_control` |
| Many tools with a shared description | a point on the last tool |
| A single request with no repeats | no cache needed |
| A prefix shorter than the model's minimum | the cache will not work |
| A prefix that changes with every request | make it stable first |

- ❌ Do not put the time, a UUID and counters at the start of a prompt: they reset the cache.
- ❌ Do not expect cache hits for parallel requests until the first one has started answering.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | how a prefix works, automatic and explicit `cache_control`, `usage`, checking the effect, cost calculation |
| [2. Advanced theory](advanced.en.md) | the order of TTLs, tools and thinking, `max_tokens: 0` pre-warming, parallel requests, a stable-prefix architecture, diagnostics |
| [3. Expert theory](expert.en.md) | a reference of prices and minimum lengths, the invalidation table, the lookback window, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn claude 05-prompt-caching level 5` |
| 6. Tests | not created yet — `/learn claude 05-prompt-caching level 6` |

## Related materials

- Prerequisites: [02 · Messages API](../02-messages-api/overview.en.md), [04 · Structured outputs](../04-structured-output/overview.en.md) (changing the schema resets the cache)
- Next: [06 · Documents and citations](../06-documents-and-citations/overview.en.md)
- Analogues in other tracks: [LLM](../../../llm/overview.en.md), [Python](../../../python/overview.en.md)
- [10 · CLAUDE.md and memory](../10-memory/overview.en.md)
