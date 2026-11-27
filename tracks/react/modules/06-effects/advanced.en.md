---
title: "Effects: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 06-effects
part: 2
tags: [react, effects, use-effect, hooks, cleanup]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Effects: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Order of calls

```tsx
function Order({ id }: { id: string }) {
  log.push('render ' + id);
  useLayoutEffect(() => { log.push('layout ' + id); return () => { log.push('layout cleanup ' + id); }; }, [id]);
  useEffect(() => { log.push('effect ' + id); return () => { log.push('cleanup ' + id); }; }, [id]);
  return null;
}
```

```text
mount a:      render a | layout a | effect a
change to b:  render b | layout cleanup a | layout b | cleanup a | effect b
same b again: render b
unmount:      layout cleanup b | cleanup b
```

Layout effects run before the browser repaints, ordinary effects after the commit. Children's effects run before the parent's:

```text
render parent | render child | effect child | effect parent
```

## Dependencies by reference

```tsx
const options = { v };                               // a new object on every render
useEffect(() => { counts.none++; });                 // no array
useEffect(() => { counts.empty++; }, []);
useEffect(() => { counts.obj++; }, [options]);
useEffect(() => { counts.prim++; }, [v]);
// three renders with the same v:  {"none":3,"empty":1,"obj":3,"prim":1}
```

An object or function created during render is new every time. Create it inside the effect, move it out of the component, or depend on its primitive fields.

## Stale values

```tsx
useEffect(() => {
  const id = setInterval(() => setN(n + 1), 5);      // n is frozen at 0
  return () => clearInterval(id);
}, []);
// the screen stays at 1

useEffect(() => {
  const id = setInterval(() => setN(x => x + 1), 5); // no stale value
  return () => clearInterval(id);
}, []);
// the number keeps growing
```

The first version captured `n` from the first render. Include the value in the dependencies, or avoid reading it with an updater function.

## Races

```tsx
useEffect(() => {
  let ignore = false;
  load(id).then(data => { if (!ignore) setData(data); });
  return () => { ignore = true; };
}, [id]);
// id changes from a slow request to a fast one:
// without the flag the screen ends up with the old, slow data; with it, with the new data
```

For `fetch`, an `AbortController` also cancels the request itself.

## You might not need an effect

```tsx
// avoid: an extra render with an outdated value
const [full, setFull] = useState('');
useEffect(() => { setFull(first + ' Smith'); }, [first]);
// renders:  "" then "Ann Smith"

// instead
const full = first + ' Smith';
```

| Need | Without an effect |
|---|---|
| a value computed from props or state | calculate it during render |
| resetting state for another item | a `key` |
| reacting to a user's action | the event handler |
| telling the parent about a change | call the parent's function in the same handler |

## Best practices

- One effect per thing being synchronised.
- Write the cleanup at the same time as the setup.
- Keep the lint rule for dependencies on.
- Extract a repeated effect into a custom hook.

## Common mistakes

- ❌ A chain of effects that set state for each other → ✅ compute in render or in the handler.
- ❌ Ignoring out-of-order responses → ✅ a flag or abort.
- ❌ Silencing the dependency warning → ✅ change the code so the value is not needed.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: useEffect](https://react.dev/reference/react/useEffect)
- [React: you might not need an effect](https://react.dev/learn/you-might-not-need-an-effect)
- [React: useLayoutEffect](https://react.dev/reference/react/useLayoutEffect)
