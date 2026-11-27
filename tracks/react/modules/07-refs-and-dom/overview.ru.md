---
title: "Refs и DOM"
lang: ru
track: react
kind: module
doc: overview
slug: 07-refs-and-dom
part: 2
levels: [1, 2, 3, 4]
tags: [react, refs, dom, portals, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Refs и DOM

🌐 **Русский** · [English](overview.en.md)

## Коротко

Ref — это ячейка, переживающая рендеры, изменение которой рендер не вызывает. У неё два назначения: помнить значения, от которых экран не зависит (идентификаторы таймеров, прежние значения), и добираться до узлов DOM, созданных React, чтобы поставить фокус, прокрутить или измерить. Родственный инструмент — портал (portal): он помещает часть интерфейса в другое место DOM, оставляя её в том же дереве React.

## Ключевые идеи

- `ref.current` можно менять без рендера.
- Не читайте и не записывайте его во время рендера.
- Ref на DOM React задаёт при фиксации, после рендера.
- Callback ref может вернуть функцию очистки (React 19).
- Функциональные компоненты получают `ref` как prop (React 19).
- `useImperativeHandle` открывает небольшой API вместо всего узла.
- События из портала всплывают по дереву React.

## Минимальный пример

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

## Когда применять, а когда нет

- ✅ Фокус, выделение текста, прокрутка, измерения, управление медиа.
- ✅ Идентификаторы таймеров и другие не отображаемые значения.
- ❌ Данные, которые видны на экране: это состояние.
- ❌ Изменение узлов DOM, которыми управляет React.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `useRef`, доступ к DOM, ref против состояния |
| [2. Продвинутая теория](advanced.ru.md) | callback ref, списки ref, `ref` как prop, `flushSync`, порталы |
| [3. Экспертная теория](expert.ru.md) | `useImperativeHandle`, справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn react 07-refs-and-dom level 5` |
| 6. Тесты | ещё не создан — `/learn react 07-refs-and-dom level 6` |

## Связанные материалы

- Пререквизиты: [06 · Эффекты](../06-effects/overview.ru.md)
- Сам API DOM: [DOM и события в браузере](../../../javascript/modules/17-browser-dom-and-events/overview.ru.md)
- `ref` как prop в композиции: [02 · Props и композиция](../02-props-and-composition/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте React](../../overview.ru.md)
