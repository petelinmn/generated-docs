---
title: "Компоненты и JSX: продвинутая теория"
lang: ru
track: react
kind: module
doc: advanced
slug: 01-components-and-jsx
part: 1
tags: [react, jsx, components, rendering]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Компоненты и JSX: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Что можно выводить

| Значение в качестве дочернего | Результат |
|---|---|
| строка, число | текст, включая `0` и `NaN` |
| элемент | выводится |
| массив | каждый член по порядку |
| `false`, `true`, `null`, `undefined`, `''` | ничего видимого |
| простой объект | ошибка |

```tsx
<p>{0}{false}{null}{undefined}{true}{''}{'x'}{NaN}</p>   // <p>0xNaN</p>
<p>{[1, 2, 3]}</p>                                       // <p>123</p>

const items: string[] = [];
<div>{items.length && <b>has</b>}</div>       // <div>0</div>
<div>{items.length > 0 && <b>has</b>}</div>   // <div></div>

<div>{false ?? 'a'}{0 || 'b'}{0 ?? 'c'}</div>  // <div>b0</div>
```

`items.length && ...` для пустого списка выведет `0`: слева от `&&` должно быть логическое значение.

## Динамические теги и компоненты

```tsx
const UI = { Badge: ({ text }: { text: string }) => <mark>{text}</mark> };
<UI.Badge text="new" />          // <mark>new</mark>

const tag = 'section';
const Tag = tag;                 // must start with a capital letter
<Tag>dyn</Tag>                   // <section>dyn</section>

function Maybe({ show }: { show: boolean }) {
  if (!show) return null;        // renders nothing
  return <b>yes</b>;
}
```

Регистр первой буквы определяет смысл в JSX: `<section>` — строка `'section'`, `<Tag>` — значение переменной `Tag`.

## Экранирование и сырой HTML

```tsx
<p>{'<img src=x onerror=alert(1)>'}</p>
// <p>&lt;img src=x onerror=alert(1)&gt;</p>

<div dangerouslySetInnerHTML={{ __html: '<b>raw</b>' }} />
// <div><b>raw</b></div>
```

Текст из данных экранируется. `dangerouslySetInnerHTML` это отключает, поэтому используйте его только с очищенным HTML.

## Чистота

Компонент должен вычислять результат из входа и не изменять ничего, что существовало до вызова. React может вызвать его не один раз.

```tsx
let guests = 0;
function Impure() {
  guests++;                       // side effect during render
  return <h2>Guest {guests}</h2>;
}

// client render in development inside <StrictMode>, two <Impure />:
// <h2>Guest 2</h2><h2>Guest 4</h2>
```

`StrictMode` в режиме разработки вызывает функции компонентов дважды, чтобы такие ошибки проявились. В production он этого не делает.

## Лучшие практики

- Один компонент — одна обязанность; повторяющуюся разметку выносите.
- Объявляйте компоненты на верхнем уровне модуля, а не внутри других компонентов.
- Сложные условия выносите из JSX в переменные перед `return`.
- Используйте семантические элементы и атрибуты `aria-*`.

## Частые ошибки

- ❌ `count && <Badge />` с числом → ✅ `count > 0 && <Badge />`.
- ❌ Менять глобальную переменную при рендере → ✅ вычислять локально.
- ❌ Сырой HTML пользователей в `dangerouslySetInnerHTML` → ✅ очищать или выводить текстом.
- ❌ Глубоко вложенный тернарный оператор → ✅ переменные или компоненты поменьше.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [React: разметка на JSX](https://react.dev/learn/writing-markup-with-jsx)
- [React: рендер и фиксация](https://react.dev/learn/render-and-commit)
- [React: StrictMode](https://react.dev/reference/react/StrictMode)
