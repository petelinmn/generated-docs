---
title: "Списки, ключи и идентичность: базовая теория"
lang: ru
track: react
kind: module
doc: basics
slug: 04-lists-and-keys
part: 1
tags: [react, keys, lists, identity, state]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Списки, ключи и идентичность: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [состояние и события](../03-state-and-events/overview.ru.md) · **Время:** ~1,5 ч

Все результаты получены на React 19.3.1.

## Из данных в элементы

```tsx
const users = [{ id: 'u1', name: 'Ann' }, { id: 'u2', name: 'Bob' }];

<ul>
  {users.map(u => <li key={u.id}>{u.name}</li>)}
</ul>
```

`map` возвращает массив, и React выводит каждый его элемент.

## Правила для ключей

- Уникальны среди соседей (а не во всём приложении).
- Устойчивы: один и тот же элемент сохраняет ключ между рендерами.
- Пишутся на самом внешнем элементе, возвращаемом из `map`.
- Не передаются компоненту: если ему нужен id, передайте его другим prop.

```tsx
<ul>{['a', 'b'].map(x => <li>{x}</li>)}</ul>
// console: Each child in a list should have a unique "key" prop.

<ul>{['a', 'a'].map(x => <li key={x}>{x}</li>)}</ul>
// console: Encountered two children with the same key ...

<><ul><li key="1">x</li></ul><ul><li key="1">y</li></ul></>
// no message: keys have to be unique only among siblings
```

## Фильтрация и сортировка

Видимый список вычисляйте при рендере; не держите вторую копию в состоянии.

```tsx
const visible = products
  .filter(p => p.name.toLowerCase().includes(query.toLowerCase()))
  .toSorted((a, b) => a.name.localeCompare(b.name));

return <ul>{visible.map(p => <ProductRow key={p.id} product={p} />)}</ul>;
```

`toSorted` возвращает новый массив и не трогает исходный.

## Ошибки новичка

- ❌ Нет `key` → ✅ id из данных.
- ❌ `key` на внутреннем элементе → ✅ на элементе, возвращаемом из `map`.
- ❌ Читать `props.key` → ✅ передать отдельный prop.
- ❌ `products.sort(...)` над состоянием или props → ✅ `toSorted` или копия.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [React: вывод списков](https://react.dev/learn/rendering-lists)
- [React: сохранение и сброс состояния](https://react.dev/learn/preserving-and-resetting-state)
