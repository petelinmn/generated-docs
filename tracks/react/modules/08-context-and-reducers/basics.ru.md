---
title: "Context и редьюсеры: базовая теория"
lang: ru
track: react
kind: module
doc: basics
slug: 08-context-and-reducers
part: 2
tags: [react, context, use-reducer, state-management, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Context и редьюсеры: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [состояние и события](../03-state-and-events/overview.ru.md) · **Время:** ~2 ч

Все результаты получены на React 19.3.1.

## useReducer

```tsx
type State = { count: number };
type Action = { type: 'inc' } | { type: 'add'; by: number };

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case 'inc': return { count: state.count + 1 };
    case 'add': return { count: state.count + action.by };
  }
}

function Counter() {
  const [state, dispatch] = useReducer(reducer, { count: 0 });
  return <button onClick={() => dispatch({ type: 'inc' })}>{state.count}</button>;
}
```

`dispatch` отправляет action; React вызывает редьюсер с текущим состоянием и рендерит с результатом. Объединённый тип actions позволяет TypeScript проверить каждую ветку.

```tsx
dispatch({ type: 'inc' }); dispatch({ type: 'inc' }); dispatch({ type: 'inc' });
console.log(state.count);   // 0: the current render still has the old state
// next screen: 3, one render
```

## Context

```tsx
const Theme = createContext('light');

function Label() {
  return <b>{useContext(Theme)}</b>;
}

<>
  <Label />
  <Theme value="dark">
    <Label />
    <Theme value="blue"><Label /></Theme>
  </Theme>
</>
// <b>light</b><b>dark</b><b>blue</b>
```

`createContext(default)` создаёт его, провайдер задаёт значение для поддерева, `useContext` читает ближайший. С React 19 провайдером служит сам context (`<Theme value=...>`); `Theme.Provider` — прежняя форма.

## Ошибки новичка

- ❌ Изменять `state` внутри редьюсера → ✅ возвращать новый объект.
- ❌ Запрос или таймер внутри редьюсера → ✅ в обработчике или эффекте.
- ❌ Забыть провайдер и молча получить значение по умолчанию → ✅ хук с проверкой.
- ❌ Context вместо одного prop → ✅ передать prop.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: useReducer](https://react.dev/reference/react/useReducer)
- [React: createContext](https://react.dev/reference/react/createContext)
- [React: useContext](https://react.dev/reference/react/useContext)
- [React: передача данных вглубь через context](https://react.dev/learn/passing-data-deeply-with-context)
