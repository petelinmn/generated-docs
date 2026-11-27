---
title: "Props и композиция: продвинутая теория"
lang: ru
track: react
kind: module
doc: advanced
slug: 02-props-and-composition
part: 1
tags: [react, props, composition, children, typescript]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Props и композиция: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Приём остальных props

```tsx
type ButtonProps = ComponentProps<'button'> & { variant?: 'primary' | 'plain' };

function Button({ variant = 'plain', className = '', ...rest }: ButtonProps) {
  return <button type="button" {...rest} className={`btn ${variant} ${className}`.trim()} />;
}

<Button disabled>Save</Button>
// <button type="button" disabled="" class="btn plain">Save</button>
<Button variant="primary" type="submit" className="wide">Go</Button>
// <button type="submit" class="btn primary wide">Go</button>
```

`ComponentProps<'button'>` типизирует всё, что принимает `<button>`. Порядок важен: записанное после `{...rest}` перекрывает его, записанное до может быть перекрыто. Здесь у `type` есть значение по умолчанию, которое вызывающий может сменить, а `className` объединяется, а не заменяется.

## Слоты

Компонент с несколькими областями содержимого принимает несколько props с элементами (`header`, `actions`, `children`). Раскладку решает компонент, содержимое — вызывающий.

## Render props

Когда содержимое зависит от данных, которые держит компонент, передают функцию:

```tsx
function List<T>({ items, renderItem, getKey }: {
  items: T[]; renderItem: (item: T) => ReactNode; getKey: (item: T) => string;
}) {
  return <ul>{items.map(item => <li key={getKey(item)}>{renderItem(item)}</li>)}</ul>;
}

<List items={[{ id: 'a', n: 'Ann' }]} getKey={u => u.id} renderItem={u => <b>{u.n}</b>} />
// <ul><li><b>Ann</b></li></ul>
```

## Специализация

Конкретный компонент выводит общий с заранее заданными props. Для этого в React используют композицию, а не наследование.

```tsx
function Dialog({ children }: { children: ReactNode }) { return <div role="dialog">{children}</div>; }

function ConfirmDialog({ question }: { question: string }) {
  return <Dialog><p>{question}</p><Button variant="primary">Yes</Button></Dialog>;
}
// <div role="dialog"><p>Sure?</p><button type="button" class="btn primary">Yes</button></div>
```

## Ref

```tsx
function MyInput({ ref }: { ref?: Ref<HTMLInputElement> }) {
  return <input ref={ref} />;                   // React 19: ref is an ordinary prop
}

function Form() {
  const ref = useRef<HTMLInputElement>(null);
  return <><MyInput ref={ref} /><button onClick={() => ref.current?.focus()}>focus</button></>;
}
```

До React 19 функциональному компоненту был нужен `forwardRef`; в React 19 `ref` — это prop функциональных компонентов.

## Композиция и повторный рендер

```tsx
let slow = 0;
function Slow() { slow++; return <p>slow</p>; }

function Inside() {
  const [n, setN] = useState(0);
  return <div><button onClick={() => setN(n + 1)}>{n}</button><Slow /></div>;
}

function Wrapper({ children }: { children: ReactNode }) {
  const [n, setN] = useState(0);
  return <div><button onClick={() => setN(n + 1)}>{n}</button>{children}</div>;
}

// <Inside />, two clicks:                     slow = 3
// <Wrapper><Slow /></Wrapper>, two clicks:    slow = 1
```

Когда обновляется `Inside`, он создаёт новый элемент `<Slow />`, и `Slow` выполняется снова. `Wrapper` получает готовый элемент от родителя: это тот же объект, поэтому React его пропускает.

## Лучшие практики

- Называйте props событий `onSomething`, обработчики — `handleSomething`.
- Предпочитайте содержимое (`children`, слоты) настроечным флагам.
- Держите список props небольшим; объединяйте данные, всегда передаваемые вместе.
- Расширяйте нативные элементы через `ComponentProps`, а не перечисляйте атрибуты вручную.

## Частые ошибки

- ❌ `{...rest}` в конце, когда должны победить ваши значения → ✅ пишите их после spread.
- ❌ Заменять `className` вызывающего → ✅ объединять.
- ❌ Разворачивать в DOM-элемент весь объект данных → ✅ передавать только нужные атрибуты.
- ❌ Протаскивать значение через пять слоёв → ✅ передать готовый элемент или использовать context.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [React: передача props компоненту](https://react.dev/learn/passing-props-to-a-component)
- [React: мышление в стиле React](https://react.dev/learn/thinking-in-react)
- [React: общее состояние компонентов](https://react.dev/learn/sharing-state-between-components)
- [Выпуск React 19](https://react.dev/blog/2024/12/05/react-19)
- [React: memo](https://react.dev/reference/react/memo)
