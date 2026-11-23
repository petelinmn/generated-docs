---
title: "Переменные и области видимости: базовая теория"
lang: ru
track: javascript
kind: module
doc: basics
slug: 03-scopes-and-hoisting
part: 1
tags: [let, const, var, hoisting, tdz, block-scope, closure, shadowing]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Переменные и области видимости: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Окружение и инструменты](../01-environment/overview.ru.md), [02 · Типы и значения](../02-types-and-values/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены на Node.js 24.15 в файле ES-модуля (строгий режим).

## Зачем это нужно

Область видимости определяет, где переменная существует и кто её видит. Непонимание областей даёт «утечки» переменных, неожиданные `undefined` и классические баги с замыканиями в циклах.

## let, const и var

- **`const`** — переменная, которую нельзя переприсвоить; объявляется с обязательным значением.
- **`let`** — переменная, значение которой можно менять.
- **`var`** — старый способ; другая область видимости и всплытие, поэтому в новом коде не используется.

```js
const arr = [1]; arr.push(2);
console.log(arr);
try { const k = 1; k = 2; } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
[ 1, 2 ]
TypeError: Assignment to constant variable.
```

`const` запрещает **переприсваивание**: массив `arr` менять можно, а `k = 2` — ошибка. Содержимое объекта не замораживается:

```js
const frozen = Object.freeze({ a: 1, nested: { b: 1 } });
frozen.nested.b = 2;
try { frozen.a = 2 } catch (e) { console.log(e.name + ': ' + e.message) }
console.log(frozen);
```

```text
TypeError: Cannot assign to read only property 'a' of object '#<Object>'
{ a: 1, nested: { b: 2 } }
```

`Object.freeze` защищает только верхний уровень свойств (поверхностная заморозка): вложенный объект остался изменяемым.

## Блочная область

`let` и `const` видны только внутри блока `{ … }`, а `var` из блока «протекает» в функцию:

```js
if (true) { var leaked = 'var leaks'; let kept = 'let stays'; }
console.log(leaked, typeof kept);
for (var i = 0; i < 3; i++) {}
console.log('i after var loop:', i);
```

```text
var leaks undefined
i after var loop: 3
```

`typeof kept` равен `'undefined'`, потому что за пределами блока такого имени нет; `var i` остаётся доступной после цикла.

## Всплытие (hoisting)

Объявления «поднимаются» к началу своей области: движок знает о них до выполнения кода.

```js
console.log(typeof v1, v1); var v1 = 5; console.log(v1);
console.log(hoisted()); function hoisted() { return 'function declarations are hoisted'; }
try { expr() } catch (e) { console.log(e.name + ': ' + e.message) } var expr = function () {};
```

```text
undefined undefined
5
function declarations are hoisted
TypeError: expr is not a function
```

- `var v1` всплыла, но без значения: до строки с присваиванием она равна `undefined`.
- Объявление функции (`function hoisted() {}`) всплывает целиком: её можно вызвать до объявления.
- Функциональное **выражение** (`var expr = function () {}`) всплывает как `var`: до присваивания там `undefined`, вызов даёт `TypeError: expr is not a function`.

## Временная мёртвая зона (TDZ)

`let`, `const` и `class` тоже «всплывают», но до строки объявления находятся в мёртвой зоне:

```js
try { console.log(l1); let l1 = 1; } catch (e) { console.log(e.name + ': ' + e.message) }
try { console.log(typeof c1); const c1 = 1; } catch (e) { console.log(e.name + ': ' + e.message) }
try { new K(); class K {} } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
ReferenceError: Cannot access 'l1' before initialization
ReferenceError: Cannot access 'c1' before initialization
ReferenceError: Cannot access 'K' before initialization
```

Даже `typeof` внутри TDZ вызывает ошибку. Это защита от использования переменной до того, как ей задано значение: лучше явная ошибка, чем тихое `undefined`.

## Замыкание в цикле: var и let

Классическая ловушка: функции, созданные в цикле, запоминают **переменную**, а не её значение на момент создания.

```js
const fv = []; for (var a = 0; a < 3; a++) fv.push(() => a);
const fl = []; for (let b = 0; b < 3; b++) fl.push(() => b);
console.log(fv.map(f => f()), fl.map(f => f()));
const timeouts = [];
for (var j = 0; j < 3; j++) setTimeout(() => timeouts.push(j), 0);
setTimeout(() => console.log('setTimeout var:', timeouts), 5);
```

```text
[ 3, 3, 3 ] [ 0, 1, 2 ]
setTimeout var: [ 3, 3, 3 ]
```

С `var` одна переменная `a` на весь цикл, и к моменту вызова функций она равна `3`. С `let` на каждой итерации создаётся своя копия, поэтому функции видят `0`, `1`, `2`.

## Повторное объявление и затенение

```js
var dup = 1; var dup = 2; console.log(dup);
try { eval('let z = 1; let z = 2;') } catch (e) { console.log(e.name + ': ' + e.message) }
let sh = 'outer'; function f() { let sh = 'inner'; return sh; }
console.log(f(), sh);
```

```text
2
SyntaxError: Identifier 'z' has already been declared
inner outer
```

- `var` допускает повторное объявление, `let` и `const` — нет (`SyntaxError`, ошибка видна ещё до выполнения).
- **Затенение:** внутренняя `sh` скрывает внешнюю, но не меняет её.

## Ошибки новичка

- ❌ `var` в новом коде → ✅ `const`, а при необходимости `let`.
- ❌ Читать переменную до объявления (`let x`) → ✅ объявлять сверху блока.
- ❌ Считать `const` неизменяемым → ✅ `const` фиксирует привязку, а не содержимое.
- ❌ Замыкания в цикле с `var` → ✅ `let` в заголовке цикла.
- ❌ Вызывать функциональное выражение до присваивания → ✅ объявление функции или порядок кода.
- ❌ Объявлять одну и ту же переменную дважды → ✅ уникальные имена; `let` и `const` защитят ошибкой.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — цепочка областей, область модуля, функции в блоках
- Ранее: [02 · Типы и значения](../02-types-and-values/overview.ru.md) (`typeof` необъявленной переменной)
- Замыкания подробно: модуль 10 дорожной карты [трека](../../overview.ru.md)

## Источники

- [MDN: let](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/let)
- [MDN: const](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/const)
- [MDN: Hoisting](https://developer.mozilla.org/en-US/docs/Glossary/Hoisting)
- [MDN: Closures](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Closures)
- [ECMAScript: Declarations and the Variable Statement](https://tc39.es/ecma262/#sec-declarations-and-the-variable-statement)
