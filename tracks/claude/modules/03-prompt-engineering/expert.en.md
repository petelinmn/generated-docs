---
title: "Prompt engineering: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 03-prompt-engineering
part: 1
tags: [claude, prompt-engineering, instruction-following, xml-tags, long-context, prompt-injection, prompt-evaluation]
versions: "Claude 5.5 family; recommendations of the Prompting best practices page as of 2026-11"
status: draft
updated: 2026-11-24
---

# Prompt engineering: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. The reference data comes from the Claude Platform documentation; numerical effects (for example "up to 30%") are given as claimed by Anthropic, not measured here.

## Reference

### Techniques and rules

| Technique | Rule | When it helps | Risk |
|---|---|---|---|
| Clarity | task, audience, format, constraints; numbered steps if order matters | almost always | an overlong prompt hides the main point |
| Motivation | explain the reason for a rule | rules that must generalize | extra text |
| A role | one sentence in `system` | tone, expertise | a role "out of thin air" without a task gives nothing |
| XML tags | constant descriptive names, nesting | a mix of instructions and data | unescaped closing tags |
| Examples | 3–5 relevant, diverse, in `<example>` | format, style, classification | answers skewed toward a single example |
| Positive phrasing | "what to do", not "what not to do" | format control | prohibitions backfire |
| Long context | documents on top, the question at the end, `<quotes>` before the answer | inputs from ~20 thousand tokens | a question at the start is worse |
| Chains | separate calls per step | verifiable multi-step tasks | cost and latency |
| Thinking | `thinking` + `effort`, general instructions | complex and agentic tasks | "write out your reasoning" instructions |
| Structured output | `output_config.format` instead of requests in text | strict JSON | — |

### Request components and where to put what

| Place | What to put |
|---|---|
| `system` | the role, permanent rules, general context, output format |
| The start of a `user` message | long documents and data |
| The end of a `user` message | the specific question or task |
| `<examples>` tags | input and output examples |
| Later messages | reminders, clarifications during the conversation |

### Model specifics (from the guides)

| Model | Specific |
|---|---|
| Opus 5.5 | prompts for Opus 5 usually work; default effort `medium`; thinking cannot be turned off; remove requests to write out reasoning (a `reasoning_extraction` refusal risk) |
| Sonnet 5.5 | effort `high` for the general case; possible premature "giving up" of a turn and extra tests/documentation; `thinking: between_tools` |
| Fable 5.2 | start at `high`; the word "never" is read as an absolute; delegation and "don't think" instructions need care |
| All 4.6+ models | prefill of the last assistant message gives a 400 |

## Internals

- **The model sees a sequence of tokens.** XML tags, roles and numbering are just text: there is no special markup parsing on the API side (apart from the structure of `system`/`messages`). Tags work because the trained model recognizes such patterns.
- **Instruction following.** New models carry out instructions literally and are less verbose; so what is "implied" must be written explicitly, and emphasis from old prompts removed.
- **Long context.** Quality depends on where the information sits: documents at the start and the question at the end is the documentation's recommendation; asking to quote fragments first (`<quotes>`) makes the model find the relevant before reasoning.
- **`system` versus `user`.** The contents of `system` are operator instructions and part of the cacheable prefix ([module 05](../../overview.en.md) of the roadmap); data from untrusted sources should be kept in `user` inside tags and not mixed with instructions.
- **Determinism.** The sampling parameters (`temperature`, `top_p`, `top_k`) were removed in API version `2026-10-21` ([module 02](../02-messages-api/advanced.en.md#parameters-removed-in-api-version-2026-10-21)): repeatability is achieved by prompt structure, output format and checking the result, not by a "zero temperature".
- **Emphasis and overcorrection.** The louder a rule, the harder the model applies it, including where it does not fit; that is the reason for the "fewer instructions" advice.

> ⚠️ Verify: the statements about the internal mechanism (exactly how the model reacts to tags and order) are general understanding and documentation data, not measurement results; the "up to 30%" effect of the question's position was obtained by Anthropic in its tests and was not reproduced here.

## Gotchas

### 1. The prompt "learned" one example

If all examples are of one class, answers shift toward it: choose diverse ones.

### 2. A prohibition backfires

"Do not use markdown" is often worse than a description of the desired format.

### 3. Tags are broken by data

User text with a closing tag breaks the structure: escape it (in the experiment `</` → `<\/`).

### 4. Parsing without a check

`tag(text, "answer")` returned `None` when there was no tag: a branch for absence is needed.

### 5. Emphasis carries over to a new model

"CRITICAL / MUST / NEVER" from old prompts cause skews; remove it.

### 6. Prefill

The last assistant message on 4.6+ models gives a 400.

### 7. The question at the start of a long input

The recommendation is documents on top, the question at the end.

### 8. Several changes at once

You cannot tell what exactly helped; change one at a time and compare on one set.

### 9. A prompt without a test set

Fixing one case breaks another: you need reference inputs.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| A long detailed prompt | controllability | cost, risk of contradictions |
| A short prompt + examples | compact, conveys the format | depends on the choice of examples |
| A chain of calls | verifiability | latency and cost |
| Structured outputs | a guaranteed schema | restrictions on the schema (module 04 of the roadmap) |
| Fine-tuning | stable behavior | not covered in this module; its availability for Claude models was not checked here |
| Prompt libraries and templating | reuse | a dependency; not run |

## Interview questions

<details><summary>1. How do you check that a prompt is clear enough?</summary>

Show it to a colleague with no context: if they are confused, so is the model.

→ [Basic theory](basics.en.md#clear-and-direct-instructions)

</details>

<details><summary>2. What are XML tags for and what do they not do?</summary>

They separate instructions, data and examples; but it is a convention, the API does not parse them, and you parse the reply.

→ [Basic theory](basics.en.md#xml-tags)

</details>

<details><summary>3. Trick: why is "do not use markdown" worse?</summary>

A negative phrasing does not say what is wanted; "write in flowing paragraphs" sets the target format.

→ [Gotcha 2](#2-a-prohibition-backfires)

</details>

<details><summary>4. How many examples are needed and what should they be like?</summary>

3–5, relevant and diverse, in `<example>` tags.

→ [Basic theory](basics.en.md#examples-multishot)

</details>

<details><summary>5. How do you build a prompt with long documents?</summary>

Documents on top (in `<document index>`), the question at the end, a request to quote fragments first.

→ [Basic theory](basics.en.md#long-documents)

</details>

<details><summary>6. What do you do about prefill on new models?</summary>

It returns a 400: use structured outputs, an instruction in `system` or a move into the user message.

→ [Advanced theory](advanced.en.md#prefill-no-longer-works)

</details>

<details><summary>7. Trick: a prompt that worked beautifully on an old model gives skews on a new one. Why?</summary>

Emphasis and excess rules are taken literally and applied too widely; simplify.

→ [Gotcha 5](#5-emphasis-carries-over-to-a-new-model)

</details>

<details><summary>8. How do you defend against instructions inside foreign text?</summary>

Tags, escaping, a random tag identifier, marking "this is data", minimal rights for tools; there are no guarantees.

→ [Advanced theory](advanced.en.md#untrusted-text-and-injection)

</details>

<details><summary>9. When do you split a task into a chain?</summary>

When different instructions, a check of an intermediate result or different models for steps are needed.

→ [Advanced theory](advanced.en.md#prompt-chains)

</details>

<details><summary>10. What do the <code>thinking</code>/<code>effort</code> parameters control and what does the prompt control?</summary>

Parameters — the depth of reasoning and token spend; the prompt — the task, format and constraints.

→ [Advanced theory](advanced.en.md#thinking-and-effort)

</details>

<details><summary>11. Trick: the reply does not contain the expected tag. What does your code do?</summary>

It must handle `None` (a retry, a fallback parse, an error), not crash.

→ [Gotcha 4](#4-parsing-without-a-check)

</details>

<details><summary>12. How do you improve a prompt systematically?</summary>

A set of inputs, a fixed evaluation, one change at a time, comparison on the same set.

→ [Advanced theory](advanced.en.md#iteration-and-evaluation)

</details>

## Cheat sheet

```text
system: a role + permanent rules   |   user: <documents>…</documents> then the question
<examples><example>…</example></examples>  (3–5, diverse)
Write "what to do" (the format), not "what not to do"
The answer in a tag → re.search(r"<answer>(.*?)</answer>", text, re.S); check None
Long input: documents on top, the question below, <quotes> before the answer
Thinking: thinking adaptive + effort; do not ask to "write out your reasoning"
New models: fewer instructions, no CRITICAL/NEVER; prefill → 400
```

- Explain the reason for rules.
- Data from foreign sources — in tags and as data.
- One change — one check.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Request parameters: [02 · Messages API](../02-messages-api/expert.en.md)
- Model selection: [01 · The Claude family and model selection](../01-models/expert.en.md)

## Sources

- [Prompting best practices](https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices)
- [Prompt engineering overview](https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/overview)
- [Prompting Claude Opus 5.5](https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5)
- [Prompting Claude Fable 5.2](https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-fable-5-2)
