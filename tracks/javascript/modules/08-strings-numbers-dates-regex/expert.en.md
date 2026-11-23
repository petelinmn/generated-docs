---
title: "Strings, numbers, dates, regular expressions: expert"
lang: en
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

# Strings, numbers, dates, regular expressions: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15 (an ES module, the machine's time zone is UTC+4).

## Reference

### String methods

| Group | Methods (those run in the experiment are in bold) |
|---|---|
| Access | `at`, `charAt`, `charCodeAt`, **`codePointAt`**, `[]`, `length` |
| Search | `indexOf`, `lastIndexOf`, **`includes`**, **`startsWith`**, **`endsWith`**, `search`, `match`, `matchAll` |
| Slices | `slice`, `substring`, `split` |
| Modification (return a new one) | `trim`, `trimStart`, `trimEnd`, `padStart`, `padEnd`, `repeat`, `replace`, `replaceAll`, `toUpperCase`, `toLowerCase`, `normalize`, `concat` |
| Comparison | `localeCompare`, `<` / `>` |
| Other | `String.raw`, `String.fromCharCode`, `String.fromCodePoint`, `isWellFormed`, `toWellFormed` |

> ⚠️ Verify: `charAt`, `concat`, `isWellFormed`, `toWellFormed`, `trimEnd` and `substr` (legacy) come from the documentation and were not run in the experiments.

### Number and Math

| Method | Result (in the experiment) |
|---|---|
| `toFixed(2)` | a string: `(1234.5678).toFixed(2)` → `'1234.57'`, `(1.005).toFixed(2)` → `'1.00'` |
| `toPrecision(6)` | `'1234.57'` |
| `toExponential(2)` | `'1.23e-6'` for `0.000001234` |
| `toString(radix)` | `(255).toString(16)` → `'ff'`, `(255).toString(2)` → `'11111111'` |
| `Math.round` | `2.5` → `3`, `-2.5` → `-2` |
| `Math.trunc/floor/ceil` | `-2.7` → `-2/-3`, `-2.2` → `-2` |
| `Math.max()/min()` | `-Infinity` / `Infinity`; with `NaN` → `NaN` |
| `Math.hypot/cbrt/sign` | `5`, `3`, `-1` |
| `**` | `2 ** 10` → `1024`, `(-8) ** (1/3)` → `NaN` |
| `%` | `-5 % 3` → `-2`, `5.5 % 2` → `1.5` |

### Forms of the Date constructor

| Form | Meaning |
|---|---|
| `new Date()` | the current moment |
| `new Date(ms)` | milliseconds from the epoch (UTC) |
| `new Date('2024-01-15')` | only a date → **UTC** (in the experiment `2024-01-15T00:00:00.000Z`) |
| `new Date('2024-01-15T00:00:00')` | date and time without a zone → **local time** |
| `new Date('2024-01-15T00:00:00Z')` | UTC |
| `new Date(y, m, d, h, mi, s, ms)` | **local** time; `m` from zero; overflow "spills over" |
| `Date.UTC(y, m, d, ...)` | milliseconds for UTC components |
| `new Date('nope')` | `Invalid Date`, `getTime()` is `NaN` |

Range: ±8.64·10¹⁵ ms from the epoch; the maximum in the experiment is `+275760-09-13T00:00:00.000Z`, one more gives `NaN`.

> ⚠️ Verify: parsing of non-standard formats (`'Jan 15 2024'`, `'2024/01/15'`) depends on the implementation and was not run in the experiments.

### RegExp flags

| Flag | Property | Effect |
|---|---|---|
| `g` | `global` | all matches; `lastIndex` stores the position |
| `i` | `ignoreCase` | case-insensitive |
| `m` | `multiline` | `^`/`$` on each line |
| `s` | `dotAll` | `.` matches `\n` |
| `u` | `unicode` | code points, `\u{…}`, `\p{…}` |
| `y` | `sticky` | match exactly at `lastIndex` |
| `d` | `hasIndices` | positions in `indices` |

> ⚠️ Verify: the `v` flag (extended Unicode mode) and `RegExp.escape` in Node.js 24.15 were not run in the experiments.

### Intl: summary

| Object | What it does (in the experiment) |
|---|---|
| `Intl.NumberFormat` | currencies, separators, `compact`: `1.2M` |
| `Intl.DateTimeFormat` | date formats, `timeZone`, `dateStyle` |
| `Intl.PluralRules` | `ru`: `2` → `few`; `en`: `1` → `one` |
| `Intl.ListFormat` | `a, b, and c` |
| `Intl.RelativeTimeFormat` | `yesterday` for `-1, 'day'` |
| `Intl.Collator` | sorting by language rules |
| `Intl.Segmenter` | graphemes, words, sentences |

## Internals

- **A string is a sequence of 16-bit code units.** Indexes, `length`, `charCodeAt` operate on units, while iteration and `codePointAt` operate on code points. A "visible character" (a grapheme) can consist of several points.
- **Numbers are a `double` (IEEE 754).** `toFixed` and `toPrecision` format the exact binary value, which is why `1.005` prints as `1.00`. The `%` operation is a remainder with the dividend's sign.
- **Date is a single number.** An object stores milliseconds from the epoch in UTC; the "local" getters (`getDate`, `getHours`) each time apply the runtime's time zone, while `getUTC*` do not. So identical code gives different output in different zones.
- **Parsing a date string.** The ISO format (`YYYY-MM-DD`, `…T…Z`) is standardized: only a date is UTC, a date with a time without a zone is local time. Other formats depend on the implementation.
- **Regular expressions are a backtracking automaton.** On failure the engine tries alternatives; nested repeats give an exponential number of variants (ReDoS: in the experiment `/^(a+)+$/` on 24 characters took more than 50 ms). Lookahead/lookbehind check context without consuming characters, lazy quantifiers take the minimum.
- **`RegExp` state.** For `g` and `y` an object has `lastIndex`; `test` and `exec` read and update it (in the experiment the sequence `1`, `2`, `0`). `match` with `g` and `replace` reset it themselves.
- **`Intl`** relies on locale data (ICU) in the Node.js build; language support depends on the build.

> ⚠️ Verify: the design of V8's regular expression optimizations (native compilation, `irregexp`, the linear engine) and the string's in-memory size (one-byte versus two-byte representation) were not checked.

## Gotchas

### 1. `length` does not equal the number of characters

`'😀'.length` is `2`, the family emoji is `8` units, `5` points, `1` grapheme (in the experiment).

### 2. Two kinds of `é`

`'é' === 'é'` is `false`; `normalize()` helps.

### 3. `toFixed` and rounding

`(1.005).toFixed(2)` is `'1.00'`, `Math.round(1.005 * 100) / 100` is `1`.

### 4. `Math.round` on halves

`Math.round(-2.5)` is `-2`, `Math.round(2.5)` is `3`.

### 5. Months from zero and overflow

`new Date(2024, 0, 31, 25).getDate()` is `1` (a day of the next month); `setUTCMonth(1)` on 31 January gives 2 March.

### 6. A date as a string versus a date as numbers

`new Date('2024-01-15')` is UTC, while `new Date(2024, 0, 15)` is local time. In the UTC+4 zone their `getTime()` values are not equal (in the experiment `false`).

### 7. The state of a `g` regex

`true, true, false` on the same expression for the same string.

### 8. `replaceAll` with an ordinary `RegExp`

`TypeError: String.prototype.replaceAll called with a non-global RegExp argument`.

### 9. Backtracking in a lookahead

On `'12px'` it matches `'1'`. You need to forbid the number's continuation: `\d+(?!\d|px)`.

### 10. ReDoS

`/^(a+)+$/` on `'aaaa…b'` of 24 characters took more than 50 ms (in the experiment).

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Built-in `Date` | available everywhere | an awkward API, zone traps |
| Date libraries (date-fns, Luxon) | a convenient API, zones | a dependency; not run |
| `Temporal` | a modern API | absent in Node.js 24.15 (in the experiment `typeof Temporal` is `'undefined'`) |
| `Intl` | localization without dependencies | depends on ICU data |
| Regular expressions | concise patterns | readability, ReDoS |
| A parser or a finite automaton | reliable for complex formats | more code |
| Integer minimal units for money | exactness | manual formatting |

## Interview questions

<details><summary>1. Why is <code>'😀'.length</code> equal to 2?</summary>

A string is stored as 16-bit UTF-16 code units; an emoji is a surrogate pair.

→ [Advanced theory](advanced.en.md#unicode-in-detail)

</details>

<details><summary>2. How do you compare strings correctly with the language in mind?</summary>

`localeCompare` or `Intl.Collator`; the operators `<`, `>` compare codes.

→ [Basic theory](basics.en.md#comparing-strings)

</details>

<details><summary>3. Trick: why does <code>(1.005).toFixed(2)</code> give <code>'1.00'</code>?</summary>

The number is stored as `1.00499…`; `toFixed` formats the exact value.

→ [Gotcha 3](#3-tofixed-and-rounding)

</details>

<details><summary>4. What does a <code>Date</code> object store?</summary>

Milliseconds from `1970-01-01T00:00:00Z`; the zone is applied only when reading "local" getters and formatting.

→ [Internals](#internals)

</details>

<details><summary>5. Trick: what is the difference between <code>new Date('2024-01-15')</code> and <code>new Date(2024, 0, 15)</code>?</summary>

The first is midnight UTC, the second is midnight local time; the values coincide only in the UTC zone.

→ [Gotcha 6](#6-a-date-as-a-string-versus-a-date-as-numbers)

</details>

<details><summary>6. Why is the month in <code>Date</code> numbered from zero and what happens on overflow?</summary>

A historical decision; overflow "spills over" into the next period (`new Date(2024, 1, 0)` is 31 January).

→ [Basic theory](basics.en.md#date-the-basics)

</details>

<details><summary>7. How does <code>match</code> with the <code>g</code> flag differ from without it?</summary>

Without `g` — the first match with groups and `index`; with `g` — an array of all matches without groups (`matchAll` gives groups).

→ [Basic theory](basics.en.md#regular-expressions-the-basics)

</details>

<details><summary>8. Trick: what happens on repeated <code>test</code> calls on <code>/a/g</code>?</summary>

The result depends on `lastIndex`: `true, true, false` on the same string.

→ [Gotcha 7](#7-the-state-of-a-g-regex)

</details>

<details><summary>9. What is ReDoS and how do you avoid it?</summary>

Exponential backtracking on nested repeats; avoid `(a+)+`, limit input length, use proven patterns.

→ [Advanced theory](advanced.en.md#errors-and-redos)

</details>

<details><summary>10. What do the flags <code>u</code>, <code>s</code>, <code>y</code>, <code>d</code> do?</summary>

`u` — Unicode, `s` — the dot includes `\n`, `y` — search exactly at `lastIndex`, `d` — match positions.

→ [Reference](#regexp-flags)

</details>

<details><summary>11. How do you format numbers and dates for another language?</summary>

`Intl.NumberFormat`, `Intl.DateTimeFormat` (or `toLocale…String`) with the language and `timeZone` specified.

→ [Advanced theory](advanced.en.md#intl-in-detail)

</details>

<details><summary>12. Trick: what does <code>/\d+(?!px)/.exec('12px')</code> return?</summary>

`'1'`: the engine backtracked one digit, after which there is no `px`.

→ [Gotcha 9](#9-backtracking-in-a-lookahead)

</details>

## Cheat sheet

```text
s.slice / at / includes / padStart / replaceAll / localeCompare / normalize
[...s]  new Intl.Segmenter('en', { granularity: 'grapheme' })
n.toFixed(2)  n.toString(16)  Math.round / trunc / floor / ceil
new Intl.NumberFormat(lang, opts).format(n)   new Intl.DateTimeFormat(lang, { timeZone }).format(d)
new Date(Date.UTC(…))  d.toISOString()  d.getTime()  Date.now()
/(?<name>…)/g  s.match / matchAll / replace / test   flags g i m s u y d
```

- Store time in UTC; months from zero.
- A `g` regex has state; `replaceAll` requires `g`.
- Do not use regular expressions for HTML.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Numbers and comparison: [02 · Types and values](../02-types-and-values/overview.en.md)
- Arrays and collections: [07 · Arrays and collections](../07-arrays-and-collections/overview.en.md)
- Security and performance — modules 19 and 20 of the [track](../../overview.en.md) roadmap

## Sources

- [ECMAScript: String Objects](https://tc39.es/ecma262/#sec-string-objects)
- [ECMAScript: Date Objects](https://tc39.es/ecma262/#sec-date-objects)
- [ECMAScript: RegExp Objects](https://tc39.es/ecma262/#sec-regexp-regular-expression-objects)
- [ECMA-402: Intl](https://tc39.es/ecma402/)
- [MDN: Date.parse()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date/parse)
