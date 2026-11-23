---
title: "Упражнения: объекты и прототипы"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 05-objects-and-prototypes
part: 1
tags: [objects, prototype, destructuring, spread, structured-clone, freeze]
status: draft
updated: 2026-11-23
---

# Упражнения: объекты и прототипы

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Запускайте код в файле ES-модуля.

## 1. 🟢 Деструктуризация

**Задача:** из объекта `{ id: 1, user: { name: 'Ann' }, tags: ['a', 'b', 'c'] }` одним выражением достаньте `id`, `name`, первый тег и остаток тегов. Для отсутствующего `role` задайте значение `'guest'`.

**Ожидаемый результат:** `1 Ann a [ 'b', 'c' ] guest`.

<details><summary>Подсказка</summary>

Вложенный шаблон `user: { name }`, массив `tags: [first, ...others]`, умолчание `role = 'guest'`.

</details>

<details><summary>Решение</summary>

```js
const { id, user: { name }, tags: [first, ...others], role = 'guest' } =
  { id: 1, user: { name: 'Ann' }, tags: ['a', 'b', 'c'] };
console.log(id, name, first, others, role);
```

```text
1 Ann a [ 'b', 'c' ] guest
```

</details>

## 2. 🟢 Словарь частот

**Задача:** по строке `'abracadabra'` постройте объект «символ → число вхождений» и отсортируйте пары по убыванию.

**Ожидаемый результат:** `[ [ 'a', 5 ], [ 'b', 2 ], [ 'r', 2 ], [ 'c', 1 ], [ 'd', 1 ] ]`.

<details><summary>Подсказка</summary>

`for...of` по строке, `freq[ch] = (freq[ch] ?? 0) + 1`, затем `Object.entries(...).sort(...)`.

</details>

<details><summary>Решение</summary>

```js
const freq = {};
for (const ch of 'abracadabra') freq[ch] = (freq[ch] ?? 0) + 1;
console.log(Object.entries(freq).sort((a, b) => b[1] - a[1]));
```

```text
[ [ 'a', 5 ], [ 'b', 2 ], [ 'r', 2 ], [ 'c', 1 ], [ 'd', 1 ] ]
```

Сортировка стабильна, поэтому пары с равным числом остаются в порядке добавления.

</details>

## 3. 🟡 Поверхностная и глубокая копия

**Задача:** сделайте копии объекта `{ n: { v: 1 }, d: new Date(0) }` двумя способами: `{ ...o }` и `structuredClone`. Измените `n.v` в копиях и покажите, у какого из способов исходный объект изменился.

**Ожидаемый результат:** при spread исходный `n.v` стал `99`; при `structuredClone` остался `1`.

<details><summary>Подсказка</summary>

Сравните значения после присваивания `copy.n.v = 99`.

</details>

<details><summary>Решение</summary>

```js
const o = { n: { v: 1 }, d: new Date(0) };
const shallow = { ...o }; shallow.n.v = 99;
console.log(o.n.v);
const o2 = { n: { v: 1 }, d: new Date(0) };
const deep = structuredClone(o2); deep.n.v = 99;
console.log(o2.n.v, deep.d instanceof Date);
```

```text
99
1 true
```

</details>

## 4. 🟡 Защита объекта

**Задача:** создайте три объекта: заморожен, запечатан и нерасширяем. Для каждого проверьте, можно ли изменить значение, добавить и удалить свойство (в строгом режиме).

**Ожидаемый результат:** для `freeze` все три операции дают ошибку; для `seal` меняется значение, а добавление и удаление — ошибка; для `preventExtensions` меняется значение и удаляется свойство, а добавление — ошибка.

<details><summary>Подсказка</summary>

Оберните операции в `try`/`catch`.

</details>

<details><summary>Решение</summary>

См. код и вывод в [продвинутой теории](advanced.ru.md#freeze-seal-и-preventextensions).

</details>

## 5. 🔴 Прототипы: наследование без классов

**Задача:** создайте объект `animal` с методом `speak()` и объект `dog` на его основе (`Object.create`) с полем `name` и методом `speak()`, который вызывает метод прототипа и дописывает `' (woof)'`. Покажите цепочку прототипов и убедитесь, что `speak` — не собственное свойство `Object.create(animal)`.

**Ожидаемый результат:** `Rex makes a sound (woof)`, `Object.getPrototypeOf(dog) === animal`, `Object.hasOwn(Object.create(animal), 'speak')` — `false`.

<details><summary>Подсказка</summary>

Вызов метода прототипа: `animal.speak.call(this)` или `Object.getPrototypeOf(dog).speak.call(this)`.

</details>

<details><summary>Решение</summary>

```js
const animal = { speak() { return `${this.name} makes a sound`; } };
const dog = Object.create(animal);
dog.name = 'Rex';
dog.speak = function () { return animal.speak.call(this) + ' (woof)'; };
console.log(dog.speak(), Object.getPrototypeOf(dog) === animal, Object.hasOwn(Object.create(animal), 'speak'));
```

```text
Rex makes a sound (woof) true false
```

В классах для этого есть `extends` и `super` (модуль 06 дорожной карты).

</details>
