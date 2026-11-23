---
title: "Arrays and collections"
lang: en
track: javascript
kind: module
doc: overview
slug: 07-arrays-and-collections
part: 1
levels: [1, 2, 3, 4]
tags: [array, map, set, weakmap, typed-array, sort, iteration, immutability, array-methods, array-holes]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Arrays and collections

🌐 [Русский](overview.ru.md) · **English**

## In short

An array (`Array`) is an ordered list of values with indexes `0, 1, 2…` and a `length` property. It has a rich set of methods: `map`, `filter`, `reduce`, `find`, `sort` and others. Some methods **change** the array in place (`sort`, `reverse`, `splice`), while the newer "copying" ones (`toSorted`, `toReversed`, `with`, `toSpliced`) return a copy. Besides arrays there are **collections**: `Set` (unique values), `Map` (keys of any type with insertion order preserved), the weak `WeakMap` and `WeakSet`, and for binary data — typed arrays (`Uint8Array` and others). An array in JavaScript is an object, so it can have "holes", and `typeof []` is `'object'`.

## Key ideas

- **Create arrays with the literal `[...]`;** `Array(3)` creates three empty slots ("holes"), not three `undefined`s.
- **`length` can be changed:** shrinking truncates, writing past the end leaves holes.
- **Tell mutating methods from copying ones:** `sort()` changes the array, `toSorted()` does not.
- **`sort()` without a comparison function sorts as strings:** `[10, 9, 1].sort()` is `[1, 10, 9]`.
- **`reduce` of an empty array without an initial value is a `TypeError`.**
- **Array copies are shallow;** `[1] === [1]` is false.
- **`Set` removes duplicates** (the "SameValueZero" comparison: `NaN` equals `NaN`); `Set` has operations `union`, `intersection` and others.
- **`Map` accepts keys of any type** (including objects and `NaN`) and remembers insertion order.
- **Weak collections** accept only objects as keys, are not iterable and do not prevent garbage collection.
- **Typed arrays** hold numbers of a fixed type and "wrap" values (`256` in a `Uint8Array` is `0`).

## Minimal example

```js
const nums = [3, 1, 2];
console.log(nums.map(n => n * 2), nums.toSorted(), nums);
console.log([...new Set([1, 2, 2, 3])], new Map([['a', 1]]).get('a'));
```

```text
[ 6, 2, 4 ] [ 1, 2, 3 ] [ 3, 1, 2 ]
[ 1, 2, 3 ] 1
```

`toSorted` returned a sorted copy; the original `nums` did not change.

## When to use and when not

| Task | Choice |
|---|---|
| An ordered list | `Array` |
| Unique values | `Set` |
| A dictionary with arbitrary keys | `Map` |
| A dictionary with string keys and JSON | a plain object ([module 05](../05-objects-and-prototypes/overview.en.md)) |
| Metadata attached to objects without memory leaks | `WeakMap` |
| Binary data, bytes | typed arrays |
| Working with a list immutably | `toSorted`, `toReversed`, `with`, `toSpliced`, `map`, `filter` |

- ❌ Do not use `for...in` for arrays: it iterates string keys and "extra" properties.
- ❌ Do not add elements at a huge index: the array becomes "holey".

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | creation, `length` and holes, mutating and copying methods, searching, `map`/`filter`/`reduce`, sorting, iteration, copying |
| [2. Advanced theory](advanced.en.md) | `Set` and set operations, `Map` and comparison with an object, `WeakMap`/`WeakSet`, typed arrays, `ArrayBuffer`, `DataView` |
| [3. Expert theory](expert.en.md) | a reference of methods, SameValueZero semantics, holes, internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 07-arrays-and-collections level 5` |
| 6. Tests | not created yet — `/learn javascript 07-arrays-and-collections level 6` |

## Related materials

- Prerequisites: [02 · Types and values](../02-types-and-values/overview.en.md), [04 · Functions](../04-functions/overview.en.md) (callbacks), [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md) (copying, `groupBy`)
- Next: [08 · Strings, numbers, dates, regular expressions](../08-strings-numbers-dates-regex/overview.en.md)
- Iterators and generators in depth: [11 · Iterators and generators](../11-iterators-and-generators/overview.en.md)
- Analogues in other tracks: [Python: lists, dicts, sets](../../../python/overview.en.md), [C#: collections and LINQ](../../../csharp/overview.en.md)
- Typing arrays and collections: [TypeScript](../../../typescript/overview.en.md)
