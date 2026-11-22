---
title: "Квиз: Коллекции и обобщения"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 06-collections-and-generics
part: 1
tags: [list, dictionary, hashset, generics, variance, collections]
status: draft
updated: 2026-11-22
---

# Квиз: Коллекции и обобщения

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какая коллекция подходит, чтобы быстро проверять «встречался ли элемент» и хранить только уникальные значения?

- A) `List<T>`
- B) `HashSet<T>`
- C) `Queue<T>`
- D) массив

<details><summary>Ответ</summary>

**B.** `HashSet<T>` хранит уникальные элементы, а `Add` и `Contains` работают за O(1) в среднем. В `List<T>` поиск линейный, `Queue<T>` — очередь FIFO, массив имеет фиксированный размер.
→ [Базовая теория](basics.ru.md#hashsett-queuet-stackt)

</details>

## 2. 🟢 Что безопаснее при чтении из словаря, если ключа может не быть?

- A) `dict[key]`
- B) `dict.TryGetValue(key, out var value)`
- C) `dict.Keys[key]`
- D) `(string)dict[key]`

<details><summary>Ответ</summary>

**B.** Индексатор для отсутствующего ключа бросает `KeyNotFoundException`, а `TryGetValue` возвращает `false` без исключения и без повторного поиска.
→ [Базовая теория](basics.ru.md#dictionarytkey-tvalue)

</details>

## 3. 🟢 Выберите все верные утверждения

- [ ] A) `Queue<T>` — первым вышел тот, кто первым пришёл (FIFO)
- [ ] B) `Stack<T>` — первым выходит последний добавленный (LIFO)
- [ ] C) размер массива можно менять после создания через `Add`
- [ ] D) `List<T>` хранит элементы во внутреннем массиве

<details><summary>Ответ</summary>

**A, B, D.** C неверно: размер массива фиксирован, а `Add` есть у `List<T>`, который сам заменяет внутренний массив при росте.
→ [Базовая теория](basics.ru.md#listt)

</details>

## 4. 🟢 Что выведет код?

```csharp
var list = new List<int>();
for (int i = 0; i < 20; i++) list.Add(i);
Console.WriteLine(list.Capacity);
```

- A) `20`
- B) `32`
- C) `16`
- D) `64`

<details><summary>Ответ</summary>

**B.** Ёмкость растёт удвоением (4, 8, 16, 32): список из 20 элементов имел ёмкость 32 (проверено). Если размер известен заранее — `new List<int>(20)` и повторных выделений не будет.
→ [Продвинутая теория](advanced.ru.md#ёмкость-списка)

</details>

## 5. 🟡 Что произойдёт?

```csharp
var nums = new List<int> { 1, 2, 3, 4, 5, 6 };
foreach (var n in nums)
    if (n % 2 == 0) nums.Remove(n);
```

- A) удалятся все чётные
- B) `InvalidOperationException`
- C) `IndexOutOfRangeException`
- D) бесконечный цикл

<details><summary>Ответ</summary>

**B.** Изменение `List<T>` во время `foreach` бросает `InvalidOperationException`: перечислитель отслеживает версию коллекции (проверено). Безопасно: `nums.RemoveAll(n => n % 2 == 0)` или обход копии.
→ [Продвинутая теория](advanced.ru.md#изменение-коллекции-при-обходе)

</details>

## 6. 🟡 Что из перечисленного скомпилируется и сработает без ошибки?

- A) `List<object> l = new List<string>();`
- B) `IEnumerable<object> e = new List<string> { "a" };`
- C) `object[] a = new string[1]; a[0] = 1;`
- D) `List<string> l = new List<object>();`

<details><summary>Ответ</summary>

**B.** `IEnumerable<out T>` ковариантен: последовательность строк — последовательность объектов (читать безопасно). `List<T>` инвариантен, поэтому A и D не компилируются. C компилируется, но во время выполнения даёт `ArrayTypeMismatchException`: массивы ковариантны, и это известная дыра.
→ [Продвинутая теория](advanced.ru.md#вариантность)

</details>

## 7. 🟡 Что вернёт `BinarySearch`?

```csharp
var a = new List<int> { 1, 3, 5 };
Console.WriteLine($"{a.BinarySearch(3)} {a.BinarySearch(4)}");
```

- A) `1 -1`
- B) `1 -3`
- C) `1 2`
- D) `0 -2`

<details><summary>Ответ</summary>

**B.** Найденный элемент — его индекс (`1`). Для отсутствующего возвращается побитовое отрицание позиции вставки: `~2 = -3`. Проверка `result >= 0` означает «найден»; позиция вставки — `~result`.
→ [Экспертная теория](expert.ru.md#результат-binarysearch)

</details>

## 8. 🟡 Выберите все верные утверждения

- [ ] A) `ReadOnlyCollection<T>` — представление над исходным списком: добавления в исходный список видны через него
- [ ] B) `ImmutableList<T>` после `ToImmutableList()` не меняется, когда меняют исходный список
- [ ] C) приведение `IReadOnlyList<int>` обратно к `List<int>` всегда даёт `null`
- [ ] D) порядок перечисления `Dictionary` гарантирован порядком вставки

<details><summary>Ответ</summary>

**A, B.** C неверно: интерфейс только скрывает методы, `as List<int>` вернул не `null` (проверено). D неверно: порядок — побочный эффект реализации, после `Remove` и вставки он изменился (`3,1,2` → `3,9,2`).
→ [Продвинутая теория](advanced.ru.md#неизменяемые-и-только-для-чтения)

</details>

## 9. 🟡 Открытый вопрос: как выбрать коллекцию для задачи «часто искать по ключу, изредка перебирать по порядку добавления»?

<details><summary>Ключевые пункты хорошего ответа</summary>

- поиск по ключу — `Dictionary<TKey, TValue>` (O(1) в среднем);
- порядок вставки `Dictionary` не гарантирован — если он нужен, нужна отдельная структура (`List<TKey>`, `LinkedList<T>`) или `OrderedDictionary<TKey, TValue>`;
- ключ должен быть неизменяемым и иметь корректные `Equals`/`GetHashCode`;
- для частых вставок в середину и обходов подойдёт `LinkedList<T>`/`SortedDictionary`, если нужен ещё и порядок по ключу.

</details>

## 10. 🔴 Что выведет код?

```csharp
class Counter<T> { public static int Count; public Counter() { Count++; } }

new Counter<int>(); new Counter<int>(); new Counter<string>();
Console.WriteLine($"{Counter<int>.Count} {Counter<string>.Count}");
```

- A) `3 3`
- B) `2 1`
- C) `1 2`
- D) `2 2`

<details><summary>Ответ</summary>

**B.** У каждого закрытого типа (`Counter<int>`, `Counter<string>`) свои статические поля, потому что runtime создаёт отдельный тип для каждой комбинации аргументов (проверено).
→ [Экспертная теория](expert.ru.md#статические-поля-обобщённого-типа)

</details>

## 11. 🔴 Что верно про сортировку?

- A) `Enumerable.OrderBy` устойчива: равные ключи сохраняют исходный порядок
- B) `List<T>.Sort` и `Array.Sort` гарантируют устойчивость
- C) устойчивость сортировки влияет только на скорость
- D) `OrderBy` сортирует исходный список на месте

<details><summary>Ответ</summary>

**A.** `OrderBy` сохранил порядок равных ключей (`c,b,a` для `("b",1),("a",1),("c",0)` при сортировке по числу). `Sort` не даёт такой гарантии и меняет коллекцию на месте, а `OrderBy` возвращает новую последовательность.
→ [Экспертная теория](expert.ru.md#устойчивость-сортировки)

</details>
