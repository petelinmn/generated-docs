---
title: "Компоненты и JSX: экспертная теория"
lang: ru
track: react
kind: module
doc: expert
slug: 01-components-and-jsx
part: 1
tags: [react, jsx, components, rendering]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Компоненты и JSX: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

### Элементы

Компилятор превращает JSX в вызовы `jsx()` из `react/jsx-runtime` (automatic runtime), поэтому для самого JSX `import React` не нужен. Результат — обычный объект:

```tsx
const el = <h1 id="a">x</h1>;

String(el.$$typeof);          // 'Symbol(react.transitional.element)'
el.type;                      // 'h1'
Object.isFrozen(el);          // true   (development build)
Object.isFrozen(el.props);    // true   (development build)

isValidElement(el);                           // true
isValidElement({ type: 'h1', props: {} });    // false

jsx('h1', { className: 't', children: 'Hi' }); // what the compiler emits: <h1 class="t">Hi</h1>
```

Элемент — это описание, а не узел DOM и не экземпляр компонента. Символ `$$typeof` помечает настоящие элементы: объект, пришедший как JSON, не может содержать символ и поэтому элементом не считается.

### Запуск, рендер, фиксация

| Этап | Что происходит |
|---|---|
| Запуск (trigger) | первый рендер или обновление состояния в компоненте либо у предка |
| Рендер (render) | React вызывает компоненты и строит новое дерево элементов |
| Фиксация (commit) | React меняет только отличающиеся узлы DOM |

Рендер компонента влечёт рендер компонентов, которые он возвращает. Рендер — это ещё не обновление DOM.

## Справочник

| JSX | По смыслу компилируется в |
|---|---|
| `<div a="1">x</div>` | type `'div'`, props `{ a: '1', children: 'x' }` |
| `<Card />` | type `Card` (сама функция) |
| `<UI.Badge />` | type `UI.Badge` |
| `<>...</>` | type `Fragment` |
| `<div {...props} />` | props, развёрнутые из объекта |
| `{/* comment */}` | ничего |

| Атрибут | Замечание |
|---|---|
| `className`, `htmlFor` | вместо `class`, `for` |
| `style` | объект; имена в camelCase; числам без единиц добавляется `px` там, где CSS требует единицу |
| `aria-*`, `data-*` | пишутся с дефисами |
| `dangerouslySetInnerHTML={{ __html }}` | вставляет сырой HTML |
| `key` | идентичность среди соседей, компоненту не передаётся |
| `ref` | доступ к узлу DOM; в React 19 функциональный компонент получает его как prop |

| API вывода | Назначение |
|---|---|
| `createRoot(node).render(element)` | клиентское приложение |
| `hydrateRoot(node, element)` | подключение к HTML, отрендеренному на сервере |
| `renderToStaticMarkup(element)` | строка HTML без атрибутов React, как в этих примерах |
| `createElement`, `cloneElement`, `isValidElement`, `Fragment` | работа с элементами напрямую |

## Подвохи

### Имя со строчной буквы — это тег, а не ваша функция

```tsx
function my() { return <em>custom</em>; }
<my />        // <my></my>   — an unknown HTML tag, the function is not called
```

### Объект не может быть дочерним

```tsx
<p>{{ a: 1 }}</p>
// Error: Objects are not valid as a React child (found: object with keys {a}). ...
```

Частая причина — вывод `Date` или целой записи вместо одного её поля.

### Перенос строки после тега съедает пробел

```tsx
<p>
  one <i>two</i>
  three
</p>
// <p>one <i>two</i>three</p>

<p>
  one <i>two</i>{' '}
  three
</p>
// <p>one <i>two</i> three</p>
```

JSX удаляет пробельные участки с переносом строки. Там, где пробел нужен, пишите `{' '}`.

### Нечистый компонент даёт неожиданный вывод

```tsx
let guests = 0;
function Impure() {
  guests++;                       // side effect during render
  return <h2>Guest {guests}</h2>;
}

// client render in development inside <StrictMode>, two <Impure />:
// <h2>Guest 2</h2><h2>Guest 4</h2>
```

На сервере, где каждый компонент вызывался один раз, тот же код вывел `Guest 1` и `Guest 2`.

## Компромиссы и альтернативы

| Выбор | Плюс | Минус |
|---|---|---|
| JSX | разметка и логика вместе, проверка типов | нужна сборка |
| `createElement` вручную | без компилятора | многословно |
| Серверный рендер и фреймворки | быстрый первый показ, поисковики | больше движущихся частей |
| Только клиент (SPA на Vite) | просто развернуть | пусто, пока не загрузится JavaScript |
| Чистый код на DOM | нет зависимости | ручная синхронизация данных и вида |

## Вопросы с собеседований

**1. Что такое JSX?**

<details><summary>Ответ</summary>

Расширение синтаксиса, компилируемое в вызовы функций, создающие объекты-элементы.

</details>

**2. Чем элемент отличается от компонента?**

<details><summary>Ответ</summary>

Компонент — функция; элемент — неизменяемый объект с описанием того, что вывести: тип и props.

</details>

**3. Почему имена компонентов пишут с заглавной?**

<details><summary>Ответ</summary>

Тег со строчной буквы считается встроенным HTML-элементом.

</details>

**4. Из каких этапов состоит обновление?**

<details><summary>Ответ</summary>

Запуск (trigger), рендер (вызов компонентов), фиксация (изменение DOM).

</details>

**5. Всегда ли рендер меняет DOM?**

<details><summary>Ответ</summary>

Нет. React меняет только отличающиеся узлы.

</details>

**6. Что делает StrictMode?**

<details><summary>Ответ</summary>

В разработке дважды вызывает функции, которые должны быть чистыми, и выполняет дополнительные проверки; на production не влияет.

</details>

**7. Почему пользовательский текст в JSX безопасен?**

<details><summary>Ответ</summary>

Он экранируется. `dangerouslySetInnerHTML` эту защиту убирает. URL — отдельный вопрос: React 19 блокирует `javascript:`-адреса, но другие схемы, например `data:`, пропускает, поэтому адреса от пользователей проверяйте.

</details>

**8. Зачем нужен фрагмент?**

<details><summary>Ответ</summary>

Чтобы вернуть несколько элементов без лишнего узла DOM.

</details>

**9. Подвох: что покажет `{items.length && <List />}` для пустого массива?**

<details><summary>Ответ</summary>

`0`.

</details>

**10. Подвох: можно ли написать `if` в фигурных скобках JSX?**

<details><summary>Ответ</summary>

Нет, только выражения. Тернарный оператор, `&&` или `if` до `return`.

</details>

**11. Почему не объявлять компонент внутри другого?**

<details><summary>Ответ</summary>

Каждый рендер создаёт новую функцию, React считает её другим типом и сбрасывает состояние.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: разметка на JSX](https://react.dev/learn/writing-markup-with-jsx)
- [React: рендер и фиксация](https://react.dev/learn/render-and-commit)
- [React: StrictMode](https://react.dev/reference/react/StrictMode)
- [Выпуск React 19](https://react.dev/blog/2024/12/05/react-19)
- [Версии React](https://react.dev/versions)
