---
title: "Состояние и события: продвинутая теория"
lang: ru
track: react
kind: module
doc: advanced
slug: 03-state-and-events
part: 1
tags: [react, state, use-state, events, hooks]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Состояние и события: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Состояние — это снимок

```tsx
onClick={() => { setN(n + 1); setN(n + 1); setN(n + 1); console.log(n); }}
// logs 0; the next render shows 1

onClick={() => { setN(x => x + 1); setN(x => x + 1); setN(x => x + 1); }}
// n grows by 3; one render

onClick={() => { setN(n + 5); setN(x => x + 1); }}
// from 4:  4 + 5 = 9, then 9 + 1 = 10
```

Обработчики, созданные во время рендера, видят значения именно этого рендера. `setN` не меняет `n`, а просит следующий рендер. Функция, переданная в установку, получает значение, поставленное в очередь до неё.

## Пакетные обновления и отложенный код

```tsx
onClick={() => setTimeout(() => { setA(1); setB(1); }, 5)}     // one render, not two

onClick={() => { setN(n + 1); setTimeout(() => console.log(n), 5); }}
// the screen shows 1, the timeout logs 0
```

С React 18 обновления объединяются автоматически, в том числе в таймерах и промисах. Таймер во втором примере по-прежнему держит старый снимок.

## Объекты

```tsx
const [u, setU] = useState({ name: 'Ann', age: 1 });

u.age++; setU(u);                       // screen: still 1 (same object)
setU({ ...u, age: u.age + 1 });         // screen: 3 (the mutation had already made age 2)
```

React сравнивает новое значение со старым через `Object.is`. Изменённый объект — тот же объект, поэтому обновление не запрашивается.

## Массивы

```tsx
// list starts as [3, 1, 2]
list.push(9); setList(list);                    // screen: 3,1,2   (nothing happens)
setList([...list, 4]);                          // screen: 3,1,2,9,4   (the 9 surfaces now)
setList(list.toSorted((a, b) => a - b));        // 1,2,3,4,9   (TypeScript: lib es2023)
setList(list.filter(x => x !== 1));             // 2,3,4,9
setList(list.map(x => x * 10));                 // 20,30,40,90
```

Используйте `filter`, `map`, spread и копии. `push`, `splice`, `sort` и `reverse` меняют массив на месте.

## Где живёт состояние

```tsx
function Field({ label }: { label: string }) {
  const [n, setN] = useState(0);
  return <button onClick={() => setN(n + 1)}>{label}:{n}</button>;
}

<Field label={who} />               // click, then switch who from ann to bob:  bob:1
<Field key={who} label={who} />     // the same steps:                          bob:0

<><Field label="a" /><Field label="b" /></>   // click the first:  a:1  b:0
```

Состояние привязано к позиции в дереве рендера, а не к написанному вами тегу JSX. Тот же компонент в той же позиции сохраняет состояние при смене props; другой `key` даёт новое.

Другой тип элемента выше тоже сбрасывает всё ниже:

```tsx
{b ? <section><Counter /></section> : <div><Counter /></div>}
// count to 1, then toggle b:  the counter is 0 again — the parent type changed
```

## Лучшие практики

- Храните минимум; остальное вычисляйте при рендере.
- Связанные значения держите вместе, независимые — раздельно.
- Используйте функции-обновления, когда новое значение зависит от старого.
- Всю форму сбрасывайте сменой `key`.

## Частые ошибки

- ❌ Изменить объект и установить его же → ✅ создать копию.
- ❌ Копировать props в состояние → ✅ использовать prop.
- ❌ Объявлять компонент внутри компонента → ✅ на уровне модуля.
- ❌ Хранить и список, и его длину → ✅ вычислять длину.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [React: useState](https://react.dev/reference/react/useState)
- [React: реакция на события](https://react.dev/learn/responding-to-events)
- [React: состояние как снимок](https://react.dev/learn/state-as-a-snapshot)
- [React: сохранение и сброс состояния](https://react.dev/learn/preserving-and-resetting-state)
- [React: updating arrays in state](https://react.dev/learn/updating-arrays-in-state)
- [React: flushSync](https://react.dev/reference/react-dom/flushSync)
- [React: rules of Hooks](https://react.dev/reference/rules/rules-of-hooks)
- [React 18: automatic batching](https://react.dev/blog/2022/03/29/react-v18)
