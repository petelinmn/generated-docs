---
title: "Эффекты: экспертная теория"
lang: ru
track: react
kind: module
doc: expert
slug: 06-effects
part: 2
tags: [react, effects, use-effect, hooks, cleanup]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Эффекты: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

После рендера React фиксирует изменения в DOM, а затем обрабатывает эффекты: для каждого эффекта с изменившимися зависимостями выполняет старую очистку и новую установку. Обычно эффекты выполняются после того, как браузер отрисовал экран, но React может запустить их и раньше, например когда обновление вызвано взаимодействием. Не полагайтесь на точный момент.

### useEffectEvent

Появился в React 19.2. Событие эффекта (effect event) — это функция, которая всегда видит свежие props и состояние и не является причиной перезапуска эффекта.

```tsx
function Chat({ room, theme }: { room: string; theme: string }) {
  const onMessage = useEffectEvent(() => { show(room, theme); });   // sees the latest props

  useEffect(() => {
    const connection = createConnection(room);
    connection.on('message', onMessage);
    connection.connect();
    return () => connection.disconnect();
  }, [room]);                                  // theme is not a reason to reconnect
}
// theme changes from light to dark: one connection, and the next message sees "dark"
```

Правила: вызывайте только из эффектов (включая layout- и insertion-эффекты) или других событий эффекта, не во время рендера; не передавайте другим компонентам и хукам; не указывайте в зависимостях.

## Справочник

| Хук | Выполняется | Для чего |
|---|---|---|
| `useEffect` | после фиксации | синхронизация с внешними системами |
| `useLayoutEffect` | до перерисовки, блокируя её | измерение и исправление раскладки |
| `useInsertionEffect` | до layout-эффектов | вставка стилей библиотеками CSS-in-JS |
| `useEffectEvent` | когда его вызывает эффект | читать свежие значения без перезапуска |

| Событие | Что вызывает React |
|---|---|
| монтирование | установку |
| изменилась зависимость | старую очистку, затем новую установку |
| размонтирование | очистку |
| StrictMode, разработка | лишние установку и очистку при монтировании |

## Подвохи

### Async-функция в роли эффекта

```tsx
useEffect(async () => { await load(); }, []);
// console: ... must not return anything besides a function, which is used for clean-up.
// on unmount: TypeError: destroy is not a function

useEffect(() => {
  async function run() { await load(); }
  run();
}, []);
```

### Зависимость, новая при каждом рендере

```tsx
const options = { v };                               // a new object on every render
useEffect(() => { counts.none++; });                 // no array
useEffect(() => { counts.empty++; }, []);
useEffect(() => { counts.obj++; }, [options]);
useEffect(() => { counts.prim++; }, [v]);
// three renders with the same v:  {"none":3,"empty":1,"obj":3,"prim":1}
```

### Состояние, задаваемое эффектом, который от него зависит

```tsx
const [o, setO] = useState({});
useEffect(() => { setO({}); }, [o]);    // a new object each time: never settles
// console: Maximum update depth exceeded. This can happen when a component calls setState inside useEffect, ...
```

### Эффект, вычисляющий данные, рендерит дважды

```tsx
// avoid: an extra render with an outdated value
const [full, setFull] = useState('');
useEffect(() => { setFull(first + ' Smith'); }, [first]);
// renders:  "" then "Ann Smith"

// instead
const full = first + ' Smith';
```

## Компромиссы и альтернативы

| Задача | Вариант | Замечание |
|---|---|---|
| Загрузка данных | эффект с `fetch` | гонки, ошибки и кэширование на вас |
| | библиотека данных | кэш, повторы, объединение запросов |
| | загрузчик фреймворка или Server Components | данные до показа страницы |
| Внешнее хранилище | `useSyncExternalStore` | создан для подписок |
| Производные данные | вычисление, `useMemo`, если дорого | без эффекта |

## Вопросы с собеседований

**1. Когда выполняется эффект?**

<details><summary>Ответ</summary>

После фиксации, на клиенте. Момент относительно отрисовки может различаться.

</details>

**2. Зачем нужна очистка?**

<details><summary>Ответ</summary>

Отменить установку перед следующим запуском и при размонтировании.

</details>

**3. `[]` против отсутствия массива?**

<details><summary>Ответ</summary>

`[]` — один раз после первой фиксации; без массива — после каждой.

</details>

**4. Как сравниваются зависимости?**

<details><summary>Ответ</summary>

Через `Object.is`.

</details>

**5. Почему в разработке эффект запускается дважды?**

<details><summary>Ответ</summary>

StrictMode добавляет цикл установки и очистки, чтобы выявить недостающую очистку.

</details>

**6. `useEffect` против `useLayoutEffect`?**

<details><summary>Ответ</summary>

Layout-эффект выполняется до перерисовки и блокирует её; его используют, чтобы измерить и поправить раскладку.

</details>

**7. Что такое `useEffectEvent`?**

<details><summary>Ответ</summary>

Способ вызывать из эффекта логику, читающую свежие props и состояние, не заставляя эффект перезапускаться.

</details>

**8. Как обрабатывать ответы не по порядку?**

<details><summary>Ответ</summary>

Ставить в очистке флаг игнорирования или отменять запрос.

</details>

**9. Подвох: почему эффект не может быть `async`?**

<details><summary>Ответ</summary>

Он вернул бы промис вместо функции очистки; React падает, пытаясь его вызвать.

</details>

**10. Подвох: выполняются ли эффекты на сервере?**

<details><summary>Ответ</summary>

Нет.

</details>

**11. Что раньше: эффект родителя или потомка?**

<details><summary>Ответ</summary>

Потомка.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: useEffect](https://react.dev/reference/react/useEffect)
- [React: возможно, эффект не нужен](https://react.dev/learn/you-might-not-need-an-effect)
- [React: useLayoutEffect](https://react.dev/reference/react/useLayoutEffect)
- [React: useEffectEvent](https://react.dev/reference/react/useEffectEvent)
- [React 19.2](https://react.dev/blog/2025/10/01/react-19-2)
