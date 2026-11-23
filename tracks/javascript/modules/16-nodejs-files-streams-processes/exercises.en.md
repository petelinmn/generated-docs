---
title: "Exercises: Node.js — files, streams, processes"
lang: en
track: javascript
kind: module
doc: exercises
slug: 16-nodejs-files-streams-processes
part: 2
tags: [fs, streams, pipeline, child-process, parse-args]
status: draft
updated: 2026-11-23
---

# Exercises: Node.js — files, streams, processes

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. A working folder with a `package.json` (`{"type": "module"}`). All the solutions use a temporary folder `dir`:

```js
import fsp from 'node:fs/promises';
import fs from 'node:fs';
import path from 'node:path';
import os from 'node:os';
const dir = await fsp.mkdtemp(path.join(os.tmpdir(), 'ex16-'));
// … the solution …
await fsp.rm(dir, { recursive: true, force: true });   // clean up after yourself
```

## 1. 🟢 Lines and words

**Task:** write the text `one two\nthree\n\nfour five six\n` to a temporary file, read it back and print the number of non-empty lines and the number of words.

**Expected result:** `3 6`.

<details><summary>Hint</summary>

`split('\n').filter(Boolean)` for lines and `split(/\s+/).filter(Boolean)` for words.

</details>

<details><summary>Solution</summary>

```js
const f = path.join(dir, 'text.txt'); await fsp.writeFile(f, 'one two\nthree\n\nfour five six\n');
const text = await fsp.readFile(f, 'utf8');
const lines = text.split('\n').filter(Boolean);
console.log(lines.length, text.split(/\s+/).filter(Boolean).length);
```

```text
3 6
```

</details>

## 2. 🟢 Compression and decompression

**Task:** compress the file from exercise 1 into gzip and decompress it back with streams; make sure the contents match.

**Expected result:** `true`.

<details><summary>Hint</summary>

`pipeline(createReadStream, createGzip(), createWriteStream)` and the same with `createGunzip()`.

</details>

<details><summary>Solution</summary>

```js
import zlib from 'node:zlib';
import { pipeline } from 'node:stream/promises';
const gz = f + '.gz'; const back = path.join(dir, 'back.txt');
await pipeline(fs.createReadStream(f), zlib.createGzip(), fs.createWriteStream(gz));
await pipeline(fs.createReadStream(gz), zlib.createGunzip(), fs.createWriteStream(back));
console.log((await fsp.readFile(back, 'utf8')) === text);
```

```text
true
```

</details>

## 3. 🟡 Numbering lines

**Task:** read a file line by line (`readline`), number the lines with a transform stream and write them to another file.

**Expected result:** the contents `"1: one two\n2: three\n3: \n4: four five six\n"`.

<details><summary>Hint</summary>

`Readable.from(readlineInterface)`, a `Transform` with `objectMode: true`, all joined with `pipeline`.

</details>

<details><summary>Solution</summary>

```js
import readline from 'node:readline';
import { Readable, Transform } from 'node:stream';
let n = 0; const out = path.join(dir, 'numbered.txt');
const rl = readline.createInterface({ input: fs.createReadStream(f) });
const numberer = new Transform({ objectMode: true, transform(line, enc, cb) { cb(null, `${++n}: ${line}\n`) } });
await pipeline(Readable.from(rl), numberer, fs.createWriteStream(out));
console.log(JSON.stringify(await fsp.readFile(out, 'utf8')));
```

```text
"1: one two\n2: three\n3: \n4: four five six\n"
```

The empty line is preserved (`3: `).

</details>

## 4. 🟡 Running a program safely

**Task:** run `node` with the arguments `a b` and `;echo hacked` so that they are not interpreted by a shell, and print the result. Then run a process with `process.exit(5)` and get its exit code.

**Expected result:** `a b+;echo hacked` and `5`.

<details><summary>Hint</summary>

`promisify(execFile)(process.execPath, [arguments])`; the exit code is `e.code` in the rejection.

</details>

<details><summary>Solution</summary>

```js
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
const run = promisify(execFile);
const r = await run(process.execPath, ['-p', 'process.argv.slice(1).join("+")', 'a b', ';echo hacked']);
console.log(r.stdout.trim());
const failed = await run(process.execPath, ['-e', 'process.exit(5)']).then(() => 0, e => e.code);
console.log(failed);
```

```text
a b+;echo hacked
5
```

The argument `;echo hacked` is passed as an ordinary string: there is no shell.

</details>

## 5. 🔴 A tiny CLI

**Task:** the function `cli(argv)` accepts a flag `-u` (uppercase), a parameter `--times N` (default `1`) and a positional word. Without a word it returns usage text and sets `process.exitCode = 2`.

**Expected result:** `HI HI | usage: cli [-u] [--times N] WORD | exitCode 2`.

<details><summary>Hint</summary>

`parseArgs({ args, options, allowPositionals: true })`; `times` arrives as a string.

</details>

<details><summary>Solution</summary>

```js
import { parseArgs } from 'node:util';
function cli(argv) {
  const { values, positionals } = parseArgs({ args: argv, options: { upper: { type: 'boolean', short: 'u' }, times: { type: 'string', default: '1' } }, allowPositionals: true });
  const word = positionals[0]; if (!word) { process.exitCode = 2; return 'usage: cli [-u] [--times N] WORD'; }
  const s = values.upper ? word.toUpperCase() : word; return Array(Number(values.times)).fill(s).join(' ');
}
console.log(cli(['-u', '--times', '2', 'hi']), '|', cli([]), '| exitCode', process.exitCode);
process.exitCode = 0;
```

```text
HI HI | usage: cli [-u] [--times N] WORD | exitCode 2
```

In the solution, resetting `process.exitCode = 0` is needed only because this is a teaching example.

</details>
