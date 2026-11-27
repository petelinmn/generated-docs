---
title: "Упражнения: серверный рендеринг"
lang: ru
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

# Упражнения: серверный рендеринг

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки.

## 1. 🟢 Серверная страница с данными

**Задача:** отрисовать список дел на сервере, безопасно вложить данные в страницу и гидратировать с ними.

<details><summary>Подсказка</summary>

Экранируйте `<` в JSON.

</details>

<details><summary>Решение</summary>

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

## 2. 🟡 Местное время без расхождения

**Задача:** на сервере показывать ISO-строку, в браузере — местный формат.

<details><summary>Подсказка</summary>

Серверный снимок `useSyncExternalStore`.

</details>

<details><summary>Решение</summary>

```tsx
const subscribe = () => () => {};

function LocalTime({ iso }: { iso: string }) {
  const isClient = useSyncExternalStore(subscribe, () => true, () => false);
  return <time dateTime={iso}>{isClient ? new Date(iso).toLocaleString() : iso}</time>;
}
```

</details>

## 3. 🔴 Streaming с кодами ответа

**Задача:** отдавать 200 при удачном каркасе, 500 и запасную страницу при его падении.

<details><summary>Подсказка</summary>

`onShellReady`, `onShellError`, `onError`.

</details>

<details><summary>Решение</summary>

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
