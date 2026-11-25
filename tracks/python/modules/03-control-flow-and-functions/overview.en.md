---
title: "Control flow and functions"
lang: en
track: python
kind: module
doc: overview
slug: 03-control-flow-and-functions
part: 1
levels: [1, 2, 3, 4]
tags: [control-flow, if, match, loops, functions, args, kwargs, scopes, closure]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Control flow and functions

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Control flow selects branches and repeats actions with `if`, `match`, `for`, and `while`. Functions package code into named blocks with parameters and a return value. Positional and named arguments, `*args`, `**kwargs`, and scope rules determine how data is passed.

## Key ideas

- **`if/elif/else` executes exactly one matching branch.**
- **`match` selects branches by value structure; it was introduced in Python 3.10.**
- **`for` traverses iterators and collections, while `while` runs as long as a condition is true.**
- **`break` exits a loop; `continue` moves to the next iteration.**
- **A function is invoked with arguments, and `return` passes one result object.**
- **Arguments are passed by references to objects by default.**
- **Names in functions follow LEGB, and a closure retains access to an outer scope.**

## Minimal example

```python
def classify(score):
    if score >= 90:
        return "A"
    if score >= 75:
        return "B"
    return "C"

print(classify(92))
print(classify(80))
```

```text
A
B
```

## When to use and when not

- ✅ Use `if/elif/else` for sequential condition checks.
- ✅ Use `match` for data structure and type patterns.
- ✅ Use `for` for a known collection or range.
- ✅ Use `while` for state that changes until a stop condition.
- ❌ Do not mutate a collection during direct iteration without reason.
- ❌ Do not use mutable values as default arguments.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | `if`, `match`, loops, break/continue, functions, parameters |
| [2. Advanced](advanced.en.md) | `*args`, `**kwargs`, scopes, closures, lambdas, best practices |
| [3. Expert](expert.en.md) | call reference, LEGB, frame internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not generated yet — `/learn python 03-control-flow-and-functions level 5` |
| 6. Tests | not generated yet — `/learn python 03-control-flow-and-functions level 6` |

## Related materials

- Prerequisites: [02 · Types and data structures](../02-types-and-data-structures/overview.en.md).
- Next: the next module in the [Python track roadmap](../../overview.en.md).
- JavaScript analogue: [03 · Variables and scopes](../../../javascript/modules/03-scopes-and-hoisting/overview.en.md).
- C# analogue: [03 · Control flow and methods](../../../csharp/modules/03-control-flow-and-methods/overview.en.md).

## Sources

- [Python control flow](https://docs.python.org/3/tutorial/controlflow.html)
- [Python compound statements](https://docs.python.org/3/reference/compound_stmts.html)
- [Python data model: execution model](https://docs.python.org/3/reference/executionmodel.html)
