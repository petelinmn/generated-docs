---
title: "Node.js: файлы, потоки, процессы"
lang: ru
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

# Node.js: файлы, потоки, процессы

🌐 **Русский** · [English](overview.en.md)

## Коротко

Node.js превращает JavaScript в язык для серверов и утилит. Три опоры: файловая система (`node:fs`, `node:path`), потоки (streams) для обработки данных по частям и дочерние процессы (`node:child_process`) для запуска других программ. Вместе с `process` (аргументы, окружение, коды выхода) они позволяют писать консольные инструменты и сервисы.

## Ключевые идеи

- **У `fs` три стиля:** колбэки, синхронные `*Sync` и промисы (`node:fs/promises`); для нового кода — промисы.
- **Пути собирают через `path`,** а не конкатенацией: разделители зависят от ОС.
- **Файл можно читать целиком или потоком:** большие файлы — только потоком.
- **Поток (stream)** отдаёт и принимает данные порциями (chunks); `pipeline` соединяет потоки и корректно обрабатывает ошибки.
- **Обратное давление (backpressure):** `write()` возвращает `false`, когда буфер полон; `pipeline` и `for await` учитывают это сами.
- **`Buffer`** — байты; строки в Node.js — в UTF-8.
- **`child_process`:** `execFile` и `spawn` запускают программы; `exec` использует оболочку и опасен с пользовательским вводом.
- **`process`:** `argv`, `env`, `exitCode`, `cwd()`; значения `process.env` — всегда строки.
- **`util.parseArgs`** разбирает аргументы командной строки без зависимостей.

## Минимальный пример

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

## Когда применять и когда нет

| Задача | Выбор |
|---|---|
| Небольшой файл целиком | `fsp.readFile` / `writeFile` |
| Большой файл, обработка по строкам или кускам | потоки, `readline`, `pipeline` |
| Запустить внешнюю программу | `execFile` / `spawn` (без оболочки) |
| Скрипт с несколькими командами оболочки | `exec` — только для доверенных строк |
| Разбор аргументов CLI | `util.parseArgs` |
| Секреты | переменные окружения или `.env` через `--env-file` |

- ❌ Не читайте огромные файлы целиком в память.
- ❌ Не подставляйте пользовательский ввод в `exec`.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `fs`, `path`, ошибки, `process`, окружение, коды выхода |
| [2. Продвинутая теория](advanced.ru.md) | потоки, `pipeline`, обратное давление, `Transform`, gzip, `readline`, `Buffer`, дочерние процессы, `parseArgs` |
| [3. Экспертная теория](expert.ru.md) | справочник, внутреннее устройство, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn javascript 16-nodejs-files-streams-processes level 5` |
| 6. Тесты | пока не создан — `/learn javascript 16-nodejs-files-streams-processes level 6` |

## Связанные материалы

- Пререквизиты: [09 · Модули](../09-modules/overview.ru.md), [12 · Промисы и async/await](../12-promises-and-async-await/overview.ru.md), [13 · Цикл событий и параллелизм](../13-event-loop-and-concurrency/overview.ru.md)
- Дальше: [17 · Браузер: DOM и события](../17-browser-dom-and-events/overview.ru.md)
- Аналоги в других треках: [Linux: файлы, процессы и каналы](../../../linux/overview.ru.md), [Python](../../../python/overview.ru.md) (`pathlib`, `subprocess`), [C#: ввод-вывод](../../../csharp/overview.ru.md)
