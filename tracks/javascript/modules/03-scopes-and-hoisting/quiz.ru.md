---
title: "Квиз: переменные и области видимости"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 03-scopes-and-hoisting
part: 1
tags: [let, const, var, hoisting, tdz, closure]
status: draft
updated: 2026-11-23
---

# Квиз: переменные и области видимости

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что выведет код?

```js
console.log(typeof v1, v1); var v1 = 5;
```

- A) `ReferenceError`
- B) `undefined undefined`
- C) `number 5`
- D) `object null`

<details><summary>Ответ</summary>

**B.** Объявление `var v1` всплыло, а значение присваивается позже: до этого `v1` равна `undefined`.
→ [Базовая теория](basics.ru.md#всплытие-hoisting)

</details>

## 2. 🟢 Что произойдёт, если прочитать `let x` до строки с объявлением?

- A) Получим `undefined`
- B) `ReferenceError: Cannot access 'x' before initialization`
- C) `SyntaxError`
- D) `TypeError`

<details><summary>Ответ</summary>

**B.** Переменная находится во временной мёртвой зоне (TDZ).
→ [Базовая теория](basics.ru.md#временная-мёртвая-зона-tdz)

</details>

## 3. 🟢 Что делает `const arr = [1]; arr.push(2);`?

- A) Ошибка: `const` запрещает изменения
- B) Работает: `arr` становится `[1, 2]`
- C) Создаёт копию массива
- D) Ошибка только в строгом режиме

<details><summary>Ответ</summary>

**B.** `const` запрещает переприсваивание (`arr = []` вызовет `TypeError`), но не изменение содержимого.
→ [Базовая теория](basics.ru.md#let-const-и-var)

</details>

## 4. 🟢 Видна ли переменная `leaked` после `if (true) { var leaked = 1; }`?

- A) Нет, она живёт только в блоке
- B) Да, у `var` область — функция, а не блок
- C) Только внутри `if`
- D) Ошибка при объявлении

<details><summary>Ответ</summary>

**B.** Если бы вместо `var` был `let`, имя за блоком было бы недоступно.
→ [Базовая теория](basics.ru.md#блочная-область)

</details>

## 5. 🟡 Что выведет код?

```js
const fv = []; for (var a = 0; a < 3; a++) fv.push(() => a);
console.log(fv.map(f => f()));
```

- A) `[0, 1, 2]`
- B) `[3, 3, 3]`
- C) `[2, 2, 2]`
- D) `[undefined, undefined, undefined]`

<details><summary>Ответ</summary>

**B.** У `var` одна переменная на весь цикл, к моменту вызова она равна `3`. С `let` результат `[0, 1, 2]`.
→ [Базовая теория](basics.ru.md#замыкание-в-цикле-var-и-let)

</details>

## 6. 🟡 Что вернёт вызов `expr()` до строки `var expr = function () {}`?

- A) `ReferenceError: expr is not defined`
- B) `TypeError: expr is not a function`
- C) `undefined`
- D) Вызов сработает

<details><summary>Ответ</summary>

**B.** `expr` всплыла со значением `undefined`. Для объявления функции (`function expr() {}`) вызов был бы допустим.
→ [Базовая теория](basics.ru.md#всплытие-hoisting)

</details>

## 7. 🟡 В ES-модуле: `var gv = 1; console.log(globalThis.gv);`. Что выведет?

- A) `1`
- B) `undefined`
- C) `null`
- D) Ошибку

<details><summary>Ответ</summary>

**B.** Переменные верхнего уровня файла принадлежат модулю. В `node -e` результат был бы `1`.
→ [Продвинутая теория](advanced.ru.md#область-модуля-и-глобальная-область)

</details>

## 8. 🟡 Выберите все верные утверждения

- [ ] A) В строгом режиме функция, объявленная в блоке, не видна за его пределами
- [ ] B) Все ветки `switch` делят одну блочную область
- [ ] C) `let` допускает повторное объявление в той же области
- [ ] D) Параметр `catch (e)` виден только внутри `catch`

<details><summary>Ответ</summary>

**A, B, D.** C неверно: повторное `let` — `SyntaxError`.
→ [Продвинутая теория](advanced.ru.md#функции-в-блоках)

</details>

## 9. 🟡 Что вернёт `g()`?

```js
let dflt = 'outer';
function g(a = dflt) { let dflt = 'body'; return a; }
```

- A) `'body'`
- B) `'outer'`
- C) `undefined`
- D) `ReferenceError`

<details><summary>Ответ</summary>

**B.** Значения по умолчанию вычисляются в области параметров и не видят переменные тела.
→ [Продвинутая теория](advanced.ru.md#область-параметров-по-умолчанию)

</details>

## 10. 🔴 Найдите ошибку

> «`typeof` безопасен: он никогда не бросает ошибку, даже для необъявленной переменной, значит, и в TDZ вернёт `'undefined'`».

<details><summary>Ответ</summary>

Для необъявленного имени `typeof` возвращает `'undefined'`, но внутри TDZ (`typeof c1` до `const c1`) бросает `ReferenceError`: привязка существует, но не инициализирована.
→ [Подвох 1](expert.ru.md#1-typeof-внутри-tdz-падает)

</details>

## 11. 🔴 Что выведет `for (const i = 0; i < 2; i++) {}`?

- A) Ничего, цикл выполнится
- B) `TypeError: Assignment to constant variable.`
- C) `SyntaxError`
- D) `ReferenceError`

<details><summary>Ответ</summary>

**B.** `i++` переприсваивает `const`. Работает `for (const x of [1, 2])` — привязка создаётся заново на каждой итерации.
→ [Подвох 3](expert.ru.md#3-const-в-цикле-for)

</details>

## 12. 🔴 Открытый вопрос

Объясните, как работает всплытие и чем TDZ отличается от всплытия `var`.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Все привязки области создаются до выполнения её кода; «поднимается» не текст, а создание привязок.
- `var` и функции инициализируются сразу (`undefined` или тело функции).
- `let`/`const`/`class` остаются неинициализированными до строки объявления — обращение к ним даёт `ReferenceError`.

</details>

→ [Экспертная теория](expert.ru.md#внутреннее-устройство)
