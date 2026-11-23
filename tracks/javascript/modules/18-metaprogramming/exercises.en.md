---
title: "Exercises: metaprogramming"
lang: en
track: javascript
kind: module
doc: exercises
slug: 18-metaprogramming
part: 3
tags: [symbol, proxy, reflect, to-primitive, tagged-template]
status: draft
updated: 2026-11-23
---

# Exercises: metaprogramming

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. Run the code in an ES module file (a `package.json` with `{"type": "module"}`).

## 1. 🟢 A hidden identifier

**Task:** add to the object `{ name: 'Ann' }` a hidden field `id` with the value `42` so that it does not appear in `Object.keys` and `JSON.stringify` but can be read by its key.

**Expected result:** `[ 'name' ] {"name":"Ann"} 42 2` (the keys, the JSON, the value, the number of all own keys).

<details><summary>Hint</summary>

A symbol key: `const ID = Symbol('id')`; `{ [ID]: 42 }`; the number of keys — `Reflect.ownKeys`.

</details>

<details><summary>Solution</summary>

```js
const ID = Symbol('id'); const user = { name: 'Ann', [ID]: 42 };
console.log(Object.keys(user), JSON.stringify(user), user[ID], Reflect.ownKeys(user).length);
```

```text
[ 'name' ] {"name":"Ann"} 42 2
```

</details>

## 2. 🟢 A read counter

**Task:** wrap `{ a: 1, b: 2 }` in a `Proxy` that counts how many times each property was read. Read `a` twice and `b` once.

**Expected result:** `{ a: 2, b: 1 }`.

<details><summary>Hint</summary>

The `get` trap and `Reflect.get(t, k, r)`.

</details>

<details><summary>Solution</summary>

```js
const counts = {};
const watched = new Proxy({ a: 1, b: 2 }, { get(t, k, r) { counts[k] = (counts[k] ?? 0) + 1; return Reflect.get(t, k, r); } });
watched.a; watched.a; watched.b; console.log(counts);
```

```text
{ a: 2, b: 1 }
```

</details>

## 3. 🟡 Money with `Symbol.toPrimitive`

**Task:** a class `Money(cents, currency)`: in a string context it gives `19.99 USD`, in a numeric one the number of cents. Check `` `${m}` ``, `m + 1`, `m > 1000` and `String(m)` for `1999` cents.

**Expected result:** `19.99 USD 2000 true 19.99 USD`.

<details><summary>Hint</summary>

The method `[Symbol.toPrimitive](hint)`: when `hint === 'string'` return a string, otherwise a number.

</details>

<details><summary>Solution</summary>

```js
class Money { constructor(c, cur) { this.c = c; this.cur = cur } [Symbol.toPrimitive](hint) { return hint === 'string' ? `${(this.c / 100).toFixed(2)} ${this.cur}` : this.c } }
const m = new Money(1999, 'USD'); console.log(`${m}`, m + 1, m > 1000, String(m));
```

```text
19.99 USD 2000 true 19.99 USD
```

</details>

## 4. 🟡 A read-only view

**Task:** a function `readonly(obj)` returns a proxy that throws `TypeError('read-only')` on writing and deleting a property but reads values.

**Expected result:** `80`, `read-only`, `read-only`.

<details><summary>Hint</summary>

The traps `set` and `deleteProperty`, in which an error must be thrown.

</details>

<details><summary>Solution</summary>

```js
const readonly = obj => new Proxy(obj, { set() { throw new TypeError('read-only') }, deleteProperty() { throw new TypeError('read-only') } });
const cfg = readonly({ port: 80 }); console.log(cfg.port);
try { cfg.port = 1 } catch (e) { console.log(e.message) }
try { delete cfg.port } catch (e) { console.log(e.message) }
```

```text
80
read-only
read-only
```

The view is shallow: nested objects remain mutable.

</details>

## 5. 🔴 A safe SQL query

**Task:** a tag function `sql` turns a template into `{ text, values }`: it puts `$1`, `$2`, … in place of the substitutions and collects the values in an array (protection against SQL injection). Check with `id = 7` and a malicious string.

**Expected result:** `{ text: 'SELECT * FROM users WHERE id = $1 AND name = $2', values: [ 7, "x'; DROP TABLE users;--" ] }`.

<details><summary>Hint</summary>

`strings.reduce((acc, s, i) => acc + '$' + i + s)` — without an initial value `strings[0]` is taken first and the index starts at `1`.

</details>

<details><summary>Solution</summary>

```js
const sql = (strings, ...values) => ({ text: strings.reduce((acc, s, i) => acc + '$' + i + s), values });
const id = 7, name = "x'; DROP TABLE users;--";
console.log(sql`SELECT * FROM users WHERE id = ${id} AND name = ${name}`);
```

```text
{
  text: 'SELECT * FROM users WHERE id = $1 AND name = $2',
  values: [ 7, "x'; DROP TABLE users;--" ]
}
```

The values do not get into the query text: the database driver passes them separately ([module 20](../../overview.en.md)).

</details>
