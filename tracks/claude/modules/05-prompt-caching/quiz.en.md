---
title: "Quiz: prompt caching"
lang: en
track: claude
kind: module
doc: quiz
slug: 05-prompt-caching
part: 1
tags: [claude, prompt-caching, cache-control, cache-ttl, cost-optimization]
status: draft
updated: 2026-11-24
---

# Quiz: prompt caching

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is cached?

- A) Only the model's reply
- B) The request prefix up to the block with `cache_control` in the order `tools`, `system`, `messages`
- C) Only `system`
- D) The whole request regardless of changes

<details><summary>Answer</summary>

**B.** A full match of the prefix is needed.
→ [Basic theory](basics.en.md#how-a-prefix-works)

</details>

## 2. 🟢 How many explicit `cache_control` points does a request allow?

- A) 1
- B) 4
- C) 10
- D) Unlimited

<details><summary>Answer</summary>

**B.** Automatic mode takes one of the four.
→ [Basic theory](basics.en.md#explicit-breakpoints)

</details>

## 3. 🟢 What is the default TTL and how do you set an hour?

- A) 1 hour; `"ttl": "5m"`
- B) 5 minutes; `"ttl": "1h"`
- C) 24 hours; `"ttl": "1d"`
- D) No TTL

<details><summary>Answer</summary>

**B.** The lifetime is renewed on every use.
→ [Basic theory](basics.en.md#explicit-breakpoints)

</details>

## 4. 🟢 Which field shows a read from the cache?

- A) `usage.cache_read_input_tokens`
- B) `usage.output_tokens`
- C) `stop_reason`
- D) `model`

<details><summary>Answer</summary>

**A.** A write is `cache_creation_input_tokens`; uncached input is `input_tokens`.
→ [Basic theory](basics.en.md#reading-usage)

</details>

## 5. 🟡 What happens if the prefix is shorter than the model's minimum?

- A) A 400 error
- B) No cache is created, no error; both `cache_*` fields are 0
- C) It is cached partially
- D) The request is rejected

<details><summary>Answer</summary>

**B.** The minimum depends on the model (512–4096 tokens).
→ [Expert](expert.en.md#minimum-prefix-length)

</details>

## 6. 🟡 What should you do before launching a hundred parallel requests with a new prefix?

- A) Nothing
- B) Send one request (or a `max_tokens: 0` pre-warm) and wait for the response to start
- C) Increase `max_tokens`
- D) Set a 1-hour TTL

<details><summary>Answer</summary>

**B.** A write is available only after the first response starts.
→ [Advanced theory](advanced.en.md#parallel-requests)

</details>

## 7. 🟡 How should breakpoints with different TTLs be ordered?

- A) A short TTL before a long one
- B) A long TTL before a short one
- C) The order does not matter
- D) The same TTL is required

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#the-order-and-mixing-of-ttls)

</details>

## 8. 🟡 What resets both the `system` cache and the `messages` cache but not the tools cache?

- A) Changing a tool description
- B) Turning citations on or off
- C) Adding an image to a message
- D) Changing `tool_choice`

<details><summary>Answer</summary>

**B.** Changing tools resets everything, `tool_choice` and images — only `messages`.
→ [Advanced theory](advanced.en.md#what-breaks-the-cache-the-hierarchy)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) A 5-minute cache write costs 1.25× the base input price
- [ ] B) A read usually costs 0.1× the base price
- [ ] C) The cache breakpoints themselves are paid
- [ ] D) Caching does not change the result of the answer

<details><summary>Answer</summary>

**A, B, D.** C is wrong: you pay for writes, reads and uncached input.
→ [Expert](expert.en.md#price-multipliers)

</details>

## 10. 🔴 Find the mistake

> "I will put the current time at the start of `system`: that way the model knows the date, and the cache will work anyway."

<details><summary>Answer</summary>

Any change in the prefix resets the cache: the time at the start changes on every request. Variable content must go after the last breakpoint (for example into the last message).
→ [Gotcha 1](expert.en.md#1-variable-text-at-the-start)

</details>

## 11. 🔴 What is the total input size?

- A) `input_tokens`
- B) `cache_read_input_tokens + cache_creation_input_tokens + input_tokens`
- C) `cache_creation_input_tokens − cache_read_input_tokens`
- D) `output_tokens + input_tokens`

<details><summary>Answer</summary>

**B.** In the experiment `0 + 2048 + 20 = 2068`.
→ [Expert](expert.en.md#usage-fields)

</details>

## 12. 🔴 How do you pre-warm the cache without generating a reply?

- A) `max_tokens: 1` and ignore the reply
- B) `max_tokens: 0` with a breakpoint on the last shared block
- C) `stream: true`
- D) It is impossible

<details><summary>Answer</summary>

**B.** The reply is an empty `content` and `stop_reason: "max_tokens"`; streaming and `output_config.format` are not allowed.
→ [Advanced theory](advanced.en.md#pre-warming-the-cache-max_tokens-0)

</details>

## 13. 🔴 Open question

A chatbot with a large reference and tools is queried rarely (once in 10 minutes). How do you set up caching and check that it works?

<details><summary>Key points of a good answer</summary>

- A stable prefix: tools, then `system` with the reference; the variable at the end; a deterministic order of tools and keys.
- A 1-hour TTL (requests rarer than every 5 minutes), a breakpoint on the reference; automatic `cache_control` for the history (mind the TTL order).
- Check `usage` (`cache_read_input_tokens`), the minimum prefix length, `diagnostics` on misses.
- Pre-warm before a peak; do not change the schema, `speed` and thinking without need.

</details>

→ [Advanced theory](advanced.en.md)
