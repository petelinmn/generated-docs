---
title: "Упражнения: массивы и коллекции"
lang: ru
track: javascript
kind: module
doc: exercises
slug: 07-arrays-and-collections
part: 1
tags: [array, set, map, immutability, lru-cache, sort]
status: draft
updated: 2026-11-23
---

# Упражнения: массивы и коллекции

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Запускайте код в файле ES-модуля.

## 1. 🟢 Уникальные и отсортированные

**Задача:** из массива `[5, 3, 5, 10, 1, 3]` получите отсортированные по возрастанию уникальные числа, не меняя исходный массив.

**Ожидаемый результат:** `[ 1, 3, 5, 10 ]`, исходный массив без изменений.

<details><summary>Подсказка</summary>

`Set` убирает дубликаты, `toSorted` с числовой функцией сравнения не меняет исходник.

</details>

<details><summary>Решение</summary>

```js
const src = [5, 3, 5, 10, 1, 3];
const result = [...new Set(src)].toSorted((a, b) => a - b);
console.log(result, src);
```

```text
[ 1, 3, 5, 10 ] [ 5, 3, 5, 10, 1, 3 ]
```

</details>

## 2. 🟢 Частота слов

**Задача:** по строке `'to be or not to be'` посчитайте, сколько раз встречается каждое слово, с помощью `Map`, и выведите пары в порядке убывания частоты.

**Ожидаемый результат:** `[ [ 'to', 2 ], [ 'be', 2 ], [ 'or', 1 ], [ 'not', 1 ] ]`.

<details><summary>Подсказка</summary>

`counts.set(w, (counts.get(w) ?? 0) + 1)`, затем `[...counts].sort((a, b) => b[1] - a[1])` (сортировка стабильна).

</details>

<details><summary>Решение</summary>

```js
const counts = new Map();
for (const w of 'to be or not to be'.split(' ')) counts.set(w, (counts.get(w) ?? 0) + 1);
console.log([...counts].sort((a, b) => b[1] - a[1]));
```

```text
[ [ 'to', 2 ], [ 'be', 2 ], [ 'or', 1 ], [ 'not', 1 ] ]
```

</details>

## 3. 🟡 Неизменяемые обновления

**Задача:** для списка `items = ['a', 'b', 'c']` без изменения исходного массива получите: замену элемента с индексом 1 на `'X'`; список без элемента с индексом 0; список с `'Z'`, вставленным на позицию 1.

**Ожидаемый результат:** `[ 'a', 'X', 'c' ]`, `[ 'b', 'c' ]`, `[ 'a', 'Z', 'b', 'c' ]`, исходный не изменился.

<details><summary>Подсказка</summary>

`with`, `toSpliced`.

</details>

<details><summary>Решение</summary>

```js
const items = ['a', 'b', 'c'];
console.log(items.with(1, 'X'), items.toSpliced(0, 1), items.toSpliced(1, 0, 'Z'), items);
```

```text
[ 'a', 'X', 'c' ] [ 'b', 'c' ] [ 'a', 'Z', 'b', 'c' ] [ 'a', 'b', 'c' ]
```

</details>

## 4. 🟡 Операции над множествами

**Задача:** даны теги двух статей `a = ['js', 'node', 'web']`, `b = ['web', 'css', 'js']`. Найдите общие теги, теги только первой статьи и все теги без повторов.

**Ожидаемый результат:** `[ 'js', 'web' ]`, `[ 'node' ]`, `[ 'js', 'node', 'web', 'css' ]`.

<details><summary>Подсказка</summary>

`intersection`, `difference`, `union` у `Set`.

</details>

<details><summary>Решение</summary>

```js
const A = new Set(['js', 'node', 'web']), B = new Set(['web', 'css', 'js']);
console.log([...A.intersection(B)], [...A.difference(B)], [...A.union(B)]);
```

```text
[ 'js', 'web' ] [ 'node' ] [ 'js', 'node', 'web', 'css' ]
```

</details>

## 5. 🔴 LRU-кэш на Map

**Задача:** напишите класс `Lru` с ёмкостью `cap`, методами `get(k)` и `set(k, v)`. Недавно использованные ключи должны жить дольше: при переполнении удаляется самый давно не использованный. Используйте порядок вставки `Map`.

**Ожидаемый результат:** при `cap = 2`: `set a`, `set b`, `get a`, `set c` — ключ `b` вытеснен, `a` и `c` остаются.

<details><summary>Подсказка</summary>

При `get` удалите и вставьте ключ заново, чтобы он стал последним; самый давний — первый: `this.map.keys().next().value`.

</details>

<details><summary>Решение</summary>

```js
class Lru {
  #cap; #map = new Map();
  constructor(cap) { this.#cap = cap; }
  get(k) {
    if (!this.#map.has(k)) return undefined;
    const v = this.#map.get(k);
    this.#map.delete(k); this.#map.set(k, v);
    return v;
  }
  set(k, v) {
    this.#map.delete(k); this.#map.set(k, v);
    if (this.#map.size > this.#cap) this.#map.delete(this.#map.keys().next().value);
    return this;
  }
  keys() { return [...this.#map.keys()]; }
}
const c = new Lru(2);
c.set('a', 1).set('b', 2); c.get('a'); c.set('c', 3);
console.log(c.keys(), c.get('b'));
```

```text
[ 'a', 'c' ] undefined
```

</details>
