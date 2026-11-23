---
title: "Квиз: классы"
lang: ru
track: javascript
kind: module
doc: quiz
slug: 06-classes
part: 1
tags: [classes, inheritance, private-fields, super, static]
status: draft
updated: 2026-11-23
---

# Квиз: классы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что произойдёт при `Point()` без `new`, если `Point` — класс?

- A) Вернётся `undefined`
- B) `TypeError: Class constructor Point cannot be invoked without 'new'`
- C) Создастся глобальная переменная
- D) Вернётся объект

<details><summary>Ответ</summary>

**B.** Класс нельзя вызвать как обычную функцию.
→ [Базовая теория](basics.ru.md#класс-нельзя-вызвать-без-new)

</details>

## 2. 🟢 Где лежит метод `toString`, объявленный в теле класса?

- A) В каждом экземпляре
- B) На `Class.prototype`
- C) В самом классе как статический
- D) В `Object.prototype`

<details><summary>Ответ</summary>

**B.** Метод общий для всех экземпляров и неперечисляемый. Поля (`x = 0;`) лежат в экземпляре.
→ [Базовая теория](basics.ru.md#объявление-класса)

</details>

## 3. 🟢 Что вернёт `Object.keys(p)` для экземпляра с полями `x`, `y` и `#secret`?

- A) `['x', 'y', '#secret']`
- B) `['x', 'y']`
- C) `['#secret']`
- D) `[]`

<details><summary>Ответ</summary>

**B.** Приватные поля не видны ни в `Object.keys`, ни в `JSON.stringify`.
→ [Базовая теория](basics.ru.md#приватные-члены)

</details>

## 4. 🟢 Что произойдёт при `p.#secret` вне класса?

- A) `undefined`
- B) `TypeError`
- C) `SyntaxError`: программа не запустится
- D) Значение поля

<details><summary>Ответ</summary>

**C.** Обращение к приватному имени вне класса — ошибка разбора: `Private field '#secret' must be declared in an enclosing class`.
→ [Базовая теория](basics.ru.md#приватные-члены)

</details>

## 5. 🟡 Что выведет код?

```js
class Base { constructor() { this.init(); } init() { this.a = 'base'; } }
class Derived extends Base { b = 'field'; init() { this.a = 'derived'; this.seenB = this.b; } }
console.log(new Derived());
```

- A) `Derived { a: 'derived', seenB: 'field', b: 'field' }`
- B) `Derived { a: 'derived', seenB: undefined, b: 'field' }`
- C) `Derived { a: 'base', b: 'field' }`
- D) `ReferenceError`

<details><summary>Ответ</summary>

**B.** Поля наследника создаются после `super()`, а `init` вызван из конструктора `Base` раньше.
→ [Продвинутая теория](advanced.ru.md#порядок-инициализации-полей)

</details>

## 6. 🟡 Что произойдёт при `const { method } = new F(); method();`, если `method() { return this.v; }`?

- A) Вернётся значение `v`
- B) `TypeError: Cannot read properties of undefined (reading 'v')`
- C) `undefined`
- D) `ReferenceError`

<details><summary>Ответ</summary>

**B.** `this` потерян; тело класса строгое. Решения: поле-стрелка или `bind`.
→ [Продвинутая теория](advanced.ru.md#потеря-this-и-поля-стрелки)

</details>

## 7. 🟡 Что вернёт `s.map(x => x) instanceof Stack` для `class Stack extends Array`?

- A) `false`
- B) `true`
- C) `TypeError`
- D) Зависит от длины

<details><summary>Ответ</summary>

**B.** `map` создаёт результат через «видовой» конструктор, то есть `Stack`.
→ [Продвинутая теория](advanced.ru.md#расширение-встроенных-классов)

</details>

## 8. 🟡 Чему равны `B.s` и `Object.hasOwn(B, 's')` для `class A { static s = 'A.s' }`, `class B extends A {}`?

- A) `'A.s'` и `true`
- B) `'A.s'` и `false`
- C) `undefined` и `false`
- D) `'A.s'` и `undefined`

<details><summary>Ответ</summary>

**B.** Статические члены наследуются через цепочку прототипов конструкторов, собственного `s` у `B` нет.
→ [Продвинутая теория](advanced.ru.md#статическое-наследование-и-выражения-классов)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) Поле со стрелкой создаётся заново в каждом экземпляре
- [ ] B) `#x in obj` не падает для чужих объектов
- [ ] C) Класс всплывает как объявление функции
- [ ] D) `new.target` в конструкторе показывает, на каком классе вызван `new`

<details><summary>Ответ</summary>

**A, B, D.** C неверно: до объявления класс находится в TDZ.
→ [Продвинутая теория](advanced.ru.md#потеря-this-и-поля-стрелки)

</details>

## 10. 🔴 Найдите ошибку

> «Конструктор наследника может сначала записать `this.x = 1`, а `super()` вызвать в конце — порядок не важен».

<details><summary>Ответ</summary>

В конструкторе наследника `this` не существует, пока не вызван `super(...)`: `ReferenceError: Must call super constructor ...`. Вызывайте `super` первым.
→ [Справочник](expert.ru.md#правила-конструктора)

</details>

## 11. 🔴 Что даст вызов `X.prototype.m.call(5)` для метода класса в нестрогом файле `.cjs`, где `m() { return this; }`?

- A) Объект-обёртка `Number`
- B) Примитив `5`
- C) Глобальный объект
- D) `undefined`

<details><summary>Ответ</summary>

**B.** Тело класса всегда строгое: `this` не оборачивается и не подменяется.
→ [Внутреннее устройство](expert.ru.md#внутреннее-устройство)

</details>

## 12. 🔴 Открытый вопрос

Сравните классы и фабричные функции с замыканиями: когда что выбрать?

<details><summary>Ключевые мысли хорошего ответа</summary>

- Классы: ясная структура, `instanceof`, наследование, методы на прототипе (экономия памяти), приватные `#`-члены.
- Фабрики: истинная приватность через замыкание, нет проблем с `this`, но копия методов на объект и нет `instanceof`.
- Выбор зависит от числа объектов, нужды в наследовании и инструментов (TypeScript, фреймворки).

</details>

→ [Экспертная теория](expert.ru.md#компромиссы-и-альтернативы)
