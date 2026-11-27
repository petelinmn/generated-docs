---
title: "Props and composition: basics"
lang: en
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

# Props and composition: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [components and JSX](../01-components-and-jsx/overview.en.md) · **Time:** ~2 h

All results were produced with React 19.3.1; HTML is shown as `renderToStaticMarkup` returns it. The typed examples pass `tsc --strict`.

## Props are the arguments of a component

```tsx
function Avatar({ name, size = 40 }: { name: string; size?: number }) {
  return <img alt={name} width={size} />;
}

<Avatar name="Ann" />                     // <img alt="Ann" width="40"/>
<Avatar name="Bob" size={undefined} />    // <img alt="Bob" width="40"/>
<Avatar name="Eve" size={null} />         // <img alt="Eve"/>   (a type error in TypeScript)
```

A component receives one object, usually destructured right in the parameter list. A prop written without a value (`<input disabled />`) is `true`.

## Types

```tsx
function Avatar({ name }: { name: string; size?: number }) { return <img alt={name} />; }

<Avatar />                       // error TS2741: Property 'name' is missing
<Avatar name="x" colour="red" /> // error TS2322: Property 'colour' does not exist
<Avatar name={5} />              // error TS2322: Type 'number' is not assignable to type 'string'
```

TypeScript checks the call site: a missing, unknown or wrongly typed prop is a compile error. Types disappear at run time, so data from outside still has to be validated.

## children

Whatever stands between the opening and closing tags arrives as the `children` prop. Any other prop may hold elements too:

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

## Data down, events up

```tsx
function Child({ value, onIncrement }: { value: number; onIncrement: () => void }) {
  return <button onClick={onIncrement}>{value}</button>;
}

function Parent() {
  const [v, setV] = useState(0);          // state lives in the common parent
  return <><Child value={v} onIncrement={() => setV(x => x + 1)} /><p>{v}</p></>;
}
```

The child owns nothing: it shows `value` and calls `onIncrement`. When two components need the same data, move it to their closest common parent ("lifting state up").

## Beginner mistakes

- ❌ `props.title = 'x'` → ✅ props are read-only; ask the owner to change the data.
- ❌ `onClick={onSave()}` → ✅ `onClick={onSave}` or `onClick={() => onSave(id)}`.
- ❌ One component with ten boolean flags → ✅ pass content or write several components.
- ❌ Expecting a default for `null` → ✅ defaults cover only `undefined`.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: passing props to a component](https://react.dev/learn/passing-props-to-a-component)
- [React: thinking in React](https://react.dev/learn/thinking-in-react)
- [React: sharing state between components](https://react.dev/learn/sharing-state-between-components)
