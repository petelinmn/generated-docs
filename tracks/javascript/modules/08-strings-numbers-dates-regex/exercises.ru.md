---
title: "Упражнения: строки, числа, даты, регулярные выражения"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 08-strings-numbers-dates-regex
part: 1
tags: [strings, regexp, date, intl, unicode]
status: draft
updated: 2026-11-23
---

# Упражнения: строки, числа, даты, регулярные выражения

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Запускайте код в файле ES-модуля.

## 1. 🟢 Slug

**Задача:** напишите `slugify(s)`: нижний регистр, всё, кроме латинских букв и цифр, заменяется одним дефисом, дефисы по краям убираются.

**Ожидаемый результат:** `slugify('  Hello,  World!  JS ')` → `hello-world-js`, `slugify('---A--B---')` → `a-b`.

<details><summary>Подсказка</summary>

`replace(/[^a-z0-9]+/g, '-')`, затем `replace(/^-+|-+$/g, '')`.

</details>

<details><summary>Решение</summary>

```js
const slugify = (s) => s.trim().toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '');
console.log(slugify('  Hello,  World!  JS '), slugify('---A--B---'));
```

```text
hello-world-js a-b
```

</details>

## 2. 🟢 Разбор строки лога

**Задача:** разберите строку `'2024-05-17 ERROR [db] Connection lost'` на `date`, `level`, `tag`, `msg` с помощью именованных групп. Для строки, не подходящей под формат, функция должна вернуть `undefined`.

**Ожидаемый результат:** `{ date: '2024-05-17', level: 'ERROR', tag: 'db', msg: 'Connection lost' }` и `undefined`.

<details><summary>Подсказка</summary>

`match(...)?.groups`, шаблон `(?<date>\d{4}-\d{2}-\d{2}) (?<level>[A-Z]+) \[(?<tag>\w+)\] (?<msg>.*)`.

</details>

<details><summary>Решение</summary>

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

## 3. 🟡 Календарные расчёты в UTC

**Задача:** напишите `daysInMonth(year, month)` (месяц с нуля) и `addDays(date, n)`. Проверьте февраль 2024 и 2023, декабрь 2024 и `28 февраля 2024 + 2 дня`.

**Ожидаемый результат:** `29 28 31 2024-03-01`.

<details><summary>Подсказка</summary>

«Нулевой день» следующего месяца — последний день текущего: `Date.UTC(y, m + 1, 0)`; сутки — `864e5` мс.

</details>

<details><summary>Решение</summary>

```js
const daysInMonth = (y, m) => new Date(Date.UTC(y, m + 1, 0)).getUTCDate();
const addDays = (d, n) => new Date(d.getTime() + n * 864e5);
console.log(daysInMonth(2024, 1), daysInMonth(2023, 1), daysInMonth(2024, 11), addDays(new Date(Date.UTC(2024, 1, 28)), 2).toISOString().slice(0, 10));
```

```text
29 28 31 2024-03-01
```

Сложение `n * 864e5` корректно в UTC; для местного времени с переходами на летнее время сутки бывают другой длины.

</details>

## 4. 🟡 Деньги и маска карты

**Задача:** функция `money(cents)` форматирует целое число копеек как доллары (`123450` → `$1,234.50`, `5` → `$0.05`); `mask(card)` заменяет все цифры, кроме последних четырёх, на `*`.

**Ожидаемый результат:** `$1,234.50 $0.05 ************1111`.

<details><summary>Подсказка</summary>

`Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })` и деление на `100`; `slice(-4).padStart(длина, '*')`.

</details>

<details><summary>Решение</summary>

```js
const money = (cents) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(cents / 100);
const mask = (card) => card.slice(-4).padStart(card.length, '*');
console.log(money(123450), money(5), mask('4111111111111111'));
```

```text
$1,234.50 $0.05 ************1111
```

</details>

## 5. 🔴 Длина строки тремя способами

**Задача:** для строки `'é😀👨‍👩‍👧'` выведите длину в кодовых единицах, кодовых точках и графемах, а также длину после `normalize()`. Затем сравните `'café'` и `'café'` через `===`, через `normalize()` и через `localeCompare`.

**Ожидаемый результат:** `12 8 3 11`; `false true 0`.

<details><summary>Подсказка</summary>

`text.length`, `[...text].length`, `[...new Intl.Segmenter('en', { granularity: 'grapheme' }).segment(text)].length`.

</details>

<details><summary>Решение</summary>

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

`normalize()` склеил `e` и комбинирующий акцент в одну кодовую единицу (12 → 11); `localeCompare` считает канонически равные строки равными.

</details>
