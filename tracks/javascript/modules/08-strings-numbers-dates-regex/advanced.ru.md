---
title: "Строки, числа, даты, регулярные выражения: продвинутая теория"
lang: ru
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

# Строки, числа, даты, регулярные выражения: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры выполнены на Node.js 24.15 в файле ES-модуля; часовой пояс машины — UTC+4.

## Юникод подробнее

Три уровня «длины» строки:

```js
const fam = '👨‍👩‍👧';
console.log(fam.length, [...fam].length, [...new Intl.Segmenter('en', { granularity: 'grapheme' }).segment(fam)].length);
console.log('😀'.length, '\u{1F600}' === '😀', 'abc'.charCodeAt(1), String.fromCharCode(97, 98));
```

```text
8 5 1
2 true 98 ab
```

- `length` считает 16-битные кодовые единицы (8 у семейного эмодзи).
- Итерация по строке (`[...fam]`) идёт по кодовым точкам (5: три человека и два невидимых соединителя).
- Пользователь видит **одну графему**. Правильно считать их можно через `Intl.Segmenter` (результат `1`).
- Эмодзи записывается и как литерал, и как `\u{1F600}`; `charCodeAt` возвращает кодовую единицу, `codePointAt` — кодовую точку.

### Нормализация

```js
const acc = 'e\u0301';
console.log(acc.length, acc.normalize().length, acc === 'é', acc.normalize() === 'é');
```

```text
2 1 false true
```

Два способа записать `é` дают разные строки. Перед сравнением и хешированием текст пользователя нормализуют (`normalize('NFC')`).

### Кодирование: URL, Base64, байты

```js
console.log(encodeURIComponent('a b&é'), decodeURIComponent('%D0%B0'), btoa('hi'), atob('aGk='));
console.log(new TextEncoder().encode('é'), new TextDecoder().decode(new Uint8Array([208, 176])));
```

```text
a%20b%26%C3%A9 а aGk= hi
Uint8Array(2) [ 195, 169 ] а
```

- `encodeURIComponent` кодирует часть URL (буква `é` — два байта UTF-8: `%C3%A9`).
- `btoa`/`atob` работают с Base64 для строк из «байтовых» символов.
- `TextEncoder`/`TextDecoder` переводят между строкой и байтами UTF-8: `'é'` — это байты `195, 169`.

## Шаблонные теги

Функция-«тег» получает куски строки и значения, а также сырой текст:

```js
const tag = (strs, ...v) => strs.raw.join('|') + v.join(',');
console.log(tag`a${1}b\n${2}c`, String.raw`C:\new\table`);
```

```text
a|b\n|c1,2 C:\new\table
```

`strs.raw` сохраняет `\n` как два символа, а `String.raw` удобен для путей и регулярных выражений. Так устроены библиотеки для SQL и HTML-шаблонов (в опытах не запускались).

## Intl подробнее

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

- `NumberFormat` с `style: 'currency'` подставляет символ и разделители языка (в русском формате пробелы — неразрывные).
- `PluralRules` выбирает форму множественного числа: в русском `2` — `few` («2 файла»), в английском `1` — `one`.
- `ListFormat` собирает список по правилам языка, `RelativeTimeFormat` — «вчера», «через 3 дня».
- `Collator` сортирует по правилам языка.

> ⚠️ Проверить: `Intl.DurationFormat`, `Intl.Locale` и набор доступных языков зависят от сборки Node.js (ICU); в опытах использованы `en`, `de`, `ru`.

## Date: ловушки

Результаты зависят от часового пояса машины (в опыте UTC+4):

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

- **Строка только с датой** (`'2024-01-15'`) разбирается как **UTC**; строка с временем без `Z` (`'2024-01-15T00:00:00'`) — как **местное** время; то же, что `new Date(2024, 0, 15)`. Поэтому второе сравнение истинно, а третье в поясе UTC+4 ложно (в UTC оно было бы истинным).
- Выход за диапазон «переливается»: час `25` дал следующий день (`getDate()` — `1`), а `29 февраля 2023` — `1 марта`.
- Диапазон дат ограничен ±8,64·10¹⁵ мс; за пределами — `NaN`.
- Разность дат — миллисекунды; делить на `864e5` (сутки) подходит для дат в UTC, но при переходе на летнее время сутки бывают 23 или 25 часов.

Форматирование:

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

- Всегда указывайте `timeZone` в `toLocaleDateString`/`DateTimeFormat`, если нужен предсказуемый вывод.
- `getTimezoneOffset()` — разница с UTC в минутах (`-240` для UTC+4; знак обратный привычному).
- Объект `Temporal` (современный API дат) в Node.js 24.15 **отсутствует** (`typeof Temporal` — `'undefined'`): для сложных операций с календарём и часовыми поясами используют специализированные библиотеки. Храните моменты как UTC (`toISOString()`, миллисекунды).

## Регулярные выражения подробнее

### Группы и lookaround

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

- Именованные группы `(?<имя>…)` попадают в `match.groups`.
- `matchAll` возвращает итератор всех совпадений **с группами** (нужен флаг `g`).
- Lookbehind `(?<=\$)` и lookahead `(?!px)` проверяют контекст, не включая его в результат.
- Ловушка: `/\d+(?!px)/` на `'12px'` совпало с `'1'` — движок откатился на одну цифру, после которой нет `px`. Нужно запретить и продолжение числа: `\d+(?!\d|px)` (в опыте даёт `'34'`).
- Жадные квантификаторы (`+`, `*`) берут максимум (`.*` захватил обе `<b>…</b>`), ленивые (`+?`, `*?`) — минимум.

### Флаги и состояние

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

- У регулярного выражения с флагом `g` (и `y`) есть **состояние** `lastIndex`: повторные `test` идут дальше по строке и в конце сбрасываются — результат `test` на одном и том же `RegExp` меняется. Не храните такие выражения глобально, если используете `test` с `g`.
- `s` — точка совпадает с переводом строки; `m` — `^` и `$` работают на каждой строке; `\b` — граница слова.
- `u` включает режим Юникода: без него `.` совпадает с половинкой суррогатной пары; с ним доступны `\u{…}` и `\p{L}` (свойства Юникода).
- `y` — «липкий» поиск ровно с `lastIndex`; `d` добавляет в результат `indices` (позиции).

### Ошибки и ReDoS

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

- Некорректный шаблон — `SyntaxError`.
- `replaceAll` с регулярным выражением требует флага `g`.
- **ReDoS:** вложенные повторы вроде `(a+)+` при несовпадении перебирают экспоненциально много вариантов. Время растёт экспоненциально с длиной входа: уже на 24 символах оно превысило 50 мс (в опыте). Не используйте такие шаблоны для недоверенного ввода.

### Типичные приёмы

```js
console.log('The quick brown fox'.split(' ').map(w => w[0].toUpperCase() + w.slice(1).toLowerCase()).join(' '));
console.log('camelCaseString'.replace(/([A-Z])/g, '_$1').toLowerCase(), 'a-b_c d'.split(/[-_ ]/), '  x  y '.trim().split(/\s+/));
```

```text
The Quick Brown Fox
camel_case_string [ 'a', 'b', 'c', 'd' ] [ 'x', 'y' ]
```

## Лучшие практики

- Для языка и региона — `Intl`, а не ручные форматы.
- Время храните и передавайте в UTC (ISO-строка с `Z`), в местное переводите на выводе.
- Деньги — в минимальных единицах.
- Регулярные выражения — короткие, с комментариями и тестами; для HTML и JSON — парсеры.
- Нормализуйте строки перед сравнением и поиском.
- Не создавайте общий `RegExp` с флагом `g` для `test`.

> ⚠️ Проверить: рекомендации про библиотеки для календарных расчётов (date-fns, Luxon и др.) и точное время ReDoS при разной длине входа в опытах не измерялось.

## Частые ошибки

- ❌ Сравнивать `new Date('2024-01-15')` с локальной датой → ✅ единый способ создания (UTC).
- ❌ Складывать/вычитать месяцы напрямую, не учитывая переполнение → ✅ проверять конец месяца.
- ❌ `\d+(?!px)` в расчёте на «число без px» → ✅ учитывать откат.
- ❌ `RegExp` с `g` в общем состоянии и `test` → ✅ новый литерал или `lastIndex = 0`.
- ❌ `toLocaleDateString` без `timeZone` в тестах → ✅ фиксировать пояс.
- ❌ `str.length` как число графем → ✅ `Intl.Segmenter`.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Числа и точность: [02 · Типы и значения](../02-types-and-values/expert.ru.md#числа-ieee-754)
- Копирование объектов и `groupBy`: [05 · Объекты и прототипы](../05-objects-and-prototypes/overview.ru.md)

## Источники

- [MDN: Intl](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl)
- [MDN: Intl.Segmenter](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/Segmenter)
- [MDN: Date — date-time string format](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date#date_time_string_format)
- [MDN: Regular expression syntax](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Regular_expressions)
- [MDN: Unicode character class escape](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Regular_expressions/Unicode_character_class_escape)
