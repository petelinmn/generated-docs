---
title: "LINQ: экспертная теория"
lang: ru
track: csharp
kind: module
doc: expert
slug: 07-linq
part: 1
tags: [linq, query-translation, iterator-state-machine, expression-trees, iqueryable, enumerable-shortcuts, async-linq, deferred-execution, interview-questions]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# LINQ: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

### Операторы — методы расширения

LINQ to Objects — статический класс `Enumerable` с методами расширения над `IEnumerable<T>`; LINQ для провайдеров — класс `Queryable` над `IQueryable<T>`. Запрос `people.Where(p => p.Age > 26)` — обычный вызов `Enumerable.Where(people, лямбда)`. Поэтому любой тип, реализующий `IEnumerable<T>`, получает LINQ, а свой оператор — это ещё один метод расширения.

### Запросный синтаксис — «сахар»

Компилятор **переписывает** запросные выражения в вызовы методов ещё до проверки типов:

| Запросное выражение | Метод |
|---|---|
| `from x in xs where … select …` | `xs.Where(…).Select(…)` |
| `orderby a, b descending` | `OrderBy(a).ThenByDescending(b)` |
| `group x by k` | `GroupBy(x => k)` |
| `join y in ys on a equals b` | `Join(ys, …)` |
| `join … into g` | `GroupJoin(…)` |
| несколько `from` | `SelectMany(…)` |
| `let v = …` | `Select` с промежуточным анонимным типом, который несёт и `x`, и `v` |
| `… into g` (продолжение) | вложенный запрос над результатом |

Из этого следует, что запросный синтаксис работает с **любым** типом, у которого есть нужные методы (включая ваши), и что у каждого ключевого слова ровно столько же стоимости, сколько у метода.

### Итератор — конечный автомат

Метод с `yield return` компилятор превращает в **генерируемый класс**. Проверено рефлексией: у метода `Gen()` тип называется `<<<Main>$>g__Gen|0_68>d`, он реализует и `IEnumerable<int>`, и `IEnumerator<int>`, а его поля — `<>1__state` (номер текущей точки продолжения), `<>2__current` (последнее значение) и `<>l__initialThreadId`. Локальные переменные и параметры тоже становятся полями этого класса.

- `MoveNext()` — большой `switch` по `<>1__state`: выполняет код до очередного `yield return`, запоминает точку и возвращает `true`.
- Тело итератора **не выполняется** при вызове метода — только создаётся объект; поэтому проверка аргументов в нём откладывается (см. [продвинутую теорию](advanced.ru.md#проверка-аргументов-немедленная-и-отложенная)).
- Один класс служит и перечислением, и перечислителем: при первом `GetEnumerator()` в том же потоке возвращается он же (для этого и нужен `<>l__initialThreadId`), а при повторном перечислении создаётся новый экземпляр.
- Блок `finally` в итераторе выполняется при `Dispose()` перечислителя: `break` из `foreach` его запускает (проверено: `x1 finally`).

Те же идеи лежат в основе `async`-методов (10 · Async/await).

### Оптимизации в Enumerable

Реализация LINQ специализирует работу по типу источника. Проверено на собственной коллекции, которая считает обращения:

- `Count()` — по свойству `Count`, без перечисления;
- `Any()` — по `Count`, без перечисления;
- `ToList()` — копирование через `CopyTo`;
- `TryGetNonEnumeratedCount` — раскрывает эту возможность: `True` для списка, `False` для итератора.

Результат `Select(...).Count()` над списком **выполнил** проекции (побочный эффект сработал дважды): LINQ не пропускает вызовы делегатов, потому что у них может быть наблюдаемое поведение. Не стоит строить код на том, что часть вызовов будет пропущена: внутренние оптимизации — деталь реализации, которая менялась между версиями.

### Деревья выражений и IQueryable

`Queryable.Where` принимает `Expression<Func<T,bool>>`, а не делегат (проверено: тип параметра — `Expression`1`). Компилятор строит **дерево данных**: для `x => x > 2` — `Expression.Lambda` с узлом `GreaterThan`, параметром `x` и константой `2`; `ToString()` дерева даёт `x => (x > 2)`. Провайдер (EF Core, LINQ to XML…) обходит дерево и переводит в свой язык (SQL). `ex.Compile()` превращает дерево в делегат.

- `AsQueryable()` над коллекцией в памяти создаёт `EnumerableQuery<T>`: его `Expression` — `System.Int32[].Where(x => (x > 1))`, а исполняет он дерево, компилируя его обратно в делегаты (проверено).
- В дерево нельзя превратить лямбду с телом-блоком (CS0834) и литерал кортежа (CS8143), нельзя вызвать метод с `ref`/`out` и ряд других конструкций.
- Непереводимый для провайдера вызов даёт исключение или переход к клиентской оценке — подробности в [15 · EF Core](../15-ef-core/advanced.ru.md#что-ef-умеет-перевести-в-sql).

### Асинхронный LINQ

На .NET 11 асинхронные операторы над `IAsyncEnumerable<T>` входят в состав платформы — сборка `System.Linq.AsyncEnumerable` (проверено: `ToAsyncEnumerable()` и `ToListAsync()` доступны без сторонних пакетов, `typeof(AsyncEnumerable).Assembly.GetName().Name` — `System.Linq.AsyncEnumerable`). Они потребляются через `await foreach` и методы вида `await seq.ToListAsync()`. Подробнее об `await foreach` — в 10 · Async/await.

## Справочник

> ⚠️ Проверить: версии, в которых появились отдельные операторы (`Chunk`, `MinBy`/`MaxBy` — .NET 6; `Order` — .NET 7; `CountBy`, `AggregateBy`, `Index` — .NET 9; `LeftJoin`/`RightJoin` — .NET 10), сверены не по официальной документации, а по памяти. На .NET 11 все они работают (проверено запуском).

### Операторы по категориям

| Категория | Операторы |
|---|---|
| Фильтрация | `Where`, `OfType`, `Take`, `Skip`, `TakeWhile`, `SkipWhile`, `TakeLast`, `SkipLast`, `Take(Range)` |
| Проекция | `Select`, `SelectMany`, `Zip`, `Index`, `Cast` |
| Сортировка | `OrderBy`, `OrderByDescending`, `ThenBy`, `ThenByDescending`, `Order`, `OrderDescending`, `Reverse` |
| Группировка | `GroupBy`, `ToLookup`, `CountBy`, `AggregateBy`, `Chunk` |
| Соединения | `Join`, `GroupJoin`, `LeftJoin`, `RightJoin`, `SelectMany` |
| Множества | `Distinct`, `DistinctBy`, `Union`, `UnionBy`, `Intersect`, `IntersectBy`, `Except`, `ExceptBy`, `Concat`, `Append`, `Prepend` |
| Элементы | `First`, `FirstOrDefault`, `Last`, `LastOrDefault`, `Single`, `SingleOrDefault`, `ElementAt`, `ElementAtOrDefault`, `DefaultIfEmpty` |
| Кванторы | `Any`, `All`, `Contains`, `SequenceEqual` |
| Агрегаты | `Count`, `LongCount`, `Sum`, `Min`, `Max`, `MinBy`, `MaxBy`, `Average`, `Aggregate`, `TryGetNonEnumeratedCount` |
| Преобразование | `ToList`, `ToArray`, `ToDictionary`, `ToHashSet`, `ToLookup`, `AsEnumerable`, `AsQueryable`, `ToAsyncEnumerable` |
| Генерация | `Enumerable.Range`, `Repeat`, `Empty` |

### Способ выполнения

| Группа | Операторы |
|---|---|
| Отложенные стриминговые | `Where`, `Select`, `Take`, `Skip`, `SelectMany`, `Concat`, `Zip`, `Distinct`, `Union` |
| Отложенные буферизующие | `OrderBy`, `GroupBy`, `Reverse` (весь источник); `Join`, `GroupJoin`, `Except`, `Intersect` (вторая последовательность) |
| Немедленные | `ToList`, `ToArray`, `ToDictionary`, `ToLookup`, `Count`, `Sum`, `Min`, `Max`, `First`, `Single`, `Any`, `All`, `Aggregate`, `SequenceEqual` |

### Запросные ключевые слова

| Слово | Назначение |
|---|---|
| `from … in` | источник и переменная диапазона |
| `where` | фильтр |
| `select` | проекция |
| `orderby … ascending/descending` | сортировка |
| `group … by …` | группировка |
| `join … in … on … equals …` | соединение |
| `let` | новая переменная в запросе |
| `into` | продолжение запроса |

## Подвохи

Каждый подвох проверен запуском.

### Запрос видит изменения источника

`q = nums.Where(x => x > 1)`, затем `nums.Add(4)`: перебор дал `2,3,4`. А `ToList()` до добавления — снимок, не меняется.

### Переменная захвачена по ссылке

Запрос с `limit`, изменённым после создания, использовал новое значение (`5` элементов вместо `3`).

### for и foreach захватывают по-разному

`for` — одна переменная на цикл: `3,3,3`; `foreach` — новая на итерацию: `0,1,2`.

### Двойное перечисление

`Count()` и `Sum()` над одним запросом выполнили источник дважды: `e1 e2 e1 e2`.

### OrderBy буферизует, Take не спасает

`Src("a",3).OrderBy(x => -x).Take(1)` прочитал все три элемента перед первым результатом.

### Join читает всю вторую последовательность

`outer.Join(inner, …).Take(1)`: журнал `o1 i1 i2 i3 got1` — одна внешняя запись, три внутренних.

### Пустая последовательность по-разному

`int[]`: `Max()` и `Average()` — исключение; `int?[]`: `Max()` — `null`; `string[]`: `Max()` — `null`; `Sum()` — `0`.

### Sum проверяет переполнение

`new[] { int.MaxValue, 1 }.Sum()` — `OverflowException`; через `long` — `2147483648`.

### Единственный элемент

`Single()` на двух элементах — «Sequence contains more than one element», на пустой — «Sequence contains no elements».

### ToDictionary и дубликаты

`new[]{1,1}.ToDictionary(x => x)` — `ArgumentException`: «An item with the same key has already been added. Key: 1». `ToLookup` дубликаты допускает.

### Ошибка в лямбде отложена

`Select(x => 1 / x)` создаётся без ошибки; `DivideByZeroException` — при `ToList()`.

### Проверка аргументов: LINQ немедленно, ваш итератор — нет

`Enumerable.Where<int>(null!, …)` бросает сразу, а собственный `yield`-метод — только при первом `MoveNext`.

### Select с побочными эффектами и Count

`list.Select(x => { Log(); return x; }).Count()` вызвал лямбду для каждого элемента: оптимизация `Count` не отменяет вызов делегата.

### yield и try/catch

`yield return` внутри `try` с `catch` — ошибка CS1626; параметр `ref` в итераторе — CS1623.

## Компромиссы и альтернативы

| Задача | Вариант | Цена |
|---|---|---|
| Преобразование коллекции | LINQ | делегаты и итераторы: выделения памяти |
| | цикл `foreach` | быстрее в горячем коде, но больше кода |
| Фильтр + преобразование | `Where` + `Select` | два итератора |
| Поиск по ключу в цикле | `ToDictionary`/`ToLookup` один раз | память на таблицу |
| Минимум/максимум | `MinBy`/`MaxBy` | один проход, без сортировки |
| Параллельная обработка | PLINQ (`AsParallel`) | накладные расходы, порядок не гарантирован |
| Запросы к БД | `IQueryable` (EF Core) | ограничения на переводимые выражения |

LINQ пишется ради читаемости; профилируйте перед тем, как заменять его циклами. Для узких мест замеряйте выделения ([12 · Память и GC](../12-memory-and-gc/advanced.ru.md#измерение-выделений-памяти)).

## Вопросы с собеседований

**1. Что такое отложенное выполнение? Приведи пример ошибки из-за него.**

<details><summary>Ответ</summary>

Запрос — описание; он выполняется при перечислении (`foreach`, `ToList`, `Count`…), а не при объявлении. Ошибка: запрос создали, потом изменили источник или захваченную переменную — результат отличается от ожидаемого (`2,3,4` вместо `2,3`); или перебрали запрос дважды и выполнили дорогой источник дважды. Решение — материализация (`ToList`).

</details>

**2. Какие операторы отложенные, какие немедленные? Что такое стриминг и буферизация?**

<details><summary>Ответ</summary>

Отложенные: `Where`, `Select`, `Take`, `OrderBy`, `GroupBy`, `Join`…; немедленные: `ToList`, `Count`, `Sum`, `First`, `Any`. Стриминговые (`Where`, `Select`, `Distinct`) отдают элементы по мере чтения источника; буферизующие (`OrderBy`, `GroupBy`, `Reverse`) читают источник целиком до первого результата; `Join` читает целиком вторую последовательность (проверено по журналам).

</details>

**3. Чем запросный синтаксис отличается от синтаксиса методов?**

<details><summary>Ответ</summary>

Равноценны: компилятор переписывает запросный в вызовы методов. Запросный удобен для `join`, нескольких `from` и `let`; часть операторов (`Take`, `Count`, `Distinct`) ключевых слов не имеют. Запросный синтаксис работает с любым типом, у которого есть нужные методы.

</details>

**4. Чем `IEnumerable<T>` отличается от `IQueryable<T>`?**

<details><summary>Ответ</summary>

`IEnumerable` исполняется в памяти, лямбды — делегаты `Func<…>`. `IQueryable` хранит выражение (`Expression<Func<…>>`) — дерево данных, которое провайдер переводит в другой язык (SQL). Ранний `AsEnumerable()` переносит остаток запроса в память. Подробнее — [15 · EF Core](../15-ef-core/advanced.ru.md#iqueryable-и-ienumerable).

</details>

**5. Что такое дерево выражений и чем оно отличается от делегата?**

<details><summary>Ответ</summary>

Делегат — скомпилированный код; дерево — структура данных, описывающая код (`x => x > 2` → лямбда с узлом `GreaterThan`, параметром и константой), которую можно разобрать, изменить и перевести или скомпилировать (`Compile()`). Не всё можно записать деревом: лямбды с блоками (CS0834) и литералы кортежей (CS8143) нельзя.

</details>

**6. Как работает `yield return`?**

<details><summary>Ответ</summary>

Компилятор превращает метод в генерируемый класс-конечный автомат с полями `<>1__state`, `<>2__current` и сохранёнными локальными переменными; `MoveNext` выполняет код до очередного `yield return`. Тело не выполняется при вызове метода, а `finally` — при `Dispose` (в том числе при `break`).

</details>

**7. Почему `Count() > 0` хуже `Any()`?**

<details><summary>Ответ</summary>

Для источников, которые не коллекции, `Count()` проходит всю последовательность (проверено: 3 шага против 1 у `Any()`). Для `ICollection<T>` оба используют свойство `Count`, но `Any()` выражает намерение и безопасен для ленивых источников.

</details>

**8. Подвох: что выведет код?**

```csharp
var fs = new List<Func<int>>();
for (int i = 0; i < 3; i++) fs.Add(() => i);
Console.WriteLine(string.Join(",", fs.Select(f => f())));
```

<details><summary>Ответ</summary>

`3,3,3`. В цикле `for` одна переменная `i` на все итерации, и лямбды читают её итоговое значение. С `foreach` результат был бы `0,1,2`. Исправление — локальная копия внутри тела цикла.

</details>

**9. Что произойдёт при `new[] { int.MaxValue, 1 }.Sum()`? Как обойти?**

<details><summary>Ответ</summary>

`OverflowException`: `Sum` для `int` проверяет переполнение. Обход: `Sum(x => (long)x)` (результат `2147483648`).

</details>

**10. Как вести себя с пустыми последовательностями?**

<details><summary>Ответ</summary>

`First`, `Single`, `Last`, `Max`/`Min`/`Average` для `int` бросают `InvalidOperationException`; `FirstOrDefault`, `SingleOrDefault` возвращают значение по умолчанию (в .NET 6+ можно задать своё); `Max` для `int?` и ссылочных типов — `null`; `Sum`/`Count` — `0`; `DefaultIfEmpty(x)` подставляет значение.

</details>

**11. В чём разница между `First`, `Single` и `FirstOrDefault`?**

<details><summary>Ответ</summary>

`First` — первый, исключение, если пусто; `Single` — единственный, исключение при пустой и при **более чем одном**; `FirstOrDefault` — первый или значение по умолчанию. `Single` выражает инвариант «ровно один».

</details>

**12. Как реализовать левое соединение?**

<details><summary>Ответ</summary>

Классика: `GroupJoin` и `SelectMany` с `DefaultIfEmpty()`; в запросном синтаксисе — `join … into g from x in g.DefaultIfEmpty()`. В современных версиях — оператор `LeftJoin` (на .NET 11 вернул `1x` и `2-` для отсутствующего).

</details>

**13. Чем `GroupBy` отличается от `ToLookup`?**

<details><summary>Ответ</summary>

`GroupBy` отложенный: группирует при перечислении. `ToLookup` немедленный: строит таблицу «ключ → элементы», доступ по ключу (`lk["QA"]`), для отсутствующего ключа — пустая последовательность (проверено), а не исключение.

</details>

**14. Подвох: почему собственный `yield`-метод с `ArgumentNullException.ThrowIfNull` не бросает сразу, а встроенный `Where` бросает?**

<details><summary>Ответ</summary>

Тело итератора не выполняется до первого `MoveNext`, поэтому проверка откладывается. Методы LINQ разделены на внешнюю обёртку, которая проверяет аргументы немедленно, и внутренний итератор. Тот же приём применяют и в собственном коде.

</details>

**15. Как `OrderBy` связан со стабильностью и сколько памяти он использует?**

<details><summary>Ответ</summary>

`OrderBy` стабилен: равные ключи сохраняют исходный порядок (проверено на `("b",1),("a",1),("c",0)` → `c,b,a`). Он буферизует весь источник, то есть `O(n)` памяти, и сортирует при первом `MoveNext`. `OrderBy(...).First()` лучше заменить на `MinBy`.

</details>

**16. Что такое `AggregateBy`/`CountBy` и зачем они?**

<details><summary>Ответ</summary>

Операторы .NET 9: `CountBy(key)` считает элементы по ключу без промежуточных групп (`"hello".CountBy(c => c)` → `h=1, e=1, l=2, o=1`), `AggregateBy(key, seed, func)` — свёртка по ключу. Они заменяют `GroupBy(...).Select(g => (g.Key, g.Count()))` и экономят выделения.

</details>

**17. Есть ли асинхронный LINQ?**

<details><summary>Ответ</summary>

Да. На .NET 11 операторы над `IAsyncEnumerable<T>` входят в платформу (сборка `System.Linq.AsyncEnumerable`): `ToAsyncEnumerable()`, `ToListAsync()` и другие (проверено). Раньше для этого использовали пакет `System.Linq.Async`. Потребляется через `await foreach`.

</details>

## Шпаргалка

| Нужно | Решение |
|---|---|
| Зафиксировать результат | `ToList()`, `ToArray()` |
| Есть ли элементы | `Any()` |
| Минимум/максимум по полю | `MinBy`, `MaxBy` |
| Подсчёт по ключу | `CountBy`, `GroupBy` + `Count` |
| Поиск по ключу многократно | `ToDictionary`, `ToLookup` |
| Левое соединение | `LeftJoin` или `GroupJoin` + `DefaultIfEmpty` |
| Развернуть вложенное | `SelectMany` |
| Разбить на пачки | `Chunk(n)` |
| Устойчивая сортировка | `OrderBy` |
| Не падать на пустом | `FirstOrDefault`, `DefaultIfEmpty` |
| Сумма без переполнения | `Sum(x => (long)x)` |
| Ленивая последовательность | `yield return` |

## Связанные материалы

- Раньше: [базовая теория](basics.ru.md), [продвинутая теория](advanced.ru.md)
- Коллекции: [06 · Коллекции и обобщения](../06-collections-and-generics/expert.ru.md)
- Делегаты и лямбды: [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md)
- LINQ над БД: [15 · EF Core](../15-ef-core/expert.ru.md)
- `async`/`await` и `await foreach`: 10 · Async/await
- Замеры выделений: [12 · Память и GC](../12-memory-and-gc/advanced.ru.md#измерение-выделений-памяти)

## Источники

- [LINQ](https://learn.microsoft.com/dotnet/csharp/linq/)
- [Перевод запросных выражений в вызовы методов (спецификация C#)](https://learn.microsoft.com/dotnet/csharp/language-reference/language-specification/expressions#127-query-expressions)
- [yield](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/yield)
- [Деревья выражений](https://learn.microsoft.com/dotnet/csharp/advanced-topics/expression-trees/)
- [Что нового в .NET 10: библиотеки](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/libraries)
- [Enumerable](https://learn.microsoft.com/dotnet/api/system.linq.enumerable)
- [Queryable](https://learn.microsoft.com/dotnet/api/system.linq.queryable)
