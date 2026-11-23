---
title: "Strings, numbers, dates, regular expressions: basics"
lang: en
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

# Strings, numbers, dates, regular expressions: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Types and values](../02-types-and-values/overview.en.md), [07 · Arrays and collections](../07-arrays-and-collections/overview.en.md) · **Time:** ~3 h

All examples were run on Node.js 24.15 in an ES module file. The machine's time zone in the experiments is UTC+4 (`Asia/Yerevan`): results that depend on local time are noted with a caveat.

## Why it matters

Text, numbers, dates and patterns appear in every program: input handling, output formatting, logs, APIs.

## Strings

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

- Strings are immutable: `trim`, `replace` and others return a **new** string.
- `replace` with a plain string replaces only the **first** occurrence, `replaceAll` replaces all.
- `slice(start, end)` accepts negative indexes; `substring(3, 1)` swaps the bounds and returns `'el'`.
- `padStart`/`padEnd` pad to a length, `repeat` repeats.

### Comparing strings

```js
console.log('abc' < 'abd', 'a' < 'B', 'a'.localeCompare('B'));
console.log(['ä', 'a', 'z'].sort(), ['ä', 'a', 'z'].sort((x, y) => x.localeCompare(y, 'de')), 'ß'.toUpperCase());
```

```text
true false -1
[ 'a', 'z', 'ä' ] [ 'a', 'ä', 'z' ] SS
```

- The operators `<`, `>` compare character **codes**: all capitals come before lowercase letters (`'a' < 'B'` is `false`), and `'ä'` ends up after `'z'`.
- `localeCompare` compares by language rules and returns a negative number, `0` or a positive number; use it (or `Intl.Collator`) to sort alphabetically.
- Case conversion depends on the language: `'ß'.toUpperCase()` is `'SS'`.

### Unicode: a first look

```js
const e = '😀', acc = 'é';
console.log(e.length, [...e].length, e.codePointAt(0).toString(16), String.fromCodePoint(0x1F600));
console.log(acc.length, acc.normalize().length, acc === 'é', acc.normalize() === 'é');
```

```text
2 1 1f600 😀
2 1 false true
```

- An emoji takes **two** code units (`length` is `2`) but one character (code point): `[...e].length` is `1`.
- The same visible character `é` can be written as one character or as `e` + a "combining accent"; `normalize()` brings them to one form, otherwise `===` will not work.

### Template literals

```js
const a = 1, b = 2;
console.log(`sum: ${a + b}`, `line1
line2`.split('\n').length);
```

```text
sum: 3 2
```

In backticks you can substitute expressions `${...}` and write multi-line text.

## Numbers

### Formatting

```js
console.log((1234.5678).toFixed(2), (0.000001234).toExponential(2), (1234.5678).toPrecision(6), (255).toString(16), (255).toString(2), Number('0b101'));
console.log(Math.round(1.005 * 100) / 100, +(1.005).toFixed(2), 0.1 + 0.7, (0.1 + 0.7) * 10, Math.floor((0.1 + 0.7) * 10));
```

```text
1234.57 1.23e-6 1234.57 ff 11111111 5
1 1 0.7999999999999999 7.999999999999999 7
```

- `toFixed(n)` rounds to `n` digits and **returns a string**; `toPrecision(n)` — to `n` significant digits; `toExponential(n)` — exponential notation.
- `toString(radix)` converts to another number system (`'ff'`, `'11111111'`).
- Because of the binary representation `1.005` is stored as `1.00499…`, so rounding gives `1.00`, and `(0.1 + 0.7) * 10` is `7.999…`, so `Math.floor` gives `7`. For money count in integer minimal units ([module 02](../02-types-and-values/expert.en.md#numbers-ieee-754)).

### Math

```js
console.log(Math.round(2.5), Math.round(-2.5), Math.trunc(-2.7), Math.floor(-2.7), Math.ceil(-2.2), Math.sign(-3), Math.hypot(3, 4), Math.cbrt(27));
console.log(Math.max(), Math.min(), Math.max(...[1, NaN]), 2 ** 10, 2 ** -1, (-8) ** (1 / 3), 5 % 3, -5 % 3, 5.5 % 2);
```

```text
3 -2 -2 -3 -2 -1 5 3
-Infinity Infinity NaN 1024 0.5 NaN 2 -2 1.5
```

- `Math.round` rounds a half **up** (toward `+∞`): `2.5` → `3`, `-2.5` → `-2`.
- `trunc` drops the fractional part, `floor` rounds down, `ceil` rounds up.
- `Math.max()` with no arguments is `-Infinity`, `Math.min()` is `Infinity`; any `NaN` gives `NaN`.
- `%` keeps the sign of the dividend: `-5 % 3` is `-2`.

## Intl: formatting by language

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

The rules for separators and currencies depend on the language (`de-DE` — a dot for thousands and a comma for the fractional part). More in the [advanced theory](advanced.en.md#intl-in-detail).

## Date: the basics

`Date` is a moment in time in milliseconds from `1970-01-01T00:00:00Z`:

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

- Months are numbered **from zero** (`getUTCMonth()` is `0` for January); `getUTCDay()` is the weekday (`0` is Sunday).
- `toISOString()` is the standard string in UTC; `getTime()` is milliseconds; `Date.now()` is the current moment.
- Going out of range "overflows": `setUTCMonth(1)` for 31 January gives `2 March` (February 2024 has no 31st); `new Date(2024, 1, 0)` is the "zero day" of February, that is, 31 January.

```js
console.log(new Date('nope').getTime(), String(new Date(NaN)));
console.log(new Date('2024-03-10T12:00:00Z') - new Date('2024-03-09T12:00:00Z'));
```

```text
NaN Invalid Date
86400000
```

- An invalid date is `Invalid Date` with `getTime()` equal to `NaN`.
- The difference of dates is a number of milliseconds (`86400000` is one day).

## Regular expressions: the basics

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

- `/…/flags` is a literal; `\d` is a digit, `+` is "one or more", `(…)` is a capture group.
- `match` without `g` returns the first match with groups (`m[1]`) and `index`; with `g` an array of all matches without groups; no match gives `null`.
- `replace(re, string)` understands `$1`, `$2` (group numbers); a function can be passed.
- `test` returns `true`/`false`, `split` accepts a regular expression, `search` returns an index or `-1`.
- Flags: `g` — all matches, `i` — case-insensitive.

## Beginner mistakes

- ❌ Comparing strings with `<` to sort alphabetically → ✅ `localeCompare`.
- ❌ `'abc'.length` as the number of characters for an emoji → ✅ `[...s].length` (and remember graphemes, see the [advanced theory](advanced.en.md#unicode-in-detail)).
- ❌ `toFixed` for money → ✅ integer minimal units and `Intl.NumberFormat`.
- ❌ Month `1` for January in `Date` → ✅ months from zero.
- ❌ `new Date('2024-01-15')` and `new Date(2024, 0, 15)` as the same thing → ✅ the first is UTC, the second is local time.
- ❌ `str.replace('-', '+')` to replace all → ✅ `replaceAll` or `/-/g`.
- ❌ Expecting `match` with `g` and groups to return an array of them → ✅ for the groups of all matches use `matchAll`.

## Related materials

- Next: [advanced theory](advanced.en.md) — Unicode, `Intl`, `Date`, regular expressions in detail
- Earlier: [02 · Types and values](../02-types-and-values/overview.en.md), [07 · Arrays and collections](../07-arrays-and-collections/overview.en.md)

## Sources

- [MDN: String](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String)
- [MDN: Number](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number)
- [MDN: Math](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Math)
- [MDN: Date](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date)
- [MDN: Regular expressions](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Regular_expressions)
