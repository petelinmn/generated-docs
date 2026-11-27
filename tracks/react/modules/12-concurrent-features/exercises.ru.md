---
title: "Упражнения: конкурентные возможности"
lang: ru
track: react
kind: module
doc: exercises
slug: 12-concurrent-features
part: 3
tags: [react, suspense, transitions, concurrent-rendering, error-boundaries]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Упражнения: конкурентные возможности

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. `Boundary` — класс из [базовой теории](basics.ru.md).

## 1. 🟢 График по требованию

**Задача:** загружать код графика только после нажатия кнопки.

<details><summary>Подсказка</summary>

`lazy`, `Suspense` и булево состояние.

</details>

<details><summary>Решение</summary>

```tsx
const Chart = lazy(() => import('./Chart'));

function Dashboard() {
  const [show, setShow] = useState(false);
  return <>
    <button onClick={() => setShow(true)}>Show chart</button>
    {show && <Suspense fallback={<p>loading</p>}><Chart /></Suspense>}
  </>;
}
```

</details>

## 2. 🟡 Отзывчивый поиск

**Задача:** ввод остаётся плавным, пока фильтруется длинный список; приглушать список, пока он отстаёт.

<details><summary>Подсказка</summary>

`useDeferredValue` и `memo`.

</details>

<details><summary>Решение</summary>

```tsx
const SlowList = memo(function SlowList({ query }: { query: string }) {
  return <ul>{items.filter(i => i.includes(query)).map(i => <li key={i}>{i}</li>)}</ul>;
});

function Search() {
  const [query, setQuery] = useState('');
  const deferred = useDeferredValue(query);
  return <>
    <input aria-label="query" value={query} onChange={e => setQuery(e.target.value)} />
    <div style={{ opacity: query === deferred ? 1 : 0.5 }}><SlowList query={deferred} /></div>
  </>;
}
```

</details>

## 3. 🔴 Страницы с ошибками

**Задача:** переключать страницы в transition; неудачная показывает ошибку, повторное переключение восстанавливает работу.

<details><summary>Подсказка</summary>

Error boundary хранит ошибку, пока не будет пересоздана.

</details>

<details><summary>Решение</summary>

```tsx
function Pages({ load }: { load: (id: string) => Promise<string> }) {
  const [id, setId] = useState('home');
  const [isPending, start] = useTransition();
  return <>
    {['home', 'about'].map(page =>
      <button key={page} disabled={isPending} onClick={() => start(() => setId(page))}>{page}</button>)}
    <Boundary key={id}>
      <Suspense fallback={<p>loading</p>}><Content promise={load(id)} /></Suspense>
    </Boundary>
  </>;
}

function Content({ promise }: { promise: Promise<string> }) {
  return <article>{use(promise)}</article>;
}
// load must return the same promise for the same id (a cache)
```

`key={id}` даёт каждой странице свежую границу.

</details>
