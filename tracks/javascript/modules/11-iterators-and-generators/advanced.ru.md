---
title: "Итераторы и генераторы: продвинутый уровень"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 11-iterators-and-generators
part: 2
tags: [generator, yield-delegation, iterator-return, iterator-helpers, async-generator, for-await, array-fromasync]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Итераторы и генераторы: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры запускались на Node.js 24.15.

## Двусторонний yield

`yield` — это и выход, и вход: значение из `next(x)` становится результатом выражения `yield`. Так генератор превращается в «сопрограмму»:

```js
function* avg() { let sum = 0, n = 0; while (true) { const x = yield n ? sum / n : undefined; sum += x; n++; } }
const a = avg(); a.next(); console.log(a.next(10).value, a.next(20).value, a.next(60).value);
```

```text
10 15 30
```

Первый `next()` только запускает генератор до первого `yield`; дальше каждое `next(x)` передаёт число, а возвращает текущее среднее.

## yield*: делегирование

`yield*` отдаёт значения другого итерируемого объекта, а его результатом становится значение `return` вложенного генератора:

```js
function* inner() { yield 'a'; return 'ret'; }
function* outer() { const r = yield* inner(); yield r; yield* [1, 2]; }
console.log([...outer()]);
```

```text
[ 'a', 'ret', 1, 2 ]
```

Значение `return` вложенного генератора (`'ret'`) попало в `r` и было отдано отдельным `yield`.

## return() и throw()

`return(v)` досрочно завершает генератор, `throw(e)` бросает исключение в точку `yield`. Блоки `try`/`catch`/`finally` внутри генератора работают:

```js
function* t() { try { yield 1; yield 2 } catch (e) { console.log('caught', e); yield 'recovered' } finally { console.log('cleanup') } }
const tg = t(); tg.next(); console.log(tg.throw('boom')); console.log(tg.return('bye')); console.log(tg.next());
const tg2 = t(); tg2.next(); console.log(tg2.return('early'));
```

```text
caught boom
{ value: 'recovered', done: false }
cleanup
{ value: 'bye', done: true }
{ value: undefined, done: true }
cleanup
{ value: 'early', done: true }
```

- `throw('boom')` был перехвачен внутри генератора, который продолжил и отдал `'recovered'`.
- `return('bye')` выполнил `finally` (`cleanup`) и завершил генератор.
- Для завершённого генератора `next()` возвращает `{ value: undefined, done: true }`.

## Закрытие итераторов

Если `for...of` прерывается (`break`, `return`, исключение) или деструктуризация берёт не все значения, вызывается `return()` итератора — если он есть:

```js
const withReturn = { [Symbol.iterator]() { let i = 0; return { next: () => ({ value: i++, done: false }), return() { console.log('return called'); return { done: true }; } }; } };
for (const x of withReturn) { if (x === 1) break; }
const [first] = withReturn;
```

```text
return called
return called
```

Поэтому генератор с `try/finally` освободит ресурс при `break`. После **нормального** завершения `return()` не вызывается; при исключении в теле цикла — вызывается.

## Хелперы итераторов

В ES2025 у итераторов есть методы, работающие лениво и не создающие промежуточных массивов: `map`, `filter`, `take`, `drop`, `flatMap`, `reduce`, `toArray`, `forEach`, `some`, `every`, `find`. В Node.js 24 они доступны:

```js
function* nat() { let n = 0; while (true) yield n++; }
console.log(nat().drop(3).take(2).toArray(), nat().take(5).reduce((a, b) => a + b, 0));
console.log(nat().take(5).some(x => x > 3), nat().take(5).every(x => x < 5), nat().find(x => x > 10));
console.log([1, 2].values().flatMap(x => [x, x * 10]).toArray());
console.log(typeof Iterator, Iterator.from([1, 2]).toArray());
```

```text
[ 3, 4 ] 10
true true 11
[ 1, 10, 2, 20 ]
function [ 1, 2 ]
```

- Хелперы есть у **итераторов**, а не у массивов: `[1, 2].values().map(...)` работает, а `[1, 2].map(...)` — это обычный метод массива. Результат хелпера — тоже итератор (`[object Iterator Helper]`), поэтому в конце нужен `toArray()`.
- Они **ленивы**: пока нет потребителя, функции не вызываются:

```js
const log = []; const lazy = [1, 2, 3].values().map(x => { log.push(x); return x; }).take(2);
console.log(log); lazy.toArray(); console.log(log);
```

```text
[]
[ 1, 2 ]
```

- У результата `map` нет `length`: `[1, 2].values().map(x => x).length` — `undefined`.
- `Iterator.from(obj)` оборачивает итерируемый объект или объект с `next()` в настоящий итератор.

## Асинхронные итераторы и генераторы

Асинхронный итератор возвращает из `next()` **промис** результата. Асинхронный генератор — `async function*` — использует `await` и `yield` вместе; его перебирают через `for await...of`:

```js
async function* ag() { yield 1; await null; yield 2; }
for await (const v of ag()) console.log('av', v);
const a = ag(); console.log(await a.next(), await a.return('r'), await a.next());
console.log(Object.prototype.toString.call(a), typeof a[Symbol.asyncIterator]);
```

```text
av 1
av 2
{ value: 1, done: false } { value: 'r', done: true } { value: undefined, done: true }
[object AsyncGenerator] function
```

`for await` работает и с обычными итерируемыми объектами: значения-промисы он дожидается:

```js
for await (const v of [Promise.resolve('p1'), 'plain']) console.log(v);
```

```text
p1
plain
```

Типичный сценарий — постраничная загрузка или поток событий:

```js
const sleep = ms => new Promise(r => setTimeout(r, ms));
async function* pages(n) { for (let p = 1; p <= n; p++) { await sleep(1); yield { page: p, items: [p * 10, p * 10 + 1] }; } }
const all = []; for await (const { items } of pages(3)) all.push(...items); console.log(all);
```

```text
[ 10, 11, 20, 21, 30, 31 ]
```

- `for await` **последователен**: следующий шаг начинается после завершения предыдущего; для параллелизма нужны промисы ([модуль 12](../12-promises-and-async-await/overview.ru.md)).
- Ошибка в асинхронном генераторе превращается в отклонённый промис и перехватывается `try/catch` вокруг `for await` (в опыте `caught oops`).
- Досрочный `break` вызывает `return()`: `finally` в асинхронном генераторе выполняется (в опыте `released`).
- `Array.fromAsync(ag())` собирает асинхронный итерируемый объект в массив: `[ 1, 2 ]`; для массива с промисами — тоже (`[ 1, 2 ]`).

## Лучшие практики

- Делайте `[Symbol.iterator]` генератором: проще и корректно закрывается.
- Бесконечные генераторы всегда ограничивайте (`take`, `break`).
- Освобождайте ресурсы в `finally` генератора.
- Для длинных конвейеров используйте хелперы итераторов, а не промежуточные массивы.
- Не используйте `for await` для независимых запросов, которые можно запустить одновременно.

## Частые ошибки

- ❌ Забыть, что первый `next(x)` отбрасывает `x` → ✅ первый вызов — запуск.
- ❌ Ждать, что `yield*` вернёт «значения» вместо `return` → ✅ результат `yield*` — `return` вложенного.
- ❌ Вызывать `.map` на генераторе в окружении без хелперов итераторов → ✅ проверить версию Node.js (24 поддерживает).
- ❌ Разбирать большой поток в массив целиком → ✅ перебирать лениво.
- ❌ Полагаться на `return()` после нормального завершения → ✅ он не вызывается.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Промисы и `async`/`await`: [12 · Промисы и async/await](../12-promises-and-async-await/overview.ru.md)
- Методы массивов: [07 · Массивы и коллекции](../07-arrays-and-collections/advanced.ru.md)

## Источники

- [MDN: Iterator helpers](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Iterator)
- [MDN: yield*](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/yield*)
- [MDN: Generator.prototype.return](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Generator/return)
- [MDN: for await...of](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/for-await...of)
- [MDN: async function*](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function*)
- [MDN: Array.fromAsync](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/fromAsync)
