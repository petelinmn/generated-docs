---
title: "Упражнения: refs и DOM"
lang: ru
track: react
kind: module
doc: exercises
slug: 07-refs-and-dom
part: 2
tags: [react, refs, dom, portals, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Упражнения: refs и DOM

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки.

## 1. 🟢 Автофокус

**Задача:** поле с подписью, получающее фокус при появлении.

<details><summary>Подсказка</summary>

Когда узел существует?

</details>

<details><summary>Решение</summary>

```tsx
function AutoFocusInput({ label }: { label: string }) {
  const input = useRef<HTMLInputElement>(null);
  useEffect(() => { input.current?.focus(); }, []);
  return <label>{label} <input ref={input} /></label>;
}
```

</details>

## 2. 🟡 Поиск с задержкой

**Задача:** вызывать `onSearch` через 300 мс после того, как пользователь перестал печатать; после исчезновения компонента не вызывать.

<details><summary>Подсказка</summary>

Id таймера должен переживать рендеры; очистите его при размонтировании.

</details>

<details><summary>Решение</summary>

```tsx
function Debounced({ onSearch }: { onSearch: (q: string) => void }) {
  const timer = useRef<ReturnType<typeof setTimeout>>(undefined);
  useEffect(() => () => clearTimeout(timer.current), []);
  return (
    <input
      aria-label="search"
      onChange={e => {
        const value = e.target.value;
        clearTimeout(timer.current);
        timer.current = setTimeout(() => onSearch(value), 300);
      }}
    />
  );
}
```

</details>

## 3. 🔴 Прокрутка к новому сообщению

**Задача:** после добавления сообщения прокрутить список к нему.

<details><summary>Подсказка</summary>

Сразу после `setMessages` нового `<li>` в DOM ещё нет.

</details>

<details><summary>Решение</summary>

```tsx
function Chat() {
  const [messages, setMessages] = useState(['hi']);
  const list = useRef<HTMLUListElement>(null);

  function add(text: string) {
    flushSync(() => setMessages(m => [...m, text]));
    list.current?.lastElementChild?.scrollIntoView?.({ block: 'nearest' });
  }

  return (
    <>
      <ul ref={list}>{messages.map((m, i) => <li key={i}>{m}</li>)}</ul>
      <button onClick={() => add('new')}>Add</button>
    </>
  );
}
```

Необязательный вызов `scrollIntoView?.()` сохраняет работу там, где метода нет, как в jsdom.

</details>
