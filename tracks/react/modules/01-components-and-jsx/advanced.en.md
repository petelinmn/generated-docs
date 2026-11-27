---
title: "Components and JSX: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 01-components-and-jsx
part: 1
tags: [react, jsx, components, rendering]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Components and JSX: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## What can be rendered

| Value as a child | Result |
|---|---|
| string, number | text, including `0` and `NaN` |
| element | rendered |
| array | each item in order |
| `false`, `true`, `null`, `undefined`, `''` | nothing visible |
| plain object | error |

```tsx
<p>{0}{false}{null}{undefined}{true}{''}{'x'}{NaN}</p>   // <p>0xNaN</p>
<p>{[1, 2, 3]}</p>                                       // <p>123</p>

const items: string[] = [];
<div>{items.length && <b>has</b>}</div>       // <div>0</div>
<div>{items.length > 0 && <b>has</b>}</div>   // <div></div>

<div>{false ?? 'a'}{0 || 'b'}{0 ?? 'c'}</div>  // <div>b0</div>
```

`items.length && ...` prints `0` for an empty list: put a boolean on the left of `&&`.

## Dynamic tags and components

```tsx
const UI = { Badge: ({ text }: { text: string }) => <mark>{text}</mark> };
<UI.Badge text="new" />          // <mark>new</mark>

const tag = 'section';
const Tag = tag;                 // must start with a capital letter
<Tag>dyn</Tag>                   // <section>dyn</section>

function Maybe({ show }: { show: boolean }) {
  if (!show) return null;        // renders nothing
  return <b>yes</b>;
}
```

The case of the first letter decides what JSX means: `<section>` is the string `'section'`, `<Tag>` is the value of the variable `Tag`.

## Escaping and raw HTML

```tsx
<p>{'<img src=x onerror=alert(1)>'}</p>
// <p>&lt;img src=x onerror=alert(1)&gt;</p>

<div dangerouslySetInnerHTML={{ __html: '<b>raw</b>' }} />
// <div><b>raw</b></div>
```

Text from data is escaped. `dangerouslySetInnerHTML` switches that off, so use it only with HTML you have sanitised.

## Purity

A component should compute its output from its input and change nothing that existed before the call. React may call it more than once.

```tsx
let guests = 0;
function Impure() {
  guests++;                       // side effect during render
  return <h2>Guest {guests}</h2>;
}

// client render in development inside <StrictMode>, two <Impure />:
// <h2>Guest 2</h2><h2>Guest 4</h2>
```

`StrictMode` calls component functions twice in development to expose such mistakes. It does not do this in production.

## Best practices

- One component per responsibility; extract repeated markup.
- Declare components at the top level of a module, not inside other components.
- Move complex conditions out of JSX into variables above `return`.
- Use semantic elements and `aria-*` attributes.

## Common mistakes

- ❌ `count && <Badge />` with a number → ✅ `count > 0 && <Badge />`.
- ❌ Changing a global variable during render → ✅ compute locally.
- ❌ Raw HTML from users in `dangerouslySetInnerHTML` → ✅ sanitise or render as text.
- ❌ A deeply nested ternary → ✅ variables or smaller components.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: writing markup with JSX](https://react.dev/learn/writing-markup-with-jsx)
- [React: render and commit](https://react.dev/learn/render-and-commit)
- [React: StrictMode](https://react.dev/reference/react/StrictMode)
