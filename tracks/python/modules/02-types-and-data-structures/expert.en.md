---
title: "Types and data structures: expert"
lang: en
track: python
kind: module
doc: expert
slug: 02-types-and-data-structures
part: 1
tags: [data-model, object-identity, mutability, hashing, references, garbage-collection]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Types and data structures: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

This builds on [advanced theory](advanced.en.md).

## Built-in container reference

| Type | Mutability | Uniqueness | Order | Use |
|---|---|---|---|---|
| `list` | mutable | no | preserved | sequence with frequent updates |
| `tuple` | immutable container | no | preserved | record or key |
| `dict` | mutable | keys unique | insertion order preserved | lookup by key |
| `set` | mutable | unique elements | not guaranteed | set operations |
| `frozenset` | immutable | unique elements | not guaranteed | immutable set |
| `range` | immutable | no | preserved | numeric sequence |

## Object model

Every object has identity, type, and value. Identity stays unchanged for an existing object, type determines supported operations, and mutable types can change value.

Variables do not contain objects: they are references to objects. Assignment therefore does not copy a container.

## Reference internals

CPython uses reference counting and an additional cyclic garbage collector. An object may be released immediately once it reaches zero references. Cyclic groups are detected separately.

Do not rely on immediate garbage collection or function-exit finalization in portable code. Python implementations may differ.

## Hashability

Dictionaries and sets require hashable keys. A custom immutable object is hashable only when it implements compatible `__hash__` and `__eq__`.

## Traps

### 1. A tuple with mutable contents

```python
value = ([1],)
try:
    hash(value)
except TypeError as error:
    print(type(error).__name__)
```

A tuple is immutable as a container of references, but a nested list remains mutable.

### 2. A slice creates a shallow copy

```python
a = [[1]]
b = a[:]
b[0].append(2)
print(a)
```

Nested lists require a deep copy.

### 3. Number comparison

```python
print(0.1 + 0.2 == 0.3)
```

```text
False
```

Use an explicit tolerance for approximate comparison; use exact representations for money.

### 4. Process-dependent hashing

String and bytes hashes may be salted and differ between interpreter runs. Do not store a hash as a stable database key.

## Interview questions

<details><summary>1. How does `==` differ from `is`?</summary>

`==` invokes object equality semantics, while `is` tests whether two references name the same object. Use `is` for `None`.

</details>

<details><summary>2. Why cannot a list be a dictionary key?</summary>

It is mutable and unhashable. Mutation after insertion could break the dictionary structure.

</details>

<details><summary>3. Is a tuple completely immutable?</summary>

The tuple cannot change its set of references, but those references may lead to mutable objects.

</details>

<details><summary>4. How does copy differ from deepcopy?</summary>

`copy` copies only the top level, while `deepcopy` recursively copies nested mutable objects.

</details>

<details><summary>5. Why is `0.1 + 0.2 != 0.3`?</summary>

Both numbers in binary floating-point representation are approximate. The result differs from the literal `0.3`.

</details>

<details><summary>6. What does argument passing mean in Python?</summary>

A reference to the object is passed, not a copy. Reassigning the parameter does not change the caller's variable.

</details>

<details><summary>7. Why does set not guarantee order?</summary>

It is optimized for uniqueness and hash-based lookup. Order depends on the table and hashes.

</details>

<details><summary>8. How does a dictionary work at a high level?</summary>

It is a hash table of keys and values. A key determines a bucket, and equality resolves collisions.

</details>

<details><summary>9. Is memory manually freed after deleting a variable?</summary>

No. A variable is not a memory allocator. Reclamation is performed by the implementation's garbage collector.

</details>

<details><summary>10. How does `range` differ from `list`?</summary>

`range` stores sequence parameters and produces values as a compact representation; `list` stores references to elements.

</details>

<details><summary>11. Which type represents an immutable set?</summary>

`frozenset`.

</details>

<details><summary>12. Can a dictionary use a custom equality key?</summary>

Yes, if the object is hashable and implements compatible `__hash__` and `__eq__`.

</details>

## Tradeoffs and alternatives

Use `Decimal` for financial and exact decimal calculations. For numeric arrays, use specialized libraries, and for named fields use dataclass or TypedDict.

## Related materials

- Advanced: [types and data structures](advanced.en.md).
- Next module: [03 · Control flow and functions](../03-control-flow-and-functions/overview.en.md).
- JavaScript comparison: [Types and values](../../../javascript/modules/02-types-and-values/overview.en.md).

## Sources

- [Python data model](https://docs.python.org/3/reference/datamodel.html)
- [Python built-in types](https://docs.python.org/3/library/stdtypes.html)
- [Python garbage collection](https://docs.python.org/3/library/gc.html)
