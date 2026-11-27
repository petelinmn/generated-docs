---
title: "Списки, ключи и идентичность: продвинутая теория"
lang: ru
track: react
kind: module
doc: advanced
slug: 04-lists-and-keys
part: 1
tags: [react, keys, lists, identity, state]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Списки, ключи и идентичность: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Что делает неверный ключ

```tsx
function Item({ name }: { name: string }) {
  const [n, setN] = useState(0);
  return <li><button onClick={() => setN(n + 1)}>{name}:{n}</button></li>;
}

// items = ['a', 'b', 'c'];  click a once, then reverse, then delete the first item
```

| Шаг | `key={item}` (устойчивый id) | `key={index}` | `key={Math.random()}` |
|---|---|---|---|
| после клика | `a:1 b:0 c:0` | `a:1 b:0 c:0` | `a:1 b:0 c:0` |
| переворот | `c:0 b:0 a:1` | `c:1 b:0 a:0` | `c:0 b:0 a:0` |
| удаление первого | `b:0 a:1` | `b:1 a:0` | `b:0 a:0` |

С устойчивым id состояние переезжает вместе с элементом. С индексом оно остаётся на позиции, и его наследует другой элемент. Со случайным ключом каждый рендер создаёт новые элементы, поэтому всё состояние теряется.

То же происходит с тем, чем React не управляет: текст, набранный в неуправляемом поле, при индексных ключах остаётся на своей позиции, когда выше вставляют строку.

## Несколько элементов на одну запись

```tsx
<dl>
  {rows.map(r => (
    <Fragment key={r.id}>        {/* <> ... </> cannot take a key */}
      <dt>{r.term}</dt>
      <dd>{r.definition}</dd>
    </Fragment>
  ))}
</dl>
```

## Создание идентификаторов

Id создают один раз, вместе с данными: сервером или в обработчике, добавляющем запись (`crypto.randomUUID()`), а не во время рендера.

## Намеренный сброс состояния

```tsx
<ProfileForm key={user.id} user={user} />   // another user: a new form with empty state
```

## Скрыть или удалить

```tsx
<ul style={{ display: show ? 'block' : 'none' }}><Item name="a" /></ul>
// click, hide, show:  a:1  — hidden by CSS, the component stays mounted
```

Условный вывод (`{show && <List />}`) удаляет компонент вместе с состоянием; CSS или атрибут `hidden` его сохраняют.

## Лучшие практики

- Создавайте id там, где создаются данные.
- В состоянии держите критерии фильтра и сортировки, а не отфильтрованный результат.
- Поднимайте состояние выше, если оно должно пережить удаление элемента.

## Частые ошибки

- ❌ `key={index}` в редактируемом списке → ✅ id из данных.
- ❌ `key={Math.random()}`, чтобы «принудительно обновить» → ✅ исправить то, что не обновляется.
- ❌ Ключи, собранные из редактируемого текста → ✅ состояние сбрасывается при каждой правке; берите id.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [React: вывод списков](https://react.dev/learn/rendering-lists)
- [React: сохранение и сброс состояния](https://react.dev/learn/preserving-and-resetting-state)
