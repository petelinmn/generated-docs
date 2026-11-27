---
title: "Lists, keys and identity: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 04-lists-and-keys
part: 1
tags: [react, keys, lists, identity, state]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Lists, keys and identity: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## What a wrong key does

```tsx
function Item({ name }: { name: string }) {
  const [n, setN] = useState(0);
  return <li><button onClick={() => setN(n + 1)}>{name}:{n}</button></li>;
}

// items = ['a', 'b', 'c'];  click a once, then reverse, then delete the first item
```

| Step | `key={item}` (stable id) | `key={index}` | `key={Math.random()}` |
|---|---|---|---|
| after the click | `a:1 b:0 c:0` | `a:1 b:0 c:0` | `a:1 b:0 c:0` |
| reverse | `c:0 b:0 a:1` | `c:1 b:0 a:0` | `c:0 b:0 a:0` |
| delete the first | `b:0 a:1` | `b:1 a:0` | `b:0 a:0` |

With a stable id the state travels with its item. With an index the state stays at the position, and a different item inherits it. With a random key every render creates new items, so all state is lost.

The same happens to what React does not control: text typed into an uncontrolled input stays at its position when index keys are used and a row is inserted above.

## Several elements per item

```tsx
<dl>
  {rows.map(r => (
    <Fragment key={r.id}>        {/* <> ... </> cannot take a key */}
      <dt>{r.term}</dt>
      <dd>{r.definition}</dd>
    </Fragment>
  ))}
</dl>
```

## Creating ids

An id has to be created once, together with the data: by the server, or in the event handler that adds the item (`crypto.randomUUID()`), not during render.

## Resetting state on purpose

```tsx
<ProfileForm key={user.id} user={user} />   // another user: a new form with empty state
```

## Hiding versus removing

```tsx
<ul style={{ display: show ? 'block' : 'none' }}><Item name="a" /></ul>
// click, hide, show:  a:1  — hidden by CSS, the component stays mounted
```

Conditional rendering (`{show && <List />}`) removes the component and its state; CSS or the `hidden` attribute keeps it.

## Best practices

- Create ids where data is created.
- Keep the filter and sort criteria in state, not the filtered result.
- Move state up if it must survive removal of the item.

## Common mistakes

- ❌ `key={index}` in an editable list → ✅ a data id.
- ❌ `key={Math.random()}` to "force an update" → ✅ fix what is not updating.
- ❌ Composite keys built from editable text → ✅ the state is reset on every edit; use an id.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: rendering lists](https://react.dev/learn/rendering-lists)
- [React: preserving and resetting state](https://react.dev/learn/preserving-and-resetting-state)
