---
title: "Batch, evals and reliability: exercises"
lang: en
track: claude
kind: module
doc: exercises
slug: 14-evals
part: 3
tags: [claude, evals, batch-api, retries, reliability]
versions: "Claude 5.5 family; anthropic 2.14.0; Message Batches API"
status: draft
updated: 2026-11-25
---

# Batch, evals and reliability: exercises

🌐 [Русский](exercises.ru.md) · **English**

> You need `evals.py`, `reliability.py` and `batch.py` from the theory. Exercises 1–4 run without an API key.

## 1. New cases

Add an empty string and compute the accuracy of a stub that always answers `other`.

<details><summary>Hint</summary>

Pass your own list.

</details>

<details><summary>Solution</summary>

```python
from evals import CASES, run_eval

cases = CASES + [{"id": "empty", "input": "", "expected": "other"}]
print(run_eval(lambda text: "other", cases))
```

</details>

## 2. A CI threshold

Exit with an error if accuracy is below 0.9.

<details><summary>Hint</summary>

`sys.exit`.

</details>

<details><summary>Solution</summary>

```python
import sys

from evals import run_eval

accuracy, failed = run_eval(lambda text: "other")
print(accuracy, failed)
sys.exit(0 if accuracy >= 0.9 else 1)
```

</details>

## 3. Failures by category

Count failures per expected label.

<details><summary>Hint</summary>

`Counter`.

</details>

<details><summary>Solution</summary>

```python
from collections import Counter

from evals import CASES, run_eval

_, failed = run_eval(lambda text: "other")
print(Counter(c["expected"] for c in CASES if c["id"] in failed))
```

</details>

## 4. A strict judge

Write a verdict format check without calling the API.

<details><summary>Hint</summary>

Only `pass` or `fail`.

</details>

<details><summary>Solution</summary>

```python
def parse_verdict(text):
    verdict = text.strip().lower()
    if verdict not in ("pass", "fail"):
        raise ValueError(f"unexpected verdict: {verdict!r}")
    return verdict == "pass"

print(parse_verdict(" PASS "), parse_verdict("fail"))
```

</details>

## 5. An eval through a batch

Run the cases through `run_batch` and compute accuracy.

<details><summary>Hint</summary>

`custom_id` = case id.

</details>

<details><summary>Solution</summary>

```python
from batch import run_batch
from evals import CASES, grade

prompts = {c["id"]: "Label as refund, delivery or other. Answer with the label only: " + c["input"] for c in CASES}
answers, failed = run_batch(client, prompts)
print(sum(grade(answers.get(c["id"], ""), c["expected"]) for c in CASES) / len(CASES), failed)
```

</details>
