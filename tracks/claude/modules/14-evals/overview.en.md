---
title: "Batch, evals and reliability"
lang: en
track: claude
kind: module
doc: overview
slug: 14-evals
part: 3
tags: [claude, evals, batch-api, retries, reliability]
versions: "Claude 5.5 family; anthropic 2.14.0; Message Batches API"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Batch, evals and reliability

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

How to check answer quality (**evals**), survive errors and limits and process large volumes cheaply with **Message Batches**.

## Key ideas

- Without an eval there is no proof of improvement.
- The SDK retries some errors itself.
- A refusal is a 200 response.
- Batches are half price and asynchronous.
- Batch results are matched by `custom_id`.

## Minimal example

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

## When to use it and when not

Any application users will see. For a one-off experiment reading the output is enough.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude 14-evals level 5` |
| 6. Tests | — | `/learn claude 14-evals level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [03 · Prompt engineering](../03-prompt-engineering/overview.en.md)
- [13 · Compaction and long context](../13-long-context/overview.en.md)
- [15 · Observability](../15-observability/overview.en.md)
