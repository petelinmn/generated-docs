---
title: "Node.js: файлы, потоки, процессы: основы"
lang: ru
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

# Node.js: файлы, потоки, процессы: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [09 · Модули](../09-modules/overview.ru.md), [12 · Промисы и async/await](../12-promises-and-async-await/overview.ru.md), [14 · Ошибки и отладка](../14-errors-and-debugging/overview.ru.md) · **Время:** ~2.5 ч

Все примеры запускались на Node.js 24.15 (Windows) в папке с `package.json` (`{"type": "module"}`). Пути в выводе приведены к `/`.

## Зачем это нужно

Почти любая программа на Node.js читает и пишет файлы, получает параметры и окружение, завершается с кодом. Это основа консольных утилит и сервисов.

## Три стиля fs

```js
import fs from 'node:fs';
import fsp from 'node:fs/promises';
const text = fs.readFileSync('a.txt', 'utf8');                 // синхронно
fs.readFile('a.txt', 'utf8', (err, data) => { /* колбэк */ });   // колбэки
const text2 = await fsp.readFile('a.txt', 'utf8');              // промисы
```

- **Синхронные** функции блокируют цикл событий ([модуль 13](../13-event-loop-and-concurrency/basics.ru.md#блокировка-цикла)): допустимы в скриптах запуска и CLI, но не в обработчиках сервера.
- **Колбэки** — старый стиль (`(err, data)`), сегодня почти не нужны.
- **`node:fs/promises`** — стиль по умолчанию для нового кода.

## Чтение и запись

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

- `writeFile` создаёт или **перезаписывает** файл, `appendFile` добавляет в конец.
- `readFile(путь, 'utf8')` возвращает строку; без кодировки — `Buffer` (байты, `length` `18`).
- `stat` даёт метаданные: `isFile()`, `isDirectory()`, `size`, `mtime`.
- `mkdtemp` создаёт временную папку с уникальным именем — удобно для тестов и скриптов.

Другие операции: `rename`, `copyFile`, `rm`, `access`. Запись с флагом `'a'`: `fs.writeFileSync(f, 'b', { flag: 'a' })` дописывает в конец (в опыте `a` + `b` → `ab`).

## Каталоги

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

- `mkdir(…, { recursive: true })` создаёт всю цепочку и не падает, если папка уже есть.
- `readdir` возвращает имена; `{ recursive: true }` — обход вглубь; `{ withFileTypes: true }` — объекты `Dirent` с `isDirectory()`.
- `JSON.parse(await readFile(…, 'utf8'))` — типичный способ прочитать JSON-файл.

Удаление: `await fsp.rm(dir, { recursive: true, force: true })`. Без `force` отсутствующий путь даёт `ENOENT`, а непустая папка без `recursive` — ошибку.

## Пути: модуль path

Пути собирают через `path`, а не строками: разделитель зависит от ОС.

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

(На Windows `path.join` выдаёт `b\c.txt`; в выводе приведено к `/`.)

- `join` соединяет и нормализует (`..` сокращается), `resolve` строит абсолютный путь от текущего каталога.
- `basename`, `extname`, `dirname`, `parse` разбирают путь; расширение у `file.tar.gz` — `.gz`.
- `path.posix` и `path.win32` дают поведение конкретной ОС независимо от текущей.
- В ES-модулях нет `__dirname`: используйте `import.meta.dirname` и `import.meta.filename` ([модуль 09](../09-modules/basics.ru.md)); из `import.meta.url` путь получают через `fileURLToPath`.

## Ошибки файловой системы

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

- Ошибки несут стабильный `code`: `ENOENT` (нет файла), `EEXIST` (уже есть), `EACCES` (нет прав), `EISDIR`, `ENOTEMPTY`.
- Проверка «есть ли файл» **перед** действием (`existsSync`, `access`) создаёт гонку: между проверкой и действием файл может измениться. Надёжнее выполнять действие и обрабатывать ошибку.

## Процесс, окружение, аргументы

```js
import process from 'node:process';
console.log(process.argv.length >= 2, typeof process.pid, process.platform, typeof process.cwd(), process.version.startsWith('v24'));
process.env.NEW_VAR = 42; console.log(typeof process.env.NEW_VAR, process.env.NEW_VAR, 'MISSING' in process.env, process.env.MISSING ?? 'default');
```

```text
true number win32 string true
string 42 false default
```

- `process.argv` — массив: путь к `node`, путь к скрипту, затем ваши аргументы.
- `process.env` — объект переменных окружения; значения **всегда строки** (число `42` превратилось в строку `'42'`), отсутствующая переменная — `undefined`.
- `process.cwd()` — текущий каталог, `process.platform` — `'win32'`, `'linux'`, `'darwin'`.
- Файл настроек: `node --env-file=.env app.js` загружает пары `КЛЮЧ=значение` в `process.env` (в опыте `API_KEY=secret123`, значения в кавычках без кавычек; комментарии пропускаются; отсутствующий файл — ошибка `not found`, а `--env-file-if-exists` его пропускает).

> Секреты храните в `.env` и добавьте файл в `.gitignore`: в репозиторий он попадать не должен.

## Коды выхода

```js
process.on('exit', c => console.log('exit', c));
process.exitCode = 2;   // задать код, не прерывая работу
// process.exit(1)       // немедленный выход: незавершённые операции потеряются
```

- `0` — успех, ненулевой код — ошибка; CI и оболочка опираются на него.
- Предпочитайте `process.exitCode = N` и естественное завершение: `process.exit()` обрывает работу и может потерять неотправленные данные.
- Событие `exit` получает итоговый код.

## Ошибки новичков

- ❌ Склеивать пути строками (`dir + '/' + name`) → ✅ `path.join`.
- ❌ Читать большой файл целиком → ✅ поток ([продвинутая теория](advanced.ru.md#потоки)).
- ❌ Синхронный `fs` в обработчике запросов → ✅ `fs/promises`.
- ❌ Проверять `existsSync`, а потом читать → ✅ читать и ловить `ENOENT`.
- ❌ Считать `process.env.PORT` числом → ✅ `Number(process.env.PORT)`.
- ❌ Коммитить `.env` с секретами → ✅ `.gitignore`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — потоки, `pipeline`, дочерние процессы
- Ранее: [14 · Ошибки и отладка](../14-errors-and-debugging/advanced.ru.md#коды-ошибок-nodejs)

## Источники

- [Node.js: File system](https://nodejs.org/docs/latest-v24.x/api/fs.html)
- [Node.js: Path](https://nodejs.org/docs/latest-v24.x/api/path.html)
- [Node.js: Process](https://nodejs.org/docs/latest-v24.x/api/process.html)
- [Node.js: Environment variables (`--env-file`)](https://nodejs.org/docs/latest-v24.x/api/cli.html#--env-fileconfig)
