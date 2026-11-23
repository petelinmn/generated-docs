---
title: "Квиз: типы и значения"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 02-types-and-values
part: 1
tags: [types, typeof, coercion, equality, nan, nullish-coalescing]
status: draft
updated: 2026-11-23
---

# Квиз: типы и значения

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что выведет `typeof null`?

- A) `'null'`
- B) `'undefined'`
- C) `'object'`
- D) `'number'`

<details><summary>Ответ</summary>

**C.** Историческая особенность: `null` — примитив, но `typeof` возвращает `'object'`.
→ [Базовая теория](basics.ru.md#типы-и-typeof)

</details>

## 2. 🟢 Чему равно `0.1 + 0.2 === 0.3`?

- A) `true`
- B) `false`
- C) `NaN`
- D) Ошибка

<details><summary>Ответ</summary>

**B.** `0.1 + 0.2` равно `0.30000000000000004`: двоичные дроби приблизительны. Сравнивать нужно с допуском (`Number.EPSILON`).
→ [Базовая теория](basics.ru.md#числа)

</details>

## 3. 🟢 Как правильно проверить, что `x` — это `NaN`?

- A) `x === NaN`
- B) `x == NaN`
- C) `Number.isNaN(x)`
- D) `typeof x === 'NaN'`

<details><summary>Ответ</summary>

**C.** `NaN` не равен даже самому себе, поэтому A и B всегда ложны.
→ [Базовая теория](basics.ru.md#nan)

</details>

## 4. 🟢 Что вернёт `'5' + 3` и `'5' - 3`?

- A) `8` и `2`
- B) `'53'` и `2`
- C) `'53'` и `'53'`
- D) `8` и `'2'`

<details><summary>Ответ</summary>

**B.** `+` при наличии строки склеивает, `-` приводит операнды к числу.
→ [Базовая теория](basics.ru.md#приведение-типов-и-сравнение)

</details>

## 5. 🟢 Какое значение подставит `cfg.port ?? 3000`, если `cfg.port` равно `0`?

- A) `3000`
- B) `0`
- C) `undefined`
- D) Ошибка

<details><summary>Ответ</summary>

**B.** `??` реагирует только на `null` и `undefined`. `cfg.port || 3000` дало бы `3000`.
→ [Базовая теория](basics.ru.md#--и-)

</details>

## 6. 🟡 Что вернёт `1n === 1` и `1n == 1`?

- A) `true` и `true`
- B) `false` и `true`
- C) `true` и `false`
- D) Ошибку `TypeError`

<details><summary>Ответ</summary>

**B.** Типы разные (`bigint` и `number`), строгое сравнение ложно; нестрогое сравнивает по значению.
→ [Продвинутая теория](advanced.ru.md#bigint)

</details>

## 7. 🟡 Что вернёт `Boolean(new Boolean(false))`?

- A) `false`
- B) `true`
- C) `undefined`
- D) Ошибку

<details><summary>Ответ</summary>

**B.** `new Boolean(false)` — объект, а любой объект истинен.
→ [Продвинутая теория](advanced.ru.md#объекты-обёртки)

</details>

## 8. 🟡 Что делает `JSON.stringify({ a: undefined, c: NaN, e: [undefined] })`?

- A) `{"a":null,"c":null,"e":[null]}`
- B) `{"c":null,"e":[null]}`
- C) `{"a":"undefined","c":"NaN","e":["undefined"]}`
- D) Ошибку

<details><summary>Ответ</summary>

**B.** Свойство `undefined` пропускается, `NaN` превращается в `null`, `undefined` в массиве — в `null`.
→ [Продвинутая теория](advanced.ru.md#json-и-типы)

</details>

## 9. 🟡 Выберите все ложные значения

- [ ] A) `0n`
- [ ] B) `'0'`
- [ ] C) `[]`
- [ ] D) `NaN`

<details><summary>Ответ</summary>

**A, D.** `'0'`, `[]` и `{}` истинны. Ложных значений ровно восемь.
→ [Базовая теория](basics.ru.md#истинность-truthy-и-falsy)

</details>

## 10. 🟡 Функция `mutate(obj) { obj.n = 99; obj = { n: 0 }; }` вызвана с `o1 = { n: 1 }`. Чему равно `o1.n`?

- A) `1`
- B) `0`
- C) `99`
- D) `undefined`

<details><summary>Ответ</summary>

**C.** Параметр получил копию ссылки: изменение поля видно снаружи, переприсваивание параметра — нет.
→ [Базовая теория](basics.ru.md#значения-и-ссылки)

</details>

## 11. 🔴 Найдите ошибку

> «Для проверки, что значение — объект, достаточно `typeof x === 'object'`».

<details><summary>Ответ</summary>

Условие истинно и для `null`, и для массивов, дат. Нужно `x !== null && typeof x === 'object'`, для массивов — `Array.isArray(x)`.
→ [Подвох 1](expert.ru.md#1-typeof-null--object)

</details>

## 12. 🔴 Что выведет `[10, 9, 1].sort()`?

- A) `[1, 9, 10]`
- B) `[1, 10, 9]`
- C) `[10, 9, 1]`
- D) Ошибку

<details><summary>Ответ</summary>

**B.** По умолчанию `sort` сравнивает строковые представления. Числовой порядок: `sort((a, b) => a - b)`.
→ [Подвох 3](expert.ru.md#3-sort-по-умолчанию-сортирует-строки)

</details>

## 13. 🔴 Открытый вопрос

Объясните, почему рекомендуют использовать `===`, и назовите единственный разумный случай использования `==`.

<details><summary>Ключевые мысли хорошего ответа</summary>

- `==` приводит типы по сложным правилам (`[] == false`, `0 == ''`, `'0' == false`), результаты неочевидны.
- `===` сравнивает без приведения, поэтому предсказуем.
- Оправданный случай: `x == null` — одновременная проверка на `null` и `undefined`.

</details>

→ [Экспертная теория](expert.ru.md#алгоритм-нестрогого-сравнения)
