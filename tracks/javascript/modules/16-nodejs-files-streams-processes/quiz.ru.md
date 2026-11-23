---
title: "Квиз: Node.js — файлы, потоки, процессы"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 16-nodejs-files-streams-processes
part: 2
tags: [fs, streams, pipeline, child-process, process-env]
status: draft
updated: 2026-11-23
---

# Квиз: Node.js — файлы, потоки, процессы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какой стиль `fs` рекомендуется для нового кода?

- A) Синхронный `*Sync`
- B) Колбэки
- C) `node:fs/promises`
- D) Только потоки

<details><summary>Ответ</summary>

**C.** Промисы удобны с `async`/`await`; синхронные вызовы блокируют цикл событий.
→ [Базовая теория](basics.ru.md#три-стиля-fs)

</details>

## 2. 🟢 Что вернёт `fsp.readFile(path)` без указания кодировки?

- A) Строку
- B) `Buffer`
- C) Массив строк
- D) `undefined`

<details><summary>Ответ</summary>

**B.** Без кодировки возвращаются байты.
→ [Базовая теория](basics.ru.md#чтение-и-запись)

</details>

## 3. 🟢 Что станет с `process.env.NEW_VAR = 42`?

- A) Число `42`
- B) Строка `'42'`
- C) `undefined`
- D) Ошибка

<details><summary>Ответ</summary>

**B.** Значения `process.env` — всегда строки.
→ [Базовая теория](basics.ru.md#процесс-окружение-аргументы)

</details>

## 4. 🟢 Как правильно собрать путь?

- A) `dir + '/' + name`
- B) `path.join(dir, name)`
- C) `` `${dir}\${name}` ``
- D) `dir.concat(name)`

<details><summary>Ответ</summary>

**B.** `path.join` учитывает разделители ОС и нормализует путь.
→ [Базовая теория](basics.ru.md#пути-модуль-path)

</details>

## 5. 🟡 Чем `pipeline` лучше `readable.pipe(writable)`?

- A) Быстрее
- B) Пробрасывает ошибки, уничтожает все звенья и возвращает промис
- C) Работает только с файлами
- D) Ничем

<details><summary>Ответ</summary>

**B.** `pipe` не передаёт ошибки и не закрывает приёмник.
→ [Продвинутая теория](advanced.ru.md#pipeline)

</details>

## 6. 🟡 Что означает `false` из `writable.write(chunk)`?

- A) Запись не удалась
- B) Внутренний буфер заполнен: дождитесь `drain`
- C) Поток закрыт
- D) Данные потеряны

<details><summary>Ответ</summary>

**B.** Это сигнал обратного давления.
→ [Продвинутая теория](advanced.ru.md#обратное-давление)

</details>

## 7. 🟡 Сколько байт занимает строка `'héllo'` в `Buffer.from`?

- A) 5
- B) 6
- C) 10
- D) 4

<details><summary>Ответ</summary>

**B.** `é` — два байта в UTF-8: `length` строки 5, длина буфера 6.
→ [Продвинутая теория](advanced.ru.md#buffer)

</details>

## 8. 🟡 Какую функцию использовать для запуска программы с аргументами от пользователя?

- A) `exec('prog ' + arg)`
- B) `execFile('prog', [arg])`
- C) `eval`
- D) `exec` с кавычками

<details><summary>Ответ</summary>

**B.** Без оболочки аргумент не может добавить лишние команды.
→ [Продвинутая теория](advanced.ru.md#дочерние-процессы)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) `process.exitCode = 1` не прерывает программу
- [ ] B) `process.exit()` может прервать незавершённые операции
- [ ] C) `parseArgs` возвращает числа для `--count=3`
- [ ] D) В ES-модуле есть `import.meta.dirname`

<details><summary>Ответ</summary>

**A, B, D.** C неверно: значения — строки.
→ [Базовая теория](basics.ru.md#коды-выхода)

</details>

## 10. 🔴 Найдите ошибку

> «`if (fs.existsSync(f)) data = fs.readFileSync(f)` — безопасный способ читать файл, который может отсутствовать».

<details><summary>Ответ</summary>

Между проверкой и чтением файл может исчезнуть (гонка). Надёжнее читать и ловить `ENOENT`.
→ [Подвох 1](expert.ru.md#1-existssync-и-гонка)

</details>

## 11. 🔴 Что произойдёт при ошибке в `Readable` без обработчика `error`?

- A) Ошибка проглатывается
- B) Процесс упадёт с `Unhandled 'error' event`
- C) Поток закроется молча
- D) Ошибка станет предупреждением

<details><summary>Ответ</summary>

**B.** Событие `error` без слушателя бросает исключение.
→ [Подвох 4](expert.ru.md#4-поток-без-error-обработчика)

</details>

## 12. 🔴 Чему равен `code` при запуске несуществующей программы через `spawn`?

- A) `ENOENT` в событии `error`
- B) `0` в событии `exit`
- C) Исключение при вызове `spawn`
- D) `ERR_UNKNOWN`

<details><summary>Ответ</summary>

**A.** Ошибка приходит событием `error` (в опыте `spawn error ENOENT`); без обработчика процесс упадёт.
→ [Продвинутая теория](advanced.ru.md#дочерние-процессы)

</details>

## 13. 🔴 Открытый вопрос

Как обработать файл в несколько гигабайт (подсчёт строк с заданным словом) и сжать результат, не исчерпав память?

<details><summary>Ключевые мысли хорошего ответа</summary>

- Читать потоком (`createReadStream`), строки получать через `readline`; не использовать `readFile`.
- Фильтровать в `Transform` (objectMode) и записывать результат через сжатие (`createGzip`) в файл.
- Соединить звенья `pipeline` (ошибки, обратное давление, закрытие).
- Обработать `error` и коды выхода (`process.exitCode`), не использовать синхронные вызовы.

</details>

→ [Продвинутая теория](advanced.ru.md#потоки)
