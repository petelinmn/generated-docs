---
title: "Arrays and collections: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 07-arrays-and-collections
part: 1
tags: [array-exotic-object, samevaluezero, sort-stability, sparse-arrays, typed-arrays, map-set-ordering, array-methods]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Arrays and collections: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15 (an ES module).

## Reference

### Array methods

| Group | Methods | Change the original array |
|---|---|---|
| Creation | `Array.of`, `Array.from`, `Array(n)`, `[...x]` | — |
| Adding and removing | `push`, `pop`, `shift`, `unshift`, `splice` | yes |
| Rearranging | `sort`, `reverse`, `fill`, `copyWithin` | yes (in the experiment `sort` returned the same array) |
| Copying analogues | `toSorted`, `toReversed`, `toSpliced`, `with` | no |
| Slices and joining | `slice`, `concat`, `join`, `flat`, `flatMap` | no |
| Searching | `indexOf`, `lastIndexOf`, `includes`, `find`, `findIndex`, `findLast`, `findLastIndex`, `at` | no |
| Checks | `some`, `every`, `Array.isArray` | no |
| Traversal | `forEach`, `map`, `filter`, `reduce`, `reduceRight`, `entries`, `keys`, `values` | no |
| Grouping | `Object.groupBy`, `Map.groupBy` (static, not array methods) | no |

> ⚠️ Verify: `push`, `pop`, `shift`, `unshift`, `values`, `lastIndexOf` and `copyWithin` are mentioned in this topic by the documentation; the experiments ran `fill`, `copyWithin`, `lastIndexOf`, `splice`.

### Comparing values: three algorithms

| Operation | Algorithm | `NaN` | `0` and `-0` |
|---|---|---|---|
| `===`, `indexOf`, `lastIndexOf` | strict equality | `NaN !== NaN` | equal |
| `includes`, `Set`, `Map` | SameValueZero | equal | equal (in the experiment `Set` merged `0` and `-0`) |
| `Object.is` | SameValue | equal | different |

In the experiment: `[NaN].indexOf(NaN)` is `-1`, `[NaN].includes(NaN)` is `true`, `new Set([NaN, NaN]).size` is `1`.

### Collections: summary

| | `Array` | `Set` | `Map` | `WeakMap`/`WeakSet` |
|---|---|---|---|---|
| Order | by index | insertion | insertion | none |
| Uniqueness | no | values | keys | keys |
| Key/value type | any | any | any keys | objects only (and unregistered symbols) |
| Iteration | yes | yes | yes | no |
| `size`/`length` | `length` | `size` | `size` | none |

### Typed arrays

`Int8Array`, `Uint8Array`, `Uint8ClampedArray`, `Int16Array`, `Uint16Array`, `Int32Array`, `Uint32Array`, `Float32Array`, `Float64Array`, `BigInt64Array`, `BigUint64Array`. The element size is `BYTES_PER_ELEMENT` (in the experiment `Uint8Array` — `1`).

> ⚠️ Verify: `Float16Array`, `BigInt64Array`, `Atomics` and `SharedArrayBuffer` were not run in the experiments.

## Internals

- **An array is an "exotic" object.** Indexes are ordinary properties with string keys (`'0'`, `'1'`), but `length` behaves specially: writing `length = n` deletes elements with indexes ≥ n (in the experiment `a.length = 1`), and writing at an index ≥ `length` increases `length` (in the experiment `a[4] = 9` → `length` 5).
- **Holes** are missing indexes: `2 in a` is false, `Object.keys` skips them. The methods `map`, `forEach`, `filter` traverse only existing indexes, while `join` and iteration (`for...of`, `...`) go through all of them, turning holes into `undefined`/empty strings (in the experiment `Array(3).join('-')` is `'--'`, and `[...Array(2).keys()]` is `[0, 1]`).
- **Sorting.** `sort` is stable (a specification requirement since ES2019; in the experiment `'bdac'`), by default it compares the results of `String(x)` by UTF-16 code units; `undefined` and holes are always at the end.
- **`Map` and `Set`** per the specification keep entries in insertion order, and updating an existing key does not change its position (in the experiment `order` is `[['b', 3], ['a', 2]]`).
- **Weak collections.** A key does not keep an object from being garbage collected; because of that there is no iteration and no `size` (in the experiment `typeof wm.size` is `'undefined'`, `[...wm]` is a `TypeError`).
- **Typed arrays are views on an `ArrayBuffer`.** Several views can share one buffer; `subarray` does not copy (in the experiment `u8.subarray(1, 3).buffer === u8.buffer`), `slice` copies. Reading and writing convert a number to the element type (`ToUint8`: modulo 256; `Uint8Clamped`: clamping with rounding to the nearest even).
- **`DataView`** reads and writes with an explicit byte order; without the flag — big-endian, with `true` — little-endian (in the experiment `258` and `513`).

> ⚠️ Verify: V8's internal element kinds (`PACKED_SMI`, `HOLEY_DOUBLE` and so on), the effect of holes on speed and the exact sort algorithm (TimSort in V8) come from general knowledge and were not checked in the experiments; details are in module 19 of the roadmap.

## Gotchas

### 1. `sort()` without a comparison function compares strings

`[10, 9, 1].sort()` → `[ 1, 10, 9 ]`.

### 2. `sort`, `reverse`, `splice` change the original array

`const same = m.sort(); same === m` is `true` (in the experiment). Use `toSorted`, `toReversed`, `toSpliced`.

### 3. Holes versus `undefined`

`Array(2).map(() => 1)` → `[ <2 empty items> ]`, while `[undefined, undefined].map(() => 1)` → `[ 1, 1 ]` (in the experiment). `[, 1].forEach` calls the function once.

### 4. `indexOf(NaN)` does not find it

`[NaN].indexOf(NaN)` is `-1`; you need `includes`.

### 5. `reduce` without an initial value

On an empty array — `TypeError: Reduce of empty array with no initial value`.

### 6. `for...in` iterates strings and extras

`['0','1','2','extra']` for an array with an added property (in the experiment).

### 7. `new Array(n)` and a numeric argument

`Array(3)` is three holes, `Array.of(3)` is `[3]`; `new Array(-1)` is `RangeError: Invalid array length`.

### 8. Serializing a `Map` to JSON

`JSON.stringify(map)` gives `{}`: the content of a `Map` and `Set` is lost; you must expand it into an array of pairs or values.

### 9. Typed arrays wrap values

`new Uint8Array([256, -1])` → `[0, 255]`; `Uint8ClampedArray` clamps; `map` keeps the type and wraps again (`500` → `244`).

### 10. `concat` and nesting

`concat` unwraps only array arguments, one level deep: `[[9]]` stays an array inside.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `Array` + methods | universal, many methods | holes, some methods mutate |
| Copying methods (`toSorted`...) | safe for other people's arrays | create a copy (memory) |
| `Set` instead of `includes` on an array | uniqueness and a fast `has` | a different type; not indexable |
| `Map` instead of an object | any keys, order, `size` | no JSON out of the box |
| `WeakMap` | no leaks, private metadata | cannot be enumerated |
| Typed arrays | compact, fast for numbers and bytes | a fixed type, wrapping |
| Persistent structures (libraries) | immutability | an external dependency; not run |

## Interview questions

<details><summary>1. How does <code>Array(3)</code> differ from <code>Array.of(3)</code>?</summary>

`Array(3)` is an array of three empty slots (length 3), `Array.of(3)` is an array of one element `3`.

→ [Basic theory](basics.en.md#creation)

</details>

<details><summary>2. What are holes and how do methods treat them?</summary>

Missing indexes; `map`/`forEach` skip them, `join` and iteration see them as `undefined`.

→ [Internals](#internals)

</details>

<details><summary>3. Trick: what does <code>[10, 9, 1].sort()</code> return?</summary>

`[1, 10, 9]`: by default the sort is string-based.

→ [Gotcha 1](#1-sort-without-a-comparison-function-compares-strings)

</details>

<details><summary>4. Which array methods change it in place?</summary>

`push`, `pop`, `shift`, `unshift`, `splice`, `sort`, `reverse`, `fill`, `copyWithin`. The copying analogues: `toSorted`, `toReversed`, `toSpliced`, `with`.

→ [Reference](#array-methods)

</details>

<details><summary>5. Trick: why is <code>[NaN].indexOf(NaN)</code> <code>-1</code> while <code>includes</code> is <code>true</code>?</summary>

`indexOf` uses strict equality (`NaN !== NaN`), `includes` uses SameValueZero.

→ [Reference](#comparing-values-three-algorithms)

</details>

<details><summary>6. How do <code>Set</code> and <code>Map</code> differ from an array and an object?</summary>

`Set` is unique values, `Map` is any keys with insertion order and `size`; both use SameValueZero comparison.

→ [Advanced theory](advanced.en.md#set)

</details>

<details><summary>7. Why is <code>WeakMap</code> needed and why is it not iterable?</summary>

It holds data attached to key objects without hindering garbage collection; the content depends on GC, so it must not be observable by iteration.

→ [Advanced theory](advanced.en.md#weakmap-and-weakset)

</details>

<details><summary>8. What happens on <code>new Uint8Array([256, -1])</code>?</summary>

The values wrap modulo 256: `[0, 255]`.

→ [Advanced theory](advanced.en.md#typed-arrays)

</details>

<details><summary>9. How do you remove duplicates from an array?</summary>

`[...new Set(arr)]`; for objects — by a key through a `Map`.

→ [Advanced theory](advanced.en.md#set)

</details>

<details><summary>10. Trick: how do you sort numbers correctly?</summary>

`arr.sort((a, b) => a - b)` or `toSorted` with the same function.

→ [Basic theory](basics.en.md#sorting)

</details>

<details><summary>11. What does <code>reduce</code> return on an empty array?</summary>

Without an initial value — a `TypeError`; with one — that value.

→ [Gotcha 5](#5-reduce-without-an-initial-value)

</details>

<details><summary>12. How do you serialize a <code>Map</code> to JSON?</summary>

`JSON.stringify([...map])` or `Object.fromEntries(map)` (for string keys).

→ [Gotcha 8](#8-serializing-a-map-to-json)

</details>

## Cheat sheet

```text
[...a]  a.slice()  structuredClone(a)       a.toSorted()  a.toReversed()  a.with(i, v)  a.toSpliced(i, n)
a.sort((x, y) => x - y)                      a.reduce((acc, x) => …, init)
a.find / findLast / includes / at(-1)        Object.groupBy(a, f)   Map.groupBy(a, f)
new Set(a)  [...set]  s.union / intersection / difference
new Map(entries)  map.get / set / has / delete / size
new WeakMap()  new Uint8Array(buf)  new DataView(buf)
```

- The literal `[…]`; avoid holes.
- Sort numbers with a comparison function.
- Copies are shallow.
- Duplicates — `Set`; a dictionary — `Map`; metadata — `WeakMap`.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Value types: [02 · Types and values](../02-types-and-values/overview.en.md)
- Objects and copying: [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md)
- Iterators and performance — modules 11 and 19 of the [track](../../overview.en.md) roadmap

## Sources

- [ECMAScript: Array Objects](https://tc39.es/ecma262/#sec-array-objects)
- [ECMAScript: SameValueZero](https://tc39.es/ecma262/#sec-samevaluezero)
- [ECMAScript: Keyed Collections](https://tc39.es/ecma262/#sec-keyed-collection)
- [ECMAScript: TypedArray Objects](https://tc39.es/ecma262/#sec-typedarray-objects)
- [MDN: Array.prototype.toSorted()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/toSorted)
