---
title: "Упражнения: итераторы и генераторы"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 11-iterators-and-generators
part: 2
tags: [iterator, generator, async-generator, iterator-helpers]
status: draft
updated: 2026-11-23
---

# Упражнения: итераторы и генераторы

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Код запускается в файле с `package.json` (`{"type": "module"}`).

## 1. 🟢 Обратный отсчёт

**Задача:** создайте класс `Countdown(n)`, который можно перебрать через `for...of` и spread: он отдаёт `n`, `n-1`, …, `1`. Найдите максимум из `Countdown(4)` через `Math.max(...)`.

**Ожидаемый результат:** `[ 3, 2, 1 ] 4`.

<details><summary>Подсказка</summary>

Метод `*[Symbol.iterator]() { … }` — генератор.

</details>

<details><summary>Решение</summary>

```js
class Countdown { constructor(n) { this.n = n } *[Symbol.iterator]() { for (let i = this.n; i > 0; i--) yield i; } }
console.log([...new Countdown(3)], Math.max(...new Countdown(4)));
```

```text
[ 3, 2, 1 ] 4
```

</details>

## 2. 🟢 Фибоначчи и `take`

**Задача:** напишите бесконечный генератор `fib()` и функцию `take(n, it)`. Выведите первые 8 чисел.

**Ожидаемый результат:** `0 1 1 2 3 5 8 13`.

<details><summary>Подсказка</summary>

Состояние — пара `[a, b]`; `take` выходит из цикла после `n` значений.

</details>

<details><summary>Решение</summary>

```js
function* fib() { let [a, b] = [0, 1]; while (true) { yield a; [a, b] = [b, a + b]; } }
function* take(n, it) { if (n <= 0) return; let i = 0; for (const v of it) { yield v; if (++i >= n) return; } }
console.log([...take(8, fib())].join(' '));
```

```text
0 1 1 2 3 5 8 13
```

</details>

## 3. 🟡 Скользящее среднее

**Задача:** напишите генератор `avg()`, который принимает числа через `next(x)` и возвращает текущее среднее. Подайте `10`, `20`, `60`.

**Ожидаемый результат:** `10 15 30`.

<details><summary>Подсказка</summary>

Первый `next()` только запускает генератор; храните сумму и счётчик в локальных переменных.

</details>

<details><summary>Решение</summary>

```js
function* avg() { let sum = 0, n = 0; while (true) { const x = yield n ? sum / n : undefined; sum += x; n++; } }
const a = avg(); a.next(); console.log(a.next(10).value, a.next(20).value, a.next(60).value);
```

```text
10 15 30
```

</details>

## 4. 🟡 Пачки и хелперы

**Задача:** (а) напишите генератор `chunk(it, size)`, который отдаёт массивы по `size` элементов (последний может быть короче). (б) Через хелперы итераторов получите первые 5 чётных чисел Фибоначчи.

**Ожидаемый результат:** `[ [ 1, 2 ], [ 3, 4 ], [ 5 ] ]` и `[ 0, 2, 8, 34, 144 ]`.

<details><summary>Подсказка</summary>

Для (б) используйте `fib().filter(...).take(5).toArray()`.

</details>

<details><summary>Решение</summary>

```js
function* chunk(it, size) { let buf = []; for (const v of it) { buf.push(v); if (buf.length === size) { yield buf; buf = []; } } if (buf.length) yield buf; }
console.log([...chunk([1, 2, 3, 4, 5], 2)]);
console.log(fib().filter(x => x % 2 === 0).take(5).toArray());
```

```text
[ [ 1, 2 ], [ 3, 4 ], [ 5 ] ]
[ 0, 2, 8, 34, 144 ]
```

`fib` — генератор из упражнения 2.

</details>

## 5. 🔴 Постраничная загрузка и освобождение ресурса

**Задача:** (а) напишите асинхронный генератор `pages(n)`, который с небольшой задержкой отдаёт `{ page, items }` и соберите все `items`. (б) Напишите асинхронный генератор с `try/finally`, который печатает `released`, и убедитесь, что досрочный `break` из `for await` освобождает ресурс.

**Ожидаемый результат:** `[ 10, 11, 20, 21, 30, 31 ]` и `released`.

<details><summary>Подсказка</summary>

`for await (const { items } of pages(3)) …`; `break` вызывает `return()` генератора, а он выполняет `finally`.

</details>

<details><summary>Решение</summary>

```js
const sleep = ms => new Promise(r => setTimeout(r, ms));
async function* pages(n) { for (let p = 1; p <= n; p++) { await sleep(1); yield { page: p, items: [p * 10, p * 10 + 1] }; } }
const all = []; for await (const { items } of pages(3)) all.push(...items); console.log(all);
async function* resource() { try { yield 1; yield 2; yield 3 } finally { console.log('released') } }
for await (const v of resource()) { if (v === 2) break; }
```

```text
[ 10, 11, 20, 21, 30, 31 ]
released
```

</details>
