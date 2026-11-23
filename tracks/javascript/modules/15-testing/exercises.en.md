---
title: "Exercises: testing"
lang: en
track: javascript
kind: module
doc: exercises
slug: 15-testing
part: 2
tags: [node-test, assert, mock, fake-timers, hooks]
status: draft
updated: 2026-11-23
---

# Exercises: testing

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. A working folder with a `package.json` (`{"type": "module"}`); the code under test is in `lib/util.js`, the tests in `test/ex.test.js`. Run: `node --test`.

The code under test (`lib/util.js`):

```js
export function debounce(fn, ms) { let t; return (...a) => { clearTimeout(t); t = setTimeout(() => fn(...a), ms); }; }
export async function retry(fn, times) { for (let i = 0; ; i++) { try { return await fn() } catch (e) { if (i >= times) throw e } } }
export function slugify(s) { return s.trim().toLowerCase().replace(/\s+/g, '-'); }
```

All the solutions together (`test/ex.test.js`) gave 9 passing tests.

## 1. 🟢 Tests for `slugify`

**Task:** write three tests: words are lowercased and joined with a hyphen (`'  Hello Big World '` → `'hello-big-world'`), a single word stays a word, and `null` leads to a `TypeError`.

**Expected result:** three green tests of the `slugify` suite.

<details><summary>Hint</summary>

`assert.equal` for values and `assert.throws(() => slugify(null), TypeError)` for the error.

</details>

<details><summary>Solution</summary>

```js
import { test, describe, it, beforeEach, mock } from 'node:test';
import assert from 'node:assert/strict';
import { slugify, debounce, retry } from '../lib/util.js';

describe('slugify', () => {
  it('lowercases and joins words', () => { assert.equal(slugify('  Hello Big World '), 'hello-big-world'); });
  it('keeps a single word', () => { assert.equal(slugify('Word'), 'word'); });
  it('throws for non-strings', () => { assert.throws(() => slugify(null), TypeError); });
});
```

```text
▶ slugify
  ✔ lowercases and joins words
  ✔ keeps a single word
  ✔ throws for non-strings
✔ slugify
```

</details>

## 2. 🟢 The `beforeEach` hook

**Task:** a suite has an array `items`. The first test adds an element to it, the second checks that the array is empty again. Achieve this with a hook, not with test order.

**Expected result:** both tests pass.

<details><summary>Hint</summary>

`beforeEach(() => { items = []; })`.

</details>

<details><summary>Solution</summary>

```js
describe('beforeEach resets state', () => {
  let items;
  beforeEach(() => { items = []; });
  it('first test sees an empty array', () => { items.push(1); assert.deepEqual(items, [1]); });
  it('second test sees an empty array again', () => { assert.deepEqual(items, []); });
});
```

```text
▶ beforeEach resets state
  ✔ first test sees an empty array
  ✔ second test sees an empty array again
✔ beforeEach resets state
```

</details>

## 3. 🟡 A mock and retries

**Task:** test `retry` with `mock.fn`: the function throws twice and returns `'ok'` the third time (the number of calls is `3`). A second test: the function always fails; with `times = 2` there must be three calls and a rejection with the original message.

**Expected result:** both tests pass.

<details><summary>Hint</summary>

`f.mock.mockImplementationOnce(impl, callNumber)`; for the rejection — `await assert.rejects(…)`.

</details>

<details><summary>Solution</summary>

```js
test('retry succeeds on the third attempt', async () => {
  const f = mock.fn(async () => 'ok');
  f.mock.mockImplementationOnce(async () => { throw new Error('fail 1') }, 0);
  f.mock.mockImplementationOnce(async () => { throw new Error('fail 2') }, 1);
  assert.equal(await retry(f, 5), 'ok');
  assert.equal(f.mock.callCount(), 3);
});
test('retry gives up', async () => {
  const f = mock.fn(async () => { throw new Error('always') });
  await assert.rejects(retry(f, 2), { message: 'always' });
  assert.equal(f.mock.callCount(), 3);
});
```

```text
✔ retry succeeds on the third attempt
✔ retry gives up
```

</details>

## 4. 🟡 `debounce` with fake timers

**Task:** check that `debounce(spy, 100)` after the calls `d(1)`, a 50 ms pause, `d(2)` calls `spy` once and only 100 ms after the last call, with the argument `2`. Do not use real timers.

**Expected result:** the test passes instantly.

<details><summary>Hint</summary>

`t.mock.timers.enable({ apis: ['setTimeout'] })`, then `tick(50)`, `tick(99)`, `tick(1)`.

</details>

<details><summary>Solution</summary>

```js
test('debounce calls once with the last arguments', (t) => {
  t.mock.timers.enable({ apis: ['setTimeout'] });
  const spy = mock.fn();
  const d = debounce(spy, 100);
  d(1); t.mock.timers.tick(50); d(2); t.mock.timers.tick(99);
  assert.equal(spy.mock.callCount(), 0);
  t.mock.timers.tick(1);
  assert.equal(spy.mock.callCount(), 1);
  assert.deepEqual(spy.mock.calls[0].arguments, [2]);
});
```

```text
✔ debounce calls once with the last arguments
```

</details>

## 5. 🔴 A partial comparison

**Task:** the object `user` has many fields. Check only `id` and `meta.tz` without listing the rest.

**Expected result:** the test passes.

<details><summary>Hint</summary>

`assert.partialDeepStrictEqual(actual, expectedPart)`.

</details>

<details><summary>Solution</summary>

```js
test('partial structural check', () => {
  const user = { id: 1, name: 'A', roles: ['x', 'y'], meta: { created: 1, tz: 'UTC' } };
  assert.partialDeepStrictEqual(user, { id: 1, meta: { tz: 'UTC' } });
});
```

```text
✔ partial structural check
```

The method was available in Node.js 24.15 (in the experiment).

</details>
