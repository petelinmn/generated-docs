---
title: "Exercises: types and data structures"
lang: en
track: python
kind: module
doc: exercises
slug: 02-types-and-data-structures
part: 1
tags: [types, list, tuple, dict, set, copy]
status: draft
updated: 2026-11-25
---

# Exercises: types and data structures

🌐 [Русский](exercises.ru.md) · **English**

> Solve them yourself before opening hints.

## 1. 🟢 Determine the type

**Task:** write a loop that prints the type of each element in a list.

**Expected result:** `42:int`, `3.14:float`, `hello:str`, `True:bool`, `None:NoneType`.

<details><summary>Hint</summary>

Use `type(value).__name__`.

</details>

<details><summary>Solution</summary>

```python
values = [42, 3.14, "hello", True, None]
for value in values:
    print(f"{value!r}:{type(value).__name__}")
```

</details>

## 2. 🟢 Copy and change a list

**Task:** create a copy of `source`, add an element to the copy, and verify the original list.

**Expected result:** the source remains unchanged.

<details><summary>Hint</summary>

For a flat list, `source.copy()` or a slice works.

</details>

<details><summary>Solution</summary>

```python
source = [1, 2]
copy = source.copy()
copy.append(3)
print(source)
print(copy)
```

</details>

## 3. 🟡 Mapping and multiple keys

**Task:** create a dictionary that reports the count of every string in a list.

**Expected result:** `{"a": 2, "b": 1}`.

<details><summary>Hint</summary>

Use `dict.get(item, 0) + 1`.

</details>

<details><summary>Solution</summary>

```python
items = ["a", "b", "a"]
counts = {}
for item in items:
    counts[item] = counts.get(item, 0) + 1
print(counts)
```

</details>

## 4. 🟡 Shallow copy

**Task:** show that a shallow copy of a dictionary retains a reference to a nested list.

**Expected result:** the source nested list changes together with the copy.

<details><summary>Hint</summary>

Use `source.copy()` and change the nested object through the copy.

</details>

<details><summary>Solution</summary>

```python
source = {"items": [1]}
copy = source.copy()
copy["items"].append(2)
print(source)
print(copy)
```

</details>

## 5. 🔴 Deep copy

**Task:** create an independent deep copy of a nested dictionary.

**Expected result:** changing the copy does not change the source.

<details><summary>Hint</summary>

Import `deepcopy`.

</details>

<details><summary>Solution</summary>

```python
from copy import deepcopy

source = {"items": [1]}
copy = deepcopy(source)
copy["items"].append(2)
print(source)
print(copy)
```

</details>
