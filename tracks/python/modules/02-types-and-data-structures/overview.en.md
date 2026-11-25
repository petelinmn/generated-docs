---
title: "Types and data structures"
lang: en
track: python
kind: module
doc: overview
slug: 02-types-and-data-structures
part: 1
levels: [1, 2, 3, 4]
tags: [types, numbers, strings, list, tuple, dict, set, mutability, references]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Types and data structures

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Python represents data as objects: every object has identity, type, and value. Built-in types cover numbers, strings, booleans, sequences, mappings, and sets. Most containers are passed by reference rather than copied. Understanding mutability and identity makes code behavior predictable.

## Key ideas

- **A variable has no fixed type:** the type belongs to the object.
- **`is` compares identity; `==` compares values.**
- **`int`, `float`, `str`, `tuple`, and other immutable types cannot change after creation.**
- **`list`, `dict`, and `set` are mutable:** another alias can observe their changes.
- **A tuple is immutable but may contain mutable elements.**
- **Slices and copy operations create containers, but not necessarily deep copies.**
- **Losing references enables garbage collection, but memory cleanup is not guaranteed to be immediate.**

## Minimal example

```python
numbers = [1, 2, 3]
alias = numbers
alias.append(4)

print(numbers)
print(numbers is alias)
print(numbers == alias)
print(type(numbers).__name__)
```

```text
[1, 2, 3, 4]
True
True
list
```

## When to use and when not

- ✅ Use `list` for an ordered mutable collection.
- ✅ Use `tuple` for an immutable record or key.
- ✅ Use `dict` for named key–value pairs.
- ✅ Use `set` for unique unordered elements.
- ❌ Do not use a mutable object as a dictionary key or set element.
- ❌ Do not rely on an interned singleton of a type; use `is None` for `None`.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | types, numbers, strings, lists, tuples, dictionaries, sets |
| [2. Advanced](advanced.en.md) | mutability, copying, identity, comparisons, representation |
| [3. Expert](expert.en.md) | type reference, object model internals, traps, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not generated yet — `/learn python 02-types-and-data-structures level 5` |
| 6. Tests | not generated yet — `/learn python 02-types-and-data-structures level 6` |

## Related materials

- Prerequisites: [01 · Environment and tools](../01-environment/overview.en.md).
- Next: [03 · Control flow and functions](../03-control-flow-and-functions/overview.en.md).
- JavaScript analogue: [02 · Types and values](../../../javascript/modules/02-types-and-values/overview.en.md).
- C# analogue: types and variables in the [.NET track](../../../csharp/overview.en.md).

## Sources

- [Python built-in types](https://docs.python.org/3/library/stdtypes.html)
- [Python data model](https://docs.python.org/3/reference/datamodel.html)
- [Python numeric types](https://docs.python.org/3/library/stdtypes.html#numeric-types-int-float-complex)
