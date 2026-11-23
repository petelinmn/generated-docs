---
title: "Exercises: classes"
lang: en
track: javascript
kind: module
doc: exercises
slug: 06-classes
part: 1
tags: [classes, inheritance, private-fields, static, errors, mixins]
status: draft
updated: 2026-11-23
---

# Exercises: classes

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. Run the code in an ES module file.

## 1. 🟢 A class with a getter

**Task:** write a class `Rect` with fields `w` and `h`, a getter `area` and a method `toString()` that returns `'w×h'`.

**Expected result:** for `new Rect(3, 4)` — `12` and `3×4`.

<details><summary>Hint</summary>

`get area() { return this.w * this.h; }`.

</details>

<details><summary>Solution</summary>

```js
class Rect {
  constructor(w, h) { this.w = w; this.h = h; }
  get area() { return this.w * this.h; }
  toString() { return `${this.w}×${this.h}`; }
}
const r = new Rect(3, 4);
console.log(r.area, `${r}`);
```

```text
12 3×4
```

</details>

## 2. 🟢 A private counter

**Task:** a class `Counter` with a private field `#n`, a method `inc()` (returns the new value) and a getter `value`. Check that `Object.keys(c)` is empty.

**Expected result:** `1 2 2 []`.

<details><summary>Hint</summary>

`#n = 0; inc() { return ++this.#n; } get value() { return this.#n; }`.

</details>

<details><summary>Solution</summary>

```js
class Counter {
  #n = 0;
  inc() { return ++this.#n; }
  get value() { return this.#n; }
}
const c = new Counter();
console.log(c.inc(), c.inc(), c.value, Object.keys(c));
```

```text
1 2 2 []
```

</details>

## 3. 🟡 Inheritance and `super`

**Task:** a class `Shape` with a method `describe()` returning `'shape'`; `Circle extends Shape` with a radius `r` overrides `describe()` so that it appends `' circle r=<r>'` to the parent's result.

**Expected result:** `shape circle r=2`.

<details><summary>Hint</summary>

`super.describe() + ' circle r=' + this.r`; do not forget `super()` in the constructor.

</details>

<details><summary>Solution</summary>

```js
class Shape { describe() { return 'shape'; } }
class Circle extends Shape {
  constructor(r) { super(); this.r = r; }
  describe() { return `${super.describe()} circle r=${this.r}`; }
}
console.log(new Circle(2).describe());
```

```text
shape circle r=2
```

</details>

## 4. 🟡 A lost `this`

**Task:** a class `Btn` has a method `click()` that returns `this.label`. Show the error on `const { click } = new Btn('ok'); click();` and fix it two ways: an arrow field and `bind` in the constructor.

**Expected result:** `TypeError: Cannot read properties of undefined (reading 'label')`; then `ok` in both variants.

<details><summary>Hint</summary>

`click = () => this.label;` and `this.click = this.click.bind(this)`.

</details>

<details><summary>Solution</summary>

```js
class Btn { constructor(l) { this.label = l; } click() { return this.label; } }
try { const { click } = new Btn('ok'); click(); } catch (e) { console.log(e.name + ': ' + e.message); }
class Btn2 { constructor(l) { this.label = l; } click = () => this.label; }
class Btn3 { constructor(l) { this.label = l; this.click = this.click.bind(this); } click() { return this.label; } }
console.log(new Btn2('ok').click.call(null), new Btn3('ok').click.call(null));
```

```text
TypeError: Cannot read properties of undefined (reading 'label')
ok ok
```

(`.call(null)` shows that `this` does not depend on the way of calling.)

</details>

## 5. 🔴 An abstract class and a custom error

**Task:** (a) make an abstract class `Base` whose `new Base()` is forbidden while its descendant `Impl` can be created. (b) Write `ValidationError extends Error` with a field `field`; check `e instanceof Error`, `e.name` and `String(e)`.

**Expected result:** (a) `TypeError: abstract`, `true`; (b) `true ValidationError age ValidationError: bad`.

<details><summary>Hint</summary>

(a) `new.target === Base`; (b) `this.name = 'ValidationError'`.

</details>

<details><summary>Solution</summary>

```js
class Base { constructor() { if (new.target === Base) throw new TypeError('abstract'); } }
try { new Base(); } catch (e) { console.log(e.name + ': ' + e.message); }
console.log(new (class Impl extends Base {})() instanceof Base);
class ValidationError extends Error {
  constructor(message, field) { super(message); this.name = 'ValidationError'; this.field = field; }
}
const e = new ValidationError('bad', 'age');
console.log(e instanceof Error, e.name, e.field, String(e));
```

```text
TypeError: abstract
true
true ValidationError age ValidationError: bad
```

</details>
