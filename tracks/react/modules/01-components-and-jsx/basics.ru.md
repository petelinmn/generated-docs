---
title: "Компоненты и JSX: базовая теория"
lang: ru
track: react
kind: module
doc: basics
slug: 01-components-and-jsx
part: 1
tags: [react, jsx, components, rendering]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Компоненты и JSX: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** функции, массивы и модули JavaScript; основы HTML · **Время:** ~2 ч

## Создание проекта

Документация React рекомендует начинать рабочие приложения с фреймворка. Для обучения хватает простого проекта Vite. Create React App объявлен устаревшим.

```bash
npm create vite@latest my-app -- --template react-ts
cd my-app
npm install
npm run dev
```

Примеры модуля выполнены на React 19.3.1. Чтобы показать получающийся HTML, в них используется `renderToStaticMarkup`.

## Компонент

```tsx
import { renderToStaticMarkup } from 'react-dom/server';

function Greeting() {
  const name = 'Ann';
  return <h1 className="title">Hello, {name}!</h1>;
}

console.log(renderToStaticMarkup(<Greeting />));
// <h1 class="title">Hello, Ann!</h1>
```

Компонент — это функция. Её имя начинается с заглавной буквы, и используют её как тег. В приложении корень выводит верхний компонент в узел DOM:

```tsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Greeting />
  </StrictMode>,
);
```

## Правила JSX

| HTML | JSX |
|---|---|
| несколько элементов верхнего уровня | один корень или фрагмент `<>...</>` |
| `<img>` можно не закрывать | все теги закрыты: `<img />` |
| `class`, `for` | `className`, `htmlFor` |
| `tabindex`, `maxlength` | `tabIndex`, `maxLength` |
| `style="color:red"` | `style={{ color: 'red' }}` |

`aria-*` и `data-*` сохраняют дефисы.

```tsx
// one root: a fragment adds no DOM node
<>
  <li>a</li>
  <li>b</li>
</>
// <li>a</li><li>b</li>

<a href="/x" aria-label="go" data-id="7" tabIndex={0}>go</a>
<label htmlFor="n">N<input id="n" disabled /></label>
<div style={{ color: 'red', fontSize: 12, lineHeight: 2 }} />
// <div style="color:red;font-size:12px;line-height:2"></div>
```

## Фигурные скобки

В скобках пишут любое **выражение**: переменную, вычисление, вызов функции. Инструкции вроде `if` и `for` там написать нельзя.

## Списки и условия

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

`map` превращает данные в элементы, у каждого — устойчивый `key`. Для условий используют `? :` или `&&`.

## Ошибки новичка

- ❌ `function button()` с использованием `<button />` → ✅ имена компонентов с заглавной.
- ❌ `class=` → ✅ `className=`.
- ❌ Список без `key` → ✅ устойчивый идентификатор из данных.
- ❌ Вызывать компонент как `Greeting()` → ✅ использовать как `<Greeting />`.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: разметка на JSX](https://react.dev/learn/writing-markup-with-jsx)
- [React: рендер и фиксация](https://react.dev/learn/render-and-commit)
- [React: StrictMode](https://react.dev/reference/react/StrictMode)
- [React: создание приложения](https://react.dev/learn/creating-a-react-app)
- [React: сборка с нуля](https://react.dev/learn/build-a-react-app-from-scratch)
