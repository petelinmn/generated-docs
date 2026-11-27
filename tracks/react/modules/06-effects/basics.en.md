---
title: "Effects: basics"
lang: en
track: react
kind: module
doc: basics
slug: 06-effects
part: 2
tags: [react, effects, use-effect, hooks, cleanup]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Effects: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [state and events](../03-state-and-events/overview.en.md) · **Time:** ~2 h

All results were produced with React 19.3.1.

## Three parts of an effect

```tsx
useEffect(() => {
  const connection = createConnection(room);
  connection.connect();
  return () => connection.disconnect();      // cleanup
}, [room]);                                   // run again when room changes
```

| Part | Meaning |
|---|---|
| setup | what to start |
| cleanup | how to stop it |
| dependencies | when to start over |

The effect runs after the commit, on the client only: it does not run during server rendering.

## Dependencies

| Written | Runs |
|---|---|
| no array | after every commit |
| `[]` | after the first one |
| `[a, b]` | when `a` or `b` differs (`Object.is`) |

Every value from the component that the effect reads belongs in the array.

## Cleanup

Before the effect runs again React calls the previous cleanup with the old values; it also calls it when the component is removed.

## Development double run

```text
<StrictMode> in development, an effect with [] that connects and disconnects:
connect | disconnect | connect
```

It checks that cleanup really undoes the setup. Do not suppress it: fix the cleanup.

## Beginner mistakes

- ❌ A subscription without cleanup → ✅ return a function that unsubscribes.
- ❌ Leaving a used value out of the array → ✅ include it, or remove the need for it.
- ❌ `useEffect(async () => ...)` → ✅ declare an async function inside and call it.
- ❌ Fighting the double run with a flag → ✅ make setup and cleanup symmetrical.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [React: useEffect](https://react.dev/reference/react/useEffect)
- [React: you might not need an effect](https://react.dev/learn/you-might-not-need-an-effect)
- [React: useLayoutEffect](https://react.dev/reference/react/useLayoutEffect)
