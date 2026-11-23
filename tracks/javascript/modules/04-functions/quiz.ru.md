---
title: "Квиз: функции"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 04-functions
part: 1
tags: [functions, this, arrow-functions, bind, default-parameters]
status: draft
updated: 2026-11-23
---

# Квиз: функции

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что вернёт `decl(1)` для `function decl(a, b) { return a + b; }`?

- A) `1`
- B) `NaN`
- C) Ошибку: не хватает аргумента
- D) `undefined`

<details><summary>Ответ</summary>

**B.** Недостающий аргумент равен `undefined`, и `1 + undefined` — `NaN`.
→ [Базовая теория](basics.ru.md#параметры-и-аргументы)

</details>

## 2. 🟢 Что вернёт `dflt(1, null)` для `function dflt(a, b = 2) { return [a, b]; }`?

- A) `[1, 2]`
- B) `[1, null]`
- C) `[1, undefined]`
- D) Ошибку

<details><summary>Ответ</summary>

**B.** Значение по умолчанию подставляется только при `undefined`.
→ [Базовая теория](basics.ru.md#значения-по-умолчанию)

</details>

## 3. 🟢 Что вернёт функция?

```js
function f() { return
  42 }
```

- A) `42`
- B) `undefined`
- C) `null`
- D) `SyntaxError`

<details><summary>Ответ</summary>

**B.** После `return` автоматически вставляется `;`, поэтому `42` не достигается.
→ [Базовая теория](basics.ru.md#возврат-значения)

</details>

## 4. 🟢 Чему равно `['1','2','3'].map(parseInt)`?

- A) `[1, 2, 3]`
- B) `[1, NaN, NaN]`
- C) `['1','2','3']`
- D) Ошибка

<details><summary>Ответ</summary>

**B.** `map` передаёт индекс вторым аргументом, и `parseInt` считает его системой счисления.
→ [Базовая теория](basics.ru.md#функции-как-значения)

</details>

## 5. 🟡 Что вернёт `detached()`?

```js
const obj = { name: 'obj', m() { return this?.name; } };
const detached = obj.m;
```

- A) `'obj'`
- B) `undefined`
- C) `TypeError`
- D) `''`

<details><summary>Ответ</summary>

**B.** В строгом режиме `this` оторванного метода — `undefined`; `?.` предотвращает ошибку. Без `?.` была бы `TypeError`.
→ [Базовая теория](basics.ru.md#this-основы)

</details>

## 6. 🟡 Чему равно `bound()` после `const bound = obj.m.bind({ name: 'bound' })` и `bound.call({ name: 'x' })`?

- A) `'bound'` в обоих случаях
- B) `'bound'` и `'x'`
- C) `'obj'` и `'x'`
- D) `undefined` в обоих

<details><summary>Ответ</summary>

**A.** `bind` фиксирует `this` окончательно; `call` не может его изменить.
→ [Продвинутая теория](advanced.ru.md#call-apply-и-bind)

</details>

## 7. 🟡 Что произойдёт при `new arrow()`, где `arrow = (a, b) => a + b`?

- A) Создастся объект
- B) `TypeError: arrow is not a constructor`
- C) `undefined`
- D) `ReferenceError`

<details><summary>Ответ</summary>

**B.** У стрелок нет `[[Construct]]` и `prototype`.
→ [Продвинутая теория](advanced.ru.md#ограничения-стрелочных-функций)

</details>

## 8. 🟡 Чему равна длина `((a, b = 1, ...c) => {}).length`?

- A) `3`
- B) `2`
- C) `1`
- D) `0`

<details><summary>Ответ</summary>

**C.** `length` считает параметры до первого значения по умолчанию или `...rest`.
→ [Базовая теория](basics.ru.md#три-формы-функций)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) `arguments` — настоящий массив
- [ ] B) `...rest` — настоящий массив
- [ ] C) У стрелки нет собственного `this`
- [ ] D) Значение по умолчанию вычисляется один раз при определении функции

<details><summary>Ответ</summary>

**B, C.** A неверно (`Array.isArray(arguments)` — `false`); D неверно: умолчание вычисляется при каждом вызове.
→ [Базовая теория](basics.ru.md#остаточные-параметры-и-arguments)

</details>

## 10. 🟡 Что вернёт `strictThis.call(5)` в ES-модуле для `function strictThis() { return this; }`?

- A) Объект `Number`
- B) Примитив `5`
- C) `undefined`
- D) Глобальный объект

<details><summary>Ответ</summary>

**B.** В строгом режиме `this` не оборачивается. В нестрогом коде был бы объект-обёртка.
→ [Справочник](expert.ru.md#правила-this-в-порядке-приоритета)

</details>

## 11. 🔴 Найдите ошибку

> «Стрелочная функция — это просто короткая запись обычной, поэтому `obj = { m: () => this.name }` вернёт имя объекта».

<details><summary>Ответ</summary>

У стрелки нет собственного `this`: он берётся из окружающей области (в модуле — `undefined`), а не из `obj`. Для метода используйте `m() { return this.name }`.
→ [Подвох 5](expert.ru.md#5-стрелка-в-роли-метода)

</details>

## 12. 🔴 Что произойдёт при `Person('x')` без `new` для `function Person(name) { this.name = name; }` в строгом режиме?

- A) Создастся глобальное свойство `name`
- B) `TypeError: Cannot set properties of undefined (setting 'name')`
- C) Вернётся объект `Person`
- D) Ничего

<details><summary>Ответ</summary>

**B.** `this` равен `undefined`, присваивание свойства падает.
→ [Подвох 6](expert.ru.md#6-конструктор-без-new)

</details>

## 13. 🔴 Открытый вопрос

Перечислите, как определяется `this` в разных способах вызова, и объясните, почему метод, переданный как обратный вызов, ломается.

<details><summary>Ключевые мысли хорошего ответа</summary>

- `new` — новый объект; `bind` — зафиксированный; `call`/`apply` — заданный; `obj.f()` — `obj`; `f()` — `undefined` (строгий режим) или глобальный объект; стрелка — внешний `this`.
- Передача метода как значения отрывает его от объекта, поэтому `this` теряется; решение: `bind`, стрелка-обёртка или метод-поле класса.

</details>

→ [Экспертная теория](expert.ru.md#правила-this-в-порядке-приоритета)
