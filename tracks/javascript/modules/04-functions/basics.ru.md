---
title: "Функции: базовая теория"
lang: ru
track: javascript
kind: module
doc: basics
slug: 04-functions
part: 1
tags: [functions, arrow-functions, parameters, return, this, recursion, callbacks]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Функции: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Типы и значения](../02-types-and-values/overview.ru.md), [03 · Переменные и области видимости](../03-scopes-and-hoisting/overview.ru.md) · **Время:** ~2,5 ч

Все примеры выполнены на Node.js 24.15 в файле ES-модуля (строгий режим).

## Зачем это нужно

Функция — основная единица кода в JavaScript: на ней строятся обработчики событий, обратные вызовы, модули и классы. Особенности вызова и `this` — самый частый источник путаницы.

## Три формы функций

```js
function decl(a, b) { return a + b; }                       // объявление
const expr = function (a, b) { return a + b; };             // функциональное выражение
const named = function fact(n) { return n <= 1 ? 1 : n * fact(n - 1); };
const arrow = (a, b) => a + b;                              // стрелочная функция
console.log(decl.name, expr.name, named.name, arrow.name, decl.length, ((a, b = 1, ...c) => {}).length);
try { fact(3) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
decl expr fact arrow 2 1
ReferenceError: fact is not defined
```

- **Объявление** всплывает целиком: его можно вызвать до строки с объявлением ([модуль 03](../03-scopes-and-hoisting/basics.ru.md#всплытие-hoisting)).
- **Выражение** и **стрелка** — обычные значения, присвоенные переменной; всплывает только переменная (со значением `undefined`).
- Свойство `name` берётся из имени переменной или из имени выражения (`fact`). Имя функционального выражения видно только внутри него, поэтому снаружи `fact` не определена.
- `length` — число параметров до первого значения по умолчанию или `...rest`.
- Стрелка с одним выражением возвращает его значение без `return`: `(a, b) => a + b`.

## Параметры и аргументы

```js
function decl(a, b) { return a + b; }
console.log(decl(1), decl(1, 2, 3), decl('a', 'b'));
```

```text
NaN 3 ab
```

JavaScript не проверяет число аргументов: лишние игнорируются, недостающие равны `undefined` (поэтому `1 + undefined` — `NaN`).

### Значения по умолчанию

```js
function dflt(a, b = a * 2, c = (() => 'lazy')()) { return [a, b, c]; }
console.log(dflt(1), dflt(1, undefined), dflt(1, null), dflt(1, 5));
let n = 0; const f = (a = ++n) => a;
console.log(f(), f(), f(10), n);
```

```text
[ 1, 2, 'lazy' ] [ 1, 2, 'lazy' ] [ 1, null, 'lazy' ] [ 1, 5, 'lazy' ]
1 2 10 2
```

- Значение по умолчанию подставляется только при `undefined`; `null` остаётся `null`.
- Оно вычисляется **заново при каждом вызове** (`f()` дал `1`, затем `2`) и только если аргумент не передан (`f(10)` не увеличил `n`).
- Значение по умолчанию может использовать предыдущие параметры (`b = a * 2`).

### Остаточные параметры и `arguments`

```js
function rest(first, ...others) { return [first, others, arguments.length]; }
function args() { return [arguments.length, Array.isArray(arguments), typeof arguments, [...arguments]]; }
console.log(rest(1, 2, 3), rest(), args('a', 'b'));
```

```text
[ 1, [ 2, 3 ], 3 ] [ undefined, [], 0 ] [ 2, false, 'object', [ 'a', 'b' ] ]
```

- `...others` собирает остаток в **настоящий массив** (`[2, 3]`); без аргументов — пустой массив.
- `arguments` — объект, похожий на массив: у него есть `length`, но нет методов массива (`Array.isArray` — `false`). В новом коде используйте `...rest`. У стрелочной функции нет собственного `arguments`: внутри обычной функции она видит `arguments` внешней функции (в опыте `outer('outerArg')` вернула `'outerArg'`), а на верхнем уровне модуля `arguments` не определён.

## Возврат значения

Функция без `return` возвращает `undefined`. Будьте осторожны с переносом строки:

```js
function noret() {}
function asi() { return
  42 }
console.log(noret(), asi());
```

```text
undefined undefined
```

После `return` автоматически вставляется `;` (ASI), поэтому `42` — недостижимый код. Начинайте возвращаемое выражение на той же строке: `return 42`, `return (` для многострочных.

## this: основы

В обычной функции `this` определяется **способом вызова**:

```js
const obj = {
  name: 'obj',
  m() { return this?.name; },
  arrow: () => typeof this,
  nested() { return [1].map(() => this.name); },
  nestedFn() { return [1].map(function () { return this; }); },
};
console.log(obj.m(), obj.arrow(), obj.nested(), obj.nestedFn());
const detached = obj.m;
console.log(detached());
function strictThis() { return this; }
console.log(strictThis());
```

```text
obj undefined [ 'obj' ] [ undefined ]
undefined
undefined
```

- **Вызов как метод** `obj.m()` — `this` равен `obj`.
- **Стрелка** берёт `this` из окружающего кода: в `nested` внутри `map` она видит `this` метода (`'obj'`), а на верхнем уровне модуля `this` — `undefined` (поэтому `obj.arrow()` вернул `'undefined'`).
- **Обычная функция внутри `map`** (`nestedFn`) вызывается без объекта: `this` равен `undefined`.
- **Оторванный метод** (`const detached = obj.m; detached()`) потерял объект: `this?.name` вернул `undefined`. Без `?.` была бы ошибка `Cannot read properties of undefined`.
- Обычный вызов `strictThis()` в строгом режиме — `this` равен `undefined` (в нестрогом — глобальный объект).

Явно задать `this` позволяют `call`, `apply` и `bind` — они разобраны в [продвинутой теории](advanced.ru.md#call-apply-и-bind).

## Функции как значения

Функцию можно передавать как аргумент (обратный вызов, callback) и возвращать:

```js
console.log([1, 2, 3].map(n => n * 2), ['1', '2', '3'].map(parseInt), ['1', '2', '3'].map(Number));
const compose = (...fs) => x => fs.reduceRight((v, f) => f(v), x);
console.log(compose(x => x + 1, x => x * 2)(5));
```

```text
[ 2, 4, 6 ] [ 1, NaN, NaN ] [ 1, 2, 3 ]
11
```

- `['1','2','3'].map(parseInt)` даёт `[1, NaN, NaN]`: `map` передаёт в функцию три аргумента (значение, индекс, массив), и `parseInt` принимает индекс как систему счисления. Оборачивайте: `.map(s => parseInt(s, 10))` или используйте `Number`.
- `compose` возвращает новую функцию, которая применяет переданные справа налево: `5 * 2 + 1 = 11`.

## Рекурсия и стек

```js
const depth = (n) => depth(n + 1);
try { depth(0) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
RangeError: Maximum call stack size exceeded
```

Каждый вызов занимает место в стеке вызовов. Бесконечная или слишком глубокая рекурсия приводит к `RangeError`. Для больших глубин используйте цикл или явный стек.

## Ошибки новичка

- ❌ Потерять `this` у метода, переданного как обратный вызов → ✅ стрелка-обёртка или `bind`.
- ❌ `['1','2','3'].map(parseInt)` → ✅ `.map(Number)` или `s => parseInt(s, 10)`.
- ❌ Перенос строки после `return` → ✅ значение на той же строке.
- ❌ Ожидать проверку числа аргументов → ✅ проверять самостоятельно или использовать значения по умолчанию.
- ❌ Использовать `arguments` в новом коде → ✅ `...rest`.
- ❌ Стрелка как метод, которому нужен `this` объекта → ✅ сокращённый метод `m() {}`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — `call`/`apply`/`bind`, конструкторы, `new.target`
- Следующий модуль: [05 · Объекты и прототипы](../05-objects-and-prototypes/overview.ru.md)
- Ранее: [03 · Переменные и области видимости](../03-scopes-and-hoisting/overview.ru.md) (замыкания)

## Источники

- [MDN: Functions](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Functions)
- [MDN: Arrow function expressions](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Functions/Arrow_functions)
- [MDN: Default parameters](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Functions/Default_parameters)
- [MDN: Rest parameters](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Functions/rest_parameters)
- [MDN: this](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/this)
