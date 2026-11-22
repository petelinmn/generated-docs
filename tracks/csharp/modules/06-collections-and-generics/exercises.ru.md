---
title: "Упражнения: Коллекции и обобщения"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 06-collections-and-generics
part: 1
tags: [dictionary, hashset, list, generics, constraints, lru-cache]
status: draft
updated: 2026-11-22
---

# Упражнения: Коллекции и обобщения

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами. Код проверен запуском; запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Частота слов

**Задача:** напиши `Dictionary<string, int> WordFrequency(string text)`: считает слова без учёта регистра; разделители — пробел и знаки `, . ! ?`.

**Ожидаемый результат:**

```text
"The cat and the hat. The end!" → The=3, and=1, cat=1, end=1, hat=1
```

(ключ хранит написание первого вхождения — `The`.)

<details><summary>Подсказка</summary>

`new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)` и `freq[w] = freq.GetValueOrDefault(w) + 1`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: counting with a dictionary and a case-insensitive comparer.
static Dictionary<string, int> WordFrequency(string text)
{
    var freq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    foreach (var w in text.Split([' ', ',', '.', '!', '?'], StringSplitOptions.RemoveEmptyEntries))
        freq[w] = freq.GetValueOrDefault(w) + 1;
    return freq;
}
```

Проверено: после сортировки по убыванию частоты и по ключу вывод совпадает с таблицей.

</details>

## 2. 🟢 Уникальные элементы с сохранением порядка

**Задача:** напиши обобщённый `List<T> Distinct<T>(IEnumerable<T> src)`, который убирает повторы и сохраняет порядок первого вхождения. Без LINQ.

**Ожидаемый результат:**

```text
Distinct([3, 1, 3, 2, 1]) → 3,1,2
```

<details><summary>Подсказка</summary>

`HashSet<T>.Add` возвращает `true`, только если элемента ещё не было.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: de-duplicate while preserving order.
static List<T> Distinct<T>(IEnumerable<T> src) where T : notnull
{
    var seen = new HashSet<T>();
    var result = new List<T>();
    foreach (var x in src)
        if (seen.Add(x)) result.Add(x);
    return result;
}
```

Проверено: `3,1,2`. Множество даёт O(1) проверку, список — порядок.

</details>

## 3. 🟡 Безопасное удаление при обходе

**Задача:** в списке `1..6` нужно удалить чётные числа. Покажи, что `foreach` + `Remove` бросает исключение, и сделай удаление правильно.

**Ожидаемый результат:**

```text
foreach + Remove → InvalidOperationException
RemoveAll        → удалено 3, остались 1,3,5
```

<details><summary>Подсказка</summary>

У `List<T>` есть метод `RemoveAll(Predicate<T>)`, возвращающий число удалённых элементов.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: never remove from a list inside foreach; use RemoveAll.
var nums = new List<int> { 1, 2, 3, 4, 5, 6 };
try { foreach (var n in nums) if (n % 2 == 0) nums.Remove(n); }
catch (InvalidOperationException) { Console.WriteLine("InvalidOperationException"); }

nums = [1, 2, 3, 4, 5, 6];
int removed = nums.RemoveAll(n => n % 2 == 0);
Console.WriteLine($"{removed} {string.Join(",", nums)}");   // 3 1,3,5
```

Проверено: исключение и затем `3 1,3,5`.

</details>

## 4. 🟡 Обобщённый максимум

**Задача:** напиши `T Max<T>(IEnumerable<T> items) where T : IComparable<T>`. Для пустой последовательности бросай `InvalidOperationException("Sequence is empty.")`.

**Ожидаемый результат:**

```text
Max([3, 9, 2])              → 9
Max(["pear", "apple", "zebra"]) → zebra
Max([])                     → InvalidOperationException
```

<details><summary>Подсказка</summary>

Ограничение `IComparable<T>` даёт `CompareTo`. Возьми перечислитель вручную, чтобы проверить пустоту.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: a generic method with a constraint.
static T Max<T>(IEnumerable<T> items) where T : IComparable<T>
{
    using var e = items.GetEnumerator();
    if (!e.MoveNext()) throw new InvalidOperationException("Sequence is empty.");
    var max = e.Current;
    while (e.MoveNext())
        if (e.Current.CompareTo(max) > 0) max = e.Current;
    return max;
}
```

Проверено: `9`, `zebra`, исключение с сообщением `Sequence is empty.`.

</details>

## 5. 🔴 LRU-кэш

**Задача:** напиши `LruCache<TKey, TValue>(int capacity)` с методами `Put` и `TryGet`. При переполнении вытесняется самый давно использованный элемент; `TryGet` и повторный `Put` считаются использованием. Все операции — O(1).

**Ожидаемый результат:**

```text
capacity = 2: Put(a), Put(b), TryGet(a) → True 1
Put(c) вытесняет b → TryGet(b) False, TryGet(a) True, TryGet(c) True, Count = 2
```

<details><summary>Подсказка</summary>

`Dictionary<TKey, LinkedListNode<…>>` для поиска и `LinkedList<…>` для порядка использования: голова — самый свежий, хвост — кандидат на вытеснение.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: LRU cache — a dictionary for O(1) lookup plus a linked list for recency order.
class LruCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _map = new();
    private readonly LinkedList<(TKey Key, TValue Value)> _order = new();   // front = most recent

    public LruCache(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        _capacity = capacity;
    }

    public int Count => _map.Count;

    public bool TryGet(TKey key, out TValue value)
    {
        if (_map.TryGetValue(key, out var node))
        {
            _order.Remove(node);
            _order.AddFirst(node);
            value = node.Value.Value;
            return true;
        }
        value = default!;
        return false;
    }

    public void Put(TKey key, TValue value)
    {
        if (_map.TryGetValue(key, out var existing)) { _order.Remove(existing); _map.Remove(key); }
        else if (_map.Count == _capacity)
        {
            var last = _order.Last!;
            _order.RemoveLast();
            _map.Remove(last.Value.Key);
        }
        _map[key] = _order.AddFirst((key, value));
    }
}
```

Проверено: вывод совпадает с таблицей. Реализация не потокобезопасна; для многопоточного кода нужна синхронизация ([18 · Многопоточность](../18-concurrency/overview.ru.md)).

</details>
