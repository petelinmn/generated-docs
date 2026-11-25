---
title: "Batch, evals and reliability: quiz"
lang: en
track: claude
kind: module
doc: quiz
slug: 14-evals
part: 3
tags: [claude, evals, batch-api, retries, reliability]
versions: "Claude 5.5 family; anthropic 2.14.0; Message Batches API"
status: draft
updated: 2026-11-25
---

# Batch, evals and reliability: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is an eval?

- A) Python's `eval`
- B) a price tier
- C) examples, a grading method and a threshold
- D) a request log

<details><summary>Answer</summary>

**C.** An eval is a repeatable quality check: inputs, a grading rule and a pass bar. A request log does not say whether an answer is good, and Python's `eval` or a price plan are unrelated.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 The cheapest way to check an exact label?

- A) with code
- B) with a model
- C) with a human
- D) impossible

<details><summary>Answer</summary>

**A.** A code comparison is fast, free and deterministic. Models and people are for judgements code cannot express.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 A good success criterion:

- A) "answers well"
- B) "fast"
- C) the prompt author's opinion
- D) a measurable threshold on a held-out set

<details><summary>Answer</summary>

**D.** A criterion must be measurable and checked on data the prompt was not tuned on. "Good", "fast" and the author's opinion give no clear verdict.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 Why do failed cases matter more than the average?

- A) it is cheaper
- B) they show what to fix
- C) the average cannot be computed
- D) there are fewer

<details><summary>Answer</summary>

**B.** The average shows the level; individual failures show the cause, an input class or a flawed instruction. The average is computable, and cost is not the point.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 What does the SDK retry by default?

- [ ] A) 401
- [ ] B) 429
- [ ] C) 529
- [ ] D) 400

<details><summary>Answer</summary>

**B, C.** 429 and 529 are transient, so the SDK retries them with backoff. 400 and 401 are request and key errors that would only repeat.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 How many attempts with `max_retries=2`?

- A) 2
- B) 1
- C) unlimited
- D) up to 3

<details><summary>Answer</summary>

**D.** `max_retries` counts retries beyond the first attempt: 1 + 2 = 3. They are not unlimited.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What does a model refusal look like?

- A) 200 with `stop_reason: "refusal"`
- B) 403
- C) a `RefusalError` exception
- D) 429

<details><summary>Answer</summary>

**A.** A refusal is a successful HTTP reply, so you must check `stop_reason`. It raises no exception and is not a 403 or 429.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What is true of Message Batches?

- [ ] A) they support `stream`
- [ ] B) an answer in seconds is guaranteed
- [ ] C) half price
- [ ] D) results in any order

<details><summary>Answer</summary>

**C, D.** Batches cost half and results come in any order, so match them by `custom_id`. Streaming is unsupported and no quick turnaround is promised.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 What does NOT count towards ITPM?

- A) `input_tokens`
- B) `cache_creation_input_tokens`
- C) everything counts
- D) `cache_read_input_tokens`

<details><summary>Answer</summary>

**D.** Cache reads do not consume the input-token limit by default. Regular input and cache writes do.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 The `overloaded_error` status:

- A) 502
- B) 529
- C) 503
- D) 429

<details><summary>Answer</summary>

**B.** `overloaded_error` is a 529. 429 means your own limit, and 502 and 503 are not tied to this type.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 A 429 without `retry-after` from the tier spend cap:

- A) it is a 400
- B) the SDK fixes it
- C) retrying will not help
- D) it passes in a second

<details><summary>Answer</summary>

**C.** Access returns next month or after raising the limit. It is a 429, not a 400; the SDK retries in vain, and a second changes nothing.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: you must move to a new model. How do you show nothing got worse?

<details><summary>Answer</summary>

A fixed set with criteria; run both models (batch is fine); compare per case and category; several runs; latency, cost, refusals, truncation; inspect differences; gradual rollout with monitoring and rollback.
→ [Advanced theory](advanced.en.md)

</details>
