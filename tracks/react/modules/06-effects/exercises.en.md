---
title: "Exercises: effects"
lang: en
track: react
kind: module
doc: exercises
slug: 06-effects
part: 2
tags: [react, effects, use-effect, hooks, cleanup]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Exercises: effects

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints.

## 1. 🟢 Window width

**Task:** a hook that returns the current window width and unsubscribes when the component goes away.

<details><summary>Hint</summary>

`addEventListener` in setup, `removeEventListener` in cleanup, the same function.

</details>

<details><summary>Solution</summary>

```tsx
function useWindowWidth() {
  const [width, setWidth] = useState(window.innerWidth);
  useEffect(() => {
    const onResize = () => setWidth(window.innerWidth);
    window.addEventListener('resize', onResize);
    return () => window.removeEventListener('resize', onResize);
  }, []);
  return width;
}
```

</details>

## 2. 🟡 A stopwatch

**Task:** count seconds while `running` is true; pausing keeps the number.

<details><summary>Hint</summary>

An effect may return early. How do you avoid reading `seconds`?

</details>

<details><summary>Solution</summary>

```tsx
function Stopwatch({ running }: { running: boolean }) {
  const [seconds, setSeconds] = useState(0);
  useEffect(() => {
    if (!running) return;
    const id = setInterval(() => setSeconds(s => s + 1), 1000);
    return () => clearInterval(id);
  }, [running]);
  return <p>{seconds}</p>;
}
```

</details>

## 3. 🔴 Loading a user

**Task:** show loading, the name or an error; switching `id` quickly must never show another user's data.

<details><summary>Hint</summary>

Abort in cleanup, and remember which `id` the state belongs to.

</details>

<details><summary>Solution</summary>

```tsx
function User({ id }: { id: string }) {
  const [state, setState] = useState<{ id: string; name?: string; error?: string }>({ id });

  useEffect(() => {
    const controller = new AbortController();
    fetch(`/users/${id}`, { signal: controller.signal })
      .then(r => { if (!r.ok) throw new Error(`HTTP ${r.status}`); return r.json(); })
      .then(user => setState({ id, name: user.name }))
      .catch(e => { if (!controller.signal.aborted) setState({ id, error: String(e.message) }); });
    return () => controller.abort();
  }, [id]);

  if (state.id !== id || (!state.name && !state.error)) return <p>loading</p>;
  return state.error ? <p role="alert">{state.error}</p> : <p>{state.name}</p>;
}
```

Storing the `id` with the data shows "loading" right away after a switch, without an extra effect that clears the state.

</details>
