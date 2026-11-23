---
title: "Node.js: files, streams, processes: expert"
lang: en
track: javascript
kind: module
doc: expert
slug: 16-nodejs-files-streams-processes
part: 2
tags: [fs, streams, backpressure, high-water-mark, child-process, command-injection, libuv-threadpool, process-exit]
versions: "Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Node.js: files, streams, processes: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All results were obtained by running on Node.js 24.15 (Windows) unless stated otherwise.

## Reference

### The main functions of `node:fs/promises`

| Function | What it does |
|---|---|
| `readFile(path[, enc])` / `writeFile(path, data[, opts])` / `appendFile` | read, overwrite, append; `{ flag: 'a' }` — append |
| `readdir(path[, { recursive, withFileTypes }])` | a list of names or `Dirent`s |
| `mkdir(path[, { recursive }])` / `mkdtemp(prefix)` | create a folder (a chain) / a temporary folder |
| `rm(path[, { recursive, force }])` | delete; `force` ignores `ENOENT` |
| `stat` / `access` | metadata / an access check (no access — a rejection) |
| `rename` / `copyFile` | rename (move) / copy |
| `open` → `FileHandle` | a descriptor: `read`, `write`, `createReadStream` |

### Streams: kinds and events

| Class | Purpose | Events and methods |
|---|---|---|
| `Readable` | a source | `data`, `end`, `error`, `close`; `read()`, `pipe()`, `for await` |
| `Writable` | a sink | `drain`, `finish`, `error`, `close`; `write()`, `end()` |
| `Duplex` | both directions | the events of both |
| `Transform` | transformation | `_transform(chunk, enc, cb)` |
| `PassThrough` | passes through unchanged | — |

Default `highWaterMark` values (in the experiment): a file `Readable` — `65536` bytes, a plain one — `16384`, objectMode — `16`. A `Writable` has the properties `writableLength` (bytes in the buffer) and `writableNeedDrain` (whether to wait for `drain`).

### `node:child_process` functions

| Function | Shell | Output | Returns |
|---|---|---|---|
| `exec(cmd)` | yes | collected into a buffer | callback / promise via `promisify` |
| `execFile(file, args)` | no | collected into a buffer | callback / promise |
| `spawn(file, args)` | no (by default) | `stdout`/`stderr` streams | `ChildProcess` |
| `fork(module)` | no | a message channel with a child Node.js | `ChildProcess` |
| `execSync`, `execFileSync`, `spawnSync` | depends | synchronous | block the loop |

`ChildProcess` events: `spawn`, `error` (failed to start), `exit` and `close` (finish: code, signal). `kill()` sends `SIGTERM` by default.

> ⚠️ Verify: `fork` and the IPC channel, the options `shell`, `stdio`, `detached`, `timeout`, `signal` were not run in this material.

### `process`

| Member | Meaning |
|---|---|
| `argv`, `execPath`, `cwd()`, `pid`, `platform`, `version`, `uptime()` | process information (in the experiment the types and `v24`, `win32`) |
| `env` | string values, missing ones are `undefined`; assignment converts to a string |
| `exitCode`, `exit(code)` | the exit code: set / exit at once |
| `stdin`, `stdout`, `stderr` | streams; `stdout.isTTY` is `undefined` if the output is redirected |
| `on('exit')`, `on('SIGINT')` etc. | process events and signals |

## Internals

- **The libuv thread pool.** Asynchronous `fs` file operations (unlike network ones) run in the libuv thread pool (4 threads by default): the main thread is not blocked, but a large number of simultaneous disk accesses hits the pool. The synchronous `*Sync` ones run in the main thread.
- **Streams and `highWaterMark`.** A stream's internal buffer is limited by a threshold; `push()`/`write()` return `false` when it is reached. `pipeline` stops the source and resumes it on `drain`.
- **Errors without a handler.** An `error` event of a stream with no listener **throws an exception** and brings the process down (in the experiment, with `r.destroy(new Error('r failed'))` on a `Readable` with no handler the process ended with `Unhandled 'error' event`).
- **`pipe` and `pipeline`.** `pipe` only moves data and does not close the destination on a source error (in the experiment `destroyed: false`); `pipeline` destroys all links and rejects the promise.
- **`Buffer` is a `Uint8Array`.** It is a "window" over an `ArrayBuffer`; UTF-8 strings take a variable number of bytes (in the experiment `'héllo'`: 5 characters, 6 bytes).
- **`readFile` with `'utf8'`.** Invalid bytes are replaced with `U+FFFD` without an error (in the experiment `[0xff, 0xfe, 0x41]` → `"��A"`): binary data is read without an encoding.
- **`process.exit()`.** Terminates the process at once without waiting for pending operations; `exit` events are synchronous. `exitCode` lets you finish naturally.
- **Console output.** `process.stdout` for files and pipes can be asynchronous; so `process.exit()` right after large writes may truncate the output.

> ⚠️ Verify: the size and behavior of the libuv thread pool, the synchrony of `process.stdout` in different environments (terminal, file, pipe; Windows and Linux), and the loss of output on `process.exit()` come from the Node.js documentation and were not reproduced in the experiments.

## Gotchas

### 1. `existsSync` and a race

The file may disappear between the check and the read: act and handle `ENOENT`.

### 2. `exec` and command injection

The string goes to a shell: `exec('ls ' + userInput)` with `; rm -rf …` runs extra things. Use `execFile`/`spawn` with an argument array.

### 3. `pipe` does not forward errors

The destination stays open; use `pipeline`.

### 4. A stream without an `error` handler

It brings the process down (`Unhandled 'error' event`).

### 5. `process.env` converts to a string

`process.env.NEW_VAR = 42` → `'42'`; an assigned `undefined` also becomes the string `'undefined'` (in the experiment).

### 6. String length ≠ length in bytes

`'héllo'.length` is `5`, `Buffer.byteLength('héllo')` is `6`.

### 7. `readdir({ recursive: true })` returns relative paths

With the OS separator: on Windows `x\y` (in the experiment `[ 'x', 'x\\y' ]`). Normalize with `path.sep` or `path`.

### 8. `writeFile` overwrites

Without the flag `'a'` the file's contents are replaced.

### 9. `parseArgs` returns strings

`--count=3` → `'3'`; convert numbers yourself.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `readFile` whole | simpler | memory proportional to the file |
| Streams + `pipeline` | constant memory, backpressure | more complex code |
| `for await` over a stream | simple syntax | no fine tuning |
| Web streams (`ReadableStream`) | a standard, the same in the browser | a different API; in Node.js via `Readable.toWeb` |
| `exec` | shell commands | injection, a buffer limit |
| `execFile`/`spawn` | no shell, safe | no `|` and `>` |
| Libraries (execa, commander, yargs) | convenience | a dependency; not run |

## Interview questions

<details><summary>1. Why do you need streams if there is <code>readFile</code>?</summary>

Streams process data in chunks with constant memory, while `readFile` loads the whole file.

→ [Advanced theory](advanced.en.md#streams)

</details>

<details><summary>2. What is backpressure?</summary>

A mechanism where a slow sink stops the source: `write()` returns `false`, we wait for `drain`.

→ [Advanced theory](advanced.en.md#backpressure)

</details>

<details><summary>3. Trick: how is <code>pipeline</code> better than <code>pipe</code>?</summary>

`pipeline` forwards errors, destroys all links and returns a promise; `pipe` does not.

→ [Gotcha 3](#3-pipe-does-not-forward-errors)

</details>

<details><summary>4. How does <code>exec</code> differ from <code>execFile</code> and why is the first dangerous?</summary>

`exec` starts a shell; user input in the command can add extra commands (injection).

→ [Gotcha 2](#2-exec-and-command-injection)

</details>

<details><summary>5. How do you find where the script lives in an ES module?</summary>

`import.meta.dirname` / `import.meta.filename` or `fileURLToPath(import.meta.url)`.

→ [Basic theory](basics.en.md#paths-the-path-module)

</details>

<details><summary>6. Trick: why is <code>existsSync</code> before reading unreliable?</summary>

Between the check and the read the file can disappear (a race); it is more reliable to catch `ENOENT`.

→ [Gotcha 1](#1-existssync-and-a-race)

</details>

<details><summary>7. What type are the values in <code>process.env</code>?</summary>

Only strings; a missing one is `undefined`.

→ [Gotcha 5](#5-processenv-converts-to-a-string)

</details>

<details><summary>8. How do you terminate a program with an error correctly?</summary>

Set `process.exitCode = 1` and let it finish naturally; `process.exit(1)` cuts off pending operations.

→ [Basic theory](basics.en.md#exit-codes)

</details>

<details><summary>9. What happens if a stream has no <code>error</code> handler?</summary>

The `error` event throws an exception and brings the process down.

→ [Gotcha 4](#4-a-stream-without-an-error-handler)

</details>

<details><summary>10. Trick: why does a string's <code>Buffer</code> length differ from its <code>length</code>?</summary>

A string's `length` is in UTF-16 code units, a buffer's length is in UTF-8 bytes (`é` is 2 bytes).

→ [Gotcha 6](#6-string-length--length-in-bytes)

</details>

<details><summary>11. How do you safely run an external program with user-supplied arguments?</summary>

`execFile(file, [arguments])` or `spawn` without `shell`; do not glue the command string together.

→ [Advanced theory](advanced.en.md#child-processes)

</details>

<details><summary>12. How do you parse CLI arguments without dependencies?</summary>

`util.parseArgs({ options, allowPositionals })`; values are strings and booleans.

→ [Advanced theory](advanced.en.md#command-line-arguments-parseargs)

</details>

## Cheat sheet

```text
fsp.readFile/writeFile/appendFile/readdir({recursive})/mkdir({recursive})/rm({recursive,force})/stat/rename
path.join/resolve/basename/extname/parse; import.meta.dirname
await pipeline(src, transform, dest)   // from node:stream/promises
execFile(file, [args]) / spawn(file, [args]) — no shell; exec — a shell
process.argv/env/exitCode/cwd(); node --env-file=.env app.js
util.parseArgs({ options, allowPositionals })
```

- Big data — streams and `pipeline`.
- `exec` — only for trusted strings.
- `exitCode` instead of `exit()`.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- The event loop and execution threads: [13 · The event loop and concurrency](../13-event-loop-and-concurrency/expert.en.md)
- Errors and Node.js codes: [14 · Errors and debugging](../14-errors-and-debugging/expert.en.md)
- The Linux analogue: [Linux](../../../linux/overview.en.md)

## Sources

- [Node.js: Stream](https://nodejs.org/docs/latest-v24.x/api/stream.html)
- [Node.js: File system](https://nodejs.org/docs/latest-v24.x/api/fs.html)
- [Node.js: Child process](https://nodejs.org/docs/latest-v24.x/api/child_process.html)
- [Node.js: Process I/O](https://nodejs.org/docs/latest-v24.x/api/process.html#a-note-on-process-io)
- [OWASP: Command Injection](https://owasp.org/www-community/attacks/Command_Injection)
