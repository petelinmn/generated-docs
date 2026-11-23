---
title: "Итераторы и генераторы: основы"
lang: ru
track: javascript
kind: module
doc: basics
slug: 11-iterators-and-generators
part: 2
tags: [iterator, iterable, for-of, spread, generator, yield, lazy-evaluation]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Итераторы и генераторы: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [07 · Массивы и коллекции](../07-arrays-and-collections/overview.ru.md), [10 · Замыкания и функциональный стиль](../10-closures-and-functional/overview.ru.md) · **Время:** ~2.5 ч

Все примеры запускались на Node.js 24.15; файлы лежат рядом с `package.json` (`{"type": "module"}`).

## Зачем это нужно

Перебор значений встречается везде: массивы, строки, `Map`, `Set`, результаты запросов. Протокол итерации даёт единый способ перебирать любые источники и позволяет описывать свои. Генераторы дают ленивые последовательности, в том числе бесконечные.

## Протокол итерации

Итератор — объект с методом `next()`, возвращающим `{ value, done }`. Итерируемый объект — тот, у кого есть метод `[Symbol.iterator]()`, возвращающий итератор:

```js
const it = [10, 20][Symbol.iterator]();
console.log(it.next(), it.next(), it.next(), typeof it[Symbol.iterator], it[Symbol.iterator]() === it);
console.log(Object.prototype.toString.call(it), Object.prototype.toString.call(function* () {}()));
```

```text
{ value: 10, done: false } { value: 20, done: false } { value: undefined, done: true } function true
[object Array Iterator] [object Generator]
```

- Пока значения есть, `done` — `false`; после конца — `{ value: undefined, done: true }`.
- У встроенных итераторов есть и свой `[Symbol.iterator]`, возвращающий самого себя (`true`): итератор сам итерируем.

## Что принимает итерируемые объекты

```js
console.log([...'a😀b'], [...new Map([[1, 'x']])], [...new Set([1, 1, 2])], Object.entries({ a: 1 }));
console.log([...[1, 2].entries()], [...[1, 2].keys()]);
```

```text
[ 'a', '😀', 'b' ] [ [ 1, 'x' ] ] [ 1, 2 ] [ [ 'a', 1 ] ]
[ [ 0, 1 ], [ 1, 2 ] ] [ 0, 1 ]
```

- Строка итерируется по **символам Unicode** (code points), а не по кодовым единицам: `😀` — один элемент ([модуль 08](../08-strings-numbers-dates-regex/overview.ru.md)).
- `Map` отдаёт пары `[ключ, значение]`, `Set` — значения без повторов.
- Потребители итерируемых: `for...of`, spread (`...`), деструктуризация, `Array.from`, `new Map(...)`, `new Set(...)`, `Promise.all` и др.
- Обычный объект не итерируем: `Object.entries(obj)` превращает его в итерируемый массив пар.

```js
try { for (const x of {}) ; } catch (e) { console.log(e.name + ': ' + e.message) }
try { [...123] } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
TypeError: {} is not iterable
TypeError: 123 is not iterable
```

## Свой итерируемый объект

```js
class Range {
  constructor(a, b) { this.a = a; this.b = b }
  [Symbol.iterator]() {
    let i = this.a, b = this.b;
    return { next: () => i < b ? { value: i++, done: false } : { value: undefined, done: true } };
  }
}
console.log([...new Range(0, 3)], Array.from(new Range(1, 4)), Math.max(...new Range(0, 5)));
const [p, q] = new Range(5, 10); console.log(p, q);
for (const v of new Range(0, 2)) console.log('v', v);
```

```text
[ 0, 1, 2 ] [ 1, 2, 3 ] 4
5 6
v 0
v 1
```

Состояние (`i`) лежит в замыкании: каждый вызов `[Symbol.iterator]()` создаёт независимый перебор.

## Генераторы

Функция со звёздочкой `function*` — генератор. Вызов не выполняет тело, а возвращает объект-генератор; `yield` приостанавливает выполнение и отдаёт значение:

```js
function* gen() { console.log('start'); const x = yield 1; console.log('got', x); const y = yield x * 2; return y; }
const g = gen();
console.log(g.next('ignored')); console.log(g.next(5)); console.log(g.next('end')); console.log(g.next());
```

```text
start
{ value: 1, done: false }
got 5
{ value: 10, done: false }
{ value: 'end', done: true }
{ value: undefined, done: true }
```

- Тело начинает выполняться только при первом `next()`; аргумент первого `next` отбрасывается.
- `next(5)` возвращает `5` как результат выражения `yield 1`.
- `return` задаёт последнее значение с `done: true`; после этого — `{ value: undefined, done: true }`.
- Генератор можно пройти **один раз**: повторный `[...once]` даёт пустой массив.

Тот же `Range` короче через генератор:

```js
class Countdown { constructor(n) { this.n = n } *[Symbol.iterator]() { for (let i = this.n; i > 0; i--) yield i; } }
console.log([...new Countdown(3)]);
```

```text
[ 3, 2, 1 ]
```

## Ленивость и бесконечные последовательности

Генератор вычисляет следующее значение только по запросу, поэтому может быть бесконечным:

```js
function* nat() { let n = 0; while (true) yield n++; }
function* take(n, it) { let i = 0; for (const v of it) { if (i++ >= n) return; yield v; } }
console.log([...take(5, nat())]);
function* r() { yield 1; return 99; } console.log([...r()]);
```

```text
[ 0, 1, 2, 3, 4 ]
[ 1 ]
```

- `take` останавливает бесконечный перебор: `for...of` не доходит до конца.
- `for...of` и spread **игнорируют** значение `return` (`99` в результат не попало).

## Ошибки новичков

- ❌ Перебирать обычный объект через `for...of` → ✅ `Object.entries(obj)`.
- ❌ Ждать, что `[...nat()]` для бесконечного генератора завершится → ✅ ограничить `take` или `break`.
- ❌ Использовать генератор повторно → ✅ создать новый вызовом функции.
- ❌ Ждать `yield` из `return` в `for...of` → ✅ использовать `yield`.
- ❌ Вызывать `new` для генератора → ✅ обычный вызов (`new gen()` — `TypeError`).

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — двусторонний `yield`, `yield*`, хелперы, асинхронные итераторы
- Ранее: [07 · Массивы и коллекции](../07-arrays-and-collections/basics.ru.md#итерация)

## Источники

- [MDN: Iteration protocols](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Iteration_protocols)
- [MDN: function*](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/function*)
- [MDN: yield](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/yield)
- [MDN: for...of](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/for...of)
