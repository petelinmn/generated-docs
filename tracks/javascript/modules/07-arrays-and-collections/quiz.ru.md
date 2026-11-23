---
title: "Квиз: массивы и коллекции"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 07-arrays-and-collections
part: 1
tags: [array, sort, set, map, weakmap, typed-array]
status: draft
updated: 2026-11-23
---

# Квиз: массивы и коллекции

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что выведет `console.log(Array(3), Array.of(3))`?

- A) `[ 3 ] [ 3 ]`
- B) `[ <3 empty items> ] [ 3 ]`
- C) `[ 0, 0, 0 ] [ 3 ]`
- D) `[ undefined, undefined, undefined ] [ 3 ]`

<details><summary>Ответ</summary>

**B.** `Array(3)` создаёт три пустых слота, `Array.of(3)` — массив из одного элемента.
→ [Базовая теория](basics.ru.md#создание)

</details>

## 2. 🟢 Что станет с массивом `[1, 2, 3]` после `a.length = 1`?

- A) Ничего
- B) `[1]`
- C) `[1, <2 empty items>]`
- D) Ошибка

<details><summary>Ответ</summary>

**B.** Уменьшение `length` обрезает массив.
→ [Базовая теория](basics.ru.md#length-и-дырки)

</details>

## 3. 🟢 Что вернёт `[10, 9, 1].sort()`?

- A) `[1, 9, 10]`
- B) `[1, 10, 9]`
- C) `[10, 9, 1]`
- D) `[9, 10, 1]`

<details><summary>Ответ</summary>

**B.** По умолчанию сортировка строковая. Числовая: `sort((a, b) => a - b)`.
→ [Базовая теория](basics.ru.md#сортировка)

</details>

## 4. 🟢 Меняет ли `toSorted()` исходный массив?

- A) Да
- B) Нет, возвращает отсортированную копию
- C) Только для чисел
- D) Только для строк

<details><summary>Ответ</summary>

**B.** В отличие от `sort()`, который меняет массив и возвращает его же.
→ [Базовая теория](basics.ru.md#изменяющие-и-копирующие-методы)

</details>

## 5. 🟡 Что вернёт `[].reduce((a, b) => a + b)`?

- A) `0`
- B) `undefined`
- C) `TypeError: Reduce of empty array with no initial value`
- D) `NaN`

<details><summary>Ответ</summary>

**C.** С начальным значением `[].reduce(f, 0)` вернул бы `0`.
→ [Базовая теория](basics.ru.md#map-filter-reduce-flat)

</details>

## 6. 🟡 Чему равны `[NaN].indexOf(NaN)` и `[NaN].includes(NaN)`?

- A) `0` и `true`
- B) `-1` и `true`
- C) `-1` и `false`
- D) `0` и `false`

<details><summary>Ответ</summary>

**B.** `indexOf` использует строгое равенство, `includes` — SameValueZero.
→ [Базовая теория](basics.ru.md#поиск)

</details>

## 7. 🟡 Сколько элементов в `new Set([1, 2, 2, 3, NaN, NaN, 0, -0])`?

- A) `8`
- B) `7`
- C) `5`
- D) `4`

<details><summary>Ответ</summary>

**C.** Остаются `1, 2, 3, NaN, 0`: `NaN` равен `NaN`, а `0` и `-0` считаются одним значением.
→ [Продвинутая теория](advanced.ru.md#set)

</details>

## 8. 🟡 Что вернёт `JSON.stringify(new Map([[1, 2]]))`?

- A) `[[1,2]]`
- B) `{"1":2}`
- C) `{}`
- D) `TypeError`

<details><summary>Ответ</summary>

**C.** `Map` не сериализуется напрямую: используйте `JSON.stringify([...map])`.
→ [Продвинутая теория](advanced.ru.md#map)

</details>

## 9. 🟡 Что вернёт `new Uint8Array([250, 255, 256, -1, 1.9])`?

- A) `[250, 255, 256, -1, 1.9]`
- B) `[250, 255, 0, 255, 1]`
- C) `[250, 255, 255, 0, 2]`
- D) `RangeError`

<details><summary>Ответ</summary>

**B.** Значения заворачиваются по модулю 256, дробная часть отбрасывается. `Uint8ClampedArray` зажимает диапазон.
→ [Продвинутая теория](advanced.ru.md#типизированные-массивы)

</details>

## 10. 🟡 Выберите все верные утверждения

- [ ] A) `Map` принимает объекты в качестве ключей
- [ ] B) `WeakMap` можно перебрать через `for...of`
- [ ] C) `Set` сохраняет порядок вставки
- [ ] D) `WeakMap` принимает строку как ключ

<details><summary>Ответ</summary>

**A, C.** B неверно (нет итерации), D неверно: `TypeError: Invalid value used as weak map key`.
→ [Продвинутая теория](advanced.ru.md#weakmap-и-weakset)

</details>

## 11. 🔴 Найдите ошибку

> «`Array(2).map(() => 1)` вернёт `[1, 1]`».

<details><summary>Ответ</summary>

`Array(2)` — две дырки, а `map` пропускает дырки: результат `[ <2 empty items> ]`. Нужно `Array.from({ length: 2 }, () => 1)` или `Array(2).fill(1)`.
→ [Подвох 3](expert.ru.md#3-дырки-против-undefined)

</details>

## 12. 🔴 Что выведет код?

```js
const order = new Map(); order.set('b', 1).set('a', 2).set('b', 3);
console.log([...order]);
```

- A) `[['a', 2], ['b', 3]]`
- B) `[['b', 3], ['a', 2]]`
- C) `[['b', 1], ['a', 2], ['b', 3]]`
- D) `[['b', 1], ['a', 2]]`

<details><summary>Ответ</summary>

**B.** Повторное `set` обновляет значение, но не позицию ключа.
→ [Внутреннее устройство](expert.ru.md#внутреннее-устройство)

</details>

## 13. 🔴 Открытый вопрос

Когда выбрать `Map`, а когда обычный объект?

<details><summary>Ключевые мысли хорошего ответа</summary>

- `Map` — произвольные ключи (объекты, `NaN`), порядок вставки, `size`, частые добавления и удаления, нет коллизий с прототипом.
- Объект — фиксированный набор строковых полей, JSON, деструктуризация.
- Объект как словарь для внешних ключей опасен (`__proto__`); вариант — `Object.create(null)` или `Map`.

</details>

→ [Продвинутая теория](advanced.ru.md#map-или-обычный-объект)
