---
title: "LINQ: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 07-linq
part: 1
tags: [linq, query-syntax, method-syntax, deferred-execution, where, select, orderby, groupby, lambda, ienumerable]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# LINQ: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md), [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md).

## Зачем это нужно

LINQ (Language Integrated Query) — единый способ запрашивать данные: фильтровать, преобразовывать, сортировать, группировать и агрегировать **любые последовательности** — массивы, списки, словари, XML и базы данных ([15 · EF Core](../15-ef-core/overview.ru.md)). Вместо циклов с временными списками вы описываете, **что** нужно получить:

```csharp
var people = new[] { new P("Ann", "IT", 30), new P("Bob", "HR", 25), new P("Cid", "IT", 35) };

var names = people
    .Where(p => p.Age > 26)
    .OrderByDescending(p => p.Age)
    .Select(p => p.Name)
    .ToList();                                  // Cid, Ann

record P(string Name, string Dept, int Age);
```

LINQ работает с любым `IEnumerable<T>`; методы находятся в пространстве имён `System.Linq` (оно подключено неявными `using` в новых проектах).

## Лямбда-выражения

В запросах передают **лямбды** — короткие анонимные функции `параметры => выражение`:

```csharp
Func<int, bool> isEven = x => x % 2 == 0;
Console.WriteLine(isEven(4));    // True
```

`p => p.Age > 26` читается «для каждого `p` — вернуть, больше ли возраст 26». Подробнее — в [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md).

## Основные операторы

| Задача | Оператор | Пример |
|---|---|---|
| Отфильтровать | `Where` | `people.Where(p => p.Dept == "IT")` |
| Преобразовать | `Select` | `people.Select(p => p.Name)` |
| Отсортировать | `OrderBy`, `OrderByDescending`, `ThenBy` | `people.OrderBy(p => p.Age).ThenBy(p => p.Name)` |
| Взять / пропустить | `Take`, `Skip`, `TakeWhile`, `SkipWhile` | `list.Skip(10).Take(5)` |
| Первый элемент | `First`, `FirstOrDefault`, `Single`, `Last` | `people.FirstOrDefault(p => p.Age > 30)` |
| Проверить | `Any`, `All`, `Contains` | `people.Any(p => p.Dept == "HR")` |
| Посчитать | `Count`, `Sum`, `Min`, `Max`, `Average` | `people.Sum(p => p.Age)` |
| Убрать повторы | `Distinct`, `DistinctBy` | `people.Select(p => p.Dept).Distinct()` |
| Сгруппировать | `GroupBy` | `people.GroupBy(p => p.Dept)` |
| Объединить | `Concat`, `Union`, `Intersect`, `Except`, `Zip` | `a.Union(b)` |
| Соединить | `Join`, `GroupJoin` | см. [продвинутую теорию](advanced.ru.md#группировка-и-соединения) |
| Развернуть | `SelectMany` | `orders.SelectMany(o => o.Items)` |
| В коллекцию | `ToList`, `ToArray`, `ToDictionary`, `ToHashSet` | `query.ToList()` |

Операторы **составляются в цепочку**: результат одного — вход следующего.

### Выбор одного элемента

| Метод | Если элемента нет | Если их больше одного |
|---|---|---|
| `First()` | `InvalidOperationException` | первый |
| `FirstOrDefault()` | значение по умолчанию (`null`/`0`) | первый |
| `Single()` | `InvalidOperationException` | `InvalidOperationException` |
| `SingleOrDefault()` | значение по умолчанию | `InvalidOperationException` |

Проверено: `First()` и `Single()` на пустой последовательности бросают «Sequence contains no elements», а `Single()` на двух элементах — «Sequence contains more than one element». С .NET 6 у `FirstOrDefault` есть перегрузка с собственным значением по умолчанию: `empty.FirstOrDefault(-1)` вернуло `-1`.

## Отложенное выполнение

Главное свойство LINQ: **запрос — это описание, оно не выполняется, пока результат не потребуется**.

```csharp
var nums = new List<int> { 1, 2, 3 };
var q = nums.Where(x => x > 1);      // nothing has run yet
nums.Add(4);
Console.WriteLine(string.Join(",", q));   // 2,3,4 — the query sees the later Add
```

Запрос выполняется при **перечислении**: `foreach`, `ToList()`, `Count()`, `First()`, `Sum()` и т. д. Каждое перечисление выполняет запрос заново. Чтобы зафиксировать результат, материализуйте его: `ToList()` или `ToArray()`:

```csharp
var snap = nums.Where(x => x > 1).ToList();   // a snapshot: 2,3,4
nums.Add(5);
Console.WriteLine(snap.Count);                // 3 — the list does not change
Console.WriteLine(q.Count());                 // 4 — the query is evaluated again
```

Подробнее о том, какие операторы отложенные и какие немедленные, — в [продвинутой теории](advanced.ru.md#отложенное-и-немедленное-выполнение).

## Синтаксис запросов

Кроме методов, есть похожий на SQL **синтаксис запросов** (query syntax). Оба варианта равноценны: компилятор переводит синтаксис запросов в вызовы методов.

```csharp
var result =
    from p in people
    where p.Age > 26
    orderby p.Age descending
    select p.Name.ToUpper();          // CID, ANN
```

Основные ключевые слова: `from`, `where`, `select`, `orderby`, `group … by`, `join … on … equals`, `let`, `into`. Часть операторов (`Take`, `Skip`, `Distinct`, `Count`, …) в запросном синтаксисе не имеет ключевого слова и вызывается как метод: `(from p in people select p.Age).Max()`. Обычно используют синтаксис методов; запросный удобен для соединений и нескольких `from`.

## Группировка

```csharp
var byDept = people.GroupBy(p => p.Dept);
foreach (var g in byDept)
    Console.WriteLine($"{g.Key}: {g.Count()} — {string.Join("/", g.Select(p => p.Name))}");
// IT: 2 — Ann/Cid
// HR: 1 — Bob
```

`GroupBy` возвращает последовательность групп: у каждой есть `Key` и элементы. Порядок групп — по первому появлению ключа (`IT`, затем `HR`).

## Материализация в коллекции

```csharp
List<string> list = people.Select(p => p.Name).ToList();
Dictionary<string, int> ages = people.ToDictionary(p => p.Name, p => p.Age);
HashSet<string> depts = people.Select(p => p.Dept).ToHashSet();
```

`ToDictionary` бросает `ArgumentException`, если ключ повторяется (проверено: «An item with the same key has already been added»). Для нескольких значений на ключ используйте `ToLookup`.

## Ошибки новичка

- ❌ Считать, что запрос выполнен в момент объявления → ✅ он выполняется при перечислении; используйте `ToList()` для снимка.
- ❌ Перечислять один запрос несколько раз → ✅ каждый проход заново выполняет запрос и его побочные эффекты: сохраните в список.
- ❌ `First()` на возможно пустой последовательности → ✅ `FirstOrDefault()` и проверка.
- ❌ `Count() > 0` ради проверки на наличие → ✅ `Any()`: он останавливается на первом элементе.
- ❌ `Select` с побочными эффектами (запись, вызов API) → ✅ запросы только читают и преобразуют, действия — в `foreach`.
- ❌ `ToList()` слишком рано в цепочке → ✅ материализуйте в конце.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — стриминг и буферизация, замыкания, соединения, итераторы `yield`, `IQueryable`
- Коллекции и их интерфейсы: [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md)
- Лямбды и делегаты: [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md)
- LINQ к базе данных: [15 · EF Core](../15-ef-core/basics.ru.md#запросы-linq-превращается-в-sql)
- То же в других треках: [Python](../../../python/overview.ru.md) — генераторы списков, `itertools`, `map`/`filter`

## Источники

- [LINQ: обзор](https://learn.microsoft.com/dotnet/csharp/linq/)
- [Синтаксис запросов и синтаксис методов](https://learn.microsoft.com/dotnet/csharp/linq/get-started/query-expression-basics)
- [Стандартные операторы запросов](https://learn.microsoft.com/dotnet/csharp/linq/standard-query-operators/)
- [Enumerable](https://learn.microsoft.com/dotnet/api/system.linq.enumerable)
