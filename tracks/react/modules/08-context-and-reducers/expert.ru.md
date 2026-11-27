---
title: "Context и редьюсеры: экспертная теория"
lang: ru
track: react
kind: module
doc: expert
slug: 08-context-and-reducers
part: 2
tags: [react, context, use-reducer, state-management, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Context и редьюсеры: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

`useReducer` хранит очередь actions; при следующем рендере React по порядку прогоняет через них редьюсер. `useState` — тот же механизм со встроенным редьюсером. Для context React ищет вверх по дереву ближайший провайдер и рендерит потребителей, когда его значение не равно прежнему по `Object.is`.

### use

```tsx
function Maybe({ on }: { on: boolean }) {
  if (!on) return <i>off</i>;
  const theme = use(Theme);              // allowed after a condition
  return <i>{theme}</i>;
}
```

`use` читает context как `useContext`, но его можно вызывать в условиях и циклах. Читать им context в Server Components нельзя.

## Справочник

| API | Назначение |
|---|---|
| `useReducer(reducer, initial)` | состояние с редьюсером |
| `useReducer(reducer, arg, init)` | `init(arg)` один раз вычисляет начальное состояние |
| `dispatch(action)` | запросить обновление |
| `createContext(default)` | создать context |
| `<Context value={v}>` | предоставить (React 19) |
| `<Context.Provider value={v}>` | прежний способ предоставления |
| `useContext(Context)` | прочитать ближайшее значение |
| `use(Context)` | прочитать, можно по условию |

## Подвохи

### Мутирующий редьюсер удваивает эффект

```tsx
function mutating(s: { items: string[] }, a: string) {
  s.items.push(a);
  return { items: s.items };
}
// inside <StrictMode>, one dispatch('a'):  the screen shows  a,a
```

### undefined — это не «нет провайдера»

```tsx
<Theme value={undefined}><Label /></Theme>   // <b></b> — not the default 'light'
```

### Нельзя потреблять то, что сам предоставляешь

```tsx
function Self() {
  const theme = useContext(Theme);                         // from above, not from below
  return <Theme value="inner">{theme}<Label /></Theme>;    // light<b>inner</b>
}
```

### Значение без мемоизации

```tsx
<User value={{ name: 'Ann' }}>            {/* a new object on every render */}
const value = useMemo(() => ({ name: 'Ann' }), []);
<User value={value}>                      {/* the same object */}
```

```text
two re-renders of the provider's owner:
new object every time:  consumer 2, memo consumer 2, memo component without context 0
memoized value:         consumer 0, memo consumer 0, memo component without context 0
```

## Компромиссы и альтернативы

| Задача | Вариант | Замечание |
|---|---|---|
| Общее для близких компонентов | поднять состояние | явно |
| Пропустить средние слои | передать элементы через `children` | нет скрытой зависимости |
| На всё приложение, редко меняется | context | просто |
| На всё приложение, часто меняется, выборочное чтение | внешнее хранилище | подписчики выбирают, что читать |
| Данные сервера | библиотека данных | кэш и повторная загрузка |

## Вопросы с собеседований

**1. `useState` или `useReducer`?**

<details><summary>Ответ</summary>

`useReducer`, когда обновлений много и они связаны: правила собраны в одной проверяемой функции.

</details>

**2. Почему редьюсер должен быть чистым?**

<details><summary>Ответ</summary>

React может вызвать его не один раз; StrictMode делает это при разработке.

</details>

**3. Стабилен ли `dispatch`?**

<details><summary>Ответ</summary>

Да.

</details>

**4. Когда используется значение context по умолчанию?**

<details><summary>Ответ</summary>

Только когда выше нет провайдера.

</details>

**5. Останавливает ли `memo` обновления от context?**

<details><summary>Ответ</summary>

Нет.

</details>

**6. Как уменьшить рендеры из-за context?**

<details><summary>Ответ</summary>

Мемоизировать значение, разделить context, опустить состояние ближе к месту использования.

</details>

**7. Чем `use` отличается от `useContext`?**

<details><summary>Ответ</summary>

`use` можно вызывать в условиях и циклах.

</details>

**8. Является ли context менеджером состояния?**

<details><summary>Ответ</summary>

Он только доставляет значение; состояние и обновления дают `useState`, `useReducer` или хранилище.

</details>

**9. Подвох: `<Theme value={undefined}>` — что получат потребители?**

<details><summary>Ответ</summary>

`undefined`, а не значение по умолчанию.

</details>

**10. Подвох: может ли компонент прочитать context, который сам предоставляет?**

<details><summary>Ответ</summary>

Нет, `useContext` смотрит выше компонента.

</details>

**11. Может ли редьюсер вернуть то же состояние?**

<details><summary>Ответ</summary>

Да; React пропустит рендер потомков, хотя сам компонент может вызвать.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: useReducer](https://react.dev/reference/react/useReducer)
- [React: createContext](https://react.dev/reference/react/createContext)
- [React: useContext](https://react.dev/reference/react/useContext)
- [React: передача данных вглубь через context](https://react.dev/learn/passing-data-deeply-with-context)
- [React: use](https://react.dev/reference/react/use)
