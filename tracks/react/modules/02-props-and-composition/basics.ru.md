---
title: "Props и композиция: базовая теория"
lang: ru
track: react
kind: module
doc: basics
slug: 02-props-and-composition
part: 1
tags: [react, props, composition, children, typescript]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Props и композиция: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [компоненты и JSX](../01-components-and-jsx/overview.ru.md) · **Время:** ~2 ч

Все результаты получены на React 19.3.1; HTML показан так, как его возвращает `renderToStaticMarkup`. Типизированные примеры проходят `tsc --strict`.

## Props — аргументы компонента

```tsx
function Avatar({ name, size = 40 }: { name: string; size?: number }) {
  return <img alt={name} width={size} />;
}

<Avatar name="Ann" />                     // <img alt="Ann" width="40"/>
<Avatar name="Bob" size={undefined} />    // <img alt="Bob" width="40"/>
<Avatar name="Eve" size={null} />         // <img alt="Eve"/>   (a type error in TypeScript)
```

Компонент получает один объект, который обычно деструктурируют прямо в списке параметров. Prop, записанный без значения (`<input disabled />`), равен `true`.

## Типы

```tsx
function Avatar({ name }: { name: string; size?: number }) { return <img alt={name} />; }

<Avatar />                       // error TS2741: Property 'name' is missing
<Avatar name="x" colour="red" /> // error TS2322: Property 'colour' does not exist
<Avatar name={5} />              // error TS2322: Type 'number' is not assignable to type 'string'
```

TypeScript проверяет место вызова: пропущенный, неизвестный или неверно типизированный prop — ошибка компиляции. Во время выполнения типов нет, поэтому данные извне всё равно нужно проверять.

## children

Всё, что стоит между открывающим и закрывающим тегами, приходит в prop `children`. Элементы может нести и любой другой prop:

```tsx
function Card({ title, actions, children }: { title: string; actions?: ReactNode; children: ReactNode }) {
  return (
    <section>
      <h2>{title}</h2>
      {children}
      {actions && <footer>{actions}</footer>}
    </section>
  );
}

<Card title="T" actions={<Button>OK</Button>}><p>Body</p></Card>
// <section><h2>T</h2><p>Body</p><footer><button type="button" class="btn plain">OK</button></footer></section>
```

## Данные вниз, события вверх

```tsx
function Child({ value, onIncrement }: { value: number; onIncrement: () => void }) {
  return <button onClick={onIncrement}>{value}</button>;
}

function Parent() {
  const [v, setV] = useState(0);          // state lives in the common parent
  return <><Child value={v} onIncrement={() => setV(x => x + 1)} /><p>{v}</p></>;
}
```

Потомок ничем не владеет: показывает `value` и вызывает `onIncrement`. Когда двум компонентам нужны одни данные, их переносят в ближайшего общего родителя («подъём состояния»).

## Ошибки новичка

- ❌ `props.title = 'x'` → ✅ props только для чтения; попросите владельца изменить данные.
- ❌ `onClick={onSave()}` → ✅ `onClick={onSave}` или `onClick={() => onSave(id)}`.
- ❌ Один компонент с десятью булевыми флагами → ✅ передавайте содержимое или напишите несколько компонентов.
- ❌ Ждать значение по умолчанию для `null` → ✅ оно подставляется только вместо `undefined`.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: передача props компоненту](https://react.dev/learn/passing-props-to-a-component)
- [React: мышление в стиле React](https://react.dev/learn/thinking-in-react)
- [React: общее состояние компонентов](https://react.dev/learn/sharing-state-between-components)
