---
title: "Strings, numbers, dates, regular expressions: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 08-strings-numbers-dates-regex
part: 1
tags: [unicode, grapheme, normalization, textencoder, tagged-templates, intl, date-pitfalls, timezone, regexp, lookaround, named-groups, redos]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Strings, numbers, dates, regular expressions: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15 in an ES module file; the machine's time zone is UTC+4.

## Unicode in detail

Three levels of a string's "length":

```js
const fam = '👨‍👩‍👧';
console.log(fam.length, [...fam].length, [...new Intl.Segmenter('en', { granularity: 'grapheme' }).segment(fam)].length);
console.log('😀'.length, '\u{1F600}' === '😀', 'abc'.charCodeAt(1), String.fromCharCode(97, 98));
```

```text
8 5 1
2 true 98 ab
```

- `length` counts 16-bit code units (8 for the family emoji).
- Iterating a string (`[...fam]`) goes by code points (5: three people and two invisible joiners).
- The user sees **one grapheme**. They can be counted correctly with `Intl.Segmenter` (the result is `1`).
- An emoji can be written both as a literal and as `\u{1F600}`; `charCodeAt` returns a code unit, `codePointAt` a code point.

### Normalization

```js
const acc = 'e\u0301';
console.log(acc.length, acc.normalize().length, acc === 'é', acc.normalize() === 'é');
```

```text
2 1 false true
```

Two ways of writing `é` give different strings. Before comparing and hashing, user text is normalized (`normalize('NFC')`).

### Encoding: URL, Base64, bytes

```js
console.log(encodeURIComponent('a b&é'), decodeURIComponent('%D0%B0'), btoa('hi'), atob('aGk='));
console.log(new TextEncoder().encode('é'), new TextDecoder().decode(new Uint8Array([208, 176])));
```

```text
a%20b%26%C3%A9 а aGk= hi
Uint8Array(2) [ 195, 169 ] а
```

- `encodeURIComponent` encodes a part of a URL (the letter `é` is two UTF-8 bytes: `%C3%A9`).
- `btoa`/`atob` work with Base64 for strings of "byte" characters.
- `TextEncoder`/`TextDecoder` convert between a string and UTF-8 bytes: `'é'` is the bytes `195, 169`.

## Tagged templates

A "tag" function receives the pieces of the string, the values and the raw text:

```js
const tag = (strs, ...v) => strs.raw.join('|') + v.join(',');
console.log(tag`a${1}b\n${2}c`, String.raw`C:\new\table`);
```

```text
a|b\n|c1,2 C:\new\table
```

`strs.raw` keeps `\n` as two characters, and `String.raw` is handy for paths and regular expressions. SQL and HTML template libraries are built this way (not run in the experiments).

## Intl in detail

```js
console.log(new Intl.NumberFormat('ru-RU', { style: 'currency', currency: 'RUB' }).format(1234.5));
console.log(new Intl.PluralRules('ru').select(2), new Intl.PluralRules('en').select(1));
console.log(new Intl.ListFormat('en', { type: 'conjunction' }).format(['a', 'b', 'c']), new Intl.RelativeTimeFormat('en', { numeric: 'auto' }).format(-1, 'day'));
console.log(['b', 'a', 'ä'].sort(new Intl.Collator('de').compare));
```

```text
1 234,50 ₽
few one
a, b, and c yesterday
[ 'a', 'ä', 'b' ]
```

- `NumberFormat` with `style: 'currency'` substitutes the language's symbol and separators (in the Russian format the spaces are non-breaking).
- `PluralRules` picks the plural form: in Russian `2` is `few` ("2 файла"), in English `1` is `one`.
- `ListFormat` assembles a list by language rules, `RelativeTimeFormat` — "yesterday", "in 3 days".
- `Collator` sorts by language rules.

> ⚠️ Verify: `Intl.DurationFormat`, `Intl.Locale` and the set of available languages depend on the Node.js build (ICU); the experiments used `en`, `de`, `ru`.

## Date: traps

The results depend on the machine's time zone (UTC+4 in the experiment):

```js
console.log(new Date('2024-01-15').toISOString());
console.log(new Date('2024-01-15T00:00:00').getTime() === new Date(2024, 0, 15).getTime());
console.log(new Date('2024-01-15').getTime() === new Date(2024, 0, 15).getTime());
console.log(new Date(2024, 0, 31, 25).getDate(), new Date(Date.UTC(2023, 1, 29)).toISOString().slice(0, 10));
console.log(new Date(8.64e15).toISOString(), new Date(8.64e15 + 1).getTime());
console.log(Math.round((new Date('2024-12-25') - new Date('2024-01-01')) / 864e5));
```

```text
2024-01-15T00:00:00.000Z
true
false
1 2023-03-01
+275760-09-13T00:00:00.000Z NaN
359
```

- A **string with only a date** (`'2024-01-15'`) is parsed as **UTC**; a string with a time without a zone (`'2024-01-15T00:00:00'`) as **local** time; the same as `new Date(2024, 0, 15)`. So the second comparison is true, while the third is false in the UTC+4 zone (in UTC it would be true).
- Going out of range "overflows": the hour `25` gave the next day (`getDate()` is `1`), and `29 February 2023` gave `1 March`.
- The date range is limited to ±8.64·10¹⁵ ms; beyond it — `NaN`.
- The difference of dates is milliseconds; dividing by `864e5` (a day) works for dates in UTC, but across a daylight-saving change a day can be 23 or 25 hours.

Formatting:

```js
console.log(new Date(Date.UTC(2024, 5, 1)).toLocaleDateString('en-US', { timeZone: 'UTC', year: 'numeric', month: 'long', day: 'numeric' }));
console.log(new Date(Date.UTC(2024, 5, 1)).toLocaleDateString('ru-RU', { timeZone: 'UTC' }), new Intl.DateTimeFormat('en-GB', { dateStyle: 'full', timeZone: 'UTC' }).format(new Date(0)));
console.log(Intl.DateTimeFormat().resolvedOptions().timeZone, new Date(0).getTimezoneOffset(), typeof Temporal);
```

```text
June 1, 2024
01.06.2024 Thursday, 1 January 1970
Asia/Yerevan -240 undefined
```

- Always specify `timeZone` in `toLocaleDateString`/`DateTimeFormat` if you need predictable output.
- `getTimezoneOffset()` is the difference from UTC in minutes (`-240` for UTC+4; the sign is the opposite of what you expect).
- The `Temporal` object (the modern date API) is **absent** in Node.js 24.15 (`typeof Temporal` is `'undefined'`): for complex calendar and time-zone operations specialized libraries are used. Store moments as UTC (`toISOString()`, milliseconds).

## Regular expressions in detail

### Groups and lookaround

```js
const named = '2024-05-17'.match(/(?<y>\d{4})-(?<mo>\d{2})-(?<d>\d{2})/);
console.log({ ...named.groups });
console.log([...'a1b2'.matchAll(/[a-z](\d)/g)].map(m => m[1]));
console.log(/(?<=\$)\d+/.exec('cost $42')[0], /\d+(?!px)/.exec('12px 34em')[0]);
console.log('aaa'.match(/a+?/)[0], 'aaa'.match(/a+/)[0], '<b>x</b><b>y</b>'.match(/<b>.*<\/b>/)[0], '<b>x</b><b>y</b>'.match(/<b>.*?<\/b>/)[0]);
```

```text
{ y: '2024', mo: '05', d: '17' }
[ '1', '2' ]
42 1
a aaa <b>x</b><b>y</b> <b>x</b>
```

- Named groups `(?<name>…)` land in `match.groups`.
- `matchAll` returns an iterator of all matches **with groups** (the flag `g` is required).
- Lookbehind `(?<=\$)` and lookahead `(?!px)` check the context without including it in the result.
- A trap: `/\d+(?!px)/` on `'12px'` matched `'1'` — the engine backtracked one digit, after which there is no `px`. You need to forbid the number's continuation too: `\d+(?!\d|px)` (in the experiment it gives `'34'`).
- Greedy quantifiers (`+`, `*`) take the maximum (`.*` captured both `<b>…</b>`), lazy ones (`+?`, `*?`) take the minimum.

### Flags and state

```js
const g = /a/g;
console.log(g.test('aa'), g.lastIndex, g.test('aa'), g.lastIndex, g.test('aa'), g.lastIndex);
console.log(/a.c/.test('a\nc'), /a.c/s.test('a\nc'), /^b/m.test('a\nb'), /\bcat\b/.test('concat'));
console.log(/\u{1F600}/u.test('😀'), /^.$/.test('😀'), /^.$/u.test('😀'), /\p{L}+/u.exec('héllo wörld')[0], /\p{Emoji}/u.test('😀'));
console.log(new RegExp('a.b', 'gi').flags, /x/y.sticky, /a/dgi.flags);
const dre = /(?<w>b)/d.exec('abc');
console.log(dre.indices[0], dre.indices.groups.w);
```

```text
true 1 true 2 false 0
false true true false
true false true héllo true
gi true dgi
[ 1, 2 ] [ 1, 2 ]
```

- A regular expression with the `g` (and `y`) flag has **state** `lastIndex`: repeated `test` calls move along the string and reset at the end — the result of `test` on the same `RegExp` changes. Do not keep such expressions globally if you use `test` with `g`.
- `s` — the dot matches a newline; `m` — `^` and `$` work on each line; `\b` — a word boundary.
- `u` turns on Unicode mode: without it `.` matches half of a surrogate pair; with it `\u{…}` and `\p{L}` (Unicode properties) are available.
- `y` — "sticky" search exactly at `lastIndex`; `d` adds `indices` (positions) to the result.

### Errors and ReDoS

```js
try { new RegExp('[') } catch (e) { console.log(e.name + ': ' + e.message) }
try { 'x'.replaceAll(/x/, 'y') } catch (e) { console.log(e.name + ': ' + e.message) }
const start = performance.now(); /^(a+)+$/.test('a'.repeat(24) + 'b');
console.log('redos ms > 50:', performance.now() - start > 50);
```

```text
SyntaxError: Invalid regular expression: /[/: Unterminated character class
TypeError: String.prototype.replaceAll called with a non-global RegExp argument
redos ms > 50: true
```

- An invalid pattern is a `SyntaxError`.
- `replaceAll` with a regular expression requires the `g` flag.
- **ReDoS:** nested repeats like `(a+)+` on a mismatch try an exponential number of variants. The time grows exponentially with the input length: already at 24 characters it exceeded 50 ms (in the experiment). Do not use such patterns for untrusted input.

### Typical techniques

```js
console.log('The quick brown fox'.split(' ').map(w => w[0].toUpperCase() + w.slice(1).toLowerCase()).join(' '));
console.log('camelCaseString'.replace(/([A-Z])/g, '_$1').toLowerCase(), 'a-b_c d'.split(/[-_ ]/), '  x  y '.trim().split(/\s+/));
```

```text
The Quick Brown Fox
camel_case_string [ 'a', 'b', 'c', 'd' ] [ 'x', 'y' ]
```

## Best practices

- For language and region — `Intl`, not hand-made formats.
- Store and pass time in UTC (an ISO string with `Z`), convert to local on output.
- Money — in minimal units.
- Regular expressions — short, commented and tested; for HTML and JSON — parsers.
- Normalize strings before comparing and searching.
- Do not create a shared `RegExp` with the `g` flag for `test`.

> ⚠️ Verify: the recommendations about calendar libraries (date-fns, Luxon and others) and the exact ReDoS time for different input lengths were not checked in the experiments.

## Common mistakes

- ❌ Comparing `new Date('2024-01-15')` with a local date → ✅ one way of creating (UTC).
- ❌ Adding/subtracting months directly without accounting for overflow → ✅ check the end of the month.
- ❌ `\d+(?!px)` expecting "a number without px" → ✅ account for backtracking.
- ❌ A `RegExp` with `g` in shared state and `test` → ✅ a new literal or `lastIndex = 0`.
- ❌ `toLocaleDateString` without `timeZone` in tests → ✅ fix the zone.
- ❌ `str.length` as the number of graphemes → ✅ `Intl.Segmenter`.

## Related materials

- Next: [expert theory](expert.en.md)
- Numbers and precision: [02 · Types and values](../02-types-and-values/expert.en.md#numbers-ieee-754)
- Copying objects and `groupBy`: [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md)

## Sources

- [MDN: Intl](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl)
- [MDN: Intl.Segmenter](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/Segmenter)
- [MDN: Date — date-time string format](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date#date_time_string_format)
- [MDN: Regular expression syntax](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Regular_expressions)
- [MDN: Unicode character class escape](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Regular_expressions/Unicode_character_class_escape)
