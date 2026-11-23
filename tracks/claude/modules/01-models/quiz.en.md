---
title: "Quiz: The Claude family and model selection"
lang: en
track: claude
kind: module
doc: quiz
slug: 01-models
part: 1
tags: [claude, model-selection, effort, context-window]
status: verified
updated: 2026-11-23
---

# Quiz: The Claude family and model selection

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the question level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

**Levels:** [Overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · **4. Quiz** · [5. Runnable examples](code/examples/) · [6. Tests](code/tests/)

## 1. 🟢 Which model is the starting point for low-latency and high-volume workloads?

- A) `claude-fable-5-2`
- B) `claude-opus-5-5`
- C) `claude-sonnet-5-5`
- D) `claude-haiku-5-5`

<details><summary>Answer</summary>

**D.** Haiku 5.5 is optimized for low latency and cost efficiency. The others are not the starting profiles for this exact scenario. See [basics](basics.en.md).

</details>

## 2. 🟢 Which parameter controls effort in the Messages API?

- A) `output_config.effort`
- B) `thinking.budget_tokens`
- C) `messages.effort`
- D) `context_effort`

<details><summary>Answer</summary>

**A.** Effort is set with `output_config.effort`. Extended-thinking budget uses separate legacy thinking controls. See [basics](basics.en.md).

</details>

## 3. 🟢 What enters the context window?

- A) Only the original user prompt
- B) Only previous answers
- C) System prompt, messages, tools and generated output
- D) Only training data

<details><summary>Answer</summary>

**C.** The context window includes the system prompt, complete history, tools, tool results and request documents/images, plus turn output. See [basics](basics.en.md).

</details>

## 4. 🟡 Which approach is best for a high share of simple tasks and rare escalation?

- A) Always Fable
- B) A low-cost executor with escalation to a stronger advisor model
- C) One Opus for every task
- D) Increase only `max_tokens`

<details><summary>Answer</summary>

**B.** A multimodel pattern saves routine tokens and reserves difficult decisions for a stronger model. The alternatives do not relate cost to workload distribution. See [advanced theory](advanced.en.md).

</details>

## 5. 🟢 What does this code print?

```python
models = {
    "high_volume": "claude-haiku-5-5",
    "demanding": "claude-fable-5-2",
}
print(models.get("unknown", "claude-opus-5-5"))
```

<details><summary>Answer</summary>

```text
claude-opus-5-5
```

`dict.get` returns the third fallback argument when the key is missing.
See [basics](basics.en.md).

</details>

## 6. 🟡 Select all correct statements

- [ ] A) Effort can be tested within one model
- [ ] B) 1M context guarantees perfect retrieval
- [ ] C) Evals should pin prompts and datasets
- [ ] D) Partner-platform lifecycle always matches the Claude API

<details><summary>Answer</summary>

**A, C.** Effort is a tunable parameter within one model, and evals require a reproducible set. 1M context does not guarantee perfect retrieval, and partner lifecycle may differ. See [advanced theory](advanced.en.md).

</details>

## 7. 🟢 Which API parameter sets the absolute maximum generated output?

- A) `effort`
- B) `max_tokens`
- C) `context_window`
- D) `cache_control`

<details><summary>Answer</summary>

**B.** `max_tokens` limits generated output; it does not set the complete context-window size. See [basics](basics.en.md).

</details>

## 8. 🟡 Find the bug

```python
config = {
    "provider": "claude-api",
    "model": "claude-opus-5-5",
}
print(config["deployment"])
```

<details><summary>Answer</summary>

The key `deployment` is missing. The Claude API needs `model`, while a deployment name is needed only on a partner platform with deployment configuration. See [basics](basics.en.md).

</details>

## 9. 🟡 What should you test first after changing models?

- A) Only the happy path
- B) Real prompts, edge cases and the output contract
- C) Only price
- D) Only latency

<details><summary>Answer</summary>

**B.** A model change affects accuracy, tool strategy and stop behavior; all of these need production-like data. See [advanced theory](advanced.en.md).

</details>

## 10. 🔴 How should a dateless model ID in the 4.6+ generation be interpreted?

- A) Always an alias
- B) Always a Bedrock profile
- C) It can be an own pinned snapshot
- D) Always a deprecated model

<details><summary>Answer</summary>

**C.** A dateless ID from the 4.6+ generation is its own pinned snapshot and is repeated in the alias row. Earlier generations can have different alias semantics. See [expert theory](expert.en.md).

</details>

## 11. 🟡 Find the problem in this migration plan

```text
Switch claude-sonnet-5-5 to claude-opus-5-5,
leave tools and system unchanged,
send all traffic immediately.
```

<details><summary>Answer</summary>

The plan ignores breaking changes and feature differences. Run a test matrix, verify tools/system/earlier messages, thinking and partner deployment, and use staged rollout. See [advanced theory](advanced.en.md).

</details>

## 12. 🔴 Open question

How would you choose a model for a service handling 10,000 classifications per hour with automatic escalation for low confidence?

<details><summary>Key points of a good answer</summary>

- Start with Haiku or another low-cost balanced option.
- Use the same eval set and fixed output contract.
- Measure accuracy, latency, cost and escalation share.
- Escalate uncertain or complex cases to Sonnet/Opus/Fable.
- Tune thresholds and perform error-cost analysis.
- Verify prompt caching and partner deployment for production.
See [expert theory](expert.en.md).

</details>
