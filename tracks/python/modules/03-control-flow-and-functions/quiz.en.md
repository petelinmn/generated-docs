---
title: "Quiz: control flow and functions"
lang: en
track: python
kind: module
doc: quiz
slug: 03-control-flow-and-functions
part: 1
tags: [control-flow, loops, functions, scopes, args, kwargs]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Quiz: control flow and functions

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer.

## 1. 🟢 What does this loop print?

```python
for number in range(3):
    print(number)
```

- A) `1 2 3`
- B) `0 1 2`
- C) `0 1 2 3`
- D) Nothing

<details><summary>Answer</summary>

**B.** `range(3)` contains 0, 1, and 2.

</details>

## 2. 🟢 What does a function without `return` return?

- A) `0`
- B) `False`
- C) `None`
- D) An empty string

<details><summary>Answer</summary>

**C.** A function without explicit `return` returns `None`.

</details>

## 3. 🟢 What is `break` for?

- A) Ending the program
- B) Ending the nearest loop
- C) Skipping an iteration
- D) Ending a function

<details><summary>Answer</summary>

**B.** `continue` skips an iteration; `break` exits the nearest loop.

</details>

## 4. 🟢 What does `*args` collect?

- A) Named arguments
- B) Positional arguments into a tuple
- C) Global variables
- D) Dictionary keys

<details><summary>Answer</summary>

**B.** Extra positional arguments are collected into the `args` tuple.

</details>

## 5. 🟡 What does this print?

```python
def add_item(item, items=[]):
    items.append(item)
    return items

print(add_item("a"))
print(add_item("b"))
```

- A) `['a']`, `['b']`
- B) `['a']`, `['a', 'b']`
- C) `[]`, `['b']`
- D) Error

<details><summary>Answer</summary>

**B.** The mutable default is shared by all calls.

</details>

## 6. 🟡 What does `nonlocal` do?

- A) Creates a global variable
- B) Binds a name to an enclosing function variable
- C) Deletes a variable
- D) Copies a local dictionary

<details><summary>Answer</summary>

**B.** `nonlocal` permits assignment to an enclosing function variable.

</details>

## 7. 🟡 What does the match print?

```python
value = ("stop",)
match value:
    case ("move", distance):
        print(distance)
    case ("stop",):
        print("stopped")
```

- A) `1`
- B) `stopped`
- C) `stop`
- D) Nothing

<details><summary>Answer</summary>

**B.** The tuple has the exact structure required by the second pattern.

</details>

## 8. 🟡 Select safe alternatives to a mutable default

- [ ] A) `items=[]`
- [ ] B) `items=None`
- [ ] C) Create `[]` inside the function when `items is None`
- [ ] D) A global list

<details><summary>Answer</summary>

**B, C.** `None` is not mutable; a local list is created at call time.

</details>

## 9. 🔴 Find the bug

```python
functions = [lambda: value for value in range(3)]
print([function() for function in functions])
```

<details><summary>Answer</summary>

Every lambda reads the same final loop variable, so the result is `[2, 2, 2]`. Capture the value through a default:

```python
functions = [lambda value=value: value for value in range(3)]
```

</details>

## 10. 🔴 Open question

Name Python's name lookup order and explain the role of a closure.

<details><summary>Key points of a good answer</summary>

- LEGB: local, enclosing, global, builtins.
- A closure retains references to external-scope cells after the factory function exits.
- Late binding must be considered when closures read loop variables.

</details>
