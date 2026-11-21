---
title: "Коллекции и обобщения: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 06-collections-and-generics
part: 1
tags: [collections, generics, list, dictionary, hashset, queue, stack, array, ienumerable, collection-expressions, type-parameters]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Коллекции и обобщения: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md)

**Пререквизиты:** [02 · Типы и переменные](../02-types-and-variables/overview.ru.md), [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md).

## Зачем это нужно

Почти любая программа хранит наборы данных: список заказов, словарь настроек, очередь задач. Для этого в .NET есть **коллекции** — готовые структуры данных. Они **обобщённые** (generic): тип элементов указывается параметром (`List<int>`, `Dictionary<string, Order>`), поэтому компилятор проверяет типы, а значения не упаковываются (см. [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/advanced.ru.md#изменяемые-структуры)).

## Массивы

Массив — фиксированное количество элементов одного типа. Размер задаётся при создании и не меняется:

```csharp
int[] numbers = [1, 2, 3];          // collection expression
int[] zeros = new int[5];           // five zeros
Console.WriteLine(numbers[0]);      // 1
Console.WriteLine(numbers.Length);  // 3
```

- Индексы с нуля; выход за границы — `IndexOutOfRangeException`.
- Массив — **ссылочный тип**: `int[] b = a;` не копирует элементы (`b[0] = 9` меняет и `a[0]`); копию делает `Clone()` (поверхностная) или `[..a]`.
- Бывают многомерные (`int[2, 3]`, у них `Length` — общее число элементов: 6) и «зубчатые» (`int[][]`, массив массивов, внутренние массивы изначально `null`).

## List<T>

`List<T>` — динамический массив: можно добавлять и удалять элементы.

```csharp
var names = new List<string> { "Ann", "Bob" };
names.Add("Cid");
names.Remove("Bob");
names.Insert(0, "Zed");
Console.WriteLine(names.Count);             // 3
Console.WriteLine(string.Join(",", names)); // Zed,Ann,Cid
Console.WriteLine(names.Contains("Ann"));   // True
```

Основное: `Add`, `AddRange`, `Insert`, `Remove`, `RemoveAt`, `RemoveAll(predicate)`, `Contains`, `IndexOf`, `Sort`, `Reverse`, индексатор `list[i]`, `Count`. Доступ по индексу быстрый, поиск по значению (`Contains`, `IndexOf`) — линейный.

## Dictionary<TKey, TValue>

Словарь хранит пары «ключ — значение» и находит значение по ключу практически мгновенно:

```csharp
var ages = new Dictionary<string, int> { ["Ann"] = 30 };
ages["Bob"] = 25;                        // add or overwrite
Console.WriteLine(ages["Ann"]);          // 30

if (ages.TryGetValue("Cid", out int age))
    Console.WriteLine(age);
else
    Console.WriteLine("no such key");    // no such key

foreach (var (name, value) in ages)
    Console.WriteLine($"{name}: {value}");
```

Правила:

- ключи уникальны: `Add` с существующим ключом бросает `ArgumentException`, а присваивание через индексатор перезаписывает значение;
- обращение `ages["zzz"]` к отсутствующему ключу бросает `KeyNotFoundException`; безопаснее `TryGetValue`, `ContainsKey` или `GetValueOrDefault`;
- ключ не может быть `null` (`ArgumentNullException`);
- порядок перечисления не стоит считать гарантированным (подробнее — в [продвинутой теории](advanced.ru.md#dictionary-подробнее)).

## HashSet<T>, Queue<T>, Stack<T>

| Коллекция | Назначение | Основные операции |
|---|---|---|
| `HashSet<T>` | множество уникальных значений | `Add` (возвращает `false` для дубликата), `Contains`, `Remove`, `UnionWith`, `IntersectWith` |
| `Queue<T>` | очередь: первым пришёл — первым вышел (FIFO) | `Enqueue`, `Dequeue`, `Peek`, `TryDequeue` |
| `Stack<T>` | стек: последним пришёл — первым вышел (LIFO) | `Push`, `Pop`, `Peek` |
| `SortedSet<T>`, `SortedDictionary<K,V>` | то же, но упорядоченные по ключу | как у `HashSet`/`Dictionary` |
| `PriorityQueue<TElement, TPriority>` | очередь с приоритетом | `Enqueue(элемент, приоритет)`, `Dequeue` |
| `LinkedList<T>` | двусвязный список | `AddFirst`, `AddLast`, `Remove` узла |

Примеры: `hs.Add(2)` для уже существующего элемента вернул `False`; `PriorityQueue` с приоритетами 5, 1, 3 выдаёт элементы в порядке приоритета `1, 3, 5` (меньшее значение — раньше); `Peek` на пустой очереди бросает `InvalidOperationException`, а `TryDequeue` возвращает `false`.

## Интерфейсы коллекций

Коллекции реализуют общие интерфейсы, поэтому код можно писать для интерфейса:

| Интерфейс | Что даёт |
|---|---|
| `IEnumerable<T>` | только обход (`foreach`) |
| `IReadOnlyCollection<T>`, `IReadOnlyList<T>` | + `Count` и индекс, без изменения |
| `ICollection<T>` | + `Add`, `Remove`, `Count` |
| `IList<T>` | + доступ по индексу, `Insert` |
| `IDictionary<K,V>`, `IReadOnlyDictionary<K,V>` | словарь |
| `ISet<T>` | операции над множествами |

Правило: **принимай самый узкий интерфейс**, который нужен (`IEnumerable<T>` для чтения, `IReadOnlyList<T>`, если нужен индекс), а **возвращай** — подходящий конкретный или читаемый интерфейс.

```csharp
static int Total(IEnumerable<int> values) => values.Sum();   // works with arrays, lists, sets…
```

## Выражения коллекций

C# 12 добавил единый синтаксис создания коллекций — квадратные скобки, а оператор `..` вставляет (spread) другую последовательность:

```csharp
List<int> a = [1, 2, 3];
int[] b = [..a, 4];               // 1,2,3,4
ReadOnlySpan<int> c = [1, 2];     // no allocation on the heap
Dictionary<string, int> d = new() { ["a"] = 1 };
```

Тип результата определяется целевым типом слева: массив, `List<T>`, `Span<T>`, интерфейс (`IEnumerable<T>`, `IReadOnlyList<T>`, `ICollection<T>`).

## Обобщения (generics)

Обобщённый тип или метод описывается **параметром типа** `T`, конкретный тип подставляется при использовании. Без обобщений пришлось бы писать отдельный класс для каждого типа или хранить `object` с приведениями и упаковкой.

```csharp
public class Box<T>(T value)
{
    public T Value { get; } = value;
}

var a = new Box<int>(1);
var b = new Box<string>("s");           // a different closed type
```

Обобщённый метод:

```csharp
static T Max<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) >= 0 ? a : b;

Console.WriteLine(Max(3, 7));           // 7 — T is inferred as int
Console.WriteLine(Max("a", "b"));       // b — T is inferred as string
```

Тип `T` обычно выводится из аргументов. **Ограничение** `where T : IComparable<T>` говорит компилятору, что у `T` есть метод `CompareTo`; без ограничения вызвать его нельзя. Основные ограничения: `class`, `struct`, `new()`, конкретный базовый класс или интерфейс, `notnull` и `unmanaged` — полный список в [продвинутой теории](advanced.ru.md#ограничения-параметров-типа).

`default(T)` — значение по умолчанию параметра типа: `0` для `int`, `null` для `string`, нулевая дата для `DateTime`.

## Какую коллекцию выбрать

| Нужно | Выбор |
|---|---|
| Последовательность, добавление в конец, доступ по индексу | `List<T>` |
| Фиксированный набор элементов, максимальная скорость | массив |
| Поиск значения по ключу | `Dictionary<K,V>` |
| Проверка принадлежности, уникальные значения | `HashSet<T>` |
| Очередь (FIFO) / стек (LIFO) | `Queue<T>` / `Stack<T>` |
| Всегда отсортировано | `SortedSet<T>` / `SortedDictionary<K,V>` |
| Сначала самый важный | `PriorityQueue<T,P>` |
| Только читать | `IReadOnlyList<T>`, `ImmutableList<T>` |

## Ошибки новичка

- ❌ Изменять список внутри `foreach` по нему → ✅ `InvalidOperationException`; используй `RemoveAll` или цикл `for` с конца.
- ❌ Искать в `List<T>` по значению в цикле → ✅ `HashSet<T>` или `Dictionary`.
- ❌ Читать `dict[key]` без проверки → ✅ `TryGetValue`.
- ❌ Копировать массив присваиванием → ✅ `Clone()` или `[..a]`.
- ❌ Использовать необобщённые `ArrayList`, `Hashtable` → ✅ обобщённые аналоги: в `ArrayList` каждое число упаковывается.
- ❌ Возвращать из метода внутренний `List<T>` наружу → ✅ `IReadOnlyList<T>`, чтобы вызывающий код не менял состояние.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — ёмкость, правила изменения при обходе, ограничения, вариантность, неизменяемые коллекции
- Структуры, записи, равенство (важно для ключей): [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md)
- Запросы к коллекциям: [07 · LINQ](../07-linq/overview.ru.md)
- Классы и наследование: [04 · ООП: классы и объекты](../04-oop-basics/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — `list`, `dict`, `set`, `deque`, `heapq`, аннотации типов

## Источники

- [Коллекции](https://learn.microsoft.com/dotnet/standard/collections/)
- [Выбор коллекции](https://learn.microsoft.com/dotnet/standard/collections/selecting-a-collection-class)
- [Обобщения](https://learn.microsoft.com/dotnet/csharp/fundamentals/types/generics)
- [Выражения коллекций](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/collection-expressions)
- [List<T>](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)
- [Dictionary<TKey,TValue>](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)
