---
title: "Конкурентные возможности: экспертная теория"
lang: ru
track: react
kind: module
doc: expert
slug: 12-concurrent-features
part: 3
tags: [react, suspense, transitions, concurrent-rendering, error-boundaries]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Конкурентные возможности: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

React может начать рендер обновления, приостановить или отбросить его при появлении более срочного и зафиксировать только завершённый результат. Компонент «приостанавливается» (suspends), когда не готов (код или данные ещё грузятся); тогда React показывает fallback ближайшей границы, если только это не transition и содержимое уже на экране.

## Activity

```tsx
<Activity mode={on ? 'visible' : 'hidden'}><Panel /></Activity>
// count to 1, hide:  display: none !important;  effects cleaned up
// show again:        the count is still 1;       effects run again
```

`<Activity>` (перечислен среди новинок React 19.2) скрывает поддерево, не уничтожая состояние.

## Новое в 19.3

В React 19.3 стал стабильным `<ViewTransition>`: он анимирует изменения, происходящие в transitions и при раскрытии Suspense. Подробности — в заметке о выпуске.

## Справочник

| API | Назначение |
|---|---|
| `<Suspense fallback>` | заглушка, пока не готово |
| `lazy(load)` | разделение кода |
| `use(promise)` | прочитать кэшированный промис |
| `useTransition()` | `[isPending, startTransition]` |
| `startTransition(fn)` | transition вне хука компонента |
| `useDeferredValue(value, initialValue?)` | отстающая копия значения |
| `<Activity mode>` | скрыть, сохранив состояние |
| `<ViewTransition>` | анимировать переходы |
| `getDerivedStateFromError`, `componentDidCatch` | error boundary |

## Подвохи

### Граница прячет весь раздел

```tsx
<Suspense fallback={<p>loading</p>}>
  <h1>title</h1>
  <Wait />               {/* suspends */}
</Suspense>
// <p>loading</p>  — the title is hidden too
```

### Обычное обновление прячет старую страницу

```tsx
function Tabs() {
  const [id, setId] = useState('one');
  const [isPending, start] = useTransition();
  return <>
    <button onClick={() => start(() => setId('two'))}>go</button>
    <span>{isPending ? 'pending' : 'idle'}</span>
    <Suspense fallback={<p>loading</p>}><Page id={id} /></Suspense>
  </>;
}
```

```text
while page two is loading
  plain setId:      idle      old page hidden, "loading" shown
  inside start():   pending   old page ONE stays on screen
afterwards: idle, TWO
```

### Ошибки в обработчиках не перехватываются

```tsx
<Boundary><button onClick={() => { throw new Error('in handler'); }}>x</button></Boundary>
// the button stays; the error is reported globally, the boundary does not catch it
```

### Обновления после await

```tsx
start(async () => {
  await save();
  startTransition(() => setMessage('done'));   // wrap updates after await again
});
// meanwhile isPending is true
```

## Компромиссы и альтернативы

| Выбор | Плюс | Минус |
|---|---|---|
| Спиннер-fallback | просто | содержимое прыгает |
| Transition со старым содержимым | спокойно | нужен признак ожидания |
| Debounce | меньше запросов | фиксированная задержка |
| `useDeferredValue` | подстраивается под устройство | не уменьшает число запросов |
| Скрытие через `Activity` | состояние сохраняется | память |

## Вопросы с собеседований

**1. Для чего `Suspense`?**

<details><summary>Ответ</summary>

Показывать fallback, пока потомок не готов.

</details>

**2. Что его активирует?**

<details><summary>Ответ</summary>

Компоненты `lazy`, `use` с кэшированным промисом, данные фреймворка с поддержкой Suspense. Не загрузка в эффекте.

</details>

**3. Что такое transition?**

<details><summary>Ответ</summary>

Несрочное прерываемое обновление, не прячущее уже показанное.

</details>

**4. `useTransition` или `useDeferredValue`?**

<details><summary>Ответ</summary>

Первый — когда вы управляете обновлением состояния; второй — когда лишь получаете значение.

</details>

**5. Это debounce?**

<details><summary>Ответ</summary>

Нет, фиксированной задержки нет.

</details>

**6. Что не ловят error boundaries?**

<details><summary>Ответ</summary>

Обработчики событий, асинхронные callback, серверный рендер и свои ошибки. Ошибки в функциях `startTransition` — исключение, они перехватываются.

</details>

**7. Что делает `Activity` в скрытом режиме?**

<details><summary>Ответ</summary>

Скрывает через `display: none`, очищает эффекты, сохраняет состояние.

</details>

**8. Подвох: почему с `use` не работает промис, созданный в рендере?**

<details><summary>Ответ</summary>

Каждый рендер создаёт новый, и он никогда не готов.

</details>

**9. Подвох: может ли error boundary быть функциональным компонентом?**

<details><summary>Ответ</summary>

Нет.

</details>

**10. Можно ли обернуть `use` в `try`/`catch`?**

<details><summary>Ответ</summary>

Нет.

</details>

**11. Почему управляемое поле остаётся срочным?**

<details><summary>Ответ</summary>

Ввод должен отражаться сразу; обновления в transition не могут управлять полем.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: Suspense](https://react.dev/reference/react/Suspense)
- [React: useTransition](https://react.dev/reference/react/useTransition)
- [React: useDeferredValue](https://react.dev/reference/react/useDeferredValue)
- [React: use](https://react.dev/reference/react/use)
- [React: error boundaries](https://react.dev/reference/react/Component)
- [React: Activity](https://react.dev/reference/react/Activity)
- [React 19.3](https://react.dev/blog/2026/09/09/react-19-3)
