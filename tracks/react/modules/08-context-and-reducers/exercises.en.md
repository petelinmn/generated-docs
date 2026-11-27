---
title: "Exercises: context and reducers"
lang: en
track: react
kind: module
doc: exercises
slug: 08-context-and-reducers
part: 2
tags: [react, context, use-reducer, state-management, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Exercises: context and reducers

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints.

## 1. 🟢 A to-do reducer

**Task:** actions `added`, `toggled`, `removed`; no mutation.

<details><summary>Hint</summary>

Spread, `map`, `filter`, and a union type of actions.

</details>

<details><summary>Solution</summary>

```tsx
type Todo = { id: number; text: string; done: boolean };
type TodoAction =
  | { type: 'added'; id: number; text: string }
  | { type: 'toggled'; id: number }
  | { type: 'removed'; id: number };

function todos(state: Todo[], action: TodoAction): Todo[] {
  switch (action.type) {
    case 'added': return [...state, { id: action.id, text: action.text, done: false }];
    case 'toggled': return state.map(t => (t.id === action.id ? { ...t, done: !t.done } : t));
    case 'removed': return state.filter(t => t.id !== action.id);
  }
}
```

With `strict` TypeScript a missing branch is a compile error, because the function would then not return a value.

</details>

## 2. 🟡 A theme provider

**Task:** provide the theme and a toggle; reading outside the provider must fail loudly.

<details><summary>Hint</summary>

`null` as the default, and a stable value.

</details>

<details><summary>Solution</summary>

```tsx
type ThemeValue = { theme: 'light' | 'dark'; toggle: () => void };
const ThemeContext = createContext<ThemeValue | null>(null);

function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setTheme] = useState<'light' | 'dark'>('light');
  const toggle = useCallback(() => setTheme(t => (t === 'light' ? 'dark' : 'light')), []);
  const value = useMemo(() => ({ theme, toggle }), [theme, toggle]);
  return <ThemeContext value={value}>{children}</ThemeContext>;
}

function useTheme() {
  const value = useContext(ThemeContext);
  if (!value) throw new Error('useTheme must be used inside ThemeProvider');
  return value;
}
```

</details>

## 3. 🔴 A button that does not re-render

**Task:** a counter in context; the "+" button must not re-render when the count changes.

<details><summary>Hint</summary>

Two contexts. Which value never changes?

</details>

<details><summary>Solution</summary>

```tsx
const CountState = createContext(0);
const CountDispatch = createContext<Dispatch<number>>(() => {});

function CountProvider({ children }: { children: ReactNode }) {
  const [count, add] = useReducer((s: number, by: number) => s + by, 0);
  return (
    <CountDispatch value={add}>
      <CountState value={count}>{children}</CountState>
    </CountDispatch>
  );
}

function AddButton() {
  const add = useContext(CountDispatch);           // does not re-render when count changes
  return <button onClick={() => add(1)}>+</button>;
}
```

The button is passed in as `children`, and it reads only the dispatch context, whose value stays the same.

</details>
