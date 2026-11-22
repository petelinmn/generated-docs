---
title: "Современный C#: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 11-modern-csharp
part: 2
tags: [pattern-matching, list-patterns, extension-members, field-keyword, collection-expressions, primary-constructor, required-members, null-conditional-assignment]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Современный C#: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая теория](basics.ru.md). **Время:** ~1,5 ч.

## Образцы подробнее

### Вложенные свойства и защитные условия

Образец свойств может обращаться к вложенным свойствам через точку (C# 10):

```csharp
var o = new Order(new Customer(new Address("Berlin")), 120m);
Console.WriteLine(o is { Customer.Address.City: "Berlin", Total: > 100 });   // True

record Customer(Address Address);
record Address(string City);
record Order(Customer Customer, decimal Total);
```

Если в цепочке встретится `null`, образец просто не совпадает — исключения не будет. Образец `{ }` означает «не `null`», поэтому `x is { } v` — компактная проверка с захватом: `o.Customer is { } c ? c.Address.City : "none"`. Дополнительное условие к ветви `switch` задаётся через `when`:

```csharp
static string Classify(object o) => o switch
{
    null => "null",
    int n when n < 0 => "negative int",
    int and >= 0 and < 10 => "digit",
    int => "big int",
    string { Length: 0 } => "empty string",
    string s => $"string {s}",
    Point { X: 0, Y: 0 } => "origin",
    Point(var x, var y) when x == y => "diagonal",
    Point { X: > 0 } => "right half",
    _ => "other",
};

record Point(int X, int Y);
```

Проверено на значениях `null`, `-5`, `7`, `100`, `""`, `"hi"`, `Point(0,0)`, `Point(3,3)`, `Point(2,1)`: ответы — `null`, `negative int`, `digit`, `big int`, `empty string`, `string hi`, `origin`, `diagonal`, `right half`. Порядок ветвей важен: более частный образец ставят выше общего. Ветвь, которая не может сработать (всё уже покрыто выше), — ошибка компиляции `CS8510` (проверено).

### Списочные образцы

Образец списка (C# 11) сопоставляет массивы, `List<T>` и всё, у чего есть `Length`/`Count` и индексатор. Срез `..` подходит под любое число элементов и может захватываться:

```csharp
int[] arr = [1, 2, 3, 4, 5];
Console.WriteLine(arr is [1, _, 3, ..]);           // True
Console.WriteLine(arr is [.., 4, 5]);              // True
if (arr is [var first, .. var mid, var last])
    Console.WriteLine($"{first} {string.Join(",", mid)} {last}");   // 1 2,3,4 5
Console.WriteLine(arr is [_, _]);                  // False: exactly two elements
```

Практический пример — разбор аргументов командной строки: `["add", var item]` — ровно один аргумент, `["add", var first, .. var rest]` — больше. Образцы работают и со строками: `"abc" is ['a', .. var tail]`.

## Первичные конструкторы: тонкости

Параметр первичного конструктора, использованный в методе, «захватывается» в скрытое поле. Если одновременно скопировать его в свойство **и** обратиться к нему в методе, значение хранится дважды, и компилятор предупреждает `CS9124`:

```csharp
class Svc(string name)
{
    public string Name { get; } = name;   // copy
    public string Show() => name;         // capture: warning CS9124 (stored twice)
}
```

Правила: используйте параметр либо только для инициализации, либо только как захваченное состояние. Параметры нельзя изменить из-за пределов типа и они не появляются в API. Для `struct` и `class` дополнительные конструкторы обязаны вызывать первичный через `: this(...)`.

## Выражения коллекций: типы и spread

| Целевой тип | Что создаётся |
|---|---|
| `T[]`, `List<T>`, `HashSet<T>` | массив / список / множество |
| `Span<T>`, `ReadOnlySpan<T>` | обычно на стеке или как массив данных сборки |
| `ImmutableArray<T>` и другие с `[CollectionBuilder]` | через фабричный метод |
| `IEnumerable<T>`, `IReadOnlyList<T>` | неизменяемая скрытая коллекция |
| `IList<T>`, `ICollection<T>` | `List<T>` |

Проверено: `IEnumerable<int> en = [..a1, ..l1]` создаёт скрытый тип `<>z__ReadOnlyArray`, поэтому приводить результат к `List<T>` нельзя. Каждое выражение `..source` вычисляется **один раз**: два spread-а над генератором — два вызова (проверено: `calls=2`).

Свой тип поддерживает `[...]`, если реализует `IEnumerable` и помечен `CollectionBuilder`:

```csharp
[CollectionBuilder(typeof(MyListBuilder), nameof(MyListBuilder.Create))]
class MyList<T> : IEnumerable<T> { /* ... */ }

static class MyListBuilder
{
    public static MyList<T> Create<T>(ReadOnlySpan<T> items) { /* copy items */ }
}

MyList<int> ml = [1, 2, 3];   // calls MyListBuilder.Create
```

### Аргументы выражения коллекции (C# 15)

Первый элемент `with(...)` передаёт аргументы конструктору или фабрике коллекции — например, ёмкость списка:

```csharp
List<int> xs = [with(capacity: 10), 1, 2, 3];
Console.WriteLine($"{xs.Count} cap={xs.Capacity}");   // 3 cap=10
```

## Свойства: `field`, `required`, `init`

Ключевое слово `field` (C# 14) — обращение к автоматическому полю свойства, чтобы добавить логику без объявления своего поля:

```csharp
class Temperature
{
    public double Celsius { get; set => field = Math.Max(value, -273.15); }   // validate in the setter
}

var t = new Temperature { Celsius = -500 };
Console.WriteLine(t.Celsius == -273.15);   // True
```

Если в вашем типе уже есть член с именем `field`, пишите `@field` или `this.field`. Конструктор с атрибутом `[SetsRequiredMembers]` обещает, что сам задаёт все `required`-члены:

```csharp
class Req
{
    public required string Name { get; init; }

    [SetsRequiredMembers]   // using System.Diagnostics.CodeAnalysis;
    public Req(string n) { Name = n; }
}
```

## Null-условное присваивание

C# 14 разрешает `?.` слева от присваивания: правая часть вычисляется, только если объект не `null`:

```csharp
Holder? h = null;
h?.Value = 5;                     // nothing happens, 5 is not even evaluated
h = new Holder();
h?.Value = 7;
Console.WriteLine(h.Value);       // 7

class Holder { public int Value { get; set; } }
```

## Extension members

Блок `extension` (C# 14) позволяет добавлять к типам не только методы, но и **свойства** и **статические** члены:

```csharp
static class Ext
{
    extension(string s)
    {
        public IEnumerable<string> Words => s.Split(',');                         // instance property
        public static bool IsNullOrEmpty2(string? v) => string.IsNullOrEmpty(v);  // "static" member of string
    }

    extension<T>(IEnumerable<T> src)
    {
        public bool IsEmpty => !src.Any();
    }
}

Console.WriteLine("a,b,c".Words.Count());        // 3
Console.WriteLine(string.IsNullOrEmpty2(""));    // True
Console.WriteLine(new[] { 1, 2, 3 }.IsEmpty);    // False
```

Старые методы расширения с `this` продолжают работать. Расширения не имеют доступа к закрытым членам типа и не меняют его: это синтаксический сахар над статическими методами.

## Прочие возможности

| Возможность | С версии | Пример |
|---|---|---|
| Псевдонимы любых типов | C# 12 | `using Coord = (double Lat, double Lon);` |
| `params` для коллекций и `Span` | C# 13 | `static int Sum(params ReadOnlySpan<int> xs)` |
| Тип `System.Threading.Lock` | C# 13 / .NET 9 | `lock (new Lock()) { … }` ([18 · Многопоточность](../18-concurrency/overview.ru.md)) |
| Inline arrays | C# 12 | `[InlineArray(4)] struct Buf4 { int _e0; }` |
| `nameof` для несвязанных обобщений | C# 14 | `nameof(List<>)` → `List` |
| Метки у `break`/`continue` | C# 15 | `continue outer;` |

Проверено запуском: псевдоним-кортеж, `params ReadOnlySpan<int>` (`Sum(1, 2, 3)` = 6), `nameof(List<>)` = `List`, inline array из четырёх `int` занимает 16 байт.

Метки позволяют выйти из вложенных циклов без флагов:

```csharp
outer:
for (int i = 0; i < 3; i++)
    for (int j = 0; j < 3; j++)
    {
        if (j == 2) continue outer;   // jump to the next i
        if (i == 2) break outer;      // leave both loops
        Console.WriteLine($"{i},{j}");
    }
// 0,0  0,1  1,0  1,1
```

## Лучшие практики

- Используйте `switch`-выражение, когда нужно вернуть значение; обычный `switch` — когда нужны побочные эффекты.
- Ставьте `_` последним и не злоупотребляйте им: без него компилятор помогает находить непокрытые случаи (`CS8509`).
- `required` + `init` вместо конструктора с десятком параметров для DTO и конфигураций.
- Для `[..]` с известным размером предпочитайте целевой тип (`int[]`, `List<T>`), а не `IEnumerable<T>`.
- Не включайте логику в `field`-свойство, если она тяжёлая или имеет побочные эффекты.

## Связанные материалы

- Назад: [базовая теория](basics.ru.md). Дальше: [экспертная теория](expert.ru.md) — union-типы, внутреннее устройство, подвохи.
- [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md) — типы коллекций.
- [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md) — позиционные записи, `with`.
- [07 · LINQ](../07-linq/overview.ru.md) — методы расширения и их альтернатива `extension`.
- [12a · Span и Memory](../12a-span-and-memory/overview.ru.md) — `Span<T>`, `params ReadOnlySpan<T>`.

## Источники

- [Что нового в C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14)
- [Что нового в C# 15](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15)
- [Списочные образцы](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns#list-patterns)
- [Extension members](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
