---
title: "Переменные и области видимости: продвинутая теория"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 03-scopes-and-hoisting
part: 1
tags: [scope-chain, module-scope, global-scope, function-in-block, default-parameters, switch, catch, iife]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Переменные и области видимости: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры выполнены на Node.js 24.15; где указано, использован нестрогий файл `.cjs`.

## Цепочка областей

Имя ищется в текущей области, затем в объемлющей и так далее до глобальной. Функция «запоминает» область, в которой создана, — это **замыкание**:

```js
const mk = () => { let count = 0; return () => ++count; };
const c = mk(); c();
console.log(c(), typeof count);
```

```text
2 undefined
```

Внутренняя функция видит `count` и меняет его между вызовами, а снаружи `count` недоступна (`typeof count` — `'undefined'`). Так создаются приватные данные; подробнее — в модуле 10 дорожной карты.

## Область модуля и глобальная область

Переменные верхнего уровня **файла** принадлежат модулю, а не глобальному объекту:

```js
// файл ES-модуля
var gv = 1; let gl = 2;
console.log(globalThis.gv, globalThis.gl, this);
```

```text
undefined undefined undefined
```

В браузерном классическом скрипте `var` на верхнем уровне создала бы свойство `window`. В Node.js (как в ESM, так и в CommonJS) этого нет: файл — это модуль. Но код из `node -e` выполняется в глобальной области:

```text
$ node -e "var gv=1; let gl=2; console.log(globalThis.gv, globalThis.gl)"
1 undefined
```

`var` попала в `globalThis`, а `let` — нет: `let` и `const` на верхнем уровне скрипта живут в отдельной «глобальной лексической» области, не являясь свойствами глобального объекта.

## Функции в блоках

В строгом режиме (в том числе в ESM) функция, объявленная в блоке, видна только внутри него:

```js
{ function inner() { return 'in block'; } }
try { console.log(inner()); } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
ReferenceError: inner is not defined
```

В нестрогом коде (файл `.cjs`) поведение исторически другое:

```js
// s.cjs (нестрогий режим)
{ function inner() { return 'in block'; } }
console.log(inner());
console.log(typeof hoistedFn);
if (true) { function hoistedFn() {} }
console.log(typeof hoistedFn);
```

```text
in block
undefined
function
```

Функция из блока становится доступной во внешней функции после выполнения блока, а до него — `undefined`. Эти особенности нужны только для чтения старого кода; пишите в строгом режиме.

## Область параметров по умолчанию

Значения по умолчанию вычисляются в собственной области параметров, где тело функции ещё не видно:

```js
let dflt = 'outer';
function g(a = dflt, dflt2 = a) { let dflt = 'body'; return [a, dflt2, dflt]; }
console.log(g());
```

```text
[ 'outer', 'outer', 'body' ]
```

Параметр `a` взял `dflt` из внешней области (а не `'body'` из тела функции), а второй параметр увидел первый.

## switch и catch

Все ветки `switch` разделяют одну блочную область, поэтому одинаковые `let` в разных `case` конфликтуют:

```js
eval("switch (1) { case 1: let q = 1; break; case 2: let q = 2; }");
```

```text
SyntaxError: Identifier 'q' has already been declared
```

Решение — отдельные фигурные скобки в каждой ветке: `case 1: { let q = 1; break; }`.

Параметр `catch` видим только внутри `catch`, а `var` из него «протекает» наружу:

```js
try { throw new Error('x') } catch (e) { var fromCatch = e.message }
console.log(fromCatch, typeof e);
```

```text
x undefined
```

## IIFE: немедленно вызываемая функция

До появления блочной области и модулей для изоляции переменных использовали функцию, вызываемую сразу:

```js
const r = (() => { const secret = 42; return secret * 2; })();
console.log(r);
```

```text
84
```

`secret` недоступна снаружи. Сегодня для этого достаточно блока `{ }` или модуля, а IIFE встречается в старом коде и там, где нужен `async` на верхнем уровне без модуля.

## Лучшие практики

- Объявляйте переменные как можно ближе к месту использования и в минимальной области.
- `const` по умолчанию; `let` только при переприсваивании.
- Не создавайте глобальные переменные: в строгом режиме это `ReferenceError` ([модуль 01](../01-environment/advanced.ru.md#строгий-режим)).
- Не полагайтесь на всплытие и на функции в блоках.
- Оборачивайте `case` с объявлениями в фигурные скобки.

## Частые ошибки

- ❌ Ждать, что `var` на верхнем уровне файла станет свойством `globalThis` → ✅ это переменная модуля.
- ❌ Вызывать функцию из блока вне блока → ✅ объявляйте её в нужной области.
- ❌ Повторять `let` в нескольких `case` → ✅ скобки вокруг каждой ветки.
- ❌ Использовать параметр по умолчанию, ссылающийся на переменную тела функции → ✅ значение берётся из внешней области.
- ❌ Тестировать область в `node -e` и переносить выводы на файл → ✅ в файле действуют правила модуля.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Строгий режим: [01 · Окружение и инструменты](../01-environment/advanced.ru.md)
- `typeof` и `ReferenceError`: [02 · Типы и значения](../02-types-and-values/advanced.ru.md#typeof-необъявленной-переменной)

## Источники

- [MDN: Scope](https://developer.mozilla.org/en-US/docs/Glossary/Scope)
- [MDN: Functions — Block-level functions](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/function#block-level_function_declaration)
- [MDN: Default parameters — scope](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Functions/Default_parameters#scope_effects)
- [MDN: IIFE](https://developer.mozilla.org/en-US/docs/Glossary/IIFE)
- [ECMAScript: Executable Code and Execution Contexts](https://tc39.es/ecma262/#sec-executable-code-and-execution-contexts)
