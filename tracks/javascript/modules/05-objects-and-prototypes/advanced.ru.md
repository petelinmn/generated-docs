---
title: "Объекты и прототипы: продвинутая теория"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 05-objects-and-prototypes
part: 1
tags: [property-descriptors, define-property, freeze, seal, structured-clone, deep-copy, null-prototype, symbols, prototype-pitfalls]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Объекты и прототипы: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры выполнены на Node.js 24.15 в файле ES-модуля (строгий режим).

## Дескрипторы свойств

У каждого свойства есть атрибуты. Для обычного свойства все они `true`; у аксессора вместо `value` и `writable` — `get` и `set`:

```js
console.log(Object.getOwnPropertyDescriptor({ a: 1 }, 'a'));
console.log(Object.getOwnPropertyDescriptor({ get g() { return 1 } }, 'g'));
const d = {};
Object.defineProperty(d, 'ro', { value: 1, enumerable: false });
console.log(Object.getOwnPropertyDescriptor(d, 'ro'), Object.keys(d), JSON.stringify(d));
try { d.ro = 2; } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
{ value: 1, writable: true, enumerable: true, configurable: true }
{ get: [Function: get g], set: undefined, enumerable: true, configurable: true }
{ value: 1, writable: false, enumerable: false, configurable: false } [] {}
TypeError: Cannot assign to read only property 'ro' of object '#<Object>'
```

- `writable` — можно ли менять значение; `enumerable` — виден ли в `keys`, `for...in`, `JSON`; `configurable` — можно ли удалить или переопределить свойство.
- `Object.defineProperty` по умолчанию создаёт атрибуты `false`: свойство `ro` не записывается, не перечисляется и не настраивается.
- Скрытые (неперечисляемые) свойства не попадают в `Object.keys` и в `JSON.stringify`.

## freeze, seal и preventExtensions

Три уровня защиты объекта; в строгом режиме нарушение — `TypeError`:

```js
const fz = Object.freeze({ a: 1 });
const sl = Object.seal({ a: 1 });
const pe = Object.preventExtensions({ a: 1 });
const t = (f) => { try { f() } catch (e) { console.log(e.name + ': ' + e.message) } };
t(() => { fz.a = 2 }); t(() => { sl.b = 1 }); sl.a = 5; t(() => { delete sl.a }); t(() => { pe.b = 1 }); delete pe.a;
console.log(fz, sl, pe, Object.isFrozen(fz), Object.isSealed(sl), Object.isExtensible(pe));
```

```text
TypeError: Cannot assign to read only property 'a' of object '#<Object>'
TypeError: Cannot add property b, object is not extensible
TypeError: Cannot delete property 'a' of #<Object>
TypeError: Cannot add property b, object is not extensible
{ a: 1 } { a: 5 } {} true true false
```

| Метод | Добавлять | Удалять | Менять значения |
|---|---|---|---|
| `preventExtensions` | нет | да | да |
| `seal` | нет | нет | да |
| `freeze` | нет | нет | нет |

Все три действуют на верхний уровень: вложенные объекты остаются изменяемыми ([модуль 03](../03-scopes-and-hoisting/basics.ru.md#let-const-и-var)). В нестрогом режиме нарушение молча игнорируется.

## Глубокое копирование

Поверхностная копия оставляет общие вложенные объекты ([базовая теория](basics.ru.md#копирование-поверхностное)). Для глубокой копии есть `structuredClone`:

```js
const sc = structuredClone({ d: new Date(0), m: new Map([[1, 2]]), s: new Set([1]) });
console.log(sc);
const cyc = {}; cyc.self = cyc;
console.log(structuredClone(cyc).self !== undefined);
try { structuredClone({ f() {} }) } catch (e) { console.log(e.name + ': ' + e.message) }
console.log(JSON.parse(JSON.stringify({ d: new Date(0), u: undefined, m: new Map([[1, 2]]) })));
try { JSON.stringify(cyc) } catch (e) { console.log(e.name + ': ' + e.message.split('\n')[0]) }
```

```text
{ d: 1970-01-01T00:00:00.000Z, m: Map(1) { 1 => 2 }, s: Set(1) { 1 } }
true
DataCloneError: f() {} could not be cloned.
{ d: '1970-01-01T00:00:00.000Z', m: {} }
TypeError: Converting circular structure to JSON
```

- `structuredClone` копирует `Date`, `Map`, `Set` и циклические ссылки, но не функции (`DataCloneError`).
- Клонирование через `JSON.parse(JSON.stringify(x))` ненадёжно: дата превращается в строку, `Map` — в `{}`, `undefined` пропадает, а циклические ссылки вызывают ошибку.
- Прототипы и методы классов `structuredClone` не сохраняет: копия экземпляра класса — обычный объект (в опыте `Object.getPrototypeOf(копия) === Object.prototype`, а `typeof копия.m` — `'undefined'`; приватные поля тоже не переносятся).
- Умеет копировать `Error`, `RegExp` и типизированные массивы (в опыте `Error` остаётся `Error`, `/a/g` и `Uint8Array` сохраняются).

## Ловушки прототипов

**Заморозка прототипа блокирует запись у потомков:**

```js
const base2 = Object.freeze({ ro: 1 });
const der2 = Object.create(base2);
try { der2.ro = 2 } catch (e) { console.log(e.name + ': ' + e.message) }
console.log(der2.ro);
```

```text
TypeError: Cannot assign to read only property 'ro' of object '#<Object>'
1
```

Если свойство прототипа не записываемо, присваивание у потомка (в строгом режиме) тоже падает, а не создаёт собственное.

**Изменяемые данные в прототипе общие:**

```js
const sh = { list: [] };
const a1 = Object.create(sh), a2 = Object.create(sh);
a1.list.push(1);
console.log(a2.list);
```

```text
[ 1 ]
```

`a1.list` — это `sh.list`: `push` изменил общий массив. Изменяемое состояние должно создаваться в самом объекте (в конструкторе).

## Объекты без прототипа

`Object.create(null)` создаёт «чистый» объект без унаследованных свойств:

```js
const dict = Object.create(null);
dict.k = 1;
console.log('toString' in dict, dict instanceof Object, Object.getPrototypeOf(Object.prototype));
try { String(dict) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
false false null
TypeError: Cannot convert object to primitive value
```

Такой объект безопасен как словарь (нет коллизий с `toString`, `__proto__`), но у него нет методов `Object.prototype`: его нельзя превратить в строку. `Object.groupBy` возвращает именно такой объект. Для словарей чаще берут `Map`.

## `__proto__` и `setPrototypeOf`

```js
const pr = { a: 1 };
const ch = { __proto__: pr, b: 2 };
const x = {}; Object.setPrototypeOf(x, pr);
console.log(ch.a, Object.getPrototypeOf(ch) === pr, '__proto__' in {}, Object.keys(JSON.parse('{"__proto__": 1}')), x.a);
```

```text
1 true true [ '__proto__' ]
1
```

- В литерале `{ __proto__: pr }` задаёт прототип; `JSON.parse` создаёт **собственное** свойство `__proto__` (безопасно).
- `Object.setPrototypeOf` меняет прототип существующего объекта; по документации MDN это дорогая операция, поэтому при создании предпочитайте `Object.create` (замедление в опытах не измерялось).

## Символы как ключи

```js
const sym = Symbol('hid');
const so = { [sym]: 1, vis: 2 };
console.log(Object.keys(so), JSON.stringify(so), Object.getOwnPropertySymbols(so).length, so[sym]);
```

```text
[ 'vis' ] {"vis":2} 1 1
```

Символьные ключи не попадают в `keys` и `JSON` — их используют для служебных свойств, которые не должны пересекаться с чужими ([модуль 02](../02-types-and-values/advanced.ru.md#symbol)).

## Лучшие практики

- `Object.hasOwn` вместо `hasOwnProperty`; `Object.keys`/`entries` вместо `for...in`.
- Не расширяйте `Object.prototype` и чужие прототипы.
- Копирование: `{ ...o }` для плоских объектов, `structuredClone` для вложенных данных.
- Неизменяемые данные: `Object.freeze` для констант верхнего уровня (знайте, что он поверхностный).
- Словари: `Map` или `Object.create(null)`; никогда не записывайте ключи из пользовательского ввода в обычный объект без проверки.
- Изменяемое состояние — в экземпляре, не в прототипе.

> ⚠️ Проверить: prototype pollution (запись в `Object.prototype` через вредоносные ключи) подробно разбирается в модуле 20 дорожной карты и в опытах не запускалась.

## Частые ошибки

- ❌ Клонировать через `JSON.parse(JSON.stringify(x))` объекты с датами, `Map`, `undefined` → ✅ `structuredClone`.
- ❌ Ждать, что `freeze` заморозит вложенные объекты → ✅ замораживайте рекурсивно или проектируйте плоские данные.
- ❌ Класть массив или объект в прототип → ✅ создавать в конструкторе.
- ❌ `String(Object.create(null))` → ✅ `Map` или обычный объект.
- ❌ Присваивать в потомке свойство, заданное в замороженном прототипе → ✅ `Object.defineProperty` у потомка.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Конструкторы, `new` и `prototype`: [04 · Функции](../04-functions/advanced.ru.md#функции-конструкторы-и-new)
- Классы: модуль 06 дорожной карты [трека](../../overview.ru.md)

## Источники

- [MDN: Object.defineProperty()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/defineProperty)
- [MDN: Object.freeze()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/freeze)
- [MDN: structuredClone()](https://developer.mozilla.org/en-US/docs/Web/API/Window/structuredClone)
- [MDN: Object.create()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/create)
- [MDN: Object.groupBy()](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/groupBy)
