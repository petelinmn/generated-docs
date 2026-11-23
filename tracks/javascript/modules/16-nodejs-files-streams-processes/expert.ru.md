---
title: "Node.js: файлы, потоки, процессы: экспертный уровень"
lang: ru
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

# Node.js: файлы, потоки, процессы: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все результаты получены запуском на Node.js 24.15 (Windows), если не сказано иное.

## Справочник

### Основные функции `node:fs/promises`

| Функция | Что делает |
|---|---|
| `readFile(path[, enc])` / `writeFile(path, data[, opts])` / `appendFile` | чтение, перезапись, добавление; `{ flag: 'a' }` — дописать |
| `readdir(path[, { recursive, withFileTypes }])` | список имён или `Dirent` |
| `mkdir(path[, { recursive }])` / `mkdtemp(prefix)` | создать папку (цепочку) / временную папку |
| `rm(path[, { recursive, force }])` | удалить; `force` игнорирует `ENOENT` |
| `stat` / `access` | метаданные / проверка доступа (нет доступа — отклонение) |
| `rename` / `copyFile` | переименовать (переместить) / копировать |
| `open` → `FileHandle` | дескриптор: `read`, `write`, `createReadStream` |

### Потоки: виды и события

| Класс | Назначение | События и методы |
|---|---|---|
| `Readable` | источник | `data`, `end`, `error`, `close`; `read()`, `pipe()`, `for await` |
| `Writable` | приёмник | `drain`, `finish`, `error`, `close`; `write()`, `end()` |
| `Duplex` | оба направления | события обоих |
| `Transform` | преобразование | `_transform(chunk, enc, cb)` |
| `PassThrough` | передаёт без изменений | — |

Значения по умолчанию `highWaterMark` (в опыте): файловый `Readable` — `65536` байт, обычный — `16384`, objectMode — `16`. У `Writable` свойства `writableLength` (байт в буфере) и `writableNeedDrain` (нужно ли ждать `drain`).

### Функции `node:child_process`

| Функция | Оболочка | Вывод | Возврат |
|---|---|---|---|
| `exec(cmd)` | да | собирается в буфер | колбэк / промис через `promisify` |
| `execFile(file, args)` | нет | собирается в буфер | колбэк / промис |
| `spawn(file, args)` | нет (по умолчанию) | потоки `stdout`/`stderr` | `ChildProcess` |
| `fork(module)` | нет | канал сообщений с дочерним Node.js | `ChildProcess` |
| `execSync`, `execFileSync`, `spawnSync` | зависит | синхронно | блокируют цикл |

События `ChildProcess`: `spawn`, `error` (не удалось запустить), `exit` и `close` (завершение: код, сигнал). `kill()` по умолчанию шлёт `SIGTERM`.

> ⚠️ Проверить: `fork` и IPC-канал, опции `shell`, `stdio`, `detached`, `timeout`, `signal` в этом материале не запускались.

### `process`

| Член | Значение |
|---|---|
| `argv`, `execPath`, `cwd()`, `pid`, `platform`, `version`, `uptime()` | сведения о процессе (в опыте типы и `v24`, `win32`) |
| `env` | строковые значения, отсутствующие — `undefined`; присваивание приводит к строке |
| `exitCode`, `exit(code)` | код выхода: задать / выйти немедленно |
| `stdin`, `stdout`, `stderr` | потоки; `stdout.isTTY` — `undefined`, если вывод перенаправлен |
| `on('exit')`, `on('SIGINT')` и др. | события процесса и сигналы |

## Внутреннее устройство

- **Пул потоков libuv.** Асинхронные операции с файлами `fs` (в отличие от сетевых) выполняются в пуле потоков libuv (по умолчанию 4 потока): главный поток не блокируется, но большое число одновременных обращений к диску упирается в пул. Синхронные `*Sync` выполняются в главном потоке.
- **Потоки на `highWaterMark`.** Внутренний буфер потока ограничен порогом; `push()`/`write()` возвращают `false`, когда он достигнут. `pipeline` останавливает источник и возобновляет его по `drain`.
- **Ошибки без обработчика.** Событие `error` потока без слушателя **бросает исключение** и роняет процесс (в опыте при `r.destroy(new Error('r failed'))` для `Readable` без обработчика процесс завершился с `Unhandled 'error' event`).
- **`pipe` и `pipeline`.** `pipe` только перекладывает данные и не закрывает приёмник при ошибке источника (в опыте `destroyed: false`); `pipeline` уничтожает все звенья и отклоняет промис.
- **`Buffer` — `Uint8Array`.** Это «окно» над `ArrayBuffer`; строки в UTF-8 занимают переменное число байт (в опыте `'héllo'`: 5 символов, 6 байт).
- **`readFile` с `'utf8'`.** Недопустимые байты заменяются на `U+FFFD` без ошибки (в опыте `[0xff, 0xfe, 0x41]` → `"��A"`): бинарные данные читают без кодировки.
- **`process.exit()`.** Немедленно завершает процесс, не дожидаясь отложенных операций; события `exit` синхронны. `exitCode` позволяет закончить естественно.
- **Консольный вывод.** `process.stdout` для файлов и каналов может быть асинхронным; поэтому `process.exit()` сразу после больших записей способен обрезать вывод.

> ⚠️ Проверить: размер и поведение пула потоков libuv, синхронность `process.stdout` в разных средах (терминал, файл, канал; Windows и Linux), а также потерю вывода при `process.exit()` по документации Node.js, в опытах не воспроизводились.

## Подвохи

### 1. `existsSync` и гонка

Файл может исчезнуть между проверкой и чтением: действуйте и обрабатывайте `ENOENT`.

### 2. `exec` и внедрение команд

Строка передаётся оболочке: `exec('ls ' + userInput)` с `; rm -rf …` выполнит лишнее. Используйте `execFile`/`spawn` с массивом аргументов.

### 3. `pipe` не передаёт ошибки

Приёмник остаётся открытым; используйте `pipeline`.

### 4. Поток без `error`-обработчика

Роняет процесс (`Unhandled 'error' event`).

### 5. `process.env` приводит к строке

`process.env.NEW_VAR = 42` → `'42'`; присвоенное `undefined` тоже становится строкой `'undefined'` (в опыте).

### 6. Длина строки ≠ длина в байтах

`'héllo'.length` — `5`, `Buffer.byteLength('héllo')` — `6`.

### 7. `readdir({ recursive: true })` возвращает относительные пути

С разделителем ОС: на Windows `x\y` (в опыте `[ 'x', 'x\\y' ]`). Приводите через `path.sep` или `path`.

### 8. `writeFile` перезаписывает

Без флага `'a'` содержимое файла заменяется.

### 9. `parseArgs` отдаёт строки

`--count=3` → `'3'`; числа приводите сами.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| `readFile` целиком | проще | память пропорциональна файлу |
| Потоки + `pipeline` | постоянная память, обратное давление | сложнее код |
| `for await` по потоку | простой синтаксис | нет тонкой настройки |
| Веб-потоки (`ReadableStream`) | стандарт, одинаково в браузере | другой API; в Node.js через `Readable.toWeb` |
| `exec` | команды оболочки | инъекции, лимит буфера |
| `execFile`/`spawn` | без оболочки, безопасно | нет `|` и `>` |
| Библиотеки (execa, commander, yargs) | удобство | зависимость; не запускались |

## Вопросы с собеседований

<details><summary>1. Зачем нужны потоки, если есть <code>readFile</code>?</summary>

Потоки обрабатывают данные порциями с постоянной памятью, а `readFile` загружает файл целиком.

→ [Продвинутая теория](advanced.ru.md#потоки)

</details>

<details><summary>2. Что такое обратное давление?</summary>

Механизм, при котором медленный приёмник останавливает источник: `write()` возвращает `false`, ждём `drain`.

→ [Продвинутая теория](advanced.ru.md#обратное-давление)

</details>

<details><summary>3. Подвох: чем <code>pipeline</code> лучше <code>pipe</code>?</summary>

`pipeline` пробрасывает ошибки, уничтожает все звенья и возвращает промис; `pipe` — нет.

→ [Подвох 3](#3-pipe-не-передаёт-ошибки)

</details>

<details><summary>4. Чем <code>exec</code> отличается от <code>execFile</code> и почему первый опасен?</summary>

`exec` запускает оболочку; пользовательский ввод в команде может добавить лишние команды (внедрение).

→ [Подвох 2](#2-exec-и-внедрение-команд)

</details>

<details><summary>5. Как узнать, где лежит скрипт в ES-модуле?</summary>

`import.meta.dirname` / `import.meta.filename` или `fileURLToPath(import.meta.url)`.

→ [Базовая теория](basics.ru.md#пути-модуль-path)

</details>

<details><summary>6. Подвох: почему <code>existsSync</code> перед чтением ненадёжен?</summary>

Между проверкой и чтением файл может исчезнуть (гонка); надёжнее ловить `ENOENT`.

→ [Подвох 1](#1-existssync-и-гонка)

</details>

<details><summary>7. Какого типа значения в <code>process.env</code>?</summary>

Только строки; отсутствующее — `undefined`.

→ [Подвох 5](#5-processenv-приводит-к-строке)

</details>

<details><summary>8. Как завершить программу с ошибкой правильно?</summary>

Задать `process.exitCode = 1` и дать завершиться естественно; `process.exit(1)` обрывает незавершённые операции.

→ [Базовая теория](basics.ru.md#коды-выхода)

</details>

<details><summary>9. Что произойдёт, если у потока нет обработчика <code>error</code>?</summary>

Событие `error` бросит исключение и уронит процесс.

→ [Подвох 4](#4-поток-без-error-обработчика)

</details>

<details><summary>10. Подвох: почему длина <code>Buffer</code> строки не совпадает с её <code>length</code>?</summary>

`length` строки — в кодовых единицах UTF-16, длина буфера — в байтах UTF-8 (`é` — 2 байта).

→ [Подвох 6](#6-длина-строки--длина-в-байтах)

</details>

<details><summary>11. Как безопасно запустить внешнюю программу с аргументами от пользователя?</summary>

`execFile(file, [аргументы])` или `spawn` без `shell`; не склеивать строку команды.

→ [Продвинутая теория](advanced.ru.md#дочерние-процессы)

</details>

<details><summary>12. Как разобрать аргументы CLI без зависимостей?</summary>

`util.parseArgs({ options, allowPositionals })`; значения — строки и булевы.

→ [Продвинутая теория](advanced.ru.md#аргументы-командной-строки-parseargs)

</details>

## Шпаргалка

```text
fsp.readFile/writeFile/appendFile/readdir({recursive})/mkdir({recursive})/rm({recursive,force})/stat/rename
path.join/resolve/basename/extname/parse; import.meta.dirname
await pipeline(src, transform, dest)   // из node:stream/promises
execFile(file, [args]) / spawn(file, [args]) — без оболочки; exec — оболочка
process.argv/env/exitCode/cwd(); node --env-file=.env app.js
util.parseArgs({ options, allowPositionals })
```

- Большие данные — потоки и `pipeline`.
- `exec` — только для доверенных строк.
- `exitCode` вместо `exit()`.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Цикл событий и потоки выполнения: [13 · Цикл событий и параллелизм](../13-event-loop-and-concurrency/expert.ru.md)
- Ошибки и коды Node.js: [14 · Ошибки и отладка](../14-errors-and-debugging/expert.ru.md)
- Аналог в Linux: [Linux](../../../linux/overview.ru.md)

## Источники

- [Node.js: Stream](https://nodejs.org/docs/latest-v24.x/api/stream.html)
- [Node.js: File system](https://nodejs.org/docs/latest-v24.x/api/fs.html)
- [Node.js: Child process](https://nodejs.org/docs/latest-v24.x/api/child_process.html)
- [Node.js: Process I/O](https://nodejs.org/docs/latest-v24.x/api/process.html#a-note-on-process-io)
- [OWASP: Command Injection](https://owasp.org/www-community/attacks/Command_Injection)
