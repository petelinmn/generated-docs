---
title: "Arrays and collections: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 07-arrays-and-collections
part: 1
tags: [set, map, weakmap, weakset, typed-array, arraybuffer, dataview, set-operations, samevaluezero]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Arrays and collections: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15 in an ES module file.

## Set

A `Set` stores unique values in insertion order:

```js
const s = new Set([1, 2, 2, 3, NaN, NaN, 0, -0]);
console.log(s, s.size, s.has(NaN), new Set([{}, {}]).size);
console.log([...new Set('hello')].join(''), [1, 2, 2, 3].filter((x, i, a) => a.indexOf(x) === i));
```

```text
Set(5) { 1, 2, 3, NaN, 0 } 5 true 2
helo [ 1, 2, 3 ]
```

- The comparison is **SameValueZero**: `NaN` equals `NaN`, and `0` and `-0` are the same value.
- Objects are compared by reference: two empty literals are two elements.
- A quick way to remove duplicates: `[...new Set(arr)]`.

### Set operations

In Node.js 24 `Set` has set-theory methods:

```js
const A = new Set([1, 2, 3]), B = new Set([2, 3, 4]);
console.log(A.union(B), A.intersection(B), A.difference(B), A.symmetricDifference(B));
console.log(A.isSubsetOf(new Set([1, 2, 3, 4])), A.isDisjointFrom(new Set([9])));
```

```text
Set(4) { 1, 2, 3, 4 } Set(2) { 2, 3 } Set(1) { 1 } Set(2) { 1, 4 }
true true
```

The methods return a new set and do not change the originals.

> ⚠️ Verify: older environments (Node.js below 22 and outdated browsers) lack these methods; check support in the compatibility table.

## Map

A `Map` stores "key — value" pairs; a key can be anything:

```js
const mp = new Map([['a', 1], [1, 'one']]);
mp.set({}, 'obj').set(NaN, 'nan');
console.log(mp.size, mp.get('a'), mp.get(NaN), mp.get({}), [...mp.keys()].length);
console.log(new Map(Object.entries({ x: 1 })), Object.fromEntries(new Map([['k', 2]])), [...new Map([[1, 'a'], [2, 'b']])]);
console.log(JSON.stringify(new Map([[1, 2]])), JSON.stringify([...new Map([[1, 2]])]));
const order = new Map(); order.set('b', 1).set('a', 2).set('b', 3);
console.log([...order]);
mp.delete('a'); console.log(mp.has('a'), mp.size); mp.clear(); console.log(mp.size);
console.log(Map.groupBy([1, 2, 3], n => n % 2).get(1));
```

```text
4 1 nan undefined 4
Map(1) { 'x' => 1 } { k: 2 } [ [ 1, 'a' ], [ 2, 'b' ] ]
{} [[1,2]]
[ [ 'b', 3 ], [ 'a', 2 ] ]
false 3
0
[ 1, 3 ]
```

- `set` returns the `Map` itself, so calls can be chained.
- `mp.get({})` returned `undefined`: a new literal is a different key; `NaN` is found.
- A repeated `set('b', 3)` changes the value but not the position: the order stays that of the first insertion.
- Conversions: `new Map(Object.entries(obj))` and `Object.fromEntries(map)`; `JSON.stringify(map)` gives `{}`, so pairs must be expanded.
- `Map.groupBy` groups into a `Map` (keys can be anything).

### Map or a plain object

| Property | `Map` | Object |
|---|---|---|
| Keys | any | strings and symbols |
| Order | insertion order | indexes ascending, then insertion |
| Size | `map.size` | `Object.keys(o).length` |
| Prototype | no inherited keys | yes (`toString`, `__proto__`) |
| JSON | not serialized directly | serialized |
| Frequent additions/removals | designed for it | not optimal |

## WeakMap and WeakSet

Weak collections hold keys "weakly": a key object can be collected by the garbage collector if nothing else refers to it.

```js
const wm = new WeakMap(); const key = {};
wm.set(key, 'meta');
console.log(wm.get(key), wm.has({}), typeof wm.size);
try { wm.set('str', 1) } catch (e) { console.log(e.name + ': ' + e.message) }
try { new WeakSet().add(1) } catch (e) { console.log(e.name + ': ' + e.message) }
try { [...wm] } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
meta false undefined
TypeError: Invalid value used as weak map key
TypeError: Invalid value used in weak set
TypeError: wm is not iterable
```

- Only objects (and non-registered symbols) can be keys: a string is a `TypeError`.
- There is no `size`, iteration or `clear`: the content depends on garbage collection and must not be observable.
- Use: private data and caches attached to an object without memory leaks.

> ⚠️ Verify: the actual freeing of memory (garbage collector behavior, `FinalizationRegistry`, `WeakRef`) was not observed in the experiments; more in module 19 of the roadmap.

## Typed arrays

`Uint8Array`, `Int16Array`, `Float32Array` and others are arrays of numbers of **one fixed type** on top of an `ArrayBuffer`:

```js
const u8 = new Uint8Array([250, 255, 256, -1, 1.9]);
console.log(u8, u8.length, u8.BYTES_PER_ELEMENT);
console.log(new Uint8ClampedArray([300, -5, 1.5]), new Int8Array([127, 128]), new Float32Array([0.1])[0]);
console.log(Array.isArray(u8), u8 instanceof Uint8Array, u8.map(x => x * 2), u8.slice(1, 3), u8.subarray(1, 3).buffer === u8.buffer);
```

```text
Uint8Array(5) [ 250, 255, 0, 255, 1 ] 5 1
Uint8ClampedArray(3) [ 255, 0, 2 ] Int8Array(2) [ 127, -128 ] 0.10000000149011612
false true Uint8Array(5) [ 244, 254, 0, 254, 2 ] Uint8Array(2) [ 255, 0 ] true
```

- Values **"wrap"** modulo the type: `256` → `0`, `-1` → `255`, the fractional part is dropped (`1.9` → `1`); `Int8Array([128])` is `-128`.
- `Uint8ClampedArray` **clamps** into the range `0–255` instead of wrapping (`300` → `255`, `-5` → `0`, `1.5` → `2`).
- `Float32Array` stores a number with less precision (`0.1` → `0.10000000149011612`).
- A typed array is not an `Array` (`Array.isArray` is `false`): it has no `push` or other methods that change the length, and `map` returns an array of the **same type** (values wrap again).
- `subarray` creates a "view" of the same buffer (a shared `buffer`), while `slice` creates a copy.

### ArrayBuffer and DataView

`DataView` reads and writes numbers in a buffer with an explicit byte order:

```js
const buf = new ArrayBuffer(8);
const dv = new DataView(buf);
dv.setInt16(0, 258);
console.log(new Uint8Array(buf, 0, 2), dv.getInt16(0), dv.getInt16(0, true));
```

```text
Uint8Array(2) [ 1, 2 ] 258 513
```

The number `258` (`0x0102`) was written in "most significant byte first" order (big-endian) as the bytes `1, 2`. Reading with the flag `true` interprets the bytes as little-endian and gives `513`. This is needed for binary protocols and files.

## Best practices

- For lists — `Array` and non-mutating methods where the array belongs to someone else.
- For membership and uniqueness checks — `Set` (`has` is faster than `includes`).
- For dictionaries with dynamic keys — `Map`.
- Object metadata — `WeakMap`.
- For bytes — typed arrays and `DataView`.
- Do not mix holes and `undefined`.

> ⚠️ Verify: the claims about speed (`Set.has` is faster than `Array.includes`, `Map` is faster than an object under frequent changes) come from general knowledge and were not measured in the experiments.

## Common mistakes

- ❌ `JSON.stringify(map)` → ✅ `JSON.stringify([...map])` or `Object.fromEntries(map)`.
- ❌ Expecting `set.has({})` for a copy literal → ✅ comparison by reference.
- ❌ Using a string as a `WeakMap` key → ✅ an object.
- ❌ Putting big numbers in a `Uint8Array` and being surprised by wrapping → ✅ the right type or `Uint8ClampedArray`.
- ❌ Expecting order by value in a `Map` → ✅ the order is by insertion.

## Related materials

- Next: [expert theory](expert.en.md)
- Copying and `groupBy`: [05 · Objects and prototypes](../05-objects-and-prototypes/basics.en.md#enumeration-and-conversions)
- Binary data in Node.js: module 16 of the [track](../../overview.en.md) roadmap

## Sources

- [MDN: Set](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Set)
- [MDN: Map](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Map)
- [MDN: WeakMap](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/WeakMap)
- [MDN: TypedArray](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/TypedArray)
- [MDN: DataView](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/DataView)
