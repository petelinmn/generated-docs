---
title: "Props и композиция: экспертная теория"
lang: ru
track: react
kind: module
doc: expert
slug: 02-props-and-composition
part: 1
tags: [react, props, composition, children, typescript]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Props и композиция: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Каждый рендер родителя создаёт новые объекты элементов с новыми объектами props. React сравнивает элементы в каждой позиции; если элемент — тот же самый объект, что и раньше, и ничто другое не требует обновления, поддерево не рендерится заново. Поэтому содержимое, переданное сверху, ведёт себя иначе, чем созданное внутри.

### Что лежит в children

`children` — это то, что написано: ничего, одно значение или массив.

```tsx
<Count />                       // Children.count: 0, children is undefined
<Count>a</Count>                // 1, a string
<Count>{'a'}{'b'}</Count>       // 2, an array
<Count>{false}</Count>          // 1, a boolean: empty nodes are counted too
```

`Children.count` считает и пустые узлы и не заглядывает внутрь фрагментов и отрендеренных компонентов.

### Клонирование

```tsx
function Highlight({ children }: { children: ReactElement<{ className?: string }> }) {
  return cloneElement(children, { className: 'hl' });
}
<Highlight><p className="old">x</p></Highlight>   // <p class="hl">x</p>
```

`cloneElement` накладывает новые props поверх старых. Документация React называет и `Children`, и `cloneElement` редкими средствами, ведущими к хрупкому коду; альтернативы — render props, context, массив данных или явный компонент-обёртка.

## Справочник

| Приём | Синтаксис | Для чего |
|---|---|---|
| Prop со значением по умолчанию | `{ size = 40 }` | необязательные настройки |
| Краткий булев | `<input disabled />` | флаги `true` |
| Spread | `<A {...props} />` | передача дальше |
| Rest | `({ a, ...rest })` | приём нативных атрибутов |
| `children` | `<A>content</A>` | обёртка |
| Props-слоты | `footer={<B />}` | несколько областей |
| Render prop | `renderItem={x => <B x={x} />}` | содержимое, которому нужны данные компонента |
| Callback prop | `onSelect={id => ...}` | сообщить о событии |
| Prop `ref` | `<A ref={r} />` | доступ к узлу DOM (React 19) |

| Тип | Смысл |
|---|---|
| `ReactNode` | всё, что может быть дочерним |
| `ReactElement<P>` | один элемент с props `P` |
| `ComponentProps<'tag'>` | props нативного элемента |
| `PropsWithChildren<P>` | `P` плюс необязательный `children` |

## Подвохи

### Порядок spread решает, кто победит

```tsx
<Show a="1" {...{ a: '2', b: '3' }} />   // props: {"a":"2","b":"3"}
<Show {...{ a: '2' }} a="1" />           // props: {"a":"1"}
<Show key="k1" a="x" />                  // props: {"a":"x"}   — key is not passed
```

### В разработке props заморожены

```tsx
function Mutate(props: { a: string }) {
  props.a = 'z';     // TypeError in development: the props object is frozen
  return null;
}
```

Не полагайтесь на ошибку: менять props неправильно в любой сборке.

### Состояние, скопированное из prop, за ним не следует

```tsx
function Name({ name }: { name: string }) {
  const [copy] = useState(name);        // taken only on the first render
  return <p>{copy}|{name}</p>;
}

// render <Name name="Ann" />, then <Name name="Bob" /> in the same place:
// <p>Ann|Bob</p>
```

Используйте prop напрямую. Если копия нужна намеренно, назовите prop `initialName`.

### Объект по умолчанию новый при каждом рендере

`function List({ items = [] })` создаёт новый массив всякий раз, когда prop не передан, и это ломает сравнение по ссылке в мемоизированных потомках и зависимостях эффектов. Объявляйте значение по умолчанию вне компонента.

## Компромиссы и альтернативы

| Подход | Плюс | Минус |
|---|---|---|
| Много мелких props | явно | длинные списки |
| Один prop-объект | коротко | легко создавать новый объект при каждом рендере |
| `children` и слоты | гибко, меньше рендеров | компонент не управляет содержимым |
| Render prop | доступ к внутренним данным | многословнее |
| Context | не нужно передавать через слои | скрытая зависимость |
| Составные компоненты с context | декларативный API | больше кода |

## Вопросы с собеседований

**1. Может ли компонент менять свои props?**

<details><summary>Ответ</summary>

Нет. Они только для чтения; новые значения приходят с новым рендером от родителя.

</details>

**2. Когда используется значение по умолчанию?**

<details><summary>Ответ</summary>

Когда prop отсутствует или равен `undefined`; не для `null`, `0` и `false`.

</details>

**3. Что такое `children`?**

<details><summary>Ответ</summary>

Prop с тем, что написано между тегами компонента.

</details>

**4. Как потомок передаёт данные родителю?**

<details><summary>Ответ</summary>

Вызывая функцию, полученную в props.

</details>

**5. Что такое подъём состояния?**

<details><summary>Ответ</summary>

Перенос состояния в ближайшего общего родителя тех компонентов, которым оно нужно.

</details>

**6. Композиция или наследование?**

<details><summary>Ответ</summary>

Код на React переиспользует интерфейс композицией: обёртки, слоты, специализация.

</details>

**7. Почему не советуют `Children` и `cloneElement`?**

<details><summary>Ответ</summary>

Они редко нужны и ведут к хрупкому коду; render props, context и явные компоненты понятнее.

</details>

**8. Как `children` уменьшает число рендеров?**

<details><summary>Ответ</summary>

Обёртка получает готовые элементы, и обновление её состояния не требует рендерить их заново.

</details>

**9. Подвох: может ли компонент прочитать `props.key`?**

<details><summary>Ответ</summary>

Нет. `key` использует React и не передаёт дальше; передайте значение под другим именем.

</details>

**10. Подвох: сделали `useState(props.name)`, а родитель изменил `name`. Что покажется?**

<details><summary>Ответ</summary>

Старое значение из состояния: начальное значение используется один раз.

</details>

**11. Как типизировать обёртку над нативным элементом?**

<details><summary>Ответ</summary>

`ComponentProps<'button'>` в пересечении с собственными props.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: передача props компоненту](https://react.dev/learn/passing-props-to-a-component)
- [React: мышление в стиле React](https://react.dev/learn/thinking-in-react)
- [React: общее состояние компонентов](https://react.dev/learn/sharing-state-between-components)
- [React: Children](https://react.dev/reference/react/Children)
- [React: cloneElement](https://react.dev/reference/react/cloneElement)
- [React: выбор структуры состояния](https://react.dev/learn/choosing-the-state-structure)
