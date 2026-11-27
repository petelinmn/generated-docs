---
title: "Refs и DOM: продвинутая теория"
lang: ru
track: react
kind: module
doc: advanced
slug: 07-refs-and-dom
part: 2
tags: [react, refs, dom, portals, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Refs и DOM: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Callback ref

Вместо объекта `ref` принимает функцию.

```tsx
<p ref={el => {
  box.current = el;                      // called with the node
  return () => { box.current = null; };  // React 19: cleanup on removal
}}>x</p>

// without a returned cleanup React calls the callback again with null:  element | null
```

## Много элементов

Хуки нельзя вызывать в цикле, поэтому держите один ref со словарём:

```tsx
const map = useRef(new Map<string, HTMLLIElement>());

{items.map(x => (
  <li key={x} ref={el => { map.current.set(x, el!); return () => { map.current.delete(x); }; }}>{x}</li>
))}

// later:  map.current.get('b')?.scrollIntoView();
```

## Передача ref своему компоненту

```tsx
function MyInput({ ref }: { ref?: Ref<HTMLInputElement> }) {
  return <input ref={ref} />;          // React 19: ref is a prop
}
// a component that does not use the ref prop leaves ref.current === null
```

До React 19 для этого был нужен `forwardRef`.

## Обновить, затем использовать DOM

```tsx
setItems(i => [...i, 'p']);
list.current!.lastElementChild!.textContent;     // 'a'  — the DOM is not updated yet

flushSync(() => setItems(i => [...i, 's']));
list.current!.lastElementChild!.textContent;     // 's'
```

`flushSync` заставляет обновление произойти сразу. Он вредит производительности, поэтому используйте его, только когда новый DOM нужен немедленно, например чтобы прокрутить к добавленному элементу.

## Порталы

```tsx
function Modal() {
  return (
    <div onClick={() => log.push('parent heard')}>
      {createPortal(<button>in</button>, document.getElementById('modal')!)}
    </div>
  );
}
// DOM:    #root contains <div></div>;  #modal contains <button>in</button>
// click the button:  parent heard
```

Портал меняет только то, где лежит узел DOM. Context и события следуют дереву React. Порталы применяют для диалогов, подсказок и меню, которым нужно выйти за `overflow` или порядок наложения родителя. Диалогу всё равно нужны управление фокусом и поддержка клавиатуры.

## Лучшие практики

- Если есть декларативные props (`autoFocus`, `open`), предпочитайте их.
- Очищайте таймеры и наблюдатели, хранящиеся в ref.
- Открывайте узкий интерфейс, а не узел DOM.

## Частые ошибки

- ❌ Новый callback ref с дорогой работой при каждом рендере → ✅ определить снаружи или мемоизировать.
- ❌ Читать раскладку в `useEffect` и затем двигать элементы → ✅ `useLayoutEffect`.
- ❌ Ждать, что события из портала не дойдут до родителей в React → ✅ доходят.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [React: useRef](https://react.dev/reference/react/useRef)
- [React: работа с DOM через ref](https://react.dev/learn/manipulating-the-dom-with-refs)
- [React: common components, ref callback](https://react.dev/reference/react-dom/components/common)
- [React: useImperativeHandle](https://react.dev/reference/react/useImperativeHandle)
- [React: createPortal](https://react.dev/reference/react-dom/createPortal)
