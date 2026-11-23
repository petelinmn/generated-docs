---
title: "Квиз: ошибки и отладка"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 14-errors-and-debugging
part: 2
tags: [error, try-catch, error-cause, custom-error, stack-trace]
status: draft
updated: 2026-11-23
---

# Квиз: ошибки и отладка

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что вернёт `f()`?

```js
function f() { try { return 'try' } finally { console.log('finally runs') } }
```

- A) Выведет `finally runs`, вернёт `'try'`
- B) Вернёт `'try'`, `finally` не выполнится
- C) Выведет `finally runs`, вернёт `undefined`
- D) Бросит ошибку

<details><summary>Ответ</summary>

**A.** `finally` выполняется всегда, затем возвращается результат `try`.
→ [Базовая теория](basics.ru.md#throw-и-trycatchfinally)

</details>

## 2. 🟢 Что вернёт `f()`?

```js
function f() { try { throw new Error('lost') } finally { return 'finally wins' } }
```

- A) Бросит `Error: lost`
- B) `'finally wins'`
- C) `undefined`
- D) `'lost'`

<details><summary>Ответ</summary>

**B.** `return` в `finally` перекрывает исход `try` и проглатывает ошибку.
→ [Подвох 1](expert.ru.md#1-return-в-finally-проглатывает-ошибку)

</details>

## 3. 🟢 Что напечатает `Object.keys(new Error('x'))`?

- A) `[ 'message' ]`
- B) `[ 'message', 'stack' ]`
- C) `[]`
- D) `[ 'name', 'message' ]`

<details><summary>Ответ</summary>

**C.** Собственные `message` и `stack` не перечисляются.
→ [Базовая теория](basics.ru.md#объект-error)

</details>

## 4. 🟢 Какую ошибку бросает `undefinedVar` для необъявленной переменной?

- A) `TypeError`
- B) `ReferenceError`
- C) `SyntaxError`
- D) `RangeError`

<details><summary>Ответ</summary>

**B.** `ReferenceError: undefinedVar is not defined`.
→ [Базовая теория](basics.ru.md#встроенные-типы-ошибок)

</details>

## 5. 🟡 Что выведет код?

```js
class Plain extends Error { }
console.log(new Plain('x').name, String(new Plain('x')));
```

- A) `Plain Plain: x`
- B) `Error Error: x`
- C) `Error Plain: x`
- D) `Plain Error: x`

<details><summary>Ответ</summary>

**B.** `name` остаётся `Error`, пока не задан явно: `this.name = this.constructor.name`.
→ [Продвинутая теория](advanced.ru.md#свои-классы-ошибок)

</details>

## 6. 🟡 Что выведет `JSON.stringify(new Error('boom'))`?

- A) `{"message":"boom"}`
- B) `{}`
- C) `"Error: boom"`
- D) `{"name":"Error","message":"boom"}`

<details><summary>Ответ</summary>

**B.** Поля не перечисляются; нужен `toJSON` или явное копирование.
→ [Продвинутая теория](advanced.ru.md#сериализация-ошибок)

</details>

## 7. 🟡 Что покажет `cause`?

```js
try { try { throw new Error('first') } catch (e) { throw new Error('second') } } catch (e) { console.log(e.message, e.cause) }
```

- A) `second Error: first`
- B) `second undefined`
- C) `first undefined`
- D) `second second`

<details><summary>Ответ</summary>

**B.** Без `{ cause: e }` исходная ошибка теряется.
→ [Продвинутая теория](advanced.ru.md#цепочка-причин-cause)

</details>

## 8. 🟡 Что произойдёт при `emit('error', new Error('x'))` на `EventEmitter` без слушателя `error`?

- A) Ничего
- B) Исключение будет брошено
- C) Событие уйдёт в `console.error`
- D) Промис будет отклонён

<details><summary>Ответ</summary>

**B.** Событие `error` без слушателя бросает ошибку (в опыте `emit error throws: no listener`).
→ [Продвинутая теория](advanced.ru.md#асинхронные-ошибки-и-обработчики-верхнего-уровня)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) `try/catch` ловит ошибки, брошенные в колбэке `setTimeout`, если он создан внутри `try`
- [ ] B) `catch { … }` без параметра допустим
- [ ] C) `Promise.any` бросает `AggregateError`, если отклонены все
- [ ] D) Хорошая практика — продолжать работу после `uncaughtException`

<details><summary>Ответ</summary>

**B, C.** A неверно: колбэк выполняется в другом стеке; D неверно: состояние программы неизвестно, нужно залогировать и завершиться.
→ [Продвинутая теория](advanced.ru.md#асинхронные-ошибки-и-обработчики-верхнего-уровня)

</details>

## 10. 🔴 Найдите ошибку

> «Ошибка, созданная заранее и брошенная позже из другой функции, покажет в стеке место броска».

<details><summary>Ответ</summary>

Кадры стека фиксируются при **создании** объекта: стек покажет место `new Error`, а не `throw`.
→ [Подвох 5](expert.ru.md#5-стек-указывает-на-место-создания)

</details>

## 11. 🔴 Что даст `new Error('m', {})` и `new Error('m', { cause: undefined })` для `'cause' in e`?

- A) Оба `true`
- B) Оба `false`
- C) Первый `false`, второй `true`
- D) Первый `true`, второй `false`

<details><summary>Ответ</summary>

**C.** Свойство `cause` создаётся, только если в опциях есть ключ `cause`, даже со значением `undefined`.
→ [Подвох 6](expert.ru.md#6-исход--cause--зависит-от-ключа)

</details>

## 12. 🔴 Что вернёт `foreign instanceof Error` для ошибки, созданной в другом контексте (`vm.runInNewContext('new Error("x")')`)?

- A) `true`
- B) `false`; для проверки подходит `Error.isError`
- C) `TypeError`
- D) `null`

<details><summary>Ответ</summary>

**B.** У другого контекста свой `Error`. `Error.isError(foreign)` вернёт `true`.
→ [Подвох 4](expert.ru.md#4-instanceof-error-не-работает-для-чужого-контекста)

</details>

## 13. 🔴 Открытый вопрос

Спроектируйте обработку ошибок для небольшого сервиса: какие классы ошибок, как сохранять контекст, что делать в обработчиках верхнего уровня.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Базовый класс `AppError` и подклассы (`NotFound`, `ValidationError`) с `name`, `code`/`status` и `toJSON`.
- Оборачивание с `{ cause }`: контекст добавляется, исходная ошибка сохраняется.
- Ловля по типу в одном месте (например, в обработчике запросов); неизвестные ошибки пробрасываются.
- Обработчики `uncaughtException` и `unhandledRejection` — логирование и завершение процесса; клиенту не отдают `stack`.

</details>

→ [Продвинутая теория](advanced.ru.md)
