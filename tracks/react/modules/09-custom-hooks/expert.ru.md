---
title: "Пользовательские хуки: экспертная теория"
lang: ru
track: react
kind: module
doc: expert
slug: 09-custom-hooks
part: 2
tags: [react, custom-hooks, hooks, use-sync-external-store, reuse]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Пользовательские хуки: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Для React пользовательский хук не является чем-то особым: его вызовы хуков просто входят в список хуков вызывающего компонента и сопоставляются по порядку. Поэтому правила хуков действуют и внутри него, а у каждого вызывающего компонента состояние своё.

### useDebugValue

`useDebugValue(label, format?)` добавляет подпись вашему хуку в React DevTools; функция форматирования выполняется только при осмотре компонента. Полезен в основном в общих библиотеках.

## Справочник

| API | Назначение |
|---|---|
| собственный `useX` | переиспользовать логику с состоянием |
| `useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot?)` | читать внешнее хранилище |
| `useDebugValue` | подпись в DevTools |
| `useCallback`, `useMemo` | держать возвращаемые значения устойчивыми |
| `useEffectEvent` | нереактивная логика внутри эффекта |

| Что возвращать | Когда |
|---|---|
| одно значение | один результат |
| `[value, action] as const` | ведёт себя как состояние |
| объект | три и более именованных члена |

## Подвохи

### getSnapshot, возвращающий новый объект

```tsx
useSyncExternalStore(store.subscribe, () => ({ v: n }));   // a new object on every call
// console: The result of getSnapshot should be cached to avoid an infinite loop
// Error:   Maximum update depth exceeded. ...
```

### Неустойчивое возвращаемое значение

```tsx
function useBad() { return { value: 1 }; }          // a new object every render

const obj = useBad();
useEffect(() => { runs++; }, [obj]);
// three renders:  the effect ran 3 times
```

### Общий хук — не общие данные

```tsx
function Two({ id }: { id: string }) {
  const [on, toggle] = useToggle();
  return <button id={id} onClick={toggle}>{String(on)}</button>;
}
// <Two id="a" /><Two id="b" />, click a:  true false
```

## Компромиссы и альтернативы

| Что переиспользуем | Инструмент |
|---|---|
| вычисление | обычная функция |
| разметка | компонент |
| логика с состоянием | пользовательский хук |
| данные между компонентами | props, context или хранилище |
| данные сервера | библиотека данных с кэшем |

## Вопросы с собеседований

**1. Что такое пользовательский хук?**

<details><summary>Ответ</summary>

Функция с именем на `use`, вызывающая хуки и упаковывающая переиспользуемую логику.

</details>

**2. Делят ли два вызывающих состояние?**

<details><summary>Ответ</summary>

Нет.

</details>

**3. Зачем приставка `use`?**

<details><summary>Ответ</summary>

Чтобы читатели и линтер знали, что действуют правила хуков.

</details>

**4. Откуда можно вызывать хуки?**

<details><summary>Ответ</summary>

С верхнего уровня компонентов и пользовательских хуков.

</details>

**5. Для чего `useSyncExternalStore`?**

<details><summary>Ответ</summary>

Подписка на хранилище вне React и чтение согласованного снимка.

</details>

**6. Что должен возвращать `getSnapshot`?**

<details><summary>Ответ</summary>

Одно и то же неизменяемое значение, пока хранилище не изменилось.

</details>

**7. Зачем объявлять `subscribe` вне компонента?**

<details><summary>Ответ</summary>

Новая функция при каждом рендере заставляет React подписываться заново.

</details>

**8. Что за третий аргумент у `useSyncExternalStore`?**

<details><summary>Ответ</summary>

Снимок для серверного рендера и гидратации; без него серверный рендер падает.

</details>

**9. Что такое `useDebugValue`?**

<details><summary>Ответ</summary>

Подпись пользовательского хука в React DevTools.

</details>

**10. Подвох: функция без `use` вызывает `useState`. Упадёт ли она?**

<details><summary>Ответ</summary>

Во время выполнения может работать, но линтер её не проверит, и это нарушает правила.

</details>

**11. Подвох: любая переиспользуемая функция — хук?**

<details><summary>Ответ</summary>

Нет, только вызывающие хуки.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: повторное использование логики в собственных хуках](https://react.dev/learn/reusing-logic-with-custom-hooks)
- [React: правила хуков](https://react.dev/reference/rules/rules-of-hooks)
- [React: useSyncExternalStore](https://react.dev/reference/react/useSyncExternalStore)
- [React: useDebugValue](https://react.dev/reference/react/useDebugValue)
- [React: useEffectEvent](https://react.dev/reference/react/useEffectEvent)
