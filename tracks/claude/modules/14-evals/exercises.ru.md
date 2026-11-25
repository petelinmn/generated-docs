---
title: "Batch, evals и надёжность: упражнения"
lang: ru
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

# Batch, evals и надёжность: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Нужны `evals.py`, `reliability.py` и `batch.py` из теории. Упражнения 1–4 выполняются без ключа API.

## 1. Новые случаи

Добавьте пустую строку и посчитайте точность заглушки, всегда отвечающей `other`.

<details><summary>Подсказка</summary>

Передайте свой список.

</details>

<details><summary>Решение</summary>

```python
from evals import CASES, run_eval

cases = CASES + [{"id": "empty", "input": "", "expected": "other"}]
print(run_eval(lambda text: "other", cases))
```

</details>

## 2. Порог в CI

Завершите программу ошибкой, если точность ниже 0.9.

<details><summary>Подсказка</summary>

`sys.exit`.

</details>

<details><summary>Решение</summary>

```python
import sys

from evals import run_eval

accuracy, failed = run_eval(lambda text: "other")
print(accuracy, failed)
sys.exit(0 if accuracy >= 0.9 else 1)
```

</details>

## 3. Провалы по категориям

Посчитайте, сколько провалов на каждую ожидаемую метку.

<details><summary>Подсказка</summary>

`Counter`.

</details>

<details><summary>Решение</summary>

```python
from collections import Counter

from evals import CASES, run_eval

_, failed = run_eval(lambda text: "other")
print(Counter(c["expected"] for c in CASES if c["id"] in failed))
```

</details>

## 4. Строгий судья

Напишите проверку формата вердикта без обращения к API.

<details><summary>Подсказка</summary>

Только `pass` или `fail`.

</details>

<details><summary>Решение</summary>

```python
def parse_verdict(text):
    verdict = text.strip().lower()
    if verdict not in ("pass", "fail"):
        raise ValueError(f"unexpected verdict: {verdict!r}")
    return verdict == "pass"

print(parse_verdict(" PASS "), parse_verdict("fail"))
```

</details>

## 5. Eval через batch

Прогоните случаи через `run_batch` и посчитайте точность.

<details><summary>Подсказка</summary>

`custom_id` = id случая.

</details>

<details><summary>Решение</summary>

```python
from batch import run_batch
from evals import CASES, grade

prompts = {c["id"]: "Label as refund, delivery or other. Answer with the label only: " + c["input"] for c in CASES}
answers, failed = run_batch(client, prompts)
print(sum(grade(answers.get(c["id"], ""), c["expected"]) for c in CASES) / len(CASES), failed)
```

</details>
