---
title: "LINQ: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 07-linq
part: 1
tags: [linq, deferred-execution, streaming, buffering, closures, group-join, selectmany, iterators, yield, iqueryable, expression-trees]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# LINQ: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Отложенное и немедленное выполнение

| Тип | Операторы | Когда выполняются |
|---|---|---|
| **Отложенные, стриминговые** | `Where`, `Select`, `Take`, `Skip`, `SelectMany`, `Concat`, `Zip`, `TakeWhile`, `Append`, `Distinct`, `Union` | при переборе, элемент за элементом (`Distinct` и `Union` помнят уже виденные значения) |
| **Отложенные, буферизующие** | `OrderBy`, `GroupBy`, `Reverse` — весь источник; `Join`, `GroupJoin`, `Except`, `Intersect` — всю **вторую** последовательность | при первом `MoveNext`, но сначала читают последовательность целиком |
| **Немедленные** | `ToList`, `ToArray`, `ToDictionary`, `Count`, `Sum`, `Min`, `Max`, `First`, `Any`, `All`, `Aggregate` | сразу при вызове |

Разницу видно по порядку вызовов. Источник и два стриминговых оператора обрабатывают элементы по одному (проверено по журналу):

```csharp
var s = Src("a", 3)
    .Where(x => { log.Add($"w{x}"); return true; })
    .Select(x => { log.Add($"s{x}"); return x; });

foreach (var x in s.Take(2)) log.Add($"got{x}");
// a1 w1 s1 got1 a2 w2 s2 got2   — the third element is never read
```

Каждый элемент проходит всю цепочку, прежде чем читается следующий, а `Take(2)` останавливает чтение после второго. Буферизующий `OrderBy`, наоборот, сначала читает всё:

```csharp
foreach (var x in Src("a", 3).OrderBy(x => -x).Take(1)) log.Add($"got{x}");
// a1 a2 a3 got3   — all three are read before the first result
```

Проверено и для других операторов: `Distinct().Take(1)` прочитал один элемент источника; `Join` прочитал первый элемент внешней последовательности и **всю** внутреннюю (`o1 i1 i2 i3`); `GroupBy` и `Reverse` — весь источник; `Except` — всю вторую последовательность (`x1 x2`), затем стримит первую.

Следствия: для бесконечных источников и больших файлов используйте стриминговые операторы; `OrderBy(...).First()` читает всю последовательность, а `MinBy`/`MaxBy` — тоже один проход без сортировки и обычно предпочтительнее.

## Многократное перечисление

Каждый перебор запроса выполняет его заново, вместе с побочными эффектами и стоимостью источника:

```csharp
var src3 = Src("e", 2).Select(x => x * 2);
_ = src3.Count();
_ = src3.Sum();
// log: e1 e2 e1 e2 — the source was enumerated twice
```

Если результат нужен несколько раз, материализуйте: `var list = query.ToList();`. Для `IEnumerable<T>` из API, которое может быть «ленивым» (запрос к базе, чтение файла), многократное перечисление — частая причина лишних запросов (анализаторы предупреждают об этом: CA1851).

## Замыкания: переменная читается при выполнении

Запрос захватывает **переменную**, а не её значение на момент создания запроса:

```csharp
int limit = 2;
var q2 = nums.Where(x => x > limit);
limit = 4;
Console.WriteLine(string.Join(",", q2));   // uses limit == 4
```

Второй пример — цикл. В `for` одна переменная `i` на весь цикл, поэтому замыкания видят её итоговое значение, а в `foreach` на каждой итерации создаётся новая переменная:

```csharp
var fs = new List<Func<int>>();
for (int i = 0; i < 3; i++) fs.Add(() => i);          // 3,3,3

var fs2 = new List<Func<int>>();
foreach (var i in new[] { 0, 1, 2 }) fs2.Add(() => i);  // 0,1,2
```

Проверено: `for` дал `3,3,3`, `foreach` — `0,1,2`. Решение для `for`: скопировать значение в локальную переменную внутри тела цикла.

## Ускорения и проверки на пустоту

`Count()`, `Any()`, `ToList()` и `Last()` используют знания об источнике. Проверено на собственной коллекции, которая считает обращения:

| Вызов | Для `ICollection<T>` |
|---|---|
| `Count()` | берёт свойство `Count`, перечисление **не** выполняется |
| `Any()` | берёт `Count`, перечисление не выполняется |
| `ToList()` | копирует через `CopyTo`, а не обходит элемент за элементом |
| `Last()` | для не-`IList<T>` перечисляет всю последовательность |

Для итератора, который не коллекция, `Count()` обязан пройти всю последовательность (проверено: 3 шага), а `Any()` останавливается после первого (1 шаг). Поэтому для проверки «есть ли элементы» всегда `Any()`. Метод `TryGetNonEnumeratedCount(out n)` вернёт число, только если его можно узнать без обхода: `True` для списка, `False` для итератора.

## Пустые последовательности и граничные случаи

| Вызов на пустой `int[]` | Результат |
|---|---|
| `First()`, `Single()`, `Last()` | `InvalidOperationException`: «Sequence contains no elements» |
| `Max()`, `Min()`, `Average()` (для `int`) | `InvalidOperationException` |
| `Max()` для `int?` и для ссылочных типов (`string`) | `null` без исключения |
| `Sum()`, `Count()` | `0` |
| `FirstOrDefault()` / `FirstOrDefault(-1)` | `0` / `-1` |
| `DefaultIfEmpty(7).First()` | `7` |
| `ElementAt(5)` вне диапазона | `ArgumentOutOfRangeException`; `ElementAtOrDefault(5)` — `0` |

`Sum` для `int` **проверяет переполнение**: `new[] { int.MaxValue, 1 }.Sum()` бросает `OverflowException`. Для больших сумм приводите к `long`: `Sum(x => (long)x)` дало `2147483648`.

## Группировка и соединения

```csharp
var lk = people.ToLookup(p => p.Dept);
Console.WriteLine(lk["QA"].Count());    // 0 — a missing key gives an empty sequence, not an exception

var joined = people.Join(depts, p => p.Dept, d => d.Code, (p, d) => $"{p.Name}-{d.Title}");
// Ann-Tech, Cid-Tech — only matching pairs (inner join)
```

- `GroupBy` — отложенный, группы идут в порядке первого появления ключа; `ToLookup` — немедленный, поиск по ключу.
- `Join` — внутреннее соединение (только совпавшие). Для **левого соединения**: `GroupJoin` + `SelectMany`/`DefaultIfEmpty`, а в современных версиях — оператор `LeftJoin` (на .NET 11 проверено: `new[]{1,2}.LeftJoin(…)` вернул `1x` и `2-` для отсутствующего).

```csharp
var left = depts.GroupJoin(people, d => d.Code, p => p.Dept, (d, ps) => $"{d.Code}:{ps.Count()}");
// IT:2, QA:0 — every department, even without people
```

- `SelectMany` разворачивает вложенные последовательности: `new[]{"ab","cd"}.SelectMany(s => s)` → `a,b,c,d`; с селектором результата получаются декартовы пары (`11,21,12,22`).
- `Aggregate` — свёртка: `new[]{1,2,3,4}.Aggregate((a,b) => a*b)` → `24`; с начальным значением — `Aggregate("", (acc, s) => acc + s)`.

## Операторы последних версий

Все перечисленные работают на .NET 11 (проверено запуском):

| Оператор | Пример и результат |
|---|---|
| `Chunk(n)` | `Range(1,5).Chunk(2)` → `[1,2] [3,4] [5]` |
| `MinBy`, `MaxBy` | `MinBy(s => s.Length)` → самый короткий элемент |
| `DistinctBy`, `UnionBy`, `ExceptBy`, `IntersectBy` | `DistinctBy(s => s.Length)` |
| `Order()`, `OrderDescending()` | сортировка без селектора ключа |
| `CountBy(key)` | `"hello".CountBy(c => c)` → `h=1, e=1, l=2, o=1` |
| `AggregateBy(key, seed, func)` | агрегат по группам без `GroupBy` |
| `Index()` | `["a","b"].Index()` → `(0,a), (1,b)` |
| `Zip` из трёх последовательностей | кортежи `(First, Second, Third)` |
| `Take(1..3)`, `Take(^2..)` | диапазоны; `ElementAt(^1)` — последний |
| `LeftJoin`, `RightJoin` | соединения с сохранением левой/правой стороны |

## Итераторы и yield

Метод с `yield return` — **итератор**: он возвращает `IEnumerable<T>`, но код в нём не выполняется до перебора, а выполнение приостанавливается на каждом `yield return`:

```csharp
IEnumerable<int> GenFinally()
{
    try { yield return 1; yield return 2; }
    finally { log.Add("finally"); }
}

foreach (var x in GenFinally()) { log.Add($"x{x}"); break; }
// x1 finally — breaking out of the loop disposes the enumerator, and the finally block runs
```

- `yield break` завершает последовательность.
- Итератор лениво порождает значения, поэтому подходит для бесконечных последовательностей и для потоков данных.
- В итераторе нельзя использовать параметры `ref`, `in`, `out` (ошибка CS1623) и `yield` внутри `try` с `catch` (CS1626); внутри `try`/`finally` — можно (проверено).

### Проверка аргументов: немедленная и отложенная

Методы LINQ проверяют аргументы **сразу**: `Enumerable.Where<int>(null, …)` бросает `ArgumentNullException` в момент вызова (проверено). А собственный итератор с `yield` выполняет код лишь при первом `MoveNext`, поэтому исключение появится позже:

```csharp
static IEnumerable<T> MyWhere<T>(IEnumerable<T> src, Func<T, bool> p)
{
    ArgumentNullException.ThrowIfNull(src);      // runs only on the first MoveNext
    foreach (var x in src) if (p(x)) yield return x;
}

var lazyBad = MyWhere<int>(null!, x => true);    // no exception yet
foreach (var _ in lazyBad) { }                   // ArgumentNullException here
```

Чтобы проверять сразу, разделите метод: внешний (с проверкой) возвращает результат внутреннего итератора. Ошибки в лямбдах тоже отложены: `Select(x => 1 / x)` создаётся без ошибки, а `DivideByZeroException` появляется при `ToList()`.

## IEnumerable и IQueryable

| | `IEnumerable<T>` | `IQueryable<T>` |
|---|---|---|
| Где выполняется | в памяти, в вашем процессе | у провайдера (база данных и т. д.) |
| Параметр лямбды | `Func<T, bool>` (делегат) | `Expression<Func<T, bool>>` (дерево выражений) |
| Что делает провайдер | вызывает делегат | разбирает дерево и переводит, например, в SQL |

Лямбда превращается в **дерево выражений**, если параметр имеет тип `Expression<…>`:

```csharp
Expression<Func<int, bool>> ex = x => x > 2;
Func<int, bool> fn = x => x > 2;
Console.WriteLine(ex);                         // x => (x > 2)
Console.WriteLine(ex.Body.NodeType);           // GreaterThan
Console.WriteLine(ex.Compile()(3));            // True
```

`AsQueryable()` над коллекцией в памяти даёт `EnumerableQuery`: он исполняет дерево как обычный LINQ to Objects (проверено: `qry.Provider` — `EnumerableQuery`1`). Настоящий `IQueryable` — `DbSet` из [15 · EF Core](../15-ef-core/advanced.ru.md#что-ef-умеет-перевести-в-sql): там непереводимый метод в `Where` приводит к исключению, а ранний `AsEnumerable()` переносит фильтрацию в память. В выражениях нельзя использовать литерал кортежа (ошибка CS8143) и лямбды с телом-блоком (ошибка CS0834).

## Лучшие практики

- Пиши цепочки читаемо: по оператору на строке; сложные условия выноси в именованные методы.
- Фильтруй (`Where`) **до** сортировки и проекции: меньше данных в буфере.
- `Any()` вместо `Count() > 0`; `FirstOrDefault` с проверкой вместо `First` на возможно пустых данных.
- Материализуй (`ToList`/`ToArray`), если результат нужен более одного раза.
- Не делай побочных эффектов в `Where`/`Select`.
- В «горячем» коде и при больших объёмах сравнивай с обычным циклом: LINQ создаёт делегаты и итераторы.
- Для проверки принадлежности используй `HashSet`, а не `Contains` на списке внутри запроса.

## Частые ошибки

- ❌ Многократный перебор «ленивого» источника → ✅ материализация.
- ❌ `OrderBy(...).First()` ради минимума → ✅ `MinBy`.
- ❌ Замыкание на переменную цикла `for` → ✅ копия в локальную переменную.
- ❌ `Sum` для `int` на больших данных → ✅ `Sum(x => (long)x)`.
- ❌ `Count()` на `IEnumerable` из базы данных в цикле → ✅ `Any()`, один запрос.
- ❌ Ожидать немедленной проверки аргументов в своих итераторах → ✅ внешний метод с проверкой и внутренний итератор.

## Сравнение с другими треками

| | C# LINQ | Python |
|---|---|---|
| Фильтр и проекция | `Where`, `Select` | генератор списка `[f(x) for x in xs if p(x)]`, `filter`, `map` |
| Ленивость | отложенное выполнение по умолчанию | генераторные выражения лениво, списковые — нет |
| Группировка | `GroupBy` | `itertools.groupby` (нужна сортировка) |
| Генераторы | `yield return` | `yield` |
| Запросы к БД | `IQueryable`, дерево выражений | SQLAlchemy `select()` |

## Связанные материалы

- Раньше: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md) — трансляция запросного синтаксиса, конечный автомат итератора, справочник операторов, вопросы с собеседований
- Коллекции, `Count` и `Sort`: [06 · Коллекции и обобщения](../06-collections-and-generics/advanced.ru.md)
- LINQ над БД, трансляция в SQL, клиентская оценка: [15 · EF Core](../15-ef-core/advanced.ru.md)
- Замыкания и делегаты: [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md)
- Итераторы и циклы: [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/expert.ru.md)

## Источники

- [Отложенное выполнение и ленивая оценка](https://learn.microsoft.com/dotnet/standard/linq/deferred-execution-lazy-evaluation)
- [Стандартные операторы запросов: классификация по способу выполнения](https://learn.microsoft.com/dotnet/csharp/linq/standard-query-operators/#classification-of-standard-query-operators-by-manner-of-execution)
- [Операции соединения](https://learn.microsoft.com/dotnet/csharp/linq/standard-query-operators/join-operations)
- [yield](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/yield)
- [Деревья выражений](https://learn.microsoft.com/dotnet/csharp/advanced-topics/expression-trees/)
- [Enumerable.Sum](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.sum)
- [Анализатор CA1851](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1851)
