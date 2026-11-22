---
title: "Коллекции и обобщения"
lang: ru
track: csharp
kind: module
doc: overview
slug: 06-collections-and-generics
part: 1
levels: [1, 2, 3]
tags: [collections, generics, list, dictionary, hashset, queue, stack, array, ienumerable, collection-expressions, type-parameters]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Коллекции и обобщения

🌐 **Русский** · [English](overview.en.md)

## Коротко

Коллекции — готовые структуры данных .NET: массив, `List<T>`, `Dictionary<K,V>`, `HashSet<T>`, очереди, стеки, упорядоченные и неизменяемые варианты. Все они обобщённые: тип элемента указывается параметром, компилятор проверяет типы, а значения не упаковываются. Выбор коллекции и понимание её устройства определяют и скорость, и корректность программы.

## Ключевые идеи

- **Массив** фиксированного размера; **`List<T>`** — динамический массив с ёмкостью, которая удваивается; **`Dictionary`** и **`HashSet`** — хеш-таблицы с поиском за `O(1)` в среднем.
- **Изменение коллекции во время `foreach`** даёт `InvalidOperationException`: используй `RemoveAll`, обход копии или цикл с конца.
- **Порядок в `Dictionary` не гарантирован**: после удаления новый ключ может занять освободившееся место.
- **Интерфейсы** (`IEnumerable<T>`, `IReadOnlyList<T>`, …): принимай самый узкий, не отдавай наружу изменяемые внутренние коллекции.
- **Обобщения овеществлены:** `List<int>` и `List<string>` — разные типы со своими статическими полями; значимые типы не упаковываются; ограничения `where T : …` дают доступ к членам.
- **Вариантность:** `IEnumerable<out T>` ковариантен, `Action<in T>` контравариантен, `List<T>` инвариантен; массивы ковариантны небезопасно.
- **Только для чтения ≠ неизменяемо:** `IReadOnlyList` и `ReadOnlyCollection` — представления, `Immutable*` и `Frozen*` — настоящая защита.

## Минимальный пример

```csharp
var ages = new Dictionary<string, int> { ["Ann"] = 30 };
ages["Bob"] = 25;

if (ages.TryGetValue("Cid", out int age))
    Console.WriteLine(age);
else
    Console.WriteLine("no such key");    // no such key

static T Max<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) >= 0 ? a : b;
Console.WriteLine(Max(3, 7));            // 7
```

## Когда применять, а когда нет

- ✅ `List<T>` по умолчанию для последовательностей, `Dictionary` для поиска по ключу, `HashSet` для уникальности.
- ✅ `IReadOnlyList<T>` в сигнатурах публичного API.
- ✅ `Immutable*`/`Frozen*` для общих данных между потоками и справочников.
- ❌ `ArrayList`, `Hashtable` — упаковка и отсутствие типизации.
- ❌ `List.Contains` в цикле по большим данным — `HashSet`.
- ❌ Изменяемые ключи в словарях и множествах.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | массивы, `List`, `Dictionary`, `HashSet`, очереди и стеки, интерфейсы коллекций, выражения коллекций, основы обобщений, выбор коллекции |
| [2. Продвинутая теория](advanced.ru.md) | ёмкость, изменение при обходе, словарь подробнее, сортировка, ограничения параметров типа, вариантность, неизменяемые и frozen-коллекции |
| [3. Экспертная теория](expert.ru.md) | устройство `List` и `Dictionary`, обобщения в runtime, выражения коллекций, справочники сложности и вариантности, подвохи, 17 вопросов с собеседований |
| 4. Самопроверка | ещё не создан — `/learn csharp 06-collections-and-generics level 4` |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp 06-collections-and-generics level 5` |
| 6. Тесты | ещё не создан — `/learn csharp 06-collections-and-generics level 6` |

## Связанные материалы

- Пререквизиты: [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md), [04 · ООП: классы и объекты](../04-oop-basics/overview.ru.md)
- Дальше: [07 · LINQ](../07-linq/overview.ru.md)
- Измерение выделений: [12 · Память и GC](../12-memory-and-gc/overview.ru.md)
- Потокобезопасные коллекции: [18 · Многопоточность](../18-concurrency/overview.ru.md)
- Выражения коллекций `[..]`: [11 · Современный C#](../11-modern-csharp/overview.ru.md)
- Представления на память без копирования: [12a · Span и Memory](../12a-span-and-memory/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — `list`, `dict`, `set`, `heapq`
