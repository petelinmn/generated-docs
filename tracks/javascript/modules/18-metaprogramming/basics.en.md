---
title: "Metaprogramming: basics"
lang: en
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

# Metaprogramming: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [05 · Objects and prototypes](../05-objects-and-prototypes/overview.en.md), [06 · Classes](../06-classes/overview.en.md) · **Time:** ~3 h

All examples were run on Node.js 24.15 in an ES module file (module code runs in strict mode).

## Why it matters

These are the tools frameworks are built on: reactivity (Vue), validation, ORM entities, template engines. Understanding them helps you read their code and not "break" them with unexpected behavior.

## Symbols

`Symbol` is a primitive whose value is **unique**:

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

- Two `Symbol('desc')` are **different**: the description is only a label for debugging.
- `Symbol.for(key)` takes a symbol from the global registry (the same everywhere), `Symbol.keyFor` returns the key; for an ordinary symbol — `undefined`.
- A symbol as a property key **does not appear** in `Object.keys`, `for...in` and `JSON.stringify`, but is visible via `Object.getOwnPropertySymbols` and `Reflect.ownKeys`.
- Implicit conversion to a string is forbidden (`TypeError`): you need `String(s)` or `s.description`.
- Symbols suit "hidden" keys and extending objects without name conflicts.

## Property descriptors

Every property has a descriptor: `value`, `writable`, `enumerable`, `configurable` or a `get`/`set` pair:

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

- By default `defineProperty` creates a property that is **immutable, non-enumerable and non-configurable** (all flags `false`): it is hidden from `Object.keys` but readable.
- A getter and a setter are kept in the descriptor (`get`, `set`); `Object.getOwnPropertyDescriptors` returns the descriptors of all properties (see [module 05](../05-objects-and-prototypes/overview.en.md)).

## Proxy: intercepting operations

`Proxy` wraps a **target** and a **handler** with traps:

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

Each operation called its own trap: `p.a` — `get`, `p.c = 3` — `set`, `'a' in p` — `has`, `delete` — `deleteProperty`, `Object.keys` — `ownKeys`. `JSON.stringify` successively asked for `toJSON`, the keys, the values of `a` and `c`, and `for...in` — `ownKeys`.

**Validating values on write:**

```js
const validated = new Proxy({}, { set(t, k, v) { if (k === 'age' && !Number.isInteger(v)) throw new TypeError('age must be an integer'); t[k] = v; return true; } });
validated.age = 5; try { validated.age = 'x' } catch (e) { console.log(e.message) } console.log(validated.age);
```

```text
age must be an integer
5
```

The `set` trap **must return `true`** if the write is allowed. If it returns `false`, in strict mode there is a `TypeError: 'set' on proxy: trap returned falsish for property 'x'` (in the experiment; ES module code is always strict); in sloppy mode the write is silently ignored (in the experiment in CommonJS `'y' in ro` is `false`).

## Reflect

`Reflect` is a set of functions with the same names as the traps; they perform the **ordinary** operation:

```js
console.log(Reflect.ownKeys({ b: 1, [Symbol('s')]: 2, 1: 0 }), Reflect.has({ a: 1 }, 'a'), Reflect.getPrototypeOf([]) === Array.prototype, Reflect.defineProperty({}, 'x', { value: 1 }), Reflect.construct(Date, [0]).getTime(), Reflect.apply(Math.max, null, [1, 3, 2]));
console.log(Reflect.set(Object.freeze({ a: 1 }), 'a', 2), Reflect.deleteProperty(Object.freeze({ a: 1 }), 'a'));
```

```text
[ '1', 'b', Symbol(s) ] true true true 0 3
false false
```

- `Reflect.ownKeys` returns **all** own keys: integer ones first (`'1'`), then strings, then symbols.
- Unlike operators, `Reflect.set` and `Reflect.deleteProperty` **return `false`** instead of throwing if the operation failed (a frozen object).
- In traps use `Reflect.*` for the default action: this keeps the right `receiver` and getter behavior.

## Tagged template strings

A function before a template string receives the **parts** and the **values** separately:

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

- `strings` is an array of immutable string parts, `strings.raw` the same parts **without processing escape sequences** (`'c\\n'`), `vals` the substituted values.
- A tag function can return **anything**, not only a string: template engines, parameterized database queries and localization are done this way.
- `html` above escapes the substituted values: `<script>` became text. This is how tagged templates protect against injection.
- `String.raw` is a built-in tag: `\n` stays two characters (handy for paths and regular expressions).

## Beginner mistakes

- ❌ Treating two `Symbol('x')` as one value → ✅ `Symbol.for` for a shared symbol.
- ❌ Concatenating a symbol with a string → ✅ `String(s)` or `s.description`.
- ❌ A `set` trap without `return true` → ✅ return `true` (otherwise a `TypeError` in strict mode).
- ❌ The default action via direct access `t[k]` → ✅ `Reflect.get/set` with `receiver`.
- ❌ Expecting `Object.keys` to show symbol keys → ✅ `Object.getOwnPropertySymbols` or `Reflect.ownKeys`.
- ❌ Substituting user data into a template without a tag → ✅ a tag with escaping.

## Related materials

- Next: [advanced theory](advanced.en.md) — well-known symbols, invariants, reactivity, weak references
- Earlier: [05 · Objects and prototypes](../05-objects-and-prototypes/advanced.en.md)

## Sources

- [MDN: Symbol](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Symbol)
- [MDN: Object.defineProperty](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/defineProperty)
- [MDN: Proxy](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Proxy)
- [MDN: Reflect](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Reflect)
- [MDN: Template literals](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Template_literals#tagged_templates)
