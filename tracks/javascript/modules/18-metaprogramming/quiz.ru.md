---
title: "Квиз: метапрограммирование"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 18-metaprogramming
part: 3
tags: [proxy, reflect, symbol, tagged-template, weak-ref]
status: draft
updated: 2026-11-23
---

# Квиз: метапрограммирование

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Чему равно `Symbol('a') === Symbol('a')`?

- A) `true`
- B) `false`
- C) `TypeError`
- D) Зависит от описания

<details><summary>Ответ</summary>

**B.** Каждый символ уникален; для общего символа — `Symbol.for('a')`.
→ [Базовая теория](basics.ru.md#символы)

</details>

## 2. 🟢 Что выведет `Object.keys({ [Symbol('s')]: 1, x: 2 })`?

- A) `[ 'x' ]`
- B) `[ Symbol(s), 'x' ]`
- C) `[]`
- D) `[ 'x', 'Symbol(s)' ]`

<details><summary>Ответ</summary>

**A.** Символьные ключи не попадают в `Object.keys`; их видят `Object.getOwnPropertySymbols` и `Reflect.ownKeys`.
→ [Базовая теория](basics.ru.md#символы)

</details>

## 3. 🟢 Что делает `get`-ловушка?

- A) Перехватывает чтение свойства прокси
- B) Перехватывает вызов функции
- C) Перехватывает запись
- D) Создаёт прокси

<details><summary>Ответ</summary>

**A.** Запись перехватывает `set`, вызов — `apply`.
→ [Базовая теория](basics.ru.md#proxy-перехват-операций)

</details>

## 4. 🟢 Что вернёт `String.raw` для шаблона с текстом `a\nb` (обратная косая и буква `n`)?

- A) `a`, перенос строки, `b`
- B) `a\nb` как четыре символа (`\` и `n` сохраняются)
- C) Ошибку
- D) `anb`

<details><summary>Ответ</summary>

**B.** `String.raw` не обрабатывает escape-последовательности.
→ [Базовая теория](basics.ru.md#теговые-шаблонные-строки)

</details>

## 5. 🟡 Что произойдёт, если ловушка `set` вернёт `false` в ES-модуле?

- A) Запись молча проигнорируется
- B) `TypeError: 'set' on proxy: trap returned falsish…`
- C) Значение запишется
- D) Прокси отключится

<details><summary>Ответ</summary>

**B.** Код модуля строгий; в нестрогом режиме запись молча теряется.
→ [Экспертная теория](expert.ru.md#1-ловушка-set-без-return-true)

</details>

## 6. 🟡 Что выведет код?

```js
class Temp { constructor(c) { this.c = c } [Symbol.toPrimitive](hint) { return hint === 'string' ? `${this.c}°C` : this.c } }
const t = new Temp(20); console.log(`${t}`, +t, t + 1);
```

- A) `20°C 20 21`
- B) `[object Object] NaN [object Object]1`
- C) `20 20 21`
- D) `20°C 20 20°C1`

<details><summary>Ответ</summary>

**A.** Шаблон просит `hint = 'string'`, `+t` и `t + 1` — `'number'`/`'default'` (возвращается число).
→ [Продвинутая теория](advanced.ru.md#известные-символы-well-known-symbols)

</details>

## 7. 🟡 Что произойдёт с `new Proxy(new Map(), {}).set('a', 1)`?

- A) Запись выполнится
- B) `TypeError: Method Map.prototype.set called on incompatible receiver`
- C) `undefined`
- D) Зависнет

<details><summary>Ответ</summary>

**B.** У `Map` есть внутренний слот, у прокси — нет. Методы нужно привязывать к цели.
→ [Продвинутая теория](advanced.ru.md#ограничения-и-инварианты-прокси)

</details>

## 8. 🟡 Что вернёт `Reflect.set(Object.freeze({ a: 1 }), 'a', 2)`?

- A) `true`
- B) `false`
- C) `TypeError`
- D) `2`

<details><summary>Ответ</summary>

**B.** `Reflect.set` возвращает `boolean` и не бросает исключение.
→ [Базовая теория](basics.ru.md#reflect)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) `WeakMap` можно перебрать через `for...of`
- [ ] B) Ключ `WeakMap` — объект или не зарегистрированный символ
- [ ] C) `WeakRef.deref()` может вернуть `undefined`
- [ ] D) Колбэк `FinalizationRegistry` вызывается в предсказуемый момент

<details><summary>Ответ</summary>

**B, C.** A неверно: слабые коллекции не перебираются; D неверно: сборка мусора недетерминирована.
→ [Продвинутая теория](advanced.ru.md#слабые-ссылки)

</details>

## 10. 🔴 Найдите ошибку

> «`Object.defineProperty(o, 'x', { value: 1 })` создаёт обычное перечисляемое свойство, которое можно изменять».

<details><summary>Ответ</summary>

По умолчанию `writable`, `enumerable` и `configurable` — `false`: свойство неизменяемое и не видно в `Object.keys`.
→ [Подвох 8](expert.ru.md#8-defineproperty-создаёт-скрытое-неизменяемое-свойство)

</details>

## 11. 🔴 Что произойдёт?

```js
const frozen = Object.freeze({ x: 1 });
const lie = new Proxy(frozen, { get() { return 2 } });
lie.x;
```

- A) `2`
- B) `1`
- C) `TypeError` из-за нарушения инварианта
- D) `undefined`

<details><summary>Ответ</summary>

**C.** Движок сверяет результат ловушки с неизменяемым свойством цели.
→ [Продвинутая теория](advanced.ru.md#ограничения-и-инварианты-прокси)

</details>

## 12. 🔴 Сколько раз тег-функция получит **один и тот же** объект `strings`, если вызвать `` f`x` `` дважды из одного места кода (в цикле)?

- A) Каждый раз новый объект
- B) Один и тот же, он заморожен
- C) Зависит от значений подстановок
- D) Один и тот же, но изменяемый

<details><summary>Ответ</summary>

**B.** Массив `strings` создаётся один раз на место в коде и заморожен.
→ [Внутреннее устройство](expert.ru.md#внутреннее-устройство)

</details>

## 13. 🔴 Открытый вопрос

Объясните, как работает `Proxy` на уровне внутренних методов и какие ограничения у прокси при работе со встроенными типами и приватными полями.

<details><summary>Ключевые мысли хорошего ответа</summary>

- У каждого объекта есть внутренние методы (`[[Get]]`, `[[Set]]`, `[[OwnPropertyKeys]]`…); у прокси они вызывают ловушки или операцию над целью.
- Инварианты: ловушка не может нарушить гарантии цели (замороженные и неконфигурируемые свойства).
- Внутренние слоты (`Map`, `Set`, `Date`, `Promise`) и `#`-поля принадлежат настоящему объекту; `this` = прокси ломает методы: привязываем к цели, `Reflect.get(t, k, t)`.
- Прокси не равен цели, замедляет доступ; для обычной валидации хватает функций и классов.

</details>

→ [Экспертная теория](expert.ru.md#внутреннее-устройство)
