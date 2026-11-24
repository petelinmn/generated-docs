---
title: "Prompt caching: advanced"
lang: en
track: claude
kind: module
doc: advanced
slug: 05-prompt-caching
part: 1
tags: [claude, prompt-caching, cache-ttl, cache-prewarming, cache-invalidation, cache-diagnostics, prompt-architecture]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Prompt caching: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run against a local mock server and local functions; the real behavior of the service's cache was not checked. The rules come from the documentation page "Prompt caching" (its last part was read in an abridged summary) and the page "Cache diagnostics".

## What breaks the cache: the hierarchy

The prefix order `tools` → `system` → `messages` determines what exactly is reset:

| Change | tools | system | messages |
|---|---|---|---|
| Tool definitions (name, description, parameters) | reset | reset | reset |
| Turning web search or citations on/off | kept | reset | reset |
| `speed` (fast or standard mode) | kept | reset | reset |
| `tool_choice` | kept | kept | reset |
| Adding/removing images anywhere | kept | kept | reset |
| Thinking parameters, `output_config.effort` | depends on the model | depends on the model | reset |

The higher in the prefix a change is, the more is reset. Hence the architecture rule: **the most stable goes first** (tools, then `system`), the variable at the end.

## A stable prefix: a checklist

- There is no **time, dates, UUIDs, counters** at the start: one changed byte is enough, and the prefix is different. In a hash check a string with a changing second gave different fingerprints (`False`), and an unchanged one the same (`True`).
- The order of tools and **JSON keys** is deterministic: in the experiment `json.dumps({"b": 1, "a": 2})` and `json.dumps({"a": 2, "b": 1})` gave different strings (`False`), and with `sort_keys=True` the same. This also concerns keys in tool inputs: in some languages (Swift, Go) the key order is random.
- The set of tools does not change between requests; enable the needed tool through an instruction, not by changing the list.
- Add operator instructions during a conversation with a `{"role": "system"}` message in `messages` on models that support it, not by editing the top-level `system`.
- Do not switch modes (`speed`, thinking, the `output_config.format` schema) without need: a change resets the cache.

## The order and mixing of TTLs

You can use breakpoints with different TTLs, but a **longer TTL must come before a shorter one**: a 1-hour write goes before any 5-minute one.

```python
def valid_ttl_order(blocks):
    seen_5m = False
    for ttl in blocks:
        if ttl == "5m": seen_5m = True
        elif ttl == "1h" and seen_5m: return False
    return True
print(valid_ttl_order(["1h", "5m"]), valid_ttl_order(["5m", "1h"]), valid_ttl_order(["1h", "1h", "5m"]))
```

```text
True False True
```

Billing: a read for the part up to the last hit; a 1-hour write for the tokens between the last hit and the last 1-hour breakpoint; a 5-minute write for the rest. A request that read a 5-minute entry **renews** its 5-minute lifetime even if it sets `"ttl": "1h"`. The lifetime is counted from the **start** of the request.

## Pre-warming the cache: max_tokens: 0

To write the cache in advance (for example before a load peak), send a request with `max_tokens: 0`: the write happens without generating.

```python
r = client.messages.create(model="claude-opus-5-5", max_tokens=0, cache_control={"type": "ephemeral"},
                           system=BIG_SYSTEM, messages=[{"role": "user", "content": "x"}])
print(r.content, r.stop_reason, r.usage.output_tokens, r.usage.cache_creation_input_tokens)
```

```text
[] max_tokens 0 2048
```

The reply has an empty `content`, `stop_reason: "max_tokens"`, zero output tokens (the values were returned by the mock; SDK 2.13.0 accepted `max_tokens=0`). Rules of pre-warming: the breakpoint must be on the **last shared block** with the working requests; the thinking settings and `effort` are the same; a pre-warm that writes is billed as a write. The request is rejected (`invalid_request_error`) with `stream: true`, `thinking.type: "enabled"`, `output_config.format`, a forced `tool_choice` and in Message Batches.

## Parallel requests

A cache entry **becomes available only after the first response starts**. If you start a hundred requests at once, they may all write the cache instead of reading it. The solution: send one request (or a pre-warm), wait for the response to start and only then launch the rest. For batches (Message Batches) hits are "best effort".

## Tools, thinking, documents

- **Tools:** `cache_control` is put on the last tool; everything before it is cached as one prefix.
- **Thinking:** `thinking` blocks cannot be marked with `cache_control` directly; they are cached with the other content in later turns, and when read from the cache they count as input tokens.
- **Documents and images** from user messages can be cached; for citations cache the top-level document.
- **Empty text blocks** cannot be cached.

## The lookback window

The system checks at most 20 positions for each breakpoint (the breakpoint itself is the first); a run of consecutive `tool_use` blocks counts as one position, and so does a run of `tool_result`. In very long conversations place breakpoints so that the last hit stays within the lookback window (automatic mode does this for you).

## Diagnosing misses

The page "Cache diagnostics" describes the `diagnostics` field: in the request — `"diagnostics": {"previous_message_id": null}` for the first turn and the `id` of the previous reply for the following ones; in the response — `diagnostics` with `cache_miss_reason`, whose `type` is `model_changed`, `system_changed`, `tools_changed`, `messages_changed`, `previous_message_not_found`, `unavailable`. This way you can tell **what exactly** changed.

> ⚠️ Verify: the name of the beta header and the availability of diagnostics on each model and platform were not checked in this material; the page states the status `stable`, the examples use `client.beta.messages`.

## Isolation

Organizations never share a cache. On the Claude API, Claude Platform on AWS and Foundry workspace isolation applies; on Amazon Bedrock and Google Cloud (Google managed) — by organization only.

## Best practices

- Stable first, variable last.
- Automatic `cache_control` for conversations; explicit breakpoints for parts of different "temperature".
- Check `usage` on every prompt change.
- Pre-warm the cache before a peak and do not start parallel requests "cold".
- Do not change tools, `speed`, thinking and the schema without need.
- A 1-hour TTL if requests come less often than once in 5 minutes.

## Common mistakes

- ❌ `datetime.now()` in `system` → ✅ the time in the last message.
- ❌ A nondeterministic order of tools/keys → ✅ sorting.
- ❌ A 5-minute breakpoint before a 1-hour one → ✅ put the longer TTL before the shorter.
- ❌ A hundred parallel requests with one new prefix → ✅ a pre-warm and waiting.
- ❌ Changing `speed` or the schema between requests → ✅ fix them.
- ❌ Expecting a short prefix to be cached → ✅ check the model's minimum.

## Related materials

- Next: [expert theory](expert.en.md)
- Structured output and the schema cache: [04 · Structured outputs](../04-structured-output/advanced.en.md)
- Request parameters: [02 · Messages API](../02-messages-api/expert.en.md)

## Sources

- [Prompt caching](https://platform.claude.com/docs/en/build-with-claude/prompt-caching)
- [Cache diagnostics](https://platform.claude.com/docs/en/build-with-claude/cache-diagnostics)
