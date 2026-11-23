---
title: "Strings, numbers, dates, regular expressions"
lang: en
track: javascript
kind: module
doc: overview
slug: 08-strings-numbers-dates-regex
part: 1
levels: [1, 2, 3, 4]
tags: [strings, unicode, intl, number-formatting, math, date, timezone, regexp, named-groups, lookahead, redos, template-literals]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Strings, numbers, dates, regular expressions

🌐 [Русский](overview.ru.md) · **English**

## In short

This topic joins four everyday tools. **Strings** are immutable, stored as a sequence of 16-bit UTF-16 code units, and have a rich set of methods. **Numbers** are 64-bit floating-point values: formatting uses `toFixed`, `toString(radix)` and `Intl.NumberFormat`, calculation uses the `Math` object. **Dates** (`Date`) are a number of milliseconds since 1 January 1970 (UTC) with an awkward API and time-zone traps. **Regular expressions** (`RegExp`) search and replace text by a pattern. For localization (number and date formats, lists, plurals) the built-in `Intl` is used.

## Key ideas

- **Strings are immutable;** methods return new ones. `length` counts UTF-16 code units, not characters.
- **For language-aware comparison** use `localeCompare` or `Intl.Collator`, not `<` and `>`.
- **`toFixed` returns a string and suffers from binary precision:** `(1.005).toFixed(2)` is `'1.00'`.
- **`Intl` formats numbers, dates and lists by language rules** without third-party libraries.
- **`Date` is milliseconds from the start of the epoch in UTC;** months are numbered from zero, and the string `'2024-01-15'` (without a time) is parsed as UTC, while `new Date(2024, 0, 15)` is local time.
- **Going out of range "overflows":** `new Date(2024, 1, 0)` is the last day of January.
- **Regular expressions have flags:** `g` (all matches), `i` (case), `m`, `s`, `u` (Unicode), `y`, `d`; groups can be numbered or named.
- **The `g` flag makes a `RegExp` stateful:** `lastIndex` is kept between `test` calls.
- **Greedy quantifiers and nested repeats can "blow up" the time** (ReDoS).

## Minimal example

```js
console.log('Hello'.padStart(8, '*'), (1234.5678).toFixed(2), new Intl.NumberFormat('en-US').format(1234567.891));
console.log(new Date(Date.UTC(2024, 0, 31)).toISOString(), '2024-05-17'.match(/(?<y>\d{4})-(?<m>\d{2})/).groups.y);
```

```text
***Hello 1234.57 1,234,567.891
2024-01-31T00:00:00.000Z 2024
```

## When to use and when not

| Task | Tool |
|---|---|
| Formatting numbers and currencies | `Intl.NumberFormat` |
| Formatting dates | `Intl.DateTimeFormat`, `toLocaleDateString` |
| Storing a moment in time | `Date.now()` or an ISO string in UTC |
| Complex calendar and time-zone operations | a specialized library (Temporal is absent in this environment) |
| A simple substring search | `includes`, `indexOf`, `startsWith` |
| Pattern search and replace | `RegExp` |
| Parsing structured data (HTML, JSON) | a parser, not a regular expression |

- ❌ Do not keep money in a `float`: count in minimal units (cents).
- ❌ Do not parse HTML with regular expressions.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | string methods, comparison, Unicode, number formatting, `Math`, `Intl`, `Date`, regular expressions |
| [2. Advanced theory](advanced.en.md) | code points and graphemes, normalization, encoding, tagged templates, `Intl` in detail, `Date` traps, groups, lookaround, flags, ReDoS |
| [3. Expert theory](expert.en.md) | references (strings, `Math`, flags, `Date` forms), internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 08-strings-numbers-dates-regex level 5` |
| 6. Tests | not created yet — `/learn javascript 08-strings-numbers-dates-regex level 6` |

## Related materials

- Prerequisites: [02 · Types and values](../02-types-and-values/overview.en.md) (numbers, strings, `NaN`), [07 · Arrays and collections](../07-arrays-and-collections/overview.en.md)
- Next: [09 · Modules](../09-modules/overview.en.md)
- Regular expression security: module 20 of the [track](../../overview.en.md) roadmap
- Analogues in other tracks: [Python: strings and regular expressions](../../../python/overview.en.md), [C#: strings, dates](../../../csharp/overview.en.md)
