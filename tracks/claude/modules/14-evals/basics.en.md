---
title: "Batch, evals and reliability: fundamentals"
lang: en
track: claude
kind: module
doc: basics
slug: 14-evals
part: 3
tags: [claude, evals, batch-api, retries, reliability]
versions: "Claude 5.5 family; anthropic 2.14.0; Message Batches API"
status: draft
updated: 2026-11-25
---

# Batch, evals and reliability: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Messages API](../02-messages-api/overview.en.md), [03 · Prompt engineering](../03-prompt-engineering/overview.en.md)

## How this was checked

There is no API key. The code ran with the `anthropic` 2.14.0 SDK against a local server imitating API replies and errors. Retries, exception classes and batch result handling were checked with the real SDK; service behaviour, prices and limits come from the documentation.

## Why evals

You changed a prompt or a model: is it better? Without checks that is guesswork. An **eval** is example inputs, a way to score the answer and a threshold.

Make the success criterion specific and measurable: not "classifies well" but "at least 90% correct labels on a held-out set".

## A minimal eval

```python
# evals.py
CASES = [
    {"id": "refund", "input": "I want my money back", "expected": "refund"},
    {"id": "delivery", "input": "Where is my parcel?", "expected": "delivery"},
    {"id": "other", "input": "Do you sell gift cards?", "expected": "other"},
]


def grade(output, expected):
    return output.strip().lower() == expected


def run_eval(classify, cases=CASES):
    """classify(text) -> label. Returns (accuracy, failed case ids)."""
    failed = [c["id"] for c in cases if not grade(classify(c["input"]), c["expected"])]
    return (len(cases) - len(failed)) / len(cases), failed
```

```python
from evals import run_eval


def keyword_baseline(text):            # a stand-in for a model call
    if "money" in text:
        return "refund"
    if "parcel" in text:
        return "Delivery"
    return "unknown"


print(run_eval(keyword_baseline))
```

```text
(0.6666666666666666, ['other'])
```

`grade` lowercases the answer, so `Delivery` passes; the `unknown` label fails. The list of failed cases matters more than the average: it shows what to fix.

## Three ways to grade

| Method | When |
|---|---|
| code | exact match, format, presence of facts: fast and cheap |
| a model | tone, completeness, rubric fit: validate the judge first |
| a human | the gold standard and disputes: slow and costly |

Many automatically graded examples beat a few hand-graded ones.

## Common beginner mistakes

- Testing on the examples the prompt was tuned on.
- Happy-path inputs only: add empty, long, hostile and ambiguous ones.
- One run as proof: model answers vary between runs.

## Related materials

- [Advanced theory](advanced.en.md)
- [04 · Structured outputs](../04-structured-output/overview.en.md)

## Sources

- [Define success criteria and build evaluations](https://platform.claude.com/docs/en/test-and-evaluate/develop-tests)
