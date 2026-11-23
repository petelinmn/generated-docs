---
title: "Упражнения: замыкания и функциональный стиль"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 10-closures-and-functional
part: 2
tags: [closure, pipe, memoization, immutability]
status: draft
updated: 2026-11-23
---

# Упражнения: замыкания и функциональный стиль

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Код запускается в файле с `package.json` (`{"type": "module"}`).

## 1. 🟢 Счётчик с шагом

**Задача:** напишите `makeCounter(step = 1)` с методами `inc()` (увеличивает на `step` и возвращает значение), `reset()` и `get()`. Проверьте, что два счётчика независимы.

**Ожидаемый результат:** для `makeCounter(5)` после двух `inc()` — `10`, после `reset()` — `0`; новый `makeCounter()` после одного `inc()` — `1`, старый — `0`.

<details><summary>Подсказка</summary>

Состояние `n` — локальная переменная фабрики; методы — замыкания над ней.

</details>

<details><summary>Решение</summary>

```js
function makeCounter(step = 1) { let n = 0; return { inc: () => (n += step), reset: () => { n = 0; }, get: () => n }; }
const c = makeCounter(5); c.inc(); c.inc(); console.log(c.get()); c.reset(); console.log(c.get());
const other = makeCounter(); other.inc(); console.log(other.get(), c.get());
```

```text
10
0
1 0
```

</details>

## 2. 🟢 Конвейер для slug

**Задача:** с помощью `pipe` соберите функцию, которая обрезает пробелы, переводит строку в нижний регистр и заменяет пробельные серии на `-`.

**Ожидаемый результат:** `slug('  Hello Big World ')` → `hello-big-world`.

<details><summary>Подсказка</summary>

`const pipe = (...fns) => x => fns.reduce((v, f) => f(v), x);`

</details>

<details><summary>Решение</summary>

```js
const pipe = (...fns) => x => fns.reduce((v, f) => f(v), x);
const slug = pipe(s => s.trim(), s => s.toLowerCase(), s => s.replace(/\s+/g, '-'));
console.log(slug('  Hello Big World '));
```

```text
hello-big-world
```

</details>

## 3. 🟡 Исправьте `map` и `reduce`

**Задача:** `['10', '10', '10'].map(parseInt)` даёт не то, что нужно. Исправьте так, чтобы получить `[ 10, 10, 10 ]`; затем посчитайте сумму `['1', '2', '3']` через `reduce` и сумму пустого массива без ошибки.

**Ожидаемый результат:** `[ 10, 10, 10 ] 6 0`.

<details><summary>Подсказка</summary>

Передайте основание явно и задайте начальное значение `reduce`.

</details>

<details><summary>Решение</summary>

```js
console.log(['10', '10', '10'].map(s => parseInt(s, 10)), ['1', '2', '3'].map(Number).reduce((a, b) => a + b, 0), [].reduce((a, b) => a + b, 0));
```

```text
[ 10, 10, 10 ] 6 0
```

</details>

## 4. 🟡 Мемоизация с несколькими аргументами

**Задача:** напишите `memoize(fn)`, который работает для функций с несколькими аргументами. Покажите, что функция `add` вызывается для `(1, 2)` один раз, а `(2, 1)` — отдельный вызов.

**Ожидаемый результат:** `3 3 3 2` (три результата и число вычислений).

<details><summary>Подсказка</summary>

Ключ кэша — `JSON.stringify(args)`.

</details>

<details><summary>Решение</summary>

```js
function memoize(fn) { const cache = new Map(); return (...args) => { const k = JSON.stringify(args); if (!cache.has(k)) cache.set(k, fn(...args)); return cache.get(k); }; }
let calls = 0; const add = memoize((a, b) => { calls++; return a + b; });
console.log(add(1, 2), add(1, 2), add(2, 1), calls);
```

```text
3 3 3 2
```

Подход годится только для аргументов, которые сериализуются в JSON без потерь.

</details>

## 5. 🔴 Неизменяемое обновление

**Задача:** напишите `setIn(obj, path, value)`, которая возвращает **новый** объект с изменённым вложенным значением и не меняет исходный. Неизменённые ветки должны остаться теми же ссылками (структурное разделение).

**Ожидаемый результат:** `B A true true false` — новое имя, старое имя, ветка `theme` не скопирована, массив `tags` не скопирован, объекты разные.

<details><summary>Подсказка</summary>

Рекурсия: `{ ...obj, [k]: rest.length ? setIn(obj[k] ?? {}, rest, value) : value }`.

</details>

<details><summary>Решение</summary>

```js
const setIn = (obj, [k, ...rest], v) => ({ ...obj, [k]: rest.length ? setIn(obj[k] ?? {}, rest, v) : v });
const state = { user: { name: 'A', tags: ['x'] }, theme: { dark: false } };
const next = setIn(state, ['user', 'name'], 'B');
console.log(next.user.name, state.user.name, next.theme === state.theme, next.user.tags === state.user.tags, next === state);
```

```text
B A true true false
```

Копируются только объекты на пути к изменяемому значению.

</details>
