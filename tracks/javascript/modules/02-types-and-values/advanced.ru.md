---
title: "Типы и значения: продвинутая теория"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 02-types-and-values
part: 1
tags: [bigint, symbol, wrapper-objects, object-is, coercion, json, typeof]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Типы и значения: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры выполнены на Node.js 24.15 в файле ES-модуля.

## BigInt

`BigInt` хранит целые числа произвольной длины; литерал записывается с суффиксом `n`:

```js
console.log(2n ** 64n, 10n / 3n, typeof 1n, 1n === 1, 1n == 1, BigInt(Number.MAX_SAFE_INTEGER) + 2n);
try { 1n + 1 } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
18446744073709551616n 3n bigint false true 9007199254740993n
TypeError: Cannot mix BigInt and other types, use explicit conversions
```

- Деление целочисленное: `10n / 3n` равно `3n`.
- `1n === 1` ложно (разные типы), `1n == 1` истинно.
- Смешивать `BigInt` и `number` в арифметике нельзя: нужен явный `BigInt(x)` или `Number(x)`.
- `BigInt` вне допустимого диапазона безопасных целых даёт точные значения (`…993n`), в отличие от `number`.

## Symbol

`Symbol` — уникальное значение, обычно используемое как ключ свойства:

```js
const a = Symbol('id'), b = Symbol('id');
console.log(a === b, a.toString(), a.description, Symbol.for('k') === Symbol.for('k'));
```

```text
false Symbol(id) id true
```

Каждый вызов `Symbol('id')` создаёт новое значение, описание — только для отладки. `Symbol.for('k')` возвращает общий символ из глобального реестра. Применение символов (ключи, которые не пересекаются с чужими, и «известные символы» вроде `Symbol.iterator`) разобрано в модулях 05 и 11 дорожной карты.

## Объекты-обёртки

У примитивов есть методы, потому что язык временно оборачивает их в объекты (`'x'.toUpperCase()`). Явно созданные обёртки — источник путаницы:

```js
console.log(typeof new String('x'), new String('x') === 'x', new String('x') == 'x');
console.log(typeof Object(1), Object(1) instanceof Number, (5).toFixed(2), 'x'.constructor.name);
console.log(Boolean(new Boolean(false)));
```

```text
object false true
object true 5.00 String
true
```

`new Boolean(false)` — объект, а любой объект истинен, поэтому `Boolean(new Boolean(false))` равно `true`. Не создавайте обёртки через `new`; функции `String(x)`, `Number(x)`, `Boolean(x)` без `new` просто приводят значение.

## Object.is и нули

`Object.is` сравнивает по алгоритму «SameValue»: отличается от `===` только для `NaN` и нулей:

```js
console.log(Object.is(-0, 0), 0 === -0, Object.is(NaN, NaN), NaN === NaN);
```

```text
false true true false
```

`-0` — отдельное значение: `0 === -0` истинно, но `Object.is(-0, 0)` ложно.

## Приведение типов: когда `==` удивляет

```js
console.log([] == false, [0] == false, [1] == true, '' == 0, '0' == false, null == false, NaN == NaN, [] == ![]);
```

```text
true true true true true false false true
```

- Объекты при сравнении с примитивом приводятся к строке или числу: `[] == false`: массив даёт `''`, затем `0 == 0`.
- `[] == ![]` истинно: `![]` — `false`, затем левая часть приводится так же.
- `null == false` ложно: `null` при `==` равен только `null` и `undefined`.

Эти результаты показывают, почему `==` нужно избегать. Полный алгоритм — в [экспертной теории](expert.ru.md#алгоритм-нестрогого-сравнения).

## typeof необъявленной переменной

```js
console.log(typeof notDeclared);
try { notDeclared } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
undefined
ReferenceError: notDeclared is not defined
```

`typeof` — единственный оператор, который безопасно обращается к необъявленному имени. Чтение переменной напрямую вызывает `ReferenceError` (в отличие от `undefined` у объявленной, но не инициализированной, см. [модуль 03](../03-scopes-and-hoisting/overview.ru.md)).

## JSON и типы

`JSON.stringify` не умеет часть значений:

```js
console.log(JSON.stringify({ a: undefined, b: () => 1, c: NaN, d: new Date(0), e: [undefined] }));
try { JSON.stringify({ x: 1n }) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
{"c":null,"d":"1970-01-01T00:00:00.000Z","e":[null]}
TypeError: Do not know how to serialize a BigInt
```

- Свойства со значениями `undefined`, функциями и символами пропускаются; в массиве они становятся `null`.
- `NaN` превращается в `null`.
- Дата превращается в строку ISO 8601.
- `BigInt` вызывает ошибку: нужно преобразовать вручную.

## Лучшие практики

- Сравнивайте только через `===`, проверку на пустоту делайте `x == null`.
- `Number.isNaN`, `Number.isInteger`, `Number.isSafeInteger` вместо глобальных аналогов.
- Деньги и идентификаторы больше 2⁵³ хранить целыми минимальными единицами или `BigInt`/строкой.
- Значения по умолчанию — через `??`, не `||`, когда `0` или `''` допустимы.
- Не создавайте обёртки `new String/Number/Boolean`.

> ⚠️ Проверить: `Intl.NumberFormat`, `Number.prototype.toFixed` для округления денег и `Decimal` как предложение стандарта не запускались.

## Частые ошибки

- ❌ Смешивать `BigInt` и `number` → ✅ явные преобразования.
- ❌ Сериализовать объекты с `BigInt` в JSON → ✅ превращать в строку заранее.
- ❌ `new Boolean(false)` в условии → ✅ примитивы.
- ❌ Ждать, что `JSON.stringify` сохранит `undefined` и функции → ✅ они теряются.
- ❌ Считать `-0` и `0` неотличимыми везде → ✅ `Object.is` их различает.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Строгий режим и ошибки присваивания: [01 · Окружение и инструменты](../01-environment/advanced.ru.md#строгий-режим)
- Области видимости и `ReferenceError`: [03 · Переменные и области видимости](../03-scopes-and-hoisting/overview.ru.md)

## Источники

- [MDN: BigInt](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/BigInt)
- [MDN: Symbol](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Symbol)
- [MDN: Object.is()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/is)
- [MDN: JSON.stringify()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/JSON/stringify)
- [MDN: Equality comparisons and sameness](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Equality_comparisons_and_sameness)
