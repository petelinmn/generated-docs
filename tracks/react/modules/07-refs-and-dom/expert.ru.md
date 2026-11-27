---
title: "Refs и DOM: экспертная теория"
lang: ru
track: react
kind: module
doc: expert
slug: 07-refs-and-dom
part: 2
tags: [react, refs, dom, portals, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Refs и DOM: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

`useRef` возвращает один и тот же объект при каждом рендере экземпляра; React внутрь него не заглядывает. Для `ref` на элементе React подключает его при фиксации и отключает, когда элемент исчезает или меняется ref.

### Узкий интерфейс

```tsx
type Handle = { focus(): void; clear(): void };

function Field({ ref }: { ref: Ref<Handle> }) {
  const input = useRef<HTMLInputElement>(null);
  useImperativeHandle(ref, () => ({
    focus: () => input.current!.focus(),
    clear: () => { input.current!.value = ''; },
  }), []);
  return <input ref={input} defaultValue="abc" />;
}
// the parent's ref has only focus and clear, not the DOM node
```

## Справочник

| API | Назначение |
|---|---|
| `useRef(initial)` | постоянная ячейка `{ current }` |
| `ref={object}` | React кладёт узел DOM в `current` |
| `ref={fn}` | вызывается с узлом; может вернуть очистку |
| `useImperativeHandle(ref, create, deps)` | что получит ref родителя |
| `createPortal(children, node)` | вывод в другой узел DOM |
| `flushSync(fn)` | применить обновления немедленно |

| Задача | Метод узла |
|---|---|
| Фокус | `focus()`, `blur()` |
| Прокрутка | `scrollIntoView()` |
| Размер и положение | `getBoundingClientRect()` |
| Медиа | `play()`, `pause()` |
| Нативный диалог | `showModal()`, `close()` |

## Подвохи

### Встроенный callback ref переподключается

```tsx
<p ref={() => { log.push('attach'); return () => { log.push('cleanup'); }; }}>{n}</p>
// render n=1, then n=2:  attach | cleanup | attach
```

### Запись в ref во время рендера

```tsx
function Dirty() {
  const r = useRef(0);
  r.current++;                 // writing a ref during render
  return <p>{r.current}</p>;
}
// inside <StrictMode>, the first screen shows 2
```

### DOM ещё не обновлён

```tsx
setItems(i => [...i, 'p']);
list.current!.lastElementChild!.textContent;     // 'a'  — the DOM is not updated yet

flushSync(() => setItems(i => [...i, 's']));
list.current!.lastElementChild!.textContent;     // 's'
```

### Компонент, не использующий `ref`

```tsx
function MyInput({ ref }: { ref?: Ref<HTMLInputElement> }) {
  return <input ref={ref} />;          // React 19: ref is a prop
}
// a component that does not use the ref prop leaves ref.current === null
```

## Компромиссы и альтернативы

| Задача | Вариант | Замечание |
|---|---|---|
| Управлять потомком | props | обычный путь |
| | интерфейс через ref | императивно: фокус, прокрутка, воспроизведение |
| Слой поверх всего | портал | фокусом управляете сами |
| | нативные `<dialog>` и popover | браузер берёт на себя многое |
| Следить за размером | `ResizeObserver` в эффекте | вместо опроса |

## Вопросы с собеседований

**1. Ref против состояния?**

<details><summary>Ответ</summary>

Ref меняется сразу и не вызывает рендер; состояние вызывает и служит для отображаемого.

</details>

**2. Почему не читать ref при рендере?**

<details><summary>Ответ</summary>

Рендер должен быть чистым, а значение может быть устаревшим или ещё не заданным.

</details>

**3. Когда React задаёт ref на DOM?**

<details><summary>Ответ</summary>

При фиксации.

</details>

**4. Как держать ref для списка?**

<details><summary>Ответ</summary>

Один ref со словарём, заполняемым callback ref.

</details>

**5. Что изменилось у callback ref в React 19?**

<details><summary>Ответ</summary>

Они могут возвращать функцию очистки; без неё React вызывает их с `null`.

</details>

**6. Для чего `useImperativeHandle`?**

<details><summary>Ответ</summary>

Открыть родителю выбранные методы вместо узла DOM. Когда возможно, предпочитайте props.

</details>

**7. Что такое портал?**

<details><summary>Ответ</summary>

Вывод дочерних элементов в другой узел DOM с сохранением их в том же дереве React.

</details>

**8. Всплывают ли события из портала к родителю в React?**

<details><summary>Ответ</summary>

Да.

</details>

**9. Подвох: почему callback ref выполняется при каждом рендере?**

<details><summary>Ответ</summary>

Встроенная функция каждый раз новая, поэтому React отсоединяет прежнюю и подключает новую.

</details>

**10. Подвох: задали состояние и сразу прочитали DOM. Что увидите?**

<details><summary>Ответ</summary>

Старый DOM, если обновление не обёрнуто в `flushSync`.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: useRef](https://react.dev/reference/react/useRef)
- [React: работа с DOM через ref](https://react.dev/learn/manipulating-the-dom-with-refs)
- [React: common components, ref callback](https://react.dev/reference/react-dom/components/common)
- [React: useImperativeHandle](https://react.dev/reference/react/useImperativeHandle)
- [React: createPortal](https://react.dev/reference/react-dom/createPortal)
