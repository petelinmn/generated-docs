---
title: "Замыкания и функциональный стиль: основы"
lang: ru
track: javascript
kind: module
doc: basics
slug: 10-closures-and-functional
part: 2
tags: [closure, factory, private-state, iife, lexical-scope]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Замыкания и функциональный стиль: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [03 · Переменные и области видимости](../03-scopes-and-hoisting/overview.ru.md), [04 · Функции](../04-functions/overview.ru.md) · **Время:** ~2 ч

Все примеры запускались на Node.js 24.15.

## Зачем это нужно

Замыкания позволяют хранить состояние внутри функции и не открывать его наружу: счётчики, кэши, настройки, обработчики событий. Без них пришлось бы использовать глобальные переменные или классы на каждую мелочь.

## Что такое замыкание

Функция в JavaScript запоминает область видимости, в которой она создана ([модуль 03](../03-scopes-and-hoisting/basics.ru.md)). Вызванная позже, даже в другом месте, она по-прежнему видит эти переменные:

```js
function makeCounter() { let n = 0; return { inc: () => ++n, get: () => n }; }
const c1 = makeCounter(), c2 = makeCounter();
c1.inc(); c1.inc(); c2.inc();
console.log(c1.get(), c2.get());
```

```text
2 1
```

- Каждый вызов `makeCounter` создаёт **новую** переменную `n`, поэтому счётчики независимы.
- Снаружи `n` недоступна: менять её можно только через `inc`. Так получается приватное состояние.

## Замыкание хранит переменную, а не значение

Две функции, созданные в одном вызове, делят одну переменную; изменения видны сразу:

```js
function pair() { let v = 0; return [() => v++, () => v]; }
const [up, read] = pair(); up(); up(); console.log(read());

let x = 1; const getX = () => x; x = 2; console.log(getX());
```

```text
2
2
```

`getX` вернула `2`, а не `1`: она читает переменную в момент вызова.

## Замыкания в цикле

С `var` одна переменная на весь цикл, с `let` — новая на каждую итерацию (подробнее — [модуль 03](../03-scopes-and-hoisting/basics.ru.md#замыкание-в-цикле-var-и-let)):

```js
const fs = [];
for (var i = 0; i < 3; i++) fs.push(() => i);
console.log(fs.map(f => f()));
const gs = [];
for (let j = 0; j < 3; j++) gs.push(() => j);
console.log(gs.map(f => f()));
```

```text
[ 3, 3, 3 ]
[ 0, 1, 2 ]
```

## Фабрики и приватное состояние

Функция-фабрика возвращает объект или функцию, скрывая данные:

```js
const bank = (() => { let balance = 100; return { deposit(x) { balance += x; return balance; } }; })();
console.log(bank.deposit(50), bank.balance);
```

```text
150 undefined
```

- Круглые скобки вокруг функции и вызов `()` сразу — **IIFE** (immediately invoked function expression): она создаёт область видимости и тут же выполняется.
- `bank.balance` — `undefined`: переменная недоступна снаружи.
- Для состояния с методами часто удобнее [класс](../06-classes/overview.ru.md) с `#`-полями; замыкание проще, когда метод один-два.

## Функции как значения

Функцию можно передавать и возвращать — это основа функционального стиля:

```js
const not = f => (...a) => !f(...a);
console.log([1, 2, 3, 4].filter(not(n => n % 2)));
```

```text
[ 2, 4 ]
```

`not` принимает функцию и возвращает новую — **функцию высшего порядка** (higher-order function). Подробнее — на [следующем уровне](advanced.ru.md).

## Ошибки новичков

- ❌ Ждать копию значения вместо ссылки на переменную → ✅ помнить, что замыкание видит изменения.
- ❌ Создавать функции в цикле с `var` → ✅ `let`.
- ❌ Считать, что у двух вызовов фабрики общее состояние → ✅ у каждого своё.
- ❌ Заводить класс ради одного скрытого значения → ✅ замыкание.
- ❌ Пытаться прочитать «приватную» переменную снаружи → ✅ предоставить функцию-геттер.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — каррирование, композиция, мемоизация, неизменяемость
- Ранее: [04 · Функции](../04-functions/advanced.ru.md#замыкания-независимое-состояние)

## Источники

- [MDN: Closures](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Closures)
- [MDN: IIFE](https://developer.mozilla.org/en-US/docs/Glossary/IIFE)
- [MDN: Functions — scope and the function stack](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Functions#function_scope)
