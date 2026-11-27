---
title: "Props и композиция"
lang: ru
track: react
kind: module
doc: overview
slug: 02-props-and-composition
part: 1
levels: [1, 2, 3, 4]
tags: [react, props, composition, children, typescript]
versions: "React 19.3"
status: draft
updated: 2026-11-26
---

# Props и композиция

🌐 **Русский** · [English](overview.en.md)

## Коротко

Props — это входные данные компонента: родитель их передаёт, дочерний читает и никогда не меняет. Композиция — построение крупных компонентов из мелких, в основном передачей элементов через `children` или другие props. Вместе они определяют, как данные идут по приложению и насколько переиспользуемы его части.

## Ключевые идеи

- Props только для чтения; данные идут от родителя к потомку.
- Потомок сообщает наверх, вызывая функцию, полученную через props.
- Значение по умолчанию действует, когда prop отсутствует или равен `undefined`, но не `null`.
- `children` и props с элементами позволяют обернуть содержимое, о котором компонент ничего не знает.
- Общие данные живут в ближайшем общем родителе.
- `...rest` позволяет принимать всё, что принимает нижележащий элемент.
- Передача элементов через `children` может избавить от лишних рендеров.

## Минимальный пример

```tsx
function Avatar({ name, size = 40 }: { name: string; size?: number }) {
  return <img alt={name} width={size} />;
}

<Avatar name="Ann" />                     // <img alt="Ann" width="40"/>
<Avatar name="Bob" size={undefined} />    // <img alt="Bob" width="40"/>
<Avatar name="Eve" size={null} />         // <img alt="Eve"/>   (a type error in TypeScript)
```

## Когда применять, а когда нет

- ✅ Переиспользуемые части: кнопки, карточки, диалоги, раскладки.
- ✅ Передавать содержимое вместо множества булевых флагов.
- ❌ Копировать prop в состояние «на всякий случай»: копия не обновится.
- ❌ Протаскивать значение через много слоёв, которым оно не нужно; подумайте о композиции или context.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | props, значения по умолчанию, типы TypeScript, `children`, callbacks |
| [2. Продвинутая теория](advanced.ru.md) | остальные props, слоты, render props, специализация, ref, повторный рендер |
| [3. Экспертная теория](expert.ru.md) | `Children` и `cloneElement`, справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn react 02-props-and-composition level 5` |
| 6. Тесты | ещё не создан — `/learn react 02-props-and-composition level 6` |

## Связанные материалы

- Пререквизиты: [01 · Компоненты и JSX](../01-components-and-jsx/overview.ru.md)
- Типизация props: [трек TypeScript](../../../typescript/overview.ru.md)
- Деструктуризация и rest: [объекты в JavaScript](../../../javascript/modules/05-objects-and-prototypes/overview.ru.md)
- Та же идея в других местах: [композиция в архитектуре приложений](../../../csharp/modules/21-architecture/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте React](../../overview.ru.md)
- [03 · Состояние и события](../03-state-and-events/overview.ru.md)
- [07 · Refs и DOM](../07-refs-and-dom/overview.ru.md)
- [08 · Context и редьюсеры](../08-context-and-reducers/overview.ru.md)
- [11 · Производительность рендера](../11-performance/overview.ru.md)
