---
title: "Функции: продвинутая теория"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 04-functions
part: 1
tags: [call, apply, bind, partial-application, arrow-functions, constructor-function, new, new-target, function-properties]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Функции: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры выполнены на Node.js 24.15 в файле ES-модуля; где указано, использован нестрогий `.cjs`.

## call, apply и bind

Эти методы функции позволяют явно задать `this`:

```js
const obj = { name: 'obj', m() { return this?.name; } };
function strictThis() { return this; }
console.log(strictThis(), strictThis.call(5), strictThis.call(null));
console.log(obj.m.call({ name: 'other' }), obj.m.apply({ name: 'applied' }, []));
const bound = obj.m.bind({ name: 'bound' });
console.log(bound(), bound.call({ name: 'x' }), bound.name, bound.length);
```

```text
undefined 5 null
other applied
bound bound bound m 0
```

- `f.call(thisArg, a, b)` вызывает `f` сразу, аргументы перечисляются; `f.apply(thisArg, [a, b])` — то же, но аргументы массивом (сегодня чаще пишут `f(...args)`).
- `f.bind(thisArg, …)` **возвращает новую функцию** с зафиксированным `this`.
- В строгом режиме `this` не оборачивается: `strictThis.call(5)` вернул примитив `5`, а `call(null)` — `null`. В нестрогом `.cjs`: `call(5)` даёт объект-обёртку (`typeof this` — `'object'`), а `call(null)` — глобальный объект.
- Привязанную функцию нельзя перепривязать: `bound.call({ name: 'x' })` всё равно вернул `'bound'`. Имя такой функции — `'bound m'`.

### Частичное применение

`bind` фиксирует и первые аргументы:

```js
function add(a, b, c) { return a + b + c; }
const add5 = add.bind(null, 5);
console.log(add5(1, 2), add5.length);
```

```text
8 2
```

`add5` — функция с оставшимися двумя параметрами (`length` равен `2`).

## Ограничения стрелочных функций

Стрелка не имеет собственных `this`, `arguments`, `new.target` и не может быть конструктором:

```js
const arrow = (a, b) => a + b;
try { new arrow() } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
TypeError: arrow is not a constructor
```

- `call`, `apply` и `bind` не меняют `this` стрелки: он берётся из места определения.
- Поэтому стрелки хороши для обратных вызовов внутри методов (`this` сохраняется), а методы объектов лучше записывать как `m() {}`.

## Функции-конструкторы и `new`

До появления `class` объекты создавали функции, вызванные через `new`:

```js
function Person(name) { this.name = name; }
Person.prototype.hi = function () { return 'hi ' + this.name; };
const p = new Person('Ann');
console.log(p.hi(), p instanceof Person, p.constructor === Person);
try { Person('x') } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
hi Ann true true
TypeError: Cannot set properties of undefined (setting 'name')
```

`new Person('Ann')` создаёт пустой объект, связывает его с `Person.prototype`, вызывает функцию с `this` равным этому объекту и возвращает его. Без `new` в строгом режиме `this` равен `undefined`, и присваивание падает. Прототипы разобраны в [модуле 05](../05-objects-and-prototypes/overview.ru.md), а синтаксис `class` — в модуле 06 дорожной карты.

### new.target

Внутри функции `new.target` показывает, вызвана ли она через `new`:

```js
function NT() { return new.target === undefined ? 'call' : 'new'; }
console.log(NT(), new NT() instanceof NT);
```

```text
call true
```

(`new NT()` возвращает объект, так как функция, вызванная через `new`, возвращает `this`, если не вернула другой объект.)

## Функции — это объекты

У функции есть свойства, и их можно добавлять:

```js
function decl(a, b) { return a + b; }
decl.custom = 1;
console.log(decl.custom, Object.keys(decl), typeof decl.call, decl.toString().slice(0, 30));
console.log(Object.prototype.toString.call(function* () {}), Object.prototype.toString.call(async () => {}), (async () => {}).constructor.name);
```

```text
1 [ 'custom' ] function function decl(a, b) { return a
[object GeneratorFunction] [object AsyncFunction] AsyncFunction
```

`toString()` возвращает исходный текст функции. Генераторные и асинхронные функции — отдельные виды (разобраны в модулях 11 и 12 дорожной карты).

## Вызов с распространением

`...` разворачивает массив в аргументы:

```js
console.log(Math.max(...[1, 5, 3]), Math.max.apply(null, [1, 5, 3]));
```

```text
5 5
```

## Замыкания: независимое состояние

Каждый вызов фабрики создаёт собственное окружение:

```js
const counter = () => { let c = 0; return { inc: () => ++c, get: () => c }; };
const c1 = counter(), c2 = counter(); c1.inc(); c1.inc(); c2.inc();
console.log(c1.get(), c2.get());
```

```text
2 1
```

## Лучшие практики

- Именованные вспомогательные функции — объявлениями; обратные вызовы — стрелками.
- Не полагайтесь на `arguments`: `...rest` и деструктуризация параметров понятнее.
- Передавая метод как обратный вызов, привязывайте `this` (`bind` или стрелка-обёртка).
- Значения по умолчанию вместо проверок `undefined` внутри.
- Конструкторы пишите как `class` (модуль 06).
- Для явной проверки вызова через `new` используйте `new.target`.

## Частые ошибки

- ❌ `bound.call(other)` в расчёте сменить `this` → ✅ `bind` фиксирует `this` окончательно.
- ❌ `new` на стрелке → ✅ обычная функция или `class`.
- ❌ Вызов конструктора без `new` → ✅ `new`, проверка `new.target`, или `class` (он сам выбросит ошибку).
- ❌ `Math.max.apply` с огромным массивом → ✅ ограничение на число аргументов; используйте цикл или `reduce`.
- ❌ Ожидать, что `call(null)` в строгом режиме даст глобальный объект → ✅ это `null`.

> ⚠️ Проверить: предел числа аргументов при `apply`/spread и поведение `Function.prototype.toString` для встроенных функций в опытах не проверялись.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Прототипы и `new`: [05 · Объекты и прототипы](../05-objects-and-prototypes/overview.ru.md)
- Замыкания: [03 · Переменные и области видимости](../03-scopes-and-hoisting/advanced.ru.md#цепочка-областей)

## Источники

- [MDN: Function.prototype.bind()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function/bind)
- [MDN: Function.prototype.call()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function/call)
- [MDN: new operator](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/new)
- [MDN: new.target](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/new.target)
- [MDN: Function](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function)
