---
title: "Коллекции и обобщения: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 06-collections-and-generics
part: 1
tags: [list-capacity, dictionary, modification-during-enumeration, generic-constraints, variance, covariance, immutable-collections, frozen-collections, icomparable, sorting]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Коллекции и обобщения: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md)

## Ёмкость списка

`List<T>` хранит элементы в массиве и при заполнении создаёт больший. Свойство `Capacity` показывает размер внутреннего массива, `Count` — число элементов. Проверено: пустой список имеет ёмкость 0, а при добавлении 20 элементов она менялась `0 → 4 → 8 → 16 → 32` — удваивается.

```csharp
var l = new List<int>(100);      // reserve capacity: no re-allocations while adding up to 100
l.Add(1); l.Add(2); l.Add(3);
l.TrimExcess();                  // Capacity becomes 3
```

Если размер известен заранее, задайте ёмкость в конструкторе (то же — `EnsureCapacity`): это избавляет от лишних выделений и копирования. Добавление в конец — амортизированно `O(1)`, вставка и удаление в середине — `O(n)`, потому что элементы сдвигаются.

## Изменение коллекции при обходе

Перечислитель запоминает «версию» коллекции. Если коллекция изменилась, следующий `MoveNext` бросает исключение. Проверено:

| Операция во время `foreach` | Результат |
|---|---|
| `List<T>.Add` | `InvalidOperationException`: «Collection was modified; enumeration operation may not execute.» |
| `List<T>.Remove` | `InvalidOperationException` |
| `Dictionary.Add` / `d[новый ключ] = …` | `InvalidOperationException` |
| `Dictionary.Remove` | работает (проверено на .NET 11) |
| `Dictionary[существующий ключ] = …` | работает |

Правильные способы изменить коллекцию в цикле:

```csharp
var l3 = new List<int> { 1, 2, 3, 4 };
l3.RemoveAll(x => x % 2 == 0);                // 1,3 — the best way
for (int i = l3.Count - 1; i >= 0; i--)        // from the end: indexes do not shift
    l3.RemoveAt(i);
```

Либо переберите **копию** (`foreach (var x in list.ToList())`) или соберите изменения отдельно и примените после цикла.

## Dictionary подробнее

```csharp
var dict = new Dictionary<string, int> { ["a"] = 1 };
dict["a"] = 5;                                      // overwrites
dict.TryAdd("a", 9);                                // false, nothing changes
int value = dict.GetValueOrDefault("q", -1);        // -1
var ci = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["Key"] = 1 };
Console.WriteLine(ci.ContainsKey("KEY"));           // True
```

- **Компаратор ключей.** По умолчанию — `EqualityComparer<TKey>.Default`; для строк его можно заменить, например, на нечувствительный к регистру.
- **Порядок.** В проверке без удалений ключи шли в порядке вставки (`3,1,2`), но после `Remove(1)` и `d[9] = …` новый ключ занял освободившееся место: `3,9,2`. Документация не гарантирует порядок, на него нельзя полагаться: нужен порядок — `SortedDictionary` или `List` пар.
- **Ключи** должны быть неизменяемыми и корректно реализовывать `Equals`/`GetHashCode` ([05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/advanced.ru.md#равенство-и-gethashcode)).
- **Сложность**: поиск, вставка, удаление — в среднем `O(1)`.
- **`CollectionsMarshal.GetValueRefOrAddDefault`** возвращает ссылку на значение и позволяет обновить его с одним поиском вместо двух (`TryGetValue` + запись): `ref int c = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, "k", out _); c++;`.

## Упорядоченные коллекции, очереди, списки

- `SortedDictionary<K,V>` и `SortedSet<T>` хранят ключи в порядке сортировки (в основе — сбалансированное дерево): операции `O(log n)`. Для `SortedSet` есть `Min`, `Max` и `GetViewBetween` (проверено: для `{1,3,5}` диапазон `2..5` дал `3,5`).
- `PriorityQueue<TElement, TPriority>` выдаёт элемент с **наименьшим** приоритетом. На порядок элементов с равным приоритетом полагаться нельзя.
- `LinkedList<T>` даёт `O(1)` вставку и удаление при наличии узла, но обход медленнее и память расходуется на ссылки. На практике `List<T>` почти всегда быстрее.
- `Queue<T>` и `Stack<T>` — на массивах; `TryDequeue`/`TryPop`/`TryPeek` не бросают исключений на пустой коллекции.

## Сортировка и сравнение

Сортировка использует `IComparable<T>` (естественный порядок) или переданный `IComparer<T>`/делегат:

```csharp
var words = new[] { "bb", "a", "ccc" };
Array.Sort(words, (x, y) => x.Length.CompareTo(y.Length));    // a,bb,ccc

var sorted = people.OrderBy(p => p.Age).ThenBy(p => p.Name).ToList();   // LINQ, stable
```

Важное отличие: **`OrderBy` (LINQ) устойчив** — элементы с равным ключом сохраняют исходный порядок (проверено: `("b",1),("a",1),("c",0)` → `c,b,a`), а `List<T>.Sort` и `Array.Sort` используют интроспективную сортировку и **не гарантируют устойчивость**. `BinarySearch` работает только для отсортированных данных; если элемента нет, возвращает побитовое отрицание позиции вставки (`~index`: для `{1,3,5}` и `4` вернул `-3`, то есть вставить на позицию `2`).

## Массивы и ковариантность

Массивы ссылочных типов **ковариантны**: `object[] objs = new string[] { "a" };` компилируется, но запись неподходящего типа бросает исключение во время выполнения:

```csharp
object[] objs = new string[] { "a" };
objs[0] = 1;       // ArrayTypeMismatchException
```

Это историческая особенность: безопасность проверяется при записи. У `List<T>` такой проблемы нет, потому что обобщённые классы инвариантны (см. ниже).

## Ограничения параметров типа

| Ограничение | Требует |
|---|---|
| `where T : class` / `class?` | ссылочный тип (необнуляемый / допускает `null`) |
| `where T : struct` | значимый необнуляемый тип |
| `where T : new()` | публичный конструктор без параметров |
| `where T : Base` | наследник класса `Base` |
| `where T : IFoo` | реализует интерфейс |
| `where T : U` | наследник другого параметра типа |
| `where T : notnull` | необнуляемый тип |
| `where T : unmanaged` | значимый тип без ссылок (можно брать размер и указатель) |
| `where T : allows ref struct` | (C# 13) разрешает подставлять `ref struct` |

Ограничения комбинируются: `where T : class, IComparable<T>, new()` (порядок: `class`/`struct` первым, `new()` последним). Ограничение даёт доступ к членам: проверено, `where T : new()` позволяет написать `new T()`, `where T : unmanaged` — `Unsafe.SizeOf<T>()` (4 для `int`, 8 для `long`), `where T : IComparable<T>` — вызов `CompareTo`.

## Вариантность

**Ковариантность** (`out T`) позволяет использовать более производный тип там, где ожидается базовый; **контравариантность** (`in T`) — наоборот:

```csharp
IEnumerable<object> eo = new List<string> { "a" };   // IEnumerable<out T>: covariant, OK
Func<object> fo = () => "s";                         // Func<out TResult>: covariant

Action<object> ao = o => { };
Action<string> asx = ao;                             // Action<in T>: contravariant, OK
```

- `IEnumerable<out T>`, `IReadOnlyList<out T>`, `Func<out TResult>` — ковариантны: из них только читают.
- `Action<in T>`, `IComparer<in T>`, `IEqualityComparer<in T>` — контравариантны: в них только передают.
- `List<T>`, `IList<T>`, `ICollection<T>`, `Dictionary<K,V>` — **инвариантны**: `List<string>` нельзя привести к `List<object>` (проверено: `is List<object>` — `False`), иначе в список строк можно было бы добавить число.

Вариантность работает только для **ссылочных** типов и только для интерфейсов и делегатов; для значимых типов (`IEnumerable<int>` → `IEnumerable<object>`) не работает.

## Неизменяемые и только для чтения

| Тип | Что это |
|---|---|
| `IReadOnlyList<T>` | **интерфейс**: вызывающий код не может изменять, но объект могут менять другие (и привести обратно: `iro as List<int>` — не `null`) |
| `ReadOnlyCollection<T>`, `list.AsReadOnly()` | обёртка-**представление**: изменения исходного списка видны (проверено: после `src.Add(3)` в обёртке `Count == 3`) |
| `ImmutableList<T>`, `ImmutableArray<T>`, `ImmutableDictionary` (`System.Collections.Immutable`) | действительно неизменяемые: операции возвращают новую коллекцию (`Add` дал 4 элемента при 3 в исходном) |
| `FrozenDictionary<K,V>`, `FrozenSet<T>` | (.NET 8) создаются один раз и оптимизируются под быстрое чтение; изменений нет |

Для публичного API чаще всего достаточно `IReadOnlyList<T>`. Неизменяемость нужна, когда коллекцию читают из нескольких потоков ([18 · Многопоточность](../18-concurrency/advanced.ru.md)) или хранят как значение записи. `Frozen*` подходят для справочников, которые строятся один раз при старте и читаются очень часто.

## Лучшие практики

- Задавай ёмкость, когда размер известен.
- Принимай `IEnumerable<T>`/`IReadOnlyList<T>`, возвращай `IReadOnlyList<T>` или конкретный тип; не отдавай наружу изменяемые внутренние коллекции.
- Для поиска по значению — `HashSet`/`Dictionary`, а не `List.Contains` в цикле.
- Не полагайся на порядок элементов в `Dictionary` и `HashSet`.
- Не используй изменяемые объекты как ключи.
- Ограничивай параметры типа минимально необходимым.
- Не бери `ArrayList`/`Hashtable` в новом коде.

## Частые ошибки

- ❌ Изменять коллекцию в `foreach` → ✅ `RemoveAll`, обход копии, цикл с конца.
- ❌ Двойной поиск в словаре (`ContainsKey` + индексатор) → ✅ `TryGetValue`.
- ❌ Ожидать устойчивости от `List.Sort` → ✅ `OrderBy`.
- ❌ Сохранять ссылку на внутренний список и менять его снаружи → ✅ защитная копия или `IReadOnlyList<T>`.
- ❌ Ожидать, что `List<Derived>` можно передать как `List<Base>` → ✅ `IEnumerable<Base>` или `IReadOnlyList<Base>`.
- ❌ `foreach` по `Dictionary` с добавлением ключей → ✅ собрать изменения и применить после обхода.

## Сравнение с другими треками

| | C# | Python |
|---|---|---|
| Динамический массив | `List<T>` | `list` |
| Словарь | `Dictionary<K,V>` (порядок не гарантирован) | `dict` (порядок вставки гарантирован) |
| Множество | `HashSet<T>` | `set` |
| Очередь | `Queue<T>`, `PriorityQueue` | `collections.deque`, `heapq` |
| Типы элементов | проверяются компилятором (generics) | аннотации типов, проверяет mypy |
| Неизменяемые | `ImmutableList<T>`, `FrozenSet<T>` | `tuple`, `frozenset` |

## Связанные материалы

- Раньше: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md) — устройство `List` и `Dictionary`, обобщения в runtime, справочник, вопросы с собеседований
- Равенство, `GetHashCode`, упаковка: [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/advanced.ru.md)
- Запросы к коллекциям: [07 · LINQ](../07-linq/overview.ru.md)
- Коллекции для многопоточного кода: [18 · Многопоточность](../18-concurrency/advanced.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md)

## Источники

- [Выбор класса коллекции](https://learn.microsoft.com/dotnet/standard/collections/selecting-a-collection-class)
- [Когда использовать обобщённые коллекции](https://learn.microsoft.com/dotnet/standard/collections/when-to-use-generic-collections)
- [Ограничения параметров типа](https://learn.microsoft.com/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)
- [Ковариантность и контравариантность](https://learn.microsoft.com/dotnet/standard/generics/covariance-and-contravariance)
- [Неизменяемые коллекции](https://learn.microsoft.com/dotnet/standard/collections/thread-safe/immutable-collections)
- [Frozen-коллекции](https://learn.microsoft.com/dotnet/api/system.collections.frozen)
- [Array.Sort: примечания](https://learn.microsoft.com/dotnet/api/system.array.sort)
