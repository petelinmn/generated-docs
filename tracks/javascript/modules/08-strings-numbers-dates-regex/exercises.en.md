---
title: "Exercises: strings, numbers, dates, regular expressions"
lang: en
track: javascript
kind: module
doc: exercises
slug: 08-strings-numbers-dates-regex
part: 1
tags: [strings, regexp, date, intl, unicode]
status: draft
updated: 2026-11-23
---

# Exercises: strings, numbers, dates, regular expressions

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. Run the code in an ES module file.

## 1. 🟢 A slug

**Task:** write `slugify(s)`: lowercase, everything except Latin letters and digits is replaced by a single hyphen, hyphens at the edges are removed.

**Expected result:** `slugify('  Hello,  World!  JS ')` → `hello-world-js`, `slugify('---A--B---')` → `a-b`.

<details><summary>Hint</summary>

`replace(/[^a-z0-9]+/g, '-')`, then `replace(/^-+|-+$/g, '')`.

</details>

<details><summary>Solution</summary>

```js
const slugify = (s) => s.trim().toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '');
console.log(slugify('  Hello,  World!  JS '), slugify('---A--B---'));
```

```text
hello-world-js a-b
```

</details>

## 2. 🟢 Parsing a log line

**Task:** parse the line `'2024-05-17 ERROR [db] Connection lost'` into `date`, `level`, `tag`, `msg` using named groups. For a line that does not fit the format the function must return `undefined`.

**Expected result:** `{ date: '2024-05-17', level: 'ERROR', tag: 'db', msg: 'Connection lost' }` and `undefined`.

<details><summary>Hint</summary>

`match(...)?.groups`, the pattern `(?<date>\d{4}-\d{2}-\d{2}) (?<level>[A-Z]+) \[(?<tag>\w+)\] (?<msg>.*)`.

</details>

<details><summary>Solution</summary>

```js
const parse = (line) => line.match(/^(?<date>\d{4}-\d{2}-\d{2}) (?<level>[A-Z]+) \[(?<tag>\w+)\] (?<msg>.*)$/)?.groups;
console.log({ ...parse('2024-05-17 ERROR [db] Connection lost') }, parse('garbage'));
```

```text
{
  date: '2024-05-17',
  level: 'ERROR',
  tag: 'db',
  msg: 'Connection lost'
} undefined
```

</details>

## 3. 🟡 Calendar calculations in UTC

**Task:** write `daysInMonth(year, month)` (month from zero) and `addDays(date, n)`. Check February 2024 and 2023, December 2024 and `28 February 2024 + 2 days`.

**Expected result:** `29 28 31 2024-03-01`.

<details><summary>Hint</summary>

The "zero day" of the next month is the last day of the current one: `Date.UTC(y, m + 1, 0)`; a day is `864e5` ms.

</details>

<details><summary>Solution</summary>

```js
const daysInMonth = (y, m) => new Date(Date.UTC(y, m + 1, 0)).getUTCDate();
const addDays = (d, n) => new Date(d.getTime() + n * 864e5);
console.log(daysInMonth(2024, 1), daysInMonth(2023, 1), daysInMonth(2024, 11), addDays(new Date(Date.UTC(2024, 1, 28)), 2).toISOString().slice(0, 10));
```

```text
29 28 31 2024-03-01
```

Adding `n * 864e5` is correct in UTC; in local time with daylight-saving changes a day can have a different length.

</details>

## 4. 🟡 Money and a card mask

**Task:** the function `money(cents)` formats an integer number of cents as dollars (`123450` → `$1,234.50`, `5` → `$0.05`); `mask(card)` replaces all digits except the last four with `*`.

**Expected result:** `$1,234.50 $0.05 ************1111`.

<details><summary>Hint</summary>

`Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })` and division by `100`; `slice(-4).padStart(length, '*')`.

</details>

<details><summary>Solution</summary>

```js
const money = (cents) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(cents / 100);
const mask = (card) => card.slice(-4).padStart(card.length, '*');
console.log(money(123450), money(5), mask('4111111111111111'));
```

```text
$1,234.50 $0.05 ************1111
```

</details>

## 5. 🔴 String length in three ways

**Task:** for the string `'é😀👨‍👩‍👧'` print the length in code units, code points and graphemes, and also the length after `normalize()`. Then compare `'café'` and `'café'` with `===`, with `normalize()` and with `localeCompare`.

**Expected result:** `12 8 3 11`; `false true 0`.

<details><summary>Hint</summary>

`text.length`, `[...text].length`, `[...new Intl.Segmenter('en', { granularity: 'grapheme' }).segment(text)].length`.

</details>

<details><summary>Solution</summary>

```js
const text = 'é😀👨‍👩‍👧';
const seg = new Intl.Segmenter('en', { granularity: 'grapheme' });
console.log(text.length, [...text].length, [...seg.segment(text)].length, text.normalize().length);
const a = 'café', b = 'café';
console.log(a === b, a.normalize() === b.normalize(), a.localeCompare(b));
```

```text
12 8 3 11
false true 0
```

`normalize()` merged `e` and the combining accent into one code unit (12 → 11); `localeCompare` treats canonically equal strings as equal.

</details>
