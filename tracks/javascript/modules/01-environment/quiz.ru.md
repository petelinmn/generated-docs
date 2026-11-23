---
title: "Квиз: окружение и инструменты"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 01-environment
part: 1
tags: [nodejs, npm, esm, commonjs, process-argv]
status: draft
updated: 2026-11-23
---

# Квиз: окружение и инструменты

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что выведет `node -p "1 + 1"`?

- A) Ничего, `-p` только выполняет код
- B) `2`
- C) `11`
- D) Ошибку

<details><summary>Ответ</summary>

**B.** `-p` печатает результат выражения, а `-e` лишь выполняет код.
→ [Базовая теория](basics.ru.md#три-способа-выполнить-код)

</details>

## 2. 🟢 `node args.js a --flag=1`. Что в `process.argv.slice(2)`?

- A) `['node', 'args.js', 'a', '--flag=1']`
- B) `['a', '--flag=1']`
- C) `['args.js', 'a', '--flag=1']`
- D) `[]`

<details><summary>Ответ</summary>

**B.** Первые два элемента `process.argv` — пути к Node.js и к скрипту.
→ [Базовая теория](basics.ru.md#три-способа-выполнить-код)

</details>

## 3. 🟢 Какое поле `package.json` определяет формат `.js`-файлов?

- A) `main`
- B) `type`
- C) `format`
- D) `engines`

<details><summary>Ответ</summary>

**B.** `"type": "module"` — ESM, `"commonjs"` — CommonJS.
→ [Базовая теория](basics.ru.md#packagejson)

</details>

## 4. 🟢 В проекте с `"type": "module"` написан `require('node:fs')`. Результат?

- A) Работает
- B) `ReferenceError: require is not defined in ES module scope`
- C) `SyntaxError`
- D) Предупреждение

<details><summary>Ответ</summary>

**B.** Для CommonJS переименуйте файл в `.cjs` или используйте `import`.
→ [Базовая теория](basics.ru.md#es-модули-и-commonjs)

</details>

## 5. 🟡 Как получить путь к текущей папке в ESM?

- A) `__dirname`
- B) `import.meta.dirname`
- C) `process.argv[1]`
- D) `module.path`

<details><summary>Ответ</summary>

**B.** `__dirname` и `__filename` существуют только в CommonJS.
→ [Продвинутая теория](advanced.ru.md#esm-и-commonjs-различия)

</details>

## 6. 🟡 Что произойдёт при `import { x } from './lib'` в ESM, если есть файл `lib.js`?

- A) Подключится `lib.js`
- B) `ERR_MODULE_NOT_FOUND` с подсказкой `Did you mean to import "./lib.js"?`
- C) Подключится `lib.json`
- D) Тихо вернётся `undefined`

<details><summary>Ответ</summary>

**B.** Загрузчик ESM не подбирает расширения.
→ [Продвинутая теория](advanced.ru.md#импорт-файлов-и-json)

</details>

## 7. 🟡 Код `undeclared = 5;` выполняется в `strict.js` (ESM) и в `strict.cjs`. Результат?

- A) Одинаковый в обоих
- B) В ESM `ReferenceError`, в `.cjs` создаётся глобальная переменная
- C) В ESM создаётся глобальная переменная, в `.cjs` `ReferenceError`
- D) В обоих `ReferenceError`

<details><summary>Ответ</summary>

**B.** ESM всегда строгий, CommonJS по умолчанию нестрогий.
→ [Продвинутая теория](advanced.ru.md#строгий-режим)

</details>

## 8. 🟡 Выберите все верные утверждения

- [ ] A) `process.exitCode = 3` позволяет программе завершиться штатно с кодом 3
- [ ] B) `process.exit(7)` завершает процесс немедленно
- [ ] C) `fetch` в Node.js 24 отсутствует
- [ ] D) `window` в Node.js не определён

<details><summary>Ответ</summary>

**A, B, D.** C неверно: в Node.js 24 `fetch` — функция (в опыте `typeof fetch` равен `function`).
→ [Продвинутая теория](advanced.ru.md#глобальные-объекты-nodejs)

</details>

## 9. 🔴 Найдите ошибку

> «`--env-file=.env` с `N=5` даст в `process.env.N` число 5, поэтому `process.env.N + 1` равно 6».

<details><summary>Ответ</summary>

Переменные окружения всегда строки: `process.env.N + 1` — это `'51'`. Числа нужно приводить явно (`Number(...)`).
→ [Подвох 5](expert.ru.md#5---env-file-даёт-только-строки)

</details>

## 10. 🔴 Почему на верхнем уровне CommonJS-файла `arguments.length` равен 5?

- A) Ошибка Node.js
- B) Файл выполняется как тело функции с пятью параметрами: `exports`, `require`, `module`, `__filename`, `__dirname`
- C) В файле пять строк
- D) Это число аргументов командной строки

<details><summary>Ответ</summary>

**B.** Это и есть обёртка CommonJS; у ESM такой обёртки нет.
→ [Внутреннее устройство](expert.ru.md#внутреннее-устройство)

</details>

## 11. 🔴 Открытый вопрос

Объясните, почему один и тот же файл может по-разному работать как `.js` в проекте с `"type": "module"` и как `.cjs`.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Формат файла задаётся расширением или полем `type` ближайшего `package.json`.
- ESM: строгий режим, `import`, `await` на верхнем уровне, `this` равен `undefined`, нет `require` и `__dirname`.
- CommonJS: обёртка-функция, `require`, `module.exports`, нестрогий режим по умолчанию.

</details>

→ [Экспертная теория](expert.ru.md#компромиссы-и-альтернативы)
