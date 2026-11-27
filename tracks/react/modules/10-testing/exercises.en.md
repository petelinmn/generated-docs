---
title: "Exercises: testing"
lang: en
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

# Exercises: testing

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Use the setup from [basics](basics.en.md).

## 1. 🟢 Greeting

**Task:** test that `Greeting` greets a guest without a name and the person when `name` arrives.

<details><summary>Hint</summary>

`rerender` from the result of `render`.

</details>

<details><summary>Solution</summary>

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

## 2. 🟡 A list of items

**Task:** items are added with Enter; blank input is ignored.

<details><summary>Hint</summary>

`{Enter}` inside `user.type`, and `getAllByRole('listitem')`.

</details>

<details><summary>Solution</summary>

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

## 3. 🔴 Debounced search

**Task:** prove that the search runs once, 300 ms after the last keystroke, without waiting in real time.

<details><summary>Hint</summary>

Fake timers must be known to user-event too.

</details>

<details><summary>Solution</summary>

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

`shouldAdvanceTime` lets the fake clock move with real time, which the tools that wait on timers need.

</details>
