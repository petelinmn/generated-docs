---
title: "Упражнения: Node.js — файлы, потоки, процессы"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 16-nodejs-files-streams-processes
part: 2
tags: [fs, streams, pipeline, child-process, parse-args]
status: draft
updated: 2026-11-23
---

# Упражнения: Node.js — файлы, потоки, процессы

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Рабочая папка с `package.json` (`{"type": "module"}`). Во всех решениях используется временная папка `dir`:

```js
import fsp from 'node:fs/promises';
import fs from 'node:fs';
import path from 'node:path';
import os from 'node:os';
const dir = await fsp.mkdtemp(path.join(os.tmpdir(), 'ex16-'));
// … решение …
await fsp.rm(dir, { recursive: true, force: true });   // убрать за собой
```

## 1. 🟢 Строки и слова

**Задача:** запишите во временный файл текст `one two\nthree\n\nfour five six\n`, прочитайте его и выведите число непустых строк и число слов.

**Ожидаемый результат:** `3 6`.

<details><summary>Подсказка</summary>

`split('\n').filter(Boolean)` для строк и `split(/\s+/).filter(Boolean)` для слов.

</details>

<details><summary>Решение</summary>

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

## 2. 🟢 Сжатие и распаковка

**Задача:** сожмите файл из упражнения 1 в gzip и распакуйте обратно потоками; убедитесь, что содержимое совпало.

**Ожидаемый результат:** `true`.

<details><summary>Подсказка</summary>

`pipeline(createReadStream, createGzip(), createWriteStream)` и то же с `createGunzip()`.

</details>

<details><summary>Решение</summary>

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

## 3. 🟡 Нумерация строк

**Задача:** прочитайте файл построчно (`readline`), пронумеруйте строки преобразующим потоком и запишите в другой файл.

**Ожидаемый результат:** содержимое `"1: one two\n2: three\n3: \n4: four five six\n"`.

<details><summary>Подсказка</summary>

`Readable.from(readlineInterface)`, `Transform` с `objectMode: true`, всё соединить через `pipeline`.

</details>

<details><summary>Решение</summary>

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

Пустая строка сохранена (`3: `).

</details>

## 4. 🟡 Безопасный запуск программы

**Задача:** запустите `node` с аргументами `a b` и `;echo hacked` так, чтобы они не интерпретировались оболочкой, и выведите результат. Затем запустите процесс с `process.exit(5)` и получите код выхода.

**Ожидаемый результат:** `a b+;echo hacked` и `5`.

<details><summary>Подсказка</summary>

`promisify(execFile)(process.execPath, [аргументы])`; код выхода — `e.code` в отклонении.

</details>

<details><summary>Решение</summary>

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

Аргумент `;echo hacked` передан как обычная строка: оболочки нет.

</details>

## 5. 🔴 Маленький CLI

**Задача:** функция `cli(argv)` принимает флаг `-u` (верхний регистр), параметр `--times N` (по умолчанию `1`) и слово-позиционный аргумент. Без слова она возвращает справку и ставит `process.exitCode = 2`.

**Ожидаемый результат:** `HI HI | usage: cli [-u] [--times N] WORD | exitCode 2`.

<details><summary>Подсказка</summary>

`parseArgs({ args, options, allowPositionals: true })`; `times` приходит строкой.

</details>

<details><summary>Решение</summary>

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

В решении сброс `process.exitCode = 0` нужен только потому, что это учебный пример.

</details>
