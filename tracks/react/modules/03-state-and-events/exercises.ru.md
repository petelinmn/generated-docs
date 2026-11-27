---
title: "Упражнения: состояние и события"
lang: ru
track: react
kind: module
doc: exercises
slug: 03-state-and-events
part: 1
tags: [react, state, use-state, events, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Упражнения: состояние и события

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки.

## 1. 🟢 Счётчик с шагом

**Задача:** кнопки `-`, `+` и `+2`; значение не опускается ниже нуля.

<details><summary>Подсказка</summary>

Почему `change(1); change(1)` с `setN(n + 1)` прибавил бы только один?

</details>

<details><summary>Решение</summary>

```tsx
function Stepper() {
  const [n, setN] = useState(0);
  const change = (by: number) => setN(x => Math.max(0, x + by));
  return (
    <>
      <button onClick={() => change(-1)}>-</button>
      <output>{n}</output>
      <button onClick={() => change(+1)}>+</button>
      <button onClick={() => { change(+1); change(+1); }}>+2</button>
    </>
  );
}
```

Благодаря функции-обновлению два вызова подряд складываются.

</details>

## 2. 🟡 Список дел без мутаций

**Задача:** написать `add`, `toggle` и `remove`, возвращающие новые массивы.

<details><summary>Подсказка</summary>

Spread, `map`, `filter`.

</details>

<details><summary>Решение</summary>

```tsx
type Todo = { id: number; text: string; done: boolean };

const add = (list: Todo[], id: number, text: string): Todo[] => [...list, { id, text, done: false }];
const toggle = (list: Todo[], id: number) => list.map(t => (t.id === id ? { ...t, done: !t.done } : t));
const remove = (list: Todo[], id: number) => list.filter(t => t.id !== id);

// in the component:  setTodos(list => toggle(list, id));
```

Новым объектом становится только изменённый элемент, остальные переиспользуются.

</details>

## 3. 🔴 Свой редактор на каждого пользователя

**Задача:** поле инициализируется именем пользователя; при переключении должно показываться новое имя.

<details><summary>Подсказка</summary>

`useState` берёт начальное значение один раз. Что даёт компоненту новую идентичность?

</details>

<details><summary>Решение</summary>

```tsx
function Editor({ userId, initialName }: { userId: string; initialName: string }) {
  const [name, setName] = useState(initialName);
  return <input value={name} onChange={e => setName(e.target.value)} />;
}

// in the parent: a different user gets a fresh state
<Editor key={user.id} userId={user.id} initialName={user.name} />
```

Эффект, копирующий prop в состояние, сначала отрендерил бы старое значение; `key` этого избегает.

</details>
