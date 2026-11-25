---
title: "Types and data structures: basics"
lang: en
track: python
kind: module
doc: basics
slug: 02-types-and-data-structures
part: 1
tags: [types, numbers, strings, list, tuple, dict, set, mutability]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Types and data structures: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · Environment and tools](../01-environment/overview.en.md) · **Time:** ~2 h

## Why it matters

Data structures determine how elements are stored, whether they can change, and how values are looked up. Choosing the right type reduces errors and makes code clearer.

## Types and identity

Every value belongs to a type:

```python
values = [42, 3.14, 1 + 2j, "hello", True, None]
for value in values:
    print(repr(value), type(value).__name__)
```

```text
42 int
3.14 float
(1+2j) complex
'hello' str
True bool
None NoneType
```

`True` and `False` belong to `bool`, while missing value is represented by `None`.

## Numbers and strings

```python
print(7 / 2, 7 // 2, 7 % 2, 2 ** 10)
text = "Python"
print(text.lower(), text.upper(), len(text), text[0], text[-1])
```

```text
3.5 3 1 1024
python PYTHON 6 P n
```

`/` performs floating-point division, `//` discards the fractional part with floor behavior, and `%` yields the remainder.

Strings are immutable:

```python
text = "abc"
text = text.upper()
print(text)
```

## List

`list` is an ordered mutable sequence:

```python
items = [10, 20, 30]
items.append(40)
items.insert(1, 15)
print(items)
print(items[0], items[-1], items[1:3])
```

```text
[10, 15, 20, 30, 40]
10 40 [15, 20]
```

A slice `[1:3]` includes indices 1 and 2, but not 3.

## Tuple

`tuple` is an ordered immutable sequence:

```python
point = (10, 20)
x, y = point
print(x, y)
print(point + (30,))
```

```text
10 20
(10, 20, 30)
```

A tuple is useful for a record or a result with fixed fields.

## Dictionary

`dict` stores values by keys:

```python
user = {"name": "Ada", "age": 37}
print(user["name"])
print(user.get("email", "unknown"))
user["age"] += 1
print(user)
```

```text
Ada
unknown
{'name': 'Ada', 'age': 38}
```

Keys must be hashable: strings and tuples of immutable parts usually work, while lists do not.

## Set

`set` stores unique unordered elements:

```python
numbers = {3, 1, 2, 2}
numbers.add(4)
print(numbers)
print({1, 2, 3} & {3, 4})
```

```text
{1, 2, 3, 4}
{3}
```

Set element order is not guaranteed.

## Values and references

```python
a = [1, 2]
b = a
b.append(3)
print(a, b, a is b)

c = a.copy()
c.append(4)
print(a, c, a is c)
```

```text
[1, 2, 3] [1, 2, 3] True
[1, 2, 3] [1, 2, 3, 4] False
```

Variables store references to objects. `is` checks whether they refer to the same object, while `==` compares values.

## Beginner mistakes

- ❌ Use `==` to check for `None` → ✅ `value is None`.
- ❌ Check equal content with `is` → ✅ use `==`.
- ❌ Use a list as a dictionary key → ✅ use a tuple or another hashable structure.
- ❌ Expect a tuple containing a list to be deeply immutable → ✅ distinguish the container and nested objects.
- ❌ Expect set iteration to preserve insertion order → ✅ use `list` or `dict` for order.

## Related materials

- Prerequisites: [01 · Environment and tools](../01-environment/overview.en.md).
- Next: [advanced theory](advanced.en.md).
- Next module: [03 · Control flow and functions](../03-control-flow-and-functions/overview.en.md).

## Sources

- [Python built-in types](https://docs.python.org/3/library/stdtypes.html)
- [Python lists](https://docs.python.org/3/tutorial/datastructures.html)
