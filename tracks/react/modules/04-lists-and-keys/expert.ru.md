---
title: "Списки, ключи и идентичность: экспертная теория"
lang: ru
track: react
kind: module
doc: expert
slug: 04-lists-and-keys
part: 1
tags: [react, keys, lists, identity, state]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Списки, ключи и идентичность: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

При новом рендере React сравнивает новые элементы со старыми среди детей каждого родителя. Элемент «тот же», когда совпадают тип и ключ; без ключа берётся позиция. Для того же элемента React сохраняет экземпляр компонента и его состояние и обновляет props; иначе старый размонтируется и монтируется новый.

### Позиции и пустые слоты

Элемент, равный `false` или `null`, всё равно занимает место:

```tsx
function Slot() {
  const [warn, setWarn] = useState(false);
  return <>
    <button onClick={() => setWarn(!warn)}>warn</button>
    {warn && <p>careful</p>}          {/* false still occupies this position */}
    <ul><Item name="a" /></ul>
  </>;
}
// click the item, then show the warning:  a:1  (state kept)
```

Возврат дерева другой формы — другое дело:

```tsx
if (warn) return <><button /><p>careful</p><ul><Item name="a" /></ul></>;
return <><button /><ul><Item name="a" /></ul></>;
// click the item, then switch warn:  a:0  (the list moved to another position)
```

### Родитель имеет значение

Ключ действует среди детей одного родителя:

```tsx
<ul>{!right && <Item key="a" name="a" />}</ul>
<ul>{right && <Item key="a" name="a" />}</ul>
// click, then move:  a:0 ; the old instance unmounts and a new one mounts
```

## Справочник

| Ситуация | Состояние |
|---|---|
| Тот же тип, тот же ключ или позиция | сохраняется, props обновляются |
| Другой тип в позиции | сбрасывается вместе со всем поддеревом |
| Другой `key` | сбрасывается |
| Перемещение среди соседей с устойчивым ключом | сохраняется |
| Перенос к другому родителю | сбрасывается |
| Скрытие через CSS или `hidden` | сохраняется |
| Удаление по условию | теряется |

| Источник ключа | Применение |
|---|---|
| Id из базы | выбор по умолчанию |
| `crypto.randomUUID()` при создании | записи, появляющиеся на клиенте |
| Естественное уникальное значение | если оно действительно не меняется |
| Индекс | только для неизменного списка |
| `Math.random()` при рендере | никогда |

## Подвохи

### Ключи, меняющиеся при каждом рендере

```tsx
{items.map(x => <Item key={Math.random()} name={x} />)}
// every render of the list unmounts all items and mounts new ones: state and focus are lost
```

### Изменение формы дерева теряет состояние

```tsx
if (warn) return <><button /><p>careful</p><ul><Item name="a" /></ul></>;
return <><button /><ul><Item name="a" /></ul></>;
// click the item, then switch warn:  a:0  (the list moved to another position)
```

Держите структуру устойчивой и переключайте только различающиеся части: `{warn && <p>careful</p>}`.

### Тот же ключ в другом месте

```tsx
<ul>{!right && <Item key="a" name="a" />}</ul>
<ul>{right && <Item key="a" name="a" />}</ul>
// click, then move:  a:0 ; the old instance unmounts and a new one mounts
```

## Компромиссы и альтернативы

| Выбор | Плюс | Минус |
|---|---|---|
| Держать скрытые элементы смонтированными | состояние и прокрутка сохраняются | память и стоимость рендера |
| Размонтировать и поднять состояние | дешёвый DOM | больше кода в родителе |
| Сброс через `key` | одна строка, без эффектов | всё поддерево создаётся заново |
| Виртуализированный список | выдерживает тысячи строк | строки вне экрана не смонтированы, их состояние держите снаружи |

## Вопросы с собеседований

**1. Зачем нужны ключи?**

<details><summary>Ответ</summary>

Чтобы сопоставлять элементы между рендерами и оставлять состояние и узлы DOM у нужной записи.

</details>

**2. Должны ли ключи быть уникальны глобально?**

<details><summary>Ответ</summary>

Нет, только среди соседей.

</details>

**3. Когда индекс допустим как ключ?**

<details><summary>Ответ</summary>

Когда список никогда не переставляется, не получает вставок в середину и не фильтруется, а у элементов нет своего состояния. Id из данных всё равно надёжнее.

</details>

**4. Что будет со случайным ключом?**

<details><summary>Ответ</summary>

Каждый рендер размонтирует старые элементы и монтирует новые.

</details>

**5. Может ли компонент прочитать свой ключ?**

<details><summary>Ответ</summary>

Нет.

</details>

**6. Как дать ключ нескольким элементам?**

<details><summary>Ответ</summary>

`<Fragment key={...}>`; краткая запись ключ не принимает.

</details>

**7. Почему `{warn && <p/>}` не сбрасывает компонент после него?**

<details><summary>Ответ</summary>

`false` всё равно занимает слот, поэтому позиции следующих элементов не сдвигаются.

</details>

**8. Сохранится ли состояние при переносе элемента к другому родителю?**

<details><summary>Ответ</summary>

Нет, даже с тем же ключом.

</details>

**9. Подвох: два разных списка используют `key="1"`. Проблема?**

<details><summary>Ответ</summary>

Нет.

</details>

**10. Подвох: элемент пропал с экрана, а состояние осталось. Как?**

<details><summary>Ответ</summary>

Он скрыт через CSS или `hidden`, а не удалён из дерева.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: вывод списков](https://react.dev/learn/rendering-lists)
- [React: сохранение и сброс состояния](https://react.dev/learn/preserving-and-resetting-state)
