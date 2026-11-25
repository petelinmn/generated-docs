---
title: "Deployment and production: quiz"
lang: en
track: claude
kind: module
doc: quiz
slug: 17-production
part: 3
tags: [claude, deployment, production, retries, bedrock, vertex-ai, foundry]
versions: "Claude 5.5 family; anthropic 2.14.0; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Deployment and production: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is Opus 5.5 called in Claude in Amazon Bedrock?

- A) `bedrock/claude-opus-5-5`
- B) `claude-opus-5-5@aws`
- C) `anthropic.claude-opus-5-5`
- D) `claude-opus-5-5`

<details><summary>Answer</summary>

**C.** Bedrock IDs carry the provider prefix. The bare name is for the Claude API and Claude Platform on AWS; the other two forms are not used.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 How many attempts does the SDK make by default on a 529?

- A) three
- B) one
- C) two
- D) until it succeeds

<details><summary>Answer</summary>

**A.** `max_retries=2` means the first request plus two retries. There is no endless retry.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 What goes in `model` on Microsoft Foundry?

- A) the resource URL
- B) the Azure subscription ID
- C) the region
- D) the deployment name

<details><summary>Answer</summary>

**D.** The request addresses your deployment, whose default name equals the model ID. The resource is set on the client.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 Which one is operated by AWS rather than Anthropic?

- A) Foundry
- B) Claude in Amazon Bedrock
- C) Claude Platform on AWS
- D) the Claude API

<details><summary>Answer</summary>

**B.** Claude Platform on AWS is billed through AWS but operated by Anthropic, a common mix-up.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 An overload hits after streaming started. What does the SDK do?

- A) raise `APITimeoutError`
- B) raise `APIStatusError` without retrying
- C) retry and stitch the text
- D) return a partial message silently

<details><summary>Answer</summary>

**B.** The 200 is already in, so normal retries do not apply. Nothing is stitched and it is not a timeout.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 Which features are absent from Bedrock and Vertex AI?

- [ ] A) the Files API
- [ ] B) tool use
- [ ] C) prompt caching
- [ ] D) Message Batches

<details><summary>Answer</summary>

**A, D.** Batches and Files are unsupported there; tool use and caching work everywhere.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What is true of a tier spend-cap 429?

- [ ] A) retrying will not help
- [ ] B) the SDK still retries
- [ ] C) it is a 402
- [ ] D) it clears after `retry-after`

<details><summary>Answer</summary>

**A, B.** Access returns next month or with a higher limit. There is no `retry-after`, yet the SDK treats any 429 as retryable.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 The Bedrock regional premium over global?

- A) 50%
- B) 100%
- C) 10%
- D) 0%

<details><summary>Answer</summary>

**C.** Regional is 10% more; global has no premium. 50% is the Batch API discount.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 What does `hasattr(client.messages, "batches")` give on `AnthropicVertex`?

- A) `False`
- B) `None`
- C) an exception
- D) `True`

<details><summary>Answer</summary>

**D.** The attribute exists although the platform has no batches: the SDK surface is not a capability check.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 Where is the model in a Vertex AI HTTP request?

- A) in a query parameter
- B) in the URL path
- C) in the body field `model`
- D) in an `x-model` header

<details><summary>Answer</summary>

**B.** The path ends in `MODEL:rawPredict` and the body has no `model`.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 How do you keep Claude Code models consistent across a team's platforms?

- A) set `CLOUD_ML_REGION`
- B) disable prompt caching
- C) pin `ANTHROPIC_DEFAULT_*_MODEL`
- D) nothing: defaults are identical

<details><summary>Answer</summary>

**C.** Unpinned, Foundry requested Sonnet 5.5 while mantle and Platform on AWS requested Opus 5.5.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: an EU service on Claude must survive one platform failing. What do you design?

<details><summary>Answer</summary>

Two routes that both satisfy data requirements; an eval on each; fail over only on connection errors, 429 and 5xx; a timeout below the SLA; capped retries across layers; mid-stream handling; a separate path for refusals; per-route metrics and alerts; plan for a cold cache and quotas on the backup.
→ [Advanced theory](advanced.en.md)

</details>
