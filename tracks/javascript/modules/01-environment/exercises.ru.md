---
title: "Упражнения: окружение и инструменты"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 01-environment
part: 1
tags: [nodejs, npm, esm, commonjs, node-test]
status: draft
updated: 2026-11-23
---

# Упражнения: окружение и инструменты

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Работайте в пустой папке; нужны Node.js 24 и npm.

## 1. 🟢 Аргументы командной строки

**Задача:** напишите `hello.js`, который печатает `Hello, <имя>`, где имя — первый аргумент, а без аргумента — `world`.

**Ожидаемый результат:** `node hello.js Ada` → `Hello, Ada`; `node hello.js` → `Hello, world`.

<details><summary>Подсказка</summary>

Пользовательские аргументы начинаются с индекса 2; значение по умолчанию задаёт `??`.

</details>

<details><summary>Решение</summary>

```js
console.log('Hello,', process.argv[2] ?? 'world');
```

```text
$ node hello.js Ada
Hello, Ada
```

</details>

## 2. 🟢 Проект с package.json и ESM

**Задача:** выполните `npm init -y`, переключите проект на модули ES и создайте `esm.js`, который печатает `typeof require` и значение `await Promise.resolve('ok')` на верхнем уровне.

**Ожидаемый результат:** `undefined` и `ok`.

<details><summary>Подсказка</summary>

`npm pkg set type=module`.

</details>

<details><summary>Решение</summary>

```js
console.log(typeof require);
console.log(await Promise.resolve('ok'));
```

```text
$ node esm.js
undefined
ok
```

</details>

## 3. 🟡 Один код, два формата

**Задача:** сохраните строку `undeclared = 5; console.log(typeof globalThis.undeclared);` в `a.js` (проект с `"type": "module"`) и в `a.cjs`. Сравните результаты и объясните.

**Ожидаемый результат:** `a.js` — `ReferenceError: undeclared is not defined`; `a.cjs` — печатает `number`.

<details><summary>Подсказка</summary>

ESM — строгий режим, CommonJS — нестрогий.

</details>

<details><summary>Решение</summary>

В ESM присваивание необъявленной переменной — ошибка строгого режима. В `.cjs` нестрогий режим создаёт глобальную переменную, и `typeof globalThis.undeclared` равен `number`.

```text
$ node a.js
ReferenceError: undeclared is not defined
$ node a.cjs
number
```

</details>

## 4. 🟡 Импорты

**Задача:** создайте `lib.js` (`export const x = 1;`) и `data.json` (`{"a": 1}`). Импортируйте `x` без расширения, затем с расширением; подключите JSON с атрибутом и без него.

**Ожидаемый результат:** `ERR_MODULE_NOT_FOUND` и подсказка `Did you mean to import "./lib.js"?`; `{ a: 1 }` с атрибутом и `ERR_IMPORT_ATTRIBUTE_MISSING` без него (динамический `import()`).

<details><summary>Подсказка</summary>

`import data from './data.json' with { type: 'json' }`.

</details>

<details><summary>Решение</summary>

Код и вывод — в [продвинутой теории](advanced.ru.md#импорт-файлов-и-json).

</details>

## 5. 🔴 Тесты, переменные окружения и код возврата

**Задача:** (а) добавьте тест `tests/a.test.js` на `node:test` и запустите `npm test` со скриптом `node --test`. (б) Создайте `.env` с `N=5` и покажите, что `process.env.N + 1` равно `'51'`. (в) Напишите скрипт, который завершается с кодом 3, и проверьте код в оболочке.

**Ожидаемый результат:** (а) `pass 1`; (б) `51`; (в) `exit=3`.

<details><summary>Подсказка</summary>

(б) `node --env-file=.env -p "process.env.N + 1"`; (в) `process.exitCode = 3`, затем `echo $?`.

</details>

<details><summary>Решение</summary>

```js
// tests/a.test.js
import { test } from 'node:test';
import assert from 'node:assert/strict';
test('sum', () => { assert.equal(1 + 1, 2); });
```

```text
$ npm test            # pass 1, fail 0
$ node --env-file=.env -p "process.env.N + 1"
51
$ node exitcode.js ; echo "exit=$?"
done
exit=3
```

Значения `process.env` — строки, поэтому `+ 1` склеивает `'5'` и `'1'`.

</details>
