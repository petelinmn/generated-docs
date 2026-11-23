---
title: "Метапрограммирование: основы"
lang: ru
track: javascript
kind: module
doc: basics
slug: 18-metaprogramming
part: 3
tags: [symbol, property-descriptor, proxy, reflect, tagged-template, string-raw]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Метапрограммирование: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [05 · Объекты и прототипы](../05-objects-and-prototypes/overview.ru.md), [06 · Классы](../06-classes/overview.ru.md) · **Время:** ~3 ч

Все примеры запускались на Node.js 24.15 в файле ES-модуля (код модуля выполняется в строгом режиме).

## Зачем это нужно

Это инструменты, на которых построены фреймворки: реактивность (Vue), валидация, ORM-сущности, шаблонизаторы. Понимание помогает читать их код и не «ломать» их неожиданным поведением.

## Символы

`Symbol` — примитив, значение которого **уникально**:

```js
const s1 = Symbol('desc'), s2 = Symbol('desc');
console.log(s1 === s2, s1.toString(), s1.description, typeof s1, Symbol.for('k') === Symbol.for('k'), Symbol.keyFor(Symbol.for('k')), Symbol.keyFor(s1));
const o = { [s1]: 1, regular: 2 };
console.log(Object.keys(o), JSON.stringify(o), Object.getOwnPropertySymbols(o).length, Reflect.ownKeys(o).length, s1 in o);
try { s1 + '' } catch (e) { console.log(e.name + ': ' + e.message) } console.log(String(s1), `${s1.description}`);
```

```text
false Symbol(desc) desc symbol true k undefined
[ 'regular' ] {"regular":2} 1 2 true
TypeError: Cannot convert a Symbol value to a string
Symbol(desc) desc
```

- Два `Symbol('desc')` **различны**: описание — только подпись для отладки.
- `Symbol.for(ключ)` берёт символ из глобального реестра (одинаков в разных местах), `Symbol.keyFor` возвращает ключ; у обычного символа — `undefined`.
- Символ как ключ свойства **не попадает** в `Object.keys`, `for...in` и `JSON.stringify`, но виден через `Object.getOwnPropertySymbols` и `Reflect.ownKeys`.
- Неявное преобразование в строку запрещено (`TypeError`): нужен `String(s)` или `s.description`.
- Символы удобны для «скрытых» ключей и для расширения объектов без конфликтов имён.

## Дескрипторы свойств

У каждого свойства есть дескриптор: `value`, `writable`, `enumerable`, `configurable` или пара `get`/`set`:

```js
const d = {};
Object.defineProperty(d, 'hidden', { value: 1, enumerable: false });
console.log(Object.keys(d), d.hidden, Object.getOwnPropertyDescriptor(d, 'hidden'));
const withGetter = { get now() { return 'computed' } };
console.log(Object.getOwnPropertyDescriptor(withGetter, 'now').get.name, Object.entries(Object.getOwnPropertyDescriptors(withGetter)).length);
```

```text
[] 1 { value: 1, writable: false, enumerable: false, configurable: false }
get now 1
```

- `defineProperty` по умолчанию создаёт свойство **неизменяемым, неперечисляемым и неконфигурируемым** (все флаги `false`): оно скрыто от `Object.keys`, но читается.
- Геттер и сеттер хранятся в дескрипторе (`get`, `set`); `Object.getOwnPropertyDescriptors` отдаёт дескрипторы всех свойств (см. [модуль 05](../05-objects-and-prototypes/overview.ru.md)).

## Proxy: перехват операций

`Proxy` оборачивает **цель** (target) и **обработчик** (handler) с ловушками (traps):

```js
const target = { a: 1, b: 2 };
const log = [];
const p = new Proxy(target, {
  get(t, k, r) { log.push('get ' + String(k)); return Reflect.get(t, k, r); },
  set(t, k, v, r) { log.push('set ' + k); return Reflect.set(t, k, v, r); },
  has(t, k) { log.push('has ' + k); return k in t; },
  deleteProperty(t, k) { log.push('delete ' + k); return Reflect.deleteProperty(t, k); },
  ownKeys(t) { log.push('ownKeys'); return Reflect.ownKeys(t); },
});
p.a; p.c = 3; 'a' in p; delete p.b; Object.keys(p); JSON.stringify(p); for (const k in p) { }
console.log(log);
```

```text
[
  'get a',   'set c',
  'has a',   'delete b',
  'ownKeys', 'get toJSON',
  'ownKeys', 'get a',
  'get c',   'ownKeys'
]
```

Каждая операция вызвала свою ловушку: `p.a` — `get`, `p.c = 3` — `set`, `'a' in p` — `has`, `delete` — `deleteProperty`, `Object.keys` — `ownKeys`. `JSON.stringify` последовательно запросил `toJSON`, ключи, значения `a` и `c`, а `for...in` — `ownKeys`.

**Проверка значений при записи:**

```js
const validated = new Proxy({}, { set(t, k, v) { if (k === 'age' && !Number.isInteger(v)) throw new TypeError('age must be an integer'); t[k] = v; return true; } });
validated.age = 5; try { validated.age = 'x' } catch (e) { console.log(e.message) } console.log(validated.age);
```

```text
age must be an integer
5
```

Ловушка `set` **должна вернуть `true`**, если запись разрешена. Если вернуть `false`, в строгом режиме будет `TypeError: 'set' on proxy: trap returned falsish for property 'x'` (в опыте; в ES-модулях код всегда строгий); в нестрогом запись молча игнорируется (в опыте в CommonJS `'y' in ro` — `false`).

## Reflect

`Reflect` — набор функций с теми же именами, что и ловушки; они выполняют **обычную** операцию:

```js
console.log(Reflect.ownKeys({ b: 1, [Symbol('s')]: 2, 1: 0 }), Reflect.has({ a: 1 }, 'a'), Reflect.getPrototypeOf([]) === Array.prototype, Reflect.defineProperty({}, 'x', { value: 1 }), Reflect.construct(Date, [0]).getTime(), Reflect.apply(Math.max, null, [1, 3, 2]));
console.log(Reflect.set(Object.freeze({ a: 1 }), 'a', 2), Reflect.deleteProperty(Object.freeze({ a: 1 }), 'a'));
```

```text
[ '1', 'b', Symbol(s) ] true true true 0 3
false false
```

- `Reflect.ownKeys` возвращает **все** собственные ключи: сначала целочисленные (`'1'`), затем строки, затем символы.
- В отличие от операторов, `Reflect.set` и `Reflect.deleteProperty` **возвращают `false`** вместо исключения, если операция не удалась (замороженный объект).
- В ловушках используйте `Reflect.*` для действия по умолчанию: так сохраняется правильный `receiver` и поведение геттеров.

## Теговые шаблонные строки

Функция перед шаблонной строкой получает **части** и **значения** отдельно:

```js
function tag(strings, ...vals) { return { strings: [...strings], raw: strings.raw, vals, isArr: Array.isArray(strings), frozen: Object.isFrozen(strings) }; }
console.log(tag`a${1}b${2}c\n`);
const html = (s, ...v) => s.reduce((acc, str, i) => acc + str + (i < v.length ? String(v[i]).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[c]) : ''), '');
console.log(html`<p>${'<script>alert(1)</script>'}</p>`);
console.log(String.raw`C:\new\table${1}`);
```

```text
{
  strings: [ 'a', 'b', 'c\n' ],
  raw: [ 'a', 'b', 'c\\n' ],
  vals: [ 1, 2 ],
  isArr: true,
  frozen: true
}
<p>&lt;script&gt;alert(1)&lt;/script&gt;</p>
C:\new\table1
```

- `strings` — массив неизменяемых строковых частей, `strings.raw` — те же части **без обработки escape-последовательностей** (`'c\\n'`), `vals` — подставленные значения.
- Тег-функция может вернуть **что угодно**, не только строку: так делают шаблонизаторы, запросы к БД с параметрами, локализацию.
- `html` выше экранирует подставленные значения: `<script>` превратился в текст. Так теговые шаблоны защищают от инъекций.
- `String.raw` — встроенный тег: `\n` остаётся двумя символами (удобно для путей и регулярных выражений).

## Ошибки новичков

- ❌ Считать два `Symbol('x')` одним значением → ✅ для общего символа `Symbol.for`.
- ❌ Склеивать символ со строкой → ✅ `String(s)` или `s.description`.
- ❌ Ловушка `set` без `return true` → ✅ верните `true` (в строгом режиме иначе `TypeError`).
- ❌ Действие по умолчанию через прямой доступ `t[k]` → ✅ `Reflect.get/set` с `receiver`.
- ❌ Ожидать `Object.keys` символьные ключи → ✅ `Object.getOwnPropertySymbols` или `Reflect.ownKeys`.
- ❌ Подставлять пользовательские данные в шаблон без тега → ✅ тег с экранированием.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — известные символы, инварианты, реактивность, слабые ссылки
- Ранее: [05 · Объекты и прототипы](../05-objects-and-prototypes/advanced.ru.md)

## Источники

- [MDN: Symbol](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Symbol)
- [MDN: Object.defineProperty](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/defineProperty)
- [MDN: Proxy](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Proxy)
- [MDN: Reflect](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Reflect)
- [MDN: Template literals](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Template_literals#tagged_templates)
