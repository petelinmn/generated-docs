---
title: "Prompt caching: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 05-prompt-caching
part: 1
tags: [claude, prompt-caching, cache-pricing, cache-minimum-length, cache-lookback, cache-invalidation, cache-isolation]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Prompt caching: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. Prices and minimum lengths come from the documentation page "Prompt caching" at the time of writing; they change when models ship, so check the live page and the price list. The last part of the page (the questions section) was read in a summary.

## Reference

### cache_control syntax

| Form | Where | Notes |
|---|---|---|
| `{"type": "ephemeral"}` | the top level of the request | automatic mode: a point on the last cacheable block; takes one of the 4 slots (in the experiment the SDK sent the field) |
| `{"type": "ephemeral"}` | a `system` block, `messages`, the last tool | an explicit point; a 5-minute TTL |
| `{"type": "ephemeral", "ttl": "1h"}` | the same | a 1-hour TTL (in the experiment the SDK sent the field) |

Restrictions: up to **4** points; a fifth point and an explicit `cache_control` on the last block with a TTL different from the automatic one give a 400; on `bedrock-runtime` a top-level `cache_control` is not supported (400).

### Price multipliers

| Operation | Multiplier of the base input price |
|---|---|
| A 5-minute write | 1.25× |
| A 1-hour write | 2× |
| A read (default) | 0.1× |
| A read: Fable 5.2, Mythos 5.2, Fable 5.1, Mythos 5.1 | 0.025× |
| A read: Opus 5.5, Sonnet 5.5 | 0.05× |

An example from the documentation for Haiku 5.5 (prompts up to 100 thousand tokens), $ per million tokens: input 0.10; a 5-minute write 0.125; a 1-hour write 0.20; a read 0.01; output 0.50. The cache breakpoints themselves cost nothing: you pay for writes, reads and uncached input.

### Minimum prefix length

| Tokens | Models |
|---|---|
| 512 | Opus 5.5, Opus 5, Sonnet 5.5, Haiku 5.5, Fable 5/5.1/5.2, Mythos 5/5.1/5.2 |
| 1024 | Sonnet 5, Opus 4.8, Sonnet 4.6, Sonnet 4.5 (and the retired Opus 4.1, Opus 4, Sonnet 4) |
| 2048 | Mythos Preview, Opus 4.7 |
| 4096 | Opus 4.6, Opus 4.5, Haiku 4.5 |

(For the Claude API, Claude Platform on AWS, Google Cloud (Google managed) and Foundry.) A shorter prefix is processed without a cache and without an error.

### usage fields

| Field | Meaning |
|---|---|
| `input_tokens` | tokens after the last breakpoint |
| `cache_creation_input_tokens` | written |
| `cache_read_input_tokens` | read |
| `cache_creation.ephemeral_5m_input_tokens`, `ephemeral_1h_input_tokens` | the write by TTL |

Total input = `cache_read_input_tokens + cache_creation_input_tokens + input_tokens` (in the experiment `0 + 2048 + 20 = 2068`).

### Pre-warming rules (`max_tokens: 0`)

The reply: `content: []`, `stop_reason: "max_tokens"`, 0 output tokens; the breakpoint is on the last shared block; the same `thinking` and `effort`. Rejected: `stream: true`, `thinking.type: "enabled"`, `output_config.format`, a forced `tool_choice`, use in Message Batches.

## Internals

- **The prefix and the hash.** Before each breakpoint the system hashes the prefix and looks for an entry. An entry is created **only** at breakpoints; on a miss the search goes back through earlier positions within 20 positions (the lookback window) and finds only entries written by earlier requests at their own breakpoints.
- **A byte-for-byte match.** "100 percent identical segments, including all text and images" up to and including the breakpoint are needed.
- **The lifetime** is renewed on every use and counted from the start of the request; mixing TTLs requires the order "longer before shorter".
- **Availability of a write.** An entry is visible to other requests only after the first response starts; so parallel requests started at once do not read from each other.
- **Isolation** is by organization always, by workspace on a number of platforms ([advanced theory](advanced.en.md#isolation)).
- **Thinking.** `thinking` blocks cannot have their own breakpoint; on new models the continuity of blocks between turns keeps the cache, while dropped blocks reset the messages cache.
- **Diagnostics.** The `diagnostics` field compares the request with the previous one and names the cause of a miss.

> ⚠️ Verify: the exact rules of the thinking cache by model, the behavior of a write on errors and the lifetime of diagnostic fingerprints come from the documentation and a summary and were not checked in the experiments.

## Gotchas

### 1. Variable text at the start

`Now: …` in `system` changes the prefix every time: the cache never hits (in the experiment the fingerprints differed).

### 2. Nondeterministic JSON

The order of keys or tools changes the bytes of the prefix (in the experiment the strings differed without `sort_keys`).

### 3. A short prefix

Below the model's minimum no cache is created and there is no error: check `usage`.

### 4. A parallel "cold start"

Requests started at once all write the cache: wait for the first response to start or pre-warm.

### 5. A write costs more than ordinary input

For single requests the cache is not worth it (in the experiment a write $0.0129 against $0.01034 without a cache).

### 6. Changing parameters

Switching `speed`, `tool_choice`, images, thinking or `output_config.format` resets part of the cache.

### 7. TTL order

A short TTL before a long one is not allowed: the long one goes first.

### 8. Renewing a 5-minute entry

Reading a 5-minute entry renews its 5 minutes even with `"ttl": "1h"` in the request.

### 9. Breakpoints on empty blocks

Empty text blocks cannot be cached.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Automatic cache | simple, follows the conversation | less control |
| Explicit breakpoints | precise control, different TTLs | by hand; no more than 4 |
| A 1-hour TTL | fits rare requests | the write is dearer (2×) |
| Pre-warming | a predictable start | a paid write in advance |
| Shortening the prefix | cheaper by itself | loss of context |
| Message Batches | cheaper, background | the cache is "best effort" |

## Interview questions

<details><summary>1. What exactly is cached and in what order?</summary>

The request's prefix up to the block with `cache_control` in the order `tools` → `system` → `messages`; a full match is needed.

→ [Basic theory](basics.en.md#how-a-prefix-works)

</details>

<details><summary>2. How does automatic mode differ from explicit breakpoints?</summary>

Automatic puts a point on the last cacheable block and moves it; explicit ones go on chosen blocks; no more than 4 in all.

→ [Basic theory](basics.en.md#automatic-caching)

</details>

<details><summary>3. Trick: <code>cache_read_input_tokens</code> is always 0. Causes?</summary>

The prefix is shorter than the model's minimum, there is no breakpoint, the prefix changes (time, UUID, key order), or there was no write yet.

→ [Gotcha 1](#1-variable-text-at-the-start)

</details>

<details><summary>4. How much do a write and a read cost?</summary>

A 5-minute write is 1.25×, a 1-hour write 2×, a read 0.1× of the base price (lower on a number of models).

→ [Reference](#price-multipliers)

</details>

<details><summary>5. When does the cache pay off?</summary>

From the second request if the prefix matches: a write 1.25×, a read 0.1× (for a 1-hour write — from the third).

→ [Exercises](exercises.en.md)

</details>

<details><summary>6. How do you pre-warm the cache?</summary>

A request with `max_tokens: 0` and a breakpoint on the last shared block; a reply without generation.

→ [Advanced theory](advanced.en.md#pre-warming-the-cache-max_tokens-0)

</details>

<details><summary>7. Trick: a hundred parallel requests got no hits. Why?</summary>

A write is available after the first response starts; you must wait or pre-warm.

→ [Gotcha 4](#4-a-parallel-cold-start)

</details>

<details><summary>8. What resets the cache?</summary>

A change of tools (everything), `system`/web search/citations/`speed` (system and messages), `tool_choice`/images/thinking (messages).

→ [Advanced theory](advanced.en.md#what-breaks-the-cache-the-hierarchy)

</details>

<details><summary>9. How do you mix TTLs?</summary>

A long TTL before a short one; billing splits into a read, a 1-hour write and a 5-minute write.

→ [Advanced theory](advanced.en.md#the-order-and-mixing-of-ttls)

</details>

<details><summary>10. Where do you put the breakpoint when using tools?</summary>

On the last tool: everything before it is cached as one prefix.

→ [Basic theory](basics.en.md#explicit-breakpoints)

</details>

<details><summary>11. How do you find out why there was a miss?</summary>

The `diagnostics` field (`cache_miss_reason.type`): `system_changed`, `tools_changed`, `messages_changed` and others.

→ [Advanced theory](advanced.en.md#diagnosing-misses)

</details>

<details><summary>12. Trick: no cache is created and there is no error. How do you check?</summary>

Both `cache_*` fields are 0: the prefix is shorter than the model's minimum.

→ [Gotcha 3](#3-a-short-prefix)

</details>

## Cheat sheet

```text
cache_control={"type": "ephemeral"}                    # automatic mode (top level)
{"type": "text", ..., "cache_control": {"type": "ephemeral", "ttl": "1h"}}   # an explicit point, 1 hour
tools: cache_control on the last tool; at most 4 points; a lookback window of 20 positions
usage: input_tokens + cache_creation_input_tokens + cache_read_input_tokens
price: a 5m write 1.25x, 1h 2x, a read ~0.1x; the minimum prefix is 512–4096 tokens
pre-warm: max_tokens=0; parallel requests — after the first response starts
```

- Stable first.
- Check `usage`, do not assume.
- A short prefix is silently not cached.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Request parameters and `usage`: [02 · Messages API](../02-messages-api/expert.en.md)
- Changing the schema and the cache: [04 · Structured outputs](../04-structured-output/expert.en.md)

## Sources

- [Prompt caching](https://platform.claude.com/docs/en/build-with-claude/prompt-caching)
- [Cache diagnostics](https://platform.claude.com/docs/en/build-with-claude/cache-diagnostics)
- [Pricing](https://platform.claude.com/docs/en/about-claude/pricing)
