---
title: "Exercises: server rendering"
lang: en
track: react
kind: module
doc: exercises
slug: 14-server-rendering
part: 3
tags: [react, server-side-rendering, hydration, server-components, streaming]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Exercises: server rendering

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints.

## 1. 🟢 Server page with data

**Task:** render a todo list on the server, put the data into the page safely and hydrate with it.

<details><summary>Hint</summary>

Escape `<` in the JSON.

</details>

<details><summary>Solution</summary>

```tsx
function renderPage(todos: string[]) {
  const data = JSON.stringify(todos).replace(/</g, '\\u003c');
  return `<div id="root">${renderToString(<Todos initial={todos} />)}</div>` +
         `<script id="data" type="application/json">${data}</script>`;
}

function startClient() {
  const todos: string[] = JSON.parse(document.getElementById('data')!.textContent!);
  hydrateRoot(document.getElementById('root')!, <Todos initial={todos} />);
}

function Todos({ initial }: { initial: string[] }) {
  const [todos, setTodos] = useState(initial);
  return <>
    <button onClick={() => setTodos([...todos, 'new'])}>add</button>
    <ul>{todos.map((t, i) => <li key={i}>{t}</li>)}</ul>
  </>;
}
```

</details>

## 2. 🟡 Local time without a mismatch

**Task:** show the ISO string on the server and the local format in the browser.

<details><summary>Hint</summary>

The server snapshot of `useSyncExternalStore`.

</details>

<details><summary>Solution</summary>

```tsx
const subscribe = () => () => {};

function LocalTime({ iso }: { iso: string }) {
  const isClient = useSyncExternalStore(subscribe, () => true, () => false);
  return <time dateTime={iso}>{isClient ? new Date(iso).toLocaleString() : iso}</time>;
}
```

</details>

## 3. 🔴 Streaming with status codes

**Task:** send 200 for a good shell, 500 and a fallback page if the shell fails.

<details><summary>Hint</summary>

`onShellReady`, `onShellError`, `onError`.

</details>

<details><summary>Solution</summary>

```tsx
function respond(node: ReactNode, res: PassThrough & { statusCode?: number }) {
  let failed = false;
  const { pipe } = renderToPipeableStream(node, {
    onShellReady() {
      res.statusCode = failed ? 500 : 200;
      pipe(res);
    },
    onShellError() {
      res.statusCode = 500;
      res.end('<h1>Something went wrong</h1>');
    },
    onError(error) {
      failed = true;
      console.log('logged:', (error as Error).message);
    },
  });
}
```

</details>
