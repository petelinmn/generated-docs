---
title: "Types and data structures: advanced"
lang: en
track: python
kind: module
doc: advanced
slug: 02-types-and-data-structures
part: 1
tags: [mutability, shallow-copy, deep-copy, identity, equality, hashable]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Types and data structures: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

This builds on [basics](basics.en.md).

## Mutability and aliases

An alias is a second reference to the same object:

```python
original = [1, 2]
alias = original
alias.append(3)
print(original)
```

```text
[1, 2, 3]
```

When mutable data must be passed without side effects, create the copy that fits the task.

## Shallow and deep copies

```python
from copy import deepcopy

source = {"items": [1, 2]}
shallow = source.copy()
deep = deepcopy(source)

shallow["items"].append(3)
print(source, deep)
```

```text
{'items': [1, 2, 3]} {'items': [1, 2]}
```

`dict.copy()` copies only the top level. `deepcopy()` recursively copies nested mutable objects.

For a list, `items[:]` creates a shallow copy.

## Comparison and hashing

```python
print([1, 2] == [1, 2])
print([1, 2] is [1, 2])
print(hash("python"))
try:
    hash([1, 2])
except TypeError as error:
    print(type(error).__name__)
```

```text
True
False
<hash differs>
TypeError
```

`hash("python")` depends on the environment and randomization; do not expect an exact result. Do not assume hashes remain stable across processes.

Mutable contents must not be a dictionary key or set element.

## Numbers and precision

```python
print(0.1 + 0.2)
print(0.1 + 0.2 == 0.3)

from decimal import Decimal
print(Decimal("0.1") + Decimal("0.2"))
```

```text
0.30000000000000004
False
0.3
```

Use `Decimal` or integer minor units for money. Binary floating-point values are approximate.

## Best practices

- Choose immutable records for values that must not change.
- Use `copy()` or `deepcopy()` deliberately.
- Use `is` for identity or singleton objects such as `None`.
- Use `Decimal` for exact decimal arithmetic.
- Do not rely on `set` iteration order.
- Do not create dictionaries with mutable keys.

## Common mistakes

- ❌ Treat a slice as a deep copy → ✅ use `copy.deepcopy` for nested objects.
- ❌ Compare dictionaries with `is` → ✅ use `==`.
- ❌ Use float for money → ✅ use `Decimal` or integer cents.
- ❌ Put mutable objects in a set → ✅ use immutable hashable values.
- ❌ Expect a string hash to remain stable → ✅ treat hash stability as an implementation detail.

## Related materials

- Basics: [types and data structures](basics.en.md).
- Expert: [expert theory](expert.en.md).
- JavaScript comparison: [Types and values](../../../javascript/modules/02-types-and-values/overview.en.md).

## Sources

- [Python copy module](https://docs.python.org/3/library/copy.html)
- [Python numeric types](https://docs.python.org/3/library/stdtypes.html#numeric-types-int-float-complex)
- [decimal module](https://docs.python.org/3/library/decimal.html)
