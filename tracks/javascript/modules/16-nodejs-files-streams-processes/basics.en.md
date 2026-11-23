---
title: "Node.js: files, streams, processes: basics"
lang: en
track: javascript
kind: module
doc: basics
slug: 16-nodejs-files-streams-processes
part: 2
tags: [fs, fs-promises, path, process, env, exit-code, error-codes]
versions: "Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Node.js: files, streams, processes: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [09 · Modules](../09-modules/overview.en.md), [12 · Promises and async/await](../12-promises-and-async-await/overview.en.md), [14 · Errors and debugging](../14-errors-and-debugging/overview.en.md) · **Time:** ~2.5 h

All examples were run on Node.js 24.15 (Windows) in a folder with a `package.json` (`{"type": "module"}`). Paths in the output are normalized to `/`.

## Why it matters

Almost every Node.js program reads and writes files, gets parameters and the environment, and exits with a code. This is the basis of console tools and services.

## Three styles of fs

```js
import fs from 'node:fs';
import fsp from 'node:fs/promises';
const text = fs.readFileSync('a.txt', 'utf8');                 // synchronous
fs.readFile('a.txt', 'utf8', (err, data) => { /* callback */ });   // callbacks
const text2 = await fsp.readFile('a.txt', 'utf8');              // promises
```

- **Synchronous** functions block the event loop ([module 13](../13-event-loop-and-concurrency/basics.en.md#blocking-the-loop)): acceptable in startup scripts and CLIs, but not in server handlers.
- **Callbacks** are the old style (`(err, data)`), almost unneeded today.
- **`node:fs/promises`** is the default style for new code.

## Reading and writing

```js
import fsp from 'node:fs/promises';
import path from 'node:path';
import os from 'node:os';
const dir = await fsp.mkdtemp(path.join(os.tmpdir(), 'js16-'));
const f = path.join(dir, 'a.txt');
await fsp.writeFile(f, 'line1\nline2\n'); await fsp.appendFile(f, 'line3\n');
console.log(JSON.stringify(await fsp.readFile(f, 'utf8')), (await fsp.readFile(f)).constructor.name, (await fsp.readFile(f)).length);
const st = await fsp.stat(f); console.log(st.isFile(), st.isDirectory(), st.size, st.mtime instanceof Date);
```

```text
"line1\nline2\nline3\n" Buffer 18
true false 18 true
```

- `writeFile` creates or **overwrites** a file, `appendFile` adds to the end.
- `readFile(path, 'utf8')` returns a string; without an encoding — a `Buffer` (bytes, `length` `18`).
- `stat` gives metadata: `isFile()`, `isDirectory()`, `size`, `mtime`.
- `mkdtemp` creates a temporary folder with a unique name — handy for tests and scripts.

Other operations: `rename`, `copyFile`, `rm`, `access`. Writing with the flag `'a'`: `fs.writeFileSync(f, 'b', { flag: 'a' })` appends to the end (in the experiment `a` + `b` → `ab`).

## Directories

```js
await fsp.mkdir(path.join(dir, 'x/y/z'), { recursive: true });
await fsp.writeFile(path.join(dir, 'x/y/b.json'), '{"k":1}');
const fwd = p => p.split(path.sep).join('/');
console.log((await fsp.readdir(dir)).sort(), (await fsp.readdir(dir, { recursive: true })).map(fwd).sort());
console.log((await fsp.readdir(dir, { withFileTypes: true })).map(d => d.name + ':' + (d.isDirectory() ? 'dir' : 'file')).sort());
console.log(JSON.parse(await fsp.readFile(path.join(dir, 'x/y/b.json'), 'utf8')));
```

```text
[ 'a.txt', 'x' ] [ 'a.txt', 'x', 'x/y', 'x/y/b.json', 'x/y/z' ]
[ 'a.txt:file', 'x:dir' ]
{ k: 1 }
```

- `mkdir(…, { recursive: true })` creates the whole chain and does not fail if the folder exists.
- `readdir` returns names; `{ recursive: true }` walks deep; `{ withFileTypes: true }` gives `Dirent` objects with `isDirectory()`.
- `JSON.parse(await readFile(…, 'utf8'))` is the typical way to read a JSON file.

Deletion: `await fsp.rm(dir, { recursive: true, force: true })`. Without `force` a missing path gives `ENOENT`, and a non-empty folder without `recursive` gives an error.

## Paths: the path module

Paths are built with `path`, not with strings: the separator depends on the OS.

```js
console.log(path.join('a', '..', 'b', 'c.txt'), path.basename('/x/y/file.tar.gz'), path.extname('/x/y/file.tar.gz'), path.dirname('/x/y/file.txt'), path.parse('/x/y/file.txt').name);
console.log(path.posix.join('a', 'b'), path.win32.join('a', 'b'), path.isAbsolute('/a'), path.relative('/a/b', '/a/c/d'));
import { fileURLToPath } from 'node:url';
console.log(path.basename(fileURLToPath(import.meta.url)), path.basename(import.meta.dirname), path.basename(import.meta.filename));
```

```text
b/c.txt file.tar.gz .gz /x/y file
a/b a\b true ../c/d
a.js js16 a.js
```

(On Windows `path.join` gives `b\c.txt`; the output is normalized to `/`.)

- `join` joins and normalizes (`..` is collapsed), `resolve` builds an absolute path from the current directory.
- `basename`, `extname`, `dirname`, `parse` take a path apart; the extension of `file.tar.gz` is `.gz`.
- `path.posix` and `path.win32` give the behavior of a specific OS regardless of the current one.
- ES modules have no `__dirname`: use `import.meta.dirname` and `import.meta.filename` ([module 09](../09-modules/basics.en.md)); from `import.meta.url` a path is obtained with `fileURLToPath`.

## File system errors

```js
try { await fsp.readFile(path.join(dir, 'nope')) } catch (e) { console.log(e.code, e.syscall) }
try { await fsp.mkdir(path.join(dir, 'x')) } catch (e) { console.log(e.code) }
console.log(await fsp.access(f).then(() => true, () => false), fs.existsSync(path.join(dir, 'nope')));
```

```text
ENOENT open
EEXIST
true false
```

- Errors carry a stable `code`: `ENOENT` (no file), `EEXIST` (already exists), `EACCES` (no permission), `EISDIR`, `ENOTEMPTY`.
- Checking "does the file exist" **before** an action (`existsSync`, `access`) creates a race: between the check and the action the file may change. It is more reliable to perform the action and handle the error.

## The process, the environment, arguments

```js
import process from 'node:process';
console.log(process.argv.length >= 2, typeof process.pid, process.platform, typeof process.cwd(), process.version.startsWith('v24'));
process.env.NEW_VAR = 42; console.log(typeof process.env.NEW_VAR, process.env.NEW_VAR, 'MISSING' in process.env, process.env.MISSING ?? 'default');
```

```text
true number win32 string true
string 42 false default
```

- `process.argv` is an array: the path to `node`, the path to the script, then your arguments.
- `process.env` is an object of environment variables; values are **always strings** (the number `42` became the string `'42'`), a missing variable is `undefined`.
- `process.cwd()` is the current directory, `process.platform` is `'win32'`, `'linux'`, `'darwin'`.
- A settings file: `node --env-file=.env app.js` loads `KEY=value` pairs into `process.env` (in the experiment `API_KEY=secret123`, quoted values without quotes; comments are skipped; a missing file is a `not found` error, and `--env-file-if-exists` skips it).

> Keep secrets in `.env` and add the file to `.gitignore`: it must not get into the repository.

## Exit codes

```js
process.on('exit', c => console.log('exit', c));
process.exitCode = 2;   // set the code without stopping work
// process.exit(1)       // an immediate exit: unfinished operations are lost
```

- `0` is success, a nonzero code is an error; CI and the shell rely on it.
- Prefer `process.exitCode = N` and a natural finish: `process.exit()` cuts work off and can lose unsent data.
- The `exit` event receives the final code.

## Beginner mistakes

- ❌ Joining paths with strings (`dir + '/' + name`) → ✅ `path.join`.
- ❌ Reading a big file whole → ✅ a stream ([advanced theory](advanced.en.md#streams)).
- ❌ Synchronous `fs` in a request handler → ✅ `fs/promises`.
- ❌ Checking `existsSync` and then reading → ✅ read and catch `ENOENT`.
- ❌ Treating `process.env.PORT` as a number → ✅ `Number(process.env.PORT)`.
- ❌ Committing `.env` with secrets → ✅ `.gitignore`.

## Related materials

- Next: [advanced theory](advanced.en.md) — streams, `pipeline`, child processes
- Earlier: [14 · Errors and debugging](../14-errors-and-debugging/advanced.en.md#nodejs-error-codes)

## Sources

- [Node.js: File system](https://nodejs.org/docs/latest-v24.x/api/fs.html)
- [Node.js: Path](https://nodejs.org/docs/latest-v24.x/api/path.html)
- [Node.js: Process](https://nodejs.org/docs/latest-v24.x/api/process.html)
- [Node.js: Environment variables (`--env-file`)](https://nodejs.org/docs/latest-v24.x/api/cli.html#--env-fileconfig)
