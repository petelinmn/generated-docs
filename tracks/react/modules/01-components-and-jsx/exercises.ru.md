---
title: "Упражнения: компоненты и JSX"
lang: ru
track: react
kind: module
doc: exercises
slug: 01-components-and-jsx
part: 1
tags: [react, jsx, components, rendering]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Упражнения: компоненты и JSX

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Код запускай в проекте Vite из [базовой теории](basics.ru.md).

## 1. 🟢 Цена

**Задача:** компонент `Price`, показывающий число с двумя знаками и знаком евро.

<details><summary>Подсказка</summary>

`toFixed` и фигурные скобки.

</details>

<details><summary>Решение</summary>

```tsx
function Price({ value }: { value: number }) {
  return <span>{value.toFixed(2)} €</span>;
}
<Price value={3} />   // <span>3.00 €</span>
```

</details>

## 2. 🟡 Рейтинг звёздами

**Задача:** показать 0–5 закрашенных звёзд из пяти и дать программам чтения с экрана текст «3 of 5».

<details><summary>Подсказка</summary>

`String.repeat` и `aria-label`.

</details>

<details><summary>Решение</summary>

```tsx
function Rating({ stars }: { stars: number }) {
  return <p aria-label={`${stars} of 5`}>{'★'.repeat(stars)}{'☆'.repeat(5 - stars)}</p>;
}
<Rating stars={3} />   // <p aria-label="3 of 5">★★★☆☆</p>
```

Значение вне 0–5 заставит `repeat` бросить исключение при отрицательном числе: проверяйте его там, где данные входят в программу.

</details>

## 3. 🔴 Список товаров

**Задача:** вывести товары пунктами списка; класс показывает наличие, а отсутствующие помечены в тексте.

<details><summary>Подсказка</summary>

`map`, устойчивый `key`, класс по условию.

</details>

<details><summary>Решение</summary>

```tsx
const products = [
  { id: 1, name: 'Pen', inStock: true },
  { id: 2, name: 'Book', inStock: false },
];

function Product({ name, inStock }: { name: string; inStock: boolean }) {
  return <li className={inStock ? 'in' : 'out'}>{name}{!inStock && ' (sold out)'}</li>;
}

<ul>{products.map(p => <Product key={p.id} name={p.name} inStock={p.inStock} />)}</ul>
// <ul><li class="in">Pen</li><li class="out">Book (sold out)</li></ul>
```

`key` пишут там, где создаётся массив, на самом внешнем элементе, возвращаемом из `map`.

</details>
