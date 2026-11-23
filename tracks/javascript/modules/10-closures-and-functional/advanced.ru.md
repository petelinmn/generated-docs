---
title: "Замыкания и функциональный стиль: продвинутый уровень"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 10-closures-and-functional
part: 2
tags: [higher-order-function, currying, partial-application, composition, pipe, memoization, immutability, structured-clone, object-freeze]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Замыкания и функциональный стиль: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры запускались на Node.js 24.15.

## Функции высшего порядка

Встроенные методы массивов ([модуль 07](../07-arrays-and-collections/basics.ru.md)) принимают функции. Ловушка — передача функции с лишними параметрами: `map` вызывает её с тремя аргументами (элемент, индекс, массив):

```js
console.log([1, 2, 3].map(String), ['1', '2', '3'].map(Number), ['1', '2', '3'].map(parseInt));
```

```text
[ '1', '2', '3' ] [ 1, 2, 3 ] [ 1, NaN, NaN ]
```

`parseInt` получил индекс как второй аргумент (основание системы счисления), поэтому `parseInt('2', 1)` — `NaN`. Решение: `map(s => parseInt(s, 10))` или `map(Number)`.

`reduce` сворачивает массив в одно значение; на пустом массиве без начального значения он падает:

```js
console.log([1, 2, 3].reduce((a, b) => a + b), [[1], [2]].reduce((a, b) => a.concat(b), []));
try { [].reduce((a, b) => a + b) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
6 [ 1, 2 ]
TypeError: Reduce of empty array with no initial value
```

Всегда передавайте начальное значение.

## Каррирование и частичное применение

**Частичное применение** фиксирует часть аргументов. Самый простой способ — `bind`:

```js
const mul = (a, b) => a * b; const dbl = mul.bind(null, 2); console.log(dbl(5), dbl.name, dbl.length);
```

```text
10 bound mul 1
```

**Каррирование** превращает функцию от нескольких аргументов в цепочку функций. Универсальная реализация опирается на `fn.length`:

```js
const curry = fn => function c(...a) { return a.length >= fn.length ? fn(...a) : (...b) => c(...a, ...b); };
const add3 = curry((a, b, c) => a + b + c);
console.log(add3(1)(2)(3), add3(1, 2)(3), add3(1)(2, 3));
console.log(((a, b = 2, ...r) => 0).length, ((a, b) => 0).length);
```

```text
6 6 6
1 2
```

`length` считает параметры **до первого значения по умолчанию или rest**, поэтому такая `curry` вызовет функцию, как только получит `length` аргументов: параметры после значения по умолчанию или `...rest` передать не получится (для `(a, b = 2) => …` вызов `f(1)` сразу выполняет функцию). Для таких функций число аргументов задают явно.

## Композиция

`pipe` применяет функции слева направо, `compose` — справа налево:

```js
const pipe = (...fns) => x => fns.reduce((v, f) => f(v), x);
const compose = (...fns) => x => fns.reduceRight((v, f) => f(v), x);
const inc = x => x + 1, sq = x => x * x;
console.log(pipe(inc, sq)(3), compose(inc, sq)(3));
```

```text
16 10
```

`pipe(inc, sq)(3)` — это `sq(inc(3))` = 16, а `compose(inc, sq)(3)` — `inc(sq(3))` = 10. Для композиции функции должны принимать один аргумент: поэтому каррирование и частичное применение идут рядом.

## Мемоизация

Мемоизация запоминает результат по аргументам; подходит только для **чистых** функций (результат зависит лишь от аргументов, побочных эффектов нет):

```js
function memo(fn) { const cache = new Map(); return (k) => { if (cache.has(k)) return cache.get(k); const v = fn(k); cache.set(k, v); return v; }; }
let calls = 0; const slowSq = memo(n => { calls++; return n * n; });
slowSq(4); slowSq(4); slowSq(5); console.log(calls);
const m2 = memo(o => o.a); console.log(m2({ a: 1 }), m2({ a: 1 }));
```

```text
2
1 1
```

- Для чисел и строк кэш работает: функция вызвана дважды (для `4` и `5`), а не трижды.
- Объекты-ключи сравниваются **по ссылке** ([модуль 07](../07-arrays-and-collections/advanced.ru.md)): `{ a: 1 }` дважды — это два разных ключа, и кэш разрастается. Для таких случаев нужна сериализация ключа или `WeakMap`.
- Кэш в `Map` никогда не очищается: у долгоживущих функций возможна утечка памяти.

## once

```js
const once = fn => { let done = false, r; return (...a) => done ? r : (done = true, r = fn(...a)); };
const init = once(() => { console.log('init'); return 42; }); console.log(init(), init());
```

```text
init
42 42
```

Инициализация выполнилась один раз; повторные вызовы возвращают сохранённый результат.

## Неизменяемость

Неизменяемые данные упрощают рассуждения: функция не меняет аргументы, а возвращает новое значение.

```js
'use strict';
const o = Object.freeze({ a: 1, n: { b: 2 } });
try { o.a = 2 } catch (e) { console.log(e.name + ': ' + e.message) }
o.n.b = 3; console.log(o.n.b, Object.isFrozen(o), Object.isFrozen(o.n));
```

```text
TypeError: Cannot assign to read only property 'a' of object '#<Object>'
3 true false
```

- `Object.freeze` **неглубокий**: вложенный объект `o.n` остался изменяемым.
- В нестрогом режиме присваивание молча игнорируется; в строгом ([модуль 02](../02-types-and-values/overview.ru.md) и ES-модули) — `TypeError`.
- Глубокую заморозку делают рекурсивно (`deepFreeze`), как показано в [экспертной теории](expert.ru.md#справочник).

**Копии вместо изменений:**

```js
const s = { a: 1, n: { b: 2 } };
const s2 = { ...s, a: 5 }; console.log(s2.n === s.n, s.a);
const sc = structuredClone(s); console.log(sc.n === s.n, sc);
const arr = [3, 1, 2];
console.log(arr.toSorted(), arr, arr.toReversed(), arr.with(0, 9), arr.toSpliced(1, 1), arr);
```

```text
true 1
false { a: 1, n: { b: 2 } }
[ 1, 2, 3 ] [ 3, 1, 2 ] [ 2, 1, 3 ] [ 9, 1, 2 ] [ 3, 2 ] [ 3, 1, 2 ]
```

- `{ ...s }` — **поверхностная** копия: `s2.n === s.n` — `true`.
- `structuredClone` делает глубокую копию, но не умеет копировать функции (`DataCloneError`, см. [экспертную теорию](expert.ru.md#справочник)).
- Методы `toSorted`, `toReversed`, `with`, `toSpliced` возвращают новый массив и не меняют исходный (в отличие от `sort`, `reverse`, `splice`).

## Лучшие практики

- Передавайте в `map`/`filter` явные стрелки вместо «голых» функций с несколькими параметрами.
- Всегда задавайте начальное значение в `reduce`.
- Мемоизируйте только чистые функции и ограничивайте размер кэша.
- Копируйте перед изменением (spread, `toSorted`), а не мутируйте аргументы.
- Помните, что `freeze` и spread неглубоки.

## Частые ошибки

- ❌ `['1', '2', '3'].map(parseInt)` → ✅ `map(Number)`.
- ❌ `reduce` без начального значения на возможно пустом массиве → ✅ `reduce(f, init)`.
- ❌ Каррирование функции со значениями по умолчанию через `fn.length` → ✅ явное число аргументов.
- ❌ Мемоизация с объектом в роли ключа → ✅ строковый ключ или `WeakMap`.
- ❌ Ожидание, что `Object.freeze` защитит вложенные объекты → ✅ `deepFreeze`.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Методы массивов: [07 · Массивы и коллекции](../07-arrays-and-collections/advanced.ru.md)
- Функции, `bind`, `length`: [04 · Функции](../04-functions/advanced.ru.md)

## Источники

- [MDN: Array.prototype.reduce](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/reduce)
- [MDN: Function.prototype.bind](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function/bind)
- [MDN: Function: length](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function/length)
- [MDN: Object.freeze](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/freeze)
- [MDN: structuredClone](https://developer.mozilla.org/en-US/docs/Web/API/Window/structuredClone)
- [MDN: Array.prototype.toSorted](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/toSorted)
