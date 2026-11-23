---
title: "Квиз: объекты и прототипы"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 05-objects-and-prototypes
part: 1
tags: [objects, prototype, destructuring, spread, freeze, structured-clone]
status: draft
updated: 2026-11-23
---

# Квиз: объекты и прототипы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что вернёт `Object.keys({ b: 1, 2: 1, a: 1, 1: 1 })`?

- A) `['b', '2', 'a', '1']`
- B) `['1', '2', 'b', 'a']`
- C) `['b', 'a', '1', '2']`
- D) `['a', 'b', '1', '2']`

<details><summary>Ответ</summary>

**B.** Целочисленные ключи идут первыми по возрастанию, затем строки в порядке добавления.
→ [Базовая теория](basics.ru.md#ключи-и-их-порядок)

</details>

## 2. 🟢 Что вернёт `'toString' in {}` и `Object.hasOwn({}, 'toString')`?

- A) `true` и `true`
- B) `true` и `false`
- C) `false` и `false`
- D) `false` и `true`

<details><summary>Ответ</summary>

**B.** `in` проверяет и цепочку прототипов (`toString` — у `Object.prototype`), `hasOwn` — только собственные свойства.
→ [Базовая теория](basics.ru.md#доступ-проверка-и-удаление)

</details>

## 3. 🟢 Что станет с `orig.n.v` после кода?

```js
const orig = { n: { v: 1 } };
const sp = { ...orig };
sp.n.v = 99;
```

- A) `1`
- B) `99`
- C) `undefined`
- D) Ошибка

<details><summary>Ответ</summary>

**B.** Spread копирует один уровень: вложенный объект `n` общий.
→ [Базовая теория](basics.ru.md#копирование-поверхностное)

</details>

## 4. 🟢 Что вернёт `const { z } = null;`?

- A) `undefined`
- B) `null`
- C) `TypeError`
- D) `ReferenceError`

<details><summary>Ответ</summary>

**C.** `TypeError: Cannot destructure property 'z' of 'null' as it is null.` Защитное значение: `const { z } = obj ?? {}`.
→ [Базовая теория](basics.ru.md#деструктуризация)

</details>

## 5. 🟡 `base = { v: 1 }`, `der = Object.create(base)`. Что выведет `der.v = 2; console.log(base.v, der.v);`?

- A) `2 2`
- B) `1 2`
- C) `1 1`
- D) `2 1`

<details><summary>Ответ</summary>

**B.** Запись создаёт собственное свойство у `der`, прототип не меняется.
→ [Базовая теория](basics.ru.md#прототипы-основы)

</details>

## 6. 🟡 Какой результат у `Object.freeze` на вложенном объекте `{ a: 1, nested: { b: 1 } }` после `frozen.nested.b = 2`?

- A) `TypeError`
- B) Значение изменилось: `freeze` поверхностный
- C) Значение не изменилось
- D) `nested` удалился

<details><summary>Ответ</summary>

**B.** Заморожены только свойства верхнего уровня.
→ [Продвинутая теория](advanced.ru.md#freeze-seal-и-preventextensions)

</details>

## 7. 🟡 Что делает `structuredClone` с функцией внутри объекта?

- A) Копирует функцию
- B) Пропускает свойство
- C) Бросает `DataCloneError`
- D) Заменяет на `null`

<details><summary>Ответ</summary>

**C.** `DataCloneError: f() {} could not be cloned.` `JSON.stringify` бы тихо пропустил свойство.
→ [Продвинутая теория](advanced.ru.md#глубокое-копирование)

</details>

## 8. 🟡 Что вернёт `JSON.parse(JSON.stringify({ d: new Date(0), u: undefined, m: new Map([[1, 2]]) }))`?

- A) `{ d: Date, u: undefined, m: Map }`
- B) `{ d: '1970-01-01T00:00:00.000Z', m: {} }`
- C) `{ d: 0, m: [[1, 2]] }`
- D) `TypeError`

<details><summary>Ответ</summary>

**B.** Дата стала строкой, `Map` — пустым объектом, `undefined` пропал.
→ [Продвинутая теория](advanced.ru.md#глубокое-копирование)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) `Object.keys` перечисляет унаследованные свойства
- [ ] B) `for...in` перечисляет унаследованные перечисляемые свойства
- [ ] C) `Object.create(null)` возвращает объект без прототипа
- [ ] D) `String(Object.create(null))` вернёт `'[object Object]'`

<details><summary>Ответ</summary>

**B, C.** A неверно (только собственные); D неверно: `TypeError: Cannot convert object to primitive value`.
→ [Продвинутая теория](advanced.ru.md#объекты-без-прототипа)

</details>

## 10. 🔴 Найдите ошибку

> «Положим массив в прототип: `const proto = { list: [] }` — у каждого потомка будет свой список».

<details><summary>Ответ</summary>

Массив один на всех: `a1.list.push(1)` изменяет список, который видит и `a2`. Изменяемое состояние нужно создавать в самом объекте.
→ [Подвох 5](expert.ru.md#5-изменяемое-состояние-в-прототипе-общее)

</details>

## 11. 🔴 `base2 = Object.freeze({ ro: 1 })`, `der2 = Object.create(base2)`. Что произойдёт при `der2.ro = 2` в строгом режиме?

- A) У `der2` появится собственное свойство `ro`
- B) `TypeError: Cannot assign to read only property 'ro'`
- C) Изменится `base2.ro`
- D) Ничего, молча

<details><summary>Ответ</summary>

**B.** Если унаследованное свойство не записываемо, присваивание отклоняется, а не создаёт собственное.
→ [Подвох 4](expert.ru.md#4-замороженный-прототип-блокирует-запись-у-потомка)

</details>

## 12. 🔴 Открытый вопрос

Объясните, как работают чтение и запись свойств через цепочку прототипов.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Чтение: ищется собственное свойство, затем у прототипа и далее до `null`; не найдено — `undefined`.
- Запись: создаёт (или меняет) собственное свойство; исключения — не-`writable` унаследованное свойство и аксессор.
- `hasOwn` и `keys` видят только собственные свойства, `in` и `for...in` — и унаследованные.

</details>

→ [Экспертная теория](expert.ru.md#внутреннее-устройство)
