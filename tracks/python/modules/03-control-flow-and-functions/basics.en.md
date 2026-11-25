---
title: "Control flow and functions: basics"
lang: en
track: python
kind: module
doc: basics
slug: 03-control-flow-and-functions
part: 1
tags: [control-flow, if, match, loops, functions, args, kwargs]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Control flow and functions: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Types and data structures](../02-types-and-data-structures/overview.en.md) · **Time:** ~2 h

## Why it matters

Without branching and repetition, programs only execute sequential statements. Functions let code reuse a sequence and hide implementation details.

## `if`, `elif`, `else`

```python
temperature = 18
if temperature < 10:
    print("cold")
elif temperature < 25:
    print("comfortable")
else:
    print("hot")
```

```text
comfortable
```

Python evaluates branches from top to bottom and stops after the first true condition.

## Structural matching: `match`

```python
command = ("move", 3)
match command:
    case ("move", distance):
        print("move", distance)
    case ("stop",):
        print("stop")
    case _:
        print("unknown")
```

```text
move 3
```

`case _` is a wildcard pattern and behaves like a final `else`.

## `for` loop

```python
total = 0
for number in range(1, 5):
    total += number
print(total)
```

```text
10
```

`range(1, 5)` includes 1, 2, 3, and 4, but not 5.

For a dictionary, iterate keys, key–value pairs, or values:

```python
for key, value in {"a": 1, "b": 2}.items():
    print(key, value)
```

## `while` loop

```python
count = 3
while count > 0:
    print(count)
    count -= 1
```

```text
3
2
1
```

The condition is checked before every iteration. State must change or the loop never stops.

## `break` and `continue`

```python
for number in range(6):
    if number == 2:
        continue
    if number == 4:
        break
    print(number)
```

```text
0
1
3
```

`continue` skips the rest of an iteration; `break` exits the nearest loop.

## Functions

```python
def add(left, right):
    return left + right

print(add(2, 3))
```

```text
5
```

Parameters are declared in the definition; arguments are supplied at call time. Without `return`, a function returns `None`.

## Positional and named arguments

```python
def format_name(first, last, title=""):
    return f"{title} {first} {last}".strip()

print(format_name("Ada", "Lovelace"))
print(format_name(last="Lovelace", first="Ada", title="Dr"))
```

```text
Ada Lovelace
Dr Ada Lovelace
```

## Beginner mistakes

- ❌ Use `&&` or `||` for conditions → ✅ use `and` and `or`.
- ❌ Forget a colon or indentation → ✅ validate Python syntax.
- ❌ Mutate a list during direct iteration → ✅ iterate over a copy or build new values.
- ❌ Use `while` without advancing state → ✅ make state reach the stop condition.
- ❌ Expect a function to print and return simultaneously → ✅ use `print` and `return` separately.

## Related materials

- Prerequisites: [02 · Types and data structures](../02-types-and-data-structures/overview.en.md).
- Next: [advanced theory](advanced.en.md).

## Sources

- [Python control flow tutorial](https://docs.python.org/3/tutorial/controlflow.html)
- [Python compound statements](https://docs.python.org/3/reference/compound_stmts.html)
