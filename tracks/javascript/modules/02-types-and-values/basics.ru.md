---
title: "Типы и значения: базовая теория"
lang: ru
track: javascript
kind: module
doc: basics
slug: 02-types-and-values
part: 1
tags: [types, typeof, numbers, strings, null, undefined, coercion, equality, truthy-falsy, nullish-coalescing, optional-chaining]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Типы и значения: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Окружение и инструменты](../01-environment/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены на Node.js 24.15 в файле ES-модуля (строгий режим, `"type": "module"`).

## Зачем это нужно

Большинство «странных» результатов в JavaScript — следствие неявного приведения типов и особенностей чисел. Если знать правила, поведение становится предсказуемым, а значит, баги — редкими.

## Типы и `typeof`

Оператор `typeof` возвращает строку с названием типа:

```js
for (const v of [42, 'a', true, undefined, null, 10n, Symbol('s'), {}, [], () => {}, class A {}, NaN, new Date()])
  console.log(String(typeof v).padEnd(10), Object.prototype.toString.call(v));
```

```text
number     [object Number]
string     [object String]
boolean    [object Boolean]
undefined  [object Undefined]
object     [object Null]
bigint     [object BigInt]
symbol     [object Symbol]
object     [object Object]
object     [object Array]
function   [object Function]
function   [object Function]
number     [object Number]
object     [object Date]
```

- **Примитивы:** `number`, `string`, `boolean`, `undefined`, `null`, `bigint`, `symbol`.
- **Объекты:** всё остальное — обычные объекты, массивы, даты, функции.
- `typeof null` равен `'object'` — историческая ошибка языка, оставленная ради совместимости: `null` — примитив.
- `typeof NaN` равен `'number'`: «не число» всё равно имеет тип число.
- Массив определяют `Array.isArray(x)`, а не `typeof`.

## Числа

Тип `number` один и для целых, и для дробных; это 64-битное число с плавающей точкой (IEEE 754):

```js
console.log(0.1 + 0.2, 0.1 + 0.2 === 0.3, Math.abs(0.1 + 0.2 - 0.3) < Number.EPSILON);
console.log(Number.MAX_SAFE_INTEGER, Number.MAX_SAFE_INTEGER + 1, Number.MAX_SAFE_INTEGER + 2);
console.log(1 / 0, -1 / 0, 0 / 0);
```

```text
0.30000000000000004 false true
9007199254740991 9007199254740992 9007199254740992
Infinity -Infinity NaN
```

- Дробные числа в двоичной системе представимы неточно, поэтому `0.1 + 0.2 !== 0.3`. Сравнивайте с допуском (`Number.EPSILON`) либо работайте с целыми минимальными единицами (копейками).
- Целые точны только до `Number.MAX_SAFE_INTEGER` (2⁵³ − 1): выше соседние значения «слипаются» (`…992 + 1` и `…992 + 2` дают одно и то же).
- Деление на ноль не вызывает ошибки: получается `Infinity`, `-Infinity` или `NaN`.

### NaN

`NaN` («not a number») — результат неопределённой числовой операции. Он не равен даже самому себе:

```js
console.log(NaN === NaN, Object.is(NaN, NaN));
console.log(isNaN('abc'), Number.isNaN('abc'));
console.log(Number('12px'), parseInt('12px'), Number(''), Number(null), Number(undefined), Number([]), Number([5]), Number(true));
```

```text
false true
true false
NaN 12 0 0 NaN 0 5 1
```

`Number.isNaN(x)` проверяет, что значение именно `NaN`. Глобальная `isNaN('abc')` сначала приводит аргумент к числу и поэтому возвращает `true` для любой нечисловой строки. Преобразования: `Number('12px')` строго (`NaN`), `parseInt('12px')` читает начало строки (`12`), пустая строка и `null` дают `0`.

## Строки

Строка — неизменяемая последовательность 16-битных кодовых единиц UTF-16:

```js
const s = 'héllo 😀';
console.log(s.length, [...s].length, s[0], s.toUpperCase());
let t = 'abc';
try { t[0] = 'X'; } catch (e) { console.log(e.name + ': ' + e.message); }
console.log(t, `sum: ${1 + 2}`);
```

```text
8 7 h HÉLLO 😀
TypeError: Cannot assign to read only property '0' of string 'abc'
abc sum: 3
```

- `s.length` равен 8, а символов 7: эмодзи занимает две кодовые единицы. `[...s]` итерирует по символам (кодовым точкам).
- Изменить символ строки нельзя; в строгом режиме попытка вызывает `TypeError`, а в нестрогом молча ничего не делает.
- Строки в обратных кавычках — шаблонные: `${выражение}` подставляется в текст.

## null и undefined

```js
let u;
console.log(u, typeof u, null == undefined, null === undefined, typeof null, void 0);
```

```text
undefined undefined true false object undefined
```

- `undefined` — значение «ничего не присвоено» (у объявленной переменной без значения, у отсутствующего свойства, у функции без `return`).
- `null` — намеренно «пустое» значение, которое присваивает программист.
- `null == undefined` истинно, а `null === undefined` ложно.

## Приведение типов и сравнение

Операторы часто приводят значения к другому типу. Результаты (все получены запуском):

```js
console.log('5' + 3, '5' - 3, '5' * '2', 5 + null, 5 + undefined, true + true);
console.log([] + [], [] + {}, [1, 2] + [3]);
console.log('3' > '12', 3 > '12', '10' == 10, 0 == '', null == 0, null >= 0, undefined == 0);
```

```text
53 2 10 5 NaN 2
 [object Object] 1,23
true false true true false true false
```

(Вторая строка начинается с пустой строки — результат `[] + []`.)

- **`+`** при наличии строки склеивает: `'5' + 3` — `'53'`. Остальные арифметические операторы (`-`, `*`) приводят к числу.
- **Строки сравниваются посимвольно:** `'3' > '12'` истинно, потому что `'3'` идёт после `'1'`; `3 > '12'` приводит строку к числу.
- **`==`** приводит типы по сложным правилам: `0 == ''` истинно, `null == 0` ложно, но `null >= 0` истинно.

Практическое правило: **всегда сравнивайте через `===` и `!==`.** Единственное разумное применение `==` — `x == null` (истинно для `null` и `undefined` одновременно).

## Истинность: truthy и falsy

В условиях значение приводится к булеву. Ложных (falsy) значений ровно восемь:

```js
const falsy = [false, 0, -0, 0n, '', null, undefined, NaN];
console.log(falsy.every(v => !v));
console.log(Boolean('0'), Boolean('false'), Boolean([]), Boolean({}), Boolean(' '), Boolean(-1));
```

```text
true
true true true true true true
```

Всё остальное истинно: непустая строка (даже `'0'` и `'false'`), любой массив (даже пустой), любой объект.

## `||`, `??` и `?.`

```js
const cfg = { port: 0, name: '', opt: null };
console.log(cfg.port || 3000, cfg.port ?? 3000, cfg.name || 'anon', cfg.name ?? 'anon', cfg.opt ?? 'dflt');
console.log(cfg.x?.y, cfg.x?.[0], cfg.fn?.());
```

```text
3000 0 anon  dflt
undefined undefined undefined
```

- `a || b` возьмёт `b`, если `a` ложно — в том числе при `0` и `''`. Это частая причина багов с портами и счётчиками.
- `a ?? b` возьмёт `b` только если `a` равно `null` или `undefined`.
- `a?.b`, `a?.[i]`, `a?.()` вернут `undefined`, если `a` — `null`/`undefined`, вместо ошибки.

## Значения и ссылки

Примитивы копируются по значению, объекты — по ссылке:

```js
let p = 1, q = p; q++;
console.log(p, q);
const o1 = { n: 1 }, o2 = o1; o2.n++;
console.log(o1.n, o1 === o2, { n: 2 } === { n: 2 });
function mutate(obj) { obj.n = 99; obj = { n: 0 }; }
mutate(o1);
console.log(o1.n);
```

```text
1 2
2 true false
99
```

- Изменение `q` не затронуло `p`.
- `o1` и `o2` — это ссылки на один объект; два одинаковых по содержимому литерала — разные объекты, поэтому `{n:2} === {n:2}` ложно.
- Функция получила копию ссылки: изменение поля видно снаружи (`99`), а переприсваивание `obj = {n:0}` — нет.

## Ошибки новичка

- ❌ `typeof x === 'object'` как проверка объекта → ✅ также учитывать `null` и `Array.isArray`.
- ❌ `x == y` → ✅ `x === y`.
- ❌ `if (x === NaN)` → ✅ `Number.isNaN(x)`.
- ❌ `port || 3000` при допустимом `0` → ✅ `port ?? 3000`.
- ❌ Сравнивать объекты `===` по содержимому → ✅ сравнивать поля или использовать специальную функцию.
- ❌ Складывать деньги как дроби `0.1 + 0.2` → ✅ работать с целыми минимальными единицами.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — `BigInt`, `Symbol`, обёртки, `Object.is`, JSON
- Следующий модуль: [03 · Переменные и области видимости](../03-scopes-and-hoisting/overview.ru.md)
- Ранее: [01 · Окружение и инструменты](../01-environment/overview.ru.md) (строгий режим в ESM и в CommonJS)

## Источники

- [MDN: Data structures and types](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Data_structures)
- [MDN: Equality comparisons and sameness](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Equality_comparisons_and_sameness)
- [MDN: Nullish coalescing operator](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Nullish_coalescing)
- [ECMAScript: Types](https://tc39.es/ecma262/#sec-ecmascript-data-types-and-values)
