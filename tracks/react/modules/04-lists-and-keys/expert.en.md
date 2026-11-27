---
title: "Lists, keys and identity: expert"
lang: en
track: react
kind: module
doc: expert
slug: 04-lists-and-keys
part: 1
tags: [react, keys, lists, identity, state]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Lists, keys and identity: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

On a new render React compares the new elements with the old ones among the children of each parent. An element is "the same" when its type and key match; without a key its position is used. For the same element React keeps the component instance and its state and updates props; otherwise it unmounts the old one and mounts a new one.

### Positions and empty slots

An element that is `false` or `null` still takes its place:

```tsx
function Slot() {
  const [warn, setWarn] = useState(false);
  return <>
    <button onClick={() => setWarn(!warn)}>warn</button>
    {warn && <p>careful</p>}          {/* false still occupies this position */}
    <ul><Item name="a" /></ul>
  </>;
}
// click the item, then show the warning:  a:1  (state kept)
```

Returning a differently shaped tree is another matter:

```tsx
if (warn) return <><button /><p>careful</p><ul><Item name="a" /></ul></>;
return <><button /><ul><Item name="a" /></ul></>;
// click the item, then switch warn:  a:0  (the list moved to another position)
```

### Parents matter

A key works among the children of one parent:

```tsx
<ul>{!right && <Item key="a" name="a" />}</ul>
<ul>{right && <Item key="a" name="a" />}</ul>
// click, then move:  a:0 ; the old instance unmounts and a new one mounts
```

## Reference

| Situation | State |
|---|---|
| Same type, same key or position | kept, props updated |
| Different type at the position | reset, including the whole subtree |
| Different `key` | reset |
| Moved among siblings with a stable key | kept |
| Moved to another parent | reset |
| Hidden with CSS or `hidden` | kept |
| Removed by a condition | lost |

| Key source | Use |
|---|---|
| Database id | the default choice |
| `crypto.randomUUID()` at creation | items born on the client |
| A natural unique value | if it really never changes |
| Index | only for a static list |
| `Math.random()` during render | never |

## Gotchas

### Keys that change on every render

```tsx
{items.map(x => <Item key={Math.random()} name={x} />)}
// every render of the list unmounts all items and mounts new ones: state and focus are lost
```

### A shape change loses state

```tsx
if (warn) return <><button /><p>careful</p><ul><Item name="a" /></ul></>;
return <><button /><ul><Item name="a" /></ul></>;
// click the item, then switch warn:  a:0  (the list moved to another position)
```

Keep the structure stable and switch only the parts that differ: `{warn && <p>careful</p>}`.

### The same key in another place

```tsx
<ul>{!right && <Item key="a" name="a" />}</ul>
<ul>{right && <Item key="a" name="a" />}</ul>
// click, then move:  a:0 ; the old instance unmounts and a new one mounts
```

## Trade-offs and alternatives

| Choice | Pro | Con |
|---|---|---|
| Keep hidden items mounted | state and scroll survive | memory and rendering cost |
| Unmount and lift state up | cheap DOM | more code in the parent |
| Reset by `key` | one line, no effects | the whole subtree is recreated |
| Virtualised list | handles thousands of rows | rows outside the view are not mounted, so keep their state outside |

## Interview questions

**1. Why are keys needed?**

<details><summary>Answer</summary>

To match elements between renders, so that state and DOM nodes stay with the right item.

</details>

**2. Must keys be globally unique?**

<details><summary>Answer</summary>

No, only among siblings.

</details>

**3. When is an index acceptable as a key?**

<details><summary>Answer</summary>

When the list never reorders, inserts in the middle or filters, and items have no state of their own. A data id is still safer.

</details>

**4. What happens with a random key?**

<details><summary>Answer</summary>

Every render unmounts the old items and mounts new ones.

</details>

**5. Can a component read its own key?**

<details><summary>Answer</summary>

No.

</details>

**6. How do you give a key to several elements?**

<details><summary>Answer</summary>

`<Fragment key={...}>`; the short syntax cannot take a key.

</details>

**7. Why does `{warn && <p/>}` not reset the component after it?**

<details><summary>Answer</summary>

`false` still occupies a slot, so the positions of the following elements do not shift.

</details>

**8. Does moving an element to another parent keep its state?**

<details><summary>Answer</summary>

No, even with the same key.

</details>

**9. Trick question: two different lists both use `key="1"`. A problem?**

<details><summary>Answer</summary>

No.

</details>

**10. Trick question: the item disappears from view but its state survives. How?**

<details><summary>Answer</summary>

It is hidden by CSS or `hidden`, not removed from the tree.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [React: rendering lists](https://react.dev/learn/rendering-lists)
- [React: preserving and resetting state](https://react.dev/learn/preserving-and-resetting-state)
