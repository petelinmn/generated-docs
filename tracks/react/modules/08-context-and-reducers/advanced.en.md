---
title: "Context and reducers: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 08-context-and-reducers
part: 2
tags: [react, context, use-reducer, state-management, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Context and reducers: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Lazy initial state

```tsx
useReducer(reducer, 5, n => ({ count: n * 2 }));   // the third argument runs once
```

## Reducer plus context

Put the state and `dispatch` into two contexts:

```tsx
const TasksContext = createContext<Task[] | null>(null);
const DispatchContext = createContext<Dispatch<Action> | null>(null);

function TasksProvider({ children }: { children: ReactNode }) {
  const [tasks, dispatch] = useReducer(tasksReducer, []);
  return (
    <TasksContext value={tasks}>
      <DispatchContext value={dispatch}>{children}</DispatchContext>
    </TasksContext>
  );
}
```

Components that only dispatch read the second one. Since `dispatch` never changes, they are not re-rendered because of state changes.

## A safe custom hook

```tsx
const Auth = createContext<{ user: string } | null>(null);

function useAuth() {
  const value = useContext(Auth);
  if (!value) throw new Error('useAuth must be used inside AuthProvider');
  return value;
}
```

## Re-rendering

```tsx
<User value={{ name: 'Ann' }}>            {/* a new object on every render */}
const value = useMemo(() => ({ name: 'Ann' }), []);
<User value={value}>                      {/* the same object */}
```

```text
two re-renders of the provider's owner:
new object every time:  consumer 2, memo consumer 2, memo component without context 0
memoized value:         consumer 0, memo consumer 0, memo component without context 0
```

React compares context values with `Object.is`. Every consumer of a changed context renders again, and `memo` does not stop it. A memoized value keeps consumers still when nothing really changed.

Components passed as `children` to a provider component are not re-rendered by its state unless they read the context.

## Best practices

- Name actions after what happened (`added`), not what to set.
- One reducer per area of state.
- Split contexts by how often the values change.
- Export the provider and the hooks, not the context object.

## Common mistakes

- ❌ An object literal as the value → ✅ `useMemo`.
- ❌ Fast-changing values (mouse position) in context → ✅ keep them local or use an external store.
- ❌ A reducer with a default that returns the state silently → ✅ let TypeScript check exhaustiveness, or throw.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [React: useReducer](https://react.dev/reference/react/useReducer)
- [React: createContext](https://react.dev/reference/react/createContext)
- [React: useContext](https://react.dev/reference/react/useContext)
- [React: passing data deeply with context](https://react.dev/learn/passing-data-deeply-with-context)
