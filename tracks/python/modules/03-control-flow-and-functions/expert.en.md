---
title: "Control flow and functions: expert"
lang: en
track: python
kind: module
doc: expert
slug: 03-control-flow-and-functions
part: 1
tags: [execution-model, frames, LEGB, closures, recursion, call-signatures]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Control flow and functions: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

This builds on [advanced theory](advanced.en.md).

## Parameter reference

| Form | Purpose |
|---|---|
| `def f(a, b)` | required positional-or-keyword parameters |
| `def f(a=1)` | optional parameter with default |
| `def f(a, /, b)` | `a` positional-only |
| `def f(*args)` | arbitrary positional arguments |
| `def f(a, *, b)` | `b` keyword-only |
| `def f(**kwargs)` | arbitrary named arguments |

Forms can be combined in this order: positional-only, `/`, ordinary, `*args`, keyword-only, `**kwargs`.

## Call internals

Calling creates a new frame with local names and parameter values. Returning removes a normal local frame, but a closure retains cells for external variables it needs.

Python passes a reference to an object. Assignment to the parameter inside the function does not replace the caller's variable.

## `return`

A function returns exactly one value. Multiple objects are packed into a tuple:

```python
def divide_pair(a, b):
    return divmod(a, b)

print(divide_pair(7, 3))
```

```text
(2, 1)
```

Without `return`, the result is `None`.

## Recursion

A recursive function calls itself with a converging base case:

```python
def factorial(n):
    if n <= 1:
        return 1
    return n * factorial(n - 1)

print(factorial(5))
```

```text
120
```

Deep recursion is limited by the interpreter recursion counter. Do not use recursion for unbounded traversal without an explicit base.

## Traps

### 1. Shared default

```python
def append_default(value, items=[]):
    items.append(value)
    return items
print(append_default("a"))
print(append_default("b"))
```

The second call result contains both elements.

### 2. `UnboundLocalError`

```python
value = 1
def change():
    print(value)
    value = 2
```

Calling raises `UnboundLocalError` because assignment makes `value` local, while reading occurs before assignment.

### 3. Late closure binding

```python
functions = []
for value in range(3):
    functions.append(lambda: value)
print([function() for function in functions])
```

```text
[2, 2, 2]
```

Every lambda reads the final loop-variable cell.

## Interview questions

<details><summary>1. Are Python arguments passed by value or reference?</summary>

A reference to the object is passed. This is commonly described as pass by object reference. Local reassignment of the parameter does not replace the caller's variable.

</details>

<details><summary>2. Where is a default parameter evaluated?</summary>

When the function is defined, not on every call.

</details>

<details><summary>3. How do LEGB and local variables relate?</summary>

LEGB describes name lookup through local, enclosing, global, and builtins.

</details>

<details><summary>4. What does `nonlocal` do?</summary>

It binds assignment to a variable in the nearest enclosing function instead of creating a new local variable.

</details>

<details><summary>5. How many values does a function return?</summary>

Exactly one object. Syntax with multiple expressions packs them into a tuple.

</details>

<details><summary>6. How does `for else` differ from a normal else?</summary>

The clause runs after iteration exhaustion, but not after exiting through `break`.

</details>

<details><summary>7. How do you fix late binding in lambdas?</summary>

Pass the value as a default: `lambda value=value: value`.

</details>

<details><summary>8. Is it safe to mutate a list in a loop?</summary>

Building a new list or iterating over a copy is safer, especially when deleting elements.

</details>

<details><summary>9. When is recursion safe?</summary>

When there is a converging base case and depth stays below the interpreter limit.

</details>

<details><summary>10. Why is `match` better than plain `if`?</summary>

It expresses structure checks and field extraction clearly. Plain `if` remains clearer for a single boolean condition.

</details>

## Tradeoffs and alternatives

Use iterative solutions for large depths and recursion. For callbacks, use named functions or closures with an explicit scope.

## Related materials

- Advanced: [control flow and functions](advanced.en.md).
- Next module: the [Python track roadmap](../../overview.en.md).

## Sources

- [Python execution model](https://docs.python.org/3/reference/executionmodel.html)
- [Python compound statements](https://docs.python.org/3/reference/compound_stmts.html)
