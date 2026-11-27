---
title: "Testing: advanced"
lang: en
track: react
kind: module
doc: advanced
slug: 10-testing
part: 2
tags: [react, testing, vitest, testing-library, unit-testing]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Testing: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Loading and errors

```tsx
function User({ load }: { load: () => Promise<string> }) {
  const [state, setState] = useState<{ name?: string; error?: string }>({});
  useEffect(() => {
    let ignore = false;
    load().then(
      name => { if (!ignore) setState({ name }); },
      (e: Error) => { if (!ignore) setState({ error: e.message }); },
    );
    return () => { ignore = true; };
  }, [load]);
  if (state.error) return <p role="alert">{state.error}</p>;
  return <p>{state.name ?? 'Loading'}</p>;
}
```

```tsx
test('shows the loaded name', async () => {
  const load = vi.fn().mockResolvedValue('Ann');
  render(<User load={load} />);
  expect(screen.getByText('Loading')).toBeInTheDocument();

  expect(await screen.findByText('Ann')).toBeInTheDocument();
  expect(screen.queryByText('Loading')).not.toBeInTheDocument();
});

test('shows an error', async () => {
  const load = vi.fn().mockRejectedValue(new Error('offline'));
  render(<User load={load} />);

  expect(await screen.findByRole('alert')).toHaveTextContent('offline');
});
```

The component receives `load` as a prop, so a test replaces the network with a mock. `findBy` retries until the element appears or 1000 ms pass.

## Context

```tsx
test('reads context from a wrapper', () => {
  render(<Badge />, { wrapper: ({ children }) => <Theme value="dark">{children}</Theme> });
  expect(screen.getByText('dark')).toBeInTheDocument();
});
```

## Hooks

```tsx
test('tests a hook', () => {
  const { result } = renderHook(() => useToggle());
  expect(result.current[0]).toBe(false);

  act(() => result.current[1]());

  expect(result.current[0]).toBe(true);
});
```

A call that updates state outside user-event must be wrapped in `act`.

## Time

```tsx
function Toast({ ms }: { ms: number }) {
  const [visible, setVisible] = useState(true);
  useEffect(() => {
    const id = setTimeout(() => setVisible(false), ms);
    return () => clearTimeout(id);
  }, [ms]);
  return visible ? <p role="status">Saved</p> : null;
}
```

```tsx
describe('timers', () => {
  afterEach(() => { vi.useRealTimers(); });

  test('hides the toast after the delay', () => {
    vi.useFakeTimers();
    render(<Toast ms={3000} />);

    act(() => { vi.advanceTimersByTime(2999); });
    expect(screen.getByRole('status')).toBeInTheDocument();

    act(() => { vi.advanceTimersByTime(1); });
    expect(screen.queryByRole('status')).not.toBeInTheDocument();
  });
});
```

Fake timers make a three-second delay instant and exact. Restore real timers afterwards.

## Best practices

- One behaviour per test, named after it.
- Pass dependencies in (functions, clients) so they are easy to replace.
- Prefer `findBy` to fixed sleeps.
- Use `queryBy` only to assert absence.

## Common mistakes

- ❌ `setTimeout` in a test to wait → ✅ `findBy` or fake timers.
- ❌ Mocking the component under test → ✅ mock only its surroundings.
- ❌ One huge test of everything → ✅ several small ones.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [Testing Library: about queries](https://testing-library.com/docs/queries/about)
- [Testing Library: user-event](https://testing-library.com/docs/user-event/intro)
- [React Testing Library: API](https://testing-library.com/docs/react-testing-library/api)
- [Vitest: environment](https://vitest.dev/config/environment)
- [Vitest: vi](https://vitest.dev/api/vi)
- [jest-dom: with Vitest](https://github.com/testing-library/jest-dom#with-vitest)
- [React: act](https://react.dev/reference/react/act)
