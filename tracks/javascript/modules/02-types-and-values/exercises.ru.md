---
title: "Упражнения: типы и значения"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 02-types-and-values
part: 1
tags: [types, typeof, coercion, nan, nullish-coalescing, json]
status: draft
updated: 2026-11-23
---

# Упражнения: типы и значения

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Запускайте код в файле `.js` проекта с `"type": "module"` (см. [модуль 01](../01-environment/basics.ru.md)).

## 1. 🟢 Определите тип

**Задача:** напишите функцию `kind(x)`, которая возвращает `'null'`, `'array'`, `'nan'` или результат `typeof` для остальных значений.

**Ожидаемый результат:** `kind(null)` → `'null'`, `kind([])` → `'array'`, `kind(NaN)` → `'nan'`, `kind(10n)` → `'bigint'`, `kind(() => {})` → `'function'`.

<details><summary>Подсказка</summary>

Порядок проверок важен: `typeof null` — `'object'`, а `typeof NaN` — `'number'`.

</details>

<details><summary>Решение</summary>

```js
function kind(x) {
  if (x === null) return 'null';
  if (Array.isArray(x)) return 'array';
  if (Number.isNaN(x)) return 'nan';
  return typeof x;
}
console.log(kind(null), kind([]), kind(NaN), kind(10n), kind(() => {}));
```

```text
null array nan bigint function
```

</details>

## 2. 🟢 Значения по умолчанию

**Задача:** функция `port(cfg)` возвращает `cfg.port`, а если он `null` или `undefined` — `3000`; `0` должен сохраняться.

**Ожидаемый результат:** `port({port: 0})` → `0`, `port({})` → `3000`, `port({port: null})` → `3000`.

<details><summary>Подсказка</summary>

Не `||`.

</details>

<details><summary>Решение</summary>

```js
const port = (cfg) => cfg.port ?? 3000;
console.log(port({ port: 0 }), port({}), port({ port: null }));
```

```text
0 3000 3000
```

</details>

## 3. 🟡 Безопасное сравнение дробей

**Задача:** функция `almostEqual(a, b)` возвращает `true`, если числа различаются меньше чем на `Number.EPSILON`. Проверьте на `0.1 + 0.2` и `0.3`, затем на `1` и `1.1`.

**Ожидаемый результат:** `true`, `false`.

<details><summary>Подсказка</summary>

`Math.abs(a - b) < Number.EPSILON`.

</details>

<details><summary>Решение</summary>

```js
const almostEqual = (a, b) => Math.abs(a - b) < Number.EPSILON;
console.log(almostEqual(0.1 + 0.2, 0.3), almostEqual(1, 1.1));
```

```text
true false
```

Для больших чисел фиксированный допуск `Number.EPSILON` не годится: он подходит для значений порядка единицы.

</details>

## 4. 🟡 Что выведет код?

**Задача:** предскажите, а затем проверьте результат: `[10, 9, 1].sort()`, `[] + {}`, `'5' - 3`, `null >= 0`, `JSON.stringify({ a: undefined, b: NaN })`.

**Ожидаемый результат:** `[ 1, 10, 9 ]`, `[object Object]`, `2`, `true`, `{"b":null}`.

<details><summary>Подсказка</summary>

`sort` без функции сравнения сортирует строки; `>=` приводит `null` к числу.

</details>

<details><summary>Решение</summary>

```js
console.log([10, 9, 1].sort(), [] + {}, '5' - 3, null >= 0, JSON.stringify({ a: undefined, b: NaN }));
```

```text
[ 1, 10, 9 ] [object Object] 2 true {"b":null}
```

</details>

## 5. 🔴 Значения и ссылки

**Задача:** функция `bump(o)` должна увеличивать `o.n` на 1 и **не** менять переданный объект: она возвращает новый объект. Покажите, чем это отличается от варианта, который мутирует аргумент. Затем объясните, почему `{ n: 1 } === { n: 1 }` ложно.

**Ожидаемый результат:** исходный объект сохраняет `n: 1`, результат — `n: 2`; сравнение литералов ложно.

<details><summary>Подсказка</summary>

Создайте новый объект литералом `{ ...o, n: o.n + 1 }` (распространение объектов изучается в модуле 05 дорожной карты).

</details>

<details><summary>Решение</summary>

```js
const bump = (o) => ({ ...o, n: o.n + 1 });
const a = { n: 1 };
const b = bump(a);
console.log(a.n, b.n, a === b, { n: 1 } === { n: 1 });
```

```text
1 2 false false
```

Два литерала создают два разных объекта; `===` для объектов сравнивает ссылки.

</details>
