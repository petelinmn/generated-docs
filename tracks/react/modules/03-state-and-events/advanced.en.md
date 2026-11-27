---
title: "State and events: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 03-state-and-events
part: 1
tags: [react, state, use-state, events, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# State and events: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## State is a snapshot

```tsx
onClick={() => { setN(n + 1); setN(n + 1); setN(n + 1); console.log(n); }}
// logs 0; the next render shows 1

onClick={() => { setN(x => x + 1); setN(x => x + 1); setN(x => x + 1); }}
// n grows by 3; one render

onClick={() => { setN(n + 5); setN(x => x + 1); }}
// from 4:  4 + 5 = 9, then 9 + 1 = 10
```

Handlers created during a render see that render's values. `setN` does not change `n`; it asks for the next render. A function passed to the setter receives the value queued before it.

## Batching and delayed code

```tsx
onClick={() => setTimeout(() => { setA(1); setB(1); }, 5)}     // one render, not two

onClick={() => { setN(n + 1); setTimeout(() => console.log(n), 5); }}
// the screen shows 1, the timeout logs 0
```

Since React 18, updates are batched automatically, including those in timeouts and promises. The timeout in the second example still holds the old snapshot.

## Objects

```tsx
const [u, setU] = useState({ name: 'Ann', age: 1 });

u.age++; setU(u);                       // screen: still 1 (same object)
setU({ ...u, age: u.age + 1 });         // screen: 3 (the mutation had already made age 2)
```

React compares the new value with the old one with `Object.is`. A mutated object is the same object, so nothing is requested.

## Arrays

```tsx
// list starts as [3, 1, 2]
list.push(9); setList(list);                    // screen: 3,1,2   (nothing happens)
setList([...list, 4]);                          // screen: 3,1,2,9,4   (the 9 surfaces now)
setList(list.toSorted((a, b) => a - b));        // 1,2,3,4,9   (TypeScript: lib es2023)
setList(list.filter(x => x !== 1));             // 2,3,4,9
setList(list.map(x => x * 10));                 // 20,30,40,90
```

Use `filter`, `map`, spread and copies. `push`, `splice`, `sort` and `reverse` change the array in place.

## Where state lives

```tsx
function Field({ label }: { label: string }) {
  const [n, setN] = useState(0);
  return <button onClick={() => setN(n + 1)}>{label}:{n}</button>;
}

<Field label={who} />               // click, then switch who from ann to bob:  bob:1
<Field key={who} label={who} />     // the same steps:                          bob:0

<><Field label="a" /><Field label="b" /></>   // click the first:  a:1  b:0
```

State is tied to the position in the render tree, not to the JSX tag you wrote. The same component at the same position keeps its state when props change; a different `key` gives a new one.

A different element type higher up also resets everything below:

```tsx
{b ? <section><Counter /></section> : <div><Counter /></div>}
// count to 1, then toggle b:  the counter is 0 again — the parent type changed
```

## Best practices

- Store the minimum; compute the rest during render.
- Keep related values together, independent ones separate.
- Use updater functions when the new value depends on the old.
- Reset a whole form by changing its `key`.

## Common mistakes

- ❌ Mutating then setting the same object → ✅ create a copy.
- ❌ Copying props into state → ✅ use the prop.
- ❌ Declaring a component inside a component → ✅ at module level.
- ❌ Storing both a list and its length → ✅ compute the length.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: useState](https://react.dev/reference/react/useState)
- [React: responding to events](https://react.dev/learn/responding-to-events)
- [React: state as a snapshot](https://react.dev/learn/state-as-a-snapshot)
- [React: preserving and resetting state](https://react.dev/learn/preserving-and-resetting-state)
- [React: updating arrays in state](https://react.dev/learn/updating-arrays-in-state)
- [React: flushSync](https://react.dev/reference/react-dom/flushSync)
- [React: rules of Hooks](https://react.dev/reference/rules/rules-of-hooks)
- [React 18: automatic batching](https://react.dev/blog/2022/03/29/react-v18)
