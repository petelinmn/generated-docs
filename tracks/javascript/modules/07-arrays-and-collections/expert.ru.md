---
title: "Массивы и коллекции: экспертная теория"
lang: ru
track: javascript
kind: module
doc: expert
slug: 07-arrays-and-collections
part: 1
tags: [array-exotic-object, samevaluezero, sort-stability, sparse-arrays, typed-arrays, map-set-ordering, array-methods]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Массивы и коллекции: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все результаты получены запуском на Node.js 24.15 (ES-модуль).

## Справочник

### Методы массива

| Группа | Методы | Меняют исходный массив |
|---|---|---|
| Создание | `Array.of`, `Array.from`, `Array(n)`, `[...x]` | — |
| Добавление и удаление | `push`, `pop`, `shift`, `unshift`, `splice` | да |
| Перестановка | `sort`, `reverse`, `fill`, `copyWithin` | да (в опыте `sort` вернул тот же массив) |
| Копирующие аналоги | `toSorted`, `toReversed`, `toSpliced`, `with` | нет |
| Срезы и соединение | `slice`, `concat`, `join`, `flat`, `flatMap` | нет |
| Поиск | `indexOf`, `lastIndexOf`, `includes`, `find`, `findIndex`, `findLast`, `findLastIndex`, `at` | нет |
| Проверки | `some`, `every`, `Array.isArray` | нет |
| Обход | `forEach`, `map`, `filter`, `reduce`, `reduceRight`, `entries`, `keys`, `values` | нет |
| Группировка | `Object.groupBy`, `Map.groupBy` (статические, не методы массива) | нет |

> ⚠️ Проверить: `push`, `pop`, `shift`, `unshift`, `values`, `lastIndexOf` и `copyWithin` в этой теме упомянуты по документации; в опытах запускались `fill`, `copyWithin`, `lastIndexOf`, `splice`.

### Сравнение значений: три алгоритма

| Операция | Алгоритм | `NaN` | `0` и `-0` |
|---|---|---|---|
| `===`, `indexOf`, `lastIndexOf` | строгое равенство | `NaN !== NaN` | равны |
| `includes`, `Set`, `Map` | SameValueZero | равны | равны (в опыте `Set` схлопнул `0` и `-0`) |
| `Object.is` | SameValue | равны | различны |

В опыте: `[NaN].indexOf(NaN)` — `-1`, `[NaN].includes(NaN)` — `true`, `new Set([NaN, NaN]).size` — `1`.

### Коллекции: сводка

| | `Array` | `Set` | `Map` | `WeakMap`/`WeakSet` |
|---|---|---|---|---|
| Порядок | по индексу | вставки | вставки | нет |
| Уникальность | нет | значения | ключи | ключи |
| Тип ключа/значения | любые | любые | любые ключи | только объекты (и несвязанные символы) |
| Итерация | да | да | да | нет |
| `size`/`length` | `length` | `size` | `size` | нет |

### Типизированные массивы

`Int8Array`, `Uint8Array`, `Uint8ClampedArray`, `Int16Array`, `Uint16Array`, `Int32Array`, `Uint32Array`, `Float32Array`, `Float64Array`, `BigInt64Array`, `BigUint64Array`. Размер элемента — `BYTES_PER_ELEMENT` (в опыте `Uint8Array` — `1`).

> ⚠️ Проверить: `Float16Array`, `BigInt64Array`, `Atomics` и `SharedArrayBuffer` в опытах не запускались.

## Внутреннее устройство

- **Массив — «экзотический» объект.** Индексы — обычные свойства со строковыми ключами (`'0'`, `'1'`), но `length` ведёт себя особо: запись `length = n` удаляет элементы с индексами ≥ n (в опыте `a.length = 1`), а запись по индексу ≥ `length` увеличивает `length` (в опыте `a[4] = 9` → `length` 5).
- **Дырки** — отсутствующие индексы: `2 in a` ложно, `Object.keys` их пропускает. Методы `map`, `forEach`, `filter` обходят только существующие индексы, `join` и итерация (`for...of`, `...`) — все, превращая дырки в `undefined`/пустые строки (в опыте `Array(3).join('-')` — `'--'`, а `[...Array(2).keys()]` — `[0, 1]`).
- **Сортировка.** `sort` стабилен (требование спецификации с ES2019; в опыте `'bdac'`), по умолчанию сравнивает результаты `String(x)` по кодовым единицам UTF-16; `undefined` и дырки всегда в конце.
- **`Map` и `Set`** по спецификации хранят записи в порядке вставки и обновление существующего ключа его позицию не меняет (в опыте `order` — `[['b', 3], ['a', 2]]`).
- **Слабые коллекции.** Ключ не удерживает объект от сборки мусора; из-за этого нет итерации и `size` (в опыте `typeof wm.size` — `'undefined'`, `[...wm]` — `TypeError`).
- **Типизированные массивы — виды на `ArrayBuffer`.** Несколько видов могут делить один буфер; `subarray` не копирует (в опыте `u8.subarray(1, 3).buffer === u8.buffer`), `slice` копирует. Чтение и запись приводят число к типу элемента (`ToUint8`: по модулю 256; `Uint8Clamped`: зажатие с округлением к ближайшему чётному).
- **`DataView`** читает и пишет с явным порядком байтов; без флага — big-endian, с `true` — little-endian (в опыте `258` и `513`).

> ⚠️ Проверить: внутренние виды элементов V8 (`PACKED_SMI`, `HOLEY_DOUBLE` и т. д.), влияние дырок на скорость и точный алгоритм сортировки (TimSort в V8) взяты из общих знаний и в опытах не проверялись; детали — в модуле 19 дорожной карты.

## Подвохи

### 1. `sort()` без функции сравнивает строки

`[10, 9, 1].sort()` → `[ 1, 10, 9 ]`.

### 2. `sort`, `reverse`, `splice` меняют исходный массив

`const same = m.sort(); same === m` — `true` (в опыте). Используйте `toSorted`, `toReversed`, `toSpliced`.

### 3. Дырки против `undefined`

`Array(2).map(() => 1)` → `[ <2 empty items> ]`, а `[undefined, undefined].map(() => 1)` → `[ 1, 1 ]` (в опыте). `[, 1].forEach` вызывает функцию один раз.

### 4. `indexOf(NaN)` не находит

`[NaN].indexOf(NaN)` — `-1`; нужен `includes`.

### 5. `reduce` без начального значения

На пустом массиве — `TypeError: Reduce of empty array with no initial value`.

### 6. `for...in` перебирает строки и лишнее

`['0','1','2','extra']` для массива с добавленным свойством (в опыте).

### 7. `new Array(n)` и числовой аргумент

`Array(3)` — три дырки, `Array.of(3)` — `[3]`; `new Array(-1)` — `RangeError: Invalid array length`.

### 8. Сериализация `Map` в JSON

`JSON.stringify(map)` даёт `{}`: содержимое `Map` и `Set` теряется; нужно разворачивать в массив пар или значений.

### 9. Типизированные массивы заворачивают значения

`new Uint8Array([256, -1])` → `[0, 255]`; `Uint8ClampedArray` зажимает; `map` сохраняет тип и снова заворачивает (`500` → `244`).

### 10. `concat` и вложенность

`concat` разворачивает только массивы-аргументы на один уровень: `[[9]]` остаётся массивом внутри.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| `Array` + методы | универсально, много методов | дырки, часть методов мутирует |
| Копирующие методы (`toSorted`...) | безопасно для чужих массивов | создают копию (память) |
| `Set` вместо `includes` по массиву | уникальность и быстрый `has` | другой тип; не индексируется |
| `Map` вместо объекта | любые ключи, порядок, `size` | нет JSON «из коробки» |
| `WeakMap` | нет утечек, приватные метаданные | нельзя перебрать |
| Типизированные массивы | компактно, быстро для чисел и байтов | фиксированный тип, заворот |
| Персистентные структуры (библиотеки) | неизменяемость | внешняя зависимость; не запускались |

## Вопросы с собеседований

<details><summary>1. Чем <code>Array(3)</code> отличается от <code>Array.of(3)</code>?</summary>

`Array(3)` — массив из трёх пустых слотов (длина 3), `Array.of(3)` — массив из одного элемента `3`.

→ [Базовая теория](basics.ru.md#создание)

</details>

<details><summary>2. Что такое дырки и как их обрабатывают методы?</summary>

Отсутствующие индексы; `map`/`forEach` их пропускают, `join` и итерация видят как `undefined`.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>3. Ловушка: что вернёт <code>[10, 9, 1].sort()</code>?</summary>

`[1, 10, 9]`: по умолчанию сортировка строковая.

→ [Подвох 1](#1-sort-без-функции-сравнивает-строки)

</details>

<details><summary>4. Какие методы массива меняют его на месте?</summary>

`push`, `pop`, `shift`, `unshift`, `splice`, `sort`, `reverse`, `fill`, `copyWithin`. Копирующие аналоги: `toSorted`, `toReversed`, `toSpliced`, `with`.

→ [Справочник](#методы-массива)

</details>

<details><summary>5. Ловушка: почему <code>[NaN].indexOf(NaN)</code> — <code>-1</code>, а <code>includes</code> — <code>true</code>?</summary>

`indexOf` использует строгое равенство (`NaN !== NaN`), `includes` — SameValueZero.

→ [Справочник](#сравнение-значений-три-алгоритма)

</details>

<details><summary>6. Чем <code>Set</code> и <code>Map</code> отличаются от массива и объекта?</summary>

`Set` — уникальные значения, `Map` — любые ключи с порядком вставки и `size`; у обоих сравнение SameValueZero.

→ [Продвинутая теория](advanced.ru.md#set)

</details>

<details><summary>7. Зачем нужен <code>WeakMap</code> и почему он не итерируется?</summary>

Хранит данные, привязанные к объектам-ключам, не мешая сборке мусора; содержимое зависит от GC, поэтому наблюдать его перебором нельзя.

→ [Продвинутая теория](advanced.ru.md#weakmap-и-weakset)

</details>

<details><summary>8. Что произойдёт при <code>new Uint8Array([256, -1])</code>?</summary>

Значения заворачиваются по модулю 256: `[0, 255]`.

→ [Продвинутая теория](advanced.ru.md#типизированные-массивы)

</details>

<details><summary>9. Как убрать дубликаты из массива?</summary>

`[...new Set(arr)]`; для объектов — по ключу через `Map`.

→ [Продвинутая теория](advanced.ru.md#set)

</details>

<details><summary>10. Ловушка: как правильно отсортировать числа?</summary>

`arr.sort((a, b) => a - b)` или `toSorted` с той же функцией.

→ [Базовая теория](basics.ru.md#сортировка)

</details>

<details><summary>11. Что вернёт <code>reduce</code> на пустом массиве?</summary>

Без начального значения — `TypeError`; с начальным — его самого.

→ [Подвох 5](#5-reduce-без-начального-значения)

</details>

<details><summary>12. Как сериализовать <code>Map</code> в JSON?</summary>

`JSON.stringify([...map])` или `Object.fromEntries(map)` (для строковых ключей).

→ [Подвох 8](#8-сериализация-map-в-json)

</details>

## Шпаргалка

```text
[...a]  a.slice()  structuredClone(a)       a.toSorted()  a.toReversed()  a.with(i, v)  a.toSpliced(i, n)
a.sort((x, y) => x - y)                      a.reduce((acc, x) => …, init)
a.find / findLast / includes / at(-1)        Object.groupBy(a, f)   Map.groupBy(a, f)
new Set(a)  [...set]  s.union / intersection / difference
new Map(entries)  map.get / set / has / delete / size
new WeakMap()  new Uint8Array(buf)  new DataView(buf)
```

- Литерал `[…]`; избегайте дырок.
- Числа сортируйте с функцией сравнения.
- Копии поверхностные.
- Дубликаты — `Set`; словарь — `Map`; метаданные — `WeakMap`.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Типы значений: [02 · Типы и значения](../02-types-and-values/overview.ru.md)
- Объекты и копирование: [05 · Объекты и прототипы](../05-objects-and-prototypes/overview.ru.md)
- Итераторы, производительность — модули 11 и 19 дорожной карты [трека](../../overview.ru.md)

## Источники

- [ECMAScript: Array Objects](https://tc39.es/ecma262/#sec-array-objects)
- [ECMAScript: SameValueZero](https://tc39.es/ecma262/#sec-samevaluezero)
- [ECMAScript: Keyed Collections](https://tc39.es/ecma262/#sec-keyed-collection)
- [ECMAScript: TypedArray Objects](https://tc39.es/ecma262/#sec-typedarray-objects)
- [MDN: Array.prototype.toSorted()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/toSorted)
