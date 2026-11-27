---
title: "Производительность рендера: экспертная теория"
lang: ru
track: react
kind: module
doc: expert
slug: 11-performance
part: 3
tags: [react, performance, memoization, react-compiler, profiling]
versions: "React 19.3"
status: draft
updated: 2026-11-27
---

# Производительность рендера: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Когда компонент рендерится, React рендерит возвращённые им элементы. Он может пропустить дочерний, когда элемент — тот же самый объект, что и в прошлый раз, или когда `memo`-компонент получает попарно равные props. Обновления состояния объединяются, а DOM затрагивают только зафиксированные различия.

## React Compiler

React Compiler 1.0 стабилен. Это инструмент времени сборки (например, плагин Babel), автоматически добавляющий мемоизацию. Он работает с React 17 и новее; версиям до 19 нужны настройка `target` и пакет `react-compiler-runtime`.

```bash
npm install --save-dev --save-exact babel-plugin-react-compiler@latest
```

```tsx
// source
function Total({ list, label }) {
  const total = list.reduce((a, b) => a + b, 0);
  return <p>{label}: {total}</p>;
}
```

```js
// output of babel-plugin-react-compiler 1.0.0
import { c as _c } from "react/compiler-runtime";
function Total(t0) {
  const $ = _c(5);
  const { list, label } = t0;
  let t1;
  if ($[0] !== list) {
    t1 = list.reduce(_temp, 0);
    $[0] = list;
    $[1] = t1;
  } else {
    t1 = $[1];
  }
  const total = t1;
  let t2;
  if ($[2] !== label || $[3] !== total) {
    t2 = <p>{label}: {total}</p>;
    $[2] = label;
    $[3] = total;
    $[4] = t2;
  } else {
    t2 = $[4];
  }
  return t2;
}
function _temp(a, b) { return a + b; }
```

Скомпилированная функция хранит прежние входы и результаты и пересчитывает только зависящее от изменившегося значения. Он рассчитывает на код, соблюдающий правила React; правила линтера из `eslint-plugin-react-hooks` сообщают о нарушениях вроде установки состояния во время рендера. `useMemo` и `useCallback` по-прежнему работают и служат запасным средством.

## Справочник

| Инструмент | Назначение |
|---|---|
| `memo(C, areEqual?)` | пропустить рендер при равных props |
| `useMemo(fn, deps)` | сохранить значение |
| `useCallback(fn, deps)` | сохранить функцию |
| `<Profiler id onRender>` | измерять из кода |
| React Compiler | мемоизация при сборке |
| Библиотека виртуализации | рендерить только видимые строки |
| `useTransition`, `useDeferredValue` | сохранять отзывчивость ввода: см. следующий модуль |

## Подвохи

### Компаратор, пропускающий prop

```tsx
const ById = memo(
  function ById({ user }: { user: { id: number; name: string } }) { return <i>{user.name}</i>; },
  (a, b) => a.user.id === b.user.id,        // ignores the name
);
// render with {id: 1, name: 'Ann'}, then {id: 1, name: 'Bob'}:  the screen still shows Ann
```

### Мемоизированное значение и мутация

```tsx
const sum = useMemo(() => total(list), [list]);
list.push(4);        // mutation: the same array, so the memoized value is stale
```

### Меняющийся ключ пересоздаёт всё

```tsx
{[1, 2, 3].map(i => <Row key={`${salt}-${i}`} />)}
// same salt: nothing remounts;  salt changes: all three rows are mounted again
```

### memo и новые props

```tsx
function Parent() {
  const [n, setN] = useState(0);
  const style = useMemo(() => ({ color: 'red' }), []);
  const on = useCallback(() => {}, []);
  return <>
    <button onClick={() => setN(n + 1)}>{n}</button>
    <Plain />                                  {/* renders */}
    <Memo n={1} />                             {/* skipped */}
    <MemoObj style={{ color: 'red' }} />       {/* renders: new object */}
    <MemoFn on={() => {}} />                   {/* renders: new function */}
    <MemoStable style={style} on={on} />       {/* skipped */}
    <MemoChildren><b /></MemoChildren>         {/* renders: new element */}
  </>;
}
```

## Компромиссы и альтернативы

| Выбор | Плюс | Минус |
|---|---|---|
| Перестроить состояние и композицию | без дополнительных затрат | нужно продумать устройство |
| Ручная мемоизация | точно | шум в коде, легко ошибиться |
| React Compiler | автоматически | шаг сборки, правила надо соблюдать |
| Виртуализация | огромные списки | строки не смонтированы, поиск по странице и доступность требуют работы |
| Внешнее хранилище с селекторами | точечные подписки | зависимость |

## Вопросы с собеседований

**1. Почему компонент рендерится заново?**

<details><summary>Ответ</summary>

Изменилось его состояние, отрендерился родитель или изменился читаемый context.

</details>

**2. Что сравнивает `memo`?**

<details><summary>Ответ</summary>

Каждый prop через `Object.is`.

</details>

**3. Почему `memo` может не помочь?**

<details><summary>Ответ</summary>

Объект, функция или элемент в props новые при каждом рендере.

</details>

**4. `useMemo` против `useCallback`?**

<details><summary>Ответ</summary>

Первый сохраняет вычисленное значение, второй — функцию.

</details>

**5. Гарантирует ли мемоизация что-то?**

<details><summary>Ответ</summary>

Нет, это оптимизация.

</details>

**6. Что делает React Compiler?**

<details><summary>Ответ</summary>

При сборке сам мемоизирует значения и элементы внутри компонентов и хуков.

</details>

**7. Нужен ли `useMemo` с компилятором?**

<details><summary>Ответ</summary>

Редко: они остаются запасным средством.

</details>

**8. Как измерить производительность рендера?**

<details><summary>Ответ</summary>

Профилировщик DevTools, дорожки Performance, `<Profiler>`.

</details>

**9. Подвох: мемоизированный потомок получает `children`. Пропускается ли он при рендере родителя?**

<details><summary>Ответ</summary>

Нет, если элемент создаётся в родителе: это новый объект.

</details>

**10. Подвох: при установке того же значения состояния рендера гарантированно не будет?**

<details><summary>Ответ</summary>

Дочерние пропускаются, но сам компонент React может вызвать.

</details>

**11. Почему длинный список может остаться медленным после `memo`?**

<details><summary>Ответ</summary>

Список и тысячи узлов всё равно нужно обработать; виртуализируйте его.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [React: memo](https://react.dev/reference/react/memo)
- [React: useMemo](https://react.dev/reference/react/useMemo)
- [React: useCallback](https://react.dev/reference/react/useCallback)
- [React Compiler: введение](https://react.dev/learn/react-compiler/introduction)
- [React Compiler 1.0](https://react.dev/blog/2025/10/07/react-compiler-1)
- [React: Profiler](https://react.dev/reference/react/Profiler)
- [React: Performance tracks](https://react.dev/reference/dev-tools/react-performance-tracks)
