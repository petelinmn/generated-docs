---
title: "Refs и DOM: базовая теория"
lang: ru
track: react
kind: module
doc: basics
slug: 07-refs-and-dom
part: 2
tags: [react, refs, dom, portals, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Refs и DOM: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [эффекты](../06-effects/overview.ru.md) · **Время:** ~1,5 ч

Все результаты получены на React 19.3.1 в jsdom.

## Ref на узел DOM

```tsx
function Search() {
  const input = useRef<HTMLInputElement>(null);
  return (
    <>
      <input ref={input} />
      <button onClick={() => input.current?.focus()}>Focus</button>
    </>
  );
}
```

Передайте ref в атрибут `ref` элемента, и после фиксации `ref.current` будет этим узлом.

## Ref или состояние

| | Ref | Состояние |
|---|---|---|
| Изменение вызывает рендер | нет | да |
| Читать при рендере | нет | да |
| Значение после изменения | сразу | на следующем рендере |
| Типичное содержимое | узел DOM, id таймера | то, что показывают |

```tsx
const ref = useRef(0);
<button onClick={() => { ref.current++; }}>{ref.current}</button>
// two clicks: ref.current is 2, no render, the button still shows 0
// after any state change elsewhere in the component it shows 2
```

## Когда ref доступен

```tsx
function When() {
  const ref = useRef<HTMLDivElement>(null);
  log.push(ref.current === null ? 'render null' : 'render DIV');
  useEffect(() => { log.push('effect ' + ref.current!.tagName); });
  return <div ref={ref} />;
}
// first mount:  render null | effect DIV
```

Во время первого рендера узла ещё нет. Используйте ref в обработчиках событий и эффектах.

## Запоминание значения

```tsx
const id = useRef<ReturnType<typeof setTimeout>>(undefined);
const start = () => { clearTimeout(id.current); id.current = setTimeout(send, 1000); };
const cancel = () => clearTimeout(id.current);
```

## Ошибки новичка

- ❌ Выводить `ref.current` в JSX → ✅ состояние.
- ❌ Использовать ref при рендере → ✅ в обработчике или эффекте.
- ❌ Удалять узел React через `remove()` → ✅ менять состояние.
- ❌ Переменная модуля для id таймера → ✅ ref: у каждого экземпляра свой.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: useRef](https://react.dev/reference/react/useRef)
- [React: работа с DOM через ref](https://react.dev/learn/manipulating-the-dom-with-refs)
