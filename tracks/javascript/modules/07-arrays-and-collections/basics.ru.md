---
title: "Массивы и коллекции: базовая теория"
lang: ru
track: javascript
kind: module
doc: basics
slug: 07-arrays-and-collections
part: 1
tags: [array, array-methods, sort, reduce, flat, array-holes, iteration, array-from]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Массивы и коллекции: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · Функции](../04-functions/overview.ru.md), [05 · Объекты и прототипы](../05-objects-and-prototypes/overview.ru.md) · **Время:** ~3 ч

Все примеры выполнены на Node.js 24.15 в файле ES-модуля.

## Зачем это нужно

Списки данных встречаются в любой программе: ответы API, строки таблицы, события. Массивы и их методы — главный инструмент их обработки.

## Создание

```js
console.log(Array.of(3), Array(3), new Array(3).fill(0), Array.from({ length: 3 }, (_, i) => i * i));
console.log(Array.from('héy'), Array.from(new Set([1, 1, 2])), Array.isArray([]), typeof []);
try { new Array(-1) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
[ 3 ] [ <3 empty items> ] [ 0, 0, 0 ] [ 0, 1, 4 ]
[ 'h', 'é', 'y' ] [ 1, 2 ] true object
RangeError: Invalid array length
```

- Лучший способ — литерал `[1, 2, 3]`.
- `Array(3)` — три **пустых слота** («дырки»), а не три `undefined`; `Array.of(3)` — массив из одного элемента `3`. Различие легко забыть.
- `Array.from(источник, функция)` строит массив из итерируемого объекта или «массивоподобного» `{ length: n }` и сразу преобразует элементы.
- Проверка на массив — `Array.isArray(x)`, а не `typeof` (он даёт `'object'`).

## length и «дырки»

```js
const a = [1, 2, 3]; a.length = 1;
console.log(a);
a[4] = 9;
console.log(a, a.length, 2 in a, a.indexOf(undefined), a.includes(undefined));
console.log(Object.keys([1, , 3]), [1, , 3].map(x => x * 2));
[, 1].forEach(x => console.log('visited', x));
```

```text
[ 1 ]
[ 1, <3 empty items>, 9 ] 5 false -1 true
[ '0', '2' ] [ 2, <1 empty item>, 6 ]
visited 1
```

- Присвоение `length = 1` **обрезает** массив.
- Запись `a[4] = 9` за концом увеличивает `length` до `5` и оставляет «дырки»: индекса `2` в массиве нет (`2 in a` — `false`).
- `indexOf(undefined)` дырок не находит (`-1`), а `includes(undefined)` — находит (`true`).
- `map` и `forEach` **пропускают** дырки (`map` сохраняет их в результате); `Object.keys` их не видит.

Лучше избегать дырок: создавайте массив нужной длины через `Array.from({ length: n }, ...)` или `.fill()`.

## Изменяющие и копирующие методы

Часть методов меняет исходный массив, другие — возвращают новый:

```js
let m = [3, 1, 2];
const sorted = m.toSorted();
console.log(sorted, m);
const same = m.sort();
console.log(same === m, m);
console.log(m.toReversed(), m);
console.log(m.with(0, 99), m, m.toSpliced(1, 1), m);
const removed = m.splice(1, 1);
console.log(removed, m);
console.log(m.slice(-2), m.concat([7], 8, [[9]]));
```

```text
[ 1, 2, 3 ] [ 3, 1, 2 ]
true [ 1, 2, 3 ]
[ 3, 2, 1 ] [ 1, 2, 3 ]
[ 99, 2, 3 ] [ 1, 2, 3 ] [ 1, 3 ] [ 1, 2, 3 ]
[ 2 ] [ 1, 3 ]
[ 1, 3 ] [ 1, 3, 7, 8, [ 9 ] ]
```

| Меняет массив | Возвращает копию |
|---|---|
| `sort`, `reverse`, `splice`, `fill`, `push`, `pop`, `shift`, `unshift`, `copyWithin` | `toSorted`, `toReversed`, `toSpliced`, `with`, `slice`, `concat`, `map`, `filter`, `flat` |

- `sort()` возвращает тот же массив (`same === m`), поэтому его результат — не копия.
- `splice(начало, сколько, ...вставить)` удаляет и вставляет, возвращая удалённые элементы.
- `slice(начало, конец)` не меняет массив; отрицательные индексы считаются с конца.
- `concat` «разворачивает» массивы-аргументы на один уровень (`[[9]]` дал `[ 9 ]` как один элемент).

Неизменяющие методы удобны, когда массив принадлежит другому коду (например, передан в функцию).

## Поиск

```js
const people = [{ n: 'a', age: 20 }, { n: 'b', age: 30 }, { n: 'c', age: 30 }];
console.log(people.find(p => p.age === 30).n, people.findLast(p => p.age === 30).n, people.findIndex(p => p.age > 99), people.findLastIndex(p => p.age === 30));
console.log([1, 2, 3].includes(2), [NaN].includes(NaN), [NaN].indexOf(NaN), [1, 2, 3].at(-1), [1, 2, 3].some(x => x > 2), [].every(x => false), [].some(x => true));
```

```text
b c -1 2
true true -1 3 true true false
```

- `find`/`findLast` возвращают элемент (или `undefined`), `findIndex`/`findLastIndex` — индекс (или `-1`).
- `includes` находит `NaN`, а `indexOf` — нет (разные алгоритмы сравнения).
- `at(-1)` — последний элемент.
- `some` и `every` на пустом массиве дают `false` и `true` соответственно.

## map, filter, reduce, flat

```js
console.log([1, 2, 3].map(x => x * 2), [1, 2, 3, 4].filter(x => x % 2), [1, 2, 3].reduce((s, x) => s + x));
console.log([[1, 2], [3, [4]]].flat(), [[1, 2], [3, [4]]].flat(Infinity), [1, 2].flatMap(x => [x, x * 10]));
console.log([].reduce((a, b) => a + b, 'init'), [1, 2, 3].reduceRight((a, x) => a + x, ''));
try { [].reduce((a, b) => a + b) } catch (e) { console.log(e.name + ': ' + e.message) }
console.log(Object.groupBy([1, 2, 3, 4, 5], n => (n % 2 ? 'odd' : 'even')));
```

```text
[ 2, 4, 6 ] [ 1, 3 ] 6
[ 1, 2, 3, [ 4 ] ] [ 1, 2, 3, 4 ] [ 1, 10, 2, 20 ]
init 321
TypeError: Reduce of empty array with no initial value
[Object: null prototype] { odd: [ 1, 3, 5 ], even: [ 2, 4 ] }
```

- `map` преобразует каждый элемент; `filter` оставляет подходящие; `reduce(функция, начальное)` сворачивает массив в одно значение.
- `flat(глубина)` «выравнивает» вложенность (`Infinity` — полностью); `flatMap` = `map` + `flat(1)`.
- У `reduce` на пустом массиве без начального значения — `TypeError`: **всегда задавайте начальное значение**.
- `reduceRight` идёт справа налево.

## Сортировка

```js
console.log([10, 9, 1].sort(), [10, 9, 1].sort((a, b) => a - b));
console.log(['b', 'a', 'C'].sort(), ['b', 'a', 'C'].sort((x, y) => x.localeCompare(y)), [3, undefined, 1, , 2].sort());
const st = [{ k: 1, v: 'a' }, { k: 0, v: 'b' }, { k: 1, v: 'c' }, { k: 0, v: 'd' }];
console.log(st.sort((x, y) => x.k - y.k).map(o => o.v).join(''));
```

```text
[ 1, 10, 9 ] [ 1, 9, 10 ]
[ 'C', 'a', 'b' ] [ 'a', 'b', 'C' ] [ 1, 2, 3, undefined, <1 empty item> ]
bdac
```

- Без функции сравнения `sort` сравнивает **строковые представления**: `10` идёт раньше `9`.
- Функция сравнения `(a, b) => a - b` возвращает отрицательное, ноль или положительное число.
- Строки по умолчанию сортируются по кодам символов (`'C'` раньше `'a'`); для «человеческого» порядка — `localeCompare`.
- `undefined` и дырки уходят в конец.
- Сортировка **стабильна**: элементы с равным ключом сохраняют исходный порядок (`'bd'` и `'ac'` — как были).

## Итерация

```js
for (const [i, v] of ['x', 'y'].entries()) console.log(i, v);
console.log([...['a', 'b'].keys()]);
const arr = [1, 2, 3]; arr.extra = 'e';
const ks = []; for (const i in arr) ks.push(i);
console.log(ks, arr.length);
```

```text
0 x
1 y
[ 0, 1 ]
[ '0', '1', '2', 'extra' ] 3
```

- `for...of` перебирает значения, `entries()` даёт пары `[индекс, значение]`, `keys()` — индексы.
- `for...in` перебирает **ключи-строки** и включает добавленные свойства (`'extra'`): для массивов его не используют.

## Копирование и сравнение

```js
const o = [{ x: 1 }]; const c = [...o]; c[0].x = 2;
console.log(o[0].x, [1] === [1], JSON.stringify([1, 2]) === JSON.stringify([1, 2]));
function f() { return Array.from(arguments).map(x => x * 2); }
console.log(f(1, 2), Array.prototype.map.call('abc', c => c.toUpperCase()), Array.from({ 0: 'a', 1: 'b', length: 2 }));
```

```text
2 false true
[ 2, 4 ] [ 'A', 'B', 'C' ] [ 'a', 'b' ]
```

- `[...o]` и `slice()` — **поверхностные** копии: вложенный объект общий (`o[0].x` стал `2`); для глубокой копии — `structuredClone`.
- Массивы сравниваются по ссылке; сравнение содержимого — поэлементно (`JSON.stringify` работает не для всех значений, [модуль 05](../05-objects-and-prototypes/advanced.ru.md#глубокое-копирование)).
- Методы массива работают с «массивоподобными» объектами (`arguments`, строка, `{ length: 2, 0: ..., 1: ... }`) через `Array.from` или `call`.

## Ошибки новичка

- ❌ `Array(3)` вместо `[3]` или `Array.of(3)` → ✅ помнить про дырки.
- ❌ `[10, 9, 1].sort()` для чисел → ✅ `sort((a, b) => a - b)`.
- ❌ `sort()` на массиве, который нельзя менять → ✅ `toSorted()`.
- ❌ `reduce` без начального значения → ✅ задавать его.
- ❌ `for...in` по массиву → ✅ `for...of`.
- ❌ Считать `[...arr]` глубокой копией → ✅ `structuredClone`.
- ❌ Искать `NaN` через `indexOf` → ✅ `includes`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — `Set`, `Map`, слабые коллекции, типизированные массивы
- Ранее: [05 · Объекты и прототипы](../05-objects-and-prototypes/overview.ru.md) (`Object.groupBy`, `structuredClone`)

## Источники

- [MDN: Array](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array)
- [MDN: Indexed collections](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Indexed_collections)
- [MDN: Array.prototype.sort()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/sort)
- [MDN: Sparse arrays](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array#sparse_arrays)
- [ECMAScript: Array Objects](https://tc39.es/ecma262/#sec-array-objects)
