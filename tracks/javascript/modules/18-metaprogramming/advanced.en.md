---
title: "Metaprogramming: advanced"
lang: en
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

# Metaprogramming: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15 (an ES module).

## Well-known symbols

Built-in symbols let you customize the language's behavior for your own objects:

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

| Symbol | What it customizes |
|---|---|
| `Symbol.iterator` | iteration (`for...of`, spread) — [module 11](../11-iterators-and-generators/basics.en.md) |
| `Symbol.toPrimitive` | conversion to a primitive; `hint` is `'string'`, `'number'` or `'default'` |
| `Symbol.toStringTag` | the text in `Object.prototype.toString` (`[object Temp]`) |
| `Symbol.hasInstance` | the behavior of `instanceof` (here any object with a field `c` is an "instance") |
| `Symbol.isConcatSpreadable` | whether an object is spread in `concat` |
| `Symbol.species` | which constructor methods that return a new object use |
| `Symbol.match`, `Symbol.split` etc. | the behavior of string methods with a "non-regex" argument |
| `Symbol.unscopables` | which names are excluded from `with` |

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

- With `Symbol.isConcatSpreadable` an object "looks like an array" to `concat`.
- `Symbol.species` made `map` return an ordinary `Array`, not `MyArr`: that is how subclasses control the result type ([module 07](../07-arrays-and-collections/overview.en.md)).
- String methods delegate the work to an object with `Symbol.match`/`Symbol.split` — regular expressions are built on this.

## Proxy limitations and invariants

A proxy cannot "lie" about a property that an object guarantees to be unchangeable:

```js
const frozen = Object.freeze({ x: 1 }); const lie = new Proxy(frozen, { get() { return 2 } });
try { lie.x } catch (e) { console.log(e.name + ': ' + e.message.slice(0, 60)) }
```

```text
TypeError: 'get' on proxy: property 'x' is a read-only and non-configur
```

The `get` trap returned `2`, but in the frozen object `x` is `1`: the engine checks the **invariants** and throws a `TypeError`. Similar checks exist for `has`, `ownKeys`, `getPrototypeOf`, etc.

**What breaks through a proxy:**

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

- Private fields and **internal slots** (of `Map`, `Set`, `Date`, `Promise`) are tied to the real object, not the proxy: a getter with `this` = the proxy does not see `#x`.
- The workaround: in the `get` trap run `Reflect.get(t, k, t)` (the receiver is the target) and bind methods with `v.bind(t)`.
- A proxy is **not equal** to its target: `new Proxy(target, {}) === target` is `false`; but `Array.isArray(proxyOfArray)` is `true`, and `typeof` a function proxy is `function`.

## Function and array traps

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

- `apply` and `construct` intercept a function call and `new`.
- Negative array indexes and default values are typical uses of `get`.

## A revocable proxy and the receiver

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

- `Proxy.revocable` lets you **revoke** access: after `revoke()` any operation is a `TypeError` (useful for granting temporary access).
- The third argument of `Reflect.get` is the **receiver**: the value of `this` for a getter (`'child'`). That is why traps pass `receiver` along, not the target.

## Reactivity on a Proxy

```js
function reactive(obj, onChange) { return new Proxy(obj, { set(t, k, v, r) { const old = t[k]; const ok = Reflect.set(t, k, v, r); if (old !== v) onChange(k, old, v); return ok; } }); }
const state = reactive({ n: 0 }, (k, o, v) => console.log('changed', k, o, '->', v));
state.n = 1; state.n = 1; state.n++;
```

```text
changed n 0 -> 1
changed n 1 -> 2
```

Writing the same value again did not trigger a change. Vue 3's reactivity is based on this idea; real libraries add dependency tracking, nested objects and protection against cycles.

## Weak references

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

- `WeakMap`/`WeakSet` do not hold keys and **cannot be iterated** (no `size`, `keys`): when a key object becomes unreachable, the entry may disappear.
- A key can be an object or a **non-registered** symbol; a number is a `TypeError`; a symbol from the registry (`Symbol.for`) is also a `TypeError`.
- `WeakRef.deref()` returns the object, or `undefined` if it has already been collected; `FinalizationRegistry` calls a callback after collection.
- The moment of garbage collection is **nondeterministic**: do not build logic on a callback firing.

> ⚠️ Verify: actual object release and `FinalizationRegistry` calls were not run (they need memory pressure and depend on the engine).

## Functions, `new.target`, `eval`

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

- `new.target` shows whether the function was called with `new`.
- `Function.prototype.toString()` returns the function's **source text**.
- `Function(...)` and `eval(...)` create code from a string: a direct `eval` sees local variables, an indirect `(0, eval)(...)` only the global scope. **Do not execute strings from untrusted sources**: that is arbitrary code execution ([module 20](../../overview.en.md)).

## Best practices

- Proxies are for libraries (validation, reactivity, ORM); in an application, less often.
- In traps perform the action through `Reflect.*` with the `receiver`.
- Remember invariants and slots: test proxies on real types (`Map`, classes with `#`).
- Customize well-known symbols only when really needed: the behavior is not obvious.
- Do not build logic on weak references and garbage collection.
- Do not evaluate code from strings (`eval`, `Function`).

## Common mistakes

- ❌ A `get` trap returns something wrong for a frozen property → ✅ obey the invariants.
- ❌ A proxy around `Map`/`Date` → ✅ bind methods to the target.
- ❌ Expecting `proxy === target` → ✅ compare targets separately.
- ❌ A `WeakMap` with a primitive key → ✅ an object or a local symbol.
- ❌ Relying on `WeakRef.deref()` as a cache → ✅ always check for `undefined`.
- ❌ `eval` on a user string → ✅ a parser or a safe interpreter.

## Related materials

- Next: [expert theory](expert.en.md)
- Iteration and `Symbol.iterator`: [11 · Iterators and generators](../11-iterators-and-generators/advanced.en.md)
- Private fields: [06 · Classes](../06-classes/advanced.en.md)
- Reflection in C#: [20 · Reflection and source generators](../../../csharp/modules/20-reflection-and-source-generators/overview.en.md)

## Sources

- [MDN: Proxy — invariants](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Proxy/Proxy#handler_functions)
- [MDN: Symbol — well-known symbols](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Symbol#well-known_symbols)
- [MDN: WeakRef](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/WeakRef)
- [MDN: WeakMap](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/WeakMap)
- [MDN: FinalizationRegistry](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/FinalizationRegistry)
- [MDN: new.target](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/new.target)
- [ECMAScript: Proxy Object Internal Methods](https://tc39.es/ecma262/#sec-proxy-object-internal-methods-and-internal-slots)
