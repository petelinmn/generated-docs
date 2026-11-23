---
title: "Типы и значения: экспертная теория"
lang: ru
track: javascript
kind: module
doc: expert
slug: 02-types-and-values
part: 1
tags: [type-coercion, abstract-equality, toprimitive, ieee-754, utf-16, bigint, nan, symbol]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Типы и значения: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все результаты получены запуском на Node.js 24.15 (ES-модуль, строгий режим).

## Справочник

### Типы языка и `typeof`

| Значение | `typeof` | Примечание |
|---|---|---|
| `42`, `NaN`, `Infinity` | `'number'` | один тип для целых и дробных |
| `'a'` | `'string'` | неизменяемая последовательность 16-битных кодовых единиц |
| `true` | `'boolean'` | |
| `undefined` | `'undefined'` | также для необъявленного имени, если применить `typeof` |
| `null` | `'object'` | историческая особенность |
| `10n` | `'bigint'` | |
| `Symbol('s')` | `'symbol'` | |
| `{}`, `[]`, `new Date()` | `'object'` | |
| функция, класс | `'function'` | функции — объекты, но `typeof` выделяет их |

### Приведение к числу (`Number(x)`, унарный `+`)

| Вход | Результат |
|---|---|
| `'  12  '`, `'1e3'`, `'0x1f'`, `'0b11'` | `12`, `1000`, `31`, `3` |
| `'1_000'`, `'12px'`, `'abc'` | `NaN` |
| `''`, `'  '`, `'\n'`, `null`, `[]`, `false` | `0` |
| `undefined`, `{}` | `NaN` |
| `[5]`, `true` | `5`, `1` |
| `'Infinity'` | `Infinity` |

### Приведение к строке

| Вход | `String(x)` |
|---|---|
| `null`, `undefined` | `'null'`, `'undefined'` |
| `[1, [2, 3]]` | `'1,2,3'` |
| `{}` | `'[object Object]'` |
| `-0`, `1e21`, `0.000001`, `1e-7` | `'0'`, `'1e+21'`, `'0.000001'`, `'1e-7'` |
| `Symbol('a')` | `'Symbol(a)'` — но `Symbol('a') + ''` и шаблон `${Symbol('a')}` дают `TypeError` |

### Ложные значения

`false`, `0`, `-0`, `0n`, `''`, `null`, `undefined`, `NaN` — ровно восемь.

### Проверки чисел

| Функция | Что проверяет |
|---|---|
| `Number.isNaN(x)` | значение именно `NaN` (без приведения) |
| `Number.isInteger(x)` | целое (`5.0` — да) |
| `Number.isSafeInteger(x)` | целое в безопасном диапазоне (`2 ** 53` — нет) |
| `Number.isFinite(x)` | конечное число без приведения (`'5'` — нет, глобальная `isFinite('5')` — да) |
| `Number.EPSILON` | допуск для сравнения дробей |
| `Number.MIN_VALUE`, `Number.MAX_VALUE` | `5e-324` и `1.7976931348623157e+308`; `2 ** 1024` равно `Infinity` |

## Внутреннее устройство

### Числа: IEEE 754

`number` — 64-битное двоичное число с плавающей точкой. Отсюда:

- `(0.1).toFixed(20)` равно `'0.10000000000000000555'`: десятичная дробь `0.1` в двоичной системе периодическая, хранится приближение;
- `2 ** 53 === 2 ** 53 + 1` истинно: выше 2⁵³ − 1 не все целые представимы;
- `Math.round(-0.5)` даёт `-0`, `Math.round(2.5)` — `3`, `Math.round(-2.5)` — `-2` (округление «к +∞» на половинах);
- `(1.005).toFixed(2)` равно `'1.00'`: число хранится как `1.00499999…`; `(2.5).toFixed(0)` — `'3'`, `(1.45).toFixed(1)` — `'1.4'`.

Поэтому для денег используют целые минимальные единицы, а `toFixed` и `Math.round` не заменяют правил округления.

### Строки: UTF-16

Индексы и `length` считают 16-битные кодовые единицы:

```text
'😀'.length = 2 ; '😀'.codePointAt(0) = 128512 ; '😀'.charCodeAt(0) = 55357
[...'👨‍👩‍👧'].length = 5 ; 'é'.normalize('NFD').length = 2 ; 'é'.length = 1
```

Эмодзи — «суррогатная пара» из двух единиц; семейный эмодзи складывается из трёх эмодзи и двух невидимых соединителей (`[...]` даёт 5). Один символ на экране не равен одному элементу ни по `length`, ни по `[...s]`: нужен `Intl.Segmenter` (в опытах не запускался).

### Алгоритм нестрогого сравнения

`x == y` по спецификации (в упрощённом виде):

1. Если типы совпадают — как `===`.
2. `null == undefined` истинно; с остальными значениями `null` и `undefined` не равны (`null == 0` ложно, `null == false` ложно).
3. `number` и `string` — строка приводится к числу.
4. `boolean` приводится к числу, затем сравнение повторяется (`'0' == false` → `'0' == 0` → `0 == 0`).
5. Объект и примитив — объект приводится к примитиву (`ToPrimitive`), затем сравнение повторяется (`[] == ''`, `[[]] == 0`, `[1,2] == '1,2'` истинны).
6. `bigint` сравнивается с `number` по математическому значению (`2n == 2` истинно).

### ToPrimitive: `valueOf` и `toString`

При приведении объекта к примитиву вызываются методы `valueOf` и `toString` в порядке, зависящем от «подсказки»:

```js
const o = { valueOf() { return 42 }, toString() { return 'str' } };
console.log(o + 1, `${o}`, o * 2, o == 42, String(o), [o] + '');
```

```text
43 str 84 true str str
```

- Подсказка `number` (арифметика, `==`): сначала `valueOf`, потом `toString`.
- Подсказка `string` (`${}`, `String()`): сначала `toString`.
- Для `+` и `==` подсказка по умолчанию, как у `number` — но у `Date` она `string`: `typeof (new Date(0) + 1)` равен `'string'`, а `typeof (new Date(0) - 1)` — `'number'`.
- `Symbol.toPrimitive` может переопределить правила (не запускалось).

## Подвохи

### 1. `typeof null === 'object'`

`null` — примитив. Проверка «объект»: `x !== null && typeof x === 'object'`.

### 2. `NaN`

`NaN !== NaN`; `Number.isNaN` не приводит тип, глобальная `isNaN('abc')` возвращает `true`. `[NaN].includes(NaN)` истинно, а `[NaN].indexOf(NaN)` даёт `-1` (разные алгоритмы сравнения).

### 3. `sort` по умолчанию сортирует строки

`[10, 9, 1].sort()` даёт `[1, 10, 9]`; числовой порядок задаёт `(a, b) => a - b`.

### 4. `parseInt` читает префикс

`parseInt('0x1f')` равно `31`, `parseInt('08')` — `8`, `parseInt(null)` — `NaN` (`'null'` не число), `parseInt('Infinity')` — `NaN`, тогда как `Number('Infinity')` — `Infinity`.

### 5. Цепочки сравнений

`1 < 2 < 3` истинно, а `3 > 2 > 1` ложно: `3 > 2` — `true`, затем `true > 1` — `1 > 1`.

### 6. `[] + {}` и `{} + []`

`[] + {}` даёт `'[object Object]'`. Запись `{} + []` зависит от контекста: в начале оператора (например, в `eval('{} + []')`) `{}` — пустой блок кода, остаётся `+[]`, и результат `0`; в скобках `({}) + []` — объект, и результат `'[object Object]'` (в опыте).

### 7. Обёртки и `instanceof`

`new String('x') === 'x'` ложно, `new String('x') == 'x'` истинно; `Boolean(new Boolean(false))` — `true`.

### 8. Дата и `+`

`new Date(0) + 1` склеивает строки, `new Date(0) - 1` вычитает числа; `new Date(NaN)` печатается как `Invalid Date`, `getTime()` даёт `NaN`.

### 9. Пустые слоты массива

В `[, 1]` длина 2, но индекса `0` в массиве нет (`0 in [, 1]` ложно); `[null, undefined, 1].join('-')` даёт `'--1'`.

### 10. BigInt и смешанная арифметика

`BigInt(1.5)` — `RangeError`; `Math.max(1n, 2n)` — `TypeError: Cannot convert a BigInt value to a number`; сравнения `5n > 4` и `2n == 2` работают, а `2n === 2` — нет. `structuredClone` умеет копировать `BigInt` и `Date` (в опыте результат `{ a: 1n, d: 1970-01-01T00:00:00.000Z }`).

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Неявное приведение | краткий код | неожиданные результаты |
| Явные `Number()`, `String()`, `Boolean()` | предсказуемо | многословно |
| `===` везде | нет скрытых приведений | нужно писать `== null` осознанно |
| `BigInt` | точные большие целые | не смешивается с `number`, нет в `JSON` и `Math` |
| Целые минимальные единицы для денег | точность | ручные преобразования при выводе |
| Статическая типизация ([TypeScript](../../../typescript/overview.ru.md)) | ошибки видны до запуска | типы стираются, на границах нужна проверка |

## Вопросы с собеседований

<details><summary>1. Какие типы данных есть в JavaScript?</summary>

Семь примитивов (`number`, `string`, `boolean`, `undefined`, `null`, `bigint`, `symbol`) и объекты.

→ [Базовая теория](basics.ru.md#типы-и-typeof)

</details>

<details><summary>2. Почему <code>typeof null</code> равен <code>'object'</code>?</summary>

Историческая особенность языка, сохранённая ради совместимости; `null` — примитив.

→ [Подвох 1](#1-typeof-null--object)

</details>

<details><summary>3. Почему <code>0.1 + 0.2 !== 0.3</code>?</summary>

Числа хранятся как двоичные дроби IEEE 754, `0.1` и `0.2` — приближения. Сравнивать нужно с допуском или считать в целых.

→ [Базовая теория](basics.ru.md#числа)

</details>

<details><summary>4. Как правильно проверить на <code>NaN</code>?</summary>

`Number.isNaN(x)` или `Object.is(x, NaN)`; `x === NaN` всегда ложно.

→ [Базовая теория](basics.ru.md#nan)

</details>

<details><summary>5. В чём разница между <code>==</code> и <code>===</code>?</summary>

`===` сравнивает без приведения типов; `==` приводит типы по набору правил и даёт неожиданные результаты (`[] == false`, `0 == ''`).

→ [Алгоритм нестрогого сравнения](#алгоритм-нестрогого-сравнения)

</details>

<details><summary>6. Ловушка: что вернёт <code>[] == ![]</code> и почему?</summary>

`true`: `![]` — `false`, затем сравнение `[] == false` приводит оба операнда к `0`.

→ [Продвинутая теория](advanced.ru.md#приведение-типов-когда--удивляет)

</details>

<details><summary>7. Перечислите ложные значения.</summary>

`false`, `0`, `-0`, `0n`, `''`, `null`, `undefined`, `NaN`.

→ [Справочник](#ложные-значения)

</details>

<details><summary>8. Чем <code>??</code> отличается от <code>||</code>?</summary>

`??` подставляет значение только для `null` и `undefined`, `||` — для любого ложного (`0`, `''`).

→ [Базовая теория](basics.ru.md#--и-)

</details>

<details><summary>9. Ловушка: что выведет <code>[10, 9, 1].sort()</code>?</summary>

`[1, 10, 9]`: по умолчанию сортировка лексикографическая, по строковому представлению.

→ [Подвох 3](#3-sort-по-умолчанию-сортирует-строки)

</details>

<details><summary>10. Как передаются аргументы в функцию: по значению или по ссылке?</summary>

Всегда по значению, но для объектов значением является ссылка: поле изменить можно, переприсваивание параметра снаружи не видно.

→ [Базовая теория](basics.ru.md#значения-и-ссылки)

</details>

<details><summary>11. Зачем нужен <code>BigInt</code> и в чём его ограничения?</summary>

Точные целые произвольной длины. Нельзя смешивать с `number`, нет `JSON.stringify` и `Math.max`.

→ [Продвинутая теория](advanced.ru.md#bigint)

</details>

<details><summary>12. Ловушка: почему у <code>'😀'.length</code> значение 2?</summary>

Строки — это 16-битные кодовые единицы UTF-16; эмодзи — суррогатная пара.

→ [Внутреннее устройство](#строки-utf-16)

</details>

<details><summary>13. В каком порядке вызываются <code>valueOf</code> и <code>toString</code>?</summary>

При арифметике сначала `valueOf`, при строковом контексте (`${}`, `String()`) сначала `toString`.

→ [ToPrimitive](#toprimitive-valueof-и-tostring)

</details>

## Шпаргалка

```text
typeof x ; Array.isArray(x) ; x === null
Number.isNaN(x) ; Number.isInteger(x) ; Number.isSafeInteger(x) ; Object.is(a, b)
a === b ; a ?? b ; a?.b ; x == null
Number(x) ; String(x) ; Boolean(x) ; BigInt(x)
```

- Falsy: `false 0 -0 0n '' null undefined NaN`.
- `+` склеивает при наличии строки; `-`, `*` приводят к числу.
- Объекты сравниваются по ссылке.
- Строка — UTF-16; `length` ≠ число символов.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Следующий модуль: [03 · Переменные и области видимости](../03-scopes-and-hoisting/overview.ru.md)
- Статическая типизация: [TypeScript](../../../typescript/overview.ru.md)

## Источники

- [ECMAScript: Type conversion](https://tc39.es/ecma262/#sec-type-conversion)
- [ECMAScript: IsLooselyEqual](https://tc39.es/ecma262/#sec-islooselyequal)
- [MDN: Number](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number)
- [MDN: String (UTF-16)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String#utf-16_characters_unicode_code_points_and_grapheme_clusters)
- [MDN: Symbol.toPrimitive](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Symbol/toPrimitive)
