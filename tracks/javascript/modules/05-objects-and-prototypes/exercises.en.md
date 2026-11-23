---
title: "Exercises: objects and prototypes"
lang: en
track: javascript
kind: module
doc: exercises
slug: 05-objects-and-prototypes
part: 1
tags: [objects, prototype, destructuring, spread, structured-clone, freeze]
status: draft
updated: 2026-11-23
---

# Exercises: objects and prototypes

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. Run the code in an ES module file.

## 1. 🟢 Destructuring

**Task:** from the object `{ id: 1, user: { name: 'Ann' }, tags: ['a', 'b', 'c'] }` extract `id`, `name`, the first tag and the rest of the tags in one expression. For the missing `role` set the value `'guest'`.

**Expected result:** `1 Ann a [ 'b', 'c' ] guest`.

<details><summary>Hint</summary>

The nested pattern `user: { name }`, the array `tags: [first, ...others]`, the default `role = 'guest'`.

</details>

<details><summary>Solution</summary>

```js
const { id, user: { name }, tags: [first, ...others], role = 'guest' } =
  { id: 1, user: { name: 'Ann' }, tags: ['a', 'b', 'c'] };
console.log(id, name, first, others, role);
```

```text
1 Ann a [ 'b', 'c' ] guest
```

</details>

## 2. 🟢 A frequency dictionary

**Task:** for the string `'abracadabra'` build an object "character → number of occurrences" and sort the pairs in descending order.

**Expected result:** `[ [ 'a', 5 ], [ 'b', 2 ], [ 'r', 2 ], [ 'c', 1 ], [ 'd', 1 ] ]`.

<details><summary>Hint</summary>

`for...of` over the string, `freq[ch] = (freq[ch] ?? 0) + 1`, then `Object.entries(...).sort(...)`.

</details>

<details><summary>Solution</summary>

```js
const freq = {};
for (const ch of 'abracadabra') freq[ch] = (freq[ch] ?? 0) + 1;
console.log(Object.entries(freq).sort((a, b) => b[1] - a[1]));
```

```text
[ [ 'a', 5 ], [ 'b', 2 ], [ 'r', 2 ], [ 'c', 1 ], [ 'd', 1 ] ]
```

The sort is stable, so pairs with equal counts stay in insertion order.

</details>

## 3. 🟡 Shallow and deep copy

**Task:** copy the object `{ n: { v: 1 }, d: new Date(0) }` two ways: `{ ...o }` and `structuredClone`. Change `n.v` in the copies and show which way changed the original.

**Expected result:** with spread the original `n.v` became `99`; with `structuredClone` it stayed `1`.

<details><summary>Hint</summary>

Compare the values after the assignment `copy.n.v = 99`.

</details>

<details><summary>Solution</summary>

```js
const o = { n: { v: 1 }, d: new Date(0) };
const shallow = { ...o }; shallow.n.v = 99;
console.log(o.n.v);
const o2 = { n: { v: 1 }, d: new Date(0) };
const deep = structuredClone(o2); deep.n.v = 99;
console.log(o2.n.v, deep.d instanceof Date);
```

```text
99
1 true
```

</details>

## 4. 🟡 Protecting an object

**Task:** create three objects: frozen, sealed and non-extensible. For each check whether you can change a value, add and delete a property (in strict mode).

**Expected result:** for `freeze` all three operations give an error; for `seal` a value changes while adding and deleting are errors; for `preventExtensions` a value changes and a property deletes, while adding is an error.

<details><summary>Hint</summary>

Wrap the operations in `try`/`catch`.

</details>

<details><summary>Solution</summary>

See the code and output in the [advanced theory](advanced.en.md#freeze-seal-and-preventextensions).

</details>

## 5. 🔴 Prototypes: inheritance without classes

**Task:** create an object `animal` with a method `speak()` and an object `dog` based on it (`Object.create`) with a field `name` and a method `speak()` that calls the prototype's method and appends `' (woof)'`. Show the prototype chain and make sure `speak` is not an own property of `Object.create(animal)`.

**Expected result:** `Rex makes a sound (woof)`, `Object.getPrototypeOf(dog) === animal`, `Object.hasOwn(Object.create(animal), 'speak')` is `false`.

<details><summary>Hint</summary>

Calling the prototype's method: `animal.speak.call(this)` or `Object.getPrototypeOf(dog).speak.call(this)`.

</details>

<details><summary>Solution</summary>

```js
const animal = { speak() { return `${this.name} makes a sound`; } };
const dog = Object.create(animal);
dog.name = 'Rex';
dog.speak = function () { return animal.speak.call(this) + ' (woof)'; };
console.log(dog.speak(), Object.getPrototypeOf(dog) === animal, Object.hasOwn(Object.create(animal), 'speak'));
```

```text
Rex makes a sound (woof) true false
```

In classes there are `extends` and `super` for this (module 06 of the roadmap).

</details>
