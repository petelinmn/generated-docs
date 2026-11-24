---
title: "Prompt caching: basics"
lang: en
track: claude
kind: module
doc: basics
slug: 05-prompt-caching
part: 1
tags: [claude, prompt-caching, cache-control, usage, cost-optimization]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Prompt caching: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Messages API](../02-messages-api/overview.en.md) · **Time:** ~1 h

## How the code was checked

There was no API key during preparation, so the code was run against a **local mock server** (as in [module 02](../02-messages-api/basics.en.md)): it accepts requests from the `anthropic` 2.13.0 SDK, saves the body and returns a predefined `usage` field (with a cache write or read). What was checked are the request shapes, parsing of `usage` and the calculations; **real caching on the service side, hits and misses on real models were not checked**. The rules and prices come from the documentation page "Prompt caching".

## Why it matters

If every request starts with the same large text (instructions, a reference, a document, a description of tools), it can be processed once and then read cheaply from the cache. The gain is in the cost and latency of repeated requests.

## How a prefix works

A request's **prefix** is cached: everything up to and including the block with `cache_control`, in the order `tools` → `system` → `messages`. A hit is possible only with a **full match** of the prefix (all text and images). This means:

- the cache helps when the **shared part is at the start** and the changing part at the end;
- any change inside the prefix invalidates everything after it;
- caching does not change the result of the answer, only the cost and speed.

## Automatic caching

The simplest way is `cache_control` at the top level of the request: the point is put on the last cacheable block and moves forward by itself as the conversation grows.

```python
r = client.messages.create(
    model="claude-opus-5-5", max_tokens=1024,
    cache_control={"type": "ephemeral"},
    system=BIG,  # a long unchanging text
    messages=[{"role": "user", "content": "Summarize"}],
)
u = r.usage
print(u.input_tokens, u.cache_creation_input_tokens, u.cache_read_input_tokens, u.cache_creation.ephemeral_5m_input_tokens, u.cache_creation.ephemeral_1h_input_tokens)
```

```text
20 2048 0 2048 0
```

In the experiment the SDK sent `"cache_control": {"type": "ephemeral"}` in the request body, and the mock returned a write of 2048 tokens (`cache_creation_input_tokens`), the 5-minute part (`ephemeral_5m_input_tokens`) and 20 uncached tokens in `input_tokens`.

## Explicit breakpoints

For precise control `cache_control` is put on specific blocks. The default TTL is 5 minutes; for an hour add `"ttl": "1h"`:

```python
r = client.messages.create(
    model="claude-opus-5-5", max_tokens=1024,
    system=[{"type": "text", "text": "You are an expert.", "cache_control": {"type": "ephemeral", "ttl": "1h"}}],
    messages=[{"role": "user", "content": "Q"}],
)
print(r.usage.cache_read_input_tokens)
```

```text
2048
```

(In the experiment the mock returned a read of 2048 tokens; the request body contained `{"type": "ephemeral", "ttl": "1h"}` in the `system` block.) For tools `cache_control` is put on the **last** tool: all tools before it and it itself are cached as one prefix (in the experiment the request had `cache_control` only on the second tool). The maximum is **4 breakpoints** per request; automatic mode takes one.

## Reading usage

The effect is visible in `usage`:

| Field | Meaning |
|---|---|
| `input_tokens` | tokens **after** the last breakpoint: not cached |
| `cache_creation_input_tokens` | written to the cache |
| `cache_read_input_tokens` | read from the cache |
| `cache_creation.ephemeral_5m_input_tokens` / `ephemeral_1h_input_tokens` | the write split by TTL (the sum equals `cache_creation_input_tokens`) |

The total input size: `cache_read_input_tokens + cache_creation_input_tokens + input_tokens` (in the experiment `0 + 2048 + 20 = 2068`). If `cache_creation_input_tokens` and `cache_read_input_tokens` are both `0`, the prefix was not cached: it is shorter than the model's minimum or no breakpoint was set.

## What it costs

Relative to the base price of input tokens:

| Operation | Multiplier |
|---|---|
| A 5-minute write | 1.25× |
| A 1-hour write | 2× |
| A cache read | 0.1× (lower on a number of models; see the [expert theory](expert.en.md#reference)) |
| Ordinary input with no cache | 1× |

A calculation at a conventional base price of $5 per million tokens (a number for the example, not the price of a specific model):

```python
def cost(usage, base=5.0, mult_write5=1.25, mult_write1h=2.0, mult_read=0.1):
    per = base / 1_000_000
    cc = usage.cache_creation
    w5 = cc.ephemeral_5m_input_tokens if cc else usage.cache_creation_input_tokens
    w1 = cc.ephemeral_1h_input_tokens if cc else 0
    return per * (usage.input_tokens + w5 * mult_write5 + w1 * mult_write1h + usage.cache_read_input_tokens * mult_read)
```

```python
w, h = usage_of_write_request, usage_of_read_request   # usage from the experiment on the mock
nocache = 5.0 / 1_000_000 * (20 + 2048)
print(round(cost(w), 6), round(cost(h), 6), round(nocache, 6), round(cost(h) / nocache, 3))
```

```text
0.0129 0.001124 0.01034 0.109
```

The numbers are the cost of the write, the cost of the read, the cost without a cache and the ratio "read / without cache". For 2048 cacheable tokens and 20 ordinary ones, the first send (the write) is about a quarter more expensive than a request without a cache, but every read costs about 11% of the ordinary price. The cache pays off from the second request if the prefix matches. The calculation shows orders of magnitude: take real prices from the price list.

## Beginner mistakes

- ❌ Caching a prefix shorter than the model's minimum → ✅ check the minimum length and `usage`.
- ❌ Putting changing text (time, UUID) at the start → ✅ stable at the start, variable at the end.
- ❌ Changing tools or `system` between requests → ✅ keep them unchanged.
- ❌ Expecting savings from the first request → ✅ the write is dearer; the gain is on repeats.
- ❌ Not looking at `usage` → ✅ check `cache_read_input_tokens`.
- ❌ More than 4 breakpoints → ✅ 400.

## Related materials

- Next: [advanced theory](advanced.en.md) — TTL, pre-warming, parallel requests, prefix architecture
- Earlier: [02 · Messages API](../02-messages-api/basics.en.md)
- Parallels: [LLM](../../../llm/overview.en.md)

## Sources

- [Prompt caching](https://platform.claude.com/docs/en/build-with-claude/prompt-caching)
- [Cache diagnostics](https://platform.claude.com/docs/en/build-with-claude/cache-diagnostics)
