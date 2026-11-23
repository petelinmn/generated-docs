---
title: "Testing: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 15-testing
part: 2
tags: [node-test, assert, describe, hooks, async-test, subtests, skip, todo]
versions: "Node.js 24.15 LTS (node:test, node:assert)"
status: draft
updated: 2026-11-23
---

# Testing: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [09 · Modules](../09-modules/overview.en.md), [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md), [14 · Errors and debugging](../14-errors-and-debugging/overview.en.md) · **Time:** ~2.5 h

All examples were run on Node.js 24.15 in a folder with a `package.json` (`{"type": "module"}`). The code under test is in `lib/math.js`:

```js
export const add = (a, b) => a + b;
export function divide(a, b) { if (b === 0) throw new RangeError('division by zero'); return a / b; }
export async function fetchUser(id, client) { const r = await client.get(`/users/${id}`); if (!r.ok) throw new Error('not ok'); return r.json(); }
export function untested(x) { return x > 0 ? 'pos' : 'neg'; }
```

## Why it matters

Tests fix the expected behavior and catch regressions when things change. The built-in Node.js runner lets you start without installing packages.

## The first test

The file `test/basic.test.js`:

```js
import { test, describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { add, divide } from '../lib/math.js';

test('add works', () => { assert.equal(add(1, 2), 3); });

describe('divide', () => {
  it('divides', () => { assert.equal(divide(6, 3), 2); });
  it('throws on zero', () => { assert.throws(() => divide(1, 0), { name: 'RangeError', message: 'division by zero' }); });
  it('deep equality', () => { assert.deepEqual({ a: [1, 2] }, { a: [1, 2] }); });
  it.skip('skipped one', () => { assert.fail('never runs'); });
  it.todo('todo one');
});
```

Run: `node --test`.

```text
✔ add works (0.616ms)
▶ divide
  ✔ divides (0.1988ms)
  ✔ throws on zero (0.6042ms)
  ✔ deep equality (0.4495ms)
  ﹣ skipped one (0.0885ms) # SKIP
  ✔ todo one (0.0621ms) # TODO
✔ divide (1.7438ms)
```

- `test(name, fn)` declares a test. A test **passes** if the function finished without an exception and **fails** if it threw an error.
- `describe(name, fn)` groups tests into a suite, `it` is a synonym of `test` inside a suite.
- `it.skip(…)` skips a test (`# SKIP`), `it.todo(…)` marks a planned one (`# TODO`, not counted as a failure).
- The execution times will differ.

## Assertions (assert)

The `node:assert` module throws an `AssertionError` if a condition is not met. Import the **strict** mode — `node:assert/strict`:

| Assertion | Checks |
|---|---|
| `assert.ok(x)` | the value is truthy |
| `assert.equal(a, b)` | strict equality by `Object.is` (in the experiment `assert.strictEqual(NaN, NaN)` passes) |
| `assert.notEqual(a, b)` | strict inequality |
| `assert.deepEqual(a, b)` | deep strict equality of objects, arrays, `Map`, `Set` |
| `assert.throws(fn, expected)` | `fn` throws an error matching `expected` (a class, an object, a regular expression) |
| `assert.rejects(promise, expected)` | the promise is rejected |
| `assert.match(str, re)` | the string matches the regular expression |
| `assert.fail(msg)` | an unconditional failure |

In the plain `node:assert` without `/strict`, `equal` and `deepEqual` compare loosely (`'1'` equals `1`): bugs slip through, so use the strict mode ([expert theory](expert.en.md#reference)).

On failure the runner shows the difference:

```text
✖ fails with diff
  AssertionError [ERR_ASSERTION]: Expected values to be strictly deep-equal:
  + actual - expected

    {
      a: 1,
      b: [
        1,
  +     2
  -     3
      ]
    }
```

## Hooks

Hooks prepare the environment and clean up after:

```js
import { describe, it, before, after, beforeEach, afterEach } from 'node:test';
describe('hooks', () => {
  const order = [];
  before(() => order.push('before'));
  after(() => { order.push('after'); console.log('# hooks order:', order.join(',')); });
  beforeEach(() => order.push('beforeEach'));
  afterEach(() => order.push('afterEach'));
  it('first', () => order.push('first'));
  it('second', () => order.push('second'));
});
```

```text
# hooks order: before,beforeEach,first,afterEach,beforeEach,second,afterEach,after
```

`before` and `after` run once per suite, `beforeEach` and `afterEach` around every test.

## Async tests and subtests

```js
import { test } from 'node:test';
import assert from 'node:assert/strict';
test('async and rejects', async () => {
  await assert.rejects(Promise.reject(new TypeError('bad')), TypeError);
  assert.equal(await Promise.resolve(5), 5);
});
test('subtests', async (t) => {
  await t.test('child 1', () => assert.ok(true));
  await t.test('child 2', () => assert.ok(true));
});
```

```text
✔ async and rejects (0.2662ms)
▶ subtests
  ✔ child 1 (0.1486ms)
  ✔ child 2 (0.0661ms)
✔ subtests (0.5283ms)
```

- An async test is an `async` function; the runner waits for the promise. **Do not forget `await`** before `assert.rejects`: otherwise the test ends before the check.
- Subtests are created with `t.test(...)` and `await`.

## The exit code

`node --test` exits with code `0` if all tests passed and `1` if at least one failed (in the experiment). That is why the command goes into CI and `npm test`:

```json
{ "scripts": { "test": "node --test" } }
```

## Beginner mistakes

- ❌ Forgetting `await` before an async assertion → ✅ `await assert.rejects(…)`.
- ❌ Using the loose `node:assert` → ✅ `node:assert/strict`.
- ❌ Tests that depend on each other and on order → ✅ independent tests and `beforeEach`.
- ❌ Checking the implementation instead of behavior → ✅ input and result.
- ❌ Comparing objects with `equal` → ✅ `deepEqual`.
- ❌ Hitting the network and files in unit tests → ✅ mocks ([advanced theory](advanced.en.md)).

## Related materials

- Next: [advanced theory](advanced.en.md) — mocks, fake timers, coverage, snapshots
- Earlier: [14 · Errors and debugging](../14-errors-and-debugging/basics.en.md)

## Sources

- [Node.js: Test runner](https://nodejs.org/docs/latest-v24.x/api/test.html)
- [Node.js: Assert](https://nodejs.org/docs/latest-v24.x/api/assert.html)
- [Node.js: Running tests from the command line](https://nodejs.org/docs/latest-v24.x/api/test.html#running-tests-from-the-command-line)
