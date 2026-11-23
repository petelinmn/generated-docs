---
title: "Классы: базовая теория"
lang: ru
track: javascript
kind: module
doc: basics
slug: 06-classes
part: 1
tags: [classes, constructor, methods, getters-setters, private-fields, static, inheritance, super]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Классы: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · Функции](../04-functions/overview.ru.md), [05 · Объекты и прототипы](../05-objects-and-prototypes/overview.ru.md) · **Время:** ~2,5 ч

Все примеры выполнены на Node.js 24.15 в файле ES-модуля (строгий режим).

## Зачем это нужно

Когда нужно много однотипных объектов со своим состоянием и поведением, удобно описать их шаблоном. Класс объединяет конструктор, методы и данные в одном месте и скрывает внутренности.

## Объявление класса

```js
class Point {
  static count = 0;
  #secret = 42;
  x = 0;
  constructor(x, y) { this.x = x; this.y = y; Point.count++; }
  get len() { return Math.hypot(this.x, this.y); }
  set len(v) { throw new Error('read-only'); }
  toString() { return `(${this.x}, ${this.y})`; }
  reveal() { return this.#secret; }
  static origin() { return new Point(0, 0); }
}
const p = new Point(3, 4);
console.log(p.len, `${p}`, p.reveal(), Point.count, Point.origin().x);
console.log(typeof Point, Object.getOwnPropertyNames(Point.prototype), Object.keys(p));
try { p.len = 1; } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
5 (3, 4) 42 1 0
function [ 'constructor', 'len', 'toString', 'reveal' ] [ 'x', 'y' ]
Error: read-only
```

Какие элементы здесь есть:

- **`constructor`** вызывается при `new` и задаёт начальное состояние. Если конструктор не нужен, его можно опустить.
- **Метод** (`toString`, `reveal`) лежит на `Point.prototype`, а не в каждом экземпляре.
- **Геттер/сеттер** (`get len`, `set len`) выглядят как свойство, но вызывают функцию. У `len` сеттер выбрасывает ошибку: свойство «только для чтения».
- **Поле экземпляра** (`x = 0;`) создаётся для каждого объекта до тела конструктора; `x` и `y` попадают в `Object.keys(p)`.
- **Приватное поле** `#secret` доступно только внутри тела класса и не видно в `Object.keys`.
- **Статический член** (`static count`, `static origin()`) принадлежит классу: `Point.count`. `Point.count` в выводе равен `1`: аргументы `console.log` вычисляются слева направо, и счётчик прочитан до вызова `Point.origin()` (он создаёт второй экземпляр).

Класс — это функция: `typeof Point` равен `'function'`. Методы класса не перечисляемы, их не видно в `Object.keys(Point.prototype)` (результат — пустой список).

## Класс нельзя вызвать без `new`

```js
try { Point(); } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
TypeError: Class constructor Point cannot be invoked without 'new'
```

Функция-конструктор без `new` молча делала `this` глобальным или падала позже; класс отказывает сразу и явно.

## Класс не всплывает

```js
try { new Later(); } catch (e) { console.log(e.name + ': ' + e.message); }
class Later {}
```

```text
ReferenceError: Cannot access 'Later' before initialization
```

Как и `let`/`const`, класс находится в TDZ до своего объявления ([модуль 03](../03-scopes-and-hoisting/basics.ru.md#временная-мёртвая-зона-tdz)).

## Приватные члены

Имя с `#` видно только внутри тела класса:

```js
const p = new Point(3, 4);
console.log(p['#secret'], Object.getOwnPropertyNames(p), JSON.stringify(p));
```

```text
undefined [ 'x', 'y' ] {"x":3,"y":4}
```

- Обращение `p.#secret` вне класса — **синтаксическая ошибка** (`SyntaxError: Private field '#secret' must be declared in an enclosing class`): программа не запустится вовсе.
- `p['#secret']` — это обычное строковое имя, которого у объекта нет.
- Приватные поля не попадают ни в `getOwnPropertyNames`, ни в `JSON.stringify`.

## Наследование: extends и super

```js
class Animal {
  constructor(name) { this.name = name; }
  speak() { return `${this.name} makes a sound`; }
  static create(n) { return new this(n); }
  get kind() { return 'animal'; }
}
class Dog extends Animal {
  constructor(name) { super(name); this.tricks = []; }
  speak() { return super.speak() + ' (woof)'; }
  get kind() { return 'dog/' + super.kind; }
}
const d = new Dog('Rex');
console.log(d.speak(), d.kind, d instanceof Animal, Object.getPrototypeOf(Dog) === Animal, Dog.create('Fido') instanceof Dog);
```

```text
Rex makes a sound (woof) dog/animal true true true
```

- `class Dog extends Animal` связывает прототипы: `Dog.prototype` наследует `Animal.prototype`, а сам `Dog` — `Animal` (статические члены наследуются: `Dog.create` работает, а `new this(n)` создаёт именно `Dog`).
- `super(name)` вызывает конструктор родителя; `super.speak()` и `super.kind` обращаются к методу и геттеру родителя.
- В конструкторе наследника `this` недоступен, пока не вызван `super(...)`:

```js
class Bad extends Animal { constructor() { this.x = 1; } }
try { new Bad(); } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
ReferenceError: Must call super constructor in derived class before accessing 'this' or returning from derived constructor
```

Та же ошибка, если конструктор наследника пуст и не вызывает `super`.

## Ошибки новичка

- ❌ Вызывать класс без `new` → ✅ `new Point()`.
- ❌ Использовать `this` в конструкторе наследника до `super()` → ✅ `super(...)` первой строкой.
- ❌ Обращаться к `obj.#x` снаружи → ✅ геттер или метод.
- ❌ Ждать, что методы попадут в `Object.keys` → ✅ они неперечисляемы, а поля — перечисляемы.
- ❌ Класть изменяемое состояние в `static`, когда нужно в экземпляр → ✅ поле экземпляра.
- ❌ Строить глубокие цепочки `extends` → ✅ композиция.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — приватные методы, порядок полей, встроенные классы, mixins
- Ранее: [05 · Объекты и прототипы](../05-objects-and-prototypes/overview.ru.md), [04 · Функции](../04-functions/overview.ru.md#функции-конструкторы-и-new)

## Источники

- [MDN: Classes](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes)
- [MDN: Private elements](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/Private_elements)
- [MDN: extends](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/extends)
- [MDN: static](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/static)
- [ECMAScript: Class Definitions](https://tc39.es/ecma262/#sec-class-definitions)
