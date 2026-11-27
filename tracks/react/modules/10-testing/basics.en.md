---
title: "Testing: basics"
lang: en
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

# Testing: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [forms](../05-forms/overview.en.md) · **Time:** ~2 h

The examples ran with React 19.3.1, Vitest 5.1.2, jsdom 31.0.1, React Testing Library 16.3.3, user-event 14.6.13 and jest-dom 8.0.0.

## Setup

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

The setup file adds matchers such as `toBeInTheDocument` and clears the document after each test. Automatic cleanup needs a global `afterEach`, which Vitest does not add unless its `globals` option is on. jest-dom 8 requires Vitest 5 or later. Run with `npx vitest run`.

## A first test

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

Three steps: render, act, check.

## Finding elements

| Query | Use |
|---|---|
| `getByRole` | the first choice: buttons, headings, textboxes by name |
| `getByLabelText` | form fields |
| `getByText` | non-interactive content |
| `getByTestId` | the last resort |

## A form

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

`userEvent.setup()` is called before rendering, and every interaction is awaited. `vi.fn()` records how it was called.

## Beginner mistakes

- ❌ Forgetting `await` before `user.click` → ✅ await it.
- ❌ Selecting by CSS class → ✅ by role and name.
- ❌ Checking internal state → ✅ check what is on screen.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [Testing Library: about queries](https://testing-library.com/docs/queries/about)
- [Testing Library: user-event](https://testing-library.com/docs/user-event/intro)
- [React Testing Library: API](https://testing-library.com/docs/react-testing-library/api)
- [Vitest: environment](https://vitest.dev/config/environment)
- [Vitest: vi](https://vitest.dev/api/vi)
- [jest-dom: with Vitest](https://github.com/testing-library/jest-dom#with-vitest)
