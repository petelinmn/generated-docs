---
title: "Упражнения: списки, ключи и идентичность"
lang: ru
track: react
kind: module
doc: exercises
slug: 04-lists-and-keys
part: 1
tags: [react, keys, lists, identity, state]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Упражнения: списки, ключи и идентичность

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки.

## 1. 🟢 Меню с группами

**Задача:** для каждой группы вывести заголовок и список её пунктов без лишних элементов-обёрток.

<details><summary>Подсказка</summary>

Два уровня `map` и фрагмент, принимающий ключ.

</details>

<details><summary>Решение</summary>

```tsx
type Group = { id: string; title: string; items: { id: string; label: string }[] };

function Menu({ groups }: { groups: Group[] }) {
  return <>
    {groups.map(g => (
      <Fragment key={g.id}>
        <h3>{g.title}</h3>
        <ul>{g.items.map(i => <li key={i.id}>{i.label}</li>)}</ul>
      </Fragment>
    ))}
  </>;
}
```

</details>

## 2. 🟡 Устойчивые id для новых строк

**Задача:** строки добавляются на клиенте; каждой дать id, который не меняется.

<details><summary>Подсказка</summary>

Когда именно рождается строка?

</details>

<details><summary>Решение</summary>

```tsx
type Row = { id: string; text: string };

function useRows() {
  const [rows, setRows] = useState<Row[]>([]);
  const add = (text: string) => setRows(rs => [...rs, { id: crypto.randomUUID(), text }]);
  return { rows, add };
}
```

`crypto.randomUUID()` в браузерах доступен только в защищённом контексте (HTTPS или localhost).

</details>

## 3. 🔴 Вкладки, помнящие черновики

**Задача:** переключение вкладок не должно терять набранный в каждой текст.

<details><summary>Подсказка</summary>

Удаление компонента теряет состояние. Чем ещё можно его скрыть?

</details>

<details><summary>Решение</summary>

```tsx
function Tabs({ tabs }: { tabs: { id: string; title: string }[] }) {
  const [active, setActive] = useState(tabs[0].id);
  return <>
    {tabs.map(t => <button key={t.id} onClick={() => setActive(t.id)}>{t.title}</button>)}
    {tabs.map(t => (
      <section key={t.id} hidden={t.id !== active}>
        <textarea placeholder={t.title} />
      </section>
    ))}
  </>;
}
```

Все панели остаются смонтированными; `hidden` только убирает их с экрана.

</details>
