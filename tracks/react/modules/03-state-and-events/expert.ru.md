---
title: "Состояние и события: экспертная теория"
lang: ru
track: react
kind: module
doc: expert
slug: 03-state-and-events
part: 1
tags: [react, state, use-state, events, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Состояние и события: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

React хранит состояние вне вашей функции, для каждой позиции в дереве. При каждом рендере хуки компонента сопоставляются с сохранёнными значениями **по порядку вызовов**, поэтому порядок не должен меняться:

```tsx
function Cond({ extra }: { extra: boolean }) {
  if (extra) { useState(0); }          // breaks the rules of Hooks
  const [n] = useState(1);
  return <p>{n}</p>;
}
// rendering extra={false}, then extra={true}:
// Error: Rendered more hooks than during the previous render.
```

Функция установки кладёт обновление в очередь; React обрабатывает очередь при следующем рендере, применяя значения и функции-обновления по порядку. Сама функция установки не меняется между рендерами и ничего не возвращает.

### StrictMode

```tsx
// inside <StrictMode>, development:
// component body on mount: 2 calls
// setN(v => v + 1) once: the updater runs 2 times, the value becomes 1
```

Поэтому функции-обновления должны быть чистыми.

### Принудительное обновление DOM

```tsx
onClick={() => {
  console.log(box.textContent);                  // 0
  flushSync(() => setN(n + 1));                  // from 'react-dom'
  console.log(box.textContent);                  // 1: the DOM is already updated
}}
```

`flushSync` может заметно вредить производительности; это крайняя мера.

## Справочник

| Вызов | Действие |
|---|---|
| `useState(value)` | начальное значение, используется один раз |
| `useState(fn)` | `fn` вычисляет начальное значение при первом рендере |
| `set(value)` | заменить на следующем рендере |
| `set(prev => next)` | вычислить из значения в очереди |
| то же значение (`Object.is`) | поддерево не рендерится заново |

| Возможность событий | Синтаксис |
|---|---|
| Обработчик | `onClick={handle}` |
| Фаза захвата | `onClickCapture` |
| Остановить всплытие | `e.stopPropagation()` |
| Отменить действие браузера | `e.preventDefault()` |
| Источник и текущий элемент | `e.target`, `e.currentTarget` |
| Не всплывает | `onScroll` |

| Что происходит с состоянием | Когда |
|---|---|
| сохраняется | тот же тип компонента в той же позиции |
| сбрасывается | другой тип, другой `key` или удаление |

## Подвохи

### Функция как значение состояния

```tsx
const [f, setF] = useState<() => string>(() => () => 'first');
setF(() => () => 'second');     // wrap: otherwise React treats a function as an updater
```

### Инициализатор или вызов

```tsx
useState(expensive);      // called once, on the first render
useState(expensive());    // called on every render; the result is ignored after the first
// after the first render and two updates: 1 call versus 3
```

### Компонент, объявленный внутри другого

```tsx
function Parent() {
  const [t, setT] = useState(0);
  function Inner() {                       // a new function on every render
    const [n, setN] = useState(0);
    return <button onClick={() => setN(n + 1)}>inner {n}</button>;
  }
  return <><Inner /><button onClick={() => setT(t + 1)}>outer {t}</button></>;
}
// click inner:  inner 1 outer 0
// click outer:  inner 0 outer 1
```

### Безусловное обновление во время рендера

```tsx
function Loop() {
  const [n, setN] = useState(0);
  setN(n + 1);                        // unconditional set during render
  return <p>{n}</p>;
}
// Error: Too many re-renders. React limits the number of renders to prevent an infinite loop.
```

### target — не currentTarget

```tsx
<div id="outer" onClick={e => console.log((e.target as HTMLElement).id, e.currentTarget.id)}>
  <button id="inner">x</button>
</div>
// click the button:  inner outer
```

## Компромиссы и альтернативы

| Вариант | Плюс | Минус |
|---|---|---|
| Несколько `useState` | независимые, просто | связанные обновления разбросаны |
| Один объект состояния | сгруппировано | копировать при каждом изменении |
| `useReducer` | логика обновления в одной функции | больше кода |
| Ref | без повторного рендера | экран не обновляется |
| Производные значения | никогда не расходятся | вычисляются при каждом рендере |
| Внешнее хранилище | общее для приложения | зависимость и свои правила |

## Вопросы с собеседований

**1. Почему вывод сразу после `setN` показывает старое значение?**

<details><summary>Ответ</summary>

`n` — константа текущего рендера; новое значение появится в следующем.

</details>

**2. `setN(n + 1)` трижды: какой результат?**

<details><summary>Ответ</summary>

`n + 1`. С `x => x + 1` было бы `n + 3`.

</details>

**3. Что такое пакетные обновления?**

<details><summary>Ответ</summary>

React применяет несколько обновлений за один рендер; с React 18 — и в таймерах, и в промисах.

</details>

**4. Почему нельзя мутировать состояние?**

<details><summary>Ответ</summary>

React сравнивает по ссылке и может пропустить обновление; портятся и старые снимки.

</details>

**5. Вызывает ли установка того же значения рендер?**

<details><summary>Ответ</summary>

Дочерние пропускаются; сам компонент React всё же может вызвать.

</details>

**6. Как работает ленивая инициализация?**

<details><summary>Ответ</summary>

Передают функцию: она вызывается только при первом рендере (в разработке под StrictMode дважды).

</details>

**7. Как сбросить состояние компонента?**

<details><summary>Ответ</summary>

Дать другой `key`.

</details>

**8. Почему хуки нельзя вызывать по условию?**

<details><summary>Ответ</summary>

React сопоставляет состояние с хуками по порядку вызовов.

</details>

**9. Для чего `flushSync`?**

<details><summary>Ответ</summary>

Чтобы применить обновление к DOM немедленно, например перед измерением. Он вредит производительности и является крайней мерой.

</details>

**10. Подвох: всегда ли установка состояния во время рендера — ошибка?**

<details><summary>Ответ</summary>

Безусловная зацикливается. Защищённая условием, для своего же компонента, — поддерживаемый, но редко нужный приём.

</details>

**11. Подвох: `e.target` и `e.currentTarget`?**

<details><summary>Ответ</summary>

`target` — где событие произошло, `currentTarget` — элемент, чей обработчик выполняется.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: useState](https://react.dev/reference/react/useState)
- [React: реакция на события](https://react.dev/learn/responding-to-events)
- [React: состояние как снимок](https://react.dev/learn/state-as-a-snapshot)
- [React: сохранение и сброс состояния](https://react.dev/learn/preserving-and-resetting-state)
- [React: updating arrays in state](https://react.dev/learn/updating-arrays-in-state)
- [React: flushSync](https://react.dev/reference/react-dom/flushSync)
- [React: rules of Hooks](https://react.dev/reference/rules/rules-of-hooks)
- [React 18: automatic batching](https://react.dev/blog/2022/03/29/react-v18)
