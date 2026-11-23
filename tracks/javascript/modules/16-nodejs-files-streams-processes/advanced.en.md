---
title: "Node.js: files, streams, processes: advanced"
lang: en
track: javascript
kind: module
doc: advanced
slug: 16-nodejs-files-streams-processes
part: 2
tags: [streams, backpressure, pipeline, transform, gzip, readline, buffer, child-process, parse-args]
versions: "Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Node.js: files, streams, processes: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). All examples were run on Node.js 24.15.

## Streams

A stream processes data in chunks without loading everything into memory. Four kinds: `Readable` (a source), `Writable` (a sink), `Duplex` and `Transform` (reads and writes, transforming).

```js
import { Readable } from 'node:stream';
console.log(await Readable.from(['a', 'b', 'c']).toArray());
for await (const chunk of Readable.from([1, 2])) console.log('chunk', chunk);
```

```text
[ 'a', 'b', 'c' ]
chunk 1
chunk 2
```

- `Readable.from(iterable)` makes a stream from an iterable ([module 11](../11-iterators-and-generators/basics.en.md)); a stream is itself iterable with `for await`.
- A chunk is a `Buffer` or a string; in **objectMode** — any value.

**File streams and the chunk size:**

```js
import fs from 'node:fs';
import { Readable } from 'node:stream';
let n = 0, total = 0; for await (const c of fs.createReadStream('big.txt', { highWaterMark: 16384 })) { n++; total += c.length }
console.log('chunks', n, 'total', total, fs.createReadStream('big.txt').readableHighWaterMark, new Readable().readableHighWaterMark, new Readable({ objectMode: true }).readableHighWaterMark);
```

```text
chunks 7 total 100000 65536 16384 16
```

For a 100000-byte file and a 16384 chunk it gave 7 chunks. The default `highWaterMark` of a file stream is 65536 bytes, of a plain `Readable` 16384, in objectMode 16 objects.

## pipeline

`pipeline` connects streams into a pipeline and **reliably** finishes all of them on error:

```js
import { Readable, Writable, Transform } from 'node:stream';
import { pipeline } from 'node:stream/promises';
const upper = new Transform({ transform(chunk, enc, cb) { cb(null, chunk.toString().toUpperCase()) } });
const out = []; const sink = new Writable({ write(chunk, enc, cb) { out.push(chunk.toString()); cb() } });
await pipeline(Readable.from(['ab', 'cd']), upper, sink); console.log(out);
```

```text
[ 'AB', 'CD' ]
```

- `Transform` receives a chunk and calls `cb(error, result)`; here strings are uppercased.
- `pipeline` from `node:stream/promises` returns a promise: it is fulfilled after the whole pipeline finishes and rejected on the first error.
- An error in one link **destroys the others**:

```js
import { PassThrough } from 'node:stream';
try { await pipeline(Readable.from(['x']), new Transform({ transform(c, e, cb) { cb(new Error('transform failed')) } }), new PassThrough()) } catch (e) { console.log('pipeline error:', e.message) }
```

```text
pipeline error: transform failed
```

The old `readable.pipe(writable)` does **not forward** errors and does not close the destination: in the experiment after `r.destroy(new Error(…))` the destination stayed alive (`destroyed: false`), while `pipeline` rejected the promise and destroyed the destination (`destroyed: true`). So use `pipeline`.

## gzip compression

```js
import zlib from 'node:zlib';
await pipeline(fs.createReadStream(big), zlib.createGzip(), fs.createWriteStream(gz));
await pipeline(fs.createReadStream(gz), zlib.createGunzip(), fs.createWriteStream(back));
```

100000 identical characters were compressed to less than 1000 bytes, and decompression returned all 100000 (in the experiment). The file was not loaded into memory whole.

## Backpressure

If a sink is slower than the source, data piles up in memory. The **backpressure** protocol stops the source:

```js
import { Writable } from 'node:stream';
import { once } from 'node:events';
const w = new Writable({ highWaterMark: 4, write(c, e, cb) { setImmediate(cb) } });
console.log('write returns', w.write('ab'), w.write('cdefg'), w.writableLength, w.writableNeedDrain);
await once(w, 'drain'); console.log('drained', w.writableNeedDrain); w.end();
```

```text
write returns true false 7 true
drained false
```

- `write()` returns `false` when the internal buffer has reached `highWaterMark` (here 7 bytes at a threshold of 4): you should write more after the `drain` event.
- `pipeline` and `for await` follow the protocol themselves: **do not write by hand in a loop** ignoring `false` — memory will balloon the queue.

## readline

Line-by-line reading of any stream:

```js
import readline from 'node:readline';
const lines = []; for await (const line of readline.createInterface({ input: Readable.from(['a\nb\n', 'c\n']) })) lines.push(line);
console.log(lines);
```

```text
[ 'a', 'b', 'c' ]
```

Chunk boundaries do not match line boundaries (`'a\nb\n'`, `'c\n'`), but `readline` joins them correctly. For a file: `readline.createInterface({ input: fs.createReadStream(path) })`.

## Buffer

`Buffer` is an array of bytes (a subclass of `Uint8Array`).

```js
const b = Buffer.from('héllo');
console.log(b.length, 'héllo'.length, b.toString('hex'), b.toString('base64'), Buffer.concat([Buffer.from('a'), Buffer.from('b')]).toString(), Buffer.alloc(3), Buffer.from('aGk=', 'base64').toString());
```

```text
6 5 68c3a96c6c6f aMOpbGxv ab <Buffer 00 00 00> hi
```

- The length in bytes (6) differs from the string length (5): `é` takes two bytes in UTF-8 (`c3 a9`).
- Encodings: `'utf8'`, `'hex'`, `'base64'`, `'latin1'`.
- Reading invalid UTF-8 as a string does not fail: invalid bytes are replaced with the character `�` (in the experiment `[0xff, 0xfe, 0x41]` → `"��A"`, with three bytes).

## Child processes

The module `node:child_process` runs other programs.

```js
import { execFile, spawn, spawnSync, exec } from 'node:child_process';
import { promisify } from 'node:util';
const execFileP = promisify(execFile);
const { stdout, stderr } = await execFileP(process.execPath, ['-e', 'console.log("out"); console.error("err")']);
console.log(JSON.stringify(stdout), JSON.stringify(stderr));
try { await execFileP(process.execPath, ['-e', 'process.exit(3)']) } catch (e) { console.log('exit code', e.code, e.killed, e.signal) }
```

```text
"out\n" "err\n"
exit code 3 false null
```

- `execFile(program, [arguments])` runs **without a shell** and collects the output; a nonzero exit code is a rejection with `e.code`.
- `spawn` returns a process with the streams `stdin`, `stdout`, `stderr` — for long processes and big outputs:

```js
const child = spawn(process.execPath, ['-e', 'process.stdin.on("data", d => process.stdout.write(d.toString().toUpperCase()))']);
let got = ''; child.stdout.on('data', d => got += d); child.stdin.write('hello '); child.stdin.end('world');
const [code, sig] = await new Promise(r => child.on('close', (c, s) => r([c, s]))); console.log(got, code, sig);
```

```text
HELLO WORLD 0 null
```

- `exec('echo …')` starts a **shell** (`echo shell-works` worked): handy for commands with `|` and `>`, but **dangerous** when user input is substituted (command injection). For arguments use `execFile`/`spawn`.
- A nonexistent program gives an `error` event with the code `ENOENT` (in the experiment `spawn error ENOENT spawn definitely-not-a-command-xyz`); without an `error` handler the process crashes.
- `child.kill()` sends `SIGTERM`: in the `close` event the code is `null`, the signal `'SIGTERM'` (in the experiment).
- Environment: `spawnSync(…, { env: { ...process.env, MY_VAR: 'custom' } })` — the child sees `custom`.
- The synchronous `spawnSync` and `execFileSync` block the event loop — only for scripts.

## Command-line arguments: parseArgs

```js
import { parseArgs } from 'node:util';
const { values, positionals } = parseArgs({ args: ['--name', 'Bob', '-v', '--count=3', 'file.txt'], options: { name: { type: 'string' }, v: { type: 'boolean', short: 'v' }, count: { type: 'string' } }, allowPositionals: true });
console.log(values, positionals);
try { parseArgs({ args: ['--unknown'], options: {} }) } catch (e) { console.log(e.code) }
```

```text
[Object: null prototype] { name: 'Bob', v: true, count: '3' } [ 'file.txt' ]
ERR_PARSE_ARGS_UNKNOWN_OPTION
```

Without `args` it parses `process.argv.slice(2)`. Numbers arrive as strings; an unknown option is an error.

## Best practices

- For big data use streams and `pipeline`, not `readFile`.
- Always handle stream errors (`pipeline` does it itself).
- `execFile`/`spawn` instead of `exec`; do not build a command from user input.
- Asynchronous APIs in server code; synchronous ones in scripts.
- Temporary files via `mkdtemp`; remove them in `finally`.
- Secrets from the environment, not from code.

## Common mistakes

- ❌ `readable.pipe(writable)` without error handling → ✅ `pipeline`.
- ❌ Calling `write()` in a loop ignoring `false` → ✅ wait for `drain` or use `pipeline`.
- ❌ `exec` with a user string → ✅ `execFile` with an argument array.
- ❌ No `error` handler on `spawn` → ✅ `child.on('error', …)`.
- ❌ Treating the string length as the length in bytes → ✅ `Buffer.byteLength`.
- ❌ `process.exit()` in the middle of a write → ✅ wait for completion and set `exitCode`.

## Related materials

- Next: [expert theory](expert.en.md)
- Iterators and `for await`: [11 · Iterators and generators](../11-iterators-and-generators/advanced.en.md#async-iterators-and-generators)
- Linux process streams: [Linux](../../../linux/overview.en.md)

## Sources

- [Node.js: Stream](https://nodejs.org/docs/latest-v24.x/api/stream.html)
- [Node.js: Backpressuring in Streams](https://nodejs.org/en/learn/modules/backpressuring-in-streams)
- [Node.js: Child process](https://nodejs.org/docs/latest-v24.x/api/child_process.html)
- [Node.js: Buffer](https://nodejs.org/docs/latest-v24.x/api/buffer.html)
- [Node.js: util.parseArgs](https://nodejs.org/docs/latest-v24.x/api/util.html#utilparseargsconfig)
- [Node.js: Readline](https://nodejs.org/docs/latest-v24.x/api/readline.html)
- [Node.js: Zlib](https://nodejs.org/docs/latest-v24.x/api/zlib.html)
