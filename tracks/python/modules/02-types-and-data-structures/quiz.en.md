---
title: "Quiz: types and data structures"
lang: en
track: python
kind: module
doc: quiz
slug: 02-types-and-data-structures
part: 1
tags: [types, mutability, identity, equality, list, tuple, dict, set]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Quiz: types and data structures

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which operator checks the identity of two variables?

- A) `==`
- B) `is`
- C) `===`
- D) `typeof`

<details><summary>Answer</summary>

**B.** `is` checks whether both references point to the same object.
→ [Basics](basics.en.md#values-and-references)

</details>

## 2. 🟢 Which type is mutable?

- A) `tuple`
- B) `str`
- C) `list`
- D) `frozenset`

<details><summary>Answer</summary>

**C.** A `list` can change through `append`, `pop`, slicing, and assignment.

</details>

## 3. 🟢 What does `{"a": 1}.get("b", "unknown")` print?

- A) `None`
- B) `KeyError`
- C) `"unknown"`
- D) `False`

<details><summary>Answer</summary>

**C.** `dict.get` returns the supplied default when the key is absent.

</details>

## 4. 🟢 Can a list be used as a dictionary key?

- A) Yes, always
- B) No, lists are unhashable
- C) Yes, if it has fewer than 10 elements
- D) Only inside a tuple

<details><summary>Answer</summary>

**B.** A dictionary key must be hashable.

</details>

## 5. 🟡 What does this code print?

```python
a = [1, 2]
b = a
b.append(3)
print(a)
```

- A) `[1, 2]`
- B) `[1, 2, 3]`
- C) `[3]`
- D) Error

<details><summary>Answer</summary>

**B.** `a` and `b` reference the same mutable list.

</details>

## 6. 🟡 What distinguishes shallow copy from deep copy?

- A) Nothing
- B) Deep copy also copies nested mutable objects
- C) Shallow copy copies the whole graph
- D) Deep copy creates the same object

<details><summary>Answer</summary>

**B.** `copy()` copies only the top level; `deepcopy()` handles nested objects.

</details>

## 7. 🟡 What is `0.1 + 0.2 == 0.3`?

- A) `True`
- B) `False`
- C) `None`
- D) OS-dependent

<details><summary>Answer</summary>

**B.** Binary floats approximate these values.

</details>

## 8. 🟡 Select all immutable containers

- [ ] A) `tuple`
- [ ] B) `frozenset`
- [ ] C) `list`
- [ ] D) `dict`

<details><summary>Answer</summary>

**A, B.** Tuples and frozensets cannot be mutated through their own methods.

</details>

## 9. 🔴 Find the error

```python
scores = {[1, 2]: "Ada"}
```

<details><summary>Answer</summary>

A list is unhashable. Replace it with a tuple for the dictionary key:

```python
scores = {(1, 2): "Ada"}
```

</details>

## 10. 🔴 Open question

Explain why a tuple containing `[1]` is not fully hashable and how this relates to nested mutability.

<details><summary>Key points of a good answer</summary>

- The tuple itself is immutable as a container of references.
- Its element is a mutable list.
- The list is unhashable, so hashing the complete tuple raises `TypeError`.

</details>
