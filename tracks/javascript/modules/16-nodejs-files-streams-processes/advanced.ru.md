---
title: "Node.js: файлы, потоки, процессы: продвинутый уровень"
lang: ru
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

# Node.js: файлы, потоки, процессы: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все примеры запускались на Node.js 24.15.

## Потоки

Поток (stream) обрабатывает данные порциями, не загружая всё в память. Четыре вида: `Readable` (источник), `Writable` (приёмник), `Duplex` и `Transform` (читает и пишет, преобразуя).

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

- `Readable.from(iterable)` делает поток из итерируемого объекта ([модуль 11](../11-iterators-and-generators/basics.ru.md)); поток сам итерируем через `for await`.
- Порция (chunk) — `Buffer` или строка; в **objectMode** — любое значение.

**Файловые потоки и размер порции:**

```js
import fs from 'node:fs';
import { Readable } from 'node:stream';
let n = 0, total = 0; for await (const c of fs.createReadStream('big.txt', { highWaterMark: 16384 })) { n++; total += c.length }
console.log('chunks', n, 'total', total, fs.createReadStream('big.txt').readableHighWaterMark, new Readable().readableHighWaterMark, new Readable({ objectMode: true }).readableHighWaterMark);
```

```text
chunks 7 total 100000 65536 16384 16
```

Для файла в 100000 байт и порции 16384 вышло 7 порций. По умолчанию `highWaterMark` файлового потока — 65536 байт, обычного `Readable` — 16384, в objectMode — 16 объектов.

## pipeline

`pipeline` соединяет потоки в конвейер и **надёжно** завершает все при ошибке:

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

- `Transform` получает порцию, вызывает `cb(ошибка, результат)`; здесь строки переводятся в верхний регистр.
- `pipeline` из `node:stream/promises` возвращает промис: он выполняется после завершения всего конвейера и отклоняется при первой ошибке.
- Ошибка в одном звене **уничтожает остальные**:

```js
import { PassThrough } from 'node:stream';
try { await pipeline(Readable.from(['x']), new Transform({ transform(c, e, cb) { cb(new Error('transform failed')) } }), new PassThrough()) } catch (e) { console.log('pipeline error:', e.message) }
```

```text
pipeline error: transform failed
```

Старый `readable.pipe(writable)` ошибки **не пробрасывает** и не закрывает приёмник: в опыте после `r.destroy(new Error(…))` приёмник остался живым (`destroyed: false`), а `pipeline` отклонил промис и уничтожил приёмник (`destroyed: true`). Поэтому используйте `pipeline`.

## Сжатие gzip

```js
import zlib from 'node:zlib';
await pipeline(fs.createReadStream(big), zlib.createGzip(), fs.createWriteStream(gz));
await pipeline(fs.createReadStream(gz), zlib.createGunzip(), fs.createWriteStream(back));
```

100000 одинаковых символов сжались до размера меньше 1000 байт, обратная распаковка вернула все 100000 (в опыте). Файл не загружался в память целиком.

## Обратное давление

Если приёмник медленнее источника, данные копятся в памяти. Протокол **обратного давления** (backpressure) останавливает источник:

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

- `write()` возвращает `false`, когда внутренний буфер достиг `highWaterMark` (здесь 7 байт при пороге 4): писать больше нужно после события `drain`.
- `pipeline` и `for await` соблюдают протокол сами: **не пишите вручную в цикле**, игнорируя `false` — память раздует очередь.

## readline

Построчное чтение любого потока:

```js
import readline from 'node:readline';
const lines = []; for await (const line of readline.createInterface({ input: Readable.from(['a\nb\n', 'c\n']) })) lines.push(line);
console.log(lines);
```

```text
[ 'a', 'b', 'c' ]
```

Границы порций не совпадают с границами строк (`'a\nb\n'`, `'c\n'`), но `readline` склеивает их правильно. Для файла: `readline.createInterface({ input: fs.createReadStream(путь) })`.

## Buffer

`Buffer` — массив байт (подкласс `Uint8Array`).

```js
const b = Buffer.from('héllo');
console.log(b.length, 'héllo'.length, b.toString('hex'), b.toString('base64'), Buffer.concat([Buffer.from('a'), Buffer.from('b')]).toString(), Buffer.alloc(3), Buffer.from('aGk=', 'base64').toString());
```

```text
6 5 68c3a96c6c6f aMOpbGxv ab <Buffer 00 00 00> hi
```

- Длина в байтах (6) отличается от длины строки (5): `é` занимает два байта в UTF-8 (`c3 a9`).
- Кодировки: `'utf8'`, `'hex'`, `'base64'`, `'latin1'`.
- Чтение невалидного UTF-8 как строки не падает: недопустимые байты заменяются символом `�` (в опыте `[0xff, 0xfe, 0x41]` → `"��A"` при трёх байтах).

## Дочерние процессы

Модуль `node:child_process` запускает другие программы.

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

- `execFile(программа, [аргументы])` запускает **без оболочки** и собирает вывод; ненулевой код выхода — отклонение с `e.code`.
- `spawn` возвращает процесс с потоками `stdin`, `stdout`, `stderr` — для долгих процессов и больших выводов:

```js
const child = spawn(process.execPath, ['-e', 'process.stdin.on("data", d => process.stdout.write(d.toString().toUpperCase()))']);
let got = ''; child.stdout.on('data', d => got += d); child.stdin.write('hello '); child.stdin.end('world');
const [code, sig] = await new Promise(r => child.on('close', (c, s) => r([c, s]))); console.log(got, code, sig);
```

```text
HELLO WORLD 0 null
```

- `exec('echo …')` запускает **оболочку** (`echo shell-works` сработало): удобно для команд с `|` и `>`, но **опасно** при подстановке пользовательского ввода (внедрение команд). Для аргументов — `execFile`/`spawn`.
- Несуществующая программа даёт событие `error` с кодом `ENOENT` (в опыте `spawn error ENOENT spawn definitely-not-a-command-xyz`); без обработчика `error` процесс упадёт.
- `child.kill()` посылает `SIGTERM`: в событии `close` код `null`, сигнал `'SIGTERM'` (в опыте).
- Окружение: `spawnSync(…, { env: { ...process.env, MY_VAR: 'custom' } })` — дочерний процесс видит `custom`.
- Синхронные `spawnSync` и `execFileSync` блокируют цикл событий — только для скриптов.

## Аргументы командной строки: parseArgs

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

Без `args` разбирается `process.argv.slice(2)`. Числа приходят строками; неизвестный параметр — ошибка.

## Лучшие практики

- Для больших данных — потоки и `pipeline`, не `readFile`.
- Всегда обрабатывайте ошибки потока (`pipeline` делает это сам).
- `execFile`/`spawn` вместо `exec`; не склеивайте команду из пользовательского ввода.
- Асинхронные API в серверном коде; синхронные — в скриптах.
- Временные файлы — через `mkdtemp`; удаляйте их в `finally`.
- Секреты — из окружения, не из кода.

## Частые ошибки

- ❌ `readable.pipe(writable)` без обработки ошибок → ✅ `pipeline`.
- ❌ Писать в `write()` в цикле, игнорируя `false` → ✅ ждать `drain` или использовать `pipeline`.
- ❌ `exec` с пользовательской строкой → ✅ `execFile` с массивом аргументов.
- ❌ Нет обработчика `error` у `spawn` → ✅ `child.on('error', …)`.
- ❌ Считать длину строки длиной в байтах → ✅ `Buffer.byteLength`.
- ❌ `process.exit()` посреди записи → ✅ дождаться завершения и задать `exitCode`.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Итераторы и `for await`: [11 · Итераторы и генераторы](../11-iterators-and-generators/advanced.ru.md#асинхронные-итераторы-и-генераторы)
- Потоки процессов Linux: [Linux](../../../linux/overview.ru.md)

## Источники

- [Node.js: Stream](https://nodejs.org/docs/latest-v24.x/api/stream.html)
- [Node.js: Backpressuring in Streams](https://nodejs.org/en/learn/modules/backpressuring-in-streams)
- [Node.js: Child process](https://nodejs.org/docs/latest-v24.x/api/child_process.html)
- [Node.js: Buffer](https://nodejs.org/docs/latest-v24.x/api/buffer.html)
- [Node.js: util.parseArgs](https://nodejs.org/docs/latest-v24.x/api/util.html#utilparseargsconfig)
- [Node.js: Readline](https://nodejs.org/docs/latest-v24.x/api/readline.html)
- [Node.js: Zlib](https://nodejs.org/docs/latest-v24.x/api/zlib.html)
