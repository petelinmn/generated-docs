---
title: "Состояние и события: базовая теория"
lang: ru
track: react
kind: module
doc: basics
slug: 03-state-and-events
part: 1
tags: [react, state, use-state, events, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Состояние и события: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [props и композиция](../02-props-and-composition/overview.ru.md) · **Время:** ~2 ч

Все результаты получены на React 19.3.1.

## Почему переменной недостаточно

```tsx
function Local() {
  let n = 0;
  return <button onClick={() => { n++; }}>{n}</button>;   // the screen stays 0
}
```

Переменная создаётся заново при каждом рендере, а её изменение не заставляет React рендерить.

## useState

```tsx
function Counter() {
  const [n, setN] = useState(0);
  return <button onClick={() => setN(n + 1)}>{n}</button>;
}
```

`useState(0)` даёт пару: значение для этого рендера и функцию установки. Её вызов планирует рендер, в котором значение уже новое. У каждого экземпляра компонента своё состояние. Хуки вроде `useState` вызывают только на верхнем уровне компонента.

## Обработчики событий

Передавайте функцию, а не вызывайте её.

```tsx
<button onClick={log.push('called during render')}>p</button>
// the function runs while rendering, and React reports:
// Expected `onClick` listener to be a function, instead got a value of `number` type.
```

Событие идёт от цели вверх к предкам (все события React всплывают, кроме `onScroll`). `stopPropagation` останавливает его, а `onClickCapture` срабатывает на пути вниз.

```tsx
<div onClick={() => log.push('div')}>
  <button onClick={() => log.push('button')}>p</button>
  <button onClick={e => { e.stopPropagation(); log.push('stopped'); }}>s</button>
</div>
// click p, then s:  button,div,stopped

<div onClickCapture={() => log.push('capture div')}>
  <button onClick={() => log.push('button')}>p</button>
</div>
// click:  capture div,button
```

## Форма

```tsx
function NameForm() {
  const [name, setName] = useState('');
  const [sent, setSent] = useState('');

  function submit(e: SubmitEvent<HTMLFormElement>) {   // SubmitEvent from 'react'
    e.preventDefault();                                // no page reload
    setSent(name);
  }

  return (
    <form onSubmit={submit}>
      <input value={name} onChange={e => setName(e.target.value)} />
      <button>ok</button>
      <p>{sent}</p>
    </form>
  );
}
```

`preventDefault` отменяет собственную реакцию браузера. `value` вместе с `onChange` делает поле управляемым состоянием.

## Ошибки новичка

- ❌ `onClick={save()}` → ✅ `onClick={save}`.
- ❌ Локальная переменная для данных на экране → ✅ состояние.
- ❌ Забыть `preventDefault` при отправке → ✅ иначе страница перезагрузится.
- ❌ Читать новое значение сразу после установки → ✅ сначала вычислите его в переменную.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: useState](https://react.dev/reference/react/useState)
- [React: реакция на события](https://react.dev/learn/responding-to-events)
- [React: состояние как снимок](https://react.dev/learn/state-as-a-snapshot)
- [React: сохранение и сброс состояния](https://react.dev/learn/preserving-and-resetting-state)
