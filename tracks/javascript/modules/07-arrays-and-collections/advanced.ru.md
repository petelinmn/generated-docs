---
title: "Массивы и коллекции: продвинутая теория"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 07-arrays-and-collections
part: 1
tags: [set, map, weakmap, weakset, typed-array, arraybuffer, dataview, set-operations, samevaluezero]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Массивы и коллекции: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры выполнены на Node.js 24.15 в файле ES-модуля.

## Set

`Set` хранит уникальные значения в порядке вставки:

```js
const s = new Set([1, 2, 2, 3, NaN, NaN, 0, -0]);
console.log(s, s.size, s.has(NaN), new Set([{}, {}]).size);
console.log([...new Set('hello')].join(''), [1, 2, 2, 3].filter((x, i, a) => a.indexOf(x) === i));
```

```text
Set(5) { 1, 2, 3, NaN, 0 } 5 true 2
helo [ 1, 2, 3 ]
```

- Сравнение — **SameValueZero**: `NaN` равен `NaN`, а `0` и `-0` считаются одним значением.
- Объекты сравниваются по ссылке: два пустых литерала — два элемента.
- Быстрый способ убрать дубликаты: `[...new Set(arr)]`.

### Операции над множествами

В Node.js 24 у `Set` есть методы теории множеств:

```js
const A = new Set([1, 2, 3]), B = new Set([2, 3, 4]);
console.log(A.union(B), A.intersection(B), A.difference(B), A.symmetricDifference(B));
console.log(A.isSubsetOf(new Set([1, 2, 3, 4])), A.isDisjointFrom(new Set([9])));
```

```text
Set(4) { 1, 2, 3, 4 } Set(2) { 2, 3 } Set(1) { 1 } Set(2) { 1, 4 }
true true
```

Методы возвращают новое множество и не меняют исходные.

> ⚠️ Проверить: в старых средах (Node.js ниже 22 и устаревшие браузеры) этих методов нет; поддержку нужно проверять по таблице совместимости.

## Map

`Map` хранит пары «ключ — значение»; ключом может быть что угодно:

```js
const mp = new Map([['a', 1], [1, 'one']]);
mp.set({}, 'obj').set(NaN, 'nan');
console.log(mp.size, mp.get('a'), mp.get(NaN), mp.get({}), [...mp.keys()].length);
console.log(new Map(Object.entries({ x: 1 })), Object.fromEntries(new Map([['k', 2]])), [...new Map([[1, 'a'], [2, 'b']])]);
console.log(JSON.stringify(new Map([[1, 2]])), JSON.stringify([...new Map([[1, 2]])]));
const order = new Map(); order.set('b', 1).set('a', 2).set('b', 3);
console.log([...order]);
mp.delete('a'); console.log(mp.has('a'), mp.size); mp.clear(); console.log(mp.size);
console.log(Map.groupBy([1, 2, 3], n => n % 2).get(1));
```

```text
4 1 nan undefined 4
Map(1) { 'x' => 1 } { k: 2 } [ [ 1, 'a' ], [ 2, 'b' ] ]
{} [[1,2]]
[ [ 'b', 3 ], [ 'a', 2 ] ]
false 3
0
[ 1, 3 ]
```

- `set` возвращает сам `Map`, поэтому вызовы можно цепочкой.
- `mp.get({})` вернул `undefined`: новый литерал — другой ключ; `NaN` находится.
- Повторный `set('b', 3)` меняет значение, но не позицию: порядок остаётся порядком первой вставки.
- Преобразования: `new Map(Object.entries(obj))` и `Object.fromEntries(map)`; `JSON.stringify(map)` даёт `{}`, нужно разворачивать пары.
- `Map.groupBy` группирует в `Map` (ключи могут быть любыми).

### Map или обычный объект

| Свойство | `Map` | Объект |
|---|---|---|
| Ключи | любые | строки и символы |
| Порядок | порядок вставки | индексы по возрастанию, затем вставка |
| Размер | `map.size` | `Object.keys(o).length` |
| Прототип | нет унаследованных ключей | есть (`toString`, `__proto__`) |
| JSON | не сериализуется напрямую | сериализуется |
| Частые добавления/удаления | рассчитан на это | не оптимален |

## WeakMap и WeakSet

Слабые коллекции держат ключи «слабо»: объект-ключ может быть удалён сборщиком мусора, если на него нет других ссылок.

```js
const wm = new WeakMap(); const key = {};
wm.set(key, 'meta');
console.log(wm.get(key), wm.has({}), typeof wm.size);
try { wm.set('str', 1) } catch (e) { console.log(e.name + ': ' + e.message) }
try { new WeakSet().add(1) } catch (e) { console.log(e.name + ': ' + e.message) }
try { [...wm] } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
meta false undefined
TypeError: Invalid value used as weak map key
TypeError: Invalid value used in weak set
TypeError: wm is not iterable
```

- Ключами могут быть только объекты (и незарегистрированные символы): строка — `TypeError`.
- Нет `size`, итерации и `clear`: содержимое зависит от сборки мусора и не должно быть наблюдаемо.
- Применение: приватные данные и кэши, привязанные к объекту, без утечек памяти.

> ⚠️ Проверить: фактическое освобождение памяти (поведение сборщика мусора, `FinalizationRegistry`, `WeakRef`) в опытах не наблюдалось; подробнее — в модуле 19 дорожной карты.

## Типизированные массивы

`Uint8Array`, `Int16Array`, `Float32Array` и другие — массивы чисел **одного фиксированного типа** поверх буфера `ArrayBuffer`:

```js
const u8 = new Uint8Array([250, 255, 256, -1, 1.9]);
console.log(u8, u8.length, u8.BYTES_PER_ELEMENT);
console.log(new Uint8ClampedArray([300, -5, 1.5]), new Int8Array([127, 128]), new Float32Array([0.1])[0]);
console.log(Array.isArray(u8), u8 instanceof Uint8Array, u8.map(x => x * 2), u8.slice(1, 3), u8.subarray(1, 3).buffer === u8.buffer);
```

```text
Uint8Array(5) [ 250, 255, 0, 255, 1 ] 5 1
Uint8ClampedArray(3) [ 255, 0, 2 ] Int8Array(2) [ 127, -128 ] 0.10000000149011612
false true Uint8Array(5) [ 244, 254, 0, 254, 2 ] Uint8Array(2) [ 255, 0 ] true
```

- Значения **«заворачиваются»** по модулю типа: `256` → `0`, `-1` → `255`, дробная часть отбрасывается (`1.9` → `1`); `Int8Array([128])` — `-128`.
- `Uint8ClampedArray` вместо заворота **зажимает** в диапазон `0–255` (`300` → `255`, `-5` → `0`, `1.5` → `2`).
- `Float32Array` хранит число с меньшей точностью (`0.1` → `0.10000000149011612`).
- Типизированный массив — не `Array` (`Array.isArray` — `false`): у него нет `push` и других методов, меняющих длину, а `map` возвращает массив **того же типа** (значения снова заворачиваются).
- `subarray` создаёт «вид» на тот же буфер (общий `buffer`), а `slice` — копию.

### ArrayBuffer и DataView

`DataView` читает и пишет числа в буфере с указанием порядка байтов:

```js
const buf = new ArrayBuffer(8);
const dv = new DataView(buf);
dv.setInt16(0, 258);
console.log(new Uint8Array(buf, 0, 2), dv.getInt16(0), dv.getInt16(0, true));
```

```text
Uint8Array(2) [ 1, 2 ] 258 513
```

Число `258` (`0x0102`) записано в порядке «старший байт первым» (big-endian) как байты `1, 2`. Чтение с флагом `true` трактует байты как little-endian и даёт `513`. Это нужно для бинарных протоколов и файлов.

## Лучшие практики

- Для списков — `Array` и неизменяющие методы там, где массив чужой.
- Для проверок принадлежности и уникальности — `Set` (`has` быстрее, чем `includes`).
- Для словарей с динамическими ключами — `Map`.
- Метаданные объектов — `WeakMap`.
- Для байтов — типизированные массивы и `DataView`.
- Не смешивайте дырки и `undefined`.

> ⚠️ Проверить: утверждения о скорости (`Set.has` быстрее `Array.includes`, `Map` быстрее объекта при частых изменениях) взяты из общих знаний и в опытах не измерялись.

## Частые ошибки

- ❌ `JSON.stringify(map)` → ✅ `JSON.stringify([...map])` или `Object.fromEntries(map)`.
- ❌ Ждать `set.has({})` для литерала-копии → ✅ сравнение по ссылке.
- ❌ Использовать строку как ключ `WeakMap` → ✅ объект.
- ❌ Складывать большие числа в `Uint8Array` и удивляться заворотам → ✅ нужный тип или `Uint8ClampedArray`.
- ❌ Ждать порядка по значению в `Map` → ✅ порядок — вставки.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Копирование и `groupBy`: [05 · Объекты и прототипы](../05-objects-and-prototypes/basics.ru.md#перечисление-и-преобразования)
- Двоичные данные в Node.js: модуль 16 дорожной карты [трека](../../overview.ru.md)

## Источники

- [MDN: Set](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Set)
- [MDN: Map](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Map)
- [MDN: WeakMap](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/WeakMap)
- [MDN: TypedArray](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/TypedArray)
- [MDN: DataView](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/DataView)
