---
title: "Prompt engineering"
lang: en
track: claude
kind: module
doc: overview
slug: 03-prompt-engineering
part: 1
levels: [1, 2, 3, 4]
tags: [claude, prompt-engineering, system-prompt, xml-tags, few-shot, long-context, prompt-chaining, thinking]
versions: "Claude 5.5 family; recommendations of the Prompting best practices page as of 2026-11"
status: draft
updated: 2026-11-24
---

# Prompt engineering

🌐 [Русский](overview.ru.md) · **English**

## In short

Prompt engineering is the craft of phrasing the input so the model reliably gives the result you need. For Claude the basics are simple: clear direct instructions, enough context and motivation, a role in `system`, structure through XML tags, a few diverse examples, an explicit output format and careful handling of long documents. New models follow instructions literally and are sensitive to excess "categorical" rules, so the best prompt is not the longest but the most precise.

## Key ideas

- **Clarity beats cleverness:** a good test is to show the prompt to a colleague with no context; if it confuses them, it will confuse the model too.
- **Explain "why":** a rule with a reason is generalized by the model better than a bare prohibition.
- **A role in `system`, even one sentence,** changes tone and expertise.
- **XML tags** separate instructions, data and examples; tag names are constant and descriptive.
- **Examples (multishot):** 3–5 relevant and diverse ones, in `<example>` tags.
- **Say what to do, not what not to do:** "write in flowing paragraphs" instead of "do not use markdown".
- **Long documents go first, the question last;** ask to quote the relevant parts first.
- **Prompt chains:** a complex task is split into steps with separate calls.
- **Thinking is regulated by parameters** (`effort`, adaptive thinking), not by incantations in the text.
- **New models need fewer instructions:** prompts written for old ones must be simplified and rechecked.

## Minimal example

```python
SYSTEM = "You are a senior Python reviewer. Be concise."

def build_prompt(docs, question):
    parts = ["<documents>"]
    for i, (source, text) in enumerate(docs, 1):
        parts.append(f'<document index="{i}"><source>{source}</source><document_content>{text}</document_content></document>')
    parts.append("</documents>")
    parts.append(f"First quote the passages that are relevant in <quotes> tags, then answer in <answer> tags.\n\nQuestion: {question}")
    return "\n".join(parts)

print(build_prompt([("a.txt", "Revenue grew 12%.")], "How did revenue change?"))
```

```text
<documents>
<document index="1"><source>a.txt</source><document_content>Revenue grew 12%.</document_content></document>
</documents>
First quote the passages that are relevant in <quotes> tags, then answer in <answer> tags.

Question: How did revenue change?
```

The documents go first, the question last, the answer format is set with tags.

## When to use and when not

| Task | Choice |
|---|---|
| A single tone, role, rules | `system` |
| Data, documents, examples | XML tags |
| A non-standard format or style | 3–5 examples |
| A multi-step process with checks | a chain of calls |
| Strictly JSON | structured outputs (module 04 of the roadmap), not persuasion in the prompt |
| Different behavior with the same words | evaluation (evals, module 14 of the roadmap), not guessing |

- ❌ Do not pile on "CRITICALLY IMPORTANT" and prohibitions: on new models this causes skews.
- ❌ Do not tweak a prompt "by eye" without a set of test cases.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | clear instructions, context, a role, XML tags, examples, output format, documents |
| [2. Advanced theory](advanced.en.md) | prompt chains, thinking and effort, new-model specifics, untrusted text, iteration |
| [3. Expert theory](expert.en.md) | a reference of techniques, internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn claude 03-prompt-engineering level 5` |
| 6. Tests | not created yet — `/learn claude 03-prompt-engineering level 6` |

## Related materials

- Prerequisites: [02 · Messages API](../02-messages-api/overview.en.md)
- Next: [04 · Structured outputs](../04-structured-output/overview.en.md)
- Analogues in other tracks: [LLM](../../../llm/overview.en.md), [Python](../../../python/overview.en.md)
- [14 · Batch, evals and reliability](../14-evals/overview.en.md)
