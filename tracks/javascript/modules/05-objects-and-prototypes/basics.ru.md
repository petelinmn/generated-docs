---
title: "Объекты и прототипы: базовая теория"
lang: ru
track: javascript
kind: module
doc: basics
slug: 05-objects-and-prototypes
part: 1
tags: [objects, literals, property-access, destructuring, spread, object-keys, prototype, object-create]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Объекты и прототипы: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Типы и значения](../02-types-and-values/overview.ru.md), [04 · Функции](../04-functions/overview.ru.md) · **Время:** ~2,5 ч

Все примеры выполнены на Node.js 24.15 в файле ES-модуля (строгий режим).

## Зачем это нужно

Почти всё в JavaScript, кроме примитивов, — объект: массивы, функции, даты, настройки, ответы сервера. Умение создавать, читать, копировать и разбирать объекты нужно в каждой программе.

## Литералы

```js
const key = 'dyn';
const o = {
  a: 1,
  'b-c': 2,
  [key + '1']: 3,
  f() { return 1 },
  get g() { return 'getter' },
  set s(v) { this._s = v },
};
o.s = 9;
console.log(o['b-c'], o.g, Object.keys(o));
const x = 1, y = 2;
console.log({ x, y });
```

```text
2 getter [ 'a', 'b-c', 'dyn1', 'f', 'g', 's', '_s' ]
{ x: 1, y: 2 }
```

- Ключи, которые не являются допустимым именем (`'b-c'`), записывают в кавычках и читают через `o['b-c']`.
- `[key + '1']` — **вычисляемый ключ**: выражение в скобках даёт имя свойства (`dyn1`).
- `{ x, y }` — сокращённая запись для `{ x: x, y: y }`; `f() {}` — сокращённая запись метода.
- `get`/`set` задают свойства-аксессоры. Присваивание `o.s = 9` вызвало сеттер, который создал поле `_s`; `g` и `s` в `console.log(o)` печатаются как `[Getter]` и `[Setter]`.

## Доступ, проверка и удаление

```js
const o = { a: 1 };
console.log('a' in o, 'toString' in o, Object.hasOwn(o, 'toString'), o.nope, delete o.a, 'a' in o);
```

```text
true true false undefined true false
```

- Чтение несуществующего свойства возвращает `undefined` (не ошибку).
- `'toString' in o` — `true`: оператор `in` смотрит и цепочку прототипов; `Object.hasOwn(o, k)` проверяет только собственные свойства.
- `delete o.a` удаляет свойство и возвращает `true`.
- Чтение свойства у `undefined` или `null` — ошибка; безопасно: `o?.a?.b` ([модуль 02](../02-types-and-values/basics.ru.md#--и-)).

## Ключи и их порядок

```js
console.log(Object.keys({ b: 1, 2: 1, a: 1, 1: 1, [Symbol('s')]: 1 }));
const o = {}; o[1] = 'a'; o['1'] = 'b'; o[{}] = 'c'; o[[1, 2]] = 'd';
console.log(Object.keys(o), o[1]);
```

```text
[ '1', '2', 'b', 'a' ]
[ '1', '[object Object]', '1,2' ] b
```

- Ключи всегда строки (или символы): `o[1]` и `o['1']` — одно свойство; объект как ключ превращается в `'[object Object]'`, массив — в `'1,2'`.
- Порядок перечисления: сначала ключи-«индексы» (`'1'`, `'2'`) по возрастанию, затем остальные строки в порядке добавления; символы не попадают в `Object.keys`.

## Перечисление и преобразования

```js
console.log(Object.entries({ a: 1, b: 2 }), Object.fromEntries([['a', 1], ['b', 2]]));
console.log(Object.values({ a: 1, b: 2 }), Object.keys('ab'), Object.entries([7, 8]));
console.log(Object.groupBy([1, 2, 3, 4], n => (n % 2 ? 'odd' : 'even')));
```

```text
[ [ 'a', 1 ], [ 'b', 2 ] ] { a: 1, b: 2 }
[ 1, 2 ] [ '0', '1' ] [ [ '0', 7 ], [ '1', 8 ] ]
[Object: null prototype] { odd: [ 1, 3 ], even: [ 2, 4 ] }
```

- `Object.keys`, `Object.values`, `Object.entries` возвращают массивы собственных перечисляемых свойств; `Object.fromEntries` собирает объект из пар (обратная операция к `entries`).
- Для строк и массивов ключи — индексы (`'0'`, `'1'`).
- `Object.groupBy` группирует элементы по ключу; результат — объект без прототипа (`[Object: null prototype]`).

## Копирование: поверхностное

```js
const orig = { n: { v: 1 }, k: 1 };
const sp = { ...orig, k: 2 };
sp.n.v = 99;
console.log(orig.n.v, orig.k, sp.k);
console.log(Object.assign({}, { a: 1 }, { a: 2, b: 3 }), { ...null, ...undefined, ...'ab', ...[9] });
```

```text
99 1 2
{ a: 2, b: 3 } { '0': 9, '1': 'b' }
```

- `{ ...orig }` и `Object.assign({}, orig)` копируют собственные перечисляемые свойства **одним уровнем**: вложенный объект `n` остался общим (`orig.n.v` стал `99`).
- Поздние источники перезаписывают ранние (`a: 2`).
- `...null` и `...undefined` игнорируются; строка и массив разворачиваются в индексы.

Глубокие копии — в [продвинутой теории](advanced.ru.md#глубокое-копирование).

## Деструктуризация

```js
const { a: aa, b: { c: cc = 5 } = {}, ...restObj } = { a: 1, d: 4, e: 5 };
console.log(aa, cc, restObj);
const [p0, , p2 = 'dflt', ...pr] = [1, 2, undefined, 4, 5];
console.log(p0, p2, pr);
function fn({ x = 1, y } = {}) { return [x, y]; }
console.log(fn(), fn({ y: 2 }));
let sa = 1, sb = 2; [sa, sb] = [sb, sa];
console.log(sa, sb);
try { const { z } = null; } catch (e) { console.log(e.name + ': ' + e.message); }
```

```text
1 5 { d: 4, e: 5 }
1 dflt [ 4, 5 ]
[ 1, undefined ] [ 1, 2 ]
2 1
TypeError: Cannot destructure property 'z' of 'null' as it is null.
```

- `{ a: aa }` читает поле `a` в переменную `aa`; вложенный шаблон `b: { c: cc = 5 } = {}` достаёт `c` и подставляет умолчания, если значения нет.
- `...restObj` собирает остальные поля в новый объект; `...pr` — остаток массива.
- Пропуск элемента — запятая (`[p0, , p2]`); значение по умолчанию срабатывает на `undefined`.
- Параметр можно деструктурировать прямо в сигнатуре; `= {}` делает аргумент необязательным.
- Обмен переменных: `[sa, sb] = [sb, sa]`.
- Деструктуризация `null`/`undefined` — `TypeError`.

## Прототипы: основы

У каждого объекта есть внутренняя ссылка на прототип. Свойство ищется у объекта, затем у прототипа, затем у прототипа прототипа — до `null`:

```js
const proto = { hello() { return 'hi ' + this.name } };
const child = Object.create(proto);
child.name = 'c';
console.log(child.hello(), Object.getPrototypeOf(child) === proto, proto.isPrototypeOf(child));
console.log(Object.keys(child), child.hasOwnProperty('hello'));
const fi = Object.create({ inherited: 1 }); fi.own = 2;
const keys = []; for (const k in fi) keys.push(k);
console.log(keys, Object.keys(fi));
```

```text
hi c true true
[ 'name' ] false
[ 'own', 'inherited' ] [ 'own' ]
```

- `Object.create(proto)` создаёт объект с заданным прототипом; `hello` найден у прототипа, но не является собственным свойством `child`.
- `for...in` перечисляет и унаследованные перечисляемые свойства, `Object.keys` — только собственные. Чаще нужен `Object.keys`/`entries`.
- Запись всегда создаёт собственное свойство, не изменяя прототип:

```js
const base = { v: 1 }; const der = Object.create(base);
der.v = 2; console.log(base.v, der.v);
delete der.v; console.log(der.v);
```

```text
1 2
1
```

После `delete der.v` собственное свойство исчезло, и снова виден прототипный `v`. Подробности и подводные камни — в [продвинутой теории](advanced.ru.md#ловушки-прототипов).

## Ошибки новичка

- ❌ `{ ...o }` как глубокая копия → ✅ `structuredClone(o)`.
- ❌ `'x' in o` как проверка собственного поля → ✅ `Object.hasOwn(o, 'x')`.
- ❌ Читать вложенное поле у `undefined` → ✅ `o?.a?.b`.
- ❌ Использовать объект как ключ объекта → ✅ `Map`.
- ❌ Деструктурировать значение, которое может быть `null`, без значения по умолчанию → ✅ `= {}`.
- ❌ Ждать порядка добавления для числовых ключей → ✅ числовые ключи идут по возрастанию.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — дескрипторы, `freeze`, глубокое копирование, ловушки прототипов
- Ранее: [04 · Функции](../04-functions/overview.ru.md) (конструкторы и `new`)
- Классы поверх прототипов: модуль 06 дорожной карты [трека](../../overview.ru.md)

## Источники

- [MDN: Working with objects](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Working_with_objects)
- [MDN: Inheritance and the prototype chain](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Inheritance_and_the_prototype_chain)
- [MDN: Destructuring](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Destructuring)
- [MDN: Spread syntax](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Spread_syntax)
- [MDN: Object](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object)
