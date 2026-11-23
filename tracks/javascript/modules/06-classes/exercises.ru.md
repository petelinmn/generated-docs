---
title: "Упражнения: классы"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 06-classes
part: 1
tags: [classes, inheritance, private-fields, static, errors, mixins]
status: draft
updated: 2026-11-23
---

# Упражнения: классы

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Запускайте код в файле ES-модуля.

## 1. 🟢 Класс с геттером

**Задача:** напишите класс `Rect` с полями `w` и `h`, геттером `area` и методом `toString()`, который возвращает `'w×h'`.

**Ожидаемый результат:** для `new Rect(3, 4)` — `12` и `3×4`.

<details><summary>Подсказка</summary>

`get area() { return this.w * this.h; }`.

</details>

<details><summary>Решение</summary>

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

## 2. 🟢 Приватный счётчик

**Задача:** класс `Counter` с приватным полем `#n`, методами `inc()` (возвращает новое значение) и геттером `value`. Проверьте, что `Object.keys(c)` пуст.

**Ожидаемый результат:** `1 2 2 []`.

<details><summary>Подсказка</summary>

`#n = 0; inc() { return ++this.#n; } get value() { return this.#n; }`.

</details>

<details><summary>Решение</summary>

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

## 3. 🟡 Наследование и `super`

**Задача:** класс `Shape` с методом `describe()` возвращает `'shape'`; `Circle extends Shape` с радиусом `r` переопределяет `describe()` так, чтобы добавить к результату родителя `' circle r=<r>'`.

**Ожидаемый результат:** `shape circle r=2`.

<details><summary>Подсказка</summary>

`super.describe() + ' circle r=' + this.r`; не забудьте `super()` в конструкторе.

</details>

<details><summary>Решение</summary>

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

## 4. 🟡 Потерянный `this`

**Задача:** у класса `Btn` метод `click()` возвращает `this.label`. Покажите ошибку при `const { click } = new Btn('ok'); click();` и исправьте двумя способами: полем со стрелкой и `bind` в конструкторе.

**Ожидаемый результат:** `TypeError: Cannot read properties of undefined (reading 'label')`; затем `ok` в обоих вариантах.

<details><summary>Подсказка</summary>

`click = () => this.label;` и `this.click = this.click.bind(this)`.

</details>

<details><summary>Решение</summary>

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

(`.call(null)` показывает, что `this` не зависит от способа вызова.)

</details>

## 5. 🔴 Абстрактный класс и пользовательская ошибка

**Задача:** (а) сделайте абстрактный класс `Base`, `new Base()` которого запрещён, а наследник `Impl` создаётся. (б) Напишите `ValidationError extends Error` с полем `field`; убедитесь, что `e instanceof Error`, `e.name` и `String(e)`.

**Ожидаемый результат:** (а) `TypeError: abstract`, `true`; (б) `true ValidationError age ValidationError: bad`.

<details><summary>Подсказка</summary>

(а) `new.target === Base`; (б) `this.name = 'ValidationError'`.

</details>

<details><summary>Решение</summary>

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
