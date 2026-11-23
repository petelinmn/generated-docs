---
title: "Exercises: arrays and collections"
lang: en
track: javascript
kind: module
doc: exercises
slug: 07-arrays-and-collections
part: 1
tags: [array, set, map, immutability, lru-cache, sort]
status: draft
updated: 2026-11-23
---

# Exercises: arrays and collections

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. Run the code in an ES module file.

## 1. 🟢 Unique and sorted

**Task:** from the array `[5, 3, 5, 10, 1, 3]` get the unique numbers sorted ascending without changing the original array.

**Expected result:** `[ 1, 3, 5, 10 ]`, the original array unchanged.

<details><summary>Hint</summary>

`Set` removes duplicates, `toSorted` with a numeric comparison function does not change the source.

</details>

<details><summary>Solution</summary>

```js
const src = [5, 3, 5, 10, 1, 3];
const result = [...new Set(src)].toSorted((a, b) => a - b);
console.log(result, src);
```

```text
[ 1, 3, 5, 10 ] [ 5, 3, 5, 10, 1, 3 ]
```

</details>

## 2. 🟢 Word frequency

**Task:** for the string `'to be or not to be'` count how many times each word occurs with a `Map`, and print the pairs in descending order of frequency.

**Expected result:** `[ [ 'to', 2 ], [ 'be', 2 ], [ 'or', 1 ], [ 'not', 1 ] ]`.

<details><summary>Hint</summary>

`counts.set(w, (counts.get(w) ?? 0) + 1)`, then `[...counts].sort((a, b) => b[1] - a[1])` (the sort is stable).

</details>

<details><summary>Solution</summary>

```js
const counts = new Map();
for (const w of 'to be or not to be'.split(' ')) counts.set(w, (counts.get(w) ?? 0) + 1);
console.log([...counts].sort((a, b) => b[1] - a[1]));
```

```text
[ [ 'to', 2 ], [ 'be', 2 ], [ 'or', 1 ], [ 'not', 1 ] ]
```

</details>

## 3. 🟡 Immutable updates

**Task:** for the list `items = ['a', 'b', 'c']`, without changing the original array get: the element at index 1 replaced with `'X'`; the list without the element at index 0; the list with `'Z'` inserted at position 1.

**Expected result:** `[ 'a', 'X', 'c' ]`, `[ 'b', 'c' ]`, `[ 'a', 'Z', 'b', 'c' ]`, the original unchanged.

<details><summary>Hint</summary>

`with`, `toSpliced`.

</details>

<details><summary>Solution</summary>

```js
const items = ['a', 'b', 'c'];
console.log(items.with(1, 'X'), items.toSpliced(0, 1), items.toSpliced(1, 0, 'Z'), items);
```

```text
[ 'a', 'X', 'c' ] [ 'b', 'c' ] [ 'a', 'Z', 'b', 'c' ] [ 'a', 'b', 'c' ]
```

</details>

## 4. 🟡 Set operations

**Task:** given the tags of two articles `a = ['js', 'node', 'web']`, `b = ['web', 'css', 'js']`. Find the common tags, the tags only of the first article and all tags without repeats.

**Expected result:** `[ 'js', 'web' ]`, `[ 'node' ]`, `[ 'js', 'node', 'web', 'css' ]`.

<details><summary>Hint</summary>

`intersection`, `difference`, `union` of `Set`.

</details>

<details><summary>Solution</summary>

```js
const A = new Set(['js', 'node', 'web']), B = new Set(['web', 'css', 'js']);
console.log([...A.intersection(B)], [...A.difference(B)], [...A.union(B)]);
```

```text
[ 'js', 'web' ] [ 'node' ] [ 'js', 'node', 'web', 'css' ]
```

</details>

## 5. 🔴 An LRU cache on a Map

**Task:** write a class `Lru` with capacity `cap` and methods `get(k)` and `set(k, v)`. Recently used keys must live longer: on overflow the least recently used one is removed. Use the insertion order of `Map`.

**Expected result:** with `cap = 2`: `set a`, `set b`, `get a`, `set c` — the key `b` is evicted, `a` and `c` remain.

<details><summary>Hint</summary>

On `get` delete and re-insert the key so it becomes the last; the oldest is the first: `this.map.keys().next().value`.

</details>

<details><summary>Solution</summary>

```js
class Lru {
  #cap; #map = new Map();
  constructor(cap) { this.#cap = cap; }
  get(k) {
    if (!this.#map.has(k)) return undefined;
    const v = this.#map.get(k);
    this.#map.delete(k); this.#map.set(k, v);
    return v;
  }
  set(k, v) {
    this.#map.delete(k); this.#map.set(k, v);
    if (this.#map.size > this.#cap) this.#map.delete(this.#map.keys().next().value);
    return this;
  }
  keys() { return [...this.#map.keys()]; }
}
const c = new Lru(2);
c.set('a', 1).set('b', 2); c.get('a'); c.set('c', 3);
console.log(c.keys(), c.get('b'));
```

```text
[ 'a', 'c' ] undefined
```

</details>
