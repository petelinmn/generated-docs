---
title: "Упражнения: функции"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 04-functions
part: 1
tags: [functions, this, bind, closures, default-parameters, rest-parameters]
status: draft
updated: 2026-11-23
---

# Упражнения: функции

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Запускайте код в файле ES-модуля.

## 1. 🟢 Сумма с остатком

**Задача:** напишите функцию `sum(...nums)`, которая возвращает сумму всех аргументов, и `sumWithBase(base = 0, ...nums)`, добавляющую начальное значение.

**Ожидаемый результат:** `sum(1, 2, 3)` → `6`, `sum()` → `0`, `sumWithBase(10, 1, 2)` → `13`, `sumWithBase()` → `0`.

<details><summary>Подсказка</summary>

`nums.reduce((a, b) => a + b, 0)`.

</details>

<details><summary>Решение</summary>

```js
const sum = (...nums) => nums.reduce((a, b) => a + b, 0);
const sumWithBase = (base = 0, ...nums) => base + sum(...nums);
console.log(sum(1, 2, 3), sum(), sumWithBase(10, 1, 2), sumWithBase());
```

```text
6 0 13 0
```

</details>

## 2. 🟢 Безопасный `parseInt`

**Задача:** перепишите `['1','2','3'].map(parseInt)` так, чтобы результат был `[1, 2, 3]`, двумя способами.

**Ожидаемый результат:** `[1, 2, 3]` в обоих случаях.

<details><summary>Подсказка</summary>

Либо `Number`, либо обёртка с основанием `10`.

</details>

<details><summary>Решение</summary>

```js
console.log(['1', '2', '3'].map(Number), ['1', '2', '3'].map(s => parseInt(s, 10)));
```

```text
[ 1, 2, 3 ] [ 1, 2, 3 ]
```

</details>

## 3. 🟡 Потерянный `this`

**Задача:** у объекта `counter` с полем `n` и методом `inc()` метод передан как обратный вызов (`const f = counter.inc; f()`). Покажите ошибку и исправьте двумя способами: `bind` и стрелка-обёртка.

**Ожидаемый результат:** без исправления `TypeError`; с исправлением счётчик увеличивается.

<details><summary>Подсказка</summary>

`counter.inc.bind(counter)` и `() => counter.inc()`.

</details>

<details><summary>Решение</summary>

```js
const counter = { n: 0, inc() { return ++this.n; } };
const lost = counter.inc;
try { lost() } catch (e) { console.log(e.name + ': ' + e.message) }
const bound = counter.inc.bind(counter);
const wrapped = () => counter.inc();
console.log(bound(), wrapped());
```

```text
TypeError: Cannot read properties of undefined (reading 'n')
1 2
```

</details>

## 4. 🟡 Частичное применение

**Задача:** используя `bind`, создайте из `volume(w, h, d)` функцию `volumeOfHeight10(w, d)`, у которой высота равна `10`. Покажите её `length`.

**Ожидаемый результат:** `volumeOfHeight10(2, 3)` → `60`, `length` равно `2`.

<details><summary>Подсказка</summary>

`bind` фиксирует аргументы **с начала**; чтобы зафиксировать высоту, переставьте параметры у вспомогательной функции.

</details>

<details><summary>Решение</summary>

```js
const volume = (h, w, d) => h * w * d;           // высота — первый параметр
const volumeOfHeight10 = volume.bind(null, 10);
console.log(volumeOfHeight10(2, 3), volumeOfHeight10.length);
```

```text
60 2
```

`bind` умеет фиксировать только начальные аргументы, поэтому нужный параметр ставят первым.

</details>

## 5. 🔴 Счётчики и замыкания

**Задача:** напишите фабрику `makeCounter(step = 1)`, возвращающую функцию, которая при каждом вызове увеличивает собственное значение на `step`. Два счётчика не должны влиять друг на друга. Убедитесь, что `new makeCounter()` не работает: фабрика — стрелка.

**Ожидаемый результат:** значения независимы; `new` на стрелке — `TypeError`.

<details><summary>Подсказка</summary>

Переменная счётчика — внутри фабрики; фабрика — стрелка.

</details>

<details><summary>Решение</summary>

```js
const makeCounter = (step = 1) => { let c = 0; return () => (c += step); };
const a = makeCounter(), b = makeCounter(5);
console.log(a(), a(), b(), b(), a());
try { new makeCounter() } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
1 2 5 10 3
TypeError: makeCounter is not a constructor
```

</details>
