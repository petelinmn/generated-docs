---
title: "Quiz: strings, numbers, dates, regular expressions"
lang: en
track: javascript
kind: module
doc: quiz
slug: 08-strings-numbers-dates-regex
part: 1
tags: [strings, unicode, date, intl, regexp]
status: draft
updated: 2026-11-23
---

# Quiz: strings, numbers, dates, regular expressions

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `'a-b-c'.replace('-', '+')` return?

- A) `'a+b+c'`
- B) `'a+b-c'`
- C) `'a-b-c'`
- D) `['a', 'b', 'c']`

<details><summary>Answer</summary>

**B.** `replace` with a string replaces only the first occurrence; for all use `replaceAll`.
→ [Basics](basics.en.md#strings)

</details>

## 2. 🟢 What is `'😀'.length`?

- A) `1`
- B) `2`
- C) `4`
- D) `8`

<details><summary>Answer</summary>

**B.** Two 16-bit code units (a surrogate pair); `[...'😀'].length` is `1`.
→ [Basics](basics.en.md#unicode-a-first-look)

</details>

## 3. 🟢 What does `(1.005).toFixed(2)` return?

- A) `'1.01'`
- B) `'1.00'`
- C) `1.01`
- D) `1`

<details><summary>Answer</summary>

**B.** `toFixed` returns a **string**, and `1.005` is stored as `1.00499…`.
→ [Basics](basics.en.md#formatting)

</details>

## 4. 🟢 What does `Math.round(-2.5)` return?

- A) `-3`
- B) `-2`
- C) `-2.5`
- D) `2`

<details><summary>Answer</summary>

**B.** A half rounds toward `+∞`: `2.5` → `3`, `-2.5` → `-2`.
→ [Basics](basics.en.md#math)

</details>

## 5. 🟢 What number does January have in `new Date(2024, ?, 15)`?

- A) `1`
- B) `0`
- C) `12`
- D) Any

<details><summary>Answer</summary>

**B.** Months are numbered from zero.
→ [Basics](basics.en.md#date-the-basics)

</details>

## 6. 🟡 What does `new Date(2024, 0, 31, 25).getDate()` give?

- A) `31`
- B) `25`
- C) `1`
- D) `NaN`

<details><summary>Answer</summary>

**C.** The hour `25` "overflows" into the next day (1 February).
→ [Advanced](advanced.en.md#date-traps)

</details>

## 7. 🟡 How is `new Date('2024-01-15')` parsed compared with `new Date(2024, 0, 15)`?

- A) The same
- B) The first is midnight UTC, the second is midnight local time
- C) The first is local, the second is UTC
- D) The first is `Invalid Date`

<details><summary>Answer</summary>

**B.** The values coincide only in the UTC zone; in UTC+4 they differed in the experiment.
→ [Advanced](advanced.en.md#date-traps)

</details>

## 8. 🟡 What does the code print?

```js
const g = /a/g;
console.log(g.test('aa'), g.test('aa'), g.test('aa'));
```

- A) `true true true`
- B) `true true false`
- C) `true false false`
- D) `false false false`

<details><summary>Answer</summary>

**B.** A `g` regex has state `lastIndex`: the third call starts past the end of the string and resets it.
→ [Advanced](advanced.en.md#flags-and-state)

</details>

## 9. 🟡 What does `/\d+(?!px)/.exec('12px 34em')[0]` return?

- A) `'34'`
- B) `'1'`
- C) `'12'`
- D) `null`

<details><summary>Answer</summary>

**B.** The engine backtracked one digit, after which there is no `px`. Correct: `\d+(?!\d|px)` gives `'34'`.
→ [Advanced](advanced.en.md#groups-and-lookaround)

</details>

## 10. 🟡 Select all correct statements

- [ ] A) `replaceAll` with a `RegExp` without the `g` flag throws a `TypeError`
- [ ] B) The `s` flag lets `.` match a newline
- [ ] C) `Intl.PluralRules('ru').select(2)` returns `'one'`
- [ ] D) `normalize()` brings `'é'` to `'é'`

<details><summary>Answer</summary>

**A, B, D.** C is wrong: for `2` in Russian it is `'few'`.
→ [Advanced](advanced.en.md#intl-in-detail)

</details>

## 11. 🔴 Find the flaw

> "`'ä' > 'z'` is false, so when sorting words `ä` will go before `z`."

<details><summary>Answer</summary>

The operators compare character codes: `'ä'` (U+00E4) is greater than `'z'` (U+007A), so `'ä' > 'z'` is `true`, and `sort()` puts `ä` after `z`. For language order you need `localeCompare` / `Intl.Collator`.
→ [Basics](basics.en.md#comparing-strings)

</details>

## 12. 🔴 Why can `/^(a+)+$/.test('aaaaaaaaaaaaaaaaaaaaaaaab')` run for a very long time?

- A) The string is too long for JavaScript
- B) Nested repeats cause exponential backtracking (ReDoS)
- C) `test` is always slow
- D) Because of the `g` flag

<details><summary>Answer</summary>

**B.** The engine tries an exponential number of ways to split the `a`s between groups. In the experiment on 24 characters — more than 50 ms.
→ [Advanced](advanced.en.md#errors-and-redos)

</details>

## 13. 🔴 Open question

How do you reliably store and display a moment in time in an application?

<details><summary>Key points of a good answer</summary>

- Store and pass in UTC: milliseconds or an ISO string with `Z`.
- Apply the zone and language only on output: `Intl.DateTimeFormat` with `timeZone`.
- Do not use strings without a zone; remember daylight-saving changes and field "overflow".
- For complex calendar arithmetic — a library (there is no Temporal in Node.js 24.15).

</details>

→ [Advanced](advanced.en.md#date-traps)
