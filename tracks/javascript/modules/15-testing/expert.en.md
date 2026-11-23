---
title: "Testing: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 15-testing
part: 2
tags: [node-test, test-discovery, process-isolation, assert-strict, coverage, snapshot, flaky-tests, mock-timers]
versions: "Node.js 24.15 LTS (node:test, node:assert)"
status: draft
updated: 2026-11-23
---

# Testing: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15.

## Reference

### Command-line flags (from `node --help`)

| Flag | Purpose |
|---|---|
| `--test` | start the runner (not needed separately for `node --test`) |
| `--test-concurrency=N` | the number of test files run simultaneously |
| `--test-name-pattern=…` / `--test-skip-pattern=…` | run / skip tests by name |
| `--test-only` | run only tests with `only` |
| `--test-reporter=…` / `--test-reporter-destination=…` | the report format and destination |
| `--test-timeout=…` | a test timeout |
| `--test-force-exit` | force the process to exit after the tests |
| `--test-shard=…` | run part of the tests (for parallelizing in CI) |
| `--test-update-snapshots` | regenerate snapshots |
| `--test-rerun-failures=…` | rerun failures from a state file |
| `--test-global-setup=…` | global setup before all tests |
| `--test-coverage-include/exclude/lines/branches/functions` | coverage filters and thresholds |
| `--experimental-test-coverage` | collect coverage |

> ⚠️ Verify: the action of `--test-concurrency`, `--test-force-exit`, `--test-shard`, `--test-rerun-failures` and `--test-global-setup` comes from the help and documentation, not run in the experiments.

### `node:assert` assertions (in the experiment)

| Method | Notes |
|---|---|
| `ok`, `equal`, `notEqual`, `deepEqual`, `notDeepEqual` | strict in `assert/strict` (`===`, deep strict comparison) |
| `strictEqual`, `deepStrictEqual` | a check by `Object.is` and a deep strict one; `strictEqual(NaN, NaN)` passes |
| `throws`, `doesNotThrow` | synchronous errors; the second argument is a class, an object (fields are compared), a regular expression |
| `rejects`, `doesNotReject` | for promises, **`await` is needed** |
| `match`, `doesNotMatch` | regular expressions |
| `ifError` | throws if the value is not `null`/`undefined` |
| `partialDeepStrictEqual` | nested partial comparison (available in Node.js 24.15 in the experiment) |
| `fail` | an unconditional failure |

Loose mode (`import assert from 'node:assert'`): `equal('1', 1)` and `deepEqual({ a: '1' }, { a: 1 })` **pass** (in the experiment).

### The test context `t`

`t.test(…)` is a subtest; `t.diagnostic(msg)` is a note in the report (in the experiment `ℹ custom note`); `t.plan(n)` is the expected number of checks (in the experiment `t.plan(1)` with one `t.assert.equal`); `t.assert` are assertions bound to the test (needed for `snapshot`); `t.mock` is mocks with automatic cleanup.

## Test file discovery

When started **without arguments** the runner recursively looks for files by patterns (per the documentation):

| Pattern | Example |
|---|---|
| `**/*.test.{cjs,mjs,js}` | `math.test.js` |
| `**/*-test.{cjs,mjs,js}` | `other-test.js` |
| `**/*_test.{cjs,mjs,js}` | `math_test.js` |
| `**/test-*.{cjs,mjs,js}` | `test-math.js` |
| `**/test.{cjs,mjs,js}` | `test.js` |
| `**/test/**/*.{cjs,mjs,js}` | **any** file in a `test` folder |

In the experiment `other-test.js` in the root, `lib/not-a-test.js` (the name ends with `-test.js`) and `test/helper.js` (a helper file in the `test` folder) ran. So **do not put** helper code in `test/` and do not name it so that the name matches a pattern; with arguments (`node --test test/a.test.js`) only the listed files run.

## Internals

- **A process per file.** Every test file runs in a separate Node.js process; in the experiment a global variable from the first file was `1`, and in the second — `undefined`. So state is not shared between files and file order does not matter; inside a file everything is shared.
- **Asynchrony.** The runner awaits the promise a test returned; an unhandled error or a rejected promise marks the test failed. A test without `await` on its last asynchronous action ends earlier.
- **Timeout.** When exceeded, the test is cancelled with the message `test timed out after 20ms`; in the summary such a test goes into `cancelled` (in the experiment `cancelled 1`).
- **Exit code.** `1` if there are failed or cancelled tests; `0` otherwise (in the experiment).
- **Time mock.** `mock.timers` replaces the global `setTimeout` and `Date` for the test's duration; `tick` calls timers synchronously.
- **Coverage.** The data is collected by V8; the runner turns it into a report (lines, branches, functions). A line counts as covered if any of its fragments ran, so one-line functions give "100%" of lines with an uncovered function (in the experiment `funcs 75%`).

> ⚠️ Verify: the details of V8 coverage collection and the `--test-concurrency` modes with shared resources (ports, files) were not checked.

## Gotchas

### 1. `only` without `--test-only`

The marker is ignored (the runner prints a notice); all tests run.

### 2. Your own message appends the difference

`assert.strictEqual(1, 2, 'custom message')` throws an error whose `message` is `'custom message\n\n1 !== 2\n'`: `assert.throws(…, { message: 'custom message' })` **failed** in the experiment (Node.js 24.15).

### 3. A forgotten `await` on `assert.rejects`

The test ends before the check and is marked `✔`, but the error appears later: the runner prints `generated asynchronous activity after the test ended` and marks the whole file failed (in the experiment `pass 1`, `fail 1`, exit code `1`).

### 4. The loose `node:assert`

`equal('1', 1)` passes: bugs hide. Use `node:assert/strict`.

### 5. Helper files in `test/`

The runner will execute them as tests (in the experiment `helper executed as test file`).

### 6. A snapshot on the first run

The test fails until a snapshot file is created (`--test-update-snapshots`).

### 7. Line coverage is deceptive

A one-line uncovered function gives `100%` of lines and `75%` of functions.

### 8. Shared state between tests of one file

A file is one process: a changed global object or a mock that was not reset "leaks" into the next test.

### 9. Flaky tests

Real timers, randomness, the current time and execution order make tests nondeterministic: replace them (`mock.timers`) and fix the initial data.

## Trade-offs and alternatives

| Tool | Pros | Cons |
|---|---|---|
| `node:test` | built in, no dependencies, a process per file | fewer features than Jest/Vitest; some features are experimental |
| Jest | matchers, mocks, snapshots, a large ecosystem | a dependency, its own runner |
| Vitest | fast, Vite-compatible, TypeScript | a dependency; not run here |
| Mocha + Chai | flexibility | assembled from parts |
| Playwright | end-to-end and browser tests | a separate tool |
| Contract tests | fewer mocks | harder to set up |

## Interview questions

<details><summary>1. How do you run tests in Node.js without third-party packages?</summary>

`node --test`: the runner finds files by patterns and runs them; the exit code is `0` or `1`.

→ [Basic theory](basics.en.md#the-first-test)

</details>

<details><summary>2. How does <code>assert/strict</code> differ from <code>assert</code>?</summary>

In strict mode `equal` and `deepEqual` compare strictly; in the plain one loosely (`'1'` equals `1`).

→ [Gotcha 4](#4-the-loose-nodeassert)

</details>

<details><summary>3. Trick: a test with <code>assert.rejects</code> without <code>await</code> shows <code>✔</code>, although the check should not pass. What happens?</summary>

The test ended before the check; the error appears after it ends and fails the whole file. You need `await`.

→ [Gotcha 3](#3-a-forgotten-await-on-assertrejects)

</details>

<details><summary>4. How are test files isolated?</summary>

Each file is a separate process; global state is not shared between files.

→ [Internals](#internals)

</details>

<details><summary>5. How do you replace a dependency in a test?</summary>

Pass it as a parameter and substitute `mock.fn`; or `t.mock.method(obj, 'name', impl)`.

→ [Advanced theory](advanced.en.md#mocks)

</details>

<details><summary>6. How do you test code with <code>setTimeout</code> without waiting?</summary>

`t.mock.timers.enable({ apis: ['setTimeout'] })` and `t.mock.timers.tick(ms)`.

→ [Advanced theory](advanced.en.md#fake-timers)

</details>

<details><summary>7. Trick: why did <code>{ only: true }</code> not work?</summary>

The `--test-only` flag is needed.

→ [Gotcha 1](#1-only-without---test-only)

</details>

<details><summary>8. What does coverage show and how is it deceptive?</summary>

The shares of executed lines, branches, functions; high numbers do not mean good checks and can hide uncovered functions on one line.

→ [Gotcha 7](#7-line-coverage-is-deceptive)

</details>

<details><summary>9. How do snapshots work and where are they stored?</summary>

`t.assert.snapshot` compares with a `*.snapshot` file; the first run needs `--test-update-snapshots`; the file is committed.

→ [Advanced theory](advanced.en.md#snapshots)

</details>

<details><summary>10. Trick: why does a test fail "out of nowhere" if you pass your own message to <code>assert.strictEqual</code>?</summary>

The message is not "clean": Node.js appends the difference of the values, so a comparison with the exact error text does not match.

→ [Gotcha 2](#2-your-own-message-appends-the-difference)

</details>

<details><summary>11. What makes a test flaky and how do you fix it?</summary>

Time, randomness, the network, order and shared state. Replace them, fix the data, isolate tests.

→ [Gotcha 9](#9-flaky-tests)

</details>

<details><summary>12. What is the exit code of <code>node --test</code> and why does it matter?</summary>

`0` on success, `1` on failure; CI decides from the exit code whether the build passed.

→ [Basic theory](basics.en.md#the-exit-code)

</details>

## Cheat sheet

```text
node --test [files]   --test-name-pattern  --test-only  --test-timeout  --test-reporter=dot
--experimental-test-coverage   --test-update-snapshots
import { test, describe, it, before, after, beforeEach, afterEach, mock } from 'node:test'
import assert from 'node:assert/strict'   // equal, deepEqual, throws, rejects (await!), match
mock.fn(impl) · t.mock.method(obj, 'm', impl) · t.mock.timers.enable({ apis: ['setTimeout', 'Date'] }); tick(ms)
t.assert.snapshot(value) · test(name, { timeout, skip, todo, only }, fn)
```

- A file is a separate process; inside a file the state is shared.
- `only` works only with `--test-only`.
- Do not put helper files in `test/`.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Errors and `assert`: [14 · Errors and debugging](../14-errors-and-debugging/expert.en.md)
- Timers and the event loop: [13 · The event loop and concurrency](../13-event-loop-and-concurrency/expert.en.md)
- Unit tests in C#: [16 · Unit testing](../../../csharp/modules/16-testing-unit/expert.en.md)

## Sources

- [Node.js: Test runner](https://nodejs.org/docs/latest-v24.x/api/test.html)
- [Node.js: Test runner execution model](https://nodejs.org/docs/latest-v24.x/api/test.html#test-runner-execution-model)
- [Node.js: Assert](https://nodejs.org/docs/latest-v24.x/api/assert.html)
- [Node.js: Command-line options (test flags)](https://nodejs.org/docs/latest-v24.x/api/cli.html)
