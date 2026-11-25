---
title: "Node.js: files, streams, processes"
lang: en
track: javascript
kind: module
doc: overview
slug: 16-nodejs-files-streams-processes
part: 2
levels: [1, 2, 3, 4]
tags: [nodejs, fs, path, streams, backpressure, pipeline, child-process, process-env, buffer, parse-args]
versions: "Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Node.js: files, streams, processes

🌐 [Русский](overview.ru.md) · **English**

## In short

Node.js turns JavaScript into a language for servers and utilities. Three pillars: the file system (`node:fs`, `node:path`), streams for processing data in parts, and child processes (`node:child_process`) for running other programs. Together with `process` (arguments, environment, exit codes) they let you write console tools and services.

## Key ideas

- **`fs` has three styles:** callbacks, synchronous `*Sync`, and promises (`node:fs/promises`); for new code — promises.
- **Paths are built with `path`,** not by concatenation: separators depend on the OS.
- **A file can be read whole or as a stream:** big files only as a stream.
- **A stream** hands out and accepts data in chunks; `pipeline` connects streams and handles errors correctly.
- **Backpressure:** `write()` returns `false` when the buffer is full; `pipeline` and `for await` account for this themselves.
- **`Buffer`** is bytes; strings in Node.js are UTF-8.
- **`child_process`:** `execFile` and `spawn` run programs; `exec` uses a shell and is dangerous with user input.
- **`process`:** `argv`, `env`, `exitCode`, `cwd()`; `process.env` values are always strings.
- **`util.parseArgs`** parses command-line arguments with no dependencies.

## Minimal example

```js
import fsp from 'node:fs/promises';
import path from 'node:path';
import os from 'node:os';
const file = path.join(await fsp.mkdtemp(path.join(os.tmpdir(), 'demo-')), 'a.txt');
await fsp.writeFile(file, 'line1\nline2\n');
await fsp.appendFile(file, 'line3\n');
console.log(JSON.stringify(await fsp.readFile(file, 'utf8')));
```

```text
"line1\nline2\nline3\n"
```

## When to use and when not

| Task | Choice |
|---|---|
| A small file as a whole | `fsp.readFile` / `writeFile` |
| A big file, processing by lines or chunks | streams, `readline`, `pipeline` |
| Running an external program | `execFile` / `spawn` (no shell) |
| A script with several shell commands | `exec` — only for trusted strings |
| Parsing CLI arguments | `util.parseArgs` |
| Secrets | environment variables or `.env` via `--env-file` |

- ❌ Do not read huge files into memory whole.
- ❌ Do not put user input into `exec`.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | `fs`, `path`, errors, `process`, the environment, exit codes |
| [2. Advanced theory](advanced.en.md) | streams, `pipeline`, backpressure, `Transform`, gzip, `readline`, `Buffer`, child processes, `parseArgs` |
| [3. Expert theory](expert.en.md) | a reference, internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn javascript 16-nodejs-files-streams-processes level 5` |
| 6. Tests | not created yet — `/learn javascript 16-nodejs-files-streams-processes level 6` |

## Related materials

- Prerequisites: [09 · Modules](../09-modules/overview.en.md), [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md), [13 · The event loop and concurrency](../13-event-loop-and-concurrency/overview.en.md)
- Next: [17 · Browser: the DOM and events](../17-browser-dom-and-events/overview.en.md)
- Analogues in other tracks: [Linux: files, processes and pipes](../../../linux/overview.en.md), [Python](../../../python/overview.en.md) (`pathlib`, `subprocess`), [C#: I/O](../../../csharp/overview.en.md)
- [Linux 02 · Filesystem](../../../linux/modules/02-filesystem/overview.en.md)
- [Linux 03 · Shell basics](../../../linux/modules/03-shell-basics/overview.en.md)
