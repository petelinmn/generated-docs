---
title: "Итераторы и генераторы: экспертный уровень"
lang: ru
track: javascript
kind: module
doc: expert
slug: 11-iterators-and-generators
part: 2
tags: [iteration-protocol, generator-states, iterator-prototype, async-iteration, iterator-close, for-await, events-on]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Итераторы и генераторы: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все результаты получены запуском на Node.js 24.15.

## Справочник

### Протоколы

| Протокол | Что требуется | Результат `next()` |
|---|---|---|
| Итерируемый (iterable) | метод `[Symbol.iterator]()`, возвращающий итератор | — |
| Итератор (iterator) | метод `next()`; необязательны `return()` и `throw()` | `{ value, done }` |
| Асинхронный итерируемый | метод `[Symbol.asyncIterator]()` | — |
| Асинхронный итератор | метод `next()` | промис `{ value, done }` |

### Методы объекта-генератора

| Метод | Действие | Результат в опыте |
|---|---|---|
| `next(x)` | возобновляет; `x` — результат `yield` | `{ value, done }`; для первого вызова `x` отбрасывается |
| `return(v)` | завершает; выполняет `finally` | `{ value: v, done: true }` |
| `throw(e)` | бросает `e` в точку `yield` | результат следующего `yield` или исключение наружу |

Для **ещё не начатого** генератора `return('x')` сразу завершает его (`{ value: 'x', done: true }`, затем `next()` → `{ value: undefined, done: true }`). Для **завершённого** `throw(err)` пробрасывает исключение наружу (в опыте `rethrown late`).

### Состояния генератора

| Состояние | Когда |
|---|---|
| suspendedStart | создан, `next()` ещё не вызывался |
| suspendedYield | остановлен на `yield` |
| executing | выполняется тело |
| completed | вернул `return`, выбросил исключение или завершён `return()` |

Вызов `next()` у выполняющегося генератора — `TypeError: Generator is already running` (в опыте).

### Хелперы итераторов (в опыте)

`Iterator.prototype`: `drop`, `every`, `filter`, `find`, `flatMap`, `forEach`, `map`, `reduce`, `some`, `take`, `toArray`. Статические члены `Iterator`: `from`. `new Iterator()` — `TypeError: Abstract class Iterator not directly constructable`; `typeof Iterator` — `function`.

### Потребители итерируемых и закрытие

| Потребитель | Вызывает `return()` |
|---|---|
| `for...of` при `break`/`return`/исключении | да |
| `for...of` при нормальном завершении | нет |
| Деструктуризация, взявшая не все значения (`[a, b] = it`) | да (в опыте `closed`) |
| Деструктуризация, исчерпавшая итератор | нет |
| Spread, `Array.from` | нет (читают до конца) |

## Внутреннее устройство

- **Цепочка прототипов генератора.** Объект-генератор → `g.prototype` (у каждой генераторной функции свой) → `%GeneratorPrototype%` с методами `next`, `return`, `throw` → `Iterator.prototype` (в опыте все связи `true`). Поэтому у генератора есть хелперы итераторов и `[Symbol.iterator]`, возвращающий сам объект (`go[Symbol.iterator]() === go` — `true`).
- **Конечный автомат.** Генератор — функция, которую движок превращает в автомат состояний: локальные переменные и точка остановки сохраняются в объекте между вызовами `next()`. Отсюда состояния из таблицы выше.
- **`yield` и приоритет.** `yield` имеет очень низкий приоритет: `yield 1 + 1` отдаёт `2` (в опыте). Результат `yield` обычно берут в скобки: `const x = (yield a) + 1`.
- **`yield` в контексте.** `[yield 'a', yield 'b']` — два приостановки; значения из `next(1)` и `next(2)` собираются в массив (в опыте `{ value: [ 1, 2 ], done: true }`).
- **Закрытие итератора.** Движок вызывает `return()` в операциях, которые завершаются раньше конца. Ошибка в самом `return()` при нормальном выходе подменяет результат; при исключении исходная ошибка сохраняется.

> ⚠️ Проверить: точные правила приоритета ошибок из `return()` и поведение при выходе через `throw` в цикле разобраны по спецификации, а не запуском.

- **Асинхронные генераторы и очередь.** У асинхронного генератора есть внутренняя очередь запросов: несколько вызовов `next()` подряд, не дожидаясь результата, выполняются по порядку (в опыте четыре параллельных `next()` дали `1:false 2:false 3:false undefined:true`).
- **`for await` и синхронные итерируемые.** Если у объекта нет `[Symbol.asyncIterator]`, `for await` берёт `[Symbol.iterator]` и дожидается каждого значения (в опыте `fa x`, `fa y`). Если есть оба, используется асинхронный.
- **Закрытие в `for await`.** `break` вызывает `return()` асинхронного итератора (в опыте `async closed`).
- **Потоки Node.js.** Читаемые потоки (`stream.Readable`) и `events.on(emitter, name)` реализуют асинхронную итерацию: `for await (const [v] of on(ee, 'd', { signal }))` получает `ev 1`, `ev 2`, а по сигналу отмены завершается `AbortError` (в опыте).

> ⚠️ Проверить: итерация потоков `Readable` через `for await` и тонкости обратного давления не запускались (см. [модуль 16](../../overview.ru.md) дорожной карты).

## Подвохи

### 1. Первый `next(x)` отбрасывает `x`

`g.next('ignored')` — значение никуда не попадает; генератор лишь стартует до первого `yield`.

### 2. `for...of` не видит `return`

`function* r() { yield 1; return 99 }` → `[...r()]` — `[ 1 ]`.

### 3. Итератор потребляется

`const it = [1, 2, 3][Symbol.iterator](); [...it]` даёт `[ 1, 2, 3 ]`, повторно — `[]`. Если часть значений уже взята, spread вернёт остаток (`[ 2, 3 ]`).

### 4. `return()` вызывается не всегда

Не вызывается после нормального завершения и при spread; вызывается при `break`, `throw` в теле цикла и неполной деструктуризации.

### 5. `new` для генератора

`new gen()` — `TypeError: gen is not a constructor`.

### 6. Повторный вход

Вызов `next()` изнутри самого генератора — `TypeError: Generator is already running`.

### 7. Строка по code points

`[...'👨‍👩'].length` — `3` (мужчина, соединитель нулевой ширины, женщина), а не `1`: итерация по code points не равна итерации по «видимым символам» ([модуль 08](../08-strings-numbers-dates-regex/expert.ru.md)).

### 8. `for await` последователен

Параллельные задачи он не запустит; для параллелизма — `Promise.all` ([модуль 12](../12-promises-and-async-await/overview.ru.md)).

### 9. `yield` без скобок в выражении

`const x = yield a + 1` отдаёт `a + 1`, а не `a`: приоритет `yield` низкий.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Генератор | короткий код, ленивость, закрытие | нет «заглянуть назад», один проход |
| Ручной итератор | полный контроль, `return()` | много кода |
| Массив целиком | простота, `length`, индексы | вся память сразу |
| Хелперы итераторов | ленивые конвейеры без массивов | нужна современная среда (в Node.js 24 доступны; другие версии и браузеры не проверялись) |
| Асинхронные генераторы | потоковая обработка | последовательность, сложнее отладка |
| Потоки (streams) | обратное давление, байты | сложнее API; не запускались |
| Сторонние библиотеки (RxJS и др.) | операторы, отмена | зависимость; не запускались |

## Вопросы с собеседований

<details><summary>1. Чем итерируемый объект отличается от итератора?</summary>

Итерируемый имеет `[Symbol.iterator]()`, возвращающий итератор; итератор имеет `next()` и возвращает `{ value, done }`. Встроенные итераторы итерируемы сами по себе.

→ [Базовая теория](basics.ru.md#протокол-итерации)

</details>

<details><summary>2. Какие операции потребляют итерируемые объекты?</summary>

`for...of`, spread, деструктуризация, `Array.from`, `new Map`/`new Set`, `Promise.all` и др.

→ [Базовая теория](basics.ru.md#что-принимает-итерируемые-объекты)

</details>

<details><summary>3. Что делает <code>yield</code>?</summary>

Приостанавливает генератор, отдаёт значение вызывающему; результатом выражения станет значение из следующего `next(x)`.

→ [Продвинутая теория](advanced.ru.md#двусторонний-yield)

</details>

<details><summary>4. Подвох: что возвращает первый <code>next('x')</code>?</summary>

Значение первого `yield`; аргумент `'x'` отбрасывается.

→ [Подвох 1](#1-первый-nextx-отбрасывает-x)

</details>

<details><summary>5. Чем <code>yield*</code> отличается от <code>yield</code>?</summary>

Делегирует вложенному итерируемому объекту; результат `yield*` — `return` вложенного генератора.

→ [Продвинутая теория](advanced.ru.md#yield-делегирование)

</details>

<details><summary>6. Когда вызывается <code>return()</code> итератора?</summary>

При досрочном выходе из `for...of` (`break`, `return`, исключение) и неполной деструктуризации; не вызывается после нормального завершения и в spread.

→ [Закрытие итераторов](advanced.ru.md#закрытие-итераторов)

</details>

<details><summary>7. Подвох: что выведет <code>[...r()]</code> для <code>function* r() { yield 1; return 99 }</code>?</summary>

`[ 1 ]`: значение `return` не попадает в результат.

→ [Подвох 2](#2-forof-не-видит-return)

</details>

<details><summary>8. Как сделать бесконечную последовательность безопасной?</summary>

Ограничить `take(n)` или `break`; хелперы `take`/`drop` ленивы.

→ [Базовая теория](basics.ru.md#ленивость-и-бесконечные-последовательности)

</details>

<details><summary>9. Чем асинхронный итератор отличается от синхронного?</summary>

`next()` возвращает промис; метод называется `[Symbol.asyncIterator]`; перебирают через `for await`.

→ [Продвинутая теория](advanced.ru.md#асинхронные-итераторы-и-генераторы)

</details>

<details><summary>10. Подвох: <code>for await</code> запускает итерации параллельно?</summary>

Нет, последовательно: следующий шаг начинается после предыдущего.

→ [Подвох 8](#8-for-await-последователен)

</details>

<details><summary>11. Что произойдёт, если вызвать <code>throw</code> на завершённом генераторе?</summary>

Исключение пробрасывается наружу.

→ [Справочник](#методы-объекта-генератора)

</details>

<details><summary>12. Зачем генератору <code>try/finally</code>?</summary>

Чтобы освободить ресурс при `break`, `return()` и исключении: `finally` выполняется при закрытии.

→ [Продвинутая теория](advanced.ru.md#return-и-throw)

</details>

## Шпаргалка

```text
obj[Symbol.iterator]() → { next() → { value, done }, return?() }
function* g() { const x = yield v; return r }   // g.next(x) / return(v) / throw(e)
yield* other   // делегирование; результат — return вложенного
for (const v of it) {…}   // break → it.return()
async function* ag() { yield await p }   // for await (const v of ag())
it.map(f).filter(p).take(n).toArray()   // хелперы итераторов (проверено на Node.js 24)
```

- Первый `next(x)` игнорирует `x`; `for...of` игнорирует `return`.
- Генератор проходится один раз.
- `for await` последователен.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Массивы, `Map`, `Set`: [07 · Массивы и коллекции](../07-arrays-and-collections/expert.ru.md)
- Замыкания: [10 · Замыкания и функциональный стиль](../10-closures-and-functional/expert.ru.md)
- Асинхронность: [12 · Промисы и async/await](../12-promises-and-async-await/overview.ru.md)

## Источники

- [ECMAScript: Iteration](https://tc39.es/ecma262/#sec-iteration)
- [ECMAScript: Generator Objects](https://tc39.es/ecma262/#sec-generator-objects)
- [MDN: Iteration protocols](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Iteration_protocols)
- [MDN: Generator](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Generator)
- [Node.js: events.on](https://nodejs.org/docs/latest-v24.x/api/events.html#eventsonemitter-eventname-options)
