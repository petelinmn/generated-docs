---
title: "Конкурентные возможности: продвинутая теория"
lang: ru
track: react
kind: module
doc: advanced
slug: 12-concurrent-features
part: 3
tags: [react, suspense, transitions, concurrent-rendering, error-boundaries]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Конкурентные возможности: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Transitions

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

Transition говорит: это обновление может подождать и может быть прервано. React сохраняет уже показанное содержимое и сообщает `isPending`, а не переключается на fallback. Управлять текстовым полем обновлениями в transition нельзя.

## Асинхронные actions

```tsx
start(async () => {
  await save();
  startTransition(() => setMessage('done'));   // wrap updates after await again
});
// meanwhile isPending is true
```

## Отложенные значения

```tsx
function Results({ q }: { q: string }) {
  const deferred = useDeferredValue(q);
  return <List query={deferred} />;
}
// q changes from "a" to "ab": two renders, first with deferred = "a", then "ab"

useDeferredValue('real', 'placeholder');   // first render: placeholder, then real
```

`useDeferredValue` перерендеривает в фоне с новым значением. Это не debounce: фиксированной задержки нет, и помогает он, только если медленную часть можно пропустить (например, через `memo`).

## Чтение промиса через use

```tsx
const namePromise = fetchName();          // created once, outside the component

function Name() { return <b>{use(namePromise)}</b>; }

<Suspense fallback={<p>loading</p>}><Name /></Suspense>
```

Промис должен быть кэширован: созданный во время рендера каждый раз новый. Отклонённый промис уходит в ближайшую error boundary. `use` нельзя вызывать внутри `try`/`catch`.

## Лучшие практики

- Помечайте навигацию и тяжёлые обновления как transitions.
- Показывайте `isPending`, чтобы пользователь знал, что что-то происходит.
- Берите промисы данных у фреймворка или библиотеки данных, которые их кэшируют.

## Частые ошибки

- ❌ Создавать промис в компоненте → ✅ кэшировать.
- ❌ Устанавливать состояние после `await` без `startTransition` → ✅ обернуть снова.
- ❌ `useDeferredValue` с немемоизированным потомком → ✅ он всё равно перерендерится.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [React: Suspense](https://react.dev/reference/react/Suspense)
- [React: useTransition](https://react.dev/reference/react/useTransition)
- [React: useDeferredValue](https://react.dev/reference/react/useDeferredValue)
- [React: use](https://react.dev/reference/react/use)
