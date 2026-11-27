---
title: "Упражнения: context и редьюсеры"
lang: ru
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

# Упражнения: context и редьюсеры

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки.

## 1. 🟢 Редьюсер списка дел

**Задача:** actions `added`, `toggled`, `removed`; без мутаций.

<details><summary>Подсказка</summary>

Spread, `map`, `filter` и объединённый тип actions.

</details>

<details><summary>Решение</summary>

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

При `strict` в TypeScript пропущенная ветка — ошибка компиляции, потому что функция тогда не вернула бы значение.

</details>

## 2. 🟡 Провайдер темы

**Задача:** предоставить тему и переключатель; чтение вне провайдера должно громко падать.

<details><summary>Подсказка</summary>

`null` по умолчанию и устойчивое значение.

</details>

<details><summary>Решение</summary>

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

## 3. 🔴 Кнопка без повторных рендеров

**Задача:** счётчик в context; кнопка «+» не должна рендериться при его изменении.

<details><summary>Подсказка</summary>

Два context. Какое значение никогда не меняется?

</details>

<details><summary>Решение</summary>

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

Кнопку передают через `children`, и она читает только context с dispatch, значение которого не меняется.

</details>
