---
title: "Testing: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 15-testing
part: 2
tags: [mock-fn, mock-method, fake-timers, coverage, snapshot, test-reporter, test-timeout, dependency-injection]
versions: "Node.js 24.15 LTS (node:test, node:assert)"
status: draft
updated: 2026-11-23
---

# Testing: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15; the code under test (`lib/math.js`) is from the basic level.

## Mocks

A mock replaces a dependency and remembers the calls. The most testable code is code that receives its dependencies as **parameters** (dependency injection): `fetchUser(id, client)` takes `client`.

```js
import { test, mock } from 'node:test';
import assert from 'node:assert/strict';
import { fetchUser } from '../lib/math.js';

test('mock.fn client', async () => {
  const get = mock.fn(async () => ({ ok: true, json: async () => ({ id: 7 }) }));
  const user = await fetchUser(7, { get });
  assert.deepEqual(user, { id: 7 });
  assert.equal(get.mock.callCount(), 1);
  assert.deepEqual(get.mock.calls[0].arguments, ['/users/7']);
});

test('mock rejects not ok', async () => {
  const get = mock.fn(async () => ({ ok: false }));
  await assert.rejects(fetchUser(1, { get }), { message: 'not ok' });
});
```

- `mock.fn(impl)` creates a stub function with the implementation `impl`; `fn.mock.callCount()` is the number of calls, `fn.mock.calls[i].arguments` the arguments.
- `fn.mock.mockImplementationOnce(f)` replaces the implementation for one call (in the experiment `[f(), f()]` gave `['once', 'default']`).
- The network and files in a unit test are replaced with mocks: the test is fast and deterministic.

**Replacing a method** with automatic restoration:

```js
test('mock.method restores', (t) => {
  const obj = { greet: () => 'hi' };
  const m = t.mock.method(obj, 'greet', () => 'mocked');
  assert.equal(obj.greet(), 'mocked'); assert.equal(m.mock.callCount(), 1);
  m.mock.restore(); assert.equal(obj.greet(), 'hi');
});
```

`t.mock` (the test context's mock) restores the replacements itself after the test; `mock.method` from the shared `mock` must be reset by hand (`mock.restoreAll()`).

## Fake timers

`mock.timers` replaces `setTimeout`, `setInterval`, `setImmediate` and `Date` so the test does not wait:

```js
test('fake timers', (t) => {
  t.mock.timers.enable({ apis: ['setTimeout'] });
  let fired = false; setTimeout(() => { fired = true }, 1000);
  assert.equal(fired, false); t.mock.timers.tick(999); assert.equal(fired, false); t.mock.timers.tick(1); assert.equal(fired, true);
});

test('fake Date', (t) => {
  t.mock.timers.enable({ apis: ['Date'], now: 1000 }); assert.equal(Date.now(), 1000); t.mock.timers.tick(500); assert.equal(Date.now(), 1500);
});
```

- `enable({ apis, now })` turns the replacement on; `tick(ms)` "fast-forwards" time.
- The 1000 ms timer fired instantly, as soon as time reached `1000`.
- Code that depends on the current time becomes deterministic.

## Selecting and filtering tests

| Flag | What it does |
|---|---|
| `--test-name-pattern="divide"` | run only tests whose name contains `divide` (in the experiment the tests of the `divide` suite remained) |
| `--test-skip-pattern=…` | skip the matching ones |
| `--test-only` + `{ only: true }` | run only those marked `only`; **without the flag `only` has no effect** (the runner prints `'only' and 'runOnly' require the --test-only command-line option`) |
| `--test-timeout=ms` | a global test timeout |

A timeout for one test: `test('name', { timeout: 20 }, async () => …)`; when exceeded the test fails with `test timed out after 20ms` (in the experiment).

## Code coverage

```text
$ node --test --experimental-test-coverage
ℹ start of coverage report
ℹ file      | line % | branch % | funcs % | uncovered lines
ℹ lib       |        |          |         |
ℹ  math.js  | 100.00 |   100.00 |   75.00 |
ℹ all files | 100.00 |   100.00 |   75.00 |
ℹ end of coverage report
```

(The table is abbreviated.) The function `untested` was not called, so `funcs` is `75%` (three functions out of four). Lines are nonetheless 100% covered because it is written on one line: **line coverage does not replace checking behavior**. Thresholds can be set with the flags `--test-coverage-lines`, `--test-coverage-branches`, `--test-coverage-functions`; the set of files with `--test-coverage-include` and `--test-coverage-exclude`.

> ⚠️ Verify: the flag `--experimental-test-coverage` is called experimental in the Node.js 24.15 help; the `--test-coverage-*` thresholds and file filters were not run.

## Snapshots

A snapshot saves the result and compares against it on later runs:

```js
test('snapshot', (t) => { t.assert.snapshot({ x: 1, list: [1, 2] }); });
test('file snapshot', (t) => { t.assert.fileSnapshot('plain text', './snap.txt'); });
```

- The first run **fails** with `Missing snapshots can be generated by rerunning the command with the --test-update-snapshots flag`.
- `node --test --test-update-snapshots` creates a `*.snapshot` file next to the test (in the experiment `snap.test.js.snapshot` with an entry `exports[\`snapshot 1\`] = …`); after that the test passes.
- The snapshot file is committed to the repository; changes in it are a subject of review.
- Snapshots are easy to "cement" by mistake: check updates by eye.

## Reporters

`--test-reporter=<name>` selects the format: `spec` by default (in the experiment also when piped), `dot` — one dot per test (in the experiment `...............`), and also `tap`, `junit`, `lcov`. `--test-reporter-destination=<file>` writes the report to a file — handy for CI.

> ⚠️ Verify: the `tap`, `junit`, `lcov` formats, `--test-shard` and `--test-rerun-failures` were not run in this material.

## Organizing tests

- Tests are placed next to the code (`math.test.js`) or in a `test/` folder.
- The file name and location decide whether it runs (see the [expert theory](expert.en.md#test-file-discovery)).
- Every test file runs in a **separate process**: global variables of one file are not visible to another (in the experiment `leak a 1` and `leak b undefined`).
- Testable code: pure functions, dependencies as parameters, I/O kept separate.

## Best practices

- One test checks one thing; the name describes the behavior.
- Structure: arrange → act → assert.
- Tests do not depend on order, the network, the current time and randomness.
- Mock the system's boundaries (network, disk, time), not internal details.
- Check errors too: `assert.throws`, `assert.rejects`.
- Wire `node --test` and coverage into CI.

## Common mistakes

- ❌ `only` without `--test-only` → ✅ add the flag (or remove `only`).
- ❌ Mocks not reset between tests → ✅ `t.mock`, `mock.restoreAll()`.
- ❌ Real `setTimeout` in tests → ✅ `mock.timers`.
- ❌ Chasing 100% coverage → ✅ meaningful checks.
- ❌ Not committing the snapshot file → ✅ commit and review.
- ❌ Testing private details → ✅ public behavior.

## Related materials

- Next: [expert theory](expert.en.md)
- Asynchrony and timers: [13 · The event loop and concurrency](../13-event-loop-and-concurrency/basics.en.md#timers)
- Tests with vitest: [TypeScript](../../../typescript/overview.en.md)
- Unit tests in C#: [16 · Unit testing](../../../csharp/modules/16-testing-unit/overview.en.md)

## Sources

- [Node.js: Mocking](https://nodejs.org/docs/latest-v24.x/api/test.html#mocking)
- [Node.js: Code coverage](https://nodejs.org/docs/latest-v24.x/api/test.html#collecting-code-coverage)
- [Node.js: Snapshot testing](https://nodejs.org/docs/latest-v24.x/api/test.html#snapshot-testing)
- [Node.js: Test reporters](https://nodejs.org/docs/latest-v24.x/api/test.html#test-reporters)
- [Node.js: MockTimers](https://nodejs.org/docs/latest-v24.x/api/test.html#class-mocktimers)
