---
title: "Control flow and functions: advanced"
lang: en
track: python
kind: module
doc: advanced
slug: 03-control-flow-and-functions
part: 1
tags: [args, kwargs, scopes, closure, lambda, default-arguments]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Control flow and functions: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

This builds on [basics](basics.en.md).

## `*args` and `**kwargs`

```python
def summarize(label, *values, **options):
    print(label, values, options)

summarize("result", 10, 20, unit="items")
```

```text
result (10, 20) {'unit': 'items'}
```

`*args` collects extra positional arguments in a tuple, while `**kwargs` collects named arguments in a dictionary.

## Default values

Default values are evaluated when the function is defined:

```python
def add_item(item, items=[]):
    items.append(item)
    return items

print(add_item("a"))
print(add_item("b"))
```

```text
['a']
['a', 'b']
```

The mutable object is shared by all calls. For typical behavior, use `None`:

```python
def add_item(item, items=None):
    if items is None:
        items = []
    items.append(item)
    return items
```

## Scope and LEGB

Python searches names through local, enclosing, global, then builtins:

```python
count = "global"

def outer():
    count = "enclosing"
    def inner():
        count = "local"
        return count
    return inner()

print(count, outer())
```

```text
global local
```

`global` reads or changes a module variable; `nonlocal` changes a variable from an enclosing function. Passing data through parameters is usually clearer.

## Closure

```python
def make_counter(start=0):
    value = start
    def next_value():
        nonlocal value
        value += 1
        return value
    return next_value

counter = make_counter()
print(counter(), counter())
```

```text
1 2
```

## Lambda functions

`lambda` creates a short anonymous function:

```python
names = ["Ada", "Linus", "Grace"]
print(sorted(names, key=lambda name: len(name)))
```

```text
['Ada', 'Linus', 'Grace']
```

Use a named function for complex logic.

## Best practices

- Do not use mutable defaults.
- Choose parameter order explicitly.
- Keep lambdas to a single expression.
- Prefer returned data over global mutation.
- Use closures only when state retention is needed.

## Common mistakes

- ❌ A shared-list default for an accumulator → ✅ `None` plus a local collection.
- ❌ Change global state unnecessarily → ✅ parameters and `return`.
- ❌ Mix positional and named arguments incorrectly → ✅ place named arguments after `*`.
- ❌ Use a lambda for complex logic → ✅ define a named function.

## Related materials

- Basics: [control flow and functions](basics.en.md).
- Next module: the [Python track roadmap](../../overview.en.md).

## Sources

- [Python control flow](https://docs.python.org/3/tutorial/controlflow.html)
- [Python execution model](https://docs.python.org/3/reference/executionmodel.html)
