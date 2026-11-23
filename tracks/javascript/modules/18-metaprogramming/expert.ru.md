---
title: "Метапрограммирование: экспертный уровень"
lang: ru
track: javascript
kind: module
doc: expert
slug: 18-metaprogramming
part: 3
tags: [proxy-traps, internal-methods, reflect-api, well-known-symbols, property-descriptors, weakref-gc, tagged-template-cache]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Метапрограммирование: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все результаты получены запуском на Node.js 24.15.

## Справочник

### Ловушки Proxy и соответствующие операции

| Ловушка | Какую операцию перехватывает |
|---|---|
| `get(t, k, receiver)` | чтение свойства `p.k`, `p[k]` |
| `set(t, k, v, receiver)` | запись `p.k = v` |
| `has(t, k)` | оператор `k in p` |
| `deleteProperty(t, k)` | `delete p.k` |
| `ownKeys(t)` | `Object.keys`, `Reflect.ownKeys`, `for...in`, `JSON.stringify` |
| `getOwnPropertyDescriptor(t, k)` | `Object.getOwnPropertyDescriptor`, а также `Object.keys` и spread: для каждого ключа |
| `defineProperty(t, k, desc)` | `Object.defineProperty` |
| `getPrototypeOf(t)` | `Object.getPrototypeOf`, `instanceof` |
| `setPrototypeOf(t, proto)` | `Object.setPrototypeOf` |
| `isExtensible(t)`, `preventExtensions(t)` | `Object.isExtensible`, `Object.preventExtensions` |
| `apply(t, thisArg, args)` | вызов функции-цели |
| `construct(t, args, newTarget)` | `new p(...)` |

В опыте при наличии ловушек: `Object.keys(p)` вызвал `ownKeys`, затем `getOwnPropertyDescriptor`; `p instanceof Object` — `getPrototypeOf`; `Object.freeze(p)` — `preventExtensions`, `ownKeys`, `getOwnPropertyDescriptor`, `defineProperty`; spread `{ ...p }` — `ownKeys`, `getOwnPropertyDescriptor`, `get`.

### Reflect

Для каждой ловушки есть одноимённая функция `Reflect.*` (кроме `Reflect.ownKeys`, которая реализует «все ключи»). Отличие от старых API: `Reflect.defineProperty`, `set`, `deleteProperty` возвращают `boolean`, а не бросают исключение (в опыте `Reflect.set` на замороженном объекте — `false`).

### Известные символы (список)

`Symbol.iterator`, `asyncIterator`, `hasInstance`, `isConcatSpreadable`, `match`, `matchAll`, `replace`, `search`, `species`, `split`, `toPrimitive`, `toStringTag`, `unscopables`; плюс `Symbol.dispose`, `Symbol.asyncDispose` для явного управления ресурсами ([модуль 14](../14-errors-and-debugging/advanced.ru.md#освобождение-ресурсов-finally-и-using)). `Symbol.dispose` в Node.js 24.15 — символ (в опыте `symbol`).

### Дескрипторы свойств

| Способ создания | `writable` | `enumerable` | `configurable` |
|---|---|---|---|
| Присваивание `o.x = 1`, литерал `{ x: 1 }` | `true` | `true` | `true` |
| `Object.defineProperty(o, 'x', { value: 1 })` | `false` | `false` | `false` |
| Свойство после `Object.freeze` | `false` | без изменений | `false` |

Дескриптор замороженного свойства: `{ value: 1, writable: false, enumerable: true, configurable: false }` ([модуль 10](../10-closures-and-functional/expert.ru.md#справочник)).

## Внутреннее устройство

- **Внутренние методы объекта.** У любого объекта в спецификации есть набор «внутренних методов» (`[[Get]]`, `[[Set]]`, `[[HasProperty]]`, `[[Delete]]`, `[[OwnPropertyKeys]]`, …). Обычные объекты реализуют их стандартно; **экзотические** объекты (массивы, строки, аргументы, `Proxy`) переопределяют часть. Прокси — объект, у которого каждый внутренний метод вызывает соответствующую ловушку или, если её нет, операцию над целью.
- **Инварианты.** После выполнения ловушки движок сверяет результат с состоянием цели: нельзя сообщить, что неконфигурируемого свойства нет, подменить значение неизменяемого свойства и т. п. (в опыте `TypeError: 'get' on proxy: property 'x' is a read-only and non-configurable…`).
- **Внутренние слоты.** У `Map`, `Set`, `Date`, `Promise` и объектов с `#`-полями есть слоты, привязанные к конкретному объекту; прокси их не имеет, поэтому методы с `this` = прокси падают (в опыте `Method Map.prototype.set called on incompatible receiver`, `Cannot read private member #x`).
- **Теговые шаблоны.** Массив `strings` создаётся **один раз на место** в исходном коде и кэшируется: повторный вызов из того же места получает тот же объект (в опыте `same strings object each call true`), он заморожен (`Object.isFrozen` — `true`) и имеет `raw`. Это позволяет кэшировать результат по `strings`.
- **Символы.** Символ — примитив с уникальной идентичностью; ключ-символ хранится отдельно от строковых ключей (порядок `Reflect.ownKeys`: целые, строки, символы). Символы из реестра (`Symbol.for`) нельзя использовать как ключи слабых коллекций (в опыте `TypeError`).
- **Слабые ссылки.** `WeakRef` и слабые коллекции не удерживают значение от сборки; когда именно оно будет собрано — решает движок. В пределах одной «задачи» `WeakRef.deref()` возвращает один и тот же результат (по спецификации объект остаётся живым до конца текущей задачи).

> ⚠️ Проверить: список известных символов взят из спецификации; удержание цели `WeakRef` в пределах задачи и сроки сборки мусора в опытах не проверялись.

## Подвохи

### 1. Ловушка `set` без `return true`

В строгом режиме — `TypeError: 'set' on proxy: trap returned falsish for property 'x'` (в опыте); в нестрогом запись молча теряется.

### 2. Прокси ≠ цель

`new Proxy(target, {}) === target` — `false`; `Array.isArray(proxy)` для массива — `true`.

### 3. Приватные поля и `Map` через прокси

`TypeError` из-за слотов: привязывайте методы к цели.

### 4. Инварианты

Нельзя вернуть другое значение для замороженного свойства.

### 5. `Reflect.get(t, k, t)` против `Reflect.get(t, k, r)`

С `receiver` = прокси геттер видит `this` = прокси, с целью — цель (в опыте `child` и `base`).

### 6. Символ нельзя склеить со строкой

`s + ''` — `TypeError: Cannot convert a Symbol value to a string`.

### 7. Ключ-символ скрыт

`JSON.stringify` и `Object.keys` его не видят, `Reflect.ownKeys` видит.

### 8. `defineProperty` создаёт скрытое неизменяемое свойство

Без флагов все `false` (в опыте).

### 9. Теговый шаблон: один `strings` на место

Кэшируйте по `strings`, но не изменяйте массив: он заморожен.

### 10. `WeakRef.deref()` может вернуть `undefined`

Всегда проверяйте результат.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| `Proxy` | универсальные перехваты, прозрачность | медленнее, ломает слоты и приватные поля, сложная отладка |
| Геттеры и сеттеры | просто, быстро | по одному свойству |
| `Object.defineProperty` | управление флагами | многословно |
| Явные методы (`set()`/`get()`) | предсказуемо | неудобный API |
| Классы с `#` | настоящая инкапсуляция | нельзя «подсмотреть» извне |
| Генерация кода (`new Function`) | скорость | небезопасно, трудно отлаживать |
| Декораторы и компиляторы (TypeScript, Babel) | метаданные на этапе сборки | инструменты сборки; не запускались |

## Вопросы с собеседований

<details><summary>1. Что такое <code>Proxy</code> и какие ловушки вы знаете?</summary>

Объект-обёртка, перехватывающий операции над целью; ловушки `get`, `set`, `has`, `deleteProperty`, `ownKeys`, `apply`, `construct`, `getPrototypeOf` и др.

→ [Базовая теория](basics.ru.md#proxy-перехват-операций)

</details>

<details><summary>2. Зачем нужен <code>Reflect</code>?</summary>

Выполняет стандартное действие внутри ловушки и возвращает `boolean` вместо исключения; поддерживает `receiver`.

→ [Базовая теория](basics.ru.md#reflect)

</details>

<details><summary>3. Подвох: что будет, если ловушка <code>set</code> вернёт <code>false</code>?</summary>

В строгом режиме — `TypeError`, в нестрогом запись молча игнорируется.

→ [Подвох 1](#1-ловушка-set-без-return-true)

</details>

<details><summary>4. Подвох: почему <code>new Proxy(new Map(), {}).set('a', 1)</code> падает?</summary>

У `Map` внутренний слот, у прокси его нет: `this` = прокси недопустим. Нужно привязать метод к цели.

→ [Подвох 3](#3-приватные-поля-и-map-через-прокси)

</details>

<details><summary>5. Чем символ отличается от строки как ключа?</summary>

Уникален, не виден `Object.keys`/`JSON.stringify`/`for...in`; виден `Object.getOwnPropertySymbols` и `Reflect.ownKeys`.

→ [Базовая теория](basics.ru.md#символы)

</details>

<details><summary>6. Что такое <code>Symbol.toPrimitive</code>?</summary>

Метод с аргументом `hint` (`'string'`, `'number'`, `'default'`), управляющий приведением объекта к примитиву.

→ [Продвинутая теория](advanced.ru.md#известные-символы-well-known-symbols)

</details>

<details><summary>7. Что получает тег-функция теговой шаблонной строки?</summary>

Массив строк `strings` (с `strings.raw`) и значения подстановок отдельными аргументами.

→ [Базовая теория](basics.ru.md#теговые-шаблонные-строки)

</details>

<details><summary>8. Подвох: как <code>defineProperty</code> по умолчанию настраивает свойство?</summary>

`writable`, `enumerable`, `configurable` — все `false`.

→ [Подвох 8](#8-defineproperty-создаёт-скрытое-неизменяемое-свойство)

</details>

<details><summary>9. Что такое инварианты прокси?</summary>

Проверки движка: ловушка не может нарушить гарантии цели (например, подменить значение неконфигурируемого свойства).

→ [Продвинутая теория](advanced.ru.md#ограничения-и-инварианты-прокси)

</details>

<details><summary>10. Зачем нужны <code>WeakMap</code> и <code>WeakRef</code> и в чём риск?</summary>

Хранить данные об объектах, не мешая их сборке; сроки сборки недетерминированы, ключ — только объект или локальный символ.

→ [Продвинутая теория](advanced.ru.md#слабые-ссылки)

</details>

<details><summary>11. Что делает <code>Proxy.revocable</code>?</summary>

Создаёт прокси, доступ к которому можно отозвать вызовом `revoke()`.

→ [Продвинутая теория](advanced.ru.md#отключаемый-прокси-и-receiver)

</details>

<details><summary>12. Почему не стоит использовать <code>eval</code> и <code>Function</code> на строках из ввода?</summary>

Это выполнение произвольного кода: риск внедрения.

→ [Продвинутая теория](advanced.ru.md#функции-newtarget-eval)

</details>

## Шпаргалка

```text
new Proxy(target, { get(t, k, r), set(t, k, v, r) → true, has, deleteProperty, ownKeys, apply, construct })
Reflect.get/set/has/deleteProperty/ownKeys/defineProperty/apply/construct(…)  // set/define/delete → boolean
Symbol('d') уникален; Symbol.for('k') — из реестра; Reflect.ownKeys видит символы
[Symbol.iterator] / [Symbol.toPrimitive](hint) / [Symbol.toStringTag] / static [Symbol.hasInstance]
tag`a${1}b` → tag(strings, ...vals); String.raw`\n`
Object.defineProperty(o, k, { value, writable, enumerable, configurable }) — по умолчанию всё false
WeakMap/WeakSet (ключ — объект), WeakRef.deref() → объект | undefined
```

- Прокси соблюдает инварианты и ломает слоты.
- `set` возвращает `true`.
- Не полагайтесь на сборку мусора.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Объекты и прототипы: [05 · Объекты и прототипы](../05-objects-and-prototypes/expert.ru.md)
- Классы и приватные поля: [06 · Классы](../06-classes/expert.ru.md)
- Итерация: [11 · Итераторы и генераторы](../11-iterators-and-generators/expert.ru.md)

## Источники

- [ECMAScript: Proxy Objects](https://tc39.es/ecma262/#sec-proxy-objects)
- [ECMAScript: Reflect](https://tc39.es/ecma262/#sec-reflect-object)
- [ECMAScript: Well-Known Symbols](https://tc39.es/ecma262/#sec-well-known-symbols)
- [MDN: Meta programming](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Meta_programming)
- [MDN: Object.defineProperty](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/defineProperty)
