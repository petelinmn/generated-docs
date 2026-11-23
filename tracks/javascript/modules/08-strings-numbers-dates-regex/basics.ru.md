---
title: "Строки, числа, даты, регулярные выражения: базовая теория"
lang: ru
track: javascript
kind: module
doc: basics
slug: 08-strings-numbers-dates-regex
part: 1
tags: [strings, number-formatting, math, intl, date, regexp, unicode]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Строки, числа, даты, регулярные выражения: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Типы и значения](../02-types-and-values/overview.ru.md), [07 · Массивы и коллекции](../07-arrays-and-collections/overview.ru.md) · **Время:** ~3 ч

Все примеры выполнены на Node.js 24.15 в файле ES-модуля. Часовой пояс машины в опытах — UTC+4 (`Asia/Yerevan`): результаты, зависящие от местного времени, указаны с оговоркой.

## Зачем это нужно

Текст, числа, даты и шаблоны встречаются в любой программе: обработка ввода, форматирование вывода, логи, API.

## Строки

```js
const s = '  Hello, World  ';
console.log(s.trim(), s.trimStart().length, s.length, 'abc'.padStart(5, '*'), 'abc'.padEnd(5, '-') + '|', 'ab'.repeat(3));
console.log('a-b-c'.split('-'), 'a-b-c'.replaceAll('-', '+'), 'a-b-c'.replace('-', '+'));
console.log('Hello'.at(-1), 'Hello'.slice(1, 3), 'Hello'.slice(-3), 'Hello'.substring(3, 1), 'Hello'.indexOf('l'), 'Hello'.lastIndexOf('l'));
console.log('Hello'.includes('ell'), 'Hello'.startsWith('He'), 'Hello'.endsWith('lo'));
```

```text
Hello, World 14 16 **abc abc--| ababab
[ 'a', 'b', 'c' ] a+b+c a+b-c
o el llo el 2 3
true true true
```

- Строки неизменяемы: `trim`, `replace` и другие возвращают **новую** строку.
- `replace` с обычной строкой заменяет только **первое** вхождение, `replaceAll` — все.
- `slice(начало, конец)` принимает отрицательные индексы; `substring(3, 1)` меняет границы местами и вернёт `'el'`.
- `padStart`/`padEnd` дополняют до длины, `repeat` повторяет.

### Сравнение строк

```js
console.log('abc' < 'abd', 'a' < 'B', 'a'.localeCompare('B'));
console.log(['ä', 'a', 'z'].sort(), ['ä', 'a', 'z'].sort((x, y) => x.localeCompare(y, 'de')), 'ß'.toUpperCase());
```

```text
true false -1
[ 'a', 'z', 'ä' ] [ 'a', 'ä', 'z' ] SS
```

- Операторы `<`, `>` сравнивают **коды** символов: все заглавные идут раньше строчных (`'a' < 'B'` — `false`), а `'ä'` оказывается после `'z'`.
- `localeCompare` сравнивает по правилам языка и возвращает отрицательное число, `0` или положительное; для сортировки по алфавиту используйте его (или `Intl.Collator`).
- Преобразование регистра зависит от языка: `'ß'.toUpperCase()` — `'SS'`.

### Юникод: первое знакомство

```js
const e = '😀', acc = 'é';
console.log(e.length, [...e].length, e.codePointAt(0).toString(16), String.fromCodePoint(0x1F600));
console.log(acc.length, acc.normalize().length, acc === 'é', acc.normalize() === 'é');
```

```text
2 1 1f600 😀
2 1 false true
```

- Эмодзи занимает **две** кодовые единицы (`length` — `2`), но один символ (кодовую точку): `[...e].length` — `1`.
- Один и тот же видимый символ `é` можно записать как один символ или как `e` + «комбинирующий акцент»; `normalize()` приводит к одной форме, иначе сравнение `===` не сработает.

### Шаблонные строки

```js
const a = 1, b = 2;
console.log(`sum: ${a + b}`, `line1
line2`.split('\n').length);
```

```text
sum: 3 2
```

В обратных кавычках можно подставлять выражения `${...}` и писать многострочный текст.

## Числа

### Форматирование

```js
console.log((1234.5678).toFixed(2), (0.000001234).toExponential(2), (1234.5678).toPrecision(6), (255).toString(16), (255).toString(2), Number('0b101'));
console.log(Math.round(1.005 * 100) / 100, +(1.005).toFixed(2), 0.1 + 0.7, (0.1 + 0.7) * 10, Math.floor((0.1 + 0.7) * 10));
```

```text
1234.57 1.23e-6 1234.57 ff 11111111 5
1 1 0.7999999999999999 7.999999999999999 7
```

- `toFixed(n)` округляет до `n` знаков и **возвращает строку**; `toPrecision(n)` — до `n` значащих цифр; `toExponential(n)` — экспоненциальная запись.
- `toString(основание)` переводит в другую систему счисления (`'ff'`, `'11111111'`).
- Из-за двоичного представления `1.005` хранится как `1.00499…`, поэтому округление даёт `1.00`, а `(0.1 + 0.7) * 10` — `7.999…`, и `Math.floor` даёт `7`. Для денег считайте в целых минимальных единицах ([модуль 02](../02-types-and-values/expert.ru.md#числа-ieee-754)).

### Math

```js
console.log(Math.round(2.5), Math.round(-2.5), Math.trunc(-2.7), Math.floor(-2.7), Math.ceil(-2.2), Math.sign(-3), Math.hypot(3, 4), Math.cbrt(27));
console.log(Math.max(), Math.min(), Math.max(...[1, NaN]), 2 ** 10, 2 ** -1, (-8) ** (1 / 3), 5 % 3, -5 % 3, 5.5 % 2);
```

```text
3 -2 -2 -3 -2 -1 5 3
-Infinity Infinity NaN 1024 0.5 NaN 2 -2 1.5
```

- `Math.round` округляет половину **вверх** (к `+∞`): `2.5` → `3`, `-2.5` → `-2`.
- `trunc` отбрасывает дробную часть, `floor` — вниз, `ceil` — вверх.
- `Math.max()` без аргументов — `-Infinity`, `Math.min()` — `Infinity`; любой `NaN` даёт `NaN`.
- `%` сохраняет знак делимого: `-5 % 3` — `-2`.

## Intl: форматирование по языку

```js
console.log(new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(1234.5));
console.log(new Intl.NumberFormat('de-DE').format(1234567.891), (1234567.891).toLocaleString('en-US'));
console.log(new Intl.NumberFormat('en', { notation: 'compact' }).format(1234567));
```

```text
$1,234.50
1.234.567,891 1,234,567.891
1.2M
```

Правила разделителей и валют зависят от языка (`de-DE` — точка для тысяч и запятая для дробной части). Подробнее — в [продвинутой теории](advanced.ru.md#intl-подробнее).

## Date: основы

`Date` — момент времени в миллисекундах от `1970-01-01T00:00:00Z`:

```js
const d = new Date(Date.UTC(2024, 0, 31, 12, 30, 45, 123));
console.log(d.toISOString(), d.getTime(), d.getUTCDay(), d.getUTCMonth(), d.toJSON(), typeof Date.now());
const d2 = new Date(Date.UTC(2024, 0, 31)); d2.setUTCMonth(1);
console.log(d2.toISOString());
console.log(new Date(2024, 1, 0).getDate(), new Date(0).toISOString());
```

```text
2024-01-31T12:30:45.123Z 1706704245123 3 0 2024-01-31T12:30:45.123Z number
2024-03-02T00:00:00.000Z
31 1970-01-01T00:00:00.000Z
```

- Месяцы нумеруются **с нуля** (`getUTCMonth()` — `0` для января); `getUTCDay()` — день недели (`0` — воскресенье).
- `toISOString()` — стандартная строка в UTC; `getTime()` — миллисекунды; `Date.now()` — текущий момент.
- Выход за диапазон «переливается»: `setUTCMonth(1)` для 31 января даёт `2 марта` (в феврале 2024 нет 31-го числа); `new Date(2024, 1, 0)` — «нулевой день» февраля, то есть 31 января.

```js
console.log(new Date('nope').getTime(), String(new Date(NaN)));
console.log(new Date('2024-03-10T12:00:00Z') - new Date('2024-03-09T12:00:00Z'));
```

```text
NaN Invalid Date
86400000
```

- Некорректная дата — `Invalid Date` с `getTime()` равным `NaN`.
- Разность дат — число миллисекунд (`86400000` — одни сутки).

## Регулярные выражения: основы

```js
const re = /(\d{4})-(\d{2})-(\d{2})/;
const m = '2024-05-17 and 2025-01-02'.match(re);
console.log(m[0], m[1], m.index, m.length);
console.log('a1b22c333'.match(/\d+/g), 'a1b22'.replace(/\d+/g, n => `<${n}>`), '2024-05-17'.replace(/(\d+)-(\d+)-(\d+)/, '$3.$2.$1'));
console.log('x'.match(/y/), 'x'.match(/y/g), 'a,b;c'.split(/[,;]/), 'abc'.search(/c/));
console.log(/^\d+$/.test('123'), /^\d+$/.test('12a'), /colou?r/i.test('COLOR'));
```

```text
2024-05-17 2024 0 4
[ '1', '22', '333' ] a<1>b<22> 17.05.2024
null null [ 'a', 'b', 'c' ] 2
true false true
```

- `/…/флаги` — литерал; `\d` — цифра, `+` — «один или больше», `(…)` — группа захвата.
- `match` без `g` возвращает первое совпадение с группами (`m[1]`) и `index`; с `g` — массив всех совпадений без групп; нет совпадений — `null`.
- `replace(re, строка)` понимает `$1`, `$2` (номера групп); можно передать функцию.
- `test` возвращает `true`/`false`, `split` принимает регулярное выражение, `search` — индекс или `-1`.
- Флаги: `g` — все совпадения, `i` — без учёта регистра.

## Ошибки новичка

- ❌ Сравнивать строки через `<` для сортировки по алфавиту → ✅ `localeCompare`.
- ❌ `'abc'.length` как число символов для эмодзи → ✅ `[...s].length` (и помнить о графемах, см. [продвинутую теорию](advanced.ru.md#юникод-подробнее)).
- ❌ `toFixed` для денег → ✅ целые минимальные единицы и `Intl.NumberFormat`.
- ❌ Месяц `1` для января в `Date` → ✅ месяцы с нуля.
- ❌ `new Date('2024-01-15')` и `new Date(2024, 0, 15)` как одно и то же → ✅ первое — UTC, второе — местное время.
- ❌ `str.replace('-', '+')` для замены всех → ✅ `replaceAll` или `/-/g`.
- ❌ Ждать от `match` массив с `g` и группами → ✅ для групп всех совпадений `matchAll`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — Юникод, `Intl`, `Date`, регулярные выражения подробно
- Ранее: [02 · Типы и значения](../02-types-and-values/overview.ru.md), [07 · Массивы и коллекции](../07-arrays-and-collections/overview.ru.md)

## Источники

- [MDN: String](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String)
- [MDN: Number](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number)
- [MDN: Math](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Math)
- [MDN: Date](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date)
- [MDN: Regular expressions](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Regular_expressions)
