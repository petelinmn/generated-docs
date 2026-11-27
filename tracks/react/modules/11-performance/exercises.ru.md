---
title: "Упражнения: производительность рендера"
lang: ru
track: react
kind: module
doc: exercises
slug: 11-performance
part: 3
tags: [react, performance, memoization, react-compiler, profiling]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Упражнения: производительность рендера

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. `Item` — это `{ id: number; name: string }`.

## 1. 🟢 Графику всё равно

**Задача:** ввод в поле поиска не должен перерендеривать `HeavyChart`.

<details><summary>Подсказка</summary>

Где должно жить состояние запроса?

</details>

<details><summary>Решение</summary>

```tsx
function SearchBox() {
  const [query, setQuery] = useState('');
  return <input aria-label="search" value={query} onChange={e => setQuery(e.target.value)} />;
}

function Page() {
  return <><SearchBox /><HeavyChart /></>;
}
```

Мемоизация не нужна.

</details>

## 2. 🟡 Строки, остающиеся на месте

**Задача:** удаление одной строки не должно перерендеривать остальные.

<details><summary>Подсказка</summary>

`memo` и устойчивый callback.

</details>

<details><summary>Решение</summary>

```tsx
const Row = memo(function Row({ item, onRemove }: { item: Item; onRemove: (id: number) => void }) {
  return <li>{item.name} <button onClick={() => onRemove(item.id)}>x</button></li>;
});

function List() {
  const [items, setItems] = useState<Item[]>(initial);
  const onRemove = useCallback((id: number) => setItems(list => list.filter(i => i.id !== id)), []);
  return <ul>{items.map(item => <Row key={item.id} item={item} onRemove={onRemove} />)}</ul>;
}
```

Функция-обновление позволяет `onRemove` обходиться без зависимостей.

</details>

## 3. 🔴 Фильтрация только когда нужно

**Задача:** пересчитывать отфильтрованные строки лишь при изменении строк или запроса.

<details><summary>Подсказка</summary>

`useMemo` и его зависимости.

</details>

<details><summary>Решение</summary>

```tsx
function Table({ rows, query }: { rows: Item[]; query: string }) {
  const visible = useMemo(
    () => rows.filter(r => r.name.toLowerCase().includes(query.toLowerCase())),
    [rows, query],
  );
  return <ul>{visible.map(r => <li key={r.id}>{r.name}</li>)}</ul>;
}
```

Это помогает, только если родитель не создаёт новый массив `rows` при каждом рендере.

</details>
