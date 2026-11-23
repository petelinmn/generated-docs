---
title: "Testing"
lang: en
track: javascript
kind: module
doc: overview
slug: 15-testing
part: 2
levels: [1, 2, 3, 4]
tags: [testing, node-test, assert, mock, fake-timers, coverage, snapshot, test-runner]
versions: "Node.js 24.15 LTS (node:test, node:assert)"
status: draft
updated: 2026-11-23
---

# Testing

🌐 [Русский](overview.ru.md) · **English**

## In short

Node.js has a built-in test runner `node:test` and an assertions module `node:assert`: with no dependencies and no setup you can write tests, run them with `node --test`, and use hooks, subtests, mocks, fake timers, snapshots and a coverage report. A good test is deterministic, fast, independent of the others and checks behavior, not implementation.

## Key ideas

- **`node --test`** finds and runs test files; every file runs in a **separate process**.
- **`test`, `describe`, `it`** describe tests and suites; `skip` and `todo` mark skipped ones.
- **`assert/strict`:** strict comparison (`equal`, `deepEqual`, `throws`, `rejects`); the plain `node:assert` mode is loose.
- **Hooks** `before`, `after`, `beforeEach`, `afterEach` prepare and clean up the environment.
- **Async tests** are just `async` functions; a promise's error is checked with `assert.rejects`.
- **Mocks** (`mock.fn`, `mock.method`) replace dependencies and count calls.
- **Fake timers** (`mock.timers`) control `setTimeout` and `Date` without waiting.
- **Coverage and snapshots:** `--experimental-test-coverage`, `t.assert.snapshot` with `--test-update-snapshots`.
- **The exit code** of `node --test` is `0` on success and `1` on failure: that is how tests are wired into CI.

## Minimal example

```js
// math.test.js
import { test } from 'node:test';
import assert from 'node:assert/strict';
const add = (a, b) => a + b;
test('add works', () => { assert.equal(add(1, 2), 3); });
```

```text
$ node --test
✔ add works (0.616ms)
ℹ tests 1
ℹ pass 1
ℹ fail 0
```

## When to use and when not

| Task | Choice |
|---|---|
| Unit tests of functions and modules | `node:test` + `assert/strict` |
| Replacing a dependency, checking calls | `mock.fn`, `mock.method` |
| Code with timers and dates | `mock.timers` |
| Many ready matchers, UI snapshots, the Jest ecosystem | Jest or Vitest (see [TypeScript](../../../typescript/overview.en.md)) |
| Tests in a browser | Playwright, Vitest browser mode (not run) |

- ❌ Do not test implementation details: the test breaks on every refactoring.
- ❌ Do not touch the network and the file system in unit tests.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | `test`, `describe`/`it`, assertions, hooks, async tests, subtests, running |
| [2. Advanced theory](advanced.en.md) | mocks, fake timers, filters, timeouts, coverage, snapshots, reporters, test layout |
| [3. Expert theory](expert.en.md) | a reference, file discovery, isolation, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 15-testing level 5` |
| 6. Tests | not created yet — `/learn javascript 15-testing level 6` |

## Related materials

- Prerequisites: [14 · Errors and debugging](../14-errors-and-debugging/overview.en.md), [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md), [09 · Modules](../09-modules/overview.en.md)
- Next: [16 · Node.js: files, streams, processes](../16-nodejs-files-streams-processes/overview.en.md)
- Analogues in other tracks: [C#: unit testing](../../../csharp/modules/16-testing-unit/overview.en.md), [Python](../../../python/overview.en.md) (pytest), [TypeScript](../../../typescript/overview.en.md) (vitest)
