---
title: "Exercises: control flow and functions"
lang: en
track: python
kind: module
doc: exercises
slug: 03-control-flow-and-functions
part: 1
tags: [control-flow, loops, functions, args, kwargs]
status: draft
updated: 2026-11-25
---

# Exercises: control flow and functions

🌐 [Русский](exercises.ru.md) · **English**

> Solve them yourself before opening hints.

## 1. 🟢 Classify a number

**Task:** write `classify(n)` that returns `"positive"`, `"zero"`, or `"negative"`.

**Expected result:** `classify(4) -> positive`, `classify(0) -> zero`, `classify(-2) -> negative`.

<details><summary>Hint</summary>

Check `n > 0`, `n == 0`, then the remaining case.

</details>

<details><summary>Solution</summary>

```python
def classify(n):
    if n > 0:
        return "positive"
    if n == 0:
        return "zero"
    return "negative"

print(classify(4), classify(0), classify(-2))
```

</details>

## 2. 🟢 Sum numbers

**Task:** write `sum_to(n)` to sum every integer from 1 through `n`.

**Expected result:** `sum_to(5) == 15`.

<details><summary>Hint</summary>

Use `range(1, n + 1)` and an accumulator.

</details>

<details><summary>Solution</summary>

```python
def sum_to(n):
    total = 0
    for number in range(1, n + 1):
        total += number
    return total

print(sum_to(5))
```

</details>

## 3. 🟡 Safe default

**Task:** write `append_item(value, items=None)` that does not share state between calls.

**Expected result:** two calls from an empty start produce `['a']` and `['b']`.

<details><summary>Hint</summary>

Create a local list in the `items is None` branch.

</details>

<details><summary>Solution</summary>

```python
def append_item(value, items=None):
    if items is None:
        items = []
    items.append(value)
    return items

print(append_item("a"))
print(append_item("b"))
```

</details>

## 4. 🟡 Match a command

**Task:** write `describe_command(command)` that recognizes `("move", n)`, `("stop",)`, and everything else.

**Expected result:** `move 3`, `stop`, `unknown`.

<details><summary>Hint</summary>

Use `match/case` and the `_` wildcard.

</details>

<details><summary>Solution</summary>

```python
def describe_command(command):
    match command:
        case ("move", distance):
            return f"move {distance}"
        case ("stop",):
            return "stop"
        case _:
            return "unknown"

print(describe_command(("move", 3)))
print(describe_command(("stop",)))
print(describe_command(("dance",)))
```

</details>

## 5. 🔴 Capture closure value

**Task:** create three functions with numbers 0, 1, and 2, then verify their calls.

**Expected result:** `[0, 1, 2]`.

<details><summary>Hint</summary>

Pass the loop variable through a lambda default parameter.

</details>

<details><summary>Solution</summary>

```python
functions = [lambda value=value: value for value in range(3)]
print([function() for function in functions])
```

</details>
