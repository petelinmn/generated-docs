---
title: "Prompt engineering: advanced"
lang: en
track: claude
kind: module
doc: advanced
slug: 03-prompt-engineering
part: 1
tags: [claude, prompt-chaining, thinking, effort, prefill, untrusted-input, prompt-injection, prompt-iteration]
versions: "Claude 5.5 family; recommendations of the Prompting best practices page as of 2026-11"
status: draft
updated: 2026-11-24
---

# Prompt engineering: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). As at the basic level, the quality of the techniques on real models was not measured: local functions and the request shape against a mock server were run. The recommendations for specific models come from their prompting guides at the time of writing; recheck them when you change the model.

## Prompt chains

A complex task is split into several calls, where the result of one goes to the next. A typical scheme is **draft → review against criteria → refine**. Each step is a separate request, so the intermediate result can be checked, logged or stopped:

```python
def step(prompt):
    client.messages.create(model="claude-opus-5-5", max_tokens=1024, messages=[{"role": "user", "content": prompt}])
    ...  # here you take the reply text

draft = step("Write a summary of: ...")
review = step(f"<draft>{draft}</draft>\nReview the draft against these criteria: accuracy, brevity.")
final = step(f"<draft>{draft}</draft>\n<review>{review}</review>\nRewrite the draft applying the review.")
```

In the experiment on the mock three steps gave three independent requests, and the result of the previous step went into the next in `<draft>` and `<review>` tags. When a chain is justified:

- the steps need **different instructions or roles**, and a shared prompt becomes overloaded;
- you need to **check** an intermediate result (format, facts) before continuing;
- some steps can be done by a **cheaper model** or in parallel.

When it is not needed: if the model solves the task in one call with thinking, extra steps only increase cost and latency.

> ⚠️ Verify: the recommendations on chains (the "draft → review → refine" scheme) come from a short summary of a documentation page, not from its full text.

## Thinking and effort

Modern models decide how much to think themselves: the depth is controlled by the parameters `thinking` and `output_config.effort` ([module 02](../02-messages-api/advanced.en.md#thinking-and-effort)), not by commands in the text.

- For agentic and multi-step tasks turn on **adaptive thinking** and give **general** guidance instead of a rigid sequence of steps ("think carefully about the task" is better than a script of ten points).
- If thinking is off, you can manually ask for reasoning in tags and the answer separately: "reason in `<thinking>`, the answer in `<answer>`"; the answer is then extracted by parsing the tag (see the [basic theory](basics.en.md#output-format)).
- Do not make the model write out its reasoning in the answer: on Claude Opus 5.5 such instructions are advised to be removed, because they can cause a refusal with the category `reasoning_extraction`.
- Behavior depends on effort: more on choosing it in [module 01](../01-models/advanced.en.md).

## New models: fewer instructions

The guides for Opus 5.5, Sonnet 5.5 and Fable 5.2 agree on the main point: prompts written for earlier models are often **excessive**.

| Model | What the documentation advises |
|---|---|
| Claude Opus 5.5 | prompts for Opus 5 usually work unchanged; start with effort `medium`; thinking is always on; for long agentic runs a checklist and a "continue" message help (a turn can end with `end_turn` after an intermediate message) |
| Claude Sonnet 5.5 | prompts for Sonnet 5 usually work unchanged; for the general case — effort `high`; at low effort the model sometimes asks for confirmation too early: "Keep working until everything the user asked for is done" helps; it may add unrequested tests and documentation: "Don't add features, tests, files, docs, or refactors that weren't asked for" helps |
| Claude Fable 5.2 | start with effort `high` and rerun the effort check; "delegate" instructions can lead to over-delegation, "answer directly without deliberating" does not turn off thinking at `medium` and above, and the word "never" is taken as an absolute: write "ask for approval before deleting files" instead of "never delete files"; "finish the whole task" helps at `low`/`medium` |

General conclusions:

- Remove "CRITICAL / MUST / NEVER" emphasis from old prompts: it causes **overcorrection**.
- Rules like "if in doubt, use tool X" lead to excessive calls.
- Rigid style limits ("no more than 20 words") can make the text choppy.
- State **what to do** and explain the goal; check on real examples.

> ⚠️ Verify: the model-specific recommendations are retold from the guides, some pages were read in abridged form; the instruction quotes are given as in the document and may be reworded when it is updated.

## Prefill no longer works

Starting the reply (an assistant message last in the list) on Claude 4.6 and later models returns a 400. Replacements: **structured outputs** for the format, **an instruction in `system`** ("start straight with the answer, no preamble"), moving the continuation into the user message, reminders in the next user message.

## Untrusted text and injection

When **foreign text** (an email, a page, a document) gets into a prompt, it may contain instructions ("ignore the previous instructions…"). Defense is layered:

- separate data from instructions with tags and **escape** closing tags (the `wrap_untrusted` example in the [basic theory](basics.en.md#xml-tags));
- wrap the inserted block in a tag with a **random identifier** so the text cannot guess the closing tag;
- tell the model explicitly that the tag's contents are **data**, not commands;
- do not give the model tools with side effects beyond what is needed (module 16 of the roadmap).

No technique guarantees protection: this is risk reduction, not a barrier.

## Iteration and evaluation

Editing a prompt "by eye" is unstable: fixing one case breaks another. A working cycle: a set of typical and edge inputs → a fixed prompt → evaluating the result (by format, a reference or another model) → an edit → re-evaluating on the **same** set. Change one thing at a time, keep prompt versions next to the code; more in module 14 of the roadmap.

## Best practices

- Start with a simple prompt and complicate it only for a measurable benefit.
- Separate instructions, data, examples with tags.
- Do not duplicate what a parameter already sets (effort, output format).
- Remove outdated "crutches" when changing the model.
- Treat any inserted foreign text as data.
- Keep a set of test cases.

## Common mistakes

- ❌ A growing list of "NEVER" prohibitions → ✅ short rules with a reason.
- ❌ Asking to "write out the reasoning" with thinking on → ✅ the `thinking`/`effort` parameters.
- ❌ Prefill on new models → ✅ structured outputs or an instruction.
- ❌ Inserting user text without markup → ✅ tags, escaping, a random identifier.
- ❌ Editing several places at once → ✅ one change — one check.
- ❌ Carrying an old prompt over to a new model unchecked → ✅ simplify and remeasure.

## Related materials

- Next: [expert theory](expert.en.md)
- Thinking and effort parameters: [02 · Messages API](../02-messages-api/advanced.en.md)
- Model selection: [01 · The Claude family and model selection](../01-models/advanced.en.md)

## Sources

- [Prompting best practices](https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices)
- [Prompting Claude Opus 5.5](https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5)
- [Prompting Claude Sonnet 5.5](https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-sonnet-5-5)
- [Prompting Claude Fable 5.2](https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-fable-5-2)
- [API errors: prefill is not supported](https://platform.claude.com/docs/en/api/errors#prefill-not-supported)
