---
title: "Тестирование: продвинутая теория"
lang: ru
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

# Тестирование: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Загрузка и ошибки

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

Компонент получает `load` через props, поэтому тест подменяет сеть заглушкой. `findBy` повторяет попытки, пока элемент не появится или не пройдёт 1000 мс.

## Context

```tsx
test('reads context from a wrapper', () => {
  render(<Badge />, { wrapper: ({ children }) => <Theme value="dark">{children}</Theme> });
  expect(screen.getByText('dark')).toBeInTheDocument();
});
```

## Хуки

```tsx
test('tests a hook', () => {
  const { result } = renderHook(() => useToggle());
  expect(result.current[0]).toBe(false);

  act(() => result.current[1]());

  expect(result.current[0]).toBe(true);
});
```

Вызов, обновляющий состояние вне user-event, нужно оборачивать в `act`.

## Время

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

Поддельные таймеры делают задержку в три секунды мгновенной и точной. После теста верните настоящие.

## Лучшие практики

- Один тест — одно поведение, названное по нему.
- Передавайте зависимости внутрь (функции, клиенты), чтобы их было легко подменить.
- Предпочитайте `findBy` фиксированным паузам.
- `queryBy` используйте только для проверки отсутствия.

## Частые ошибки

- ❌ `setTimeout` в тесте ради ожидания → ✅ `findBy` или поддельные таймеры.
- ❌ Подменять проверяемый компонент → ✅ подменять только окружение.
- ❌ Один огромный тест на всё → ✅ несколько маленьких.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [Testing Library: о запросах](https://testing-library.com/docs/queries/about)
- [Testing Library: user-event](https://testing-library.com/docs/user-event/intro)
- [React Testing Library: API](https://testing-library.com/docs/react-testing-library/api)
- [Vitest: environment](https://vitest.dev/config/environment)
- [Vitest: vi](https://vitest.dev/api/vi)
- [jest-dom: with Vitest](https://github.com/testing-library/jest-dom#with-vitest)
- [React: act](https://react.dev/reference/react/act)
