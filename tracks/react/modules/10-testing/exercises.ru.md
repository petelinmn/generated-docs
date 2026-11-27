---
title: "Упражнения: тестирование"
lang: ru
track: react
kind: module
doc: exercises
slug: 10-testing
part: 2
tags: [react, testing, vitest, testing-library, unit-testing]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Упражнения: тестирование

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Настройка — из [базовой теории](basics.ru.md).

## 1. 🟢 Приветствие

**Задача:** проверить, что `Greeting` без имени приветствует гостя, а при появлении `name` — человека.

<details><summary>Подсказка</summary>

`rerender` из результата `render`.

</details>

<details><summary>Решение</summary>

```tsx
function Greeting({ name }: { name?: string }) {
  return <h1>Hello, {name ?? 'guest'}!</h1>;
}

test('greets by name or as a guest', () => {
  const { rerender } = render(<Greeting />);
  expect(screen.getByRole('heading')).toHaveTextContent('Hello, guest!');

  rerender(<Greeting name="Ann" />);
  expect(screen.getByRole('heading')).toHaveTextContent('Hello, Ann!');
});
```

</details>

## 2. 🟡 Список пунктов

**Задача:** пункты добавляются по Enter; пустой ввод игнорируется.

<details><summary>Подсказка</summary>

`{Enter}` внутри `user.type` и `getAllByRole('listitem')`.

</details>

<details><summary>Решение</summary>

```tsx
function Todo() {
  const [items, setItems] = useState<string[]>([]);
  const [text, setText] = useState('');
  return (
    <>
      <form onSubmit={e => { e.preventDefault(); if (text.trim()) { setItems([...items, text.trim()]); setText(''); } }}>
        <input aria-label="new item" value={text} onChange={e => setText(e.target.value)} />
      </form>
      <ul>{items.map((item, i) => <li key={i}>{item}</li>)}</ul>
    </>
  );
}

test('adds items on Enter and ignores blank ones', async () => {
  const user = userEvent.setup();
  render(<Todo />);
  const input = screen.getByRole('textbox', { name: 'new item' });

  await user.type(input, 'milk{Enter}');
  await user.type(input, '   {Enter}');

  expect(screen.getAllByRole('listitem').map(li => li.textContent)).toEqual(['milk']);
});
```

</details>

## 3. 🔴 Поиск с задержкой

**Задача:** доказать, что поиск выполняется один раз, через 300 мс после последнего нажатия, не ожидая в реальном времени.

<details><summary>Подсказка</summary>

О поддельных таймерах должен знать и user-event.

</details>

<details><summary>Решение</summary>

```tsx
function Search({ onSearch }: { onSearch: (q: string) => void }) {
  const [q, setQ] = useState('');
  useEffect(() => {
    if (!q) return;
    const id = setTimeout(() => onSearch(q), 300);
    return () => clearTimeout(id);
  }, [q, onSearch]);
  return <input aria-label="search" value={q} onChange={e => setQ(e.target.value)} />;
}

afterEach(() => { vi.useRealTimers(); });

test('searches once after the user stops typing', async () => {
  vi.useFakeTimers({ shouldAdvanceTime: true });
  const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });
  const onSearch = vi.fn();
  render(<Search onSearch={onSearch} />);

  await user.type(screen.getByRole('textbox'), 'react');
  expect(onSearch).not.toHaveBeenCalled();

  act(() => { vi.advanceTimersByTime(300); });
  expect(onSearch).toHaveBeenCalledExactlyOnceWith('react');
});
```

`shouldAdvanceTime` двигает поддельные часы вместе с настоящими, что нужно инструментам, ожидающим таймеры.

</details>
