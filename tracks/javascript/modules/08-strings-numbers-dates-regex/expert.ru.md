---
title: "Строки, числа, даты, регулярные выражения: экспертная теория"
lang: ru
track: javascript
kind: module
doc: expert
slug: 08-strings-numbers-dates-regex
part: 1
tags: [string-api, math-api, regexp-flags, date-formats, utf-16, backtracking, intl, number-formatting]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Строки, числа, даты, регулярные выражения: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все результаты получены запуском на Node.js 24.15 (ES-модуль, часовой пояс машины UTC+4).

## Справочник

### Методы строки

| Группа | Методы (в опыте — выделены) |
|---|---|
| Доступ | `at`, `charAt`, `charCodeAt`, **`codePointAt`**, `[]`, `length` |
| Поиск | `indexOf`, `lastIndexOf`, **`includes`**, **`startsWith`**, **`endsWith`**, `search`, `match`, `matchAll` |
| Срезы | `slice`, `substring`, `split` |
| Изменение (возвращают новую) | `trim`, `trimStart`, `trimEnd`, `padStart`, `padEnd`, `repeat`, `replace`, `replaceAll`, `toUpperCase`, `toLowerCase`, `normalize`, `concat` |
| Сравнение | `localeCompare`, `<` / `>` |
| Прочее | `String.raw`, `String.fromCharCode`, `String.fromCodePoint`, `isWellFormed`, `toWellFormed` |

> ⚠️ Проверить: `charAt`, `concat`, `isWellFormed`, `toWellFormed`, `trimEnd` и `substr` (устаревший) взяты из документации и в опытах не запускались.

### Number и Math

| Метод | Результат (в опыте) |
|---|---|
| `toFixed(2)` | строка: `(1234.5678).toFixed(2)` → `'1234.57'`, `(1.005).toFixed(2)` → `'1.00'` |
| `toPrecision(6)` | `'1234.57'` |
| `toExponential(2)` | `'1.23e-6'` для `0.000001234` |
| `toString(основание)` | `(255).toString(16)` → `'ff'`, `(255).toString(2)` → `'11111111'` |
| `Math.round` | `2.5` → `3`, `-2.5` → `-2` |
| `Math.trunc/floor/ceil` | `-2.7` → `-2/-3`, `-2.2` → `-2` |
| `Math.max()/min()` | `-Infinity` / `Infinity`; с `NaN` → `NaN` |
| `Math.hypot/cbrt/sign` | `5`, `3`, `-1` |
| `**` | `2 ** 10` → `1024`, `(-8) ** (1/3)` → `NaN` |
| `%` | `-5 % 3` → `-2`, `5.5 % 2` → `1.5` |

### Формы конструктора Date

| Запись | Смысл |
|---|---|
| `new Date()` | текущий момент |
| `new Date(ms)` | миллисекунды от эпохи (UTC) |
| `new Date('2024-01-15')` | только дата → **UTC** (в опыте `2024-01-15T00:00:00.000Z`) |
| `new Date('2024-01-15T00:00:00')` | дата и время без пояса → **местное время** |
| `new Date('2024-01-15T00:00:00Z')` | UTC |
| `new Date(y, m, d, h, mi, s, ms)` | **местное** время; `m` с нуля; переполнение «переливается» |
| `Date.UTC(y, m, d, ...)` | миллисекунды для UTC-компонентов |
| `new Date('nope')` | `Invalid Date`, `getTime()` — `NaN` |

Диапазон: ±8,64·10¹⁵ мс от эпохи; максимум в опыте — `+275760-09-13T00:00:00.000Z`, на единицу больше — `NaN`.

> ⚠️ Проверить: разбор нестандартных форматов (`'Jan 15 2024'`, `'2024/01/15'`) зависит от реализации и в опытах не запускался.

### Флаги RegExp

| Флаг | Свойство | Действие |
|---|---|---|
| `g` | `global` | все совпадения; `lastIndex` хранит позицию |
| `i` | `ignoreCase` | без учёта регистра |
| `m` | `multiline` | `^`/`$` на каждой строке |
| `s` | `dotAll` | `.` совпадает с `\n` |
| `u` | `unicode` | кодовые точки, `\u{…}`, `\p{…}` |
| `y` | `sticky` | совпадение строго с `lastIndex` |
| `d` | `hasIndices` | позиции в `indices` |

> ⚠️ Проверить: флаг `v` (расширенный режим Юникода) и `RegExp.escape` в Node.js 24.15 в опытах не запускались.

### Intl: сводка

| Объект | Что делает (в опыте) |
|---|---|
| `Intl.NumberFormat` | валюты, разделители, `compact`: `1.2M` |
| `Intl.DateTimeFormat` | форматы дат, `timeZone`, `dateStyle` |
| `Intl.PluralRules` | `ru`: `2` → `few`; `en`: `1` → `one` |
| `Intl.ListFormat` | `a, b, and c` |
| `Intl.RelativeTimeFormat` | `yesterday` для `-1, 'day'` |
| `Intl.Collator` | сортировка по правилам языка |
| `Intl.Segmenter` | графемы, слова, предложения |

## Внутреннее устройство

- **Строка — последовательность 16-битных кодовых единиц.** Индексы, `length`, `charCodeAt` оперируют единицами, а итерация и `codePointAt` — кодовыми точками. «Видимый символ» (графема) может состоять из нескольких точек.
- **Числа — `double` (IEEE 754).** `toFixed` и `toPrecision` форматируют точное двоичное значение, поэтому `1.005` печатается как `1.00`. Операция `%` — остаток со знаком делимого.
- **Date — одно число.** Объект хранит миллисекунды от эпохи в UTC; «местные» геттеры (`getDate`, `getHours`) каждый раз применяют часовой пояс среды выполнения, а `getUTC*` — нет. Поэтому одинаковый код даёт разный вывод в разных поясах.
- **Разбор строки даты.** Формат ISO (`YYYY-MM-DD`, `…T…Z`) стандартизован: только дата — UTC, дата со временем без пояса — местное время. Остальные форматы зависят от реализации.
- **Регулярные выражения — откатывающийся (backtracking) автомат.** При неудаче движок пробует альтернативы; вложенные повторы дают экспоненциальное число вариантов (ReDoS: в опыте `/^(a+)+$/` на 24 символах медленнее 50 мс). Lookahead/lookbehind проверяют контекст без потребления символов, ленивые квантификаторы берут минимум.
- **Состояние `RegExp`.** Для `g` и `y` у объекта есть `lastIndex`; `test` и `exec` читают его и обновляют (в опыте последовательность `1`, `2`, `0`). `match` с `g` и `replace` сбрасывают его сами.
- **`Intl`** опирается на данные локалей (ICU) в сборке Node.js; поддержка языков зависит от сборки.

> ⚠️ Проверить: устройство оптимизаций регулярных выражений V8 (компиляция в нативный код, `irregexp`, линейный движок), а также размер строки в памяти (однобайтовое и двухбайтовое представление) не проверялись.

## Подвохи

### 1. `length` не равен числу символов

`'😀'.length` — `2`, семейный эмодзи — `8` единиц, `5` точек, `1` графема (в опыте).

### 2. Два вида `é`

`'é' === 'é'` — `false`; помогает `normalize()`.

### 3. `toFixed` и округление

`(1.005).toFixed(2)` — `'1.00'`, `Math.round(1.005 * 100) / 100` — `1`.

### 4. `Math.round` на половинах

`Math.round(-2.5)` — `-2`, `Math.round(2.5)` — `3`.

### 5. Месяцы с нуля и переполнение

`new Date(2024, 0, 31, 25).getDate()` — `1` (день следующего месяца); `setUTCMonth(1)` у 31 января даёт 2 марта.

### 6. Дата строкой и дата числами

`new Date('2024-01-15')` — UTC, а `new Date(2024, 0, 15)` — местное время. В поясе UTC+4 их `getTime()` не равны (в опыте `false`).

### 7. Состояние `g`-регулярки

`true, true, false` на одном и том же выражении для одной и той же строки.

### 8. `replaceAll` с обычным `RegExp`

`TypeError: String.prototype.replaceAll called with a non-global RegExp argument`.

### 9. Откат в lookahead

На `'12px'` совпадает `'1'`. Нужен запрет продолжения числа: `\d+(?!\d|px)`.

### 10. ReDoS

`/^(a+)+$/` на `'aaaa…b'` из 24 символов превысил 50 мс (в опыте).

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Встроенный `Date` | везде доступен | неудобный API, ловушки поясов |
| Библиотеки дат (date-fns, Luxon) | удобный API, пояса | зависимость; не запускались |
| `Temporal` | современный API | отсутствует в Node.js 24.15 (в опыте `typeof Temporal` — `'undefined'`) |
| `Intl` | локализация без зависимостей | зависит от данных ICU |
| Регулярные выражения | краткие шаблоны | читаемость, ReDoS |
| Парсер или конечный автомат | надёжно для сложных форматов | больше кода |
| Целые минимальные единицы для денег | точность | ручное форматирование |

## Вопросы с собеседований

<details><summary>1. Почему <code>'😀'.length</code> равен 2?</summary>

Строка хранится как 16-битные кодовые единицы UTF-16; эмодзи — суррогатная пара.

→ [Продвинутая теория](advanced.ru.md#юникод-подробнее)

</details>

<details><summary>2. Как правильно сравнивать строки с учётом языка?</summary>

`localeCompare` или `Intl.Collator`; операторы `<`, `>` сравнивают коды.

→ [Базовая теория](basics.ru.md#сравнение-строк)

</details>

<details><summary>3. Ловушка: почему <code>(1.005).toFixed(2)</code> даёт <code>'1.00'</code>?</summary>

Число хранится как `1.00499…`; `toFixed` форматирует точное значение.

→ [Подвох 3](#3-tofixed-и-округление)

</details>

<details><summary>4. Что хранит объект <code>Date</code>?</summary>

Миллисекунды от `1970-01-01T00:00:00Z`; пояс применяется только при чтении «местных» геттеров и форматировании.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>5. Ловушка: в чём разница между <code>new Date('2024-01-15')</code> и <code>new Date(2024, 0, 15)</code>?</summary>

Первое — полночь UTC, второе — полночь по местному времени; значения совпадают только в поясе UTC.

→ [Подвох 6](#6-дата-строкой-и-дата-числами)

</details>

<details><summary>6. Почему месяц в <code>Date</code> нумеруется с нуля и что будет при переполнении?</summary>

Историческое решение; переполнение «переливается» в следующий период (`new Date(2024, 1, 0)` — 31 января).

→ [Базовая теория](basics.ru.md#date-основы)

</details>

<details><summary>7. Чем <code>match</code> с флагом <code>g</code> отличается от без него?</summary>

Без `g` — первое совпадение с группами и `index`; с `g` — массив всех совпадений без групп (`matchAll` даёт группы).

→ [Базовая теория](basics.ru.md#регулярные-выражения-основы)

</details>

<details><summary>8. Ловушка: что произойдёт при повторных <code>test</code> на <code>/a/g</code>?</summary>

Результат зависит от `lastIndex`: `true, true, false` на одной и той же строке.

→ [Подвох 7](#7-состояние-g-регулярки)

</details>

<details><summary>9. Что такое ReDoS и как его избежать?</summary>

Экспоненциальный откат при вложенных повторах; избегать `(a+)+`, ограничивать длину ввода, использовать проверенные шаблоны.

→ [Продвинутая теория](advanced.ru.md#ошибки-и-redos)

</details>

<details><summary>10. Что делают флаги <code>u</code>, <code>s</code>, <code>y</code>, <code>d</code>?</summary>

`u` — Юникод, `s` — точка включает `\n`, `y` — поиск ровно с `lastIndex`, `d` — позиции совпадений.

→ [Справочник](#флаги-regexp)

</details>

<details><summary>11. Как форматировать числа и даты для другого языка?</summary>

`Intl.NumberFormat`, `Intl.DateTimeFormat` (или `toLocale…String`) с указанием языка и `timeZone`.

→ [Продвинутая теория](advanced.ru.md#intl-подробнее)

</details>

<details><summary>12. Ловушка: что вернёт <code>/\d+(?!px)/.exec('12px')</code>?</summary>

`'1'`: движок откатился на одну цифру, после которой нет `px`.

→ [Подвох 9](#9-откат-в-lookahead)

</details>

## Шпаргалка

```text
s.slice / at / includes / padStart / replaceAll / localeCompare / normalize
[...s]  new Intl.Segmenter('en', { granularity: 'grapheme' })
n.toFixed(2)  n.toString(16)  Math.round / trunc / floor / ceil
new Intl.NumberFormat(lang, opts).format(n)   new Intl.DateTimeFormat(lang, { timeZone }).format(d)
new Date(Date.UTC(…))  d.toISOString()  d.getTime()  Date.now()
/(?<name>…)/g  s.match / matchAll / replace / test   флаги g i m s u y d
```

- Время храните в UTC; месяцы с нуля.
- `g`-регулярка — с состоянием; `replaceAll` требует `g`.
- Не используйте регулярные выражения для HTML.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Числа и сравнение: [02 · Типы и значения](../02-types-and-values/overview.ru.md)
- Массивы и коллекции: [07 · Массивы и коллекции](../07-arrays-and-collections/overview.ru.md)
- Безопасность и производительность — модули 19 и 20 дорожной карты [трека](../../overview.ru.md)

## Источники

- [ECMAScript: String Objects](https://tc39.es/ecma262/#sec-string-objects)
- [ECMAScript: Date Objects](https://tc39.es/ecma262/#sec-date-objects)
- [ECMAScript: RegExp Objects](https://tc39.es/ecma262/#sec-regexp-regular-expression-objects)
- [ECMA-402: Intl](https://tc39.es/ecma402/)
- [MDN: Date.parse()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date/parse)
