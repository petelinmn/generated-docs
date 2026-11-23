---
title: "Классы: продвинутая теория"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 06-classes
part: 1
tags: [private-methods, static-blocks, field-initialization, this-binding, extending-builtins, mixins, new-target, symbol-hasinstance]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Классы: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры выполнены на Node.js 24.15 в файле ES-модуля (строгий режим).

## Приватные методы, статические приватные члены и блоки

```js
class Point {
  static count = 0;
  #secret = 42;
  constructor(x, y) { this.x = x; this.y = y; Point.count++; }
  #priv() { return 'p'; }
  callPriv() { return this.#priv(); }
  static #sp = 'static private';
  static getSp() { return Point.#sp; }
  static { Point.initialized = true; }
  has(o) { return #secret in o; }
}
const p = new Point(3, 4);
console.log(p.callPriv(), Point.getSp(), Point.initialized, p.has(p), p.has({}));
```

```text
p static private true true false
```

- `#priv()` — приватный метод; `static #sp` — приватное статическое поле (доступ через имя класса внутри его тела).
- `static { ... }` — **статический блок**: код, выполняемый один раз при создании класса; полезен для сложной инициализации статических полей.
- `#secret in o` — **проверка «бренда»**: истинна, если объект создан этим классом и содержит приватное поле; не падает для чужих объектов, в отличие от обращения `o.#secret`.

Обращение `p.#secret` вне класса не запускается из-за синтаксической ошибки: `SyntaxError: Private field '#secret' must be declared in an enclosing class`.

## Порядок инициализации полей

Поля наследника создаются **после** возврата из `super()`. Это даёт ловушку, если конструктор родителя вызывает переопределяемый метод:

```js
class Base { constructor() { this.init(); } init() { this.a = 'base'; } }
class Derived extends Base {
  b = 'field';
  init() { this.a = 'derived'; this.seenB = this.b; }
}
console.log(new Derived());
```

```text
Derived { a: 'derived', seenB: undefined, b: 'field' }
```

Метод `init` наследника был вызван из конструктора `Base`, когда поле `b` ещё не создано, поэтому `this.b` равно `undefined`. Не вызывайте переопределяемые методы из конструктора.

## Потеря `this` и поля-стрелки

Методы лежат на прототипе, а не в экземпляре, поэтому при передаче как значения они теряют `this`:

```js
class F { arrow = () => this.v; v = 1; method() { return this.v; } }
const f = new F();
const { arrow, method } = f;
console.log(arrow(), Object.hasOwn(f, 'arrow'), Object.hasOwn(f, 'method'));
try { method(); } catch (e) { console.log(e.name + ': ' + e.message); }
class C { constructor() { this.onClick = this.onClick.bind(this); } onClick() { return this instanceof C; } }
const { onClick } = new C();
console.log(onClick());
```

```text
1 true false
TypeError: Cannot read properties of undefined (reading 'v')
true
```

- **Поле со стрелкой** (`arrow = () => this.v`) всегда видит экземпляр, но создаётся заново в **каждом** объекте (`hasOwn(f, 'arrow')` — `true`), а метод — нет.
- **`bind` в конструкторе** — другой способ: собственное свойство-копия, привязанная к экземпляру.
- Для обработчиков событий и обратных вызовов используйте один из этих приёмов.

## Расширение встроенных классов

```js
class Stack extends Array { peek() { return this.at(-1); } }
const s = Stack.from([1, 2, 3]);
console.log(s.peek(), s instanceof Array, s.map(x => x) instanceof Stack, s.length, Array.isArray(s));
class MyErr extends Error { constructor(m, code) { super(m); this.name = 'MyErr'; this.code = code; } }
const e = new MyErr('boom', 7);
console.log(e instanceof Error, e.name, e.message, e.code, String(e));
```

```text
3 true true 3 true
true MyErr boom 7 MyErr: boom
```

- Подкласс `Array` получает все методы массива, а `map` возвращает экземпляр `Stack` (используется «видовой» конструктор).
- Пользовательская ошибка — наследник `Error`: вызовите `super(message)`, задайте `name` и дополнительные поля.

## Примеси (mixins)

Когда нужно добавить поведение без наследования, используют функцию, возвращающую класс:

```js
const Serializable = (B) => class extends B { toJSON() { return { ...this }; } };
class User { constructor(n) { this.n = n; } }
class SU extends Serializable(User) {}
console.log(JSON.stringify(new SU('a')), SU.name);
```

```text
{"n":"a"} SU
```

`Serializable(User)` создаёт промежуточный класс между `SU` и `User`. Такой приём разрешает множественное «подмешивание», которого нет у `extends` (только один родитель).

## new.target и абстрактные классы

```js
class Abs { constructor() { if (new.target === Abs) throw new TypeError('abstract'); } }
try { new Abs(); } catch (e) { console.log(e.name + ': ' + e.message); }
console.log(new (class Impl extends Abs {})() instanceof Abs);
```

```text
TypeError: abstract
true
```

В конструкторе `new.target` — класс, на котором вызван `new`. Так запрещают прямое создание «абстрактного» базового класса ([модуль 04](../04-functions/advanced.ru.md#newtarget)).

## Настройка `instanceof` и `toString`

```js
class Even { static [Symbol.hasInstance](n) { return n % 2 === 0; } }
console.log(2 instanceof Even, 3 instanceof Even);
class Tagged { get [Symbol.toStringTag]() { return 'T'; } }
console.log(String(new Tagged()));
```

```text
true false
[object T]
```

`Symbol.hasInstance` переопределяет `instanceof`, а `Symbol.toStringTag` — результат `Object.prototype.toString` и `String(obj)`. Это «известные символы» (метапрограммирование, модуль 18 дорожной карты).

## Статическое наследование и выражения классов

```js
class A { static s = 'A.s'; static who() { return this.name; } }
class B extends A {}
console.log(B.s, B.who(), Object.hasOwn(B, 's'));
const E = class Named {};
console.log(E.name, typeof Named);
```

```text
A.s B false
Named undefined
```

- Статические члены наследуются через прототип конструкторов: `B.s` читается у `A`, у `B` собственного `s` нет. Внутри статического метода `this` — тот класс, у которого его вызвали (`B.who()` — `'B'`).
- У выражения класса имя `Named` видно только внутри класса; снаружи `typeof Named` равен `'undefined'`, имя переменной — `E`.

## Лучшие практики

- Предпочитайте композицию и небольшие иерархии (один-два уровня).
- Состояние делайте приватным (`#`), открывайте методами и геттерами.
- Не вызывайте переопределяемые методы из конструктора.
- Для обработчиков — поле со стрелкой или `bind`.
- Абстрактность — через `new.target`; для интерфейсов — договорённость или TypeScript.
- Ошибки — наследники `Error` с полем `name`/`code`.

## Частые ошибки

- ❌ Вызвать переопределяемый метод в конструкторе родителя → ✅ вынести инициализацию в явный метод.
- ❌ Передать метод как обратный вызов без привязки → ✅ поле-стрелка или `bind`.
- ❌ Обращаться к `#x` чужого объекта без проверки → ✅ `#x in obj`.
- ❌ Ожидать копию статического поля у наследника → ✅ оно общее и читается через цепочку.
- ❌ Строить множественное наследование → ✅ примеси или композиция.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- `this`, `bind`, `new.target`: [04 · Функции](../04-functions/advanced.ru.md)
- Прототипы: [05 · Объекты и прототипы](../05-objects-and-prototypes/advanced.ru.md)

## Источники

- [MDN: Class static initialization blocks](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/Static_initialization_blocks)
- [MDN: Private elements](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/Private_elements)
- [MDN: Mix-ins](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/extends#mix-ins)
- [MDN: Symbol.hasInstance](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Symbol/hasInstance)
- [MDN: Symbol.toStringTag](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Symbol/toStringTag)
