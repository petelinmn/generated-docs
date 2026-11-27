---
title: "Тестирование: базовая теория"
lang: ru
track: react
kind: module
doc: basics
slug: 10-testing
part: 2
tags: [react, testing, vitest, testing-library, unit-testing]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Тестирование: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [формы](../05-forms/overview.ru.md) · **Время:** ~2 ч

Примеры выполнены с React 19.3.1, Vitest 5.1.2, jsdom 31.0.1, React Testing Library 16.3.3, user-event 14.6.13 и jest-dom 8.0.0.

## Настройка

```bash
npm install -D vitest jsdom @testing-library/react @testing-library/dom \
  @testing-library/user-event @testing-library/jest-dom
```

`vitest.config.ts`:

```ts
import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    environment: 'jsdom',
    setupFiles: ['./vitest.setup.ts'],
  },
});
```

`vitest.setup.ts`:

```ts
import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

afterEach(() => { cleanup(); });
```

Файл настройки добавляет проверки вроде `toBeInTheDocument` и очищает документ после каждого теста. Автоматической очистке нужен глобальный `afterEach`, а Vitest не добавляет его без включённой опции `globals`. jest-dom 8 требует Vitest 5 или новее. Запуск: `npx vitest run`.

## Первый тест

```tsx
function Counter() {
  const [n, setN] = useState(0);
  return (
    <>
      <p>Count: {n}</p>
      <button onClick={() => setN(n + 1)}>Add</button>
    </>
  );
}

test('increments on click', async () => {
  const user = userEvent.setup();
  render(<Counter />);

  await user.click(screen.getByRole('button', { name: 'Add' }));

  expect(screen.getByText('Count: 1')).toBeInTheDocument();
});
```

Три шага: отрендерить, подействовать, проверить.

## Поиск элементов

| Запрос | Применение |
|---|---|
| `getByRole` | первый выбор: кнопки, заголовки, поля по имени |
| `getByLabelText` | поля форм |
| `getByText` | неинтерактивное содержимое |
| `getByTestId` | крайняя мера |

## Форма

```tsx
function Login({ onSubmit }: { onSubmit: (email: string) => void }) {
  const [email, setEmail] = useState('');
  return (
    <form onSubmit={e => { e.preventDefault(); onSubmit(email); }}>
      <label>
        Email <input type="email" value={email} onChange={e => setEmail(e.target.value)} />
      </label>
      <button disabled={!email}>Sign in</button>
    </form>
  );
}
```

```tsx
test('submits the typed email', async () => {
  const user = userEvent.setup();
  const onSubmit = vi.fn();
  render(<Login onSubmit={onSubmit} />);
  const button = screen.getByRole('button', { name: 'Sign in' });
  expect(button).toBeDisabled();

  await user.type(screen.getByRole('textbox', { name: 'Email' }), 'a@b.co');
  await user.click(button);

  expect(onSubmit).toHaveBeenCalledExactlyOnceWith('a@b.co');
});
```

`userEvent.setup()` вызывают до рендера, а каждое действие ждут через `await`. `vi.fn()` запоминает, как его вызывали.

## Ошибки новичка

- ❌ Забыть `await` перед `user.click` → ✅ ждать.
- ❌ Искать по CSS-классу → ✅ по роли и имени.
- ❌ Проверять внутреннее состояние → ✅ проверять то, что на экране.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [Testing Library: о запросах](https://testing-library.com/docs/queries/about)
- [Testing Library: user-event](https://testing-library.com/docs/user-event/intro)
- [React Testing Library: API](https://testing-library.com/docs/react-testing-library/api)
- [Vitest: environment](https://vitest.dev/config/environment)
- [Vitest: vi](https://vitest.dev/api/vi)
- [jest-dom: с Vitest](https://github.com/testing-library/jest-dom#with-vitest)
