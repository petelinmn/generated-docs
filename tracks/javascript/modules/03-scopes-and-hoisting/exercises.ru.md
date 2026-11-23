---
title: "Упражнения: переменные и области видимости"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 03-scopes-and-hoisting
part: 1
tags: [let, const, var, hoisting, tdz, closure]
status: draft
updated: 2026-11-23
---

# Упражнения: переменные и области видимости

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Запускайте код в файле ES-модуля (`"type": "module"`).

## 1. 🟢 Всплытие

**Задача:** предскажите вывод и проверьте: `console.log(typeof a, a); var a = 1;` и затем вызов `f()` до объявления `function f() { return 'ok'; }`.

**Ожидаемый результат:** `undefined undefined`, затем `ok`.

<details><summary>Подсказка</summary>

`var` всплывает как `undefined`, функция — целиком.

</details>

<details><summary>Решение</summary>

```js
console.log(typeof a, a); var a = 1;
console.log(f()); function f() { return 'ok'; }
```

```text
undefined undefined
ok
```

</details>

## 2. 🟢 TDZ

**Задача:** получите `ReferenceError` от `let`, от `const` и от `class`, обратившись к ним до объявления. Выведите сообщения.

**Ожидаемый результат:** `ReferenceError: Cannot access 'l1' before initialization` и аналогичные для `c1` и `K`.

<details><summary>Подсказка</summary>

Оберните обращения в `try`/`catch` и выведите `e.name + ': ' + e.message`.

</details>

<details><summary>Решение</summary>

```js
const t = (f) => { try { f() } catch (e) { console.log(e.name + ': ' + e.message) } };
t(() => { console.log(l1); let l1 = 1; });
t(() => { console.log(c1); const c1 = 1; });
t(() => new K()); class K {}
```

```text
ReferenceError: Cannot access 'l1' before initialization
ReferenceError: Cannot access 'c1' before initialization
ReferenceError: Cannot access 'K' before initialization
```

</details>

## 3. 🟡 Замыкания в цикле

**Задача:** создайте три функции в цикле, которые возвращают индексы `0`, `1`, `2`, двумя способами: через `let` и через `var` с вспомогательной функцией (IIFE). Покажите, что `var` без помощи даёт `3, 3, 3`.

**Ожидаемый результат:** `[3, 3, 3]`, `[0, 1, 2]`, `[0, 1, 2]`.

<details><summary>Подсказка</summary>

IIFE: `((n) => () => n)(a)`.

</details>

<details><summary>Решение</summary>

```js
const v = []; for (var a = 0; a < 3; a++) v.push(() => a);
const l = []; for (let b = 0; b < 3; b++) l.push(() => b);
const w = []; for (var c = 0; c < 3; c++) w.push(((n) => () => n)(c));
console.log(v.map(f => f()), l.map(f => f()), w.map(f => f()));
```

```text
[ 3, 3, 3 ] [ 0, 1, 2 ] [ 0, 1, 2 ]
```

</details>

## 4. 🟡 Область модуля

**Задача:** в ES-модуле объявите `var gv = 1; let gl = 2;` и выведите `globalThis.gv`, `globalThis.gl`. Затем выполните то же в `node -e`. Объясните разницу.

**Ожидаемый результат:** в файле `undefined undefined`; в `node -e` — `1 undefined`.

<details><summary>Подсказка</summary>

Файл — модуль со своей областью, `-e` — глобальная область, но `let` не создаёт свойства глобального объекта.

</details>

<details><summary>Решение</summary>

```text
$ node module.js
undefined undefined
$ node -e "var gv=1; let gl=2; console.log(globalThis.gv, globalThis.gl)"
1 undefined
```

</details>

## 5. 🔴 Функции в блоках и параметры по умолчанию

**Задача:** (а) покажите разницу между строгим и нестрогим режимом на функции, объявленной в блоке (`.js` в ES-модуле и `.cjs`). (б) Предскажите результат `((a, b = () => a) => { var a = 2; return b(); })(1)` и объясните.

**Ожидаемый результат:** (а) `ReferenceError: inner is not defined` в модуле, `in block` в `.cjs`; (б) `1`.

<details><summary>Подсказка</summary>

(б) Замыкание в значении по умолчанию создаётся в области параметров, а `var a` в теле — другая переменная.

</details>

<details><summary>Решение</summary>

Код и вывод (а) — в [продвинутой теории](advanced.ru.md#функции-в-блоках).

```js
console.log(((a, b = () => a) => { var a = 2; return b(); })(1));
```

```text
1
```

`b` видит параметр `a`, равный `1`; `var a = 2` в теле создаёт отдельную переменную.

</details>
