---
title: "Упражнения: LINQ"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 07-linq
part: 1
tags: [linq, groupby, groupjoin, deferred-execution, iterator, yield]
status: draft
updated: 2026-11-22
---

# Упражнения: LINQ

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами. Код проверен запуском; запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Самые длинные слова

**Задача:** из массива слов возьми три самых длинных среди слов длиннее 3 символов; при равной длине — по алфавиту.

**Ожидаемый результат:**

```text
["apple", "fig", "banana", "kiwi", "cherry", "date"] → banana,cherry,apple
```

<details><summary>Подсказка</summary>

`Where` → `OrderByDescending(длина)` → `ThenBy(слово)` → `Take(3)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: filter, order by several keys, take.
string[] words = ["apple", "fig", "banana", "kiwi", "cherry", "date"];
var top = words.Where(w => w.Length > 3)
               .OrderByDescending(w => w.Length)
               .ThenBy(w => w)
               .Take(3);
Console.WriteLine(string.Join(",", top));   // banana,cherry,apple
```

Проверено: `banana,cherry,apple`. `banana` и `cherry` — по 6 букв, при равенстве выигрывает алфавитный порядок.

</details>

## 2. 🟢 Группировка по первой букве

**Задача:** сгруппируй слова по первой букве и выведи `буква:количество` в порядке букв.

**Ожидаемый результат:**

```text
["apple", "avocado", "banana", "blueberry", "cherry"] → a:2 b:2 c:1
```

<details><summary>Подсказка</summary>

`GroupBy(w => w[0])`, затем `OrderBy(g => g.Key)` и `g.Count()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: grouping and counting.
string[] more = ["apple", "avocado", "banana", "blueberry", "cherry"];
var groups = more.GroupBy(w => w[0])
                 .OrderBy(g => g.Key)
                 .Select(g => $"{g.Key}:{g.Count()}");
Console.WriteLine(string.Join(" ", groups));   // a:2 b:2 c:1
```

Проверено: `a:2 b:2 c:1`.

</details>

## 3. 🟡 Двойное перечисление

**Задача:** источник — итератор с счётчиком `evaluations`, отдающий `1, 2, 3`. Вызови над ним `Count()` и `Sum()`, покажи число выполнений, затем исправь код так, чтобы источник выполнялся один раз.

**Ожидаемый результат:**

```text
Без материализации: 3 6 evaluations=6
С ToList():         3 6 evaluations=3
```

<details><summary>Подсказка</summary>

Каждый терминальный оператор запускает отложенный запрос заново. Сохрани результат: `ToList()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: deferred queries run once per terminal operator — materialize once.
int evaluations = 0;
IEnumerable<int> Source() { foreach (var i in new[] { 1, 2, 3 }) { evaluations++; yield return i; } }

var lazy = Source();
var c = lazy.Count(); var s = lazy.Sum();
Console.WriteLine($"{c} {s} evaluations={evaluations}");     // 3 6 evaluations=6

evaluations = 0;
var snap = Source().ToList();
var c2 = snap.Count; var s2 = snap.Sum();
Console.WriteLine($"{c2} {s2} evaluations={evaluations}");   // 3 6 evaluations=3
```

Проверено: `evaluations=6` и `evaluations=3`. Для запроса к БД двойное перечисление — это два запроса.

</details>

## 4. 🟡 Итог по клиентам

**Задача:** даны клиенты `(Id, Name)` и заказы `(CustomerId, Amount)`. Выведи сумму заказов каждого клиента, **включая клиентов без заказов** (сумма 0), по убыванию суммы, затем по имени.

**Ожидаемый результат:**

```text
клиенты Ann(1), Bob(2), Cid(3); заказы: 1→10, 1→5, 2→7
→ Ann=15, Bob=7, Cid=0
```

<details><summary>Подсказка</summary>

`Join` потеряет клиентов без заказов — нужен `GroupJoin`: для каждого клиента он отдаёт его заказы (возможно, пустую группу).

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: GroupJoin keeps customers that have no orders.
record Customer(int Id, string Name);
record Order(int CustomerId, decimal Amount);

var customers = new[] { new Customer(1, "Ann"), new Customer(2, "Bob"), new Customer(3, "Cid") };
var orders = new[] { new Order(1, 10m), new Order(1, 5m), new Order(2, 7m) };

var totals = customers
    .GroupJoin(orders, c => c.Id, o => o.CustomerId,
               (c, os) => new { c.Name, Total = os.Sum(o => o.Amount) })
    .OrderByDescending(x => x.Total)
    .ThenBy(x => x.Name);

foreach (var t in totals) Console.WriteLine($"{t.Name}={t.Total}");
```

Проверено: `Ann=15`, `Bob=7`, `Cid=0`. `Sum` по пустой группе даёт `0`.

</details>

## 5. 🔴 Свой оператор Batch

**Задача:** напиши `IEnumerable<IReadOnlyList<T>> Batch<T>(IEnumerable<T> source, int size)`, который режет последовательность на пачки по `size` (последняя может быть короче). Проверка аргументов должна срабатывать **сразу при вызове**, а не при переборе.

**Ожидаемый результат:**

```text
Batch(1..7, 3)     → 1,2,3 | 4,5,6 | 7
Batch(1..3, 0)     → ArgumentOutOfRangeException сразу (ParamName = "size")
```

<details><summary>Подсказка</summary>

Метод с `yield` откладывает даже проверки. Раздели его: внешний метод проверяет аргументы и возвращает вызов локального итератора.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: a custom LINQ operator with eager argument validation.
static IEnumerable<IReadOnlyList<T>> Batch<T>(IEnumerable<T> source, int size)
{
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);
    return Iterate(source, size);

    static IEnumerable<IReadOnlyList<T>> Iterate(IEnumerable<T> src, int size)
    {
        var buf = new List<T>(size);
        foreach (var x in src)
        {
            buf.Add(x);
            if (buf.Count == size) { yield return buf; buf = new List<T>(size); }
        }
        if (buf.Count > 0) yield return buf;
    }
}
```

Проверено: `1,2,3 | 4,5,6 | 7` и немедленное исключение для `size = 0`. В .NET уже есть `Enumerable.Chunk`; упражнение показывает приём «обёртка + итератор».

</details>
