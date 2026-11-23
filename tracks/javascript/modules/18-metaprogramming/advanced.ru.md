---
title: "Метапрограммирование: продвинутый уровень"
lang: ru
track: javascript
kind: module
doc: advanced
slug: 18-metaprogramming
part: 3
tags: [proxy-invariants, revocable-proxy, well-known-symbols, to-primitive, symbol-species, reactivity, weak-ref, weak-map, function-constructor]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Метапрограммирование: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры запускались на Node.js 24.15 (ES-модуль).

## Известные символы (well-known symbols)

Встроенные символы позволяют настроить поведение языка для своих объектов:

```js
class Temp {
  constructor(c) { this.c = c }
  [Symbol.toPrimitive](hint) { return hint === 'string' ? `${this.c}°C` : this.c }
  get [Symbol.toStringTag]() { return 'Temp' }
  static [Symbol.hasInstance](x) { return typeof x === 'object' && x !== null && 'c' in x }
}
const t = new Temp(20);
console.log(`${t}`, +t, t + 1, Object.prototype.toString.call(t), ({ c: 1 }) instanceof Temp);
```

```text
20°C 20 21 [object Temp] true
```

| Символ | Что настраивает |
|---|---|
| `Symbol.iterator` | итерацию (`for...of`, spread) — [модуль 11](../11-iterators-and-generators/basics.ru.md) |
| `Symbol.toPrimitive` | приведение к примитиву; `hint` — `'string'`, `'number'` или `'default'` |
| `Symbol.toStringTag` | текст в `Object.prototype.toString` (`[object Temp]`) |
| `Symbol.hasInstance` | поведение `instanceof` (здесь любой объект с полем `c` — «экземпляр») |
| `Symbol.isConcatSpreadable` | раскрывается ли объект в `concat` |
| `Symbol.species` | какой конструктор используют методы, возвращающие новый объект |
| `Symbol.match`, `Symbol.split` и др. | поведение методов строк с «не регулярным» аргументом |
| `Symbol.unscopables` | какие имена исключаются из `with` |

```js
const spec = { [Symbol.isConcatSpreadable]: true, length: 2, 0: 'a', 1: 'b' }; console.log([1].concat(spec));
class MyArr extends Array { static get [Symbol.species]() { return Array } }
const ma = new MyArr(1, 2, 3); console.log(ma instanceof MyArr, ma.map(x => x) instanceof MyArr, ma.map(x => x) instanceof Array);
console.log('abc'.match({ [Symbol.match](s) { return ['custom', s] } }), 'a-b'.split({ [Symbol.split](s) { return ['split!'] } }));
```

```text
[ 1, 'a', 'b' ]
true false true
[ 'custom', 'abc' ] [ 'split!' ]
```

- С `Symbol.isConcatSpreadable` объект «похож на массив» для `concat`.
- `Symbol.species` вернул из `map` обычный `Array`, а не `MyArr`: так наследники управляют типом результата ([модуль 07](../07-arrays-and-collections/overview.ru.md)).
- Методы строк делегируют работу объекту с `Symbol.match`/`Symbol.split` — на этом построены регулярные выражения.

## Ограничения и инварианты прокси

Прокси не может «солгать» о свойстве, которое объект гарантирует неизменным:

```js
const frozen = Object.freeze({ x: 1 }); const lie = new Proxy(frozen, { get() { return 2 } });
try { lie.x } catch (e) { console.log(e.name + ': ' + e.message.slice(0, 60)) }
```

```text
TypeError: 'get' on proxy: property 'x' is a read-only and non-configur
```

Ловушка `get` вернула `2`, но у замороженного объекта `x` равно `1`: движок проверяет **инварианты** и бросает `TypeError`. Подобные проверки есть у `has`, `ownKeys`, `getPrototypeOf` и др.

**Что ломается через прокси:**

```js
class C { #x = 1; get x() { return this.#x } }
const cp = new Proxy(new C(), {}); try { cp.x } catch (e) { console.log(e.name + ': ' + e.message) }
const cp2 = new Proxy(new C(), { get(t, k) { const v = Reflect.get(t, k, t); return typeof v === 'function' ? v.bind(t) : v } }); console.log(cp2.x);
try { new Proxy(new Map(), {}).set('a', 1) } catch (e) { console.log(e.name + ': ' + e.message) }
```

```text
TypeError: Cannot read private member #x from an object whose class did not declare it
1
TypeError: Method Map.prototype.set called on incompatible receiver #<Map>
```

- Приватные поля и **внутренние слоты** (у `Map`, `Set`, `Date`, `Promise`) привязаны к настоящему объекту, а не к прокси: геттер с `this` = прокси не видит `#x`.
- Обход: в ловушке `get` выполнять `Reflect.get(t, k, t)` (receiver — цель) и привязывать методы `v.bind(t)`.
- Прокси **не равен** цели: `new Proxy(target, {}) === target` — `false`; зато `Array.isArray(proxyOfArray)` — `true`, а `typeof` прокси функции — `function`.

## Ловушки функций и массивов

```js
const traced = new Proxy(function add(a, b) { return a + b }, {
  apply(t, thisArg, args) { return 'called with ' + args.join(',') + ' => ' + Reflect.apply(t, thisArg, args) },
  construct(t, args) { return { constructed: args } },
});
console.log(traced(1, 2), new traced(9));
const arr = new Proxy([10, 20, 30], { get(t, k, r) { const i = Number(k); return Number.isInteger(i) && i < 0 ? t[t.length + i] : Reflect.get(t, k, r); } });
console.log(arr[-1], arr[0], arr.length, Array.isArray(arr));
const def = new Proxy({}, { get: (t, k) => k in t ? t[k] : `no ${String(k)}` }); console.log(def.x);
```

```text
called with 1,2 => 3 { constructed: [ 9 ] }
30 10 3 true
no x
```

- `apply` и `construct` перехватывают вызов функции и `new`.
- Отрицательные индексы массива и значения по умолчанию — типовые применения `get`.

## Отключаемый прокси и receiver

```js
const { proxy, revoke } = Proxy.revocable({ a: 1 }, {}); console.log(proxy.a); revoke();
try { proxy.a } catch (e) { console.log(e.name + ': ' + e.message) }
const base = { get who() { return this.name }, name: 'base' }; const child = Object.create(base); child.name = 'child';
console.log(Reflect.get(base, 'who', child), base.who, Reflect.get(base, 'who'));
```

```text
1
TypeError: Cannot perform 'get' on a proxy that has been revoked
child base base
```

- `Proxy.revocable` позволяет **отозвать** доступ: после `revoke()` любая операция — `TypeError` (полезно для выдачи временного доступа).
- Третий аргумент `Reflect.get` — **receiver**: значение `this` для геттера (`'child'`). Поэтому в ловушках передают `receiver`, а не цель.

## Реактивность на Proxy

```js
function reactive(obj, onChange) { return new Proxy(obj, { set(t, k, v, r) { const old = t[k]; const ok = Reflect.set(t, k, v, r); if (old !== v) onChange(k, old, v); return ok; } }); }
const state = reactive({ n: 0 }, (k, o, v) => console.log('changed', k, o, '->', v));
state.n = 1; state.n = 1; state.n++;
```

```text
changed n 0 -> 1
changed n 1 -> 2
```

Повторная запись того же значения изменение не вызвала. На этой идее основана реактивность во Vue 3; в реальных библиотеках добавляют отслеживание зависимостей, вложенные объекты и защиту от циклов.

## Слабые ссылки

```js
const wr = new WeakRef({ big: 1 }); console.log(typeof wr.deref(), wr.deref().big, typeof FinalizationRegistry);
const ws = new WeakSet(); const key = {}; ws.add(key); console.log(ws.has(key), ws.has({}));
try { new WeakMap().set(1, 1) } catch (e) { console.log(e.name + ': ' + e.message) }
const wm2 = new WeakMap(); const sk = Symbol('local'); wm2.set(sk, 'sym-key'); console.log(wm2.get(sk)); try { wm2.set(Symbol.for('reg'), 1) } catch (e) { console.log(e.name) }
```

```text
object 1 function
true false
TypeError: Invalid value used as weak map key
sym-key
TypeError
```

- `WeakMap`/`WeakSet` не удерживают ключи и **не перебираются** (нет `size`, `keys`): когда ключ-объект недостижим, запись может исчезнуть.
- Ключом может быть объект или **не зарегистрированный** символ; число — `TypeError`; символ из реестра (`Symbol.for`) — тоже `TypeError`.
- `WeakRef.deref()` возвращает объект или `undefined`, если он уже собран; `FinalizationRegistry` вызывает колбэк после сборки.
- Момент сборки мусора **недетерминирован**: нельзя строить логику на срабатывании колбэка.

> ⚠️ Проверить: фактическое освобождение объектов и вызов `FinalizationRegistry` не запускались (требуют давления на память и зависят от движка).

## Функции, `new.target`, `eval`

```js
function F() { return new.target ? 'new' : 'call' } console.log(new F() instanceof F, F());
console.log((function foo(a, b) { return a + b }).toString(), (x => x * 2).toString());
console.log(typeof Function('a', 'b', 'return a + b'), Function('a', 'b', 'return a + b')(1, 2), eval('1 + 2'), (0, eval)('typeof module'));
```

```text
true call
function foo(a, b) { return a + b } x => x * 2
function 3 3 undefined
```

- `new.target` показывает, вызвали ли функцию через `new`.
- `Function.prototype.toString()` возвращает **исходный текст** функции.
- `Function(...)` и `eval(...)` создают код из строки: прямой `eval` видит локальные переменные, непрямой `(0, eval)(...)` — только глобальную область. **Не выполняйте строки из недоверенных источников**: это выполнение произвольного кода ([модуль 20](../../overview.ru.md)).

## Лучшие практики

- Прокси — для библиотек (валидация, реактивность, ORM); в приложении — реже.
- В ловушках выполняйте действие через `Reflect.*` с `receiver`.
- Помните об инвариантах и слотах: тестируйте прокси на реальных типах (`Map`, классы с `#`).
- Известные символы настраивайте только когда это действительно нужно: поведение неочевидно.
- Не строить логику на слабых ссылках и сборке мусора.
- Не вычисляйте код из строк (`eval`, `Function`).

## Частые ошибки

- ❌ Ловушка `get` возвращает не то, что нужно для замороженного свойства → ✅ соблюдать инварианты.
- ❌ Прокси вокруг `Map`/`Date` → ✅ привязывать методы к цели.
- ❌ Ожидать `proxy === target` → ✅ сравнивать цели отдельно.
- ❌ `WeakMap` с примитивным ключом → ✅ объект или локальный символ.
- ❌ Полагаться на `WeakRef.deref()` как на кэш → ✅ всегда проверять `undefined`.
- ❌ `eval` на пользовательской строке → ✅ парсер или безопасный интерпретатор.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Итерация и `Symbol.iterator`: [11 · Итераторы и генераторы](../11-iterators-and-generators/advanced.ru.md)
- Приватные поля: [06 · Классы](../06-classes/advanced.ru.md)
- Отражение в C#: [20 · Отражение и генераторы кода](../../../csharp/modules/20-reflection-and-source-generators/overview.ru.md)

## Источники

- [MDN: Proxy — инварианты](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Proxy/Proxy#handler_functions)
- [MDN: Symbol — известные символы](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Symbol#well-known_symbols)
- [MDN: WeakRef](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/WeakRef)
- [MDN: WeakMap](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/WeakMap)
- [MDN: FinalizationRegistry](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/FinalizationRegistry)
- [MDN: new.target](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/new.target)
- [ECMAScript: Proxy Object Internal Methods](https://tc39.es/ecma262/#sec-proxy-object-internal-methods-and-internal-slots)
