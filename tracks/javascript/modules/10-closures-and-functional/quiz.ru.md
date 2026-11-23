---
title: "Квиз: замыкания и функциональный стиль"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 10-closures-and-functional
part: 2
tags: [closure, currying, memoization, immutability]
status: draft
updated: 2026-11-23
---

# Квиз: замыкания и функциональный стиль

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что выведет код?

```js
function makeCounter() { let n = 0; return { inc: () => ++n, get: () => n }; }
const c1 = makeCounter(), c2 = makeCounter();
c1.inc(); c1.inc(); c2.inc();
console.log(c1.get(), c2.get());
```

- A) `3 3`
- B) `2 1`
- C) `1 1`
- D) `2 2`

<details><summary>Ответ</summary>

**B.** Каждый вызов `makeCounter` создаёт своё окружение со своей `n`.
→ [Базовая теория](basics.ru.md#что-такое-замыкание)

</details>

## 2. 🟢 Что выведет код?

```js
let x = 1; const getX = () => x; x = 2; console.log(getX());
```

- A) `1`
- B) `2`
- C) `undefined`
- D) `ReferenceError`

<details><summary>Ответ</summary>

**B.** Замыкание хранит переменную, а не значение на момент создания.
→ [Базовая теория](basics.ru.md#замыкание-хранит-переменную-а-не-значение)

</details>

## 3. 🟢 Что выведет код?

```js
const fs = [];
for (var i = 0; i < 3; i++) fs.push(() => i);
console.log(fs.map(f => f()));
```

- A) `[ 0, 1, 2 ]`
- B) `[ 3, 3, 3 ]`
- C) `[ 2, 2, 2 ]`
- D) `[]`

<details><summary>Ответ</summary>

**B.** У `var` одна переменная на весь цикл; к моменту вызова `i` равна `3`. С `let` было бы `[ 0, 1, 2 ]`.
→ [Базовая теория](basics.ru.md#замыкания-в-цикле)

</details>

## 4. 🟢 Что вернёт `bank.balance`?

```js
const bank = (() => { let balance = 100; return { deposit(x) { balance += x; return balance; } }; })();
```

- A) `100`
- B) `undefined`
- C) `ReferenceError`
- D) `0`

<details><summary>Ответ</summary>

**B.** `balance` — локальная переменная замыкания, у объекта нет такого свойства.
→ [Базовая теория](basics.ru.md#фабрики-и-приватное-состояние)

</details>

## 5. 🟡 Что вернёт `['1', '2', '3'].map(parseInt)`?

- A) `[ 1, 2, 3 ]`
- B) `[ 1, NaN, NaN ]`
- C) `[ '1', '2', '3' ]`
- D) `TypeError`

<details><summary>Ответ</summary>

**B.** `map` передаёт индекс вторым аргументом, а `parseInt` трактует его как основание системы счисления.
→ [Продвинутая теория](advanced.ru.md#функции-высшего-порядка)

</details>

## 6. 🟡 Что выведет код?

```js
const pipe = (...fns) => x => fns.reduce((v, f) => f(v), x);
const inc = x => x + 1, sq = x => x * x;
console.log(pipe(inc, sq)(3));
```

- A) `10`
- B) `16`
- C) `7`
- D) `12`

<details><summary>Ответ</summary>

**B.** `pipe` идёт слева направо: `sq(inc(3))` = `16`. `compose(inc, sq)(3)` дал бы `10`.
→ [Продвинутая теория](advanced.ru.md#композиция)

</details>

## 7. 🟡 Что произойдёт при `[].reduce((a, b) => a + b)`?

- A) `0`
- B) `undefined`
- C) `TypeError: Reduce of empty array with no initial value`
- D) `NaN`

<details><summary>Ответ</summary>

**C.** Без начального значения пустой массив — ошибка. Передавайте начальное значение: `reduce(f, 0)`.
→ [Продвинутая теория](advanced.ru.md#функции-высшего-порядка)

</details>

## 8. 🟡 Сколько раз выполнится функция?

```js
let calls = 0; const slowSq = memo(n => { calls++; return n * n; });
slowSq(4); slowSq(4); slowSq(5); console.log(calls);
```

(`memo` — кэш на `Map`.)

- A) `3`
- B) `2`
- C) `1`
- D) `0`

<details><summary>Ответ</summary>

**B.** Для `4` результат взят из кэша; вычислены `4` и `5`.
→ [Продвинутая теория](advanced.ru.md#мемоизация)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) `Object.freeze` замораживает вложенные объекты
- [ ] B) `{ ...o }` делает поверхностную копию
- [ ] C) `structuredClone` умеет копировать функции
- [ ] D) `toSorted()` не изменяет исходный массив

<details><summary>Ответ</summary>

**B, D.** `freeze` неглубокий (A неверно), `structuredClone` выбрасывает `DataCloneError` на функциях (C неверно).
→ [Продвинутая теория](advanced.ru.md#неизменяемость)

</details>

## 10. 🔴 Найдите ошибку

> «`curry((a, b = 2) => a + b)(1)` вернёт функцию, ожидающую второй аргумент».

<details><summary>Ответ</summary>

`fn.length` равен `1` (считаются параметры до первого значения по умолчанию), поэтому функция вызывается сразу и возвращает `3`.
→ [Подвох 5](expert.ru.md#5-каррирование-и-значения-по-умолчанию)

</details>

## 11. 🔴 Что выведет код в нестрогом режиме (CommonJS-файл)?

```js
const o = Object.freeze({ a: 1 }); o.a = 2; o.z = 1; console.log(o.a, o.z);
```

- A) `2 1`
- B) `1 undefined`
- C) `TypeError`
- D) `1 1`

<details><summary>Ответ</summary>

**B.** В нестрогом режиме запись молча игнорируется; в строгом режиме и в ES-модуле была бы `TypeError`.
→ [Подвох 3](expert.ru.md#3-заморозка-и-режимы)

</details>

## 12. 🔴 Что даст `structuredClone(new K())` для экземпляра класса `K`?

- A) Экземпляр `K` с методами
- B) Обычный объект без прототипа `K`: `instanceof K` — `false`
- C) `DataCloneError` для любого класса
- D) `null`

<details><summary>Ответ</summary>

**B.** Копируются данные, прототип теряется (`k.constructor === Object`).
→ [Способы копирования](expert.ru.md#способы-копирования)

</details>

## 13. 🔴 Открытый вопрос

Объясните, что такое замыкание на уровне окружений, и почему счётчики из одной фабрики независимы.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Каждый вызов функции создаёт запись окружения с локальными переменными и ссылкой на внешнее окружение.
- Функция при создании запоминает окружение (`[[Environment]]`); поиск имён идёт по цепочке.
- Замыкание ссылается на привязку, а не на значение, поэтому видит изменения.
- Два вызова фабрики создают два окружения: отсюда независимость; две функции из одного вызова делят одно окружение.

</details>

→ [Экспертная теория](expert.ru.md#внутреннее-устройство)
