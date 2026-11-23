---
title: "Arrays and collections: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 07-arrays-and-collections
part: 1
tags: [array, array-methods, sort, reduce, flat, array-holes, iteration, array-from]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Arrays and collections: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · Functions](../04-functions/overview.en.md), [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md) · **Time:** ~3 h

All examples were run on Node.js 24.15 in an ES module file.

## Why it matters

Lists of data appear in every program: API responses, table rows, events. Arrays and their methods are the main tool for processing them.

## Creation

```js
console.log(Array.of(3), Array(3), new Array(3).fill(0), Array.from({ length: 3 }, (_, i) => i * i));
console.log(Array.from('héy'), Array.from(new Set([1, 1, 2])), Array.isArray([]), typeof []);
try { new Array(-1) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
[ 3 ] [ <3 empty items> ] [ 0, 0, 0 ] [ 0, 1, 4 ]
[ 'h', 'é', 'y' ] [ 1, 2 ] true object
RangeError: Invalid array length
```

- The best way is the literal `[1, 2, 3]`.
- `Array(3)` is three **empty slots** ("holes"), not three `undefined`s; `Array.of(3)` is an array of one element `3`. The difference is easy to forget.
- `Array.from(source, function)` builds an array from an iterable or an "array-like" `{ length: n }` and transforms the elements at once.
- To check for an array use `Array.isArray(x)`, not `typeof` (it gives `'object'`).

## length and "holes"

```js
const a = [1, 2, 3]; a.length = 1;
console.log(a);
a[4] = 9;
console.log(a, a.length, 2 in a, a.indexOf(undefined), a.includes(undefined));
console.log(Object.keys([1, , 3]), [1, , 3].map(x => x * 2));
[, 1].forEach(x => console.log('visited', x));
```

```text
[ 1 ]
[ 1, <3 empty items>, 9 ] 5 false -1 true
[ '0', '2' ] [ 2, <1 empty item>, 6 ]
visited 1
```

- Assigning `length = 1` **truncates** the array.
- Writing `a[4] = 9` past the end increases `length` to `5` and leaves "holes": there is no index `2` in the array (`2 in a` is `false`).
- `indexOf(undefined)` does not find holes (`-1`), while `includes(undefined)` does (`true`).
- `map` and `forEach` **skip** holes (`map` keeps them in the result); `Object.keys` does not see them.

Better to avoid holes: create an array of the needed length with `Array.from({ length: n }, ...)` or `.fill()`.

## Mutating and copying methods

Some methods change the original array, others return a new one:

```js
let m = [3, 1, 2];
const sorted = m.toSorted();
console.log(sorted, m);
const same = m.sort();
console.log(same === m, m);
console.log(m.toReversed(), m);
console.log(m.with(0, 99), m, m.toSpliced(1, 1), m);
const removed = m.splice(1, 1);
console.log(removed, m);
console.log(m.slice(-2), m.concat([7], 8, [[9]]));
```

```text
[ 1, 2, 3 ] [ 3, 1, 2 ]
true [ 1, 2, 3 ]
[ 3, 2, 1 ] [ 1, 2, 3 ]
[ 99, 2, 3 ] [ 1, 2, 3 ] [ 1, 3 ] [ 1, 2, 3 ]
[ 2 ] [ 1, 3 ]
[ 1, 3 ] [ 1, 3, 7, 8, [ 9 ] ]
```

| Mutates the array | Returns a copy |
|---|---|
| `sort`, `reverse`, `splice`, `fill`, `push`, `pop`, `shift`, `unshift`, `copyWithin` | `toSorted`, `toReversed`, `toSpliced`, `with`, `slice`, `concat`, `map`, `filter`, `flat` |

- `sort()` returns the same array (`same === m`), so its result is not a copy.
- `splice(start, count, ...insert)` deletes and inserts, returning the removed elements.
- `slice(start, end)` does not change the array; negative indexes count from the end.
- `concat` "unwraps" array arguments one level (`[[9]]` gave `[ 9 ]` as a single element).

Non-mutating methods are handy when the array belongs to other code (for example, was passed into a function).

## Searching

```js
const people = [{ n: 'a', age: 20 }, { n: 'b', age: 30 }, { n: 'c', age: 30 }];
console.log(people.find(p => p.age === 30).n, people.findLast(p => p.age === 30).n, people.findIndex(p => p.age > 99), people.findLastIndex(p => p.age === 30));
console.log([1, 2, 3].includes(2), [NaN].includes(NaN), [NaN].indexOf(NaN), [1, 2, 3].at(-1), [1, 2, 3].some(x => x > 2), [].every(x => false), [].some(x => true));
```

```text
b c -1 2
true true -1 3 true true false
```

- `find`/`findLast` return an element (or `undefined`), `findIndex`/`findLastIndex` an index (or `-1`).
- `includes` finds `NaN`, while `indexOf` does not (different comparison algorithms).
- `at(-1)` is the last element.
- `some` and `every` on an empty array give `false` and `true` respectively.

## map, filter, reduce, flat

```js
console.log([1, 2, 3].map(x => x * 2), [1, 2, 3, 4].filter(x => x % 2), [1, 2, 3].reduce((s, x) => s + x));
console.log([[1, 2], [3, [4]]].flat(), [[1, 2], [3, [4]]].flat(Infinity), [1, 2].flatMap(x => [x, x * 10]));
console.log([].reduce((a, b) => a + b, 'init'), [1, 2, 3].reduceRight((a, x) => a + x, ''));
try { [].reduce((a, b) => a + b) } catch (e) { console.log(e.name + ': ' + e.message) }
console.log(Object.groupBy([1, 2, 3, 4, 5], n => (n % 2 ? 'odd' : 'even')));
```

```text
[ 2, 4, 6 ] [ 1, 3 ] 6
[ 1, 2, 3, [ 4 ] ] [ 1, 2, 3, 4 ] [ 1, 10, 2, 20 ]
init 321
TypeError: Reduce of empty array with no initial value
[Object: null prototype] { odd: [ 1, 3, 5 ], even: [ 2, 4 ] }
```

- `map` transforms each element; `filter` keeps the matching ones; `reduce(function, initial)` folds the array into one value.
- `flat(depth)` "flattens" nesting (`Infinity` — completely); `flatMap` = `map` + `flat(1)`.
- `reduce` on an empty array without an initial value is a `TypeError`: **always give an initial value**.
- `reduceRight` goes from right to left.

## Sorting

```js
console.log([10, 9, 1].sort(), [10, 9, 1].sort((a, b) => a - b));
console.log(['b', 'a', 'C'].sort(), ['b', 'a', 'C'].sort((x, y) => x.localeCompare(y)), [3, undefined, 1, , 2].sort());
const st = [{ k: 1, v: 'a' }, { k: 0, v: 'b' }, { k: 1, v: 'c' }, { k: 0, v: 'd' }];
console.log(st.sort((x, y) => x.k - y.k).map(o => o.v).join(''));
```

```text
[ 1, 10, 9 ] [ 1, 9, 10 ]
[ 'C', 'a', 'b' ] [ 'a', 'b', 'C' ] [ 1, 2, 3, undefined, <1 empty item> ]
bdac
```

- Without a comparison function `sort` compares the **string representations**: `10` goes before `9`.
- The comparison function `(a, b) => a - b` returns a negative number, zero or a positive number.
- Strings are sorted by character codes by default (`'C'` before `'a'`); for a "human" order use `localeCompare`.
- `undefined` and holes go to the end.
- The sort is **stable**: elements with an equal key keep the original order (`'bd'` and `'ac'` — as they were).

## Iteration

```js
for (const [i, v] of ['x', 'y'].entries()) console.log(i, v);
console.log([...['a', 'b'].keys()]);
const arr = [1, 2, 3]; arr.extra = 'e';
const ks = []; for (const i in arr) ks.push(i);
console.log(ks, arr.length);
```

```text
0 x
1 y
[ 0, 1 ]
[ '0', '1', '2', 'extra' ] 3
```

- `for...of` iterates values, `entries()` gives `[index, value]` pairs, `keys()` gives indexes.
- `for...in` iterates **string keys** and includes added properties (`'extra'`): it is not used for arrays.

## Copying and comparing

```js
const o = [{ x: 1 }]; const c = [...o]; c[0].x = 2;
console.log(o[0].x, [1] === [1], JSON.stringify([1, 2]) === JSON.stringify([1, 2]));
function f() { return Array.from(arguments).map(x => x * 2); }
console.log(f(1, 2), Array.prototype.map.call('abc', c => c.toUpperCase()), Array.from({ 0: 'a', 1: 'b', length: 2 }));
```

```text
2 false true
[ 2, 4 ] [ 'A', 'B', 'C' ] [ 'a', 'b' ]
```

- `[...o]` and `slice()` are **shallow** copies: the nested object is shared (`o[0].x` became `2`); for a deep copy use `structuredClone`.
- Arrays are compared by reference; comparing content is element by element (`JSON.stringify` does not work for all values, [module 05](../05-objects-and-prototypes/advanced.en.md#deep-copying)).
- Array methods work with "array-like" objects (`arguments`, a string, `{ length: 2, 0: ..., 1: ... }`) through `Array.from` or `call`.

## Beginner mistakes

- ❌ `Array(3)` instead of `[3]` or `Array.of(3)` → ✅ remember the holes.
- ❌ `[10, 9, 1].sort()` for numbers → ✅ `sort((a, b) => a - b)`.
- ❌ `sort()` on an array that must not be changed → ✅ `toSorted()`.
- ❌ `reduce` without an initial value → ✅ give one.
- ❌ `for...in` over an array → ✅ `for...of`.
- ❌ Treating `[...arr]` as a deep copy → ✅ `structuredClone`.
- ❌ Looking for `NaN` through `indexOf` → ✅ `includes`.

## Related materials

- Next: [advanced theory](advanced.en.md) — `Set`, `Map`, weak collections, typed arrays
- Earlier: [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md) (`Object.groupBy`, `structuredClone`)

## Sources

- [MDN: Array](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array)
- [MDN: Indexed collections](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Indexed_collections)
- [MDN: Array.prototype.sort()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/sort)
- [MDN: Sparse arrays](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array#sparse_arrays)
- [ECMAScript: Array Objects](https://tc39.es/ecma262/#sec-array-objects)
