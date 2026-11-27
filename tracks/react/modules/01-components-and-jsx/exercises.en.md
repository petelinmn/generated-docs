---
title: "Exercises: components and JSX"
lang: en
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

# Exercises: components and JSX

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Run your code in a Vite project from [basics](basics.en.md).

## 1. 🟢 A price

**Task:** a component `Price` that shows a number with two decimals and the euro sign.

<details><summary>Hint</summary>

`toFixed` and braces.

</details>

<details><summary>Solution</summary>

```tsx
function Price({ value }: { value: number }) {
  return <span>{value.toFixed(2)} €</span>;
}
<Price value={3} />   // <span>3.00 €</span>
```

</details>

## 2. 🟡 A star rating

**Task:** show 0–5 filled stars out of five and give screen readers the text "3 of 5".

<details><summary>Hint</summary>

`String.repeat` and an `aria-label`.

</details>

<details><summary>Solution</summary>

```tsx
function Rating({ stars }: { stars: number }) {
  return <p aria-label={`${stars} of 5`}>{'★'.repeat(stars)}{'☆'.repeat(5 - stars)}</p>;
}
<Rating stars={3} />   // <p aria-label="3 of 5">★★★☆☆</p>
```

A value outside 0–5 would make `repeat` throw for a negative count: validate it where the data enters.

</details>

## 3. 🔴 A product list

**Task:** render products as list items; the class shows availability, and sold-out ones are marked in the text.

<details><summary>Hint</summary>

`map`, a stable `key`, a conditional class.

</details>

<details><summary>Solution</summary>

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

The `key` is written where the array is created, on the outermost element returned by `map`.

</details>
