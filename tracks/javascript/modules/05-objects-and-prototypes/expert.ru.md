---
title: "Объекты и прототипы: экспертная теория"
lang: ru
track: javascript
kind: module
doc: expert
slug: 05-objects-and-prototypes
part: 1
tags: [object-api, own-keys-order, property-lookup, prototype-chain, internal-slots, null-prototype, descriptors]
versions: "ECMAScript 2025+, Node.js 24.15 LTS"
status: draft
updated: 2026-11-23
---

# Объекты и прототипы: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все результаты получены запуском на Node.js 24.15 (ES-модуль, строгий режим).

## Справочник

### Статические методы `Object`

| Метод | Что делает |
|---|---|
| `Object.keys(o)`, `values(o)`, `entries(o)` | массивы собственных перечисляемых строковых ключей, значений, пар |
| `Object.fromEntries(iter)` | объект из пар (в опыте работает и с `Map`) |
| `Object.assign(target, ...src)` | копирует собственные перечисляемые свойства (включая символьные); поздние перезаписывают ранние |
| `Object.hasOwn(o, k)` | есть ли собственное свойство |
| `Object.create(proto, descriptors?)` | объект с заданным прототипом (в опыте `Object.create({...}, { own: { value: 2, enumerable: true } })`) |
| `Object.getPrototypeOf(o)`, `setPrototypeOf(o, p)` | чтение и изменение прототипа |
| `Object.getOwnPropertyNames(o)` | все собственные строковые ключи, включая неперечисляемые (у строки `'ab'` — `'0'`, `'1'`, `'length'`) |
| `Object.getOwnPropertySymbols(o)` | собственные символьные ключи |
| `Object.getOwnPropertyDescriptor(o, k)`, `getOwnPropertyDescriptors(o)` | дескриптор(ы) |
| `Object.defineProperty(o, k, d)` | создать или изменить свойство с атрибутами |
| `Object.freeze`, `seal`, `preventExtensions` и `isFrozen`, `isSealed`, `isExtensible` | уровни защиты |
| `Object.groupBy(iter, fn)` | группировка; результат без прототипа |
| `Object.is(a, b)` | сравнение SameValue |

> ⚠️ Проверить: `Object.defineProperties`, `Object.entries` для символьных ключей и `Object.getOwnPropertyDescriptors` для классов взяты из документации; в опытах не запускались (дескрипторы литерала проверены).

### Порядок собственных ключей

1. Ключи — целочисленные индексы (`'1'`, `'2'`) по возрастанию.
2. Остальные строки — в порядке создания.
3. Символы — в порядке создания (не входят в `Object.keys`).

В опыте `Object.keys({ b: 1, 2: 1, a: 1, 1: 1, [Symbol('s')]: 1 })` вернул `[ '1', '2', 'b', 'a' ]`.

### Атрибуты и защита

| Операция | Результат в строгом режиме |
|---|---|
| Запись в не-`writable` свойство | `TypeError: Cannot assign to read only property 'a' of object '#<Object>'` |
| Добавление свойства к нерасширяемому объекту | `TypeError: Cannot add property b, object is not extensible` |
| Удаление свойства запечатанного объекта | `TypeError: Cannot delete property 'a' of #<Object>` |
| Деструктуризация `null` | `TypeError: Cannot destructure property 'z' of 'null' as it is null.` |
| `String(Object.create(null))` | `TypeError: Cannot convert object to primitive value` |
| `structuredClone` с функцией | `DataCloneError: f() {} could not be cloned.` |

## Внутреннее устройство

- **Внутренний слот `[[Prototype]]`.** Это ссылка на прототип или `null`. Её читают `Object.getPrototypeOf` и (устаревший) аксессор `__proto__` из `Object.prototype`. В литерале `{ __proto__: p }` задаёт прототип, а ключ `"__proto__"` из `JSON.parse` — обычное собственное свойство (в опыте).
- **Чтение `o.k` (`[[Get]]`).** Ищется собственное свойство; если его нет — запрос передаётся прототипу и так далее до `null`; если не нашлось — `undefined`. У аксессора вызывается `get` с исходным объектом как `this`.
- **Запись `o.k = v` (`[[Set]]`).** Ищется свойство по цепочке: если найден не-`writable` (в опыте замороженный прототип) или аксессор без `set` — запись отклоняется (в строгом режиме `TypeError`); если найден аксессор с `set` — вызывается он; иначе создаётся **собственное** свойство у `o` (в опыте `der.v = 2` не изменило `base.v`).
- **Перечислимость.** `Object.keys`, `entries`, `JSON.stringify`, `Object.assign` и spread учитывают только собственные перечисляемые свойства; `for...in` добавляет унаследованные перечисляемые.
- **Методы встроенных объектов не перечисляются:** `Object.keys(class.prototype)` — пустой список, дескриптор `toString` имеет `enumerable: false` (в опыте).
- **Объект и ключи.** Не-строковые ключи приводятся через `ToPropertyKey` (в опыте объект и массив стали строками), символы остаются символами.
- **Дальнейшее.** Движки хранят «форму» объекта (hidden class) и кэшируют поиск, поэтому порядок добавления свойств влияет на скорость; подробности — в модуле 19 дорожной карты.

> ⚠️ Проверить: запись в аксессор без `set` и вызов `set` при записи через прототип (по спецификации), детали внутренних слотов (`[[Extensible]]`), прокси-объекты (`Proxy` меняет `[[Get]]`/`[[Set]]`) и устройство hidden classes в V8 в опытах не проверялись.

## Подвохи

### 1. Порядок ключей не равен порядку записи

Числовые ключи всплывают вперёд и сортируются (`[ '1', '2', 'b', 'a' ]`).

### 2. `o[1]` и `o['1']` — одно свойство; объект как ключ — `'[object Object]'`

Поэтому `o[{}] = 'c'` и `o[{ other: 1 }] = 'd'` перезаписывают одно свойство.

### 3. Поверхностная копия и общие вложенные объекты

`{ ...orig }` оставляет `n` общим: `sp.n.v = 99` изменило и `orig` (в опыте).

### 4. Замороженный прототип блокирует запись у потомка

Присваивание `der2.ro = 2` падает, хотя у `der2` нет собственного `ro` (в опыте).

### 5. Изменяемое состояние в прототипе общее

`a1.list.push(1)` изменил список, видимый и через `a2` (в опыте).

### 6. Объект без прототипа не приводится к строке

`String(Object.create(null))` — `TypeError`; в шаблонной строке то же. Логирование через `console.log` безопасно.

### 7. Деструктуризация `null` и `undefined` падает

Нужно значение по умолчанию (`= {}`) или предварительная проверка.

### 8. `JSON.stringify` теряет данные

`undefined`, функции и символы пропускаются; `Date` становится строкой; `Map` — `{}`; циклическая ссылка — `TypeError` (в опыте).

### 9. `Object.freeze` поверхностный

`frozen.nested.b = 2` сработало; для глубокой защиты нужна рекурсия.

### 10. `for...in` и унаследованные ключи

`for (const k in fi)` дал `[ 'own', 'inherited' ]`, `Object.keys(fi)` — только `[ 'own' ]`.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Обычный объект как словарь | литерал, JSON | коллизии с `__proto__`/`toString`, строковые ключи, нет размера |
| `Object.create(null)` | без унаследованных ключей | нет методов `Object.prototype` |
| `Map` | любые ключи, порядок, `size` | не сериализуется в JSON напрямую |
| `{ ...o }` | кратко | поверхностная копия |
| `structuredClone` | глубокая копия, циклы, `Date`, `Map` | теряет прототипы и функции |
| `Object.freeze` | дёшево | поверхностно |
| Классы (модуль 06) | структура и инкапсуляция | синтаксис, больше кода |

## Вопросы с собеседований

<details><summary>1. Что такое прототип и цепочка прототипов?</summary>

Объект, к которому обращаются при отсутствии свойства у самого объекта; цепочка заканчивается `null`.

→ [Базовая теория](basics.ru.md#прототипы-основы)

</details>

<details><summary>2. Чем <code>in</code> отличается от <code>Object.hasOwn</code>?</summary>

`in` проверяет и цепочку прототипов, `hasOwn` — только собственные свойства.

→ [Базовая теория](basics.ru.md#доступ-проверка-и-удаление)

</details>

<details><summary>3. Ловушка: изменит ли <code>der.v = 2</code> значение у <code>base</code>?</summary>

Нет: запись создаёт собственное свойство у `der`. Прототипное значение снова видно после `delete der.v`.

→ [Базовая теория](basics.ru.md#прототипы-основы)

</details>

<details><summary>4. В каком порядке перечисляются ключи объекта?</summary>

Сначала целочисленные индексы по возрастанию, затем строки в порядке добавления, затем символы.

→ [Справочник](#порядок-собственных-ключей)

</details>

<details><summary>5. Чем поверхностная копия отличается от глубокой?</summary>

Поверхностная копирует один уровень, вложенные объекты общие; глубокая копирует рекурсивно (`structuredClone`).

→ [Продвинутая теория](advanced.ru.md#глубокое-копирование)

</details>

<details><summary>6. Чем <code>freeze</code>, <code>seal</code> и <code>preventExtensions</code> различаются?</summary>

`preventExtensions` запрещает добавление; `seal` ещё и удаление; `freeze` ещё и изменение значений. Все поверхностные.

→ [Продвинутая теория](advanced.ru.md#freeze-seal-и-preventextensions)

</details>

<details><summary>7. Ловушка: почему <code>JSON.parse(JSON.stringify(x))</code> — плохой способ копирования?</summary>

Теряются `undefined`, функции, символы; `Date` становится строкой, `Map` — `{}`; циклы дают ошибку.

→ [Подвох 8](#8-jsonstringify-теряет-данные)

</details>

<details><summary>8. Что такое дескриптор свойства?</summary>

Набор атрибутов: `value` (или `get`/`set`), `writable`, `enumerable`, `configurable`.

→ [Продвинутая теория](advanced.ru.md#дескрипторы-свойств)

</details>

<details><summary>9. Зачем нужен <code>Object.create(null)</code>?</summary>

Объект-словарь без унаследованных ключей; цена — отсутствие `toString` и прочих методов.

→ [Продвинутая теория](advanced.ru.md#объекты-без-прототипа)

</details>

<details><summary>10. Ловушка: что произойдёт, если положить массив в прототип и менять его через экземпляр?</summary>

Массив общий для всех экземпляров: изменение видно у всех.

→ [Подвох 5](#5-изменяемое-состояние-в-прототипе-общее)

</details>

<details><summary>11. Как работает деструктуризация с значением по умолчанию и остатком?</summary>

`{ a = 1, ...rest } = obj` читает `a` (умолчание при `undefined`) и собирает остальные поля в новый объект.

→ [Базовая теория](basics.ru.md#деструктуризация)

</details>

<details><summary>12. Чем <code>for...in</code> отличается от <code>Object.keys</code>?</summary>

`for...in` включает унаследованные перечисляемые ключи, `Object.keys` — только собственные.

→ [Подвох 10](#10-forin-и-унаследованные-ключи)

</details>

## Шпаргалка

```text
{ x, y, [k]: v, m() {}, get g() {} }   Object.hasOwn(o, k)   k in o
Object.keys / values / entries / fromEntries   Object.groupBy
{ ...o }   Object.assign({}, o)   structuredClone(o)
const { a = 1, ...rest } = o   const [x, , z = 0, ...r] = arr
Object.create(proto)   Object.create(null)   Object.getPrototypeOf(o)
Object.defineProperty   Object.freeze / seal / preventExtensions
```

- Ключи — строки/символы; порядок: индексы, строки, символы.
- Копия поверхностная; для глубокой — `structuredClone`.
- Запись создаёт собственное свойство; чтение идёт по цепочке.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Функции-конструкторы и `new`: [04 · Функции](../04-functions/overview.ru.md)
- Классы и безопасность прототипов — модули 06 и 20 дорожной карты [трека](../../overview.ru.md)

## Источники

- [ECMAScript: Ordinary Object Internal Methods](https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots)
- [ECMAScript: OrdinaryOwnPropertyKeys](https://tc39.es/ecma262/#sec-ordinaryownpropertykeys)
- [MDN: Object](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object)
- [MDN: Enumerability and ownership of properties](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Enumerability_and_ownership_of_properties)
- [MDN: Object.prototype.__proto__](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/proto)
