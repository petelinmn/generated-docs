---
title: "Квиз: модули"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 09-modules
part: 2
tags: [esm, import, export, commonjs, package-exports]
status: draft
updated: 2026-11-23
---

# Квиз: модули

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Сколько раз выполнится тело `counter.js`, если его импортируют `main.js` и `a.js`?

- A) Дважды
- B) Один раз
- C) Каждый раз при обращении к имени
- D) Зависит от порядка

<details><summary>Ответ</summary>

**B.** Модуль выполняется один раз, результат кэшируется по адресу файла.
→ [Базовая теория](basics.ru.md#export-и-import)

</details>

## 2. 🟢 Что произойдёт при `import { count } from './counter.js'; count = 5;`?

- A) `count` станет `5`
- B) `TypeError: Assignment to constant variable.`
- C) `SyntaxError`
- D) Ничего

<details><summary>Ответ</summary>

**B.** Импортированное имя нельзя присвоить: это ссылка на привязку экспортёра.
→ [Базовая теория](basics.ru.md#export-и-import)

</details>

## 3. 🟢 Модуль `counter.js` экспортирует `let count` и функцию `inc()`. Что увидит импортёр после `inc()`?

- A) Старое значение (копию)
- B) Новое значение: привязки живые
- C) `undefined`
- D) Ошибку

<details><summary>Ответ</summary>

**B.** Импорт — живая ссылка на переменную экспортёра.
→ [Базовая теория](basics.ru.md#export-и-import)

</details>

## 4. 🟢 В каком порядке выведется текст?

```js
console.log('1 main body');
import './side.js';          // side.js печатает 'side evaluated first'
console.log('2 after import statement');
```

- A) `1`, `side`, `2`
- B) `side`, `1`, `2`
- C) `1`, `2`, `side`
- D) `side`, `2`, `1`

<details><summary>Ответ</summary>

**B.** Зависимости выполняются раньше тела модуля, независимо от места `import`.
→ [Базовая теория](basics.ru.md#порядок-выполнения)

</details>

## 5. 🟡 Что содержит `Object.keys` для `export * from './counter.js'`, если у `counter.js` есть и именованные экспорты, и `default`?

- A) Всё, включая `default`
- B) Только именованные, без `default`
- C) Только `default`
- D) Пусто

<details><summary>Ответ</summary>

**B.** `export *` не переносит `default`. Нужно `export { default } from …`.
→ [Базовая теория](basics.ru.md#реэкспорт)

</details>

## 6. 🟡 Что вернёт `await import('./counter.js') === await import('./counter.js')`?

- A) `false`
- B) `true`
- C) `TypeError`
- D) Зависит от размера файла

<details><summary>Ответ</summary>

**B.** Динамический `import()` возвращает тот же объект пространства имён из кэша.
→ [Базовая теория](basics.ru.md#динамический-import)

</details>

## 7. 🟡 `lib.cjs`: `module.exports = { x: 1 }`. Что произойдёт при `import { x } from './lib.cjs'`?

- A) Вернётся `1`
- B) `SyntaxError: Named export 'x' not found...`
- C) `undefined`
- D) `ERR_MODULE_NOT_FOUND`

<details><summary>Ответ</summary>

**B.** Имена `module.exports = {…}` не распознаются статически; используйте `import lib from './lib.cjs'`.
→ [Продвинутая теория](advanced.ru.md#взаимодействие-с-commonjs)

</details>

## 8. 🟡 Что получится при `import('mylib/internal/int.js')`, если в `exports` пакета этот путь не перечислен?

- A) Файл загрузится
- B) `ERR_PACKAGE_PATH_NOT_EXPORTED`
- C) `ERR_MODULE_NOT_FOUND`
- D) `undefined`

<details><summary>Ответ</summary>

**B.** Поле `exports` закрывает все неперечисленные пути, даже если файл существует.
→ [Продвинутая теория](advanced.ru.md#packagejson-exports-и-imports)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) Из ESM можно импортировать CommonJS-файл по умолчанию
- [ ] B) В ESM доступны `__dirname` и `__filename`
- [ ] C) `createRequire(import.meta.url)` даёт `require` внутри ESM
- [ ] D) Импортированные привязки можно переприсваивать

<details><summary>Ответ</summary>

**A, C.** B и D неверны: вместо `__dirname` — `import.meta.dirname`, а присваивание импорту запрещено.
→ [Продвинутая теория](advanced.ru.md#взаимодействие-с-commonjs)

</details>

## 10. 🔴 Найдите ошибку

> «`import { which } from 'mylib'` в ESM и `require('mylib').which` в CommonJS вернут один и тот же экземпляр модуля, ведь это один пакет».

<details><summary>Ответ</summary>

При условных `exports` ESM и CommonJS получают разные файлы, то есть разные экземпляры со своим состоянием (dual package hazard): в опыте `w1 === w2` — `false`.
→ [Подвох 5](expert.ru.md#5-двойная-загрузка-пакета)

</details>

## 11. 🔴 Что произойдёт, если модуль на верхнем уровне делает `await` на 50 мс, а другой модуль его импортирует?

- A) Импортёр выполнится сразу и не увидит результат
- B) Импортёр дождётся завершения `await`
- C) Ошибка `TypeError`
- D) Импорт будет проигнорирован

<details><summary>Ответ</summary>

**B.** В опыте `after slow` напечатано после `slow done`.
→ [Базовая теория](basics.ru.md#await-на-верхнем-уровне)

</details>

## 12. 🔴 Что выведет код при цикле `ca.js` ↔ `cb.js`, если `cb.js` на верхнем уровне читает `a` (`export const a = 'A'` в `ca.js`)?

- A) `A`
- B) `undefined`
- C) `ReferenceError: Cannot access 'a' before initialization`
- D) `SyntaxError`

<details><summary>Ответ</summary>

**C.** `cb.js` вычисляется первым, а `a` ещё в TDZ. Чтение внутри функции, вызванной позже, работает (`getA()`).
→ [Продвинутая теория](advanced.ru.md#циклические-зависимости)

</details>

## 13. 🔴 Открытый вопрос

Объясните, как загружаются ES-модули: этапы, кэш и «живые» привязки.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Этапы: построение графа (поиск и разбор файлов), связывание (создание привязок, проверка экспортов), выполнение (зависимости раньше зависящих).
- Кэш по адресу: каждый модуль выполняется один раз; повторный `import()` возвращает тот же объект.
- Привязки живые: импорт — ссылка на переменную экспортёра, присваивание запрещено.
- Циклы допустимы, но обращение к ещё не вычисленному `const` — `ReferenceError`; `await` на верхнем уровне задерживает зависимых.

</details>

→ [Экспертная теория](expert.ru.md#внутреннее-устройство)
